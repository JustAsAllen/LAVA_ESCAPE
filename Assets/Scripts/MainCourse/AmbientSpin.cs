using UnityEngine;

namespace MainCourse
{
    public class AmbientSpin : MonoBehaviour
    {
        public Vector3 axis = Vector3.forward;
        public float speed = 4f;

        void Update()
        {
            transform.Rotate(axis, speed * Time.deltaTime, Space.Self);
        }
    }
}