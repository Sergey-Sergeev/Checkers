using UnityEngine;
using System;

namespace Assets.scripts.GamePlay.GameSceneScripts.PresentationLayer
{
	public class CameraController: MonoBehaviour
	{
		[SerializeField] private float BOARD_MARGIN = 1f;
		[SerializeField] private float CAMERA_LOOK_AT_ANGLE = 50;

		void Start()
		{
			float distance = (GameSettings.Instance.BoardHeight * CheckersBoard.Instance.CellSize.z) / 2 + BOARD_MARGIN;
			float angle = CAMERA_LOOK_AT_ANGLE * (MathF.PI / 180f);
			float z = MathF.Cos(angle) * -distance;
			float y = MathF.Sin(angle) * distance;

			Camera.main.transform.position = new Vector3(CheckersBoard.Instance.CENTRE_COORDS.x, y, z);
			Camera.main.transform.LookAt(CheckersBoard.Instance.CENTRE_COORDS);
		}

	}
}