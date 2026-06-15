using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using UnityEngine;

namespace Assets
{
    public class BoardPosition
    {
        public MovesHistory movesHistory {  get; private set; }
        public Checker?[,] Data { get; private set; }

        public BoardPosition(List<Checker> checkers)
        {
            Data = new Checker?[CheckersBoard.WIDTH, CheckersBoard.HEIGHT];

            for (int y = 0; y < CheckersBoard.HEIGHT; y++)
                for (int x = 0; x < CheckersBoard.WIDTH; x++)
                    Data[x, y] = null;

            for (int i = 0; i < checkers.Count; i++)
            {
                Data[checkers[i].X, checkers[i].Y] = checkers[i];
            }

            movesHistory = new MovesHistory();
        }

        public int CountOpponentCheckers(OpponentType opponent)
        {
            int count = 0;

            for (int y = 0; y < CheckersBoard.HEIGHT; y++)
                for (int x = 0; x < CheckersBoard.WIDTH; x++)
                    if (Data[x, y] != null && Data[x, y].Opponent == opponent)
                        count++;

            return count;
        }

        public bool IsOpponentCanMove(OpponentType opponent)
        {
            for (int y = 0; y < CheckersBoard.HEIGHT; y++)
                for (int x = 0; x < CheckersBoard.WIDTH; x++)
                    if (Data[x, y] != null && Data[x, y].Opponent == opponent && GetAllMoves(Data[x, y], out _).Count != 0)
                        return true;
            return false;
        }

        public bool IsCheckerExist(Checker checker)
        {
            return
                checker.X >= 0 &&
                checker.Y >= 0 &&
                checker.X < CheckersBoard.WIDTH &&
                checker.Y < CheckersBoard.HEIGHT &&
                Data[checker.X, checker.Y] == checker;
        }

        public void MakeMove(Checker checker, Vector2Int move, out List<Checker> beatenCheckers)
        {
            if (!IsCheckerExist(checker) || !IsCheckerCanMoveAt(checker, move))
                throw new Exception("Impossible move.");

            int len = Math.Abs(checker.X - move.x);
            Vector2Int dir = new Vector2Int(Math.Sign(move.x - checker.X), Math.Sign(move.y - checker.Y));
            Vector2Int cur = new Vector2Int(checker.X, checker.Y);

            Data[move.x, move.y] = checker;

           beatenCheckers = new List<Checker>();

            for (int i = 0; i < len; i++)
            {
                if (i != 0 && Data[cur.x, cur.y] != null)
                    beatenCheckers.Add(Data[cur.x, cur.y]);

                Data[cur.x, cur.y] = null;
                cur += dir;
            }

            movesHistory.Add(checker, move, beatenCheckers.Count != 0);
        }

        public bool IsCheckerCanMoveAt(Checker checker, Vector2Int move)
        {
            List<Vector2Int> moves = GetAllMoves(checker, out _);
            return moves.Count != 0 && moves.Any(c => c.x == move.x && c.y == move.y);
        }

        public List<Vector2Int> GetAllMoves(Checker checker, out bool isBeatChecker)
        {
            if (checker.Type == CheckerType.USUAL)
                return GetAllMovesAsUsual(checker, out isBeatChecker);
            return GetAllMovesAsKing(checker, out isBeatChecker);
        }

        private Vector2Int? TestBoundMoveCoords(int x, int y)
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

        private List<Vector2Int> GetAllMovesAsUsual(Checker checker, out bool isBeatChecker)
        {
            List<Vector2Int> moves = new List<Vector2Int>();

            // player line is always 0
            // Y   
            //  f2M1         f2M2 
            //     \         /
            //     f1M1   f1M2  
            //        \   /
            //          C
            //        /   \
            //     b1M1   b1M2  
            //     /         \
            //  b2M1         b2M2             
            //0                   X

            Vector2Int? f1M1 = TestBoundMoveCoords(checker.X - 1, checker.Y + 1);
            Vector2Int? f1M2 = TestBoundMoveCoords(checker.X + 1, checker.Y + 1);
            Vector2Int? f2M1 = TestBoundMoveCoords(checker.X - 2, checker.Y + 2);
            Vector2Int? f2M2 = TestBoundMoveCoords(checker.X + 2, checker.Y + 2);
            Vector2Int? b1M1 = TestBoundMoveCoords(checker.X - 1, checker.Y - 1);
            Vector2Int? b1M2 = TestBoundMoveCoords(checker.X + 1, checker.Y - 1);
            Vector2Int? b2M1 = TestBoundMoveCoords(checker.X - 2, checker.Y - 2);
            Vector2Int? b2M2 = TestBoundMoveCoords(checker.X + 2, checker.Y - 2);

            bool f1, f2, f3, f4;
            if (
                (f1 = IsCheckerCanBeatChecker(checker, f1M1, f2M1)) |
                (f2 = IsCheckerCanBeatChecker(checker, f1M2, f2M2)) |
                (f3 = IsCheckerCanBeatChecker(checker, b1M1, b2M1)) |
                (f4 = IsCheckerCanBeatChecker(checker, b1M2, b2M2))
                )
            {
                if (f1) moves.Add((Vector2Int)f2M1);
                if (f2) moves.Add((Vector2Int)f2M2);
                if (f3) moves.Add((Vector2Int)b2M1);
                if (f4) moves.Add((Vector2Int)b2M2);

                isBeatChecker = true;
            }
            else
            {
                if (f1M1 != null) moves.Add((Vector2Int)f1M1);
                if (f1M2 != null) moves.Add((Vector2Int)f1M2);
                isBeatChecker = false;
            }

            return moves;
        }

        private bool IsCheckerCanBeatChecker(Checker checker, Vector2Int? pos1, Vector2Int? pos2)
        {
            return
                pos1 != null &&
                pos2 != null &&
                Data[pos1.Value.x, pos1.Value.y] != null &&
                Data[pos2.Value.x, pos2.Value.y] == null &&
                Data[pos1.Value.x, pos1.Value.y].Opponent != checker.Opponent;
        }

        private List<Vector2Int> GetAllMovesAsKing(Checker checker, out bool isBeatChecker)
        {
            List<Vector2Int> moves = new List<Vector2Int>();

            List<Vector2Int> movesFrontLeft = GetAllMovesInDirection(checker, new Vector2Int(-1, 1), out bool isBeatCheckerFrontLeft);
            List<Vector2Int> movesFrontRight = GetAllMovesInDirection(checker, new Vector2Int(1, 1), out bool isBeatCheckerFrontRight);
            List<Vector2Int> movesBackLeft = GetAllMovesInDirection(checker, new Vector2Int(-1, -1), out bool isBeatCheckerBackLeft);
            List<Vector2Int> movesBackRight = GetAllMovesInDirection(checker, new Vector2Int(1, -1), out bool isBeatCheckerBackRight);

            isBeatChecker = isBeatCheckerFrontLeft || isBeatCheckerFrontRight || isBeatCheckerBackLeft || isBeatCheckerBackRight;

            if (isBeatChecker)
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

        private List<Vector2Int> GetAllMovesInDirection(Checker checker, Vector2Int dir, out bool isBeatChecker)
        {
            List<Vector2Int> moves = new List<Vector2Int>();
            isBeatChecker = false;

            Vector2Int cur = (new Vector2Int(checker.X, checker.Y)) + dir;

            while (cur.x > 0 && cur.y > 0 && cur.x < CheckersBoard.WIDTH && cur.y < CheckersBoard.HEIGHT)
            {
                if (Data[cur.x, cur.y] == null)
                {
                    moves.Add(cur);
                }
                else
                {
                    if (Data[cur.x, cur.y].Opponent == checker.Opponent || isBeatChecker)
                    {
                        break;
                    }
                    else
                    {
                        Vector2Int? nextMove = cur + dir;
                        nextMove = TestBoundMoveCoords(nextMove.Value.x, nextMove.Value.y);

                        if (nextMove == null || Data[nextMove.Value.x, nextMove.Value.y] != null)
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

    }
}