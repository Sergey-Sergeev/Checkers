using UnityEngine;
using System.Collections;

namespace Assets
{
	public class CheckersAI: MonoBehaviour
	{

		void Start()
		{

		}



		void Update()
		{
			if (Game.IsPaused || Game.CurrentMoveTurn != OpponentType.AI || Game.EndOfGame != EndOfGameType.None) return;




		}
	}
}