using Assets.scripts.Core;
using System.Collections;
using UnityEngine;

namespace Assets.scripts.GamePlay.GameSceneScripts.GameLogicLayer
{
    public class CheckersAI : MonoBehaviour
    {
        public MinimaxCore Minimax { get; private set; }
        public bool IsCalculating { get; private set; }
        public OpponentType AIOpponent { get; set; } = OpponentType.AI;

        private void Awake()
        {
            IsCalculating = false;
            Minimax = new MinimaxCore(
                GameSettings.Instance.BoardHeight,
                GameSettings.Instance.BoardWidth,
                GameSettings.Instance.AISearchDeep,
                GameSettings.Instance.IsGiveaways);
        }

        void Update()
        {
            if (IsCalculating ||
                GameState.IsPaused ||
                GameState.CurrentMoveTurn != AIOpponent ||
                GameState.EndOfGame != EndOfGameType.None ||
                !BoardManager.Instance.IsAllCheckersMoved) return;

            IsCalculating = true;
            StartCoroutine(ProcessAIMove());
        }

        public void RestartCalculating()
        {
            Minimax.RestartCalculating();
        }

        public void StopCalculating()
        {
            Minimax.StopCalculating();
        }

        private IEnumerator ProcessAIMove()
        {
            bool isCompleted = false;
            CheckerMove? move = null;

            var _ = ProcessAwaitable();

            async Awaitable ProcessAwaitable()
            {
                move = await Minimax.GetBestMove(BoardManager.Instance.CurrentPosition.Clone(), AIOpponent);
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
                    if (GameState.IsPaused) break;
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