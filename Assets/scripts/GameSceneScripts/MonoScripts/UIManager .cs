using UnityEngine;

namespace Assets
{
	public class UIManager : MonoBehaviour
	{
		public static UIManager Instance;

		private PositionAssessmentTab _positionAssessmentTab;
		private MovesHistoryTab _movesHistoryTab;
		private PauseTab _pauseTab;

        private void Awake()
        {
            Instance = this;
        }

        void Start()
		{
			_positionAssessmentTab = FindAnyObjectByType<PositionAssessmentTab>(FindObjectsInactive.Include);
            _movesHistoryTab = FindAnyObjectByType<MovesHistoryTab>(FindObjectsInactive.Include);
			_pauseTab = FindAnyObjectByType<PauseTab>(FindObjectsInactive.Include);
        }

		public void SwitchTabState(Tab tab)
		{
			if (IsTabVisible(tab))
				HideTab(tab);
			else ShowTab(tab);
		}

		public bool IsTabVisible(Tab tab)
		{
			return GetUIElement(tab).IsVisible;
        }

		public void HideTab(Tab tab)
		{
            GetUIElement(tab).gameObject.SetActive(false);
		}

		public void ShowTab(Tab tab)
		{
            GetUIElement(tab).gameObject.SetActive(true);
        }

		private UIElement GetUIElement(Tab tab)
		{
			switch (tab)
			{
				case Tab.PositionAssessment:
					return _positionAssessmentTab;                    
				case Tab.MovesHistory:
                    return _movesHistoryTab;                    
				case Tab.PauseTab:
                    return _pauseTab;					
			}
			return null;
		}		

		public enum Tab
		{
            PositionAssessment,
			MovesHistory,
			PauseTab
        }

	}
}