using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets
{
    public abstract class UIElement : UIBehaviour
    {
        public bool IsVisible { get; private set; }
    }
}