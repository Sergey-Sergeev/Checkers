using NUnit.Framework;
using Assets.scripts.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Assets.scripts.GamePlay;

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
            Assert.IsNotNull(result.move, "На маленькой доске должен быть ход");
        }

        [Test]
        public async Task EdgeCase_MaximumDepth_DoesNotStackOverflow()
        {
            // Arrange
            var checkers = new List<CheckerData>
            {
                new CheckerData(3, 3, CheckerType.USUAL, OpponentType.AI, BOARD_HEIGHT, BOARD_WIDTH),
                new CheckerData(5, 5, CheckerType.USUAL, OpponentType.Player, BOARD_HEIGHT, BOARD_WIDTH)
            };
            var board = new BoardPosition(checkers, BOARD_WIDTH, BOARD_HEIGHT);
            var minimaxDeep = new MinimaxCore(BOARD_HEIGHT, BOARD_WIDTH, GameSettings.AI_MAX_SEARCH_DEEP, false);

            // Act & Assert — проверяем, что нет переполнения стека
            try
            {
                var result = await minimaxDeep.GetBestMove(board, OpponentType.AI);
                Assert.IsNotNull(result.move, "Глубокий поиск должен завершиться без ошибок");
            }
            catch (InsufficientMemoryException) // или StackOverflowException
            {
                Assert.Fail("Глубокий поиск вызвал переполнение стека");
            }
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
            Assert.IsNotNull(result.move, "Дамки должны иметь ходы");
            Assert.IsTrue(result.move.HasValue, "Ход должен быть непустым");
            if (result.move.HasValue)
            {
                var move = result.move.Value;
                var checker = board.Data[move.From.x, move.From.y];
                Assert.AreEqual(CheckerType.KING, checker.Type, "Перемещаемая фигура должна быть дамкой");
                Assert.IsTrue(
                    board.IsCheckerCanMoveAt(checker, move.To),
                    "Ход дамки должен быть валидным"
                );
            }
        }

        [Test]
        public async Task EdgeCase_ConcurrentStopAndStart_DoesNotCrash()
        {
            // Arrange
            var checkers = new List<CheckerData>()
    {
        new CheckerData(0, 0, CheckerType.KING, OpponentType.Player, BOARD_HEIGHT, BOARD_WIDTH),
        new CheckerData(2, 0, CheckerType.KING, OpponentType.Player, BOARD_HEIGHT, BOARD_WIDTH),
        new CheckerData(4, 0, CheckerType.KING, OpponentType.Player, BOARD_HEIGHT, BOARD_WIDTH),
        new CheckerData(6, 0, CheckerType.KING, OpponentType.Player, BOARD_HEIGHT, BOARD_WIDTH),
        new CheckerData(1, 7, CheckerType.KING, OpponentType.AI, BOARD_HEIGHT, BOARD_WIDTH),
        new CheckerData(3, 7, CheckerType.KING, OpponentType.AI, BOARD_HEIGHT, BOARD_WIDTH),
        new CheckerData(5, 7, CheckerType.KING, OpponentType.AI, BOARD_HEIGHT, BOARD_WIDTH),
        new CheckerData(7, 7, CheckerType.KING, OpponentType.AI, BOARD_HEIGHT, BOARD_WIDTH)
    };
            var board = new BoardPosition(checkers, BOARD_WIDTH, BOARD_HEIGHT);
            var minimax = new MinimaxCore(BOARD_HEIGHT, BOARD_WIDTH, 12, false);

            // Act
            for (int i = 0; i < 3; i++)
            {
                // Запускаем вычисления
                var task = minimax.GetBestMove(board, OpponentType.AI);
                await Task.Delay(10);

                // Останавливаем
                await minimax.StopCalculating();

                // Дожидаемся завершения с защитой от исключений
                try
                {
                    var result = await task;
                    Assert.IsNull(result.move, "Должен вернуть null при остановке.");
                }
                catch (OperationCanceledException)
                {
                    // OK - операция отменена
                }

                // Перезапускаем
                await minimax.RestartCalculating();

                // Даем время на перезапуск
                await Task.Delay(5);
            }

            // Assert
            Assert.Pass("Многократный запуск/остановка не вызвал ошибок");
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
            Assert.IsNull(result.move, "На пустой доске не должно быть ходов");
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
            Assert.IsNotNull(result.move, "Одна шашка в центре должна иметь ход");
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
            Assert.IsNull(result.move, "AI не должен иметь ходов, если нет шашек AI");
            Assert.AreEqual(0f, result.points, 0.01f, "Оценка должна быть 0 (только игрок)");
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
            Assert.IsTrue(result.move.HasValue, "Должен быть ход, если есть шашки AI");
            var move = result.move.Value;
            Assert.IsTrue(
                board.IsCheckerCanMoveAt(board.Data[move.From.x, move.From.y], move.To),
                "Ход должен быть валидным"
            );
            Assert.AreEqual(1f, result.points, 0.01f, "Оценка должна быть 1 (только AI)");
        }

        [Test]
        public async Task EdgeCase_DeepSearchWithStop_StopsGracefully()
        {
            // Arrange
            var checkers = new List<CheckerData>();
            for (int i = 0; i < BOARD_HEIGHT; i++)
            {
                for (int j = 0; j < BOARD_WIDTH; j++)
                {
                    if ((i + j) % 2 == 1 && (i < 3 || i > 4))
                    {
                        var opponent = i < BOARD_HEIGHT / 2 ? OpponentType.Player : OpponentType.AI;
                        checkers.Add(new CheckerData(i, j, CheckerType.USUAL, opponent, BOARD_HEIGHT, BOARD_WIDTH));
                    }
                }
            }
            var board = new BoardPosition(checkers, BOARD_WIDTH, BOARD_HEIGHT);
            var minimax = new MinimaxCore(BOARD_HEIGHT, BOARD_WIDTH, 8, false);

            // Act - запускаем глубокий поиск и сразу останавливаем
            var task = minimax.GetBestMove(board, OpponentType.AI);
            await Task.Delay(100);
            await minimax.StopCalculating();

            // Ждем завершения задачи
            try
            {
                await task;
            }
            catch (AggregateException ae)
            {
                ae.Handle(ex =>
                {
                    if (ex is OperationCanceledException)
                        return true; // Ожидаемое исключение
                    Assert.Fail($"Неожиданное исключение: {ex.Message}");
                    return false;
                });
            }

            // Assert
            Assert.Pass("Остановка глубокого поиска выполнена успешно");
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

        [Test]
        public async Task EdgeCase_GetBestMove_WithDepthZero_ReturnsValidMove()
        {
            // Arrange
            var checkers = new List<CheckerData>
            {
                new CheckerData(3, 3, CheckerType.USUAL, OpponentType.AI, BOARD_HEIGHT, BOARD_WIDTH),
                new CheckerData(5, 5, CheckerType.USUAL, OpponentType.Player, BOARD_HEIGHT, BOARD_WIDTH)
            };
            var board = new BoardPosition(checkers, BOARD_WIDTH, BOARD_HEIGHT);
            var minimax = new MinimaxCore(BOARD_HEIGHT, BOARD_WIDTH, 0, false);

            // Act
            var result = await minimax.GetBestMove(board, OpponentType.AI);

            // Assert
            Assert.IsTrue(result.move.HasValue, "При глубине 0 должен быть возвращён ход без рекурсии");
            var move = result.move.Value;
            Assert.IsTrue(
                board.IsCheckerCanMoveAt(board.Data[move.From.x, move.From.y], move.To),
                "Ход должен быть валидным"
            );
        }
    }
}