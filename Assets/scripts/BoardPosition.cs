using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets
{
    public class BoardPosition
    {
        public CheckerData?[,] Data { get; private set; }
        public int PlayerCheckerCount { get; private set; }
        public int AICheckerCount { get; private set; }

        public BoardPosition(List<Checker> checkers)
        {
            Data = new CheckerData[CheckersBoard.WIDTH, CheckersBoard.HEIGHT];

            for (int y = 0; y < CheckersBoard.HEIGHT; y++)
                for (int x = 0; x < CheckersBoard.WIDTH; x++)
                    Data[x, y] = null;

            for (int i = 0; i < checkers.Count; i++)
            {
                Data[checkers[i].Data.X, checkers[i].Data.Y] = checkers[i].Data;

                if (checkers[i].Data.Opponent == OpponentType.Player)
                    PlayerCheckerCount++;
                else AICheckerCount++;
            }
        }

        private BoardPosition() { }

        public BoardPosition Clone()
        {
            BoardPosition clone = new BoardPosition();
            clone.Data = new CheckerData[CheckersBoard.WIDTH, CheckersBoard.HEIGHT];

            for (int y = 0; y < CheckersBoard.HEIGHT; y++)
            {
                for (int x = 0; x < CheckersBoard.WIDTH; x++)
                {
                    if (Data[x, y] != null)
                    {
                        clone.Data[x, y] = Data[x, y].Clone();
                    }
                    else
                    {
                        clone.Data[x, y] = null;
                    }
                }
            }

            clone.PlayerCheckerCount = PlayerCheckerCount;
            clone.AICheckerCount = AICheckerCount;
            return clone;
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
            if ((opponent == OpponentType.Player && PlayerCheckerCount == 0) || (opponent == OpponentType.AI && AICheckerCount == 0))
                return false;

            for (int y = 0; y < CheckersBoard.HEIGHT; y++)
                for (int x = 0; x < CheckersBoard.WIDTH; x++)
                    if (Data[x, y] != null && Data[x, y].Opponent == opponent && Data[x, y].GetAllMovesForChecker(this).Count > 0)
                        return true;
            return false;
        }

        public bool IsOpponentNeedBeatChecker(OpponentType opponent)
        {
            for (int y = 0; y < CheckersBoard.HEIGHT; y++)
                for (int x = 0; x < CheckersBoard.WIDTH; x++)
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
                checkerData.X < CheckersBoard.WIDTH &&
                checkerData.Y < CheckersBoard.HEIGHT &&
                Data[checkerData.X, checkerData.Y] == checkerData;
        }


        public void MakeMove(CheckerData checkerData, Vector2Int move, out bool isOpponentContinueBeating, out CheckerData beatenChecker, out bool isCheckerTransformd)
        {
            CheckerType lastType = checkerData.Type;

            MakeMove(checkerData, move, out beatenChecker);

            bool isNextMoveContinueBeating = Data[move.x, move.y].IsCheckerNeedBeat(this);
            isCheckerTransformd = checkerData.Type != lastType;
            isOpponentContinueBeating = isNextMoveContinueBeating && beatenChecker != null;
        }

        private void MakeMove(CheckerData checkerData, Vector2Int move, out CheckerData beatenChecker)
        {
            int len = Math.Abs(checkerData.X - move.x);
            Vector2Int dir = new Vector2Int(Math.Sign(move.x - checkerData.X), Math.Sign(move.y - checkerData.Y));
            Vector2Int cur = new Vector2Int(checkerData.X, checkerData.Y);


            //Debug.Log($"len = {len}, dir = {dir.x}, {dir.y}, cur = {cur.x}, {cur.y}, ch = {checkerData.X}, {checkerData.Y}, move = {move.x}, {move.y}");

            beatenChecker = null;
            for (int i = 0; i < len; i++)
            {
                if (i != 0 && Data[cur.x, cur.y] != null)
                    beatenChecker = Data[cur.x, cur.y];

                Data[cur.x, cur.y] = null;
                cur += dir;
            }

            if (beatenChecker != null)
            {
                if (beatenChecker.Opponent == OpponentType.Player)
                    PlayerCheckerCount--;
                else AICheckerCount--;
            }

            checkerData.SetPosition(move.x, move.y);

            Data[move.x, move.y] = checkerData;
        }

        public bool IsCheckerCanMoveAt(CheckerData checkerData, Vector2Int move)
        {
            List<CheckerMove> moves = checkerData.GetAllMovesForChecker(this);
            return moves.Count > 0 && moves.Any(c => c.To.x == move.x && c.To.y == move.y);
        }

        public List<CheckerMove> GetAllPossibleMoves(OpponentType opponent)
        {
            List<CheckerMove> allMoves = new List<CheckerMove>();

            bool isOpponentNeedBeat = false;

            for (int y = 0; y < CheckersBoard.HEIGHT; y++)
            {
                for (int x = 0; x < CheckersBoard.WIDTH; x++)
                {
                    if (Data[x, y] != null && Data[x, y].Opponent == opponent)
                    {
                        List<CheckerMove> moves = Data[x, y].GetAllMovesForChecker(this);

                        if (moves.Count != 0)
                        {
                            bool isCheckerBeat = moves[0].IsBeatOpponentChecker;

                            if (!isOpponentNeedBeat && isCheckerBeat)
                            {
                                isOpponentNeedBeat = true;
                                allMoves.RemoveAll(m => !m.IsBeatOpponentChecker);
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