using Climbing;
using UnityEngine;

namespace ParkourCastle
{
    public class FinishLine : MonoBehaviour
    {
        void OnTriggerEnter(Collider other)
        {
            if (other.GetComponentInParent<ThirdPersonController>() == null) return;
            if (GameManager.Instance != null) GameManager.Instance.Finish();
        }
    }
}
