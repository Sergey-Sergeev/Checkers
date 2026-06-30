using Assets.scripts.Core;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Tests.EditMode.Core
{
    public class MinimaxCoreGiveawayTests
    {
        private const int BOARD_WIDTH = 8;
        private const int BOARD_HEIGHT = 8;
        private const int SEARCH_DEPTH = 3;
        private MinimaxCore _minimaxGiveaway;
        private MinimaxCore _minimaxNormal;

        [SetUp]
        public void Setup()
        {
            _minimaxNormal = new MinimaxCore(BOARD_HEIGHT, BOARD_WIDTH, SEARCH_DEPTH, false);
            _minimaxGiveaway = new MinimaxCore(BOARD_HEIGHT, BOARD_WIDTH, SEARCH_DEPTH, true);
        }

        [TearDown]
        public void Teardown()
        {
            try
            {
                _minimaxNormal.DisableCalculation();
            }
            catch (Exception ex)
            {
                TestContext.WriteLine($"Ошибка при остановке normal minimax: {ex.Message}");
            }

            try
            {
                _minimaxGiveaway.DisableCalculation();
            }
            catch (Exception ex)
            {
                TestContext.WriteLine($"Ошибка при остановке giveaway minimax: {ex.Message}");
            }
        }

        [Test]
        public void GetPositionAssessment_GiveawayMode_IsDifferentFromNormal()
        {
            // Arrange - шашки на чёрных клетках
            var checkers = new List<CheckerData>
            {
                new CheckerData(0, 0, CheckerType.Usual, OpponentType.Player, BOARD_HEIGHT, BOARD_WIDTH),
                new CheckerData(3, 3, CheckerType.Usual, OpponentType.AI, BOARD_HEIGHT, BOARD_WIDTH)
            };
            var board = new BoardPosition(checkers, BOARD_WIDTH, BOARD_HEIGHT);

            // Act
            float normalAssessment = _minimaxNormal.GetPositionAssessment(board);
            float giveawayAssessment = _minimaxGiveaway.GetPositionAssessment(board);

            // Assert
            Assert.AreNotEqual(normalAssessment, giveawayAssessment, "В поддавках оценка должна отличаться от обычного режима");
        }

        [Test]
        public void GetPositionAssessment_GiveawayMode_EmptyBoard_ReturnsSame()
        {
            // Arrange - пустая доска
            var checkers = new List<CheckerData>();
            var board = new BoardPosition(checkers, BOARD_WIDTH, BOARD_HEIGHT);

            // Act
            float normalAssessment = _minimaxNormal.GetPositionAssessment(board);
            float giveawayAssessment = _minimaxGiveaway.GetPositionAssessment(board);

            // Assert - на пустой доске оценки могут совпадать
            Assert.AreEqual(normalAssessment, giveawayAssessment, 0.01f,
                "На пустой доске оценки в обоих режимах должны совпадать");
        }

        [Test]
        public void GetPositionAssessment_GiveawayMode_OnlyAICheckers_ReturnsLowScore()
        {
            // Arrange - только шашки AI (AI "выигрывает" в обычном режиме)
            var checkers = new List<CheckerData>
            {
                new CheckerData(2, 2, CheckerType.Usual, OpponentType.AI, BOARD_HEIGHT, BOARD_WIDTH),
                new CheckerData(4, 4, CheckerType.Usual, OpponentType.AI, BOARD_HEIGHT, BOARD_WIDTH)
            };
            var board = new BoardPosition(checkers, BOARD_WIDTH, BOARD_HEIGHT);

            // Act
            float normalAssessment = _minimaxNormal.GetPositionAssessment(board);
            float giveawayAssessment = _minimaxGiveaway.GetPositionAssessment(board);

            // Assert
            Assert.Greater(normalAssessment, 0.5f, "В обычном режиме: только AI — оценка должна быть высокой (AI выигрывает)");
            Assert.Less(giveawayAssessment, 0.5f, "В поддавках: только AI — оценка должна быть низкой (AI проигрывает)");
        }

        [Test]
        public void GetPositionAssessment_GiveawayMode_OnlyPlayerCheckers_ReturnsHighScore()
        {
            // Arrange - только шашки игрока (AI "проигрывает" в обычном режиме)
            var checkers = new List<CheckerData>
            {
                new CheckerData(2, 2, CheckerType.Usual, OpponentType.Player, BOARD_HEIGHT, BOARD_WIDTH),
                new CheckerData(4, 4, CheckerType.Usual, OpponentType.Player, BOARD_HEIGHT, BOARD_WIDTH)
            };
            var board = new BoardPosition(checkers, BOARD_WIDTH, BOARD_HEIGHT);

            // Act
            float normalAssessment = _minimaxNormal.GetPositionAssessment(board);
            float giveawayAssessment = _minimaxGiveaway.GetPositionAssessment(board);

            // Assert
            Assert.Less(normalAssessment, 0.5f, "В обычном режиме: только игрок — оценка должна быть низкой (AI проигрывает)");
            Assert.Greater(giveawayAssessment, 0.5f, "В поддавках: только игрок — оценка должна быть высокой (AI выигрывает)");
        }

        [Test]
        public async Task GetBestMove_GiveawayMode_ChoosesMoveThatLeadsToWorsePosition()
        {
            // Arrange
            var checkers = new List<CheckerData>
            {
                new CheckerData(6, 6, CheckerType.Usual, OpponentType.AI, BOARD_HEIGHT, BOARD_WIDTH),
                new CheckerData(3, 5, CheckerType.Usual, OpponentType.AI, BOARD_HEIGHT, BOARD_WIDTH),
                new CheckerData(3, 3, CheckerType.Usual, OpponentType.Player, BOARD_HEIGHT, BOARD_WIDTH)
            };
            var board = new BoardPosition(checkers, BOARD_WIDTH, BOARD_HEIGHT);

            // Act
            var normalResult = await _minimaxNormal.GetBestMove(board, OpponentType.AI);
            var giveawayResult = await _minimaxGiveaway.GetBestMove(board, OpponentType.AI);

            // Assert
            Assert.IsTrue(normalResult.HasValue, "Обычный режим должен найти ход");
            Assert.IsTrue(giveawayResult.HasValue, "Режим поддавков должен найти ход");

            var normalMove = normalResult.Value;
            var giveawayMove = giveawayResult.Value;

            Assert.IsTrue(board.IsCheckerCanMoveAt(board.Data[normalMove.From.x, normalMove.From.y], normalMove.To),
                "Ход в обычном режиме должен быть валидным");
            Assert.IsTrue(board.IsCheckerCanMoveAt(board.Data[giveawayMove.From.x, giveawayMove.From.y], giveawayMove.To),
                "Ход в режиме поддавков должен быть валидным");

            // Оцениваем позиции после ходов в обычном режиме
            var boardAfterNormal = board.Clone();
            boardAfterNormal.MakeMove(normalMove.From, normalMove.To, out _, out _);
            float normalScoreAfter = _minimaxNormal.GetPositionAssessment(boardAfterNormal);

            var boardAfterGiveaway = board.Clone();
            boardAfterGiveaway.MakeMove(giveawayMove.From, giveawayMove.To,  out _, out _);
            float giveawayScoreAfter = _minimaxNormal.GetPositionAssessment(boardAfterGiveaway);

            TestContext.WriteLine($"Normal move: {normalMove.From} -> {normalMove.To}, score: {normalScoreAfter:F3}");
            TestContext.WriteLine($"Giveaway move: {giveawayMove.From} -> {giveawayMove.To}, score: {giveawayScoreAfter:F3}");

            // В поддавках AI должен выбрать ход, который хуже с точки зрения обычного режима
            // (но это не гарантировано на 100% при одинаково плохих ходах)
            if (!normalMove.From.Equals(giveawayMove.From) || !normalMove.To.Equals(giveawayMove.To))
            {
                Assert.LessOrEqual(giveawayScoreAfter, normalScoreAfter,
                    "В поддавках AI должен выбрать ход с не лучшей оценкой в обычном режиме");
            }
            else
            {
                TestContext.WriteLine("Ходы совпали — это нормально для простых позиций");
            }
        }

        [Test]
        public async Task GetBestMove_GiveawayMode_MustFindBestMove()
        {
            // Arrange
            var checkers = new List<CheckerData>
            {
                new CheckerData(6, 6, CheckerType.Usual, OpponentType.AI, BOARD_HEIGHT, BOARD_WIDTH),
                new CheckerData(5, 5, CheckerType.Usual, OpponentType.Player, BOARD_HEIGHT, BOARD_WIDTH),
                new CheckerData(3, 3, CheckerType.Usual, OpponentType.AI, BOARD_HEIGHT, BOARD_WIDTH)
            };
            var board = new BoardPosition(checkers, BOARD_WIDTH, BOARD_HEIGHT);

            // Act
            var giveawayResult = await _minimaxGiveaway.GetBestMove(board, OpponentType.AI);

            // Assert
            Assert.IsTrue(giveawayResult.HasValue, "Режим поддавков должен найти ход");

            bool giveawayIsBeat = giveawayResult.Value.IsBeatOpponentChecker;

            Assert.IsTrue(giveawayResult.Value.To == new UnityEngine.Vector2Int(4, 4), "лучший ход это 4 4");
        }

        [Test]
        public async Task GetBestMove_GiveawayMode_NoBeatingAvailable_StillReturnsMove()
        {
            // Arrange - AI шашки без возможности бить (нет шашек игрока рядом)
            var checkers = new List<CheckerData>
            {
                new CheckerData(2, 2, CheckerType.Usual, OpponentType.AI, BOARD_HEIGHT, BOARD_WIDTH),
                new CheckerData(3, 3, CheckerType.Usual, OpponentType.AI, BOARD_HEIGHT, BOARD_WIDTH)
            };
            var board = new BoardPosition(checkers, BOARD_WIDTH, BOARD_HEIGHT);

            // Act
            var result = await _minimaxGiveaway.GetBestMove(board, OpponentType.AI);

            // Assert
            Assert.IsTrue(result.HasValue, "В поддавках должен быть найден ход, даже если нет взятий");
            Assert.IsFalse(result.Value.IsBeatOpponentChecker, "Ход не должен быть взятием (нет шашек для взятия)");
        }

        [Test]
        public void GetPositionAssessment_GiveawayMode_KingHasDifferentValue()
        {
            // Arrange - позиция с дамкой у игрока
            var checkersWithKing = new List<CheckerData>
            {
                new CheckerData(3, 3, CheckerType.Usual, OpponentType.AI, BOARD_HEIGHT, BOARD_WIDTH),
                new CheckerData(4, 4, CheckerType.King, OpponentType.Player, BOARD_HEIGHT, BOARD_WIDTH)
            };
            var boardWithKing = new BoardPosition(checkersWithKing, BOARD_WIDTH, BOARD_HEIGHT);

            // Позиция с обычной шашкой у игрока
            var checkersUsual = new List<CheckerData>
            {
                new CheckerData(3, 3, CheckerType.Usual, OpponentType.AI, BOARD_HEIGHT, BOARD_WIDTH),
                new CheckerData(4, 4, CheckerType.Usual, OpponentType.Player, BOARD_HEIGHT, BOARD_WIDTH)
            };
            var boardUsual = new BoardPosition(checkersUsual, BOARD_WIDTH, BOARD_HEIGHT);

            // Act
            float kingAssessment = _minimaxGiveaway.GetPositionAssessment(boardWithKing);
            float usualAssessment = _minimaxGiveaway.GetPositionAssessment(boardUsual);

            // Assert - дамка у игрока в поддавках должна делать позицию хуже для AI
            Assert.AreNotEqual(kingAssessment, usualAssessment, "Оценка с дамкой должна отличаться от оценки с обычной шашкой");
        }
    }
}