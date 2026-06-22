using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using static PlasticPipe.PlasticProtocol.Messages.Serialization.ItemHandlerMessagesSerialization;

namespace Assets.scripts.Core
{
    public class MinimaxCore
    {
        public const int USUAL_CHECKER_COUNT_OF_POINTS = 50;
        public const int KING_CHECKER_COUNT_OF_POINTS = 10 * USUAL_CHECKER_COUNT_OF_POINTS;
        public const int PROXIMITY_CENTER_COUNT_OF_POINTS = 20;
        public const int PROXIMITY_END_OF_BOARD_COUNT_OF_POINTS = 100;

        public const int MIN_SEARCH_DEEP = 1;

        private bool _isStopped = false;
        private bool _isAllThreadsExit = false;

        private List<(Task<(float points, CheckerMove? nextMove)> task, CheckerMove lastMove)> _calculatingThreads =
            new List<(Task<(float points, CheckerMove? nextMove)> task, CheckerMove lastMove)>();

        private bool _isGiveaways;
        private int _boardHeight;
        private int _boardWidth;
        private int _aiSearchDeep;
        
        private CancellationTokenSource _cts;
        private object _lockObject = new object();

        public MinimaxCore(int boardHeight, int boardWidth, int aiSearchDeep, bool isGiveaways)
        {
            _isStopped = false;
            _isAllThreadsExit = false;

            _boardHeight = boardHeight;
            _boardWidth = boardWidth;
            _isGiveaways = isGiveaways;

            if (aiSearchDeep <= 0)
                _aiSearchDeep = MIN_SEARCH_DEEP;
            else _aiSearchDeep = aiSearchDeep;

        }

        public async Task<(float points, CheckerMove? move)> GetBestMove(BoardPosition position, OpponentType opponent)
        {
            if (position == null) return (0f, null);

            lock (_lockObject)
            {
                if (_isStopped) return (0f, null);

                _isAllThreadsExit = false;
                _cts = new CancellationTokenSource();
            }

            try
            {
                var result = await EvaluateTree(
                    position,
                    opponent == OpponentType.AI,
                    float.NegativeInfinity,
                    float.PositiveInfinity,
                    0,
                    _cts.Token
                );

                lock (_lockObject)
                {
                    if (_isStopped) return (0f, null);
                    return result;
                }
            }
            catch (OperationCanceledException)
            {
                return (0f, null);
            }
            finally
            {
                lock (_lockObject)
                {
                    _isAllThreadsExit = true;
                    _calculatingThreads.Clear();
                }
            }
        }

        public async Task RestartCalculating()
        {
            await StopCalculating();

            lock (_lockObject)
            {
                _isStopped = false;
                _isAllThreadsExit = false;
                _calculatingThreads.Clear();
                _cts = null;
            }
        }

        public async Task StopCalculating()
        {
            CancellationTokenSource cts;
            List<Task> tasksToWait = new List<Task>();

            lock (_lockObject)
            {
                if (_isStopped) return;

                _isStopped = true;
                _isAllThreadsExit = true;
                cts = _cts;
                _cts = null;

                tasksToWait = _calculatingThreads.Select(t => t.task).Cast<Task>().ToList();
                _calculatingThreads.Clear();
            }

            if (cts != null)
            {
                try
                {
                    cts.Cancel();
                    cts.Dispose();
                }
                catch (ObjectDisposedException) { }
            }

            if (tasksToWait.Count > 0)
            {
                try
                {
                    await Task.WhenAll(tasksToWait);
                }
                catch (TimeoutException)
                {
                    Debug.LogWarning("StopCalculating: timeout waiting for tasks");
                }
                catch (OperationCanceledException)
                {

                }
                catch (Exception ex)
                {
                    Debug.LogError($"StopCalculating error: {ex.Message}");
                }
            }
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
                    else
                        totalAIPoints += points;
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

        private async Task<(float points, CheckerMove? move)> EvaluateTree(
            BoardPosition pos,
            bool isMax,
            float alpha,
            float betta,
            int curDeep = 0,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            lock (_lockObject)
            {
                if (_isAllThreadsExit || _isStopped)
                    return (0, null);
            }

            CheckerMove? move = null;

            if (curDeep == _aiSearchDeep ||
                (isMax && !pos.IsOpponentCanMove(OpponentType.AI)) ||
                (!isMax && !pos.IsOpponentCanMove(OpponentType.Player)))
            {
                return (GetPositionAssessment(pos), move);
            }

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
                var tasks = new List<Task<(float points, CheckerMove? nextMove)>>();
                var movesList = new List<CheckerMove>();

                for (int i = 0; i < moves.Count; i++)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    BoardPosition nextPosition = pos.Clone();
                    nextPosition.MakeMove(moves[i].From, moves[i].To, out bool isOpponentContinueBeating, out CheckerData beatenChecker, out _);
                    CheckerMove lastMove = new CheckerMove(moves[i].From, moves[i].To, moves[i].CheckerOpponent, beatenChecker != null);

                    var task = Task.Run(() => EvaluateTree(
                        nextPosition,
                        isOpponentContinueBeating ? isMax : !isMax,
                        alpha,
                        betta,
                        curDeep + 1,
                        cancellationToken
                    ), cancellationToken);

                    lock (_lockObject)
                    {
                        if (!_isStopped && !_isAllThreadsExit)
                        {
                            _calculatingThreads.Add((task, lastMove));
                            tasks.Add(task);
                            movesList.Add(lastMove);
                        }
                    }
                }


                try
                {
                    var results = await Task.WhenAll(tasks);

                    for (int i = 0; i < results.Length; i++)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        lock (_lockObject)
                        {
                            if (_isStopped || _isAllThreadsExit)
                                return (0, null);
                        }

                        if (AlphaBettaCutting(isMax, ref alpha, ref betta, ref move, ref bestPoints,
                            movesList[i], results[i].points))
                        {
                            lock (_lockObject)
                            {
                                _isAllThreadsExit = true;
                                _calculatingThreads.Clear();
                            }
                            break;
                        }
                    }
                }
                finally
                {
                    lock (_lockObject)
                    {
                        _calculatingThreads.Clear();
                    }
                }
            }
            else
            {
                for (int i = 0; i < moves.Count; i++)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    lock (_lockObject)
                    {
                        if (_isAllThreadsExit || _isStopped)
                            return (0, null);
                    }

                    BoardPosition nextPosition = pos.Clone();
                    nextPosition.MakeMove(moves[i].From, moves[i].To, out bool isOpponentContinueBeating, out CheckerData beatenChecker, out _);
                    CheckerMove lastMove = new CheckerMove(moves[i].From, moves[i].To, moves[i].CheckerOpponent, beatenChecker != null);

                    (float points, CheckerMove? _) = await EvaluateTree(
                        nextPosition,
                        isOpponentContinueBeating ? isMax : !isMax,
                        alpha, betta,
                        curDeep + 1,
                        cancellationToken
                    );

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

    }
}