using Assets.scripts.Core;
using Assets.scripts.GamePlay.GameSceneScripts;

namespace Assets.scripts.GamePlay
{
    [System.Serializable]
    public class StatData
    {
        public int aiWinsWhite;
        public int aiWinsBlack;
        public int playerWinsWhite;
        public int playerWinsBlack;
        public int totalGames;
        public int maxMoves;
        public int drawCount;
    }

    public class GameStatistic : FileStorage<StatData>
    {
        public int AIWinsWhite => _data.aiWinsWhite;
        public int AIWinsBlack => _data.aiWinsBlack;
        public int PlayerWinsWhite => _data.playerWinsWhite;
        public int PlayerWinsBlack => _data.playerWinsBlack;
        public int TotalGames => _data.totalGames;
        public int MaxMoves => _data.maxMoves;
        public int DrawCount => _data.drawCount;

        protected override string FileName => "stats.json";

        private StatData _data;
        private static GameStatistic _instance;

        public static GameStatistic Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GameStatistic();
                }
                return _instance;
            }
        }

        private GameStatistic() { }


        protected override StatData GetDefaultData()
        {
            return new StatData
            {
                aiWinsWhite = 0,
                aiWinsBlack = 0,
                playerWinsWhite = 0,
                playerWinsBlack = 0,
                totalGames = 0,
                maxMoves = 0,
                drawCount = 0
            };
        }

        public void LoadData() => _data = Load();
        public void SaveData() { if (_data != null) Save(_data); }
        public void ResetStatistics() { _data = GetDefaultData(); SaveData(); }

        public void AddGameResult(EndOfGameType endOfGame, bool playerPlayedWhite, int movesCount)
        {
            if (_data == null)
            {
                _data = GetDefaultData();
            }

            _data.totalGames++;

            if (movesCount > _data.maxMoves)
                _data.maxMoves = movesCount;

            if (endOfGame == EndOfGameType.PlayerWin)
            {
                if (playerPlayedWhite)
                    _data.playerWinsWhite++;
                else
                    _data.playerWinsBlack++;
            }
            else if (endOfGame == EndOfGameType.AIWin)
            {
                if (playerPlayedWhite)
                    _data.aiWinsBlack++;
                else
                    _data.aiWinsWhite++;
            }
            else
            {
                _data.drawCount++;
            }

            SaveData();
        }

        public static void SaveStatisticToFile() => Instance.SaveData();
        public static void ResetStatisticsStatic() => Instance.ResetStatistics();
        public static void AddGameResultStatic(EndOfGameType endOfGame, bool playerPlayedWhite, int movesCount)
            => Instance.AddGameResult(endOfGame, playerPlayedWhite, movesCount);
    }
}