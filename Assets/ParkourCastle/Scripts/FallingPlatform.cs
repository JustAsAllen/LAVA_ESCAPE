using Climbing;
using UnityEngine;

namespace ParkourCastle
{
    public class FallingPlatform : MonoBehaviour
    {
        public float shakeTime = 0.55f;
        public float respawnTime = 3.5f;

        Rigidbody body;
        Renderer skin;
        Vector3 origin;
        Quaternion rest;

        bool triggered;
        bool dropped;
        float triggerTime;

        void Awake()
        {
            body = GetComponent<Rigidbody>();
            skin = GetComponent<Renderer>();
            origin = transform.position;
            rest = transform.rotation;
        }

        public void Trigger()
        {
            if (triggered || dropped) return;
            triggered = true;
            triggerTime = Time.time;
        }

        void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.GetComponentInParent<ThirdPersonController>() == null) return;
            Trigger();
        }

        void Update()
        {
            if (!triggered) return;

            float age = Time.time - triggerTime;

            if (!dropped)
            {
                if (age < shakeTime)
                {
                    float wobble = Mathf.Sin(age * 55f) * 0.07f;
                    transform.position = origin + new Vector3(wobble, 0f, wobble * 0.5f);
                }
                else
                {
                    dropped = true;
                    transform.position = origin;
                    if (body != null)
                    {
                        body.isKinematic = false;
                        body.useGravity = true;
                        body.linearVelocity = Vector3.zero;
                    }
                }
                return;
            }

            if (skin != null && skin.enabled && transform.position.y < origin.y - 10f)
                skin.enabled = false;

            if (age > shakeTime + respawnTime)
                Reset();
        }

        void Reset()
        {
            if (body != null)
            {
                body.isKinematic = true;
                body.useGravity = false;
                body.linearVelocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
            }

            transform.SetPositionAndRotation(origin, rest);

            if (skin != null) skin.enabled = true;
            triggered = false;
            dropped = false;
        }
    }
}
