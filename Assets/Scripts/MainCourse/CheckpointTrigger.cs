using UnityEngine;

namespace MainCourse
{
    public class CheckpointTrigger : MonoBehaviour
    {
        public int index = 0;
        public Transform spawnPoint;

        void OnTriggerEnter(Collider other)
        {
            if (other.GetComponentInParent<PlayerController>() == null) return;
            if (GameManager.Instance != null)
                GameManager.Instance.RegisterCheckpoint(index, spawnPoint);
        }
    }
}