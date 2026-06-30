using Assets.scripts.Infrastructure;
using UnityEngine.EventSystems;

namespace Assets.scripts.Presentation.GameScene
{
	public class GameTabManager
	{
        public delegate void CheckerMoveHandle(string lastMove);
        public delegate void PositionAssessmentHandle(float value);
        public delegate void CurrentEndOfGameStateChangedHandle(EndOfGameType gameType);

        public event SaveMovesHistoryButtonClickHandle SaveMovesHistoryButtonClickEvent
        { add { PauseTab.Instance.SaveMovesHistoryButtonEvent += value; }  remove { PauseTab.Instance.SaveMovesHistoryButtonEvent -= value; } }

        public GameTabManager(ref CheckerMoveHandle fullMoveEvent, ref PositionAssessmentHandle positionAssessmentUpdateEvent,
           ref CurrentEndOfGameStateChangedHandle currentEndOfGameStateChangedEvent)
        {
            positionAssessmentUpdateEvent += PositionAssessmentTab.Instance.UpdateScaleValue;
            fullMoveEvent += MovesHistoryTab.Instance.AddMove;            
            currentEndOfGameStateChangedEvent += PauseTab.Instance.SetPauseTitle;
        }

        public void SwitchTabState(Tab tab)
        {
            if (IsTabVisible(tab))
                HideTab(tab);
            else ShowTab(tab);
        }

        public bool IsTabVisible(Tab tab)
        {
            return GetUIElementByTab(tab).isActiveAndEnabled;
        }

        public void HideTab(Tab tab)
        {
            GetUIElementByTab(tab).gameObject.SetActive(false);
        }

        public void ShowTab(Tab tab)
        {
            GetUIElementByTab(tab).gameObject.SetActive(true);
        }

        private UIBehaviour GetUIElementByTab(Tab tab)
        {
            switch (tab)
            {
                case Tab.PositionAssessment:
                    return PositionAssessmentTab.Instance;
                case Tab.MovesHistory:
                    return MovesHistoryTab.Instance;
                case Tab.PauseTab:
                    return PauseTab.Instance;
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