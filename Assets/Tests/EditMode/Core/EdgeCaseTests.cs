using NUnit.Framework;
using Assets.scripts.Core;
using System;
using System.Collections.Generic;
using Assets.scripts.GamePlay;
using UnityEngine;
using System.Threading.Tasks;

namespace Tests.EditMode.Core
{
    public class EdgeCaseTests
    {
        private const int BOARD_WIDTH = 8;
        private const int BOARD_HEIGHT = 8;

        [Test]
        public async Task EdgeCase_MinimalBoard_WorksCorrectly()
        {
            // Arrange
            const int smallWidth = GameSettings.BOARD_MIN_WIDTH;
            const int smallHeight = GameSettings.BOARD_MIN_HEIGHT;
            var checkers = new List<CheckerData>
            {
                new CheckerData(1, 1, CheckerType.USUAL, OpponentType.AI, smallHeight, smallWidth),
                new CheckerData(2, 2, CheckerType.USUAL, OpponentType.Player, smallHeight, smallWidth)
            };
            var board = new BoardPosition(checkers, smallWidth, smallHeight);
            var minimax = new MinimaxCore(smallHeight, smallWidth, 2, false);

            // Act
            var result = await minimax.GetBestMove(board, OpponentType.AI);

            // Assert
            Assert.IsTrue(result.HasValue, "На маленькой доске должен быть ход");
        }

        [Test]
        public async Task EdgeCase_AllCheckersAreKings_WorksCorrectly()
        {
            // Arrange
            var checkers = new List<CheckerData>
            {
                new CheckerData(3, 3, CheckerType.KING, OpponentType.AI, BOARD_HEIGHT, BOARD_WIDTH),
                new CheckerData(4, 4, CheckerType.KING, OpponentType.Player, BOARD_HEIGHT, BOARD_WIDTH),
                new CheckerData(5, 5, CheckerType.KING, OpponentType.AI, BOARD_HEIGHT, BOARD_WIDTH)
            };
            var board = new BoardPosition(checkers, BOARD_WIDTH, BOARD_HEIGHT);
            var minimax = new MinimaxCore(BOARD_HEIGHT, BOARD_WIDTH, 3, false);

            // Act
            var result = await minimax.GetBestMove(board, OpponentType.AI);

            // Assert
            Assert.IsTrue(result.HasValue, "Дамки должны иметь ходы");
            Assert.IsTrue(result.HasValue, "Ход должен быть непустым");
            if (result.HasValue)
            {
                var move = result.Value;
                var checker = board.Data[move.From.x, move.From.y];
                Assert.AreEqual(CheckerType.KING, checker.Type, "Перемещаемая фигура должна быть дамкой");
                Assert.IsTrue(
                    board.IsCheckerCanMoveAt(checker, move.To),
                    "Ход дамки должен быть валидным"
                );
            }
        }

        [Test]
        public async Task EdgeCase_EmptyBoard_ReturnsNoMove()
        {
            // Arrange
            var checkers = new List<CheckerData>();
            var board = new BoardPosition(checkers, BOARD_WIDTH, BOARD_HEIGHT);
            var minimax = new MinimaxCore(BOARD_HEIGHT, BOARD_WIDTH, 3, false);

            // Act
            var result = await minimax.GetBestMove(board, OpponentType.AI);

            // Assert
            Assert.IsFalse(result.HasValue, "На пустой доске не должно быть ходов");
        }

        [Test]
        public async Task EdgeCase_OnlyOneChecker_ReturnsNotNull()
        {
            // Arrange - одна шашка AI
            var checkers = new List<CheckerData>
            {
                new CheckerData(3, 3, CheckerType.USUAL, OpponentType.AI, BOARD_HEIGHT, BOARD_WIDTH)
            };
            var board = new BoardPosition(checkers, BOARD_WIDTH, BOARD_HEIGHT);
            var minimax = new MinimaxCore(BOARD_HEIGHT, BOARD_WIDTH, 3, false);

            // Act
            var result = await minimax.GetBestMove(board, OpponentType.AI);

            // Assert
            Assert.IsTrue(result.HasValue, "Одна шашка в центре должна иметь ход");
        }

        [Test]
        public async Task EdgeCase_BoardWithOnlyPlayerCheckers_AIReturnsNoMove()
        {
            // Arrange - только шашки игрока
            var checkers = new List<CheckerData>
            {
                new CheckerData(0, 0, CheckerType.USUAL, OpponentType.Player, BOARD_HEIGHT, BOARD_WIDTH),
                new CheckerData(2, 0, CheckerType.USUAL, OpponentType.Player, BOARD_HEIGHT, BOARD_WIDTH),
                new CheckerData(4, 0, CheckerType.USUAL, OpponentType.Player, BOARD_HEIGHT, BOARD_WIDTH)
            };
            var board = new BoardPosition(checkers, BOARD_WIDTH, BOARD_HEIGHT);
            var minimax = new MinimaxCore(BOARD_HEIGHT, BOARD_WIDTH, 3, false);

            // Act
            var result = await minimax.GetBestMove(board, OpponentType.AI);

            // Assert
            Assert.IsFalse(result.HasValue, "AI не должен иметь ходов, если нет шашек AI");
        }

        [Test]
        public async Task EdgeCase_BoardWithOnlyAICheckers_AIReturnsMove()
        {
            // Arrange - только шашки AI
            var checkers = new List<CheckerData>
            {
                new CheckerData(3, 3, CheckerType.USUAL, OpponentType.AI, BOARD_HEIGHT, BOARD_WIDTH),
                new CheckerData(4, 4, CheckerType.USUAL, OpponentType.AI, BOARD_HEIGHT, BOARD_WIDTH)
            };
            var board = new BoardPosition(checkers, BOARD_WIDTH, BOARD_HEIGHT);
            var minimax = new MinimaxCore(BOARD_HEIGHT, BOARD_WIDTH, 3, false);

            // Act
            var result = await minimax.GetBestMove(board, OpponentType.AI);

            // Assert
            Assert.IsTrue(result.HasValue, "Должен быть ход, если есть шашки AI");
            var move = result.Value;
            Assert.IsTrue(
                board.IsCheckerCanMoveAt(board.Data[move.From.x, move.From.y], move.To),
                "Ход должен быть валидным"
            );
        }

        [Test]
        public async Task EdgeCase_GetPointsForChecker_WithNullChecker_DoesNotCrash()
        {
            // Arrange
            var emptyCheckersList = new List<CheckerData>();
            var board = new BoardPosition(emptyCheckersList, BOARD_WIDTH, BOARD_HEIGHT);
            var minimax = new MinimaxCore(BOARD_HEIGHT, BOARD_WIDTH, 3, false);

            // Act & Assert — метод должен быть синхронным и не выбрасывать исключений
            Assert.DoesNotThrow(() =>
            {
                var points = minimax.GetPointsForChecker(board, null);
                // Дополнительно: проверяем, что метод возвращает какое-то значение
                Assert.IsNotNull(points, "Метод не должен возвращать null");
            }, "Метод должен обрабатывать null checker");
        }

    }
}