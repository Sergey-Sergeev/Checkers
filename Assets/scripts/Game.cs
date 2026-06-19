using UnityEngine;
using System.Collections;
using System;

namespace Assets
{
    public class Game
    {
        public static bool IsPaused { get; private set; }
        public static OpponentType CurrentMoveTurn { get; private set; } = GameSettings.FirstMoveTurn;
        public static EndOfGameType EndOfGame { get; private set; } = EndOfGameType.None;

        public static void SwitchOpponentTurn()
        {
            CheckEndOfGame();
            CurrentMoveTurn = (CurrentMoveTurn == OpponentType.Player ? OpponentType.AI : OpponentType.Player);
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
            EndOfGame = EndOfGameType.None;

            if (BoardEntities.Instance.CurrentPosition.CountOpponentCheckers(OpponentType.Player) == 0)
                EndOfGame = EndOfGameType.AIWin;
            else if (BoardEntities.Instance.CurrentPosition.CountOpponentCheckers(OpponentType.AI) == 0)
                EndOfGame = EndOfGameType.PlayerWin;
            else if ((!BoardEntities.Instance.CurrentPosition.IsOpponentCanMove(OpponentType.Player) && CurrentMoveTurn == OpponentType.Player) ||
                     (!BoardEntities.Instance.CurrentPosition.IsOpponentCanMove(OpponentType.AI) && CurrentMoveTurn == OpponentType.AI) ||
                     BoardEntities.Instance.MovesHistory.IsRuleOf15MovesFulFilled())
                EndOfGame = EndOfGameType.Draw;

            if (EndOfGame != EndOfGameType.None)
                Pause();
        }
    }
}