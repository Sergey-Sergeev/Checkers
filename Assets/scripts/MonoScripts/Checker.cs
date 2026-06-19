using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Assets
{
    public class Checker : MonoBehaviour
    {
        private const float CHECKER_ROTATE_SPEED = 5;
        private const float PARTICLES_DESTROY_TIME = 5f;
        private const float PIXEL_SIZE = 0.1f;

        public CheckerData Data;
        public bool IsMoving { get; set; } = false;

        public void Set(Color color, int x, int y, CheckerType type, OpponentType opponent)
        {
            GetComponent<Renderer>().material.color = color;
            Data = new CheckerData(x, y, type, opponent);
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