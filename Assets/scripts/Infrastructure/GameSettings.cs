using Assets.scripts.Core;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.scripts.Infrastructure
{
    public class GameSettings : FileStorage<GameSettings.SettingsData>
    {
        public const string PLAYER_STR = "Player";
        public const string AI_STR = "AI";
        public const string DRAW_PAUSE_TITLE = "Draw";
        public const string AI_WIN_PAUSE_TITLE = AI_STR + " - win";
        public const string PLAYER_WIN_PAUSE_TITLE = PLAYER_STR + " - win";
        public const string DEFAULT_PAUSE_TITLE = "Pause";

        public const int BOARD_MAX_HEIGHT = 10;
        public const int BOARD_MAX_WIDTH = 10;
        public const int BOARD_MIN_HEIGHT = 4;
        public const int BOARD_MIN_WIDTH = 4;
        public const int AI_MAX_SEARCH_DEEP = 11;
        public const int AI_MIN_SEARCH_DEEP = 1;
        public const int MAX_COUNT_OF_CHECKERS_FOR_OPPONENT = 20;
        public const int MIN_COUNT_OF_CHECKERS_FOR_OPPONENT = 1;

        public int OpponentCountOfChechers { get => _data.opponentCountOfChechers;
            set {
                if (value >= MIN_COUNT_OF_CHECKERS_FOR_OPPONENT && value <= MAX_COUNT_OF_CHECKERS_FOR_OPPONENT)
                {
                    int count = 0;
                    for (int i = 0; i < BoardWidth; i++)
                        for (int j = 0; j < BoardHeight; j++)
                            if ((i + j) % 2 == 0)
                                count++;

                    int maxForCurrentBoard = count - 1;

                    _data.opponentCountOfChechers = 2 * value > maxForCurrentBoard ? maxForCurrentBoard / 2 : value;                    
                }
            } }

        public OpponentType FirstMoveTurn { get => _data.firstMoveTurn; set => _data.firstMoveTurn = value; }
        public int BoardHeight { get => _data.boardHeight; set => _data.boardHeight = Mathf.Clamp(value, BOARD_MIN_HEIGHT, BOARD_MAX_HEIGHT); }
        public int BoardWidth { get => _data.boardWidth; set => _data.boardWidth = Mathf.Clamp(value, BOARD_MIN_WIDTH, BOARD_MAX_WIDTH); }
        public int AISearchDeep { get => _data.aiSearchDeep; set => _data.aiSearchDeep = Mathf.Clamp(value, AI_MIN_SEARCH_DEEP, AI_MAX_SEARCH_DEEP); }
        public bool IsGiveaways { get => _data.isGiveaways; set => _data.isGiveaways = value; }

        protected override string SaveFileName => "settings.json";

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

        private GameSettings() { _data = GetDefaultData(); }


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
            Save(_data);
        }

        public void ResetSettings()
        {
            _data = GetDefaultData();
        }

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
    }
}