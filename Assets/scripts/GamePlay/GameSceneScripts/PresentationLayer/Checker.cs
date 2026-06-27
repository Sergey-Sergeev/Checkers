using Assets.scripts.Core;
using System.Collections;
using UnityEngine;

namespace Assets.scripts.GamePlay.GameSceneScripts.PresentationLayer
{
    public class Checker : MonoBehaviour
    {
        private const float CHECKER_ROTATE_SPEED = 5;

        public CheckerData Data;
        public bool IsMoving { get; set; } = false;

        public void Set(Color color, int x, int y, CheckerType type, OpponentType opponent)
        {
            GetComponent<Renderer>().material.color = color;
            Data = new CheckerData(x, y, type, opponent, GameSettings.Instance.BoardHeight, GameSettings.Instance.BoardWidth);

            if (type == CheckerType.KING)
                TransformInKing();
        }

        public void TransformInKing()
        {
            Data.Type = CheckerType.KING;
            StartCoroutine(RotateCoroutine());
        }

        private IEnumerator RotateCoroutine()
        {
            while (IsMoving)
            {
                yield return null;
            }

            float start = 0;
            float end = 180;

            float progress = 0f;
            while (progress < 1f)
            {
                progress += CHECKER_ROTATE_SPEED * Time.deltaTime;
                transform.rotation = Quaternion.Euler(start + (end - start) * Mathf.Clamp01(progress), 0, 0);
                yield return null;
            }

            transform.rotation = Quaternion.Euler(end, 0, 0);
        }

        public void Destroy()
        {
            Destroy(gameObject);
        }
    }
}