using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace MainCourse
{
    public class SpeedFX : MonoBehaviour
    {
        public Volume volume;

        ChromaticAberration ca;
        LensDistortion ld;
        Vignette vg;
        Bloom bl;

        PlayerController player;
        float deathPunch;
        float warpGlow;

        void Awake()
        {
            if (volume == null)
            {
                var go = GameObject.Find("Global Volume");
                if (go != null) volume = go.GetComponent<Volume>();
            }
            if (volume != null && volume.profile != null)
            {
                volume.profile.TryGet<ChromaticAberration>(out ca);
                volume.profile.TryGet<LensDistortion>(out ld);
                volume.profile.TryGet<Vignette>(out vg);
                volume.profile.TryGet<Bloom>(out bl);
            }
        }

        void Start()
        {
            player = FindFirstObjectByType<PlayerController>();
            if (GameManager.Instance != null)
                GameManager.Instance.OnDeath += HandleDeath;
        }

        void OnDestroy()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.OnDeath -= HandleDeath;
        }

        void HandleDeath()
        {
            deathPunch = 1f;
        }

        void Update()
        {
            if (player == null)
                player = FindFirstObjectByType<PlayerController>();

            deathPunch = Mathf.MoveTowards(deathPunch, 0f, Time.deltaTime * 1.8f);

            float sp = player != null ? player.Speed : 0f;
            float normSpeed = Mathf.Clamp01((sp - 6f) / 18f);
            float dashing = player != null && player.Dashing ? 1f : 0f;
            float sliding = player != null && player.Sliding ? 1f : 0f;
            float airborne = player != null && !player.IsGrounded ? 1f : 0f;

            // Overdrive ramp: sprint speed, dash i-frames, and slides all pour in.
            float overdrive = Mathf.Clamp01(
                dashing * 1.0f +
                sliding * 0.7f +
                deathPunch * 1.2f +
                Mathf.Max(0f, normSpeed - 0.22f) * 1.4f);
            warpGlow = Mathf.Lerp(warpGlow, overdrive, 1f - Mathf.Exp(-9f * Time.deltaTime));

            // High-frequency wobble only bites while overdriven.
            float wob = Mathf.Sin(Time.unscaledTime * 38f) * warpGlow;

            float targetCA = 0.22f + warpGlow * 0.95f + wob * 0.22f;
            float targetLD = wob * 0.10f + (dashing > 0f ? 0.08f : 0f) + (sliding > 0f ? 0.045f : 0f) - airborne * 0.05f;
            float targetVig = 0.24f + deathPunch * 0.45f + warpGlow * 0.30f;
            float targetBloom = 0.55f + dashing * 2.8f + sliding * 1.5f + deathPunch * 1.3f + normSpeed * 0.4f;

            float lerp = 1f - Mathf.Exp(-7f * Time.deltaTime);

            if (ca != null) ca.intensity.Override(Mathf.Lerp(ca.intensity.value, targetCA, lerp));
            if (ld != null) ld.intensity.Override(Mathf.Lerp(ld.intensity.value, targetLD, lerp));
            if (vg != null) vg.intensity.Override(Mathf.Lerp(vg.intensity.value, targetVig, lerp));
            if (bl != null) bl.intensity.Override(Mathf.Lerp(bl.intensity.value, targetBloom, lerp));
        }
    }
}