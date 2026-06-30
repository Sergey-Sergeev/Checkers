using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.scripts.Presentation.GameScene
{
    internal class PositionAssessmentTab : UIBehaviour
    {
        [SerializeField] private RectTransform _backgroundScaleRect;
        [SerializeField] private RectTransform _foregroundScaleRect;
        [SerializeField] private TextMeshProUGUI _scaleText;

        public static PositionAssessmentTab Instance { get; private set; }

        public PositionAssessmentTab()
        {
            Instance = this;
        }

        protected override void Start()
        {
            UpdateScaleValue(0.5f);
        }

        public void UpdateScaleValue(float value)
        {
            _scaleText.text = $"{value:F2}";
            _foregroundScaleRect.offsetMin = new Vector2(
                _foregroundScaleRect.offsetMin.x,
                _backgroundScaleRect.rect.height * (1 - value)
            );
        }
    }

}