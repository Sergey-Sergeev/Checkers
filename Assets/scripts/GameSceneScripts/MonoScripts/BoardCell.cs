using UnityEngine;
using System.Collections;

namespace Assets
{
	public class BoardCell: MonoBehaviour
	{
		public int X;
		public int Y;
		public bool IsHighlighted;
		public Material DefaultMaterial;

        public void Set(int x, int y, bool isHighlighted, Material defaultMaterial)
        {
            X = x;
            Y = y;
            IsHighlighted = isHighlighted;
            DefaultMaterial = defaultMaterial;
        }
	}
}