namespace Assets.scripts.Infrastructure
{
    public class GameStatistic : FileStorage<GameStatistic.StatData>
    {
        public int AIWinsWhite => _data.aiWinsWhite;
        public int AIWinsBlack => _data.aiWinsBlack;
        public int PlayerWinsWhite => _data.playerWinsWhite;
        public int PlayerWinsBlack => _data.playerWinsBlack;
        public int TotalGames => _data.totalGames;
        public int MaxMoves => _data.maxMoves;
        public int DrawCount => _data.drawCount;

        protected override string SaveFileName => "stats.json";

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

        private GameStatistic() { _data = GetDefaultData(); }


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
        public void SaveData() { Save(_data); }

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
    }
}