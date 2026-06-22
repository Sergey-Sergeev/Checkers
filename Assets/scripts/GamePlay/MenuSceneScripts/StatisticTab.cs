using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.scripts.GamePlay.MenuSceneScripts
{
    public class StatisticTab : MonoBehaviour
    {
        [SerializeField] private GameObject _menuTabObj;
        [SerializeField] private Button _menuButton;

        [SerializeField] private TMP_Text _aiWinsWhiteValueText;
        [SerializeField] private TMP_Text _aiWinsBlackValueText;
        [SerializeField] private TMP_Text _playerWinsWhiteValueText;
        [SerializeField] private TMP_Text _playerWinsBlackValueText;
        [SerializeField] private TMP_Text _totalGamesValueText;
        [SerializeField] private TMP_Text _maxMovesValueText;


        private bool _isCreated = false;

        void Start()
        {
            if (_isCreated) return;
            _isCreated = true;

            GameStatistic.ReadStatisticFromFile();
            UpdateStatisticValues();
            _menuButton.onClick.AddListener(() => { _menuTabObj.SetActive(true); gameObject.SetActive(false); });
        }

        private void OnEnable()
        {
            if (_isCreated)
                UpdateStatisticValues();
        }


        private void UpdateStatisticValues()
        {
            _aiWinsWhiteValueText.text = GameStatistic.AIWinsWhite.ToString();
            _aiWinsBlackValueText.text = GameStatistic.AIWinsBlack.ToString();
            _playerWinsWhiteValueText.text = GameStatistic.PlayerWinsWhite.ToString();
            _playerWinsBlackValueText.text = GameStatistic.PlayerWinsBlack.ToString();
            _totalGamesValueText.text = GameStatistic.TotalGames.ToString();
            _maxMovesValueText.text = GameStatistic.MaxMoves.ToString();
        }
    }
}