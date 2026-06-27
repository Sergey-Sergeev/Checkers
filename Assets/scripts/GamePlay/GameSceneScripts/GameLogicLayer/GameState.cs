using Assets.scripts.Core;
using UnityEngine;

namespace Assets.scripts.GamePlay.GameSceneScripts.GameLogicLayer
{
    public class GameState : MonoBehaviour
    {
        public static bool IsPaused { get => UITabManager.Instance.IsTabVisible(UITabManager.Tab.PauseTab); }
        public static OpponentType CurrentMoveTurn { get; set; }
        public static EndOfGameType EndOfGame { get; private set; }

        private void Awake()
        {
            CurrentMoveTurn = GameSettings.Instance.FirstMoveTurn;
            EndOfGame = EndOfGameType.None;
        }

        public static void SwitchOpponentTurn()
        {
            CheckEndOfGame();
            CurrentMoveTurn = (CurrentMoveTurn == OpponentType.Player ? OpponentType.AI : OpponentType.Player);
        }

        public static void Pause()
        {
            UITabManager.Instance.ShowTab(UITabManager.Tab.PauseTab);
        }

        public static void UnPause()
        {
            UITabManager.Instance.HideTab(UITabManager.Tab.PauseTab);
        }

        public static void CheckEndOfGame()
        {
            EndOfGame = EndOfGameType.None;

            if (BoardManager.Instance.CurrentPosition.PlayerCheckerCount == 0)
                EndOfGame = EndOfGameType.AIWin;
            else if (BoardManager.Instance.CurrentPosition.AICheckerCount == 0)
                EndOfGame = EndOfGameType.PlayerWin;
            else if ((!BoardManager.Instance.CurrentPosition.IsOpponentCanMove(OpponentType.Player) && CurrentMoveTurn == OpponentType.AI) ||
                     (!BoardManager.Instance.CurrentPosition.IsOpponentCanMove(OpponentType.AI) && CurrentMoveTurn == OpponentType.Player) ||
                     BoardManager.Instance.MovesHistory.IsRuleOf15MovesFulFilled())
                EndOfGame = EndOfGameType.Draw;

            if (GameSettings.Instance.IsGiveaways)
            {
                if (EndOfGame == EndOfGameType.PlayerWin)
                    EndOfGame = EndOfGameType.AIWin;
                else if (EndOfGame == EndOfGameType.AIWin)
                    EndOfGame = EndOfGameType.PlayerWin;
            }

            if (EndOfGame != EndOfGameType.None)
            {
                Pause();

                GameStatistic.Instance.AddGameResult(GameState.EndOfGame,
                    GameSettings.Instance.FirstMoveTurn == OpponentType.Player, BoardManager.Instance.MovesHistory.CheckerMovesAsStrings.Count);
            }
        }
    }
}