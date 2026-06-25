using Assets.scripts.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
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

        private void MenuButton_onClickAsync()
        {
            _checkersAI.StopCalculating();
            SceneManager.LoadScene(SceneNames.Menu);
        }

        private void SaveMovesHistoryButton_clicked()
        {
            string folderPath = Path.Combine(Application.dataPath, SAVES_FOLDER_NAME);
            Directory.CreateDirectory(folderPath);

            string filePath = Path.Combine(folderPath, $"moves_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.txt");

            var lines = new List<string>();

            string opponent1 = GameSettings.Instance.FirstMoveTurn == OpponentType.Player ? Game.PLAYER_STR : Game.AI_STR;
            string opponent2 = GameSettings.Instance.FirstMoveTurn == OpponentType.AI ? Game.PLAYER_STR : Game.AI_STR;

            lines.Add($"N\t|_{opponent1}_| |_{opponent2}_|");

            int movesCount = BoardEntities.Instance.MovesHistory.CheckerMovesAsStrings.Count;

            for (int i = 0; i < movesCount / 2; i++)
            {
                lines.Add($"{i + 1}:\t[{BoardEntities.Instance.MovesHistory.CheckerMovesAsStrings[i * 2]}] - " +
                    $"[{BoardEntities.Instance.MovesHistory.CheckerMovesAsStrings[i * 2 + 1]}]");
            }

            if (movesCount % 2 == 1)
                lines.Add($"{(movesCount / 2 + 1)}:\t[{BoardEntities.Instance.MovesHistory.CheckerMovesAsStrings[movesCount - 1]}]");

            if (Game.EndOfGame != EndOfGameType.None)
                lines.Add(_title.text);


#if UNITY_WEBGL && !UNITY_EDITOR
            string content = "";
            for (int i = 0; i < lines.Count; i++) content += $"{lines[i]}\n";

            string base64 = System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(content));                     
            Application.ExternalEval(
                 " (function(data, fname) { " +
                        " var binary = atob(data); " + 
                        " var len = binary.length; " + 
                        " var arr = new Uint8Array(len); " + 
                        " for (var i = 0; i < len; i++) arr[i] = binary.charCodeAt(i); " + 
                        " var blob = new Blob([arr]); " + 
                        " var link = document.createElement('a'); " + 
                        " link.download = fname; " + 
                        " link.href = URL.createObjectURL(blob); " + 
                        " document.body.appendChild(link); " + 
                        " link.click(); " + 
                        " document.body.removeChild(link); " + 
                 " })('" + base64 + "', '" + filePath + "');");                    
#else
            File.WriteAllLines(filePath, lines);
#endif


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

        private void ExitButton_onClick()
        {
            _checkersAI.StopCalculating();

#if UNITY_WEBGL && !UNITY_EDITOR // --
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
            Game.UnPause();
        }

        protected override void OnEnable()
        {
            Game.Pause();

            if (Game.EndOfGame != EndOfGameType.None)
            {
                if (Game.EndOfGame == EndOfGameType.Draw)
                    _title.text = $"Draw";
                else _title.text = $"{(Game.EndOfGame == EndOfGameType.PlayerWin ? Game.PLAYER_STR : Game.AI_STR)} - win";
            }
        }

    }
}