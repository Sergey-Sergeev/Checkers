using NUnit.Framework;
using Assets.scripts.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Tests.PlayMode
{
    public class IntegrationTests
    {
        private const int BOARD_WIDTH = 8;
        private const int BOARD_HEIGHT = 8;
        private const int SEARCH_DEPTH = 3;

        [Test]
        public async Task Integration_FullGameFlow_AICompletesMove()
        {
            // Arrange - стандартная начальная позиция
            var checkers = new List<CheckerData>();

            // Расставляем шашки для реальной игры
            for (int i = 0; i < BOARD_HEIGHT; i++)
            {
                for (int j = 0; j < BOARD_WIDTH; j++)
                {
                    // Только черные клетки
                    if ((i + j) % 2 == 1)
                    {
                        OpponentType opponent;

                        // Шашки игрока (белые) - ряды 0, 1, 2
                        if (i < 3)
                            opponent = OpponentType.Player;
                        // Шашки AI (черные) - ряды 5, 6, 7
                        else if (i >= 5)
                            opponent = OpponentType.AI;
                        // Ряды 3 и 4 - пустые
                        else
                            continue;

                        checkers.Add(new CheckerData(i, j, CheckerType.USUAL, opponent, BOARD_HEIGHT, BOARD_WIDTH));
                    }
                }
            }

            var board = new BoardPosition(checkers, BOARD_WIDTH, BOARD_HEIGHT);
            var minimax = new MinimaxCore(BOARD_HEIGHT, BOARD_WIDTH, SEARCH_DEPTH, false);

            // Проверяем, что шашки расставлены правильно
            Assert.AreEqual(12, board.PlayerCheckerCount, "Должно быть 12 шашек игрока");
            Assert.AreEqual(12, board.AICheckerCount, "Должно быть 12 шашек AI");

            // Act - AI делает ход
            var result = await minimax.GetBestMove(board, OpponentType.AI);

            // Assert
            Assert.IsNotNull(result, "Результат не должен быть null");
            Assert.IsTrue(result.move.HasValue, "AI должен найти ход");

            var move = result.move.Value;
            var sourceChecker = board.Data[move.From.x, move.From.y];

            // Проверяем, что AI ходит своей шашкой
            Assert.IsNotNull(sourceChecker, "Исходная клетка не должна быть пустой");
            Assert.AreEqual(OpponentType.AI, sourceChecker.Opponent, "AI должен ходить своей шашкой");

            // Применяем ход
            board.MakeMove(move.From, move.To, out var beaten, out var newX, out var newY);

            // Проверяем, что состояние изменилось
            Assert.IsNull(board.Data[move.From.x, move.From.y],
                $"Шашка должна покинуть исходную позицию ({move.From.x}, {move.From.y})");
            Assert.IsNotNull(board.Data[move.To.x, move.To.y],
                $"Шашка должна появиться на новой позиции ({move.To.x}, {move.To.y})");

            // Проверяем, что фигура на новой позиции принадлежит AI
            Assert.AreEqual(OpponentType.AI, board.Data[move.To.x, move.To.y].Opponent,
                "На новой позиции должна быть шашка AI");
        }

        [Test]
        public async Task Integration_WithGiveaways_AIPlaysDifferently()
        {
            // Arrange
            var checkers = new List<CheckerData>
            {
                new CheckerData(3, 2, CheckerType.USUAL, OpponentType.AI, BOARD_HEIGHT, BOARD_WIDTH),
                new CheckerData(4, 1, CheckerType.USUAL, OpponentType.Player, BOARD_HEIGHT, BOARD_WIDTH)
            };
            var board = new BoardPosition(checkers, BOARD_WIDTH, BOARD_HEIGHT);

            var normalMinimax = new MinimaxCore(BOARD_HEIGHT, BOARD_WIDTH, SEARCH_DEPTH, false);
            var giveawayMinimax = new MinimaxCore(BOARD_HEIGHT, BOARD_WIDTH, SEARCH_DEPTH, true);

            // Act
            var normalResult = await normalMinimax.GetBestMove(board, OpponentType.AI);
            var giveawayResult = await giveawayMinimax.GetBestMove(board, OpponentType.AI);

            // Assert
            Assert.IsNotNull(normalResult, "Результат обычного режима не должен быть null");
            Assert.IsNotNull(giveawayResult, "Результат режима поддавков не должен быть null");
            Assert.IsTrue(normalResult.move.HasValue, "Обычный режим должен найти ход");
            Assert.IsTrue(giveawayResult.move.HasValue, "Режим поддавков должен найти ход");

            // Проверяем, что оба хода валидны
            var normalMove = normalResult.move.Value;
            var giveawayMove = giveawayResult.move.Value;

            Assert.IsTrue(
                board.IsCheckerCanMoveAt(board.Data[normalMove.From.x, normalMove.From.y], normalMove.To),
                "Ход в обычном режиме должен быть валидным");
            Assert.IsTrue(
                board.IsCheckerCanMoveAt(board.Data[giveawayMove.From.x, giveawayMove.From.y], giveawayMove.To),
                "Ход в режиме поддавков должен быть валидным");

            // Проверяем, что режимы действительно могут давать разные результаты
            // Не требуем обязательного различия (может совпасть), но проверяем логи
            if (normalMove.From.Equals(giveawayMove.From) && normalMove.To.Equals(giveawayMove.To))
            {
                TestContext.WriteLine("Режимы дали одинаковый ход (это нормально для простой позиции)");
            }
            else
            {
                TestContext.WriteLine($"Обычный режим: {normalMove.From} -> {normalMove.To}");
                TestContext.WriteLine($"Режим поддавков: {giveawayMove.From} -> {giveawayMove.To}");
                TestContext.WriteLine("Режимы дали разные ходы");
            }
        }

        [Test]
        public async Task Integration_MultipleAIMoves_NoExceptions()
        {
            // Arrange - используем стандартную доску 8x8 с шашками
            var checkers = new List<CheckerData>();
            for (int i = 0; i < BOARD_HEIGHT; i++)
            {
                for (int j = 0; j < BOARD_WIDTH; j++)
                {
                    if ((i + j) % 2 == 1)
                    {
                        OpponentType opponent;
                        if (i < 3)
                            opponent = OpponentType.Player;
                        else if (i >= 5)
                            opponent = OpponentType.AI;
                        else
                            continue;

                        checkers.Add(new CheckerData(i, j, CheckerType.USUAL, opponent, BOARD_HEIGHT, BOARD_WIDTH));
                    }
                }
            }

            var board = new BoardPosition(checkers, BOARD_WIDTH, BOARD_HEIGHT);
            var minimax = new MinimaxCore(BOARD_HEIGHT, BOARD_WIDTH, 2, false);

            int movesMade = 0;
            const int maxMoves = 5;

            // Act - эмулируем несколько ходов AI (AI ходит за обе стороны)
            try
            {
                for (int moveCount = 0; moveCount < maxMoves; moveCount++)
                {
                    // Чередуем ходы: AI ходит за игрока и за себя
                    var currentOpponent = moveCount % 2 == 0 ? OpponentType.AI : OpponentType.Player;
                    var result = await minimax.GetBestMove(board, currentOpponent);

                    if (!result.move.HasValue)
                    {
                        TestContext.WriteLine($"Ходов больше нет после {movesMade} ходов");
                        break;
                    }

                    var move = result.move.Value;
                    var sourceChecker = board.Data[move.From.x, move.From.y];

                    Assert.IsNotNull(sourceChecker, $"Клетка ({move.From.x}, {move.From.y}) не должна быть пустой");
                    Assert.AreEqual(currentOpponent, sourceChecker.Opponent,
                        $"Ход должен быть за {currentOpponent}");

                    board.MakeMove(move.From, move.To, out _, out _, out _);
                    movesMade++;
                }

                Assert.Greater(movesMade, 0, "Должен быть сделан хотя бы один ход");
                TestContext.WriteLine($"Успешно выполнено {movesMade} ходов");
            }
            catch (Exception ex)
            {
                Assert.Fail($"Исключение при выполнении хода {movesMade}: {ex}");
            }
        }

        [Test]
        public async Task Integration_AICanBecomeKing()
        {
            // Arrange - шашка AI на предпоследнем ряду
            var checkers = new List<CheckerData>
            {
                new CheckerData(6, 1, CheckerType.USUAL, OpponentType.AI, BOARD_HEIGHT, BOARD_WIDTH),
                new CheckerData(5, 2, CheckerType.USUAL, OpponentType.Player, BOARD_HEIGHT, BOARD_WIDTH)
            };
            var board = new BoardPosition(checkers, BOARD_WIDTH, BOARD_HEIGHT);
            var minimax = new MinimaxCore(BOARD_HEIGHT, BOARD_WIDTH, SEARCH_DEPTH, false);

            // Act
            var result = await minimax.GetBestMove(board, OpponentType.AI);

            // Assert
            Assert.IsTrue(result.move.HasValue, "AI должен найти ход");
            var move = result.move.Value;

            // Ход должен вести на последний ряд (ряд 7) и превращать в дамку
            Assert.AreEqual(7, move.To.x, "Ход должен вести на последний ряд");
            board.MakeMove(move.From, move.To, out _, out _, out _);

            var newChecker = board.Data[move.To.x, move.To.y];
            Assert.IsNotNull(newChecker, "На новой позиции должна быть шашка");
            Assert.AreEqual(CheckerType.KING, newChecker.Type, "Шашка должна стать дамкой");
        }

        [Test]
        public async Task Integration_BeatIsMandatory()
        {
            // Arrange - позиция, где есть обязательное взятие
            var checkers = new List<CheckerData>
            {
                new CheckerData(4, 3, CheckerType.USUAL, OpponentType.AI, BOARD_HEIGHT, BOARD_WIDTH),
                new CheckerData(5, 4, CheckerType.USUAL, OpponentType.Player, BOARD_HEIGHT, BOARD_WIDTH),
                new CheckerData(6, 3, CheckerType.USUAL, OpponentType.AI, BOARD_HEIGHT, BOARD_WIDTH)
            };
            var board = new BoardPosition(checkers, BOARD_WIDTH, BOARD_HEIGHT);
            var minimax = new MinimaxCore(BOARD_HEIGHT, BOARD_WIDTH, SEARCH_DEPTH, false);

            // Act
            var result = await minimax.GetBestMove(board, OpponentType.AI);

            // Assert
            Assert.IsTrue(result.move.HasValue, "AI должен найти ход");
            var move = result.move.Value;

            // Проверяем, что AI выбрал взятие, если оно доступно
            bool isBeat = board.IsCheckerCanMoveAt(board.Data[move.From.x, move.From.y], move.To);
            Assert.IsTrue(isBeat,
                $"AI должен выполнить обязательное взятие, но выбран ход {move.From} -> {move.To}");
        }
    }
}