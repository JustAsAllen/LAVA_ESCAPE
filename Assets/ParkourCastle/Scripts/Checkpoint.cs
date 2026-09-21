using Climbing;
using UnityEngine;

namespace ParkourCastle
{
    public class Checkpoint : MonoBehaviour
    {
        public int index = 1;
        public Transform spawn;

        void OnTriggerEnter(Collider other)
        {
            if (other.GetComponentInParent<ThirdPersonController>() == null) return;
            if (GameManager.Instance != null) GameManager.Instance.RegisterCheckpoint(this);
        }
    }
}
