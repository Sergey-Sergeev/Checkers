using UnityEngine;
using System.Collections;

namespace Assets
{
	public class PauseTab : UIElement
	{

        void Start()
        {
            Game.UnPause();
        }

        void Update()
        {
            
        }

        protected override void OnDisable()
        {
            Game.UnPause();
        }

        protected override void OnEnable()
        {
            Game.Pause();
        }

    }
}