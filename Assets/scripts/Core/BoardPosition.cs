using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.scripts.Core
{
    public class BoardPosition
    {
        public CheckerData?[,] Data { get; private set; }
        public int PlayerCheckerCount { get; private set; }
        public int AICheckerCount { get; private set; }

        private int _boardWidth;
        private int _boardHeight;

        public BoardPosition(List<CheckerData> checkers, int boardWidth, int boardHeight)
        {
            _boardWidth = boardWidth;
            _boardHeight = boardHeight;

            Data = new CheckerData[_boardWidth, _boardHeight];

            for (int y = 0; y < _boardHeight; y++)
                for (int x = 0; x < _boardWidth; x++)
                    Data[x, y] = null;

            for (int i = 0; i < checkers.Count; i++)
            {
                Data[checkers[i].X, checkers[i].Y] = checkers[i];

                if (checkers[i].Opponent == OpponentType.Player)
                    PlayerCheckerCount++;
                else AICheckerCount++;
            }
        }

        private BoardPosition() { }

        public BoardPosition Clone()
        {
            BoardPosition clone = new BoardPosition();
            clone.Data = (CheckerData?[,])Data.Clone();
            clone.PlayerCheckerCount = PlayerCheckerCount;
            clone.AICheckerCount = AICheckerCount;    
            clone._boardHeight = _boardHeight;
            clone._boardWidth = _boardWidth;
            return clone;
        }


        public bool IsOpponentCanMove(OpponentType opponent)
        {
            if ((opponent == OpponentType.Player && PlayerCheckerCount == 0) || (opponent == OpponentType.AI && AICheckerCount == 0))
                return false;

            for (int y = 0; y < _boardHeight; y++)
                for (int x = 0; x < _boardWidth; x++)
                    if (Data[x, y] != null && Data[x, y].Opponent == opponent && Data[x, y].GetAllMovesForChecker(this).Count > 0)
                        return true;
            return false;
        }

        public bool IsOpponentNeedBeatChecker(OpponentType opponent)
        {
            for (int y = 0; y < _boardHeight; y++)
                for (int x = 0; x < _boardWidth; x++)
                    if (Data[x, y] != null &&
                        Data[x, y].Opponent == opponent &&
                        Data[x, y].IsCheckerNeedBeat(this))
                        return true;
            return false;
        }

        public bool IsCheckerExist(CheckerData checkerData)
        {
            return
                checkerData != null &&
                checkerData.X >= 0 &&
                checkerData.Y >= 0 &&
                checkerData.X < _boardWidth &&
                checkerData.Y < _boardHeight &&
                Data[checkerData.X, checkerData.Y] == checkerData;
        }


        /// <returns>updated checkerData</returns>
        public CheckerData MakeMove(Vector2Int checkerPos, Vector2Int move, out bool isOpponentContinueBeating, out CheckerData beatenChecker, out bool isCheckerTransformd)
        {
            if (!IsCheckerCanMoveAt(Data[checkerPos.x, checkerPos.y], move))
            {
                throw new Exception("not valid move.");
            }

            CheckerType lastType = Data[checkerPos.x, checkerPos.y].Type;
            MakeMove(checkerPos, move, out beatenChecker);

            bool isNextMoveContinueBeating = Data[move.x, move.y].IsCheckerNeedBeat(this);
            isCheckerTransformd = Data[move.x, move.y].Type != lastType;
            isOpponentContinueBeating = isNextMoveContinueBeating && beatenChecker != null;
            return Data[move.x, move.y];
        }

        private void MakeMove(Vector2Int checkerPos, Vector2Int move, out CheckerData beatenChecker)
        {
            int len = Math.Abs(checkerPos.x - move.x);
            Vector2Int dir = new Vector2Int(Math.Sign(move.x - checkerPos.x), Math.Sign(move.y - checkerPos.y));

            Data[move.x, move.y] = Data[checkerPos.x, checkerPos.y].Clone(move);

            beatenChecker = null;
            for (int i = 0; i < len; i++)
            {
                if (i != 0 && Data[checkerPos.x, checkerPos.y] != null)
                    beatenChecker = Data[checkerPos.x, checkerPos.y];

                Data[checkerPos.x, checkerPos.y] = null;
                checkerPos += dir;
            }


            if (beatenChecker != null)
            {
                if (beatenChecker.Opponent == OpponentType.Player)
                    PlayerCheckerCount--;
                else AICheckerCount--;
            }
        }

        public bool IsCheckerCanMoveAt(CheckerData checkerData, Vector2Int move)
        {
            IReadOnlyList<CheckerMove> moves = checkerData.GetAllMovesForChecker(this);
            return moves.Any(c => c.To.x == move.x && c.To.y == move.y);
        }

        public List<CheckerMove> GetAllPossibleMoves(OpponentType opponent)
        {
            List<CheckerMove> allMoves = new List<CheckerMove>();

            bool isOpponentNeedBeat = false;

            for (int y = 0; y < _boardHeight; y++)
            {
                for (int x = 0; x < _boardWidth; x++)
                {
                    if (Data[x, y] != null && Data[x, y].Opponent == opponent)
                    {
                        IReadOnlyList<CheckerMove> moves = Data[x, y].GetAllMovesForChecker(this);

                        if (moves.Count != 0)
                        {
                            bool isCheckerBeat = moves[0].IsBeatOpponentChecker;

                            if (!isOpponentNeedBeat && isCheckerBeat)
                            {
                                isOpponentNeedBeat = true;
                                allMoves.Clear();
                            }

                            if ((!isOpponentNeedBeat) || (isOpponentNeedBeat && isCheckerBeat))
                            {
                                allMoves.AddRange(moves);
                            }
                        }
                    }
                }
            }

            return allMoves;
        }
    }
}