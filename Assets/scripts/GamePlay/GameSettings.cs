using Assets.scripts.Core;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.scripts.GamePlay
{
    [System.Serializable]
    public class SettingsData
    {
        public int opponentCountOfChechers;
        public OpponentType firstMoveTurn;
        public int boardHeight;
        public int boardWidth;
        public int aiSearchDeep;
        public bool isGiveaways;
    }

    public class GameSettings : FileStorage<SettingsData>
    {
        public const int BOARD_MAX_HEIGHT = 10;
        public const int BOARD_MAX_WIDTH = 10;
        public const int BOARD_MIN_HEIGHT = 4;
        public const int BOARD_MIN_WIDTH = 4;
        public const int AI_MAX_SEARCH_DEEP = 11;
        public const int AI_MIN_SEARCH_DEEP = 1;
        public const int MAX_COUNT_OF_CHECKERS_FOR_OPPONENT = 20;
        public const int MIN_COUNT_OF_CHECKERS_FOR_OPPONENT = 4;

        public int OpponentCountOfChechers => _data.opponentCountOfChechers;
        public OpponentType FirstMoveTurn => _data.firstMoveTurn;
        public int BoardHeight => _data.boardHeight;
        public int BoardWidth => _data.boardWidth;
        public int AISearchDeep => _data.aiSearchDeep;
        public bool IsGiveaways => _data.isGiveaways;

        protected override string FileName => "settings.json";

        public Color CellColor1 = Color.black;
        public Color CellColor2 = Color.white;
        public Color CheckerColor1 = Color.white;
        public Color CheckerColor2 = Color.black;

        public bool IsCustomBoard = false;
        public List<CheckerData> CustomBoardPosition = new List<CheckerData>() { };

        private SettingsData _data;
        private static GameSettings _instance;


        public static GameSettings Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GameSettings();
                }
                return _instance;
            }
        }

        private GameSettings() { }


        protected override SettingsData GetDefaultData()
        {
            return new SettingsData
            {
                opponentCountOfChechers = 12,
                firstMoveTurn = OpponentType.Player,
                boardHeight = 8,
                boardWidth = 8,
                aiSearchDeep = 6,
                isGiveaways = false
            };
        }

        public void LoadData() => _data = Load();

        public void SaveData()
        {
            if (_data != null)
                Save(_data);
        }

        public void ResetSettings()
        {
            _data = GetDefaultData();
            SaveData();
        }

        public void SetOpponentCountOfChechers(int value)
        {
            if (_data != null && value >= MIN_COUNT_OF_CHECKERS_FOR_OPPONENT && value <= MAX_COUNT_OF_CHECKERS_FOR_OPPONENT)
            {
                int maxForCurrentBoard = 0;

                if (BoardWidth % 2 == 0)
                {
                    if (BoardHeight % 2 == 0)
                        maxForCurrentBoard = (BoardWidth / 2) * (BoardHeight / 2 - 1);
                    else maxForCurrentBoard = (BoardWidth / 2) * (BoardHeight / 2);
                }
                else
                {
                    if (BoardHeight % 2 == 0)
                        maxForCurrentBoard = (BoardWidth / 2) * (BoardHeight / 2 - 1);
                    else maxForCurrentBoard = (BoardWidth / 2 + 1) * (BoardHeight / 2) - 1;
                }
                
                _data.opponentCountOfChechers = value > maxForCurrentBoard ? maxForCurrentBoard : value;
                SaveData();
            }
        }

        public void SetFirstMoveTurn(OpponentType value)
        {
            if (_data != null)
            {
                _data.firstMoveTurn = value;
                SaveData();
            }
        }

        public void SetBoardSize(int height, int width)
        {
            if (_data != null &&
                height >= BOARD_MIN_HEIGHT && height <= BOARD_MAX_HEIGHT &&
                width >= BOARD_MIN_WIDTH && width <= BOARD_MAX_WIDTH)
            {
                _data.boardHeight = height;
                _data.boardWidth = width;
                SaveData();
            }
        }

        public void SetAISearchDeep(int value)
        {
            if (_data != null && value >= AI_MIN_SEARCH_DEEP && value <= AI_MAX_SEARCH_DEEP)
            {
                _data.aiSearchDeep = value;
                SaveData();
            }
        }

        public void SetIsGiveaways(bool value)
        {
            if (_data != null)
            {
                _data.isGiveaways = value;
                SaveData();
            }
        }
    }
}