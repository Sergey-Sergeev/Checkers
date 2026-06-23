using Assets.scripts.Core;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tests.EditMode.Core
{
    public class MinimaxCoreTests
    {
        private const int BOARD_WIDTH = 8;
        private const int BOARD_HEIGHT = 8;
        private const int SEARCH_DEPTH = 3;
        private MinimaxCore _minimax;

        private List<CheckerData> _emptyCheckersList;
        private List<CheckerData> _playerCheckersList;
        private List<CheckerData> _aiCheckersList;
        private List<CheckerData> _mixedCheckersList;

        [SetUp]
        public void Setup()
        {
            _minimax = new MinimaxCore(BOARD_HEIGHT, BOARD_WIDTH, SEARCH_DEPTH, false);
            _emptyCheckersList = new List<CheckerData>();
            SetupTestLists();
        }

        [TearDown]
        public async Task Teardown()
        {
            try
            {
                await _minimax.StopCalculating();
            }
            catch (Exception ex)
            {
                TestContext.WriteLine($"Ошибка при остановке minimax: {ex.Message}");
            }
        }

        private void SetupTestLists()
        {
            _playerCheckersList = new List<CheckerData>
            {
                new CheckerData(1, 1, CheckerType.USUAL, OpponentType.Player, BOARD_HEIGHT, BOARD_WIDTH),
                new CheckerData(2, 2, CheckerType.USUAL, OpponentType.Player, BOARD_HEIGHT, BOARD_WIDTH)
            };

            _aiCheckersList = new List<CheckerData>
            {
                new CheckerData(1, 1, CheckerType.USUAL, OpponentType.AI, BOARD_HEIGHT, BOARD_WIDTH),
                new CheckerData(2, 2, CheckerType.USUAL, OpponentType.AI, BOARD_HEIGHT, BOARD_WIDTH)
            };

            _mixedCheckersList = new List<CheckerData>
            {
                new CheckerData(1, 1, CheckerType.USUAL, OpponentType.Player, BOARD_HEIGHT, BOARD_WIDTH),
                new CheckerData(2, 1, CheckerType.USUAL, OpponentType.AI, BOARD_HEIGHT, BOARD_WIDTH)
            };
        }

        [Test]
        public void GetPositionAssessment_OnlyPlayerCheckers_ReturnsZero()
        {
            // Arrange
            var board = new BoardPosition(_playerCheckersList, BOARD_WIDTH, BOARD_HEIGHT);

            // Act
            float result = _minimax.GetPositionAssessment(board);

            // Assert
            Assert.AreEqual(0f, result, 0.01f, "Только шашки игрока — AI проиграл, оценка 0");
        }

        [Test]
        public void GetPositionAssessment_OnlyAICheckers_ReturnsOne()
        {
            // Arrange
            var board = new BoardPosition(_aiCheckersList, BOARD_WIDTH, BOARD_HEIGHT);

            // Act
            float result = _minimax.GetPositionAssessment(board);

            // Assert
            Assert.AreEqual(1f, result, 0.01f, "Только шашки AI — AI выиграл, оценка 1");
        }

        [Test]
        public void GetPositionAssessment_BothCheckers_ReturnsBetweenZeroAndOne()
        {
            // Arrange
            var board = new BoardPosition(_mixedCheckersList, BOARD_WIDTH, BOARD_HEIGHT);

            // Act
            float result = _minimax.GetPositionAssessment(board);

            // Assert
            Assert.Greater(result, 0f, "Должна быть больше 0 (есть AI шашки)");
            Assert.Less(result, 1f, "Должна быть меньше 1 (есть Player шашки)");
        }

        [Test]
        public void GetPositionAssessment_GiveawayMode_InvertsAssessment()
        {
            // Arrange
            var board = new BoardPosition(_mixedCheckersList, BOARD_WIDTH, BOARD_HEIGHT);
            var giveawayMinimax = new MinimaxCore(BOARD_HEIGHT, BOARD_WIDTH, SEARCH_DEPTH, true);

            // Act
            float normalAssessment = _minimax.GetPositionAssessment(board);
            float giveawayAssessment = giveawayMinimax.GetPositionAssessment(board);

            // Assert
            Assert.AreNotEqual(normalAssessment, giveawayAssessment, "В поддавках оценка должна отличаться");
        }

        [Test]
        public void GetPointsForChecker_UsualChecker_ReturnsPositivePoints()
        {
            // Arrange 
            var checker = new CheckerData(2, 2, CheckerType.USUAL, OpponentType.Player, BOARD_HEIGHT, BOARD_WIDTH);
            var checkers = new List<CheckerData> { checker };
            var board = new BoardPosition(checkers, BOARD_WIDTH, BOARD_HEIGHT);

            // Act
            float points = _minimax.GetPointsForChecker(board, checker);

            // Assert
            Assert.Greater(points, 0f, "Обычная шашка должна давать положительные очки");
        }

        [Test]
        public void GetPointsForChecker_KingChecker_ReturnsMoreThanUsual()
        {
            // Arrange
            var usualChecker = new CheckerData(2, 2, CheckerType.USUAL, OpponentType.Player, BOARD_HEIGHT, BOARD_WIDTH);
            var kingChecker = new CheckerData(2, 2, CheckerType.KING, OpponentType.Player, BOARD_HEIGHT, BOARD_WIDTH);

            var boardUsual = new BoardPosition(new List<CheckerData> { usualChecker }, BOARD_WIDTH, BOARD_HEIGHT);
            var boardKing = new BoardPosition(new List<CheckerData> { kingChecker }, BOARD_WIDTH, BOARD_HEIGHT);

            // Act
            float usualPoints = _minimax.GetPointsForChecker(boardUsual, usualChecker);
            float kingPoints = _minimax.GetPointsForChecker(boardKing, kingChecker);

            // Assert
            Assert.Greater(kingPoints, usualPoints, "Дамка должна давать больше очков, чем обычная шашка");
        }

        [Test]
        public void GetPointsForChecker_CenterPosition_GetsBonus()
        {
            // Arrange
            var centerChecker = new CheckerData(4, 4, CheckerType.USUAL, OpponentType.Player, BOARD_HEIGHT, BOARD_WIDTH);
            var edgeChecker = new CheckerData(0, 0, CheckerType.USUAL, OpponentType.Player, BOARD_HEIGHT, BOARD_WIDTH);

            var boardCenter = new BoardPosition(new List<CheckerData> { centerChecker }, BOARD_WIDTH, BOARD_HEIGHT);
            var boardEdge = new BoardPosition(new List<CheckerData> { edgeChecker }, BOARD_WIDTH, BOARD_HEIGHT);

            // Act
            float centerPoints = _minimax.GetPointsForChecker(boardCenter, centerChecker);
            float edgePoints = _minimax.GetPointsForChecker(boardEdge, edgeChecker);

            // Assert
            Assert.Greater(centerPoints, edgePoints, "Шашка в центре должна получать бонус по сравнению с шашкой на краю");
        }

        [Test]
        public void GetPointsForChecker_AICheckerNearPromotion_GetsBonus()
        {
            // Arrange - 
            var nearPromotion = new CheckerData(1, 1, CheckerType.USUAL, OpponentType.AI, BOARD_HEIGHT, BOARD_WIDTH);
            var farFromPromotion = new CheckerData(6, 6, CheckerType.USUAL, OpponentType.AI, BOARD_HEIGHT, BOARD_WIDTH);

            var boardNear = new BoardPosition(new List<CheckerData> { nearPromotion }, BOARD_WIDTH, BOARD_HEIGHT);
            var boardFar = new BoardPosition(new List<CheckerData> { farFromPromotion }, BOARD_WIDTH, BOARD_HEIGHT);

            // Act
            float nearPoints = _minimax.GetPointsForChecker(boardNear, nearPromotion);
            float farPoints = _minimax.GetPointsForChecker(boardFar, farFromPromotion);

            // Assert
            Assert.Greater(nearPoints, farPoints, "AI шашка ближе к превращению должна получать бонус");
        }

        [Test]
        public void GetPointsForChecker_NullChecker_ReturnsZero()
        {
            // Arrange
            var board = new BoardPosition(_emptyCheckersList, BOARD_WIDTH, BOARD_HEIGHT);

            // Act
            float points = _minimax.GetPointsForChecker(board, null);

            // Assert
            Assert.AreEqual(0f, points, 0.01f, "Null checker должен давать 0 очков");
        }

        [Test]
        public async Task GetBestMove_AIHasBeatingMove_ChoosesBeat()
        {
            // Arrange
            var checkers = new List<CheckerData>
            {
                new CheckerData(2, 2, CheckerType.USUAL, OpponentType.AI, BOARD_HEIGHT, BOARD_WIDTH),
                new CheckerData(3, 3, CheckerType.USUAL, OpponentType.Player, BOARD_HEIGHT, BOARD_WIDTH)  // Под боем
            };
            var board = new BoardPosition(checkers, BOARD_WIDTH, BOARD_HEIGHT);

            // Act
            var result = await _minimax.GetBestMove(board, OpponentType.AI);

            // Assert
            Assert.IsTrue(result.move.HasValue, "AI должен найти ход");
            Assert.IsTrue(result.move.Value.IsBeatOpponentChecker, "AI должен выбрать взятие, если оно возможно");
        }

        [Test]
        public async Task GetBestMove_WhenNoMoves_ReturnsNullMove()
        {
            // Arrange
            var checkers = new List<CheckerData>
            {
                new CheckerData(0, 0, CheckerType.KING, OpponentType.AI, BOARD_HEIGHT, BOARD_WIDTH),
                new CheckerData(1, 1, CheckerType.USUAL, OpponentType.Player, BOARD_HEIGHT, BOARD_WIDTH),
                new CheckerData(2, 2, CheckerType.USUAL, OpponentType.Player, BOARD_HEIGHT, BOARD_WIDTH)
            };
            var board = new BoardPosition(checkers, BOARD_WIDTH, BOARD_HEIGHT);

            // Act
            var result = await _minimax.GetBestMove(board, OpponentType.AI);

            // Assert
            Assert.IsFalse(result.move.HasValue, "ходов нет");
        }

        [Test]
        public async Task GetBestMove_PlayerTurn_ReturnsValidMove()
        {
            // Arrange - шашка игрока может ходить
            var checkers = new List<CheckerData>
            {
                new CheckerData(2, 2, CheckerType.USUAL, OpponentType.Player, BOARD_HEIGHT, BOARD_WIDTH)
            };
            var board = new BoardPosition(checkers, BOARD_WIDTH, BOARD_HEIGHT);

            // Act
            var result = await _minimax.GetBestMove(board, OpponentType.Player);

            // Assert
            Assert.IsTrue(result.move.HasValue, "Для игрока должен быть найден ход");
        }

        [Test]
        public async Task GetBestMove_WithKing_MustBeatWhenCan()
        {
            // Arrange - дамка может ходить назад
            var checkers = new List<CheckerData>
            {
                new CheckerData(3, 3, CheckerType.KING, OpponentType.AI, BOARD_HEIGHT, BOARD_WIDTH),
                new CheckerData(5, 5, CheckerType.USUAL, OpponentType.Player, BOARD_HEIGHT, BOARD_WIDTH)
            };
            var board = new BoardPosition(checkers, BOARD_WIDTH, BOARD_HEIGHT);

            // Act
            var result = await _minimax.GetBestMove(board, OpponentType.AI);

            // Assert
            Assert.IsTrue(result.move.HasValue, "Дамка должна иметь ходы");
            Assert.IsTrue(result.move.Value.IsBeatOpponentChecker, "Дамка должна бить");
        }

        [Test]
        public async Task GetBestMove_PromotionMove_BecomesKing()
        {
            // Arrange
            var checkers = new List<CheckerData>
            {
                new CheckerData(1, 1, CheckerType.USUAL, OpponentType.AI, BOARD_HEIGHT, BOARD_WIDTH)
            };
            var board = new BoardPosition(checkers, BOARD_WIDTH, BOARD_HEIGHT);

            // Act
            var result = await _minimax.GetBestMove(board, OpponentType.AI);

            // Assert
            Assert.IsTrue(result.move.HasValue, "Должен быть ход");
            var move = result.move.Value;

            // Применяем ход и проверяем превращение
            board.MakeMove(move.From, move.To, out _, out _, out bool transformed);
            Assert.IsTrue(transformed || board.Data[move.To.x, move.To.y].Type == CheckerType.KING,
                "Если ход ведёт на последний ряд, шашка должна стать дамкой");
        }

        [Test]
        public async Task AlphaBetaPruning_DeepAndShallow_ReturnValidMoves()
        {
            // Arrange
            var checkers = new List<CheckerData>
            {
                new CheckerData(7, 7, CheckerType.USUAL, OpponentType.AI, BOARD_HEIGHT, BOARD_WIDTH),
                new CheckerData(5, 5, CheckerType.USUAL, OpponentType.AI, BOARD_HEIGHT, BOARD_WIDTH),
                new CheckerData(0, 0, CheckerType.USUAL, OpponentType.Player, BOARD_HEIGHT, BOARD_WIDTH),
                new CheckerData(2, 2, CheckerType.USUAL, OpponentType.Player, BOARD_HEIGHT, BOARD_WIDTH)
            };
            var board = new BoardPosition(checkers, BOARD_WIDTH, BOARD_HEIGHT);

            var minimaxShallow = new MinimaxCore(BOARD_HEIGHT, BOARD_WIDTH, 1, false);
            var minimaxDeep = new MinimaxCore(BOARD_HEIGHT, BOARD_WIDTH, 10, false);

            // Act
            var resultShallow = await minimaxShallow.GetBestMove(board, OpponentType.AI);
            var resultDeep = await minimaxDeep.GetBestMove(board, OpponentType.AI);

            // Assert
            Assert.IsTrue(resultShallow.move.HasValue, "Мелкий поиск должен вернуть ход");
            Assert.IsTrue(resultDeep.move.HasValue, "Глубокий поиск должен вернуть ход");
        }

        [Test]
        public async Task StopCalculating_ActuallyStopsTask()
        {
            // Arrange
            var checkers = new List<CheckerData>();
            for (int i = 0; i < BOARD_HEIGHT; i++)
            {
                for (int j = 0; j < BOARD_WIDTH; j++)
                {
                    if ((i + j) % 2 == 1 && (i < 3 && i > 4))
                    {
                        var opponent = i < BOARD_HEIGHT / 2 ? OpponentType.Player : OpponentType.AI;
                        checkers.Add(new CheckerData(i, j, CheckerType.USUAL, opponent, BOARD_HEIGHT, BOARD_WIDTH));
                    }
                }
            }
            var board = new BoardPosition(checkers, BOARD_WIDTH, BOARD_HEIGHT);
            var deepMinimax = new MinimaxCore(BOARD_HEIGHT, BOARD_WIDTH, 10, false);

            // Act
            var task = deepMinimax.GetBestMove(board, OpponentType.AI);
            await Task.Delay(100);
            await deepMinimax.StopCalculating();
            await task;

            // Assert
            Assert.IsTrue(task.IsCompleted && task.Result.move == null, "Задача должна быть отменена после StopCalculating");
        }

        [Test]
        public async Task RestartCalculating_AllowsNewCalculations()
        {
            // Arrange
            var checkers = new List<CheckerData>
            {
                new CheckerData(3, 3, CheckerType.USUAL, OpponentType.AI, BOARD_HEIGHT, BOARD_WIDTH)
            };
            var board = new BoardPosition(checkers, BOARD_WIDTH, BOARD_HEIGHT);

            // Act - останавливаем и перезапускаем
            await _minimax.StopCalculating();
            await _minimax.RestartCalculating();

            var result = await _minimax.GetBestMove(board, OpponentType.AI);

            // Assert
            Assert.IsTrue(result.move.HasValue, "После RestartCalculating должны быть доступны новые вычисления");
        }

        [Test]
        [Timeout(10000)]
        public async Task GetBestMove_CompletesWithinTimeLimit()
        {
            // Arrange
            var checkers = new List<CheckerData>();
            for (int i = 0; i < BOARD_HEIGHT; i++)
            {
                for (int j = 0; j < BOARD_WIDTH; j++)
                {
                    if ((i + j) % 2 == 1 && (i < 3 || i > 4))
                    {
                        var opponent = i < 2 ? OpponentType.Player : OpponentType.AI;
                        checkers.Add(new CheckerData(i, j, CheckerType.USUAL, opponent, BOARD_HEIGHT, BOARD_WIDTH));
                    }
                }
            }
            var board = new BoardPosition(checkers, BOARD_WIDTH, BOARD_HEIGHT);
            var minimax = new MinimaxCore(BOARD_HEIGHT, BOARD_WIDTH, 8, false);

            // Act
            var result = await minimax.GetBestMove(board, OpponentType.AI);

            // Assert
            Assert.IsTrue(result.move.HasValue, "Поиск должен завершиться за отведённое время и вернуть ход");
        }

        [Test]
        public async Task GetBestMove_SimplePosition_ChoosesBestMove()
        {
            // Arrange
            var checkers = new List<CheckerData>
            {
                new CheckerData(7, 7, CheckerType.USUAL, OpponentType.AI, BOARD_HEIGHT, BOARD_WIDTH),
                new CheckerData(3, 3, CheckerType.USUAL, OpponentType.Player, BOARD_HEIGHT, BOARD_WIDTH),
                new CheckerData(6, 6, CheckerType.USUAL, OpponentType.AI, BOARD_HEIGHT, BOARD_WIDTH)
            };
            var board = new BoardPosition(checkers, BOARD_WIDTH, BOARD_HEIGHT);

            // Act
            var result = await _minimax.GetBestMove(board, OpponentType.AI);

            // Assert
            Assert.IsTrue(result.move.HasValue, "Должен быть найден ход");
            Assert.IsTrue(result.move.Value.To == new UnityEngine.Vector2Int(5, 5), "Должен быть найден ход");
        }

        [Test]
        public async Task GetBestMove_WithMultipleBeats_ChoosesBestOne()
        {
            // Arrange
            var checkers = new List<CheckerData>
            {
                new CheckerData(2, 2, CheckerType.USUAL, OpponentType.AI, BOARD_HEIGHT, BOARD_WIDTH),
                new CheckerData(3, 3, CheckerType.USUAL, OpponentType.Player, BOARD_HEIGHT, BOARD_WIDTH),
                new CheckerData(3, 5, CheckerType.USUAL, OpponentType.Player, BOARD_HEIGHT, BOARD_WIDTH),
                new CheckerData(5, 3, CheckerType.USUAL, OpponentType.Player, BOARD_HEIGHT, BOARD_WIDTH),
                new CheckerData(1, 5, CheckerType.USUAL, OpponentType.Player, BOARD_HEIGHT, BOARD_WIDTH)
            };
            var board = new BoardPosition(checkers, BOARD_WIDTH, BOARD_HEIGHT);

            // Act
            var result = await _minimax.GetBestMove(board, OpponentType.AI);

            // Assert
            Assert.IsTrue(result.move.HasValue, "Должен быть выбран ход");
            Assert.IsTrue(result.move.Value.IsBeatOpponentChecker, "AI должен бить при возможности");

            // Проверяем, что выбранный ход валидный
            var move = result.move.Value;
            Assert.IsTrue(board.IsCheckerCanMoveAt(board.Data[move.From.x, move.From.y], move.To), "Выбранный ход должен быть валидным");

            board.MakeMove(move.From, move.To, out _, out _, out _);


            // делаем второй ход
            result = await _minimax.GetBestMove(board, OpponentType.AI);

            // Assert
            Assert.IsTrue(result.move.HasValue, "Должен быть выбран ход");
            Assert.IsTrue(result.move.Value.IsBeatOpponentChecker, "AI должен бить при возможности");

            // Проверяем, что выбранный ход валидный
            move = result.move.Value;
            Assert.IsTrue(board.IsCheckerCanMoveAt(board.Data[move.From.x, move.From.y], move.To), "Выбранный ход должен быть валидным");
            Assert.IsTrue(move.To == new UnityEngine.Vector2Int(2, 6), "Выбранный ход должен быть лучшим");
        }

        [Test]
        public async Task GetBestMove_ForKing_WithMultipleBeats_ChoosesSafeBeatOverRiskyMultipleBeat()
        {
            // Arrange
            // Ситуация: у AI есть дамка на (4, 4), которая может бить в разных направлениях
            var checkers = new List<CheckerData>
            {
                // Дамка AI
                new CheckerData(4, 4, CheckerType.KING, OpponentType.AI, BOARD_HEIGHT, BOARD_WIDTH),
        
                // Шашки противника для боя
                new CheckerData(6, 6, CheckerType.USUAL, OpponentType.Player, BOARD_HEIGHT, BOARD_WIDTH),
                new CheckerData(3, 5, CheckerType.USUAL, OpponentType.Player, BOARD_HEIGHT, BOARD_WIDTH),
                new CheckerData(5, 3, CheckerType.USUAL, OpponentType.Player, BOARD_HEIGHT, BOARD_WIDTH),
                new CheckerData(3, 3, CheckerType.USUAL, OpponentType.Player, BOARD_HEIGHT, BOARD_WIDTH),
        
                // Шашки противника, которые могут сбить дамку AI после первого боя
                new CheckerData(1, 7, CheckerType.USUAL, OpponentType.Player, BOARD_HEIGHT, BOARD_WIDTH),
                new CheckerData(7, 1, CheckerType.USUAL, OpponentType.Player, BOARD_HEIGHT, BOARD_WIDTH),                
        
                // Вариант с одной шашкой, но безопасный (дамку не собьют)
                new CheckerData(7, 3, CheckerType.USUAL, OpponentType.Player, BOARD_HEIGHT, BOARD_WIDTH),
                new CheckerData(5, 1, CheckerType.USUAL, OpponentType.Player, BOARD_HEIGHT, BOARD_WIDTH),
                new CheckerData(3, 1, CheckerType.USUAL, OpponentType.Player, BOARD_HEIGHT, BOARD_WIDTH),
                new CheckerData(1, 1, CheckerType.USUAL, OpponentType.Player, BOARD_HEIGHT, BOARD_WIDTH)
            };
            var board = new BoardPosition(checkers, BOARD_WIDTH, BOARD_HEIGHT);


            CheckerMove move = new CheckerMove();
            // Act 
            for (int i = 0; i < 6; i++)
            {                
                var result = await _minimax.GetBestMove(board, OpponentType.AI);

                Assert.IsTrue(result.move.HasValue, "Должен быть выбран ход");
                Assert.IsTrue(result.move.Value.IsBeatOpponentChecker, "AI должен бить при возможности");
                // Проверяем, что выбранный ход валидный
                move = result.move.Value;
                Assert.IsTrue(board.IsCheckerCanMoveAt(board.Data[move.From.x, move.From.y], move.To), "Выбранный ход должен быть валидным");
            }

            // Assert
            Assert.IsNotNull(move, "Ход должен быть");
            Assert.IsTrue(new UnityEngine.Vector2Int(0, 0) == move.To ||
                new UnityEngine.Vector2Int(7, 7) == move.To, "Дамка должна выбрать безопасный бой шашкой");

        }
    }
}