using UnityEngine;
using System.Collections;
using System;

namespace Assets
{
	public class CameraController: MonoBehaviour
	{
		[SerializeField] private const float BOARD_MARGIN = 10;
		[SerializeField] private const float CAMERA_LOOK_AT_ANGLE = 40;

		void Start()
		{
			float distance = (CheckersBoard.HEIGHT * CheckersBoard.Instance.CELL_HEIGHT) / 2 + BOARD_MARGIN;
			float z = CheckersBoard.Instance.CENTRE_COORDS.z - distance;
			float y = MathF.Sin(CAMERA_LOOK_AT_ANGLE) * distance;

			Camera.main.transform.position = new Vector3(CheckersBoard.Instance.CENTRE_COORDS.x, y, z);
			Camera.main.transform.LookAt(CheckersBoard.Instance.CENTRE_COORDS);
		}

	}
}