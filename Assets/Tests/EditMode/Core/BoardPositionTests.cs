using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using Assets.scripts.Core;

namespace Tests.EditMode.Shared
{
    public class BoardPositionTests
    {
        private const int BOARD_WIDTH = 8;
        private const int BOARD_HEIGHT = 8;

        [Test]
        public void BoardPosition_Clone_PlayerCheckerCount_IsPreserved()
        {
            // Arrange
            var checkers = new List<CheckerData>
            {
                new CheckerData(1, 1, CheckerType.USUAL, OpponentType.Player, BOARD_HEIGHT, BOARD_WIDTH),
                new CheckerData(2, 2, CheckerType.USUAL, OpponentType.Player, BOARD_HEIGHT, BOARD_WIDTH),
                new CheckerData(3, 3, CheckerType.USUAL, OpponentType.AI, BOARD_HEIGHT, BOARD_WIDTH)
            };
            var original = new BoardPosition(checkers, BOARD_WIDTH, BOARD_HEIGHT);

            // Act
            var clone = original.Clone();

            // Assert
            Assert.AreEqual(original.PlayerCheckerCount, clone.PlayerCheckerCount,
                "Количество шашек игрока должно совпадать");
            Assert.AreEqual(original.AICheckerCount, clone.AICheckerCount,
                "Количество шашек AI должно совпадать");
        }

        [Test]
        public void BoardPosition_IsOpponentCanMove_AIOnBlackCell_HasMoves()
        {
            // Arrange
            var checkers = new List<CheckerData>
            {
                new CheckerData(1, 1, CheckerType.USUAL, OpponentType.AI, BOARD_HEIGHT, BOARD_WIDTH)
            };
            var board = new BoardPosition(checkers, BOARD_WIDTH, BOARD_HEIGHT);

            // Act
            bool canMove = board.IsOpponentCanMove(OpponentType.AI);

            // Assert
            Assert.IsTrue(canMove, "AI шашка не на краю доски должна иметь хотя бы один ход");
        }

        [Test]
        public void BoardPosition_IsOpponentCanMove_WithBlockedCheckers_ReturnsFalse()
        {
            // Arrange
            var checkers = new List<CheckerData>
            {
                new CheckerData(0, 0, CheckerType.USUAL, OpponentType.AI, BOARD_HEIGHT, BOARD_WIDTH),
                new CheckerData(1, 1, CheckerType.USUAL, OpponentType.Player, BOARD_HEIGHT, BOARD_WIDTH),
                new CheckerData(2, 2, CheckerType.USUAL, OpponentType.Player, BOARD_HEIGHT, BOARD_WIDTH)
            };
            var board = new BoardPosition(checkers, BOARD_WIDTH, BOARD_HEIGHT);

            // Act
            bool canMove = board.IsOpponentCanMove(OpponentType.AI);

            // Assert - все на последнем ряду, ходов вперёд нет
            Assert.IsFalse(canMove, "Заблокированные шашки не должны иметь ходов");
        }

        [Test]
        public void BoardPosition_IsOpponentNeedBeatChecker_ReturnsTrue_WhenBeatAvailable()
        {
            // Arrange
            var checkers = new List<CheckerData>
            {
                new CheckerData(2, 2, CheckerType.USUAL, OpponentType.AI, BOARD_HEIGHT, BOARD_WIDTH),
                new CheckerData(3, 3, CheckerType.USUAL, OpponentType.Player, BOARD_HEIGHT, BOARD_WIDTH)
            };
            var board = new BoardPosition(checkers, BOARD_WIDTH, BOARD_HEIGHT);

            // Act
            bool needBeat = board.IsOpponentNeedBeatChecker(OpponentType.AI);

            // Assert
            Assert.IsTrue(needBeat, "AI должен видеть возможность взятия");
        }

        [Test]
        public void BoardPosition_IsOpponentNeedBeatChecker_ReturnsFalse_WhenNoBeat()
        {
            // Arrange
            var checkers = new List<CheckerData>
            {
                new CheckerData(3, 3, CheckerType.USUAL, OpponentType.AI, BOARD_HEIGHT, BOARD_WIDTH)
            };
            var board = new BoardPosition(checkers, BOARD_WIDTH, BOARD_HEIGHT);

            // Act
            bool needBeat = board.IsOpponentNeedBeatChecker(OpponentType.AI);

            // Assert
            Assert.IsFalse(needBeat, "Без шашек противника нечего бить");
        }

        [Test]
        public void BoardPosition_MakeMove_UpdatesCheckerPosition()
        {
            // Arrange - AI шашка на (2,1), ход на (3,2)
            var from = new Vector2Int(2, 2);
            var to = new Vector2Int(1, 1);
            var checkers = new List<CheckerData>
            {
                new CheckerData(from.x, from.y, CheckerType.USUAL, OpponentType.AI, BOARD_HEIGHT, BOARD_WIDTH)
            };
            var board = new BoardPosition(checkers, BOARD_WIDTH, BOARD_HEIGHT);

            // Act
            var updatedChecker = board.MakeMove(from, to, out bool continueBeating, out CheckerData beaten, out bool transformed);

            // Assert
            Assert.IsNull(board.Data[from.x, from.y], $"Старая позиция ({from.x}, {from.y}) должна быть пустой");
            Assert.IsNotNull(board.Data[to.x, to.y], $"Новая позиция ({to.x}, {to.y}) должна содержать шашку");
            Assert.AreEqual(OpponentType.AI, board.Data[to.x, to.y].Opponent, "Шашка должна принадлежать AI");
            Assert.IsNotNull(updatedChecker, "MakeMove должен вернуть перемещённую шашку");
            Assert.AreEqual(to.x, updatedChecker.X, "X координата должна обновиться");
            Assert.AreEqual(to.y, updatedChecker.Y, "Y координата должна обновиться");
        }

        [Test]
        public void BoardPosition_MakeMove_Beat_RemovesOpponentChecker()
        {
            // Arrange - AI шашка на (2,1), ход на (3,2)
            var from = new Vector2Int(2, 2);
            var to = new Vector2Int(0, 0);
            var checkers = new List<CheckerData>
            {
                new CheckerData(from.x, from.y, CheckerType.USUAL, OpponentType.AI, BOARD_HEIGHT, BOARD_WIDTH),
                new CheckerData(1,1, CheckerType.USUAL, OpponentType.Player, BOARD_HEIGHT, BOARD_WIDTH)
            };
            var board = new BoardPosition(checkers, BOARD_WIDTH, BOARD_HEIGHT);

            // Act
            var updatedChecker = board.MakeMove(from, to, out bool continueBeating, out CheckerData beaten, out bool transformed);

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
        public void BoardPosition_MakeMove_PromotesToKing_WhenReachesLastRow()
        {
            // Arrange - AI шашка на предпоследнем ряду
            var from = new Vector2Int(7, 1);
            var checkers = new List<CheckerData>
            {
                new CheckerData(from.x, from.y, CheckerType.USUAL, OpponentType.AI, BOARD_HEIGHT, BOARD_WIDTH)
            };
            var board = new BoardPosition(checkers, BOARD_WIDTH, BOARD_HEIGHT);
            var to = new Vector2Int(6, 0); // Последний ряд для AI

            // Act
            var updatedChecker = board.MakeMove(from, to, out bool continueBeating, out CheckerData beaten, out bool transformed);

            // Assert
            Assert.IsTrue(transformed, "Шашка должна превратиться в дамку");
            Assert.AreEqual(CheckerType.KING, board.Data[to.x, to.y].Type, "На последнем ряду должна быть дамка");
            Assert.AreEqual(CheckerType.KING, updatedChecker.Type, "Возвращённая шашка должна быть дамкой");
        }

        [Test]
        public void BoardPosition_MakeMove_InvalidMove_ThrowsException()
        {
            // Arrange - шашка и целевая клетка не по диагонали
            var from = new Vector2Int(2, 2);
            var checkers = new List<CheckerData>
            {
                new CheckerData(from.x, from.y, CheckerType.USUAL, OpponentType.AI, BOARD_HEIGHT, BOARD_WIDTH)
            };
            var board = new BoardPosition(checkers, BOARD_WIDTH, BOARD_HEIGHT);
            var to = new Vector2Int(2, 6); // Горизонтальный ход — невалидный

            // Act & Assert
            var ex = Assert.Throws<System.Exception>(() =>
            {
                board.MakeMove(from, to, out _, out _, out _);
            }, "Должно быть выброшено исключение при невалидном ходе");
        }

        [Test]
        public void BoardPosition_GetAllPossibleMoves_ReturnsMovesInCorrectDirection()
        {
            // Arrange - AI шашка
            var checkers = new List<CheckerData>
            {
                new CheckerData(3, 3, CheckerType.USUAL, OpponentType.AI, BOARD_HEIGHT, BOARD_WIDTH)
            };
            var board = new BoardPosition(checkers, BOARD_WIDTH, BOARD_HEIGHT);

            // Act
            var moves = board.GetAllPossibleMoves(OpponentType.AI);

            // Assert
            Assert.IsTrue(moves.Count > 0, "Должны быть ходы");

            // AI ходит вниз (увеличение X)
            foreach (var move in moves)
            {
                Assert.Less(move.To.y, move.From.y, $"AI шашка должна ходить вниз, но ход: {move.From} -> {move.To}");
            }
        }

        [Test]
        public void BoardPosition_GetAllPossibleMoves_PlayerMovesUpward()
        {
            // Arrange - шашка игрока
            var checkers = new List<CheckerData>
            {
                new CheckerData(0, 0, CheckerType.USUAL, OpponentType.Player, BOARD_HEIGHT, BOARD_WIDTH)
            };
            var board = new BoardPosition(checkers, BOARD_WIDTH, BOARD_HEIGHT);

            // Act
            var moves = board.GetAllPossibleMoves(OpponentType.Player);

            // Assert
            Assert.IsTrue(moves.Count > 0, "Должны быть ходы");

            // Игрок ходит вверх (уменьшение X)
            foreach (var move in moves)
            {
                Assert.Less(move.From.y, move.To.y, $"Шашка игрока должна ходить вверх, но ход: {move.From} -> {move.To}");
            }
        }

        [Test]
        public void BoardPosition_GetAllPossibleMoves_WhenBeatAvailable_ReturnsOnlyBeats()
        {
            // Arrange - позиция с возможностью взятия и простого хода
            var checkers = new List<CheckerData>
            {
                new CheckerData(4, 4, CheckerType.USUAL, OpponentType.AI, BOARD_HEIGHT, BOARD_WIDTH),
                new CheckerData(3, 3, CheckerType.USUAL, OpponentType.Player, BOARD_HEIGHT, BOARD_WIDTH)
            };
            var board = new BoardPosition(checkers, BOARD_WIDTH, BOARD_HEIGHT);

            // Act
            var moves = board.GetAllPossibleMoves(OpponentType.AI);

            // Assert
            Assert.IsTrue(moves.Count > 0, "Должны быть ходы");
            Assert.IsTrue(moves.TrueForAll(m => m.IsBeatOpponentChecker), "Если есть взятие, все возвращаемые ходы должны быть взятиями");
        }

        [Test]
        public void BoardPosition_IsCheckerExist_ReturnsTrue_ForExistingChecker()
        {
            // Arrange
            var checker = new CheckerData(2, 2, CheckerType.USUAL, OpponentType.AI, BOARD_HEIGHT, BOARD_WIDTH);
            var checkers = new List<CheckerData> { checker };
            var board = new BoardPosition(checkers, BOARD_WIDTH, BOARD_HEIGHT);

            // Act & Assert
            Assert.IsTrue(board.IsCheckerExist(checker), "Существующая на доске шашка должна быть найдена");
        }

        [Test]
        public void BoardPosition_IsCheckerExist_ReturnsFalse_ForCheckerNotOnTheBoard()
        {
            // Arrange
            var checker = new CheckerData(2, 2, CheckerType.USUAL, OpponentType.AI, BOARD_HEIGHT, BOARD_WIDTH);
            var checkers = new List<CheckerData> { };
            var board = new BoardPosition(checkers, BOARD_WIDTH, BOARD_HEIGHT);

            // Assert
            Assert.IsFalse(board.IsCheckerExist(checker), "Шашка, которая была перемещена (исходный объект), не должна существовать на доске");
        }

        [Test]
        public void BoardPosition_IsCheckerExist_ReturnsFalse_ForNull()
        {
            // Arrange
            var checkers = new List<CheckerData>
            {
                new CheckerData(2, 2, CheckerType.USUAL, OpponentType.AI, BOARD_HEIGHT, BOARD_WIDTH)
            };
            var board = new BoardPosition(checkers, BOARD_WIDTH, BOARD_HEIGHT);

            // Act & Assert
            Assert.IsFalse(board.IsCheckerExist(null), "null не должен считаться существующей шашкой");
        }

        [Test]
        public void BoardPosition_IsCheckerExist_ReturnsFalse_ForCheckerWithWrongPosition()
        {
            // Arrange
            var checkers = new List<CheckerData>
            {
                new CheckerData(2, 2, CheckerType.USUAL, OpponentType.AI, BOARD_HEIGHT, BOARD_WIDTH)
            };
            var board = new BoardPosition(checkers, BOARD_WIDTH, BOARD_HEIGHT);

            var wrongChecker = new CheckerData(3, 3, CheckerType.USUAL, OpponentType.AI, BOARD_HEIGHT, BOARD_WIDTH);

            // Act & Assert
            Assert.IsFalse(board.IsCheckerExist(wrongChecker), "на таких координатах нет шашки, значит она не принадлежит этой доске.");
        }

        [Test]
        public void BoardPosition_IsCheckerCanMoveAt_ReturnsTrue_ForValidMove()
        {
            // Arrange
            var checker = new CheckerData(3, 3, CheckerType.USUAL, OpponentType.Player, BOARD_HEIGHT, BOARD_WIDTH);
            var checkers = new List<CheckerData> { checker };
            var board = new BoardPosition(checkers, BOARD_WIDTH, BOARD_HEIGHT);
            var validTarget = new Vector2Int(2, 4); // Диагональ вниз-вправо

            // Act & Assert
            Assert.IsTrue(board.IsCheckerCanMoveAt(checker, validTarget), "Должен быть доступен ход по диагонали вперёд");
        }

        [Test]
        public void BoardPosition_IsCheckerCanMoveAt_ReturnsFalse_ForInvalidMove()
        {
            // Arrange
            var checker = new CheckerData(3, 3, CheckerType.USUAL, OpponentType.AI, BOARD_HEIGHT, BOARD_WIDTH);
            var checkers = new List<CheckerData> { checker };
            var board = new BoardPosition(checkers, BOARD_WIDTH, BOARD_HEIGHT);
            var invalidTarget = new Vector2Int(3, 7); // Горизонтальный ход

            // Act & Assert
            Assert.IsFalse(board.IsCheckerCanMoveAt(checker, invalidTarget), "Не должен быть доступен горизонтальный ход");
        }
    }
}