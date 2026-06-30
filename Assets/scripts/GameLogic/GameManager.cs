using Assets.scripts.Core;
using Assets.scripts.Infrastructure;
using Assets.scripts.Presentation.GameScene;
using UnityEngine;

namespace Assets.scripts.GameLogic
{
    public class GameManager : MonoBehaviour
    {
        public bool IsPaused { get; private set; }
        public OpponentType CurrentMoveTurn { get; set; } = GameSettings.Instance.FirstMoveTurn;
        public EndOfGameType CurrentEndOfGameState { get; private set; } = EndOfGameType.None;
        public static GameManager Instance { get; private set; }

        private GameTabManager.PositionAssessmentHandle _positionAssessmentHandle;
        private GameTabManager.CurrentEndOfGameStateChangedHandle _currentEndOfGameStateChangedHandle;

        public GameTabManager GameTabManager { get; private set; }
        private MovesHistory _movesHistory;

        private CheckersAI _checkersAI;

        private void Awake()
        {
            Instance = this;
            _movesHistory = new MovesHistory();
            _checkersAI = FindAnyObjectByType<CheckersAI>(FindObjectsInactive.Include);
        }

        private void Start()
        {
            BoardManager.Instance.PositionChangedEvent += OnPositionChanged;
            GameTabManager = new GameTabManager(ref _movesHistory.CheckerMoveEvent, ref _positionAssessmentHandle, ref _currentEndOfGameStateChangedHandle);
            GameTabManager.SaveMovesHistoryButtonClickEvent += _movesHistory.SaveMovesHistoryInFile;
        }

        private void OnPositionChanged(CheckerData checker, CheckerMove move, bool isContinueBeating)
        {
            _positionAssessmentHandle?.Invoke(_checkersAI.GetCurrentPositionAssessment());
            _movesHistory.Add(move, checker.Type, isContinueBeating);
        }

        public void SwitchOpponentTurn()
        {
            UpdateCurrentGameState();

            if (CurrentEndOfGameState != EndOfGameType.None)
            {
                _currentEndOfGameStateChangedHandle?.Invoke(CurrentEndOfGameState);

                Pause();

                GameStatistic.Instance.AddGameResult(CurrentEndOfGameState,
                    GameSettings.Instance.FirstMoveTurn == OpponentType.Player, _movesHistory.CheckerMovesAsStrings.Count);
            }
            else CurrentMoveTurn = (CurrentMoveTurn == OpponentType.Player ? OpponentType.AI : OpponentType.Player);
        }

        public void Pause()
        {
            IsPaused = true;
            _checkersAI.StopCalculating();
            GameTabManager.ShowTab(GameTabManager.Tab.PauseTab);
        }

        public void UnPause()
        {
            IsPaused = false;
            _checkersAI.RestartCalculating();
            GameTabManager.HideTab(GameTabManager.Tab.PauseTab);
        }

        public bool IsEndOfGame()
        {
            return CurrentEndOfGameState != EndOfGameType.None;
        }

        private void UpdateCurrentGameState()
        {
            CurrentEndOfGameState = EndOfGameType.None;

            if (BoardManager.Instance.CurrentPosition.PlayerCheckerCount == 0)
                CurrentEndOfGameState = EndOfGameType.AIWin;
            else if (BoardManager.Instance.CurrentPosition.AICheckerCount == 0)
                CurrentEndOfGameState = EndOfGameType.PlayerWin;
            else if ((!BoardManager.Instance.CurrentPosition.IsOpponentCanMove(OpponentType.Player) && CurrentMoveTurn == OpponentType.AI) ||
                     (!BoardManager.Instance.CurrentPosition.IsOpponentCanMove(OpponentType.AI) && CurrentMoveTurn == OpponentType.Player) ||
                     _movesHistory.IsRuleOf15MovesFulFilled())
                CurrentEndOfGameState = EndOfGameType.Draw;

            if (GameSettings.Instance.IsGiveaways)
            {
                if (CurrentEndOfGameState == EndOfGameType.PlayerWin)
                    CurrentEndOfGameState = EndOfGameType.AIWin;
                else if (CurrentEndOfGameState == EndOfGameType.AIWin)
                    CurrentEndOfGameState = EndOfGameType.PlayerWin;
            }
        }
    }
}