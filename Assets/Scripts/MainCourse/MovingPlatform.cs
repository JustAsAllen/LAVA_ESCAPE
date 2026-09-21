using UnityEngine;

namespace MainCourse
{
    public class MovingPlatform : MonoBehaviour
    {
        public Vector3 axis = Vector3.up;
        public float halfTravel = 1f;
        public float period = 2f;
        public float phase = 0f;

        Vector3 startLocal;

        void Awake()
        {
            startLocal = transform.localPosition;
        }

        void FixedUpdate()
        {
            float t = Mathf.Repeat((Time.time + period * phase) / period, 1f);
            float s = Mathf.Sin(t * Mathf.PI * 2f);
            transform.localPosition = startLocal + axis * halfTravel * s;
        }
    }
}