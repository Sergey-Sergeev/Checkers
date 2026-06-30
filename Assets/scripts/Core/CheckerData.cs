using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.scripts.Core
{
    public class CheckerData
    {
        public int X { get; private set; }
        public int Y { get; private set; }
        public CheckerType Type { get; set; }
        public OpponentType Opponent { get; set; }
        public int BoardHeight { get => _boardHeight; }
        public int BoardWidth { get => _boardWidth; }

        private static int _boardHeight;
        private static int _boardWidth;

        private float _curCountOfPoints;
        private List<CheckerMove> _cachedMoves;
        private SimpleMoves _usualValidMoves;
        private int _positionHash;
        private int _cachedHash;

        private CheckerData() { }

        public CheckerData(int x, int y, CheckerType type, OpponentType opponent, int boardHeight, int boardWidth)
        {
            _boardHeight = boardHeight;
            _boardWidth = boardWidth;

            Type = type;
            Opponent = opponent;
            _cachedMoves = new List<CheckerMove>();
            SetPosition(x, y);
        }

        public void SetPosition(int x, int y)
        {
            X = x;
            Y = y;
            _curCountOfPoints = 0;
            _positionHash = 0;
            _cachedHash = 0;

            if (Type == CheckerType.Usual)
            {
                if ((y == 0 && Opponent == OpponentType.AI) || (y == _boardHeight - 1 && Opponent == OpponentType.Player))
                    Type = CheckerType.King;
                else _usualValidMoves = new SimpleMoves(x, y);
            }
        }

        public float GetPointsForChecker(BoardPosition position, Func<BoardPosition, CheckerData, float> calculatingPointsFunc)
        {
            if (this._curCountOfPoints != 0)
                return this._curCountOfPoints;

            _curCountOfPoints = calculatingPointsFunc(position, this);

            return _curCountOfPoints;
        }

        public CheckerData Clone(Vector2Int newPos)
        {
            return new CheckerData(newPos.x, newPos.y, Type, Opponent, _boardHeight, _boardWidth);
        }

        public IReadOnlyList<CheckerMove> GetAllMovesForChecker(BoardPosition position)
        {
            if (Type == CheckerType.Usual)
            {
                int hash = GetHashCodeForPosition(position);

                if (hash != _positionHash || _cachedMoves == null)
                {
                    _positionHash = hash;
                    _cachedMoves = GetAllMovesAsUsual(position);
                }

                return _cachedMoves;
            }
            return GetAllMovesAsKing(position);
        }

        private int GetHashCodeForPosition(BoardPosition position)
        {
            int hash = 17;
            hash += hash * 23 + GetHashCode();

            Vector2Int?[] moves = {
                    _usualValidMoves.f1m1,
                    _usualValidMoves.f2m1,

                    _usualValidMoves.f1m2,
                    _usualValidMoves.f2m2,

                    _usualValidMoves.b1m1,
                    _usualValidMoves.b2m1,

                    _usualValidMoves.b1m2,
                    _usualValidMoves.b2m2
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

        public bool IsCheckerNeedBeat(BoardPosition position)
        {
            if (Type == CheckerType.Usual)
                return
                    IsCheckerCanBeatChecker(position, _usualValidMoves.f1m1, _usualValidMoves.f2m1) ||
                    IsCheckerCanBeatChecker(position, _usualValidMoves.f1m2, _usualValidMoves.f2m2) ||
                    IsCheckerCanBeatChecker(position, _usualValidMoves.b1m1, _usualValidMoves.b2m1) ||
                    IsCheckerCanBeatChecker(position, _usualValidMoves.b1m2, _usualValidMoves.b2m2);
            else
                return GetAllMovesAsKing(position).Any(m => m.IsBeatOpponentChecker);
        }

        private List<CheckerMove> GetAllMovesAsUsual(BoardPosition position)
        {
            List<CheckerMove> moves = new List<CheckerMove>();

            Vector2Int cur = new Vector2Int(X, Y);

            bool f1, f2, f3, f4;
            if (
                (f1 = IsCheckerCanBeatChecker(position, _usualValidMoves.f1m1, _usualValidMoves.f2m1)) |
                (f2 = IsCheckerCanBeatChecker(position, _usualValidMoves.f1m2, _usualValidMoves.f2m2)) |
                (f3 = IsCheckerCanBeatChecker(position, _usualValidMoves.b1m1, _usualValidMoves.b2m1)) |
                (f4 = IsCheckerCanBeatChecker(position, _usualValidMoves.b1m2, _usualValidMoves.b2m2))
                )
            {
                if (f1) moves.Add(new CheckerMove(cur, _usualValidMoves.f2m1.Value, Opponent, true));
                if (f2) moves.Add(new CheckerMove(cur, _usualValidMoves.f2m2.Value, Opponent, true));
                if (f3) moves.Add(new CheckerMove(cur, _usualValidMoves.b2m1.Value, Opponent, true));
                if (f4) moves.Add(new CheckerMove(cur, _usualValidMoves.b2m2.Value, Opponent, true));
            }
            else
            {
                Vector2Int? f1m1 = _usualValidMoves.f1m1;
                Vector2Int? f1m2 = _usualValidMoves.f1m2;

                if (Opponent == OpponentType.AI)
                {
                    f1m1 = _usualValidMoves.b1m1;
                    f1m2 = _usualValidMoves.b1m2;
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
        private bool IsCheckerCanBeatChecker(BoardPosition position, Vector2Int? pos1, Vector2Int? pos2)
        {
            return
                pos1.HasValue &&
                pos2.HasValue &&
                position.Data[pos1.Value.x, pos1.Value.y] != null &&
                position.Data[pos1.Value.x, pos1.Value.y].Opponent != Opponent &&
                position.Data[pos2.Value.x, pos2.Value.y] == null;
        }

        private List<CheckerMove> GetAllMovesAsKing(BoardPosition position)
        {
            List<CheckerMove> moves = new List<CheckerMove>();

            List<CheckerMove> movesFrontLeft = GetAllMovesInDirection(position, new Vector2Int(-1, 1), out bool isBeatCheckerFrontLeft);
            List<CheckerMove> movesFrontRight = GetAllMovesInDirection(position, new Vector2Int(1, 1), out bool isBeatCheckerFrontRight);
            List<CheckerMove> movesBackLeft = GetAllMovesInDirection(position, new Vector2Int(-1, -1), out bool isBeatCheckerBackLeft);
            List<CheckerMove> movesBackRight = GetAllMovesInDirection(position, new Vector2Int(1, -1), out bool isBeatCheckerBackRight);

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

        private List<CheckerMove> GetAllMovesInDirection(BoardPosition position, Vector2Int dir, out bool isBeatChecker)
        {
            List<CheckerMove> moves = new List<CheckerMove>();
            isBeatChecker = false;
            bool isCanContinueBeatingCheckers = false;
            Vector2Int checkerPos = new Vector2Int(X, Y);
            Vector2Int cur = checkerPos + dir;

            while (cur.x >= 0 && cur.y >= 0 && cur.x < _boardWidth && cur.y < _boardHeight)
            {
                if (position.Data[cur.x, cur.y] == null)
                {
                    if (isBeatChecker && !isCanContinueBeatingCheckers)
                    {
                        isCanContinueBeatingCheckers = IsKingCanContinueBeating(position, cur, dir);

                        if (isCanContinueBeatingCheckers) moves.Clear();
                    }

                    if (!isBeatChecker || !isCanContinueBeatingCheckers || IsKingCanContinueBeating(position, cur, dir))
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

        private bool IsKingCanContinueBeating(BoardPosition position, Vector2Int pos, Vector2Int startDir)
        {
            Vector2Int[] allDirs =
            {
                new Vector2Int(1, 1),
                new Vector2Int(1, -1),
                new Vector2Int(-1, 1),
                new Vector2Int(-1, -1)
            };


            for (int i = 0; i < allDirs.Length; i++)
            {
                Vector2Int curDir = allDirs[i];

                if (curDir == (startDir * -1))
                    continue;

                Vector2Int curPos = pos;

                while (curPos.x >= 0 && curPos.y >= 0 && curPos.x < _boardWidth && curPos.y < _boardHeight)
                {
                    if (position.Data[curPos.x, curPos.y] != null)
                    {
                        if (position.Data[curPos.x, curPos.y].Opponent == Opponent)
                            break;
                        else
                        {
                            Vector2Int nextPos = curPos + curDir;
                            if (TestBoundMoveCoords(nextPos.x, nextPos.y).HasValue && position.Data[nextPos.x, nextPos.y] == null)
                            {
                                return true;
                            }
                            else break;
                        }
                    }

                    curPos += curDir;
                }

            }


            return false;
        }

        private static Vector2Int? TestBoundMoveCoords(int x, int y)
        {
            if (
                x >= _boardWidth ||
                x < 0 ||
                y < 0 ||
                y >= _boardHeight
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
            if (_cachedHash == 0)
                _cachedHash = HashCode.Combine(X, Y, Type, Opponent);

            return _cachedHash;
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