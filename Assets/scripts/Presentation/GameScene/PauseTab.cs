using Assets.scripts.Infrastructure;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Assets.scripts.Presentation.GameScene
{
    public delegate void SaveMovesHistoryButtonClickHandle();

    internal class PauseTab : UIBehaviour
    {
        [SerializeField] private TMP_Text _title;
        [SerializeField] private Button _menuButton;
        [SerializeField] private Button _saveMovesHistoryButton;
        [SerializeField] private Button _exitButton;

        public static PauseTab Instance { get; private set; }

        public event SaveMovesHistoryButtonClickHandle SaveMovesHistoryButtonEvent;

        private const float SAVE_MOVES_HISTORY_BUTTON_DELAY_SECONDS = 1f;
        private string _originalButtonText;
        private bool _isTextSaveMoveHistoryButtonChanged = false;

        public PauseTab()
        {
            Instance = this;
        }

        protected override void Start()
        {
            _exitButton.onClick.AddListener(ExitButton_onClick);
            _saveMovesHistoryButton.onClick.AddListener(SaveMovesHistoryButton_clicked);
            _menuButton.onClick.AddListener(MenuButton_onClickAsync);
        }

        public void SetPauseTitle(EndOfGameType gameType)
        {
            _title.text = gameType switch
            {
                EndOfGameType.AIWin => GameSettings.AI_WIN_PAUSE_TITLE,
                EndOfGameType.PlayerWin => GameSettings.PLAYER_WIN_PAUSE_TITLE,
                EndOfGameType.Draw => GameSettings.DRAW_PAUSE_TITLE,
                _ => GameSettings.DEFAULT_PAUSE_TITLE,
            };
        }

        private void SaveMovesHistoryButton_clicked()
        {
            SaveMovesHistoryButtonEvent?.Invoke();

            TMP_Text text = _saveMovesHistoryButton.GetComponentInChildren<TMP_Text>();
            _originalButtonText = text.text;
            text.text = "Saved!";
            _saveMovesHistoryButton.interactable = false;
            _isTextSaveMoveHistoryButtonChanged = true;

            Invoke(nameof(RestoreButtonText), SAVE_MOVES_HISTORY_BUTTON_DELAY_SECONDS);
        }

        private void MenuButton_onClickAsync()
        {
            SceneManager.LoadScene(SceneNames.Menu);
        }

        private void ExitButton_onClick()
        {
            PlatformService.Quit();
        }

        private void RestoreButtonText()
        {
            TMP_Text text = _saveMovesHistoryButton.GetComponentInChildren<TMP_Text>();
            text.text = _originalButtonText;
            _saveMovesHistoryButton.interactable = true;
            _isTextSaveMoveHistoryButtonChanged = false;
        }

        protected override void OnDisable()
        {
            if (_isTextSaveMoveHistoryButtonChanged)
            {
                CancelInvoke(nameof(RestoreButtonText));
                RestoreButtonText();
            }
        }

    }
}