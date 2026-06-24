using Assets.scripts.GamePlay.GameSceneScripts;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static TMPro.TMP_Dropdown;
using Assets.scripts.Core;

namespace Assets.scripts.GamePlay.MenuSceneScripts
{
    public class SettingsTab : MonoBehaviour
    {
        [SerializeField] private GameObject _menuTabObj;
        [SerializeField] private Button _menuButton;

        [SerializeField] private TMP_Dropdown _firstMoveTurnDropdown;
        [SerializeField] private TMP_InputField _boardHeightInputField;
        [SerializeField] private TMP_InputField _boardWidthInputField;
        [SerializeField] private TMP_InputField _countOfCheckersPerOpponentInputField;
        [SerializeField] private TMP_InputField _aiSearchDeepInputField;
        [SerializeField] private Toggle _isGiveawaysToggle;


        void Start()
        {
            GameSettings.Instance.LoadData();

            SetCurrentGameSettingsValues();
            _menuButton.onClick.AddListener(SaveAndBackToMenu);

            _boardHeightInputField.onSubmit.AddListener((s) => ValidateValue(_boardHeightInputField, s, GameSettings.BOARD_MIN_HEIGHT, GameSettings.BOARD_MAX_HEIGHT));
            _boardWidthInputField.onSubmit.AddListener((s) => ValidateValue(_boardWidthInputField, s, GameSettings.BOARD_MIN_WIDTH, GameSettings.BOARD_MAX_WIDTH));
            _aiSearchDeepInputField.onSubmit.AddListener((s) => ValidateValue(_aiSearchDeepInputField, s, GameSettings.AI_MIN_SEARCH_DEEP, GameSettings.AI_MAX_SEARCH_DEEP));
            _countOfCheckersPerOpponentInputField.onSubmit.AddListener(
                (s) => ValidateValue(_countOfCheckersPerOpponentInputField, s, GameSettings.MIN_COUNT_OF_CHECKERS_FOR_OPPONENT, GameSettings.MAX_COUNT_OF_CHECKERS_FOR_OPPONENT));

            _boardHeightInputField.onDeselect.AddListener((s) => ValidateValue(_boardHeightInputField, s, GameSettings.BOARD_MIN_HEIGHT, GameSettings.BOARD_MAX_HEIGHT));
            _boardWidthInputField.onDeselect.AddListener((s) => ValidateValue(_boardWidthInputField, s, GameSettings.BOARD_MIN_WIDTH, GameSettings.BOARD_MAX_WIDTH));
            _aiSearchDeepInputField.onDeselect.AddListener((s) => ValidateValue(_aiSearchDeepInputField, s, GameSettings.AI_MIN_SEARCH_DEEP, GameSettings.AI_MAX_SEARCH_DEEP));
            _countOfCheckersPerOpponentInputField.onDeselect.AddListener(
                 (s) => ValidateValue(_countOfCheckersPerOpponentInputField, s, GameSettings.MIN_COUNT_OF_CHECKERS_FOR_OPPONENT, GameSettings.MAX_COUNT_OF_CHECKERS_FOR_OPPONENT));

        }

        private void ValidateValue(TMP_InputField inputField, string input, int min, int max)
        {
            if (int.TryParse(input, out int val))
            {
                if (val < min)
                    inputField.text = min.ToString();
                else if (val > max)
                    inputField.text = max.ToString();
                else inputField.text = input;
            }
            else
            {
                inputField.text = min.ToString();
            }
        }

        private void OnEnable()
        {
            SetCurrentGameSettingsValues();
        }

        private void SetCurrentGameSettingsValues()
        {
            _firstMoveTurnDropdown.ClearOptions();
            _firstMoveTurnDropdown.AddOptions(new List<OptionData>() {
                new OptionData(Game.PLAYER_STR),
                new OptionData(Game.AI_STR)
            });

            _firstMoveTurnDropdown.value = GameSettings.Instance.FirstMoveTurn == OpponentType.Player ? 0 : 1;

            _boardHeightInputField.text = GameSettings.Instance.BoardHeight.ToString();
            _boardWidthInputField.text = GameSettings.Instance.BoardWidth.ToString();
            _aiSearchDeepInputField.text = GameSettings.Instance.AISearchDeep.ToString();
            _countOfCheckersPerOpponentInputField.text = GameSettings.Instance.OpponentCountOfChechers.ToString();

            _isGiveawaysToggle.isOn = GameSettings.Instance.IsGiveaways;
        }


        private void SaveAndBackToMenu()
        {
            GameSettings.Instance.SetFirstMoveTurn(_firstMoveTurnDropdown.value == 0 ? OpponentType.Player : OpponentType.AI);
            GameSettings.Instance.SetBoardSize(int.Parse(_boardHeightInputField.text), int.Parse(_boardWidthInputField.text));
            GameSettings.Instance.SetAISearchDeep(int.Parse(_aiSearchDeepInputField.text));
            GameSettings.Instance.SetIsGiveaways(_isGiveawaysToggle.isOn);          
            GameSettings.Instance.SetOpponentCountOfChechers(int.Parse(_countOfCheckersPerOpponentInputField.text));

            _menuTabObj.SetActive(true);
            gameObject.SetActive(false);
        }

    }
}