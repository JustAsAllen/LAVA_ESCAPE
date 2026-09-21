using Climbing;
using UnityEngine;

namespace ParkourCastle
{
    public class KillZone : MonoBehaviour
    {
        public bool active = true;

        void OnTriggerEnter(Collider other)
        {
            if (!active) return;
            if (other.GetComponentInParent<ThirdPersonController>() == null) return;
            if (GameManager.Instance != null) GameManager.Instance.PlayerDied();
        }
    }
}
