using UnityEngine;
using System.Collections.Generic;
using NUnit.Framework;

namespace Assets
{
    public class MovesHistory
    {
        public List<CheckerMove> CheckerMoves { get; private set; }

        private const int NO_PROGRESS_MOVES_COUNT = 15;
        private int _noProgressMovesCount = 0;

        public MovesHistory()
        {
            CheckerMoves = new List<CheckerMove>();
        }


        public void Add(Checker checker, Vector2Int move, bool isBeatChecker)
        {
            CheckerMoves.Add(new CheckerMove(new Vector2Int(checker.X, checker.Y), move, isBeatChecker));

            if (checker.Type == CheckerType.KING && !isBeatChecker)
                _noProgressMovesCount++;
            else _noProgressMovesCount = 0;
        }

        public bool IsRuleOf15MovesFulFilled()
        {
            return _noProgressMovesCount == NO_PROGRESS_MOVES_COUNT;
        }


    }
}