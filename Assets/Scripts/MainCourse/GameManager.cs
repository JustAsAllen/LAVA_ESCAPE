using System;
using UnityEngine;

namespace MainCourse
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;

        public Transform startPoint;
        public CaptureFX captureFX;

        const string BestKey = "MainCourseBest";

        PlayerController player;
        float elapsed;
        float best;
        float lastDeathTime = -999f;
        bool started;
        bool finished;

        public Transform CurrentCheckpoint { get; private set; }
        public int CurrentCheckpointIndex { get; private set; }
        public int CheckpointTotal = 11;
        public int Deaths { get; private set; }
        public float CurrentTime { get { return elapsed; } }
        public bool Started { get { return started; } }
        public bool Finished { get { return finished; } }
        public float BestTime { get { return best; } }
        public int Gems { get; private set; }
        public PlayerController Player { get { return player; } }

        public event Action OnRunStarted;
        public event Action OnDeath;
        public event Action OnCheckpointChanged;
        public event Action OnGemCollected;
        public event Action OnFinish;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            best = PlayerPrefs.GetFloat(BestKey, 0f);
        }

        void Start()
        {
            player = FindFirstObjectByType<PlayerController>();
            if (player != null && startPoint != null)
                player.Teleport(startPoint);
        }

        void Update()
        {
            if (player == null)
                player = FindFirstObjectByType<PlayerController>();
            if (player == null) return;

            if (!started && !finished && player.IsMoving)
            {
                started = true;
                if (OnRunStarted != null) OnRunStarted();
            }

            if (started && !finished)
                elapsed += Time.deltaTime;
        }

        public void RegisterCheckpoint(int index, Transform respawn)
        {
            if (finished) return;
            if (index > CurrentCheckpointIndex)
            {
                CurrentCheckpointIndex = index;
                CurrentCheckpoint = respawn;
                if (captureFX != null) captureFX.Burst(respawn.position + Vector3.up * 0.4f);
                if (OnCheckpointChanged != null) OnCheckpointChanged();
            }
        }

        public void CollectGem(Vector3 pos)
        {
            if (finished) return;
            Gems++;
            if (captureFX != null) captureFX.Burst(pos);
            if (OnGemCollected != null) OnGemCollected();
        }

        public void PlayerFell()
        {
            if (finished) return;
            if (Time.time - lastDeathTime < 0.6f) return;
            lastDeathTime = Time.time;
            Deaths++;
            if (captureFX != null && player != null) captureFX.Burst(player.transform.position);
            if (OnDeath != null) OnDeath();
            TeleportToCheckpoint();
        }

        public void PlayerHitHazard()
        {
            if (finished) return;
            if (Time.time - lastDeathTime < 0.6f) return;
            lastDeathTime = Time.time;
            Deaths++;
            if (captureFX != null && player != null) captureFX.Burst(player.transform.position);
            if (OnDeath != null) OnDeath();
            TeleportToCheckpoint();
        }

        public void TeleportToCheckpoint()
        {
            if (player == null) return;
            Transform target = CurrentCheckpoint != null ? CurrentCheckpoint : startPoint;
            if (target == null) return;
            player.Teleport(target);
        }

        public void Finish()
        {
            if (finished) return;
            finished = true;
            if (best <= 0f || elapsed < best)
            {
                best = elapsed;
                PlayerPrefs.SetFloat(BestKey, best);
                PlayerPrefs.Save();
            }
            if (player != null && captureFX != null)
                captureFX.Burst(player.transform.position + Vector3.up * 1f);
            if (OnFinish != null) OnFinish();
        }

        public static string Format(float seconds)
        {
            if (seconds < 0f) seconds = 0f;
            int m = Mathf.FloorToInt(seconds / 60f);
            float r = seconds - m * 60f;
            return m.ToString("00") + ":" + r.ToString("00.00");
        }
    }
}