using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets
{
    public class PositionAssessmentTab : UIBehaviour
    {
        [SerializeField] private RectTransform _backgroundScaleRect;
        [SerializeField] private RectTransform _foregroundScaleRect;
        [SerializeField] private TextMeshProUGUI _scaleText;


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
            if(BoardEntities.Instance == null || BoardEntities.Instance.CurrentPosition == null) return;

            float value = CheckersAI.GetPositionAssessment(BoardEntities.Instance.CurrentPosition);
            _scaleText.text = $"{value:F2}";
            _foregroundScaleRect.offsetMin = new Vector2(
                _foregroundScaleRect.offsetMin.x,
                _backgroundScaleRect.rect.height * (1 - value)
            );
        }
    }

}