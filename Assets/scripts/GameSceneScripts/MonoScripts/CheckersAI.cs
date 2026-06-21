using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets
{
    public class CheckersAI : MonoBehaviour
    {
        public const int USUAL_CHECKER_COUNT_OF_POINTS = 50;
        public const int KING_CHECKER_COUNT_OF_POINTS = 10 * USUAL_CHECKER_COUNT_OF_POINTS;
        public const int PROXIMITY_CENTER_COUNT_OF_POINTS = 20;
        public const int PROXIMITY_END_OF_BOARD_COUNT_OF_POINTS = 100;
        public bool IsCalculating = false;

        private int _searchDeep = GameSettings.AISearchDeep;
        private bool _isAllThreadsExit = false;

        void Update()
        {
            if (IsCalculating ||
                Game.IsPaused ||
                Game.CurrentMoveTurn != OpponentType.AI ||
                Game.EndOfGame != EndOfGameType.None ||
                !BoardEntities.Instance.IsAllCheckersMoved) return;

            IsCalculating = true;
            StartCoroutine(ProcessAIMove());
        }

        private IEnumerator ProcessAIMove()
        {
            _isAllThreadsExit = false;

            var task = Task.Run(async () =>
            {
                (float points, CheckerMove? move) = await EvaluateTree(
                    BoardEntities.Instance.CurrentPosition.Clone(),
                    true,
                    float.NegativeInfinity,
                    float.PositiveInfinity
                );
                return move;
            });

            while (!task.IsCompleted)
            {
                yield return null;
            }

            var move = task.Result;

            while (
                !BoardEntities.Instance.TrySelectChecker(BoardEntities.Instance.CurrentPosition.Data[move.Value.From.x, move.Value.From.y], OpponentType.AI) ||
                !BoardEntities.Instance.TryMakeMoveSelectedChecker(new Vector2Int(move.Value.To.x, move.Value.To.y), OpponentType.AI)
                )
            {
                yield return null;
            }

            IsCalculating = false;
        }


        private async Task<(float points, CheckerMove? move)> EvaluateTree(BoardPosition pos, bool isMax, float alpha, float betta, int curDeep = 0)
        {
            CheckerMove? move = null;

            if (curDeep == _searchDeep || (isMax && !pos.IsOpponentCanMove(OpponentType.AI)) || (!isMax && !pos.IsOpponentCanMove(OpponentType.Player)))
                return (GetPositionAssessment(pos), move);

            float bestPoints = isMax ? float.NegativeInfinity : float.PositiveInfinity;

            List<CheckerMove> moves = pos.GetAllPossibleMoves(isMax ? OpponentType.AI : OpponentType.Player);

            if (moves.Count == 0)
                return (GetPositionAssessment(pos), move);
            else if (moves.Count == 1 && curDeep == 0)
            {
                move = moves[0];
                return (GetPositionAssessment(pos), move);
            }

            if (curDeep == 0)
            {
                var tasks = new List<(Task<(float points, CheckerMove? nextMove)> task, CheckerMove lastMove)>();

                for (int i = 0; i < moves.Count; i++)
                {
                    BoardPosition nextPosition = pos.Clone();
                    nextPosition.MakeMove(moves[i].From, moves[i].To, out bool isOpponentContinueBeating, out CheckerData beatenChecker, out _);
                    CheckerMove lastMove = new CheckerMove(moves[i].From, moves[i].To, moves[i].CheckerOpponent, beatenChecker != null);

                    tasks.Add((Task.Run(() => EvaluateTree(nextPosition, isOpponentContinueBeating ? isMax : !isMax, alpha, betta, curDeep + 1)), lastMove));
                }

                while (tasks.Count > 0)
                {
                    var completedTask = await Task.WhenAny(tasks.Select(t => t.task));
                    var completedItem = tasks.First(t => t.task == completedTask);
                    tasks.Remove(completedItem);

                    var result = await completedItem.task;
                    
                    if (AlphaBettaCutting(isMax, ref alpha, ref betta, ref move, ref bestPoints, completedItem.lastMove, result.points))
                    {
                        _isAllThreadsExit = true;
                        break;
                    }
                }
            }
            else
            {
                for (int i = 0; i < moves.Count; i++)
                {
                    if (_isAllThreadsExit) break;

                    BoardPosition nextPosition = pos.Clone();
                    nextPosition.MakeMove(moves[i].From, moves[i].To, out bool isOpponentContinueBeating, out CheckerData beatenChecker, out _);
                    CheckerMove lastMove = new CheckerMove(moves[i].From, moves[i].To, moves[i].CheckerOpponent, beatenChecker != null);

                    (float points, CheckerMove? _) = await EvaluateTree(
                                                                nextPosition,
                                                                isOpponentContinueBeating ? isMax : !isMax,
                                                                alpha, betta,
                                                                curDeep + 1);

                    if (AlphaBettaCutting(isMax, ref alpha, ref betta, ref move, ref bestPoints, lastMove, points))
                    {
                        break;
                    }
                }
            }

            return (bestPoints, move);
        }

        private static bool AlphaBettaCutting(bool isMax, ref float alpha, ref float betta,
            ref CheckerMove? move, ref float bestPoints, CheckerMove lastMove, float points)
        {
            if (isMax)
            {
                if (points > bestPoints)
                {
                    bestPoints = points;
                    move = lastMove;
                }
                if (points > alpha)
                    alpha = points;
            }
            else
            {
                if (points < bestPoints)
                {
                    bestPoints = points;
                    move = lastMove;
                }
                if (points < betta)
                    betta = points;
            }

            return alpha >= betta;
        }

        public static float GetPositionAssessment(BoardPosition position)
        {
            if (position == null) return 0.5f;
            if (position.PlayerCheckerCount == 0) return  GameSettings.IsGiveaways ? 0f : 1f;
            if (position.AICheckerCount == 0) return GameSettings.IsGiveaways ? 1f : 0f;

            float totalPlayerPoints = 0f;
            float totalAIPoints = 0f;

            for (int y = 0; y < CheckersBoard.HEIGHT; y++)
            {
                for (int x = 0; x < CheckersBoard.WIDTH; x++)
                {
                    CheckerData checker = position.Data[x, y];
                    if (checker == null) continue;

                    float points = checker.GetPointsForChecker(position);

                    if (checker.Opponent == OpponentType.Player)
                        totalPlayerPoints += points;
                    else
                        totalAIPoints += points;
                }
            }

            if (totalPlayerPoints == 0) return 1f;
            if (totalAIPoints == 0) return 0f;

            float rawScore = totalAIPoints / totalPlayerPoints;
            float score = 1f / (1f + Mathf.Exp(-rawScore + 1f));

            float assessment = Mathf.Clamp01(score);

            return GameSettings.IsGiveaways ? 1 - assessment : assessment;
        }
    }
}