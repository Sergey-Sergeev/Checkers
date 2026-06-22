using Assets.scripts.Core;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.scripts.GamePlay.GameSceneScripts
{
    public class CheckersAI : MonoBehaviour
    {
        public bool IsCalculating = false;

        private static bool _isBlocked = false;
        public static MinimaxCore Minimax { get; private set;  }

        private void Awake()
        {
            _isBlocked = false;
            Minimax = new MinimaxCore(
                GameSettings.BoardHeight,
                GameSettings.BoardWidth,
                GameSettings.AISearchDeep,
                GameSettings.IsGiveaways);
        }

        void Update()
        {
            if (_isBlocked ||
                IsCalculating ||
                Game.IsPaused ||
                Game.CurrentMoveTurn != OpponentType.AI ||
                Game.EndOfGame != EndOfGameType.None ||
                !BoardEntities.Instance.IsAllCheckersMoved) return;

            IsCalculating = true;
            StartCoroutine(ProcessAIMove());
        }

        public static async Task RestartCalculating()
        {
            await Minimax.RestartCalculating();
        }

        public static async Task StopCalculating()
        {
            await Minimax.StopCalculating();
        }

        private IEnumerator ProcessAIMove()
        {
            var task = Task.Run(async () =>
            {
                (float points, CheckerMove? move) = await Minimax.GetBestMove(BoardEntities.Instance.CurrentPosition.Clone(), OpponentType.AI);
                return move;
            });

            while (!task.IsCompleted)
            {
                yield return null;
            }

            var move = task.Result;

            while (
                !_isBlocked && move != null &&
                (!BoardEntities.Instance.TrySelectChecker(BoardEntities.Instance.CurrentPosition.Data[move.Value.From.x, move.Value.From.y], OpponentType.AI) ||
                !BoardEntities.Instance.TryMakeMoveSelectedChecker(new Vector2Int(move.Value.To.x, move.Value.To.y), OpponentType.AI))
                )
            {
                yield return null;
            }

            IsCalculating = false;
        }



    }
}