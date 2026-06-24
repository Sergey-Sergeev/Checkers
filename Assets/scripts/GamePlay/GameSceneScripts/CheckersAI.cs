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

        private bool _isBlocked = false;

        public static bool IsStoppingOrReseting { get; private set; }

        private void Awake()
        {
            IsStoppingOrReseting = false;
            IsCalculating = false;
            _isBlocked = false;
            Minimax = new MinimaxCore(
                GameSettings.Instance.BoardHeight,
                GameSettings.Instance.BoardWidth,
                GameSettings.Instance.AISearchDeep,
                GameSettings.Instance.IsGiveaways);
        }

        void Update()
        {
            if (_isBlocked ||
                IsCalculating ||
                Game.IsPaused ||
                Game.CurrentMoveTurn != AIOpponent ||
                Game.EndOfGame != EndOfGameType.None ||
                !BoardEntities.Instance.IsAllCheckersMoved) return;

            IsCalculating = true;
            StartCoroutine(ProcessAIMove());
        }

        public async Awaitable RestartCalculating()
        {
            IsStoppingOrReseting = true;
            await Minimax.RestartCalculating();
            IsStoppingOrReseting = false;
        }

        public async Awaitable StopCalculating()
        {
            IsStoppingOrReseting = true;
            await Minimax.StopCalculating();
            IsStoppingOrReseting = false;
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

            while (
                !_isBlocked && move != null &&
                (!BoardEntities.Instance.TrySelectChecker(BoardEntities.Instance.CurrentPosition.Data[move.Value.From.x, move.Value.From.y], AIOpponent) ||
                !BoardEntities.Instance.TryMakeMoveSelectedChecker(new Vector2Int(move.Value.To.x, move.Value.To.y), AIOpponent))
                )
            {
                yield return null;
            }

            IsCalculating = false;
        }
    }
}