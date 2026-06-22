using Assets.scripts.Core;
using System.Diagnostics;
using Unity.VisualScripting;
using UnityEngine;

namespace Assets.scripts.GamePlay.GameSceneScripts
{
    public class Game : MonoBehaviour
    {
        public const string PLAYER_STR = "Player";
        public const string AI_STR = "AI";
        public static bool IsPaused { get => UIManager.Instance.IsTabVisible(UIManager.Tab.PauseTab); }
        public static OpponentType CurrentMoveTurn { get; set; }
        public static EndOfGameType EndOfGame { get; private set; }

        private void Awake()
        {
            CurrentMoveTurn = GameSettings.FirstMoveTurn;
            EndOfGame = EndOfGameType.None;
        }


        public static void SwitchOpponentTurn()
        {
            CheckEndOfGame();
            CurrentMoveTurn = (CurrentMoveTurn == OpponentType.Player ? OpponentType.AI : OpponentType.Player);
        }

        public static void Pause()
        {
            UIManager.Instance.ShowTab(UIManager.Tab.PauseTab);
        }

        public static void UnPause()
        {
            UIManager.Instance.HideTab(UIManager.Tab.PauseTab);
        }

        public static void CheckEndOfGame()
        {
            EndOfGame = EndOfGameType.None;

            if (BoardEntities.Instance.CurrentPosition.PlayerCheckerCount == 0)
                EndOfGame = EndOfGameType.AIWin;
            else if (BoardEntities.Instance.CurrentPosition.AICheckerCount == 0)
                EndOfGame = EndOfGameType.PlayerWin;
            else if ((!BoardEntities.Instance.CurrentPosition.IsOpponentCanMove(OpponentType.Player) && CurrentMoveTurn == OpponentType.Player) ||
                     (!BoardEntities.Instance.CurrentPosition.IsOpponentCanMove(OpponentType.AI) && CurrentMoveTurn == OpponentType.AI) ||
                     BoardEntities.Instance.MovesHistory.IsRuleOf15MovesFulFilled())
                EndOfGame = EndOfGameType.Draw;

            if (GameSettings.IsGiveaways)
            {
                if (EndOfGame == EndOfGameType.PlayerWin)
                    EndOfGame = EndOfGameType.AIWin;
                else if (EndOfGame == EndOfGameType.AIWin)
                    EndOfGame = EndOfGameType.PlayerWin;
            }

            if (EndOfGame != EndOfGameType.None)
            {
                Pause();

                GameStatistic.AddGameResult(Game.EndOfGame == EndOfGameType.PlayerWin,
                    GameSettings.FirstMoveTurn == OpponentType.Player, BoardEntities.Instance.MovesHistory.CheckerMovesAsStrings.Count);
            }
        }
    }
}