using Assets.scripts.Core;
using Assets.scripts.Infrastructure;
using System.Collections;
using UnityEngine;

namespace Assets.scripts.GameLogic
{
    public class CheckersAI : MonoBehaviour
    {
        public MinimaxCore _minimax;
        public bool IsCalculating { get; private set; }
        public OpponentType AIOpponent { get; set; } = OpponentType.AI;

        private void Awake()
        {
            IsCalculating = false;
            _minimax = new MinimaxCore(
                GameSettings.Instance.BoardHeight,
                GameSettings.Instance.BoardWidth,
                GameSettings.Instance.AISearchDeep,
                GameSettings.Instance.IsGiveaways);
        }

        void Update()
        {
            if (IsCalculating ||
                GameManager.Instance.IsPaused ||
                GameManager.Instance.CurrentMoveTurn != AIOpponent ||
                GameManager.Instance.IsEndOfGame() ||
                !BoardManager.Instance.IsAllCheckersMoved) return;

            IsCalculating = true;
            StartCoroutine(ProcessAIMove());
        }

        public float GetCurrentPositionAssessment()
        {
            return _minimax.GetPositionAssessment(BoardManager.Instance.CurrentPosition);
        }

        public void RestartCalculating() =>  _minimax.EnableCalculation();

        public void StopCalculating() => _minimax.DisableCalculation();
        
        private IEnumerator ProcessAIMove()
        {
            bool isCompleted = false;
            CheckerMove? move = null;

            var _ = ProcessAwaitable();

            async Awaitable ProcessAwaitable()
            {
                move = await _minimax.GetBestMove(BoardManager.Instance.CurrentPosition.Clone(), AIOpponent);
                isCompleted = true;
            }

            while (!isCompleted)
            {
                yield return null;
            }


            if (move.HasValue &&
                BoardManager.Instance.CurrentPosition.IsCheckerExist(BoardManager.Instance.CurrentPosition.Data[move.Value.From.x, move.Value.From.y]) &&
                BoardManager.Instance.CurrentPosition.IsCheckerCanMoveAt(BoardManager.Instance.CurrentPosition.Data[move.Value.From.x, move.Value.From.y],
                move.Value.To))
            {
                while (!BoardManager.Instance.TrySelectChecker(BoardManager.Instance.CurrentPosition.Data[move.Value.From.x, move.Value.From.y], AIOpponent) ||
                    !BoardManager.Instance.TryMakeMoveSelectedChecker(new Vector2Int(move.Value.To.x, move.Value.To.y), AIOpponent))
                {
                    if (GameManager.Instance.IsPaused) break;
                    yield return null;
                }
            }
            else
            {
                RestartCalculating();
            }

            IsCalculating = false;
        }
    }
}