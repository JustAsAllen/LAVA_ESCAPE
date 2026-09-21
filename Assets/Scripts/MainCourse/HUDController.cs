using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MainCourse
{
    public class HUDController : MonoBehaviour
    {
        public Text timerText;
        public Text deathsText;
        public Text checkpointText;
        public Text gemText;
        public Text dashText;
        public Text bestText;
        public Text hintText;
        public Text victoryTitleText;
        public Text victoryTimeText;
        public Text victoryDeathsText;
        public GameObject victoryPanel;
        public GameObject pausePanel;

        public static bool IsPaused { get; private set; }

        GameManager gm;
        PlayerController player;
        bool subscribed;

        const int GemTotal = 13;

        void Start()
        {
            gm = GameManager.Instance;
            if (gm != null)
            {
                gm.OnDeath += HandleDeath;
                gm.OnCheckpointChanged += HandleCheckpoint;
                gm.OnGemCollected += HandleGem;
                gm.OnFinish += HandleFinish;
                subscribed = true;
            }
        }

        void OnDestroy()
        {
            if (subscribed && gm != null)
            {
                gm.OnDeath -= HandleDeath;
                gm.OnCheckpointChanged -= HandleCheckpoint;
                gm.OnGemCollected -= HandleGem;
                gm.OnFinish -= HandleFinish;
            }
        }

        void Update()
        {
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
                TogglePause();

            if (IsPaused)
            {
                if (Keyboard.current != null && Keyboard.current.qKey.wasPressedThisFrame)
                {
                    Time.timeScale = 1f;
                    Application.Quit();
                }
                return;
            }

            if (gm == null)
                gm = GameManager.Instance;
            if (gm == null) return;

            if (player == null)
                player = gm.Player != null ? gm.Player : FindFirstObjectByType<PlayerController>();

            if (timerText != null)
                timerText.text = GameManager.Format(gm.CurrentTime);

            if (bestText != null)
                bestText.text = "BEST  " + (gm.BestTime > 0f ? GameManager.Format(gm.BestTime) : "--:--.----");

            if (dashText != null)
            {
                if (player == null)
                    dashText.text = "DASH  READY";
                else if (player.Dashing)
                    dashText.text = "DASH  !!  SLOW-MO";
                else if (player.Sliding)
                    dashText.text = "SLIDING  !!";
                else if (player.DashReady)
                    dashText.text = "DASH  READY";
                else
                    dashText.text = "DASH  COOLING";
            }

            HandleRestart();
        }

        void TogglePause()
        {
            if (IsPaused) SetPaused(false);
            else SetPaused(true);
        }

        void SetPaused(bool paused)
        {
            IsPaused = paused;
            Time.timeScale = paused ? 0f : 1f;

            if (pausePanel != null)
                pausePanel.SetActive(paused);

            Cursor.lockState = paused ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = paused;
        }

        void HandleRestart()
        {
            if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
            {
                if (IsPaused) SetPaused(false);
                int idx = SceneManager.GetActiveScene().buildIndex;
                SceneManager.LoadScene(idx >= 0 ? idx : 0);
            }
        }

        void HandleDeath()
        {
            if (deathsText != null)
                deathsText.text = "" + gm.Deaths;
        }

        void HandleCheckpoint()
        {
            if (checkpointText != null)
                checkpointText.text = "CP  " + gm.CurrentCheckpointIndex + " / " + gm.CheckpointTotal;
        }

        void HandleGem()
        {
            if (gemText != null)
                gemText.text = gm.Gems + " / " + GemTotal;
        }

        void HandleFinish()
        {
            if (victoryPanel != null) victoryPanel.SetActive(true);
            if (victoryTitleText != null)
                victoryTitleText.text = "COURSE CLEARED";
            if (victoryTimeText != null)
                victoryTimeText.text = "TIME  " + GameManager.Format(gm.CurrentTime);
            if (victoryDeathsText != null)
                victoryDeathsText.text = "DEATHS  " + gm.Deaths +
                    "\nGEMS  " + gm.Gems + " / " + GemTotal +
                    "\nBEST  " + (gm.BestTime > 0f ? GameManager.Format(gm.BestTime) : "--:--.----");
            if (timerText != null)
                timerText.text = GameManager.Format(gm.CurrentTime);
        }
    }
}