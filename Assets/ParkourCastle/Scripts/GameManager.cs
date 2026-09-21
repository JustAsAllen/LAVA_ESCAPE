using System.Collections.Generic;
using Climbing;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ParkourCastle
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;

        public Transform startPoint;
        public Text timerText;
        public Text bestText;
        public Text checkpointText;
        public Text messageText;
        public float fallLimitY = -6f;
        public float deathPenalty = 5f;

        ThirdPersonController controller;
        Rigidbody body;
        Animator animator;

        readonly List<Checkpoint> checkpoints = new List<Checkpoint>();
        Checkpoint current;

        float elapsed;
        float penalty;
        float best;
        float messageUntil;
        bool running;
        bool finished;

        void Awake()
        {
            Instance = this;
            best = PlayerPrefs.GetFloat("TakeshiBest", 0f);
        }

        void Start()
        {
            controller = FindFirstObjectByType<ThirdPersonController>();
            if (controller != null)
            {
                body = controller.GetComponent<Rigidbody>();
                animator = controller.GetComponent<Animator>();
            }

            if (controller != null && startPoint != null)
                Teleport(startPoint);

            UpdateBestText();
            UpdateCheckpointText();
            Message("GET READY", 2.5f);
        }

        void Update()
        {
            var kb = Keyboard.current;
            if (kb != null && kb.rKey.wasPressedThisFrame)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                return;
            }

            if (!running && !finished && AnyInput())
                BeginRun();

            if (running && !finished)
            {
                elapsed += Time.deltaTime;
                if (controller != null && controller.transform.position.y < fallLimitY)
                    PlayerDied();
            }

            if (timerText != null)
                timerText.text = Format(elapsed + penalty);

            if (messageText != null && messageText.enabled && Time.time > messageUntil)
                messageText.enabled = false;
        }

        bool AnyInput()
        {
            if (controller == null) return false;
            var input = controller.characterInput;
            if (input == null) return false;
            return input.movement.sqrMagnitude > 0.02f || input.jump || input.run || input.drop;
        }

        void BeginRun()
        {
            running = true;
            Message("GO!", 1.2f);
        }

        public void RegisterCheckpoint(Checkpoint checkpoint)
        {
            if (checkpoint == null || finished) return;
            if (!checkpoints.Contains(checkpoint))
                checkpoints.Add(checkpoint);
            if (current == null || checkpoint.index > current.index)
            {
                current = checkpoint;
                Message("CHECKPOINT " + checkpoint.index, 1.5f);
                UpdateCheckpointText();
            }
        }

        public void PlayerDied()
        {
            if (finished) return;
            penalty += deathPenalty;
            Message("SPLASH  +" + deathPenalty.ToString("0") + "s", 1.8f);
            Teleport(current != null && current.spawn != null ? current.spawn : startPoint);
        }

        public void Finish()
        {
            if (finished) return;
            finished = true;
            running = false;

            float total = elapsed + penalty;
            if (best <= 0f || total < best)
            {
                best = total;
                PlayerPrefs.SetFloat("TakeshiBest", best);
                PlayerPrefs.Save();
            }

            UpdateBestText();
            Message("CLEARED  " + Format(total) + "\nPRESS  R  TO  RUN  AGAIN", 20f);
        }

        public void Teleport(Transform target)
        {
            if (controller == null || target == null) return;

            controller.DisableController();

            if (body != null)
            {
                body.linearVelocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
            }

            controller.transform.SetPositionAndRotation(target.position, target.rotation);
            controller.transform.localScale = Vector3.one;
            controller.isVaulting = false;
            controller.isJumping = false;
            controller.onAir = false;
            controller.ResetMovement();

            if (animator != null)
            {
                animator.Rebind();
                animator.SetBool("Jump", false);
                animator.SetBool("onAir", false);
                animator.SetBool("Land", false);
                animator.SetBool("Hanging", false);
                animator.SetBool("Run", false);
                animator.SetBool("Released", true);
                animator.SetBool("Crouch", false);
                animator.SetFloat("Velocity", 0f);
                animator.SetFloat("Horizontal", 0f);
                animator.SetInteger("Climb State", 0);
            }

            controller.EnableController();
        }

        void UpdateBestText()
        {
            if (bestText != null)
                bestText.text = best > 0f ? "BEST  " + Format(best) : "BEST  --:--";
        }

        void UpdateCheckpointText()
        {
            if (checkpointText != null)
                checkpointText.text = "CP  " + (current != null ? current.index.ToString() : "-");
        }

        public void Message(string text, float duration)
        {
            if (messageText == null) return;
            messageText.text = text;
            messageText.enabled = true;
            messageUntil = Time.time + duration;
        }

        public static string Format(float seconds)
        {
            if (seconds < 0f) seconds = 0f;
            int minutes = Mathf.FloorToInt(seconds / 60f);
            float rest = seconds - minutes * 60f;
            return minutes.ToString("00") + ":" + rest.ToString("00.00");
        }
    }
}
