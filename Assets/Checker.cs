using UnityEngine;
using System.Collections.Generic;
using NUnit.Framework;

namespace Assets
{
    public class Checker : MonoBehaviour
    {
        [SerializeField] private const float PARTICLES_DESTROY_TIME = 5f;
        [SerializeField] private const float PIXEL_SIZE = 0.1f;

        public bool IsMoving { get; set; } = false;
        public Color Color { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public CheckerType Type { get; set; }
        public OpponentType Opponent { get; set; }
        public bool IsBeaten { get; set; }

        public void Set(Color color, int x, int y, CheckerType type, OpponentType opponent, bool isBeaten = false)
        {
            Color = color;
            X = x;
            Y = y;
            Type = type;
            Opponent = opponent;
            IsBeaten = isBeaten;
        }

        public void Destroy()
        {
            IsBeaten = true;

            GameObject originalObject = gameObject;
            Vector3 originalPosition = originalObject.transform.position;

            Renderer renderer = originalObject.GetComponent<Renderer>();
            if (renderer == null) return;

            Bounds bounds = renderer.bounds;
            Material material = renderer.sharedMaterial;

            int pixelsX = Mathf.CeilToInt(bounds.size.x / PIXEL_SIZE);
            int pixelsY = Mathf.CeilToInt(bounds.size.y / PIXEL_SIZE);
            int pixelsZ = Mathf.CeilToInt(bounds.size.z / PIXEL_SIZE);

            float radius = bounds.size.x / 2f;
            float height = bounds.size.y;

            for (int x = 0; x < pixelsX; x++)
            {
                for (int z = 0; z < pixelsZ; z++)
                {
                    for (int y = 0; y < pixelsY; y++)
                    {
                        Vector3 pixelPos = bounds.min + new Vector3(
                            (x + 0.5f) * PIXEL_SIZE,
                            (y + 0.5f) * PIXEL_SIZE,
                            (z + 0.5f) * PIXEL_SIZE
                        );

                        Vector3 localPos = originalObject.transform.InverseTransformPoint(pixelPos);
                        float distanceFromCenter = Mathf.Sqrt(localPos.x * localPos.x + localPos.z * localPos.z);

                        if (distanceFromCenter <= radius && Mathf.Abs(localPos.y) <= height / 2f)
                        {
                            GameObject pixel = GameObject.CreatePrimitive(PrimitiveType.Cube);
                            pixel.transform.position = pixelPos;
                            pixel.transform.localScale = Vector3.one * PIXEL_SIZE * 0.95f;
                            pixel.GetComponent<Renderer>().material = material;

                            Rigidbody rb = pixel.AddComponent<Rigidbody>();
                            rb.mass = 0.01f;

                            Vector3 force = (pixelPos - originalPosition).normalized * Random.Range(100f, 300f);
                            rb.AddForce(force);
                            rb.AddTorque(Random.insideUnitSphere * 150f);

                            Destroy(pixel, PARTICLES_DESTROY_TIME);
                        }
                    }
                }
            }

            Destroy(originalObject);
        }

    }
}