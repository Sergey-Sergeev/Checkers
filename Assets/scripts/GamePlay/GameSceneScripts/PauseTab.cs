using Assets.scripts.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Assets.scripts.GamePlay.GameSceneScripts
{
    public class PauseTab : UIBehaviour
    {
        [SerializeField] private TMP_Text _title;
        [SerializeField] private Button _menuButton;
        [SerializeField] private Button _saveMovesHistoryButton;
        [SerializeField] private Button _exitButton;

        private bool _exiting = false;

        private const string SAVES_FOLDER_NAME = "saves";
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

        private async void MenuButton_onClickAsync()
        {
            if (_exiting) return;
            _exiting = true;

            await _checkersAI.StopCalculating();
            SceneManager.LoadScene(SceneNames.Menu);
        }

        private void SaveMovesHistoryButton_clicked()
        {
            string folderPath = Path.Combine(Application.dataPath, SAVES_FOLDER_NAME);
            Directory.CreateDirectory(folderPath);

            string filePath = Path.Combine(folderPath, $"moves_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.txt");

            var lines = new List<string>();

            string opponent1 = GameSettings.FirstMoveTurn == OpponentType.Player ? Game.PLAYER_STR : Game.AI_STR;
            string opponent2 = GameSettings.FirstMoveTurn == OpponentType.AI ? Game.PLAYER_STR : Game.AI_STR;

            lines.Add($"N\t|_{opponent1}_| |_{opponent2}_|");

            for (int i = 0; i < BoardEntities.Instance.MovesHistory.CheckerMovesAsStrings.Count - 1; i += 2)
            {
                lines.Add($"{((i + 1) / 2)}:\t[{BoardEntities.Instance.MovesHistory.CheckerMovesAsStrings[i]}] - " +
                    $"[{BoardEntities.Instance.MovesHistory.CheckerMovesAsStrings[i + 1]}]");
            }

            if (Game.EndOfGame != EndOfGameType.None)
                lines.Add(_title.text);

            File.WriteAllLines(filePath, lines);

            TMP_Text text = _saveMovesHistoryButton.GetComponentInChildren<TMP_Text>();
            _originalButtonText = text.text;
            text.text = "Saved!";
            _saveMovesHistoryButton.interactable = false;
            _isTextSaveMoveHistoryButtonChanged = true;

            Invoke(nameof(RestoreButtonText), SAVE_MOVES_HISTORY_BUTTON_DELAY_SECONDS);
        }


        private void RestoreButtonText()
        {
            TMP_Text text = _saveMovesHistoryButton.GetComponentInChildren<TMP_Text>();
            text.text = _originalButtonText;
            _saveMovesHistoryButton.interactable = true;
            _isTextSaveMoveHistoryButtonChanged = false;
        }

        private async void ExitButton_onClick()
        {
            if (_exiting) return;
            _exiting = true;

            await _checkersAI.StopCalculating();
            Application.Quit();
            _exiting = false;   // in webgl you cant exit with code
        }

        protected override async void OnDisable()
        {
            if (_isTextSaveMoveHistoryButtonChanged)
            {
                CancelInvoke(nameof(RestoreButtonText));
                RestoreButtonText();
            }

            await _checkersAI.RestartCalculating();
            Game.UnPause();
        }

        protected override void OnEnable()
        {
            Game.Pause();

            if (Game.EndOfGame != EndOfGameType.None)
            {
                _title.text = $"{(Game.EndOfGame == EndOfGameType.PlayerWin ? Game.PLAYER_STR : Game.AI_STR)} - win";
            }
        }

    }
}