using UnityEngine;
using System.Collections;

namespace Assets
{
	public class PauseTab: MonoBehaviour, ITab
	{
        public bool IsVisible => Game.IsPaused;

        void Start()
        {

        }

        void Update()
        {

        }

        public void Hide()
        {

        }

        public void Show()
        {

        }
    }
}