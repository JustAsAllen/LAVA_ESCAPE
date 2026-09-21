using UnityEngine;

namespace ParkourCastle
{
    public class RollingTrap : MonoBehaviour
    {
        public Vector3 direction = Vector3.right;
        public float distance = 5f;
        public float speed = 1.5f;
        public float phase = 0f;
        public Vector3 spinAxis = Vector3.right;
        public float spinSpeed = 240f;

        Rigidbody body;
        Vector3 origin;

        void Awake()
        {
            body = GetComponent<Rigidbody>();
            origin = transform.position;
        }

        void FixedUpdate()
        {
            float offset = Mathf.Sin((Time.time + phase) * speed) * distance;
            Vector3 target = origin + direction.normalized * offset;

            if (body != null && body.isKinematic)
            {
                body.MovePosition(target);
                if (spinSpeed != 0f)
                    body.MoveRotation(transform.rotation * Quaternion.AngleAxis(spinSpeed * Time.fixedDeltaTime, spinAxis));
            }
            else
            {
                transform.position = target;
                if (spinSpeed != 0f)
                    transform.Rotate(spinAxis, spinSpeed * Time.fixedDeltaTime, Space.Self);
            }
        }
    }
}
