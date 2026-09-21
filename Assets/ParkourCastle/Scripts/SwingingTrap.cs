using UnityEngine;

namespace ParkourCastle
{
    public class SwingingTrap : MonoBehaviour
    {
        public Vector3 axis = Vector3.forward;
        public float maxAngle = 55f;
        public float speed = 1.5f;
        public float phase = 0f;

        Rigidbody body;
        Quaternion rest;

        void Awake()
        {
            body = GetComponent<Rigidbody>();
            rest = transform.rotation;
        }

        void FixedUpdate()
        {
            float angle = Mathf.Sin((Time.time + phase) * speed) * maxAngle;
            Quaternion target = rest * Quaternion.AngleAxis(angle, axis);

            if (body != null && body.isKinematic) body.MoveRotation(target);
            else transform.rotation = target;
        }
    }
}
