using UnityEngine;

namespace MainCourse
{
    [RequireComponent(typeof(Collider))]
    public class Pickup : MonoBehaviour
    {
        public float spinSpeed = 130f;

        void Update()
        {
            transform.Rotate(0f, spinSpeed * Time.deltaTime, 0f, Space.World);
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.GetComponentInParent<PlayerController>() == null) return;
            if (GameManager.Instance != null)
                GameManager.Instance.CollectGem(transform.position);
            Destroy(gameObject);
        }
    }
}