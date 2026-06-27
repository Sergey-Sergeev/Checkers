using Assets.scripts.GamePlay.GameSceneScripts.GameLogicLayer;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.scripts.GamePlay.GameSceneScripts.PresentationLayer
{
    public class PositionAssessmentTab : UIBehaviour
    {
        [SerializeField] private RectTransform _backgroundScaleRect;
        [SerializeField] private RectTransform _foregroundScaleRect;
        [SerializeField] private TextMeshProUGUI _scaleText;

        private CheckersAI _checkersAI;

        protected override void Awake()
        {
            _checkersAI = FindAnyObjectByType<CheckersAI>(FindObjectsInactive.Include);
        }

        protected override void Start()
        {
            UpdateScaleValue();
            BoardManager.Instance.PositionChangedEvent += UpdateScaleValue;
        }

        private void UpdateScaleValue()
        {
            if (_checkersAI == null || BoardManager.Instance == null || BoardManager.Instance.CurrentPosition == null) return;

            float value = _checkersAI.Minimax.GetPositionAssessment(BoardManager.Instance.CurrentPosition);
            _scaleText.text = $"{value:F2}";
            _foregroundScaleRect.offsetMin = new Vector2(
                _foregroundScaleRect.offsetMin.x,
                _backgroundScaleRect.rect.height * (1 - value)
            );
        }
    }

}