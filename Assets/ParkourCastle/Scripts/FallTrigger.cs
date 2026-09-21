using Climbing;
using UnityEngine;

namespace ParkourCastle
{
    public class FallTrigger : MonoBehaviour
    {
        FallingPlatform platform;

        void Awake()
        {
            platform = GetComponentInParent<FallingPlatform>();
        }

        void OnTriggerEnter(Collider other)
        {
            if (platform == null) return;
            if (other.GetComponentInParent<ThirdPersonController>() == null) return;
            platform.Trigger();
        }
    }
}
