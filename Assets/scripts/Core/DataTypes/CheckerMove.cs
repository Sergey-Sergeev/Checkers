using UnityEngine;

namespace Assets.scripts.Core
{
    public struct CheckerMove
    {
        public Vector2Int From { get; private set; }
        public Vector2Int To { get; private set; }
        public OpponentType CheckerOpponent { get; private set; }
        public bool IsBeatOpponentChecker { get; private set; }

        public CheckerMove(Vector2Int from, Vector2Int to, OpponentType checkerOpponent, bool isBeatOpponentChecker)
        {
            From = from;
            To = to;
            CheckerOpponent = checkerOpponent;
            IsBeatOpponentChecker = isBeatOpponentChecker;
        }
    }
}