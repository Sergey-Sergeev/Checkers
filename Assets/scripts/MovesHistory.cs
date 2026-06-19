using UnityEngine;
using System.Collections.Generic;
using NUnit.Framework;
using System.Linq;

namespace Assets
{
    public class MovesHistory
    {
        public List<CheckerMove> CheckerMoves { get; private set; }

        public const char CHECKER_BEAT_CHECKER_SYMBOL = ':';
        public const char CHECKER_MOVED_SYMBOL = '-';


        public delegate void CheckerMoveHandle();
        public event CheckerMoveHandle CheckerMoveEvent;

        private const int NO_PROGRESS_MOVES_COUNT = 15;
        private int _noProgressMovesCount = 0;


        public MovesHistory()
        {
            CheckerMoves = new List<CheckerMove>();
        }


        public void Add(CheckerData checkerData, Vector2Int move, bool isBeatOpponentChecker, bool isOpponentContinueBeating, bool isCheckerTransformd)
        {
            if (checkerData.Type == CheckerType.KING && !isBeatOpponentChecker && !isCheckerTransformd)
                _noProgressMovesCount++;
            else _noProgressMovesCount = 0;

            CheckerMoves.Add(new CheckerMove(new Vector2Int(checkerData.X, checkerData.Y), move, checkerData.Opponent, isBeatOpponentChecker));

            if (!isOpponentContinueBeating)
            {
                CheckerMoveEvent?.Invoke();
            }
        }

        public string? GetLastMoveAsString()
        {
            if (CheckerMoves.Count == 0) return null;

            OpponentType lastOpponent = CheckerMoves.Last().CheckerOpponent;

            if (CheckerMoves.Count == 1 || CheckerMoves[CheckerMoves.Count - 2].CheckerOpponent != lastOpponent)
            {
                CheckerMove lastCheckerMove = CheckerMoves[CheckerMoves.Count - 1];

                return $"{GetConvertedToStringCoords(lastCheckerMove.From.x, lastCheckerMove.From.y)}" +
                    $"{(lastCheckerMove.IsBeatOpponentChecker ? CHECKER_BEAT_CHECKER_SYMBOL : CHECKER_MOVED_SYMBOL)}" +
                    $"{GetConvertedToStringCoords(lastCheckerMove.To.x, lastCheckerMove.To.y)}";
            }


            string move = "";

            for (int i = CheckerMoves.Count - 1; i >= 0; i--)
            {
                if (CheckerMoves[i].CheckerOpponent != lastOpponent)
                {
                    move = $"{GetConvertedToStringCoords(CheckerMoves[i + 1].From.x, CheckerMoves[i + 1].From.y)}" + move;
                    break;
                }
                else
                {
                    move = $"{CHECKER_BEAT_CHECKER_SYMBOL}{GetConvertedToStringCoords(CheckerMoves[i].To.x, CheckerMoves[i].To.y)}" + move;
                }
            }

            return move;
        }

        public bool IsRuleOf15MovesFulFilled()
        {
            return _noProgressMovesCount == NO_PROGRESS_MOVES_COUNT;
        }


        private string GetConvertedToStringCoords(int x, int y)
        {
            return $"{(x > 25 ? x : (char)('a' + x))}{y}";
        }
    }
}