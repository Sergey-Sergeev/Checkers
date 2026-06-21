using System;
using System.IO;
using UnityEngine;

namespace Assets
{
    public static class GameStatistic
    {
        public static int AIWinsWhite => _aiWinsWhite;
        public static int AIWinsBlack => _aiWinsBlack;
        public static int PlayerWinsWhite => _playerWinsWhite;
        public static int PlayerWinsBlack => _playerWinsBlack;
        public static int TotalGames => _totalGames;
        public static int MaxMoves => _maxMoves;

        private static int _aiWinsWhite;
        private static int _aiWinsBlack;
        private static int _playerWinsWhite;
        private static int _playerWinsBlack;
        private static int _totalGames;
        private static int _maxMoves;

        private const string SAVE_FILE_NAME = "stats.json";


        [System.Serializable]
        private class StatData
        {
            public int aiWinsWhite;
            public int aiWinsBlack;
            public int playerWinsWhite;
            public int playerWinsBlack;
            public int totalGames;
            public int maxMoves;
        }

        public static void ReadStatisticFromFile()
        {
            string path = GetFilePath();

            if (!File.Exists(path))
            {
                ResetStatistics();
                return;
            }

            try
            {
                string json = File.ReadAllText(path);
                StatData data = JsonUtility.FromJson<StatData>(json);

                if (data != null)
                {
                    _aiWinsWhite = data.aiWinsWhite;
                    _aiWinsBlack = data.aiWinsBlack;
                    _playerWinsWhite = data.playerWinsWhite;
                    _playerWinsBlack = data.playerWinsBlack;
                    _totalGames = data.totalGames;
                    _maxMoves = data.maxMoves;
                }
                else
                {
                    ResetStatistics();
                }
            }
            catch (Exception e)
            {
                ResetStatistics();
            }
        }

        public static void SaveStatisticToFile()
        {
            try
            {
                string path = GetFilePath();
                StatData data = new StatData
                {
                    aiWinsWhite = _aiWinsWhite,
                    aiWinsBlack = _aiWinsBlack,
                    playerWinsWhite = _playerWinsWhite,
                    playerWinsBlack = _playerWinsBlack,
                    totalGames = _totalGames,
                    maxMoves = _maxMoves
                };

                string json = JsonUtility.ToJson(data, true);
                File.WriteAllText(path, json);
            }
            catch (Exception e)
            {
                Debug.LogError("Fail to save statistic.");
            }
        }

        public static void ResetStatistics()
        {
            _aiWinsWhite = 0;
            _aiWinsBlack = 0;
            _playerWinsWhite = 0;
            _playerWinsBlack = 0;
            _totalGames = 0;
            _maxMoves = 0;
        }

        public static void AddGameResult(bool playerWon, bool playerPlayedWhite, int movesCount)
        {
            _totalGames++;

            if (movesCount > _maxMoves)
                _maxMoves = movesCount;

            if (playerWon)
            {
                if (playerPlayedWhite)
                    _playerWinsWhite++;
                else
                    _playerWinsBlack++;
            }
            else
            {
                if (playerPlayedWhite)
                    _aiWinsBlack++;
                else
                    _aiWinsWhite++;
            }

            SaveStatisticToFile();
        }

        private static string GetFilePath()
        {
            return Application.dataPath + "/" + SAVE_FILE_NAME;
        }
    }
}