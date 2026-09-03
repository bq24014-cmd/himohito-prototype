using UnityEngine;
using UnityEngine.SceneManagement;

namespace HimoHito
{
    /// <summary>
    /// Connects the graybox into one complete run: play, clear or fail, then restart.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(RopeResource), typeof(RopeController))]
    [RequireComponent(typeof(RopePlatformBuilder))]
    public sealed class PrototypeRunController : MonoBehaviour
    {
        public enum RunOutcome
        {
            WaitingToStart,
            Playing,
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
        [SerializeField, Min(0f)] private float fallRespawnDelay = 0.5f;
        [SerializeField, Min(0.01f)] private float minimumUsableRopeLength = 1f;

        private Rigidbody2D body;
        private RopeResource ropeResource;
        private RopeController ropeController;
        private RopePlatformBuilder platformBuilder;
        private PlayerMover playerMover;
        private Vector2 startPosition;
        private float startRopeCapacity;
        private float startRopeLength;
        private Vector2 checkpointPosition;
        private float checkpointRopeLength;
        private int checkpointSelectedRopeLength;
        private RopePlatformBuilder.PlatformState[] checkpointPlatformStates;
        private float automaticRespawnTimer;
        private bool startRequested;

        public RunOutcome Outcome { get; private set; } = RunOutcome.WaitingToStart;
        public RunFailureReason FailureReason { get; private set; } = RunFailureReason.None;
        public bool IsAutomaticRespawnPending { get; private set; }
        public int CurrentTutorialSection { get; private set; } = 1;
        public const int TutorialSectionCount = 5;
        public string CurrentTutorialObjective => CurrentTutorialSection switch
        {
            1 => "長さと向きを選び、ヒモを掛けて振る",
            2 => "長さを選び、中央のトゲを越える",
            3 => "地形同士に掛けたヒモを足場にする",
            4 => "作った足場からHookへ掛けて渡る",
            5 => "中央のHookを外し、1本の足場でゴールする",
            _ => string.Empty
        };

        private void Awake()
        {
            TutorialSectionOneSetup.ApplyCurrentScene();
            TutorialSectionTwoSetup.ApplyCurrentScene();
            TutorialSectionThreeSetup.ApplyCurrentScene();
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
            body.position = TutorialSectionOneSetup.StartRespawnPosition;
            Physics2D.SyncTransforms();
            ropeController.RestoreSelectedRopeLength(
                TutorialSectionOneSetup.StartingRopeLength);
            startPosition = body.position;
            startRopeCapacity = ropeResource.MaximumLength;
            startRopeLength = ropeResource.CurrentLength;

        }

        private void Start()
        {
            EnterStartScreen();
            checkpointPosition = startPosition;
            CaptureCheckpointState();
        }

        private void Update()
        {
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

            if (IsAutomaticRespawnPending)
            {
                automaticRespawnTimer -= Time.unscaledDeltaTime;
                if (automaticRespawnTimer <= 0f)
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
                IsAutomaticRespawnPending = true;
                automaticRespawnTimer = CurrentTutorialSection == 1
                    ? 0.15f
                    : fallRespawnDelay;
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
            startRequested = false;
            FailureReason = RunFailureReason.None;
            Outcome = RunOutcome.WaitingToStart;
        }

        private void UpdateStartScreen()
        {
            if (!startRequested && WasKeyboardKeyPressed())
            {
                startRequested = true;
            }

            if (startRequested && !Input.anyKey)
            {
                body.simulated = true;
                playerMover.enabled = true;
                ropeController.enabled = true;
                Outcome = RunOutcome.Playing;
            }
        }

        private static bool WasKeyboardKeyPressed()
        {
            if (!Input.anyKeyDown)
            {
                return false;
            }

            for (int button = 0; button <= 6; button++)
            {
                if (Input.GetMouseButtonDown(button))
                {
                    return false;
                }
            }

            return true;
        }

        public void MarkClear()
        {
            bool tutorialStepsComplete = gameObject.scene.name != "Tutorial" ||
                                         CurrentTutorialSection >= TutorialSectionCount;
            if (Outcome == RunOutcome.Playing && tutorialStepsComplete)
            {
                Finish(RunOutcome.Clear);
            }
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
            if (sectionNumber == 3)
            {
                ropeResource.RestoreCapacityAndCurrent(
                    TutorialSectionThreeSetup.StartingRopeAmount,
                    TutorialSectionThreeSetup.StartingRopeAmount);
            }
            ropeController.RestoreSelectedRopeLength(startingRopeLength);
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
        }

        private void RestartFromCheckpoint()
        {
            body.simulated = true;
            ropeController.DetachAndRefund();
            RestoreCheckpointState();
            body.position = checkpointPosition;
            ResetMotionAndResume();
        }

        private void RestartTutorial()
        {
            body.simulated = true;
            ropeController.DetachAndRefund();
            ropeResource.RestoreCapacityAndCurrent(
                startRopeCapacity,
                startRopeLength);
            platformBuilder.ClearPlatforms();

            CurrentTutorialSection = 1;
            checkpointPosition = startPosition;
            body.position = startPosition;
            CaptureCheckpointState();
            ResetMotionAndResume();
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
            body.linearVelocity = Vector2.zero;
            body.angularVelocity = 0f;
            playerMover.enabled = true;
            ropeController.enabled = true;
            automaticRespawnTimer = 0f;
            IsAutomaticRespawnPending = false;
            FailureReason = RunFailureReason.None;
            Outcome = RunOutcome.Playing;
        }

        private static void LoadNextStage()
        {
            int nextBuildIndex = SceneManager.GetActiveScene().buildIndex + 1;
            if (nextBuildIndex >= 0 && nextBuildIndex < SceneManager.sceneCountInBuildSettings)
            {
                SceneManager.LoadScene(nextBuildIndex);
            }
        }
    }
}
