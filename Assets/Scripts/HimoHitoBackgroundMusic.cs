using UnityEngine;
using UnityEngine.SceneManagement;

namespace HimoHito
{
    /// <summary>One persistent BGM source, independent of gameplay time and SE.</summary>
    public sealed class HimoHitoBackgroundMusic : MonoBehaviour
    {
        private const string MusicPath = "Audio/YasashiiOdori";
        private static HimoHitoBackgroundMusic instance;
        private AudioSource source;
        private PrototypeRunController tutorial;
        private MainStageRespawnOnFall mainRun;
        private MainStageGoalZone goal;
        private bool supportedScene;
        private bool warnedMissing;
        private float nextReferenceCheck;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            instance = null;
            SceneManager.sceneLoaded -= SceneLoaded;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Subscribe()
        {
            SceneManager.sceneLoaded -= SceneLoaded;
            SceneManager.sceneLoaded += SceneLoaded;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void StartInitialScene()
        {
            SceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
        }

        private static bool IsGameScene(Scene scene)
        {
            return scene.name == "Tutorial" || scene.name == "MainStage";
        }

        private static void SceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (mode == LoadSceneMode.Additive) return;
            if (instance == null)
                instance = FindFirstObjectByType<HimoHitoBackgroundMusic>();
            if (instance == null && IsGameScene(scene))
                instance = new GameObject("HimoHito Background Music")
                    .AddComponent<HimoHitoBackgroundMusic>();
            if (instance != null) instance.RefreshScene(scene);
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudio();
        }

        private void InitializeAudio()
        {
            source = GetComponent<AudioSource>();
            if (source == null) source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = true;
            source.spatialBlend = 0f;
            source.volume = 0f;
            source.priority = 128;
            source.ignoreListenerPause = true;
            source.clip = Resources.Load<AudioClip>(MusicPath);
            if (source.clip == null && !warnedMissing)
            {
                warnedMissing = true;
                Debug.LogWarning("BGM missing: download GANO's 優しい踊り from the official page " +
                    "and place it at Assets/Resources/Audio/YasashiiOdori.mp3 (see README).", this);
            }
        }

        private void RefreshScene(Scene scene)
        {
            if (source == null) InitializeAudio();
            supportedScene = IsGameScene(scene);
            tutorial = FindFirstObjectByType<PrototypeRunController>();
            mainRun = FindFirstObjectByType<MainStageRespawnOnFall>();
            goal = FindFirstObjectByType<MainStageGoalZone>();
            nextReferenceCheck = Time.unscaledTime + 0.5f;
            if (!supportedScene) source.Stop();
            else if (source.clip != null && !source.isPlaying) source.Play();
        }

        private void Update()
        {
            if (source == null || !supportedScene) return;
            // Some stage objects are created after sceneLoaded callbacks.
            if (Time.unscaledTime >= nextReferenceCheck)
            {
                if (tutorial == null) tutorial = FindFirstObjectByType<PrototypeRunController>();
                if (mainRun == null) mainRun = FindFirstObjectByType<MainStageRespawnOnFall>();
                if (goal == null) goal = FindFirstObjectByType<MainStageGoalZone>();
                nextReferenceCheck = Time.unscaledTime + 0.5f;
            }
            float target = 0.35f;
            if (tutorial != null)
            {
                switch (tutorial.Outcome)
                {
                    case PrototypeRunController.RunOutcome.WaitingToStart: target = 0.25f; break;
                    case PrototypeRunController.RunOutcome.Failed: target = 0.10f; break;
                    case PrototypeRunController.RunOutcome.Clearing:
                    case PrototypeRunController.RunOutcome.Clear: target = 0.07f; break;
                }
            }
            if (mainRun != null && mainRun.IsFailureVisible) target = 0.10f;
            if (goal != null && goal.IsCompleting) target = 0.07f;
            bool onTitle = tutorial != null &&
                tutorial.Outcome == PrototypeRunController.RunOutcome.WaitingToStart;
            if (!onTitle && Time.timeScale < 0.01f) target = Mathf.Min(target, 0.15f);
            source.volume = Mathf.MoveTowards(source.volume, target, Time.unscaledDeltaTime * 0.7f);
        }

        private void OnDestroy()
        {
            if (instance == this) instance = null;
        }
    }
}
