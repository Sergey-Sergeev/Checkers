using UnityEngine;
using System.Collections;

namespace Assets
{
    public class MovesHistoryTab : MonoBehaviour, ITab
    {
        private bool _isVisible;
        public bool IsVisible => _isVisible;

        void Start()
        {

        }

        void Update()
        {

        }

        public void Hide()
        {
            _isVisible = false;
        }

        public void Show()
        {
            _isVisible = true;
        }

    }
}