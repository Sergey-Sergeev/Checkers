using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.scripts.Core
{
    public class MinimaxCore
    {
        public const int USUAL_CHECKER_COUNT_OF_POINTS = 50;
        public const int KING_CHECKER_COUNT_OF_POINTS = 10 * USUAL_CHECKER_COUNT_OF_POINTS;
        public const int PROXIMITY_CENTER_COUNT_OF_POINTS = 20;
        public const int PROXIMITY_END_OF_BOARD_COUNT_OF_POINTS = 100;

        private bool _isStopped = false;

        private bool _isGiveaways;
        private int _boardHeight;
        private int _boardWidth;
        private int _aiSearchDeep;

        private int _nodesVisited = 0;
        private const int NODES_PER_FRAME = 20000;

        public MinimaxCore(int boardHeight, int boardWidth, int aiSearchDeep, bool isGiveaways)
        {
            _boardHeight = boardHeight;
            _boardWidth = boardWidth;
            _isGiveaways = isGiveaways;

            _isStopped = false;
            _aiSearchDeep = aiSearchDeep;
        }

        public async Awaitable<CheckerMove?> GetBestMove(BoardPosition position, OpponentType opponent)
        {
            if (position == null) return null;

            try
            {
                var result = await EvaluateTree(
                    position,
                    opponent == OpponentType.AI,
                    float.NegativeInfinity,
                    float.PositiveInfinity,
                    0
                );
                return result.move;
            }
            catch (Exception e)
            {
                Debug.LogError($"GetBestMove error: {e.Message}");
                return null;
            }
        }


        public async Awaitable RestartCalculating()
        {
            await StopCalculating();
            await Awaitable.NextFrameAsync();

            _isStopped = false;

            await Awaitable.NextFrameAsync();
        }

        public async Awaitable StopCalculating()
        {
            if (_isStopped) return;

            _isStopped = true;

            await Awaitable.NextFrameAsync();
        }

        public float GetPositionAssessment(BoardPosition position)
        {
            if (position == null) return 0.5f;
            if (position.PlayerCheckerCount == 0 && position.AICheckerCount == 0) return 0.5f;
            if (position.PlayerCheckerCount == 0) return _isGiveaways ? 0f : 1f;
            if (position.AICheckerCount == 0) return _isGiveaways ? 1f : 0f;

            float totalPlayerPoints = 0f;
            float totalAIPoints = 0f;

            for (int y = 0; y < _boardHeight; y++)
            {
                for (int x = 0; x < _boardWidth; x++)
                {
                    CheckerData checker = position.Data[x, y];
                    if (checker == null) continue;

                    float points = checker.GetPointsForChecker(position, GetPointsForChecker);

                    if (checker.Opponent == OpponentType.Player)
                        totalPlayerPoints += points;
                    else totalAIPoints += points;
                }
            }

            if (totalPlayerPoints == 0) return 1f;
            if (totalAIPoints == 0) return 0f;

            float rawScore = totalAIPoints / totalPlayerPoints;
            float score = 1f / (1f + Mathf.Exp(-rawScore + 1f));

            float assessment = Mathf.Clamp01(score);

            return _isGiveaways ? 1 - assessment : assessment;
        }

        public float GetPointsForChecker(BoardPosition position, CheckerData checker)
        {
            if (position == null || checker == null) return 0f;

            float points = 0;

            if (checker.Type == CheckerType.KING)
                points += KING_CHECKER_COUNT_OF_POINTS;
            else
            {
                points += USUAL_CHECKER_COUNT_OF_POINTS;

                float endBonus = checker.Y / (float)(_boardHeight - 1);
                if (checker.Opponent == OpponentType.AI)
                    endBonus = 1f - endBonus;

                points += endBonus * PROXIMITY_END_OF_BOARD_COUNT_OF_POINTS;
            }

            float center = _boardWidth / 2f;
            float centerBonus = 1f - Mathf.Abs(center - checker.X) / center;
            points += centerBonus * PROXIMITY_CENTER_COUNT_OF_POINTS;

            return points;
        }


        private async Awaitable<(float points, CheckerMove? move)> EvaluateTree(
            BoardPosition pos,
            bool isMax,
            float alpha,
            float beta,
            int curDeep = 0)
        {
            _nodesVisited++;

            if (_nodesVisited >= NODES_PER_FRAME)
            {
                _nodesVisited = 0;
                await Awaitable.NextFrameAsync();
            }

            if (_isStopped) return (0, null);

            CheckerMove? move = null;

            if (curDeep == _aiSearchDeep ||
                (isMax && !pos.IsOpponentCanMove(OpponentType.AI)) ||
                (!isMax && !pos.IsOpponentCanMove(OpponentType.Player)))
            {
                return (GetPositionAssessment(pos), null);
            }

            List<CheckerMove> moves = pos.GetAllPossibleMoves(isMax ? OpponentType.AI : OpponentType.Player);

            if (moves.Count == 0)
                return (0f, null);
            else if (moves.Count == 1 && curDeep == 0)
            {
                move = moves[0];
                return (0f, move);
            }

            for (int i = 0; i < moves.Count; i++)
            {
                if (_isStopped) return (0, null);

                BoardPosition nextPosition = pos.Clone();
                nextPosition.MakeMove(moves[i].From, moves[i].To, out bool isOpponentContinueBeating, out CheckerData beatenChecker, out _);
                CheckerMove lastMove = new CheckerMove(moves[i].From, moves[i].To, moves[i].CheckerOpponent, beatenChecker != null);

                (float points, CheckerMove? _) = await EvaluateTree(
                    nextPosition,
                    isOpponentContinueBeating ? isMax : !isMax,
                    alpha, beta,
                    curDeep + 1
                );

                if (isMax)
                {
                    if (points > alpha)
                    {
                        alpha = points;
                        move = lastMove;
                    }
                }
                else
                {
                    if (points < beta)
                    {
                        beta = points;
                        move = lastMove;
                    }
                }

                if (alpha >= beta)
                    break;
            }

            return (isMax ? alpha : beta, move);
        }
    }
}