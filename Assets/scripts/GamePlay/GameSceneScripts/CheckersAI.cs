using Assets.scripts.Core;
using System.Collections;
using UnityEngine;

namespace Assets.scripts.GamePlay.GameSceneScripts
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
                Game.IsPaused ||
                Game.CurrentMoveTurn != AIOpponent ||
                Game.EndOfGame != EndOfGameType.None ||
                !BoardEntities.Instance.IsAllCheckersMoved) return;

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
                move = await Minimax.GetBestMove(BoardEntities.Instance.CurrentPosition.Clone(), AIOpponent);
                isCompleted = true;
            }

            while (!isCompleted)
            {
                yield return null;
            }


            if (move.HasValue &&
                BoardEntities.Instance.CurrentPosition.IsCheckerExist(BoardEntities.Instance.CurrentPosition.Data[move.Value.From.x, move.Value.From.y]) &&
                BoardEntities.Instance.CurrentPosition.IsCheckerCanMoveAt(BoardEntities.Instance.CurrentPosition.Data[move.Value.From.x, move.Value.From.y],
                move.Value.To))
            {
                while (!BoardEntities.Instance.TrySelectChecker(BoardEntities.Instance.CurrentPosition.Data[move.Value.From.x, move.Value.From.y], AIOpponent) ||
                    !BoardEntities.Instance.TryMakeMoveSelectedChecker(new Vector2Int(move.Value.To.x, move.Value.To.y), AIOpponent))
                {
                    if (Game.IsPaused) break;
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