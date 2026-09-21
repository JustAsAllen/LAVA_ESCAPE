using UnityEngine;

namespace ParkourCastle
{
    public class SpinBar : MonoBehaviour
    {
        public Vector3 axis = Vector3.up;
        public float speed = 100f;

        Rigidbody body;

        void Awake()
        {
            body = GetComponent<Rigidbody>();
        }

        void FixedUpdate()
        {
            Quaternion target = transform.rotation * Quaternion.AngleAxis(speed * Time.fixedDeltaTime, axis);

            if (body != null && body.isKinematic) body.MoveRotation(target);
            else transform.rotation = target;
        }
    }
}
