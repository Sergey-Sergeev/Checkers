using Assets.scripts.Core;
using Assets.scripts.GameLogic;
using Assets.scripts.Infrastructure;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Tests.PlayMode
{
    [TestFixture]
    public class CheckersAIIntegrationTest
    {
        private GameObject _aiGameObject;
        private CheckersAI _aiComponent;

        [UnitySetUp]
        public IEnumerator Setup()
        {
            // Настраиваем игру
            GameSettings.Instance.FirstMoveTurn = OpponentType.Player;
            GameSettings.Instance.BoardHeight = 8;
            GameSettings.Instance.BoardWidth = 8;
            GameSettings.Instance.AISearchDeep = 4;
            GameSettings.Instance.IsGiveaways = false;
            GameSettings.Instance.OpponentCountOfChechers = 12;
            GameSettings.Instance.IsCustomBoard = true;
            GameSettings.Instance.CustomBoardPosition = CreateCustomBoard();
            yield return null;
        }

        [UnityTest]
        public IEnumerator Test_AIPlaysAsPlayer_OnLoadedScene_Deep_8_vs_Deep_4()
        {
            // Загружаем сцену
            SceneManager.LoadScene(SceneNames.Game);
            yield return null;

            // Создаем объект AI и добавляем компонент
            GameSettings.Instance.AISearchDeep = 8; // big brain time on

            _aiGameObject = new GameObject("TestAIPlaysAsPlayer");
            _aiComponent = _aiGameObject.AddComponent<CheckersAI>();
            _aiComponent.AIOpponent = OpponentType.Player;

            yield return null;

            // Act - Ждем
            while (GameManager.Instance.CurrentEndOfGameState == EndOfGameType.None)
            {
                yield return new WaitForSeconds(0.3f);
            }

            // Assert
            Assert.IsTrue(GameManager.Instance.CurrentEndOfGameState == EndOfGameType.PlayerWin, "AI с big brain time должен тащить");
        }

        [UnityTest]
        public IEnumerator Test_AIPlaysAsPlayer_OnLoadedScene_Deep_8_vs_Deep_4_Giveaways()
        {
            GameSettings.Instance.IsGiveaways = true;

            // Загружаем сцену
            SceneManager.LoadScene(SceneNames.Game);
            yield return null;

            // Создаем объект AI и добавляем компонент
            GameSettings.Instance.AISearchDeep = 8; // big brain time on

            _aiGameObject = new GameObject("TestAIPlaysAsPlayer");
            _aiComponent = _aiGameObject.AddComponent<CheckersAI>();
            _aiComponent.AIOpponent = OpponentType.Player;

            yield return null;

            // Act - Ждем
            while (GameManager.Instance.CurrentEndOfGameState == EndOfGameType.None)
            {
                yield return new WaitForSeconds(0.3f);
            }

            // Assert
            Assert.IsTrue(GameManager.Instance.CurrentEndOfGameState == EndOfGameType.PlayerWin, "AI с big brain time должен тащить");
        }

        private List<CheckerData> CreateCustomBoard()
        {
            var checkers = new List<CheckerData>();

            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    if ((x + y) % 2 == 1) continue;

                    OpponentType type;

                    // Белые шашки (игрок)
                    if (y < 3)
                    {
                        type = OpponentType.Player;
                    }
                    // Черные шашки (AI)
                    else if (y > 4)
                    {
                        type = OpponentType.AI;
                    }
                    else
                    {
                        continue;
                    }

                    checkers.Add(new CheckerData(x, y, CheckerType.Usual, type, 8, 8));
                }
            }

            return checkers;
        }

    }
}

