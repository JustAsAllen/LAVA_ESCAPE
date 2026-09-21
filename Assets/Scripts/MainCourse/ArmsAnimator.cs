using UnityEngine;

namespace MainCourse
{
    public class ArmsAnimator : MonoBehaviour
    {
        PlayerController pc;
        public Transform armL;
        public Transform armR;

        float phase;

        void Awake()
        {
            pc = GetComponentInParent<PlayerController>();
            if (armL == null) armL = transform.Find("Arm_L");
            if (armR == null) armR = transform.Find("Arm_R");
        }

        void Update()
        {
            if (pc == null || armL == null || armR == null) return;

            bool grounded = pc.IsGrounded;
            bool moving = pc.IsMoving;
            float sp = pc.Speed;
            bool sprinting = sp > 8.5f;

            if (grounded && moving)
                phase += Time.deltaTime * (8.5f + sp * 1.15f);
            else
                phase += Time.deltaTime * 1.2f;

            float swing = Mathf.Sin(phase) * (0.24f + sp * 0.018f);
            float roll = Mathf.Cos(phase) * (0.16f + (sprinting ? 0.1f : 0f) + sp * 0.012f) * 0.5f;

            Quaternion tl, tr;
            if (!grounded)
            {
                float raise = pc.WallRunning ? -0.55f : -0.95f;
                float splay = pc.WallRunning ? 0.5f : 0.24f;
                tl = Quaternion.Euler(raise, 16f + splay * 24f, -6f);
                tr = Quaternion.Euler(raise, -16f - splay * 24f, 6f);
            }
            else
            {
                tl = Quaternion.Euler(-swing * 55f, 14f, -6f - roll * 70f);
                tr = Quaternion.Euler(-swing * 55f, -14f, 6f + roll * 70f);
            }

            armL.localRotation = Quaternion.Slerp(armL.localRotation, tl, 1f - Mathf.Exp(-14f * Time.deltaTime));
            armR.localRotation = Quaternion.Slerp(armR.localRotation, tr, 1f - Mathf.Exp(-14f * Time.deltaTime));
        }
    }
}