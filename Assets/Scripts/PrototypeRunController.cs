using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace HimoHito
{
    /// <summary>
    /// Connects the graybox into one complete run: play, clear or fail, then restart.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(RopeResource), typeof(RopeController))]
    [RequireComponent(typeof(RopePlatformBuilder))]
    public sealed class PrototypeRunController : MonoBehaviour
    {
        public const float TutorialRopeCapacity = 99f;
        private const float ClearRevealDelay = 1.35f;

        public enum RunOutcome
        {
            WaitingToStart,
            Playing,
            Clearing,
            Clear,
            Failed
        }

        public enum RunFailureReason
        {
            None,
            Fell,
            RopeExhausted
        }

        [SerializeField] private float fallThreshold = -9f;
        [SerializeField, Min(0.01f)] private float minimumUsableRopeLength = 1f;
        [FormerlySerializedAs("startFromSectionThreeForDevelopment")]
        [SerializeField] private bool startFromSectionFourForDevelopment;

        private Rigidbody2D body;
        private RopeResource ropeResource;
        private RopeController ropeController;
        private RopePlatformBuilder platformBuilder;
        private PlayerMover playerMover;
        private StageOverlayControls overlayControls;
        private TutorialSectionGuide tutorialSectionGuide;
        private Vector2 startPosition;
        private float startRopeCapacity;
        private float startRopeLength;
        private int startSelectedRopeLength;
        private int startTutorialSection = 1;
        private Vector2 checkpointPosition;
        private float checkpointRopeLength;
        private int checkpointSelectedRopeLength;
        private RopePlatformBuilder.PlatformState[] checkpointPlatformStates;

        public RunOutcome Outcome { get; private set; } = RunOutcome.WaitingToStart;
        public RunFailureReason FailureReason { get; private set; } = RunFailureReason.None;
        public bool IsFallUnravelling => FallUnravelVisual.IsPlayingFor(gameObject);
        public int CurrentTutorialSection { get; private set; } = 1;
        public const int TutorialSectionCount = 4;
        public string CurrentTutorialObjective => CurrentTutorialSection switch
        {
            1 => "長さと向きを選び、ヒモを掛けて振る",
            2 => "長さを選び、中央のトゲを越える",
            3 => "地形同士に掛けたヒモを足場にする",
            4 => "中央のHookを外し、1本の足場でゴールする",
            _ => string.Empty
        };

        private void Awake()
        {
            TutorialSectionOneSetup.ApplyCurrentScene();
            TutorialSectionTwoSetup.ApplyCurrentScene();
            TutorialSectionThreeSetup.ApplyCurrentScene();
            TutorialSectionFourSetup.ApplyCurrentScene();
            TutorialFirstSectionVisuals.Apply(gameObject);

            body = GetComponent<Rigidbody2D>();
            ropeResource = GetComponent<RopeResource>();
            ropeController = GetComponent<RopeController>();
            platformBuilder = GetComponent<RopePlatformBuilder>();
            if (platformBuilder == null)
            {
                platformBuilder = gameObject.AddComponent<RopePlatformBuilder>();
            }
            playerMover = GetComponent<PlayerMover>();
            overlayControls = FindFirstObjectByType<StageOverlayControls>();
            if (overlayControls == null)
            {
                overlayControls = gameObject.AddComponent<StageOverlayControls>();
            }
            tutorialSectionGuide = GetComponent<TutorialSectionGuide>();
            if (tutorialSectionGuide == null)
            {
                tutorialSectionGuide =
                    gameObject.AddComponent<TutorialSectionGuide>();
            }
            ropeResource.RestoreCapacityAndCurrent(
                TutorialRopeCapacity,
                TutorialRopeCapacity);
            if (startFromSectionFourForDevelopment)
            {
                startTutorialSection = 4;
                CurrentTutorialSection = 4;
                body.position =
                    TutorialSectionThreeSetup.LandingRespawnPosition;
                ropeController.RestoreSelectedRopeLength(
                    TutorialSectionFourSetup.PlatformRopeLength);
            }
            else
            {
                body.position = TutorialSectionOneSetup.StartRespawnPosition;
                ropeController.RestoreSelectedRopeLength(
                    TutorialSectionOneSetup.StartingRopeLength);
            }
            Physics2D.SyncTransforms();
            startPosition = body.position;
            startRopeCapacity = ropeResource.MaximumLength;
            startRopeLength = ropeResource.CurrentLength;
            startSelectedRopeLength = ropeController.SelectedRopeLength;

        }

        private void Start()
        {
            EnterStartScreen();
            checkpointPosition = startPosition;
            CaptureCheckpointState();
        }

        private void Update()
        {
            if (MainStagePreview.IsActive) return;
            if (IsFallUnravelling) return;

            if (Outcome == RunOutcome.WaitingToStart)
            {
                UpdateStartScreen();
                return;
            }

            if (Outcome == RunOutcome.Clear &&
                (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
            {
                LoadNextStage();
                return;
            }

            if (Outcome == RunOutcome.Clearing)
            {
                return;
            }

            if (Outcome == RunOutcome.Failed &&
                Input.GetKeyDown(KeyCode.Escape))
            {
                SceneManager.LoadScene("Tutorial");
                return;
            }

            if (tutorialSectionGuide != null &&
                tutorialSectionGuide.IsVisible)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                if (Outcome == RunOutcome.Clear)
                {
                    RestartTutorial();
                }
                else
                {
                    RestartFromCheckpoint();
                }
                return;
            }

            if (Outcome != RunOutcome.Playing)
            {
                return;
            }

            bool fell = body.position.y < fallThreshold;
            bool cannotUseRope = !ropeController.IsAttached &&
                                 ropeResource.CurrentLength < minimumUsableRopeLength;
            if (fell)
            {
                Finish(RunOutcome.Failed, RunFailureReason.Fell);
                return;
            }

            if (cannotUseRope)
            {
                Finish(RunOutcome.Failed, RunFailureReason.RopeExhausted);
            }
        }

        private void EnterStartScreen()
        {
            ropeController.DetachAndRefund();
            body.linearVelocity = Vector2.zero;
            body.angularVelocity = 0f;
            body.simulated = false;
            playerMover.enabled = false;
            ropeController.enabled = false;
            FailureReason = RunFailureReason.None;
            Outcome = RunOutcome.WaitingToStart;
        }

        private void UpdateStartScreen()
        {
            if (overlayControls != null && overlayControls.IsHelpVisible)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.Return) ||
                Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                body.simulated = true;
                playerMover.enabled = true;
                ropeController.enabled = true;
                Outcome = RunOutcome.Playing;
                MainStagePreview.PlayFor(gameObject);
                return;
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                QuitApplication();
            }
        }

        private static void QuitApplication()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        public void MarkClear()
        {
            bool tutorialStepsComplete = gameObject.scene.name != "Tutorial" ||
                                         CurrentTutorialSection >= TutorialSectionCount;
            if (Outcome == RunOutcome.Playing && tutorialStepsComplete)
            {
                BeginClearSequence();
            }
        }

        private void BeginClearSequence()
        {
            ropeController.DetachAndRefund();
            body.linearVelocity = Vector2.zero;
            body.angularVelocity = 0f;
            body.simulated = false;
            FailureReason = RunFailureReason.None;
            Outcome = RunOutcome.Clearing;

            GetComponent<RopeBodyVisual>()?.PlayGoalPose(ClearRevealDelay,
                GameObject.Find(TutorialSectionFourSetup.GoalMarkerName)?.transform);

            StartCoroutine(CompleteClearSequence());
        }

        private IEnumerator CompleteClearSequence()
        {
            yield return new WaitForSecondsRealtime(ClearRevealDelay);
            GetComponent<PrototypeAudioFeedback>()?.PlayClearRevealed();
            Outcome = RunOutcome.Clear;
        }

        public void TryReachTutorialSection(
            int sectionNumber,
            Vector2 respawnPosition,
            int startingRopeLength)
        {
            bool canReachCheckpointWhileAttached =
                CurrentTutorialSection == 1 && sectionNumber == 2;
            if (Outcome != RunOutcome.Playing ||
                (ropeController.IsAttached && !canReachCheckpointWhileAttached) ||
                sectionNumber <= CurrentTutorialSection ||
                sectionNumber > TutorialSectionCount)
            {
                return;
            }

            CurrentTutorialSection = sectionNumber;
            checkpointPosition = respawnPosition;
            // Keep the player's selection across sections instead of applying the lesson preset.
            CaptureCheckpointState();
        }

        private void Finish(
            RunOutcome outcome,
            RunFailureReason failureReason = RunFailureReason.None)
        {
            ropeController.DetachAndRefund();
            body.linearVelocity = Vector2.zero;
            body.angularVelocity = 0f;
            body.simulated = false;
            FailureReason = failureReason;
            Outcome = outcome;
            if (failureReason == RunFailureReason.Fell)
            {
                playerMover.enabled = false;
                ropeController.enabled = false;
                FallUnravelVisual.Play(gameObject);
            }
            if (failureReason == RunFailureReason.RopeExhausted)
            {
                GetComponent<PrototypeAudioFeedback>()?.PlayRopeExhausted();
            }
        }

        private void RestartFromCheckpoint()
        {
            body.simulated = true;
            ropeController.DetachAndRefund();
            RestoreCheckpointState();
            body.position = checkpointPosition;
            ResetMotionAndResume();
            RespawnWeaveVisual.Play(gameObject);
        }

        private void RestartTutorial()
        {
            GetComponent<SectionArrivalFeedback>()?.ClearTrail();
            body.simulated = true;
            ropeController.DetachAndRefund();
            ropeResource.RestoreCapacityAndCurrent(
                startRopeCapacity,
                startRopeLength);
            platformBuilder.ClearPlatforms();

            CurrentTutorialSection = startTutorialSection;
            checkpointPosition = startPosition;
            body.position = startPosition;
            ropeController.RestoreSelectedRopeLength(
                startSelectedRopeLength);
            CaptureCheckpointState();
            ResetMotionAndResume();
            MainStagePreview.PlayFor(gameObject);
        }

        private void CaptureCheckpointState()
        {
            checkpointRopeLength = ropeResource.CurrentLength;
            checkpointSelectedRopeLength = ropeController.SelectedRopeLength;
            checkpointPlatformStates = platformBuilder.CapturePlatformStates();
        }

        private void RestoreCheckpointState()
        {
            ropeResource.RestoreCurrentLength(checkpointRopeLength);
            ropeController.RestoreSelectedRopeLength(checkpointSelectedRopeLength);
            platformBuilder.RestorePlatformStates(checkpointPlatformStates);
        }

        private void ResetMotionAndResume()
        {
            GetComponent<PrototypeAudioFeedback>()?.StopRopeExhaustedAudio();
            body.linearVelocity = Vector2.zero;
            body.angularVelocity = 0f;
            playerMover.enabled = true;
            ropeController.enabled = true;
            FailureReason = RunFailureReason.None;
            Outcome = RunOutcome.Playing;
            Camera.main?.GetComponent<HorizontalCameraFollow>()?.ResetFraming();
        }

        private static void LoadNextStage()
        {
            const string MainStageSceneName = "MainStage";
            if (Application.CanStreamedLevelBeLoaded(MainStageSceneName))
            {
                SceneManager.LoadScene(MainStageSceneName);
                return;
            }

            Debug.LogError(
                $"本編シーンを読み込めません。Build Settingsに" +
                $"{MainStageSceneName}が登録されているか確認してください。");
        }
    }
}
