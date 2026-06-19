using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace Assets
{
    public class PositionAssessmentTab : UIElement
    {
        [SerializeField] private RectTransform _backgroundScaleRect;
        [SerializeField] private RectTransform _foregroundScaleRect;
        [SerializeField] private TextMeshProUGUI _scaleText;


        protected override void Start()
        {
            UpdateScaleValue();
            BoardEntities.Instance.MovesHistory.CheckerMoveEvent += UpdateScaleValue;
        }

        private void UpdateScaleValue()
        {
            float value = CheckersAI.GetPositionAssessment(BoardEntities.Instance.CurrentPosition);
            _scaleText.text = $"{value:F2}";
            _foregroundScaleRect.offsetMin = new Vector2(
                _foregroundScaleRect.offsetMin.x,
                _backgroundScaleRect.rect.height * (1 - value)
            );
        }
    }

}