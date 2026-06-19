using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets
{
    public class CheckerData
    {
        public int X { get; private set; }
        public int Y { get; private set; }
        public CheckerType Type { get; set; }
        public OpponentType Opponent { get; set; }
        private float _curCountOfPoints;


        private List<CheckerMove> _cashedMoves;
        private SimpleMoves _usualMoves;
        private int _usualMovesHash;
        private int _cashedHash;


        public CheckerData(int x, int y, CheckerType type, OpponentType opponent)
        {
            SetPosition(x, y);
            Type = type;
            Opponent = opponent;
            _cashedMoves = new List<CheckerMove>();
            _usualMovesHash = 0;
            ReCalculateHash();
        }

        public void SetPosition(int x, int y)
        {
            X = x;
            Y = y;
            _curCountOfPoints = 0;

            if (Type == CheckerType.USUAL)
            {
                if ((y == 0 && Opponent == OpponentType.AI) || (y == CheckersBoard.HEIGHT - 1 && Opponent == OpponentType.Player))
                {
                    Type = CheckerType.KING;
                    ReCalculateHash();
                }

                _usualMoves = new SimpleMoves(x, y);
            }
        }

        public float GetPointsForChecker(BoardPosition position)
        {
            if (_curCountOfPoints != 0)
                return _curCountOfPoints;

            float total = 0f;

            if (Type == CheckerType.KING)
                total += CheckersAI.KING_CHECKER_COUNT_OF_POINTS;
            else
            {
                total += CheckersAI.USUAL_CHECKER_COUNT_OF_POINTS;

                float endBonus = Y / (float)(CheckersBoard.HEIGHT - 1);
                if (Opponent == OpponentType.AI)
                    endBonus = 1f - endBonus;

                total += endBonus * CheckersAI.PROXIMITY_END_OF_BOARD_COUNT_OF_POINTS;
            }

            float center = CheckersBoard.WIDTH / 2f;

            float centerBonus = 1f - Mathf.Abs(center - X) / center;
            total += centerBonus * CheckersAI.PROXIMITY_CENTER_COUNT_OF_POINTS;

            total += GetAllMovesForChecker(position).Count * CheckersAI.COUNT_OF_POINTS_PER_MOVE;

            _curCountOfPoints = total;
            return _curCountOfPoints;
        }


        public CheckerData Clone()
        {
            CheckerData clone = new CheckerData(X, Y, Type, Opponent);
            clone._usualMovesHash = _usualMovesHash;
            clone._usualMoves = _usualMoves;
            clone._curCountOfPoints = _curCountOfPoints;

            for (int i = 0; i < _cashedMoves.Count; i++)
            {
                CheckerMove checkerMove = new CheckerMove(new Vector2Int(X, Y), _cashedMoves[i].To, _cashedMoves[i].CheckerOpponent, _cashedMoves[i].IsBeatOpponentChecker);
                clone._cashedMoves.Add(checkerMove);
            }

            return clone;
        }

        public List<CheckerMove> GetAllMovesForChecker(BoardPosition position)
        {
            if (Type == CheckerType.USUAL)
            {
                //return GetAllMovesAsUsual(ref position);

                int hash = GetHashCodeForPosition(ref position);

                if (hash == _usualMovesHash)
                    return _cashedMoves;
                else _usualMovesHash = hash;

                _cashedMoves = GetAllMovesAsUsual(ref position);
                return _cashedMoves;
            }
            return GetAllMovesAsKing(ref position);
        }

        private int GetHashCodeForPosition(ref BoardPosition position)
        {
            unchecked
            {
                int hash = 17;

                hash = hash * 23 + X;
                hash = hash * 23 + Y;

                Vector2Int?[] moves = {
                    _usualMoves.f1m1,
                    _usualMoves.f2m1,

                    _usualMoves.f1m2,
                    _usualMoves.f2m2,

                    _usualMoves.b1m1,
                    _usualMoves.b2m1,

                    _usualMoves.b1m2,
                    _usualMoves.b2m2
                };

                for (int i = 0; i < moves.Length; i++)
                {
                    if (moves[i].HasValue && position.Data[moves[i].Value.x, moves[i].Value.y] != null)
                        hash = hash * 23 + position.Data[moves[i].Value.x, moves[i].Value.y].GetHashCode();
                    else if (i % 2 == 0) // for usual checker if f1m1 is null then f2m1 dont influence
                        i++;
                }

                return hash;
            }
        }

        public bool IsCheckerNeedBeat(BoardPosition position)
        {
            if (Type == CheckerType.USUAL)
                return
                    IsCheckerCanBeatChecker(ref position, Opponent, _usualMoves.f1m1, _usualMoves.f2m1) ||
                    IsCheckerCanBeatChecker(ref position, Opponent, _usualMoves.f1m2, _usualMoves.f2m2) ||
                    IsCheckerCanBeatChecker(ref position, Opponent, _usualMoves.b1m1, _usualMoves.b2m1) ||
                    IsCheckerCanBeatChecker(ref position, Opponent, _usualMoves.b1m2, _usualMoves.b2m2);
            else
                return GetAllMovesAsKing(ref position).Any(m => m.IsBeatOpponentChecker);
        }

        private List<CheckerMove> GetAllMovesAsUsual(ref BoardPosition position)
        {
            List<CheckerMove> moves = new List<CheckerMove>();

            Vector2Int cur = new Vector2Int(X, Y);

            bool f1, f2, f3, f4;
            if (
                (f1 = IsCheckerCanBeatChecker(ref position, Opponent, _usualMoves.f1m1, _usualMoves.f2m1)) |
                (f2 = IsCheckerCanBeatChecker(ref position, Opponent, _usualMoves.f1m2, _usualMoves.f2m2)) |
                (f3 = IsCheckerCanBeatChecker(ref position, Opponent, _usualMoves.b1m1, _usualMoves.b2m1)) |
                (f4 = IsCheckerCanBeatChecker(ref position, Opponent, _usualMoves.b1m2, _usualMoves.b2m2))
                )
            {
                if (f1) moves.Add(new CheckerMove(cur, _usualMoves.f2m1.Value, Opponent, true));
                if (f2) moves.Add(new CheckerMove(cur, _usualMoves.f2m2.Value, Opponent, true));
                if (f3) moves.Add(new CheckerMove(cur, _usualMoves.b2m1.Value, Opponent, true));
                if (f4) moves.Add(new CheckerMove(cur, _usualMoves.b2m2.Value, Opponent, true));
            }
            else
            {
                Vector2Int? f1m1 = _usualMoves.f1m1;
                Vector2Int? f1m2 = _usualMoves.f1m2;

                if (Opponent == OpponentType.AI)
                {
                    f1m1 = _usualMoves.b1m1;
                    f1m2 = _usualMoves.b1m2;
                }

                if (f1m1.HasValue && position.Data[f1m1.Value.x, f1m1.Value.y] == null)
                    moves.Add(new CheckerMove(cur, f1m1.Value, Opponent, false));
                if (f1m2.HasValue && position.Data[f1m2.Value.x, f1m2.Value.y] == null)
                    moves.Add(new CheckerMove(cur, f1m2.Value, Opponent, false));

            }

            return moves;
        }


        /// <summary>
        /// <code lang="text">
        /// |    |      | pos2 |
        /// |----+------+------+
        /// |    | pos1 |      |
        /// |----+------+------+
        /// | CH |      |      |
        /// </code>
        /// </summary>
        private bool IsCheckerCanBeatChecker(ref BoardPosition position, OpponentType checkerOpponent, Vector2Int? pos1, Vector2Int? pos2)
        {
            return
                pos1.HasValue &&
                pos2.HasValue &&
                position.Data[pos1.Value.x, pos1.Value.y] != null &&
                position.Data[pos1.Value.x, pos1.Value.y].Opponent != checkerOpponent &&
                position.Data[pos2.Value.x, pos2.Value.y] == null;
        }

        private List<CheckerMove> GetAllMovesAsKing(ref BoardPosition position)
        {
            List<CheckerMove> moves = new List<CheckerMove>();

            List<CheckerMove> movesFrontLeft = GetAllMovesInDirection(ref position, new Vector2Int(-1, 1), out bool isBeatCheckerFrontLeft);
            List<CheckerMove> movesFrontRight = GetAllMovesInDirection(ref position, new Vector2Int(1, 1), out bool isBeatCheckerFrontRight);
            List<CheckerMove> movesBackLeft = GetAllMovesInDirection(ref position, new Vector2Int(-1, -1), out bool isBeatCheckerBackLeft);
            List<CheckerMove> movesBackRight = GetAllMovesInDirection(ref position, new Vector2Int(1, -1), out bool isBeatCheckerBackRight);

            if (isBeatCheckerFrontLeft || isBeatCheckerFrontRight || isBeatCheckerBackLeft || isBeatCheckerBackRight)
            {
                if (isBeatCheckerFrontLeft) moves.AddRange(movesFrontLeft);
                if (isBeatCheckerFrontRight) moves.AddRange(movesFrontRight);
                if (isBeatCheckerBackLeft) moves.AddRange(movesBackLeft);
                if (isBeatCheckerBackRight) moves.AddRange(movesBackRight);
            }
            else
            {
                moves.AddRange(movesFrontLeft);
                moves.AddRange(movesFrontRight);
                moves.AddRange(movesBackLeft);
                moves.AddRange(movesBackRight);
            }

            return moves;
        }

        private List<CheckerMove> GetAllMovesInDirection(ref BoardPosition position, Vector2Int dir, out bool isBeatChecker)
        {
            List<CheckerMove> moves = new List<CheckerMove>();
            isBeatChecker = false;

            Vector2Int checkerPos = new Vector2Int(X, Y);
            Vector2Int cur = checkerPos + dir;

            while (cur.x >= 0 && cur.y >= 0 && cur.x < CheckersBoard.WIDTH && cur.y < CheckersBoard.HEIGHT)
            {
                if (position.Data[cur.x, cur.y] == null)
                {
                    moves.Add(new CheckerMove(checkerPos, cur, Opponent, isBeatChecker));
                }
                else
                {
                    if (position.Data[cur.x, cur.y].Opponent == Opponent || isBeatChecker)
                    {
                        break;
                    }
                    else
                    {
                        Vector2Int? nextMove = cur + dir;
                        nextMove = TestBoundMoveCoords(nextMove.Value.x, nextMove.Value.y);

                        if (nextMove == null || position.Data[nextMove.Value.x, nextMove.Value.y] != null)
                        {
                            break;
                        }
                        else
                        {
                            isBeatChecker = true;
                            moves.Clear();
                        }
                    }
                }

                cur += dir;
            }

            return moves;
        }

        private static Vector2Int? TestBoundMoveCoords(int x, int y)
        {
            if (
                x >= CheckersBoard.WIDTH ||
                x < 0 ||
                y < 0 ||
                y >= CheckersBoard.HEIGHT
                )
                return null;
            return new Vector2Int(x, y);
        }

        public struct SimpleMoves
        {
            // player line is always 0
            // Y   
            //  f2m1         f2m2 
            //     \         /
            //     f1m1   f1m2  
            //        \   /
            //          C
            //        /   \
            //     b1m1   b1m2  
            //     /         \
            //  b2m1         b2m2             
            //0                   X

            public Vector2Int? f1m1;
            public Vector2Int? f1m2;
            public Vector2Int? f2m1;
            public Vector2Int? f2m2;
            public Vector2Int? b1m1;
            public Vector2Int? b1m2;
            public Vector2Int? b2m1;
            public Vector2Int? b2m2;

            public SimpleMoves(int x, int y)
            {
                f1m1 = CheckerData.TestBoundMoveCoords(x - 1, y + 1);
                f1m2 = CheckerData.TestBoundMoveCoords(x + 1, y + 1);
                f2m1 = CheckerData.TestBoundMoveCoords(x - 2, y + 2);
                f2m2 = CheckerData.TestBoundMoveCoords(x + 2, y + 2);
                b1m1 = CheckerData.TestBoundMoveCoords(x - 1, y - 1);
                b1m2 = CheckerData.TestBoundMoveCoords(x + 1, y - 1);
                b2m1 = CheckerData.TestBoundMoveCoords(x - 2, y - 2);
                b2m2 = CheckerData.TestBoundMoveCoords(x + 2, y - 2);
            }

        }

        public override bool Equals(object obj)
        {
            return Equals(obj as CheckerData);
        }

        public bool Equals(CheckerData other)
        {
            return other != null &&
                   X == other.X &&
                   Y == other.Y &&
                   Type == other.Type &&
                   Opponent == other.Opponent;
        }

        public override int GetHashCode()
        {
            return _cashedHash;
        }

        private void ReCalculateHash()
        {
            _cashedHash = HashCode.Combine(Type, Opponent);
        }

        public static bool operator ==(CheckerData left, CheckerData right)
        {
            if (left is null && right is null)
                return true;
            if (left is null || right is null)
                return false;
            return left.Equals(right);
        }

        public static bool operator !=(CheckerData left, CheckerData right)
        {
            return !(left == right);
        }
    }
}