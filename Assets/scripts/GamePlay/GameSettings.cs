using Assets.scripts.Core;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Assets.scripts.GamePlay
{
    public static class GameSettings
    {
        public const int BOARD_MAX_HEIGHT = 10;
        public const int BOARD_MAX_WIDTH = 10;
        public const int BOARD_MIN_HEIGHT = 4;
        public const int BOARD_MIN_WIDTH = 4;
        public const int AI_MAX_SEARCH_DEEP = 15;
        public const int AI_MIN_SEARCH_DEEP = 1;
        public const int MAX_COUNT_OF_CHECKERS_FOR_OPPONENT = 20;
        public const int MIN_COUNT_OF_CHECKERS_FOR_OPPONENT = 4;


        public static int OpponentCountOfChechers = 12;
        public static OpponentType FirstMoveTurn = OpponentType.Player;
        public static int BoardHeight = 8;
        public static int BoardWidth = 8;
        public static int AISearchDeep = 6;
        public static bool IsGiveaways = false;


        // Change colors maybe added in the future
        public static Color CellColor1 = Color.black;
        public static Color CellColor2 = Color.white;
        public static Color CheckerColor1 = Color.white;
        public static Color CheckerColor2 = Color.black;


        public static bool IsCustomBoard = false;
        // Custom boards supports only for developing a while
        public static List<CheckerData> CustomBoardPosition = new List<CheckerData>() { };


        private const string SAVE_FILE_NAME = "settings.json";


        [System.Serializable]
        private class SettingsData
        {
            public int _opponentCountOfChechers;
            public OpponentType _firstMoveTurn;
            public int _boardHeight;
            public int _boardWidth;
            public int _aiSearchDeep;
            public bool _isGiveaways;
        }

        public static void ReadSettingsFromFile()
        {
            string path = GetFilePath();

            if (!File.Exists(path))
            {
                ResetGameSettings();
                return;
            }

            try
            {
                string json = File.ReadAllText(path);
                SettingsData data = JsonUtility.FromJson<SettingsData>(json);

                if (data != null)
                {
                    OpponentCountOfChechers = data._opponentCountOfChechers;
                    FirstMoveTurn = data._firstMoveTurn;
                    BoardHeight = data._boardHeight;
                    BoardWidth = data._boardWidth;
                    AISearchDeep = data._aiSearchDeep;
                    IsGiveaways = data._isGiveaways;
                }
                else
                {
                    ResetGameSettings();
                }
            }
            catch (Exception e)
            {
                ResetGameSettings();
            }
        }

        public static void SaveGameSettingsToFile()
        {
            try
            {
                string path = GetFilePath();
                SettingsData data = new SettingsData
                {
                    _opponentCountOfChechers = OpponentCountOfChechers,
                    _firstMoveTurn = FirstMoveTurn,
                    _boardHeight = BoardHeight,
                    _boardWidth = BoardWidth,
                    _aiSearchDeep = AISearchDeep,
                    _isGiveaways = IsGiveaways
                };

                string json = JsonUtility.ToJson(data, true);
                File.WriteAllText(path, json);
            }
            catch (Exception e)
            {
                Debug.LogError("Fail to save statistic.");
            }
        }

        public static void ResetGameSettings()
        {
            OpponentCountOfChechers = 12;
            FirstMoveTurn = OpponentType.Player;
            BoardHeight = 8;
            BoardWidth = 8;
            AISearchDeep = 10;
            IsGiveaways = false;

        }

        private static string GetFilePath()
        {
            return Application.dataPath + "/" + SAVE_FILE_NAME;
        }
    }
}