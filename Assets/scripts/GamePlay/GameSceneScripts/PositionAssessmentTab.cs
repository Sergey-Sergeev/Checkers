using Assets.scripts.Core;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.scripts.GamePlay.GameSceneScripts
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
            BoardEntities.Instance.MovesHistory.CheckerMoveEvent += UpdateScaleValue;
        }

        protected override void OnRectTransformDimensionsChange()
        {
            UpdateScaleValue();
        }

        private void UpdateScaleValue()
        {
            if (_checkersAI == null || BoardEntities.Instance == null || BoardEntities.Instance.CurrentPosition == null) return;

            float value = _checkersAI.Minimax.GetPositionAssessment(BoardEntities.Instance.CurrentPosition);
            _scaleText.text = $"{value:F2}";
            _foregroundScaleRect.offsetMin = new Vector2(
                _foregroundScaleRect.offsetMin.x,
                _backgroundScaleRect.rect.height * (1 - value)
            );
        }
    }

}