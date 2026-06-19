using UnityEngine;
using System.Collections;

namespace Assets
{
	public static class GameSettings
	{
		public static OpponentType FirstMoveTurn = OpponentType.Player;
		public static int BoardHeight = 8;
		public static int BoardWidth = 8;
        public static Color CELL_COLOR_1 = Color.black;
		public static Color CELL_COLOR_2 = Color.white;
        public static Color CHECKER_COLOR_1 = Color.white;
		public static Color CHECKER_COLOR_2 = Color.black;
    }
}