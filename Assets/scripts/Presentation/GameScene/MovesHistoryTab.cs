using Assets.scripts.Core;
using Assets.scripts.Infrastructure;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.scripts.Presentation.GameScene
{
    internal class MovesHistoryTab : UIBehaviour
    {
        [SerializeField] private GameObject _moveRowPrefab;
        [SerializeField] private Transform _contentParent;

        public static MovesHistoryTab Instance { get; private set; }

        private int _currentIndex = 0;
        private GameObject _currentRow;

        public MovesHistoryTab()
        {
            Instance = this;
        }

        protected override void Start()
        {
            CreateHeaders();
        }

        private void CreateHeaders()
        {
            GameObject headers = Instantiate(_moveRowPrefab, _contentParent);

            var nText = headers.transform.GetChild(0).GetComponent<TMP_Text>();
            var move1Text = headers.transform.GetChild(1).GetComponent<TMP_Text>();
            var move2Text = headers.transform.GetChild(2).GetComponent<TMP_Text>();

            nText.text = "#";
            move1Text.text = GameSettings.Instance.FirstMoveTurn == OpponentType.Player ? GameSettings.PLAYER_STR : GameSettings.AI_STR;
            move2Text.text = GameSettings.Instance.FirstMoveTurn == OpponentType.AI ? GameSettings.PLAYER_STR : GameSettings.AI_STR;
        }

        public void AddMove(string lastMove)
        {
            if (_currentIndex % 2 == 0)
            {
                _currentRow = Instantiate(_moveRowPrefab, _contentParent);

                var nText = _currentRow.transform.GetChild(0).GetComponent<TMP_Text>();
                var move1Text = _currentRow.transform.GetChild(1).GetComponent<TMP_Text>();
                var move2Text = _currentRow.transform.GetChild(2).GetComponent<TMP_Text>();

                int rowNumber = _currentIndex / 2;
                nText.text = (rowNumber + 1).ToString();
                move1Text.text = lastMove;

                move2Text.text = lastMove;
                move2Text.alpha = 0;


                var scrollRect = _contentParent.parent.parent.GetComponent<ScrollRect>();
                if (scrollRect != null)
                {
                    Canvas.ForceUpdateCanvases();
                    scrollRect.verticalNormalizedPosition = 0;
                }
            }
            else
            {
                if (_contentParent.childCount > 0)
                {
                    var lastRow = _contentParent.GetChild(_contentParent.childCount - 1);
                    var move2Text = lastRow.GetChild(2).GetComponent<TMP_Text>();
                    if (move2Text != null)
                    {
                        move2Text.text = lastMove;
                        move2Text.alpha = 1;
                    }
                }
            }

            _currentIndex++;
        }

    }
}