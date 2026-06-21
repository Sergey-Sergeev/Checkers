using UnityEngine.EventSystems;

namespace Assets
{
    public abstract class UIElement : UIBehaviour
    {
        public bool IsVisible { get => isActiveAndEnabled; }
    }
}