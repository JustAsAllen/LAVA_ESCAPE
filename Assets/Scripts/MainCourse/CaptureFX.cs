using UnityEngine;

namespace MainCourse
{
    public class CaptureFX : MonoBehaviour
    {
        public Material burstMat;
        public Transform worldRoot;

        public void Burst(Vector3 pos)
        {
            if (burstMat == null) return;
            Transform parent = worldRoot != null ? worldRoot : transform;
            for (int i = 0; i < 16; i++)
            {
                var g = GameObject.CreatePrimitive(PrimitiveType.Cube);
                g.name = "Spark_" + i;
                g.transform.SetParent(parent, false);
                g.transform.position = pos;
                g.transform.localScale = Vector3.one * 0.2f;
                g.GetComponent<MeshRenderer>().sharedMaterial = burstMat;
                var c = g.GetComponent<Collider>();
                if (c != null) Destroy(c);
                var spark = g.AddComponent<Spark>();
                spark.velocity = (Random.insideUnitSphere + Vector3.up * 0.6f).normalized * (6f + Random.value * 7f);
            }
        }
    }

    public class Spark : MonoBehaviour
    {
        public Vector3 velocity;
        float t;

        void Update()
        {
            t += Time.deltaTime;
            transform.position += velocity * Time.deltaTime;
            velocity *= 1f - 1.8f * Time.deltaTime;
            transform.localScale = Vector3.one * 0.2f * Mathf.Clamp01(1.4f - t);
            if (t >= 1.2f) Destroy(gameObject);
        }
    }
}