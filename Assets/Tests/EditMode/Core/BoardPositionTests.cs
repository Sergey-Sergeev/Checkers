using Assets.scripts.Core;
using NUnit.Framework;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;

namespace Tests.EditMode.Shared
{
    public class BoardPositionTests
    {
        private const int BOARD_WIDTH = 8;
        private const int BOARD_HEIGHT = 8;
        

        [Test]
        public void IsOpponentCanMove_OneCheckerInAnyPositionOnBoard_AIAndPlayerChecker()
        {
            OpponentType[] opponents = { OpponentType.AI, OpponentType.Player };

            foreach (var opponent in opponents)
            {
                for (int y = 0; y < BOARD_HEIGHT; y++)
                {
                    for (int x = 0; x < BOARD_WIDTH; x++)
                    {
                        if ((x + y) % 2 == 1) continue;

                        // Arrange
                        var checkers = new List<CheckerData>
                        {
                            new CheckerData(x, y, CheckerType.Usual, opponent, BOARD_HEIGHT, BOARD_WIDTH)
                        };
                        var board = new BoardPosition(checkers, BOARD_WIDTH, BOARD_HEIGHT);

                        // Act
                        bool canMove = board.IsOpponentCanMove(opponent);

                        // Assert
                        Assert.IsTrue(canMove, $"{opponent} must have move. ");
                    }
                }
            }
        }

        [Test]
        public void IsOpponentCanMove_IncreasingCountOfCheckersInAnyPositionOnBoard_AIAndPlayerChecker()
        {
            OpponentType[] opponents = { OpponentType.AI, OpponentType.Player };

            foreach (var opponent in opponents)
            {
                var checkers = new List<CheckerData>();

                for (int y = 0; y < BOARD_HEIGHT; y++)
                {
                    for (int x = 0; x < BOARD_WIDTH; x++)
                    {
                        if ((x + y) % 2 == 1) continue;

                        // For ai invert increasing
                        if (opponent == OpponentType.AI)
                            checkers.Add(new CheckerData(BOARD_WIDTH - x - 1, BOARD_HEIGHT - y - 1, CheckerType.Usual, opponent, BOARD_HEIGHT, BOARD_WIDTH));
                        else checkers.Add(new CheckerData(x, y, CheckerType.Usual, opponent, BOARD_HEIGHT, BOARD_WIDTH));


                        // board not full yet
                        if (checkers.Count != (BOARD_HEIGHT * BOARD_WIDTH) / 2)
                        {
                            var tempBoard = new BoardPosition(checkers, BOARD_WIDTH, BOARD_HEIGHT);

                            // Assert
                            Assert.IsTrue(tempBoard.IsOpponentCanMove(opponent), "Full board has no moves.");
                        }
                    }
                }

                var board = new BoardPosition(checkers, BOARD_WIDTH, BOARD_HEIGHT);

                // Act
                bool canMove = board.IsOpponentCanMove(opponent);

                // Assert
                Assert.IsFalse(canMove, "Full board has no moves.");

                checkers.RemoveAt(checkers.Count / 2);
                board = new BoardPosition(checkers, BOARD_WIDTH, BOARD_HEIGHT);


                canMove = board.IsOpponentCanMove(opponent);

                // Assert
                Assert.IsTrue(canMove, "There is has at least one move. ");
            }
        }

        [Test]
        public void IsOpponentCanMove_Cases()
        {
            // Data
            (List<CheckerData> checkers, OpponentType opponent)[] poses = new (List<CheckerData>, OpponentType)[]{
                // case 1
                (new List<CheckerData>()
                {
                    new CheckerData(3, 3, CheckerType.Usual, OpponentType.Player, BOARD_HEIGHT, BOARD_WIDTH),
                    new CheckerData(2, 2, CheckerType.Usual, OpponentType.Player, BOARD_HEIGHT, BOARD_WIDTH),
                    new CheckerData(1, 1, CheckerType.Usual, OpponentType.Player, BOARD_HEIGHT, BOARD_WIDTH)
                }, OpponentType.Player),

                // case 2
                (new List<CheckerData>()
                {
                    new CheckerData(3, 3, CheckerType.Usual, OpponentType.AI, BOARD_HEIGHT, BOARD_WIDTH),
                    new CheckerData(2, 2, CheckerType.Usual, OpponentType.AI, BOARD_HEIGHT, BOARD_WIDTH),
                    new CheckerData(1, 1, CheckerType.Usual, OpponentType.AI, BOARD_HEIGHT, BOARD_WIDTH)
                }, OpponentType.AI),

                // case 3
                (new List<CheckerData>()
                {
                    new CheckerData(1, 7, CheckerType.Usual, OpponentType.Player, BOARD_HEIGHT, BOARD_WIDTH)
                }, OpponentType.Player),

                // case 4
                (new List<CheckerData>()
                {
                    new CheckerData(1, 7, CheckerType.Usual, OpponentType.AI, BOARD_HEIGHT, BOARD_WIDTH)
                }, OpponentType.AI),

                // case 5
                (new List<CheckerData>()
                {
                    new CheckerData(1, 5, CheckerType.Usual, OpponentType.Player, BOARD_HEIGHT, BOARD_WIDTH),
                    new CheckerData(4, 6, CheckerType.Usual, OpponentType.Player, BOARD_HEIGHT, BOARD_WIDTH),
                    new CheckerData(5, 5, CheckerType.Usual, OpponentType.AI, BOARD_HEIGHT, BOARD_WIDTH),
                    new CheckerData(7, 7, CheckerType.Usual, OpponentType.AI, BOARD_HEIGHT, BOARD_WIDTH)
                }, OpponentType.Player),

                // case 6
                (new List<CheckerData>()
                {
                    new CheckerData(1, 5, CheckerType.Usual, OpponentType.Player, BOARD_HEIGHT, BOARD_WIDTH),
                    new CheckerData(4, 6, CheckerType.Usual, OpponentType.Player, BOARD_HEIGHT, BOARD_WIDTH),
                    new CheckerData(5, 5, CheckerType.Usual, OpponentType.AI, BOARD_HEIGHT, BOARD_WIDTH),
                    new CheckerData(7, 7, CheckerType.Usual, OpponentType.AI, BOARD_HEIGHT, BOARD_WIDTH)
                }, OpponentType.AI)
            };

            foreach (var pos in poses)
            {
                // Arrange
                var board = new BoardPosition(pos.checkers, BOARD_WIDTH, BOARD_HEIGHT);

                // Act
                bool canMove = board.IsOpponentCanMove(pos.opponent);

                // Assert
                Assert.IsTrue(canMove, $"{pos.opponent} must have move. ");
            }
        }

        [Test]
        public void IsOpponentCanMove_WithBlockedCheckers()
        {
            // Arrange
            var checkers = new List<CheckerData>
            {
                new CheckerData(0, 0, CheckerType.Usual, OpponentType.AI, BOARD_HEIGHT, BOARD_WIDTH),
                new CheckerData(1, 1, CheckerType.Usual, OpponentType.Player, BOARD_HEIGHT, BOARD_WIDTH),
                new CheckerData(2, 2, CheckerType.Usual, OpponentType.Player, BOARD_HEIGHT, BOARD_WIDTH)
            };
            var board = new BoardPosition(checkers, BOARD_WIDTH, BOARD_HEIGHT);

            // Act
            bool canMove = board.IsOpponentCanMove(OpponentType.AI);

            // Assert - все на последнем ряду, ходов вперёд нет
            Assert.IsFalse(canMove, "Blocked checkers has not moves.");
        }




        [Test]
        public void IsOpponentNeedBeatChecker_OneCheckerOnBoard()
        {
            OpponentType[] opponents = { OpponentType.AI, OpponentType.Player };

            foreach (var opponent in opponents)
            {
                for (int y = 0; y < BOARD_HEIGHT; y++)
                {
                    for (int x = 0; x < BOARD_WIDTH; x++)
                    {
                        if ((x + y) % 2 == 1) continue;

                        // Arrange
                        var checkers = new List<CheckerData>
                        {
                            new CheckerData(x, y, CheckerType.Usual, opponent, BOARD_HEIGHT, BOARD_WIDTH)
                        };
                        var board = new BoardPosition(checkers, BOARD_WIDTH, BOARD_HEIGHT);

                        // Act
                        bool needBeat = board.IsOpponentNeedBeatChecker(opponent);

                        // Assert
                        Assert.IsFalse(needBeat, $"{opponent} cant beat. ");
                    }
                }
            }
        }

        [Test]
        public void IsOpponentNeedBeatChecker_IncreasingCountOfCheckersInAnyPositionOnBoard_AIAndPlayerChecker()
        {
            OpponentType[] opponents = { OpponentType.AI, OpponentType.Player };

            foreach (var opponent in opponents)
            {
                var checkers = new List<CheckerData>();

                for (int y = 0; y < BOARD_HEIGHT; y++)
                {
                    for (int x = 0; x < BOARD_WIDTH; x++)
                    {
                        if ((x + y) % 2 == 1) continue;

                        // For ai invert increasing
                        if (opponent == OpponentType.AI)
                            checkers.Add(new CheckerData(BOARD_WIDTH - x - 1, BOARD_HEIGHT - y - 1, CheckerType.Usual, opponent, BOARD_HEIGHT, BOARD_WIDTH));
                        else checkers.Add(new CheckerData(x, y, CheckerType.Usual, opponent, BOARD_HEIGHT, BOARD_WIDTH));

                        var tempBoard = new BoardPosition(checkers, BOARD_WIDTH, BOARD_HEIGHT);

                        // Assert
                        Assert.IsFalse(tempBoard.IsOpponentNeedBeatChecker(opponent), "Opponent cant beat, because there are not antagosint checkers.");
                    }
                }
            }
        }

        [Test]
        public void IsOpponentNeedBeatChecker_Cases()
        {
            // Data
            (List<CheckerData> checkers, OpponentType opponent)[] poses = new (List<CheckerData>, OpponentType)[]{
                // case 1
                (new List<CheckerData>()
                {
                    new CheckerData(3, 3, CheckerType.Usual, OpponentType.Player, BOARD_HEIGHT, BOARD_WIDTH),
                    new CheckerData(2, 2, CheckerType.Usual, OpponentType.Player, BOARD_HEIGHT, BOARD_WIDTH),
                    new CheckerData(1, 1, CheckerType.Usual, OpponentType.Player, BOARD_HEIGHT, BOARD_WIDTH),

                    new CheckerData(4, 4, CheckerType.Usual, OpponentType.AI, BOARD_HEIGHT, BOARD_WIDTH)
                }, OpponentType.Player),

                // case 2
                (new List<CheckerData>()
                {
                    new CheckerData(3, 3, CheckerType.Usual, OpponentType.AI, BOARD_HEIGHT, BOARD_WIDTH),
                    new CheckerData(1, 1, CheckerType.Usual, OpponentType.Player, BOARD_HEIGHT, BOARD_WIDTH),

                    new CheckerData(4, 4, CheckerType.Usual, OpponentType.Player, BOARD_HEIGHT, BOARD_WIDTH)
                }, OpponentType.AI),

                // case 3
                (new List<CheckerData>()
                {
                    new CheckerData(1, 7, CheckerType.Usual, OpponentType.Player, BOARD_HEIGHT, BOARD_WIDTH),
                    new CheckerData(6, 2, CheckerType.Usual, OpponentType.AI, BOARD_HEIGHT, BOARD_WIDTH)
                }, OpponentType.Player),

                // case 4
                (new List<CheckerData>()
                {
                    new CheckerData(2, 6, CheckerType.Usual, OpponentType.Player, BOARD_HEIGHT, BOARD_WIDTH),
                    new CheckerData(6, 2, CheckerType.King, OpponentType.AI, BOARD_HEIGHT, BOARD_WIDTH)
                }, OpponentType.AI),

                // case 5
                (new List<CheckerData>()
                {
                    new CheckerData(1, 5, CheckerType.Usual, OpponentType.Player, BOARD_HEIGHT, BOARD_WIDTH),
                    new CheckerData(4, 6, CheckerType.Usual, OpponentType.Player, BOARD_HEIGHT, BOARD_WIDTH),
                    new CheckerData(5, 7, CheckerType.Usual, OpponentType.AI, BOARD_HEIGHT, BOARD_WIDTH),
                    new CheckerData(2, 4, CheckerType.Usual, OpponentType.AI, BOARD_HEIGHT, BOARD_WIDTH)
                }, OpponentType.Player),

                // case 6
                (new List<CheckerData>()
                {
                    new CheckerData(1, 5, CheckerType.Usual, OpponentType.Player, BOARD_HEIGHT, BOARD_WIDTH),
                    new CheckerData(4, 6, CheckerType.Usual, OpponentType.Player, BOARD_HEIGHT, BOARD_WIDTH),
                    new CheckerData(5, 7, CheckerType.Usual, OpponentType.AI, BOARD_HEIGHT, BOARD_WIDTH),
                    new CheckerData(7, 7, CheckerType.Usual, OpponentType.AI, BOARD_HEIGHT, BOARD_WIDTH)
                }, OpponentType.AI)
            };

            int i = 1;
            foreach (var pos in poses)
            {
                // Arrange
                var board = new BoardPosition(pos.checkers, BOARD_WIDTH, BOARD_HEIGHT);

                // Act
                bool needBeat = board.IsOpponentNeedBeatChecker(pos.opponent);

                // Assert
                Assert.IsTrue(needBeat, $"{pos.opponent} must beat. {i++}");
            }
        }




        [Test]
        public void MakeMove_UpdatesCheckerPosition()
        {
            // Arrange - AI шашка на (2,1), ход на (3,2)
            var from = new Vector2Int(2, 2);
            var to = new Vector2Int(1, 1);
            var checkers = new List<CheckerData>
            {
                new CheckerData(from.x, from.y, CheckerType.Usual, OpponentType.AI, BOARD_HEIGHT, BOARD_WIDTH)
            };
            var board = new BoardPosition(checkers, BOARD_WIDTH, BOARD_HEIGHT);

            // Act
            var updatedChecker = board.MakeMove(from, to, out CheckerData beaten, out bool transformed);

            // Assert
            Assert.IsNull(board.Data[from.x, from.y], $"Старая позиция ({from.x}, {from.y}) должна быть пустой");
            Assert.IsNotNull(board.Data[to.x, to.y], $"Новая позиция ({to.x}, {to.y}) должна содержать шашку");
            Assert.AreEqual(OpponentType.AI, board.Data[to.x, to.y].Opponent, "Шашка должна принадлежать AI");
            Assert.IsNotNull(updatedChecker, "MakeMove должен вернуть перемещённую шашку");
            Assert.AreEqual(to.x, updatedChecker.X, "X координата должна обновиться");
            Assert.AreEqual(to.y, updatedChecker.Y, "Y координата должна обновиться");
        }

        [Test]
        public void MakeMove_Beat_RemovesOpponentChecker()
        {
            // Arrange - AI шашка на (2,1), ход на (3,2)
            var from = new Vector2Int(2, 2);
            var to = new Vector2Int(0, 0);
            var checkers = new List<CheckerData>
            {
                new CheckerData(from.x, from.y, CheckerType.Usual, OpponentType.AI, BOARD_HEIGHT, BOARD_WIDTH),
                new CheckerData(1,1, CheckerType.Usual, OpponentType.Player, BOARD_HEIGHT, BOARD_WIDTH)
            };
            var board = new BoardPosition(checkers, BOARD_WIDTH, BOARD_HEIGHT);

            // Act
            var updatedChecker = board.MakeMove(from, to, out CheckerData beaten, out bool transformed);

            // Assert
            Assert.IsNull(board.Data[from.x, from.y], $"Старая позиция ({from.x}, {from.y}) должна быть пустой");
            Assert.IsNotNull(board.Data[to.x, to.y], $"Новая позиция ({to.x}, {to.y}) должна содержать шашку");
            Assert.IsNotNull(beaten, $"Метод должен вернуть сбитую шашку");
            Assert.AreEqual(OpponentType.AI, board.Data[to.x, to.y].Opponent, "Шашка должна принадлежать AI");
            Assert.IsNotNull(updatedChecker, "MakeMove должен вернуть перемещённую шашку");
            Assert.AreEqual(to.x, updatedChecker.X, "X координата должна обновиться");
            Assert.AreEqual(to.y, updatedChecker.Y, "Y координата должна обновиться");
        }

        [Test]
        public void MakeMove_PromotesToKing_WhenReachesLastRow()
        {
            // Arrange - AI шашка на предпоследнем ряду
            var from = new Vector2Int(7, 1);
            var checkers = new List<CheckerData>
            {
                new CheckerData(from.x, from.y, CheckerType.Usual, OpponentType.AI, BOARD_HEIGHT, BOARD_WIDTH)
            };
            var board = new BoardPosition(checkers, BOARD_WIDTH, BOARD_HEIGHT);
            var to = new Vector2Int(6, 0); // Последний ряд для AI

            // Act
            var updatedChecker = board.MakeMove(from, to, out CheckerData beaten, out bool transformed);

            // Assert
            Assert.IsTrue(transformed, "Шашка должна превратиться в дамку");
            Assert.AreEqual(CheckerType.King, board.Data[to.x, to.y].Type, "На последнем ряду должна быть дамка");
            Assert.AreEqual(CheckerType.King, updatedChecker.Type, "Возвращённая шашка должна быть дамкой");
        }

        [Test]
        public void MakeMove_InvalidMove_ThrowsException()
        {
            // Arrange - шашка и целевая клетка не по диагонали
            var from = new Vector2Int(2, 2);
            var checkers = new List<CheckerData>
            {
                new CheckerData(from.x, from.y, CheckerType.Usual, OpponentType.AI, BOARD_HEIGHT, BOARD_WIDTH)
            };
            var board = new BoardPosition(checkers, BOARD_WIDTH, BOARD_HEIGHT);
            var to = new Vector2Int(2, 6); // Горизонтальный ход — невалидный

            // Act & Assert
            var ex = Assert.Throws<System.Exception>(() =>
            {
                board.MakeMove(from, to, out _, out _);
            }, "Должно быть выброшено исключение при невалидном ходе");
        }




        [Test]
        public void GetAllPossibleMoves_Cases()
        {
            // Data
            (List<CheckerData> checkers, OpponentType opponent, CheckerMove[] moves)[] cases = new (List<CheckerData>, OpponentType, CheckerMove[])[]{
                // Case 1
                (new List<CheckerData>()
                {
                    new CheckerData(3, 3, CheckerType.Usual, OpponentType.Player, BOARD_HEIGHT, BOARD_WIDTH)
                },
                OpponentType.Player,
                new CheckerMove[]{
                    new CheckerMove(new Vector2Int(3,3), new Vector2Int(2,4), OpponentType.Player, false),
                    new CheckerMove(new Vector2Int(3,3), new Vector2Int(4,4), OpponentType.Player, false)
                }),

                // Case 2
                (new List<CheckerData>()
                {
                    new CheckerData(4, 4, CheckerType.Usual, OpponentType.AI, BOARD_HEIGHT, BOARD_WIDTH)
                },
                OpponentType.AI,
                new CheckerMove[]{
                    new CheckerMove(new Vector2Int(4,4), new Vector2Int(3,3), OpponentType.AI, false),
                    new CheckerMove(new Vector2Int(4,4), new Vector2Int(5,3), OpponentType.AI, false)
                }),

                // Case 3
                (new List<CheckerData>()
                {
                    new CheckerData(1, 3, CheckerType.Usual, OpponentType.Player, BOARD_HEIGHT, BOARD_WIDTH)
                },
                OpponentType.Player,
                new CheckerMove[]{
                    new CheckerMove(new Vector2Int(1,3), new Vector2Int(0,4), OpponentType.Player, false),
                    new CheckerMove(new Vector2Int(1,3), new Vector2Int(2,4), OpponentType.Player, false)
                }),

                // Case 4
                (new List<CheckerData>()
                {
                    new CheckerData(6, 4, CheckerType.Usual, OpponentType.AI, BOARD_HEIGHT, BOARD_WIDTH)
                },
                OpponentType.AI,
                new CheckerMove[]{
                    new CheckerMove(new Vector2Int(6,4), new Vector2Int(5,3), OpponentType.AI, false),
                    new CheckerMove(new Vector2Int(6,4), new Vector2Int(7,3), OpponentType.AI, false)
                }),

                // Case 5
                (new List<CheckerData>()
                {
                    new CheckerData(1, 5, CheckerType.Usual, OpponentType.Player, BOARD_HEIGHT, BOARD_WIDTH)
                },
                OpponentType.Player,
                new CheckerMove[]{
                    new CheckerMove(new Vector2Int(1,5), new Vector2Int(0,6), OpponentType.Player, false),
                    new CheckerMove(new Vector2Int(1,5), new Vector2Int(2,6), OpponentType.Player, false)
                }),

                // Case 6
                (new List<CheckerData>()
                {
                    new CheckerData(3, 3, CheckerType.Usual, OpponentType.Player, BOARD_HEIGHT, BOARD_WIDTH),
                    new CheckerData(2, 4, CheckerType.Usual, OpponentType.AI, BOARD_HEIGHT, BOARD_WIDTH)
                },
                OpponentType.Player,
                new CheckerMove[]{
                    new CheckerMove(new Vector2Int(3,3), new Vector2Int(1,5), OpponentType.Player, true)
                }),

                // Case 7
                (new List<CheckerData>()
                {
                    new CheckerData(4, 4, CheckerType.Usual, OpponentType.AI, BOARD_HEIGHT, BOARD_WIDTH),
                    new CheckerData(3, 3, CheckerType.Usual, OpponentType.Player, BOARD_HEIGHT, BOARD_WIDTH)
                },
                OpponentType.AI,
                new CheckerMove[]{
                    new CheckerMove(new Vector2Int(4,4), new Vector2Int(2,2), OpponentType.AI, true)
                }),

                // Case 8
                (new List<CheckerData>()
                {
                    new CheckerData(3, 3, CheckerType.Usual, OpponentType.Player, BOARD_HEIGHT, BOARD_WIDTH),
                    new CheckerData(2, 4, CheckerType.Usual, OpponentType.AI, BOARD_HEIGHT, BOARD_WIDTH),
                    new CheckerData(4, 4, CheckerType.Usual, OpponentType.AI, BOARD_HEIGHT, BOARD_WIDTH)
                },
                OpponentType.Player,
                new CheckerMove[]{
                    new CheckerMove(new Vector2Int(3,3), new Vector2Int(1,5), OpponentType.Player, true),
                    new CheckerMove(new Vector2Int(3,3), new Vector2Int(5,5), OpponentType.Player, true)
                }),

                // Case 9
                (new List<CheckerData>()
                {
                    new CheckerData(4, 4, CheckerType.Usual, OpponentType.AI, BOARD_HEIGHT, BOARD_WIDTH),
                    new CheckerData(3, 3, CheckerType.Usual, OpponentType.Player, BOARD_HEIGHT, BOARD_WIDTH),
                    new CheckerData(5, 3, CheckerType.Usual, OpponentType.Player, BOARD_HEIGHT, BOARD_WIDTH)
                },
                OpponentType.AI,
                new CheckerMove[]{
                    new CheckerMove(new Vector2Int(4,4), new Vector2Int(2,2), OpponentType.AI, true),
                    new CheckerMove(new Vector2Int(4,4), new Vector2Int(6,2), OpponentType.AI, true)
                }),

                // Case 10
                (new List<CheckerData>()
                {
                    new CheckerData(3, 3, CheckerType.Usual, OpponentType.Player, BOARD_HEIGHT, BOARD_WIDTH),
                    new CheckerData(2, 4, CheckerType.Usual, OpponentType.AI, BOARD_HEIGHT, BOARD_WIDTH),
                    new CheckerData(5, 5, CheckerType.Usual, OpponentType.Player, BOARD_HEIGHT, BOARD_WIDTH)
                },
                OpponentType.Player,
                new CheckerMove[]{
                    new CheckerMove(new Vector2Int(3,3), new Vector2Int(1,5), OpponentType.Player, true)
                }),

                // Case 11
                (new List<CheckerData>()
                {
                    new CheckerData(4, 4, CheckerType.Usual, OpponentType.AI, BOARD_HEIGHT, BOARD_WIDTH),
                    new CheckerData(3, 3, CheckerType.Usual, OpponentType.Player, BOARD_HEIGHT, BOARD_WIDTH),
                    new CheckerData(6, 2, CheckerType.Usual, OpponentType.AI, BOARD_HEIGHT, BOARD_WIDTH)
                },
                OpponentType.AI,
                new CheckerMove[]{
                    new CheckerMove(new Vector2Int(4,4), new Vector2Int(2,2), OpponentType.AI, true)
                }),

                // Case 12
                (new List<CheckerData>()
                {
                    new CheckerData(3, 3, CheckerType.Usual, OpponentType.Player, BOARD_HEIGHT, BOARD_WIDTH),
                    new CheckerData(2, 4, CheckerType.Usual, OpponentType.Player, BOARD_HEIGHT, BOARD_WIDTH),
                    new CheckerData(4, 4, CheckerType.Usual, OpponentType.Player, BOARD_HEIGHT, BOARD_WIDTH)
                },
                OpponentType.AI,
                new CheckerMove[]{}),

                // Case 13
                (new List<CheckerData>()
                {
                    new CheckerData(4, 4, CheckerType.Usual, OpponentType.AI, BOARD_HEIGHT, BOARD_WIDTH),
                    new CheckerData(3, 3, CheckerType.Usual, OpponentType.AI, BOARD_HEIGHT, BOARD_WIDTH),
                    new CheckerData(5, 3, CheckerType.Usual, OpponentType.AI, BOARD_HEIGHT, BOARD_WIDTH)
                },
                OpponentType.Player,
                new CheckerMove[]{}),

                // Case 14
                (new List<CheckerData>()
                {
                    new CheckerData(3, 3, CheckerType.King, OpponentType.Player, BOARD_HEIGHT, BOARD_WIDTH),

                    new CheckerData(0, 0, CheckerType.Usual, OpponentType.AI, BOARD_HEIGHT, BOARD_WIDTH),
                    new CheckerData(1, 1, CheckerType.Usual, OpponentType.AI, BOARD_HEIGHT, BOARD_WIDTH),
                    new CheckerData(5, 5, CheckerType.Usual, OpponentType.AI, BOARD_HEIGHT, BOARD_WIDTH),
                    new CheckerData(6, 6, CheckerType.Usual, OpponentType.AI, BOARD_HEIGHT, BOARD_WIDTH),
                    new CheckerData(5, 1, CheckerType.Usual, OpponentType.AI, BOARD_HEIGHT, BOARD_WIDTH),
                    new CheckerData(6, 0, CheckerType.Usual, OpponentType.AI, BOARD_HEIGHT, BOARD_WIDTH),
                    new CheckerData(1, 5, CheckerType.Usual, OpponentType.AI, BOARD_HEIGHT, BOARD_WIDTH),
                    new CheckerData(0, 6, CheckerType.Usual, OpponentType.AI, BOARD_HEIGHT, BOARD_WIDTH),
                },
                OpponentType.Player,
                new CheckerMove[]{
                    new CheckerMove(new Vector2Int(3,3), new Vector2Int(2,4), OpponentType.Player, false),
                    new CheckerMove(new Vector2Int(3,3), new Vector2Int(4,4), OpponentType.Player, false),
                    new CheckerMove(new Vector2Int(3,3), new Vector2Int(2,2), OpponentType.Player, false),
                    new CheckerMove(new Vector2Int(3,3), new Vector2Int(4,2), OpponentType.Player, false)
                }),

                // Case 15
                (new List<CheckerData>()
                {
                    new CheckerData(3, 3, CheckerType.King, OpponentType.Player, BOARD_HEIGHT, BOARD_WIDTH),
                    new CheckerData(2, 4, CheckerType.Usual, OpponentType.AI, BOARD_HEIGHT, BOARD_WIDTH)
                },
                OpponentType.Player,
                new CheckerMove[]{
                    new CheckerMove(new Vector2Int(3,3), new Vector2Int(1,5), OpponentType.Player, true),
                    new CheckerMove(new Vector2Int(3,3), new Vector2Int(0,6), OpponentType.Player, true)
                }),

                // Case 16
                (new List<CheckerData>()
                {
                    new CheckerData(4, 4, CheckerType.King, OpponentType.AI, BOARD_HEIGHT, BOARD_WIDTH),
                    new CheckerData(3, 3, CheckerType.Usual, OpponentType.Player, BOARD_HEIGHT, BOARD_WIDTH),
                    new CheckerData(1, 1, CheckerType.Usual, OpponentType.AI, BOARD_HEIGHT, BOARD_WIDTH)
                },
                OpponentType.AI,
                new CheckerMove[]{
                    new CheckerMove(new Vector2Int(4,4), new Vector2Int(2,2), OpponentType.AI, true)
                }),

                // Case 17
                (new List<CheckerData>()
                {
                    new CheckerData(0, 2, CheckerType.Usual, OpponentType.Player, BOARD_HEIGHT, BOARD_WIDTH)
                },
                OpponentType.Player,
                new CheckerMove[]{
                    new CheckerMove(new Vector2Int(0,2), new Vector2Int(1,3), OpponentType.Player, false)
                }),

                // Case 18
                (new List<CheckerData>()
                {
                    new CheckerData(7, 5, CheckerType.Usual, OpponentType.AI, BOARD_HEIGHT, BOARD_WIDTH)
                },
                OpponentType.AI,
                new CheckerMove[]{
                    new CheckerMove(new Vector2Int(7,5), new Vector2Int(6,4), OpponentType.AI, false)
                }),

                // Case 19
                (new List<CheckerData>()
                {
                    new CheckerData(2, 6, CheckerType.Usual, OpponentType.Player, BOARD_HEIGHT, BOARD_WIDTH)
                },
                OpponentType.Player,
                new CheckerMove[]{
                    new CheckerMove(new Vector2Int(2,6), new Vector2Int(1,7), OpponentType.Player, false),
                    new CheckerMove(new Vector2Int(2,6), new Vector2Int(3,7), OpponentType.Player, false)
                }),

                // Case 20
                (new List<CheckerData>()
                {
                    new CheckerData(3, 1, CheckerType.Usual, OpponentType.AI, BOARD_HEIGHT, BOARD_WIDTH)
                },
                OpponentType.AI,
                new CheckerMove[]{
                    new CheckerMove(new Vector2Int(3,1), new Vector2Int(2,0), OpponentType.AI, false),
                    new CheckerMove(new Vector2Int(3,1), new Vector2Int(4,0), OpponentType.AI, false)
                }),

                // Case 21
                (new List<CheckerData>()
                {
                    new CheckerData(3, 3, CheckerType.Usual, OpponentType.Player, BOARD_HEIGHT, BOARD_WIDTH),
                    new CheckerData(2, 4, CheckerType.Usual, OpponentType.AI, BOARD_HEIGHT, BOARD_WIDTH),
                    new CheckerData(4, 6, CheckerType.Usual, OpponentType.AI, BOARD_HEIGHT, BOARD_WIDTH),
                    new CheckerData(1, 5, CheckerType.Usual, OpponentType.Player, BOARD_HEIGHT, BOARD_WIDTH)
                },
                OpponentType.Player,
                new CheckerMove[]{
                    new CheckerMove(new Vector2Int(3,3), new Vector2Int(4,4), OpponentType.Player, false),
                    new CheckerMove(new Vector2Int(1,5), new Vector2Int(0,6), OpponentType.Player, false),
                    new CheckerMove(new Vector2Int(1,5), new Vector2Int(2,6), OpponentType.Player, false)
                })
            };

            int i = 1;
            foreach (var c in cases)
            {
                // Arrange
                var board = new BoardPosition(c.checkers, BOARD_WIDTH, BOARD_HEIGHT);

                // Act
                List<CheckerMove> moves = board.GetAllPossibleMoves(c.opponent);

                // Assert
                Assert.IsTrue(moves.Except(c.moves).Count() == 0 && c.moves.Except(moves).Count() == 0, $"Case fail. {i++}");
            }
        }




        [Test]
        public void IsCheckerExist_ReturnsTrue_ForExistingChecker()
        {
            // Arrange
            var checker = new CheckerData(2, 2, CheckerType.Usual, OpponentType.AI, BOARD_HEIGHT, BOARD_WIDTH);
            var checkers = new List<CheckerData> { checker };
            var board = new BoardPosition(checkers, BOARD_WIDTH, BOARD_HEIGHT);

            // Act & Assert
            Assert.IsTrue(board.IsCheckerExist(checker), "Существующая на доске шашка должна быть найдена");
        }

        [Test]
        public void IsCheckerExist_ReturnsFalse_ForCheckerNotOnTheBoard()
        {
            // Arrange
            var checker = new CheckerData(2, 2, CheckerType.Usual, OpponentType.AI, BOARD_HEIGHT, BOARD_WIDTH);
            var checkers = new List<CheckerData> { };
            var board = new BoardPosition(checkers, BOARD_WIDTH, BOARD_HEIGHT);

            // Assert
            Assert.IsFalse(board.IsCheckerExist(checker), "Шашка, которая была перемещена (исходный объект), не должна существовать на доске");
        }

        [Test]
        public void IsCheckerExist_ReturnsFalse_ForNull()
        {
            // Arrange
            var checkers = new List<CheckerData>
            {
                new CheckerData(2, 2, CheckerType.Usual, OpponentType.AI, BOARD_HEIGHT, BOARD_WIDTH)
            };
            var board = new BoardPosition(checkers, BOARD_WIDTH, BOARD_HEIGHT);

            // Act & Assert
            Assert.IsFalse(board.IsCheckerExist(null), "null не должен считаться существующей шашкой");
        }

        [Test]
        public void IsCheckerExist_ReturnsFalse_ForCheckerWithWrongPosition()
        {
            // Arrange
            var checkers = new List<CheckerData>
            {
                new CheckerData(2, 2, CheckerType.Usual, OpponentType.AI, BOARD_HEIGHT, BOARD_WIDTH)
            };
            var board = new BoardPosition(checkers, BOARD_WIDTH, BOARD_HEIGHT);

            var wrongChecker = new CheckerData(3, 3, CheckerType.Usual, OpponentType.AI, BOARD_HEIGHT, BOARD_WIDTH);

            // Act & Assert
            Assert.IsFalse(board.IsCheckerExist(wrongChecker), "на таких координатах нет шашки, значит она не принадлежит этой доске.");
        }




        [Test]
        public void IsCheckerCanMoveAt_ReturnsTrue_ForValidMove()
        {
            // Arrange
            var checker = new CheckerData(3, 3, CheckerType.Usual, OpponentType.Player, BOARD_HEIGHT, BOARD_WIDTH);
            var checkers = new List<CheckerData> { checker };
            var board = new BoardPosition(checkers, BOARD_WIDTH, BOARD_HEIGHT);
            var validTarget = new Vector2Int(2, 4);

            // Act & Assert
            Assert.IsTrue(board.IsCheckerCanMoveAt(checker, validTarget), "Должен быть доступен ход по диагонали вперёд");
        }

        [Test]
        public void IsCheckerCanMoveAt_ReturnsFalse_ForInvalidMove()
        {
            // Arrange
            var checker = new CheckerData(3, 3, CheckerType.Usual, OpponentType.AI, BOARD_HEIGHT, BOARD_WIDTH);
            var checkers = new List<CheckerData> { checker };
            var board = new BoardPosition(checkers, BOARD_WIDTH, BOARD_HEIGHT);
            var invalidTarget = new Vector2Int(3, 7);

            // Act & Assert
            Assert.IsFalse(board.IsCheckerCanMoveAt(checker, invalidTarget), "Не должен быть доступен горизонтальный ход");
        }
    }
}