using UnityEngine;

namespace MainCourse
{
    public class FlickerPlatform : MonoBehaviour
    {
        public float onTime = 1.6f;
        public float offTime = 0.9f;
        public float startOffset = 0f;

        bool lit;
        float timer;
        Collider[] cols;
        Renderer[] renderers;

        void Awake()
        {
            cols = GetComponentsInChildren<Collider>();
            renderers = GetComponentsInChildren<Renderer>();

            float cycle = onTime + offTime;
            float f = Mathf.Repeat(Mathf.Abs(startOffset), cycle);
            if (f < onTime)
            {
                lit = true;
                timer = onTime - f;
            }
            else
            {
                lit = false;
                timer = cycle - f;
            }
            ApplyState();
        }

        void Update()
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                lit = !lit;
                ApplyState();
                timer = lit ? onTime : offTime;
            }
        }

        void ApplyState()
        {
            for (int i = 0; i < cols.Length; i++)
                cols[i].enabled = lit;
            for (int i = 0; i < renderers.Length; i++)
                renderers[i].enabled = lit;
        }
    }
}