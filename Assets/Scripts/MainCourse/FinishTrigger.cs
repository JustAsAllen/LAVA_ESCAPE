using UnityEngine;

namespace MainCourse
{
    public class FinishTrigger : MonoBehaviour
    {
        void OnTriggerEnter(Collider other)
        {
            if (other.GetComponentInParent<PlayerController>() == null) return;
            if (GameManager.Instance != null)
                GameManager.Instance.Finish();
        }
    }
}