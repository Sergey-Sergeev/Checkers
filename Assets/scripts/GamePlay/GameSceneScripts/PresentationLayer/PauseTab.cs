using Assets.scripts.GamePlay.GameSceneScripts.GameLogicLayer;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Assets.scripts.GamePlay.GameSceneScripts.PresentationLayer
{
    public class PauseTab : UIBehaviour
    {
        [SerializeField] private TMP_Text _title;
        [SerializeField] private Button _menuButton;
        [SerializeField] private Button _saveMovesHistoryButton;
        [SerializeField] private Button _exitButton;

        private const float SAVE_MOVES_HISTORY_BUTTON_DELAY_SECONDS = 1f;
        private string _originalButtonText;
        private bool _isTextSaveMoveHistoryButtonChanged = false;

        private CheckersAI _checkersAI;

        protected override void Start()
        {
            _checkersAI = FindAnyObjectByType<CheckersAI>(FindObjectsInactive.Include);
            _exitButton.onClick.AddListener(ExitButton_onClick);
            _saveMovesHistoryButton.onClick.AddListener(SaveMovesHistoryButton_clicked);
            _menuButton.onClick.AddListener(MenuButton_onClickAsync);
        }

        private void SaveMovesHistoryButton_clicked()
        {
            BoardManager.Instance.MovesHistory.SaveMovesHistoryInFile();

            TMP_Text text = _saveMovesHistoryButton.GetComponentInChildren<TMP_Text>();
            _originalButtonText = text.text;
            text.text = "Saved!";
            _saveMovesHistoryButton.interactable = false;
            _isTextSaveMoveHistoryButtonChanged = true;

            Invoke(nameof(RestoreButtonText), SAVE_MOVES_HISTORY_BUTTON_DELAY_SECONDS);
        }

        private void MenuButton_onClickAsync()
        {
            _checkersAI.StopCalculating();
            SceneManager.LoadScene(SceneNames.Menu);
        }

        private void RestoreButtonText()
        {
            TMP_Text text = _saveMovesHistoryButton.GetComponentInChildren<TMP_Text>();
            text.text = _originalButtonText;
            _saveMovesHistoryButton.interactable = true;
            _isTextSaveMoveHistoryButtonChanged = false;
        }

        private void ExitButton_onClick()
        {
            _checkersAI.StopCalculating();

#if UNITY_WEBGL && !UNITY_EDITOR
            Application.ExternalEval("window.close();");
#else
            Application.Quit();
#endif
        }

        protected override void OnDisable()
        {
            if (_isTextSaveMoveHistoryButtonChanged)
            {
                CancelInvoke(nameof(RestoreButtonText));
                RestoreButtonText();
            }

            _checkersAI.RestartCalculating();
            GameState.UnPause();
        }

        protected override void OnEnable()
        {
            GameState.Pause();

            if (GameState.EndOfGame != EndOfGameType.None)
            {
                _title.text = GameState.EndOfGame switch
                {
                    EndOfGameType.AIWin => GameSettings.AI_WIN_PAUSE_TITLE,
                    EndOfGameType.PlayerWin => GameSettings.PLAYER_WIN_PAUSE_TITLE,
                    EndOfGameType.Draw => GameSettings.DRAW_PAUSE_TITLE,
                    _ => GameSettings.DRAW_PAUSE_TITLE,
                };
            }
        }

    }
}