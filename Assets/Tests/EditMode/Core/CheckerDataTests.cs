using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using Assets.scripts.Core;

namespace Tests.EditMode.Shared
{
    public class CheckerDataTests
    {
        private const int BOARD_WIDTH = 8;
        private const int BOARD_HEIGHT = 8;

        [Test]
        public void CheckerData_Constructor_AutoPromoteType()
        {
            // Arrange & Act
            var checkerOnLastRow = new CheckerData(0, 0, CheckerType.Usual, OpponentType.AI, BOARD_HEIGHT, BOARD_WIDTH);
            var checkerOnFirstRow = new CheckerData(7, 7, CheckerType.Usual, OpponentType.Player, BOARD_HEIGHT, BOARD_WIDTH);

            // Assert - конструктор НЕ меняет тип автоматически
            Assert.AreEqual(CheckerType.King, checkerOnLastRow.Type, "Конструктор не должен превращать шашку в дамку (это делает MakeMove)");
            Assert.AreEqual(CheckerType.King, checkerOnFirstRow.Type, "Конструктор не должен превращать шашку в дамку");
        }

        [Test]
        public void CheckerData_Constructor_UsualChecker_NotOnPromotionRow_StaysUsual()
        {
            // Arrange
            var checker = new CheckerData(3, 3, CheckerType.Usual, OpponentType.AI, BOARD_HEIGHT, BOARD_WIDTH);

            // Assert
            Assert.AreEqual(CheckerType.Usual, checker.Type, "Шашка не на последнем ряду должна оставаться обычной");
        }

        [Test]
        public void CheckerData_GetAllMovesForChecker_AICheckerMovesbackward()
        {
            // Arrange
            var checkers = new List<CheckerData>
            {
                new CheckerData(3, 3, CheckerType.Usual, OpponentType.AI, BOARD_HEIGHT, BOARD_WIDTH)
            };
            var board = new BoardPosition(checkers, BOARD_WIDTH, BOARD_HEIGHT);
            var checker = board.Data[3, 3];

            // Act
            var moves = checker.GetAllMovesForChecker(board);

            // Assert
            Assert.IsTrue(moves.Count > 0, "Шашка в центре доски должна иметь ходы");

            // AI ходит вниз (увеличение X)
            foreach (var move in moves)
            {
                Assert.Less(move.To.y, move.From.y, $"AI шашка должна ходить вниз, но ход: {move.From} -> {move.To}");
            }
        }

        [Test]
        public void CheckerData_GetAllMovesForChecker_PlayerChecker_MovesUpward()
        {
            // Arrange
            var checkers = new List<CheckerData>
            {
                new CheckerData(3, 3, CheckerType.Usual, OpponentType.Player, BOARD_HEIGHT, BOARD_WIDTH)
            };
            var board = new BoardPosition(checkers, BOARD_WIDTH, BOARD_HEIGHT);
            var checker = board.Data[3, 3];

            // Act
            var moves = checker.GetAllMovesForChecker(board);

            // Assert
            Assert.IsTrue(moves.Count > 0, "Шашка в центре доски должна иметь ходы");

            foreach (var move in moves)
            {
                Assert.Greater(move.To.y, move.From.y, $"Player шашка должна ходить вверх, но ход: {move.From} -> {move.To}");
            }
        }

        [Test]
        public void CheckerData_IsCheckerNeedBeat_ReturnsTrue_WhenCanBeatForward()
        {
            // Arrange - AI шашка на (3,3), шашка игрока на (4,2) — диагональ вниз-влево
            var checkers = new List<CheckerData>
            {
                new CheckerData(3, 3, CheckerType.Usual, OpponentType.AI, BOARD_HEIGHT, BOARD_WIDTH),
                new CheckerData(4, 2, CheckerType.Usual, OpponentType.Player, BOARD_HEIGHT, BOARD_WIDTH)
            };
            var board = new BoardPosition(checkers, BOARD_WIDTH, BOARD_HEIGHT);
            var checker = board.Data[3, 3];

            // Act
            bool needBeat = checker.IsCheckerNeedBeat(board);

            // Assert
            Assert.IsTrue(needBeat, $"Шашка AI на (3,3) должна видеть взятие шашки игрока на (4,2)");
        }

        [Test]
        public void CheckerData_IsCheckerNeedBeat_ReturnsTrue_WhenCanBeatBackward()
        {
            // Arrange
            var checkers = new List<CheckerData>
            {
                new CheckerData(3, 3, CheckerType.Usual, OpponentType.Player, BOARD_HEIGHT, BOARD_WIDTH),
                new CheckerData(2, 2, CheckerType.Usual, OpponentType.AI, BOARD_HEIGHT, BOARD_WIDTH)
            };
            var board = new BoardPosition(checkers, BOARD_WIDTH, BOARD_HEIGHT);
            var checker = board.Data[3, 3];

            // Act
            bool needBeat = checker.IsCheckerNeedBeat(board);

            // Assert
            Assert.IsTrue(needBeat, "USUAL шашка должна бить назад");
        }

        [Test]
        public void CheckerData_King_CanBeatBackward()
        {
            // Arrange
            var checkers = new List<CheckerData>
            {
                new CheckerData(1, 1, CheckerType.King, OpponentType.Player, BOARD_HEIGHT, BOARD_WIDTH),
                new CheckerData(5, 5, CheckerType.Usual, OpponentType.AI, BOARD_HEIGHT, BOARD_WIDTH)
            };
            var board = new BoardPosition(checkers, BOARD_WIDTH, BOARD_HEIGHT);
            var checker = checkers[0];

            // Act
            bool needBeat = checker.IsCheckerNeedBeat(board);

            // Assert
            Assert.IsTrue(needBeat, "KING шашка должна бить назад");
        }

        [Test]
        public void CheckerData_Clone_CreatesIndependentCopy()
        {
            // Arrange
            var original = new CheckerData(3, 3, CheckerType.Usual, OpponentType.AI, BOARD_HEIGHT, BOARD_WIDTH);

            // Act
            var clone = original.Clone(new Vector2Int(4, 4));

            // Assert
            Assert.IsNotNull(clone, "Клон не должен быть null");
            Assert.AreNotSame(original, clone, "Клон должен быть новым объектом");

            // Проверяем, что координаты обновились
            Assert.AreEqual(4, clone.X, "X клона должен быть 4");
            Assert.AreEqual(4, clone.Y, "Y клона должен быть 4");

            // Проверяем, что остальные свойства сохранились
            Assert.AreEqual(original.Type, clone.Type, "Тип должен сохраниться");
            Assert.AreEqual(original.Opponent, clone.Opponent, "Оппонент должен сохраниться");
            Assert.AreEqual(original.BoardHeight, clone.BoardHeight, "Высота доски должна сохраниться");
            Assert.AreEqual(original.BoardWidth, clone.BoardWidth, "Ширина доски должна сохраниться");

            // Проверяем независимость — изменение клона не влияет на оригинал
            clone.SetPosition(5, 5);
            Assert.AreEqual(3, original.X, "Изменение клона не должно влиять на оригинал");
        }

        [Test]
        public void CheckerData_Equals_SamePosition_ReturnsTrue()
        {
            // Arrange
            var checker1 = new CheckerData(3, 3, CheckerType.Usual, OpponentType.AI, BOARD_HEIGHT, BOARD_WIDTH);
            var checker2 = new CheckerData(3, 3, CheckerType.Usual, OpponentType.AI, BOARD_HEIGHT, BOARD_WIDTH);

            // Act & Assert
            Assert.AreEqual(checker1.X, checker2.X, "X координаты должны совпадать");
            Assert.AreEqual(checker1.Y, checker2.Y, "Y координаты должны совпадать");
            Assert.AreEqual(checker1.Type, checker2.Type, "Типы должны совпадать");
            Assert.AreEqual(checker1.Opponent, checker2.Opponent, "Оппоненты должны совпадать");
        }
          
    }
}