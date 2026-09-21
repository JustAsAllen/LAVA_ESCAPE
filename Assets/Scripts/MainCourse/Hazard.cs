using UnityEngine;

namespace MainCourse
{
    public class Hazard : MonoBehaviour
    {
        void OnCollisionEnter(Collision collision)
        {
            Kill(collision.collider.transform);
        }

        void OnTriggerEnter(Collider other)
        {
            Kill(other.transform);
        }

        void Kill(Transform t)
        {
            if (t.GetComponentInParent<PlayerController>() == null) return;
            if (GameManager.Instance != null)
                GameManager.Instance.PlayerHitHazard();
        }
    }

    public class KillTrigger : MonoBehaviour
    {
        void OnTriggerEnter(Collider other)
        {
            if (other.GetComponentInParent<PlayerController>() == null) return;
            if (GameManager.Instance != null)
                GameManager.Instance.PlayerHitHazard();
        }
    }
}