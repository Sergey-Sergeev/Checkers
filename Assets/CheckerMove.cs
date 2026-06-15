using UnityEngine;
using System.Collections;

namespace Assets
{
	public class CheckerMove
	{
		public Vector2Int Start {  get; private set; }
		public Vector2Int End { get; private set; }
		public bool IsBeatChecker { get; private set; }

		public CheckerMove(Vector2Int start, Vector2Int end, bool isBeatChecker)
		{
			Start = start;
			End = end;
			IsBeatChecker = isBeatChecker;
		}

	}
}