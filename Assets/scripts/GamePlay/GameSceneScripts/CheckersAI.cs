using Assets.scripts.Core;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.scripts.GamePlay.GameSceneScripts
{
    public class CheckersAI : MonoBehaviour
    {
        public MinimaxCore Minimax { get; private set;  }
        public bool IsCalculating { get; private set; }
        public OpponentType AIOpponent { get; set; } = OpponentType.AI;

        private bool _isBlocked = false;    

        private void Awake()
        {
            IsCalculating = false;
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
                Game.CurrentMoveTurn != AIOpponent ||
                Game.EndOfGame != EndOfGameType.None ||
                !BoardEntities.Instance.IsAllCheckersMoved) return;

            IsCalculating = true;
            StartCoroutine(ProcessAIMove());
        }

        public async Task RestartCalculating()
        {
            await Minimax.RestartCalculating();
        }

        public async Task StopCalculating()
        {
            await Minimax.StopCalculating();
        }

        private IEnumerator ProcessAIMove()
        {
            var task = Task.Run(async () =>
            {
                (float points, CheckerMove? move) = await Minimax.GetBestMove(BoardEntities.Instance.CurrentPosition.Clone(), AIOpponent);
                return move;
            });

            while (!task.IsCompleted)
            {
                yield return null;
            }

            var move = task.Result;

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