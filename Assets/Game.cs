using UnityEngine;
using System.Collections;
using System;

namespace Assets
{
    public class Game
    {
        public static bool IsPaused { get; private set; }
        public static OpponentType CurrentMoveTurn { get; private set; } = GameSettings.FirstMoveTurn;
        public static bool IsCheckerSelected { get; private set; } = false;
        public static EndOfGameType EndOfGame { get; private set; }

        public static void SetOpponentTurn()
        {
            EndOfGame = EndOfGameType.None;
            CurrentMoveTurn = CurrentMoveTurn == OpponentType.Player ? OpponentType.AI : OpponentType.Player;
            CheckEndOfGame();
        }

        public static void Pause()
        {
            IsPaused = true;
        }

        public static void UnPause()
        {
            IsPaused = false;
        }

        public static void CheckEndOfGame()
        {
            if (BoardEntities.Instance.CurrentPosition.CountOpponentCheckers(OpponentType.Player) == 0)
                EndOfGame = EndOfGameType.AIWin;
            else if (BoardEntities.Instance.CurrentPosition.CountOpponentCheckers(OpponentType.AI) == 0)
                EndOfGame = EndOfGameType.PlayerWin;
            else if (!BoardEntities.Instance.CurrentPosition.IsOpponentCanMove(OpponentType.Player) ||
                     !BoardEntities.Instance.CurrentPosition.IsOpponentCanMove(OpponentType.AI) ||
                     BoardEntities.Instance.CurrentPosition.movesHistory.IsRuleOf15MovesFulFilled())
                EndOfGame = EndOfGameType.Draw;

            Pause();
        }
    }
}