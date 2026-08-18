using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HimoHito
{
    /// <summary>
    /// Connects the graybox into one complete run: play, clear or fail, then restart.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(RopeResource), typeof(RopeController))]
    [RequireComponent(typeof(WeaveResource))]
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
        private WeaveResource weaveResource;
        private PlayerMover playerMover;
        private Vector2 startPosition;
        private Vector2 checkpointPosition;
        private float checkpointRopeLength;
        private int checkpointSelectedRopeLength;
        private int checkpointWeaveThreads;
        private readonly List<CheckpointWeaveState> checkpointWeaveStates = new();
        private float automaticRespawnTimer;
        private bool startRequested;

        public RunOutcome Outcome { get; private set; } = RunOutcome.WaitingToStart;
        public RunFailureReason FailureReason { get; private set; } = RunFailureReason.None;
        public bool IsAutomaticRespawnPending { get; private set; }
        public int CurrentTutorialSection { get; private set; } = 1;
        public const int TutorialSectionCount = 4;
        public string CurrentTutorialObjective => CurrentTutorialSection switch
        {
            1 => "Hookにヒモを掛ける",
            2 => "障害物を避けて着地する",
            3 => "消費したヒモから足場を編む",
            4 => "編んだ足場からゴールする",
            _ => string.Empty
        };

        private readonly struct CheckpointWeaveState
        {
            public CheckpointWeaveState(WeaveFrame frame, bool isCompleted)
            {
                Frame = frame;
                IsCompleted = isCompleted;
            }

            public WeaveFrame Frame { get; }
            public bool IsCompleted { get; }
        }

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            ropeResource = GetComponent<RopeResource>();
            ropeController = GetComponent<RopeController>();
            weaveResource = GetComponent<WeaveResource>();
            playerMover = GetComponent<PlayerMover>();
            startPosition = body.position;
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
                automaticRespawnTimer = fallRespawnDelay;
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
            if (Outcome == RunOutcome.Playing)
            {
                Finish(RunOutcome.Clear);
            }
        }

        public void TryReachTutorialSection(int sectionNumber, Vector2 respawnPosition)
        {
            if (Outcome != RunOutcome.Playing ||
                ropeController.IsAttached ||
                sectionNumber <= CurrentTutorialSection ||
                sectionNumber > TutorialSectionCount)
            {
                return;
            }

            CurrentTutorialSection = sectionNumber;
            checkpointPosition = respawnPosition;
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
            ropeResource.ResetToMaximum();
            weaveResource.ResetThreads();
            foreach (WeaveFrame weaveFrame in
                     FindObjectsByType<WeaveFrame>(FindObjectsSortMode.None))
            {
                weaveFrame.ResetWeave();
            }

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
            checkpointWeaveThreads = weaveResource.CurrentThreads;
            checkpointWeaveStates.Clear();
            foreach (WeaveFrame weaveFrame in
                     FindObjectsByType<WeaveFrame>(FindObjectsSortMode.None))
            {
                checkpointWeaveStates.Add(
                    new CheckpointWeaveState(weaveFrame, weaveFrame.IsCompleted));
            }
        }

        private void RestoreCheckpointState()
        {
            ropeResource.RestoreCurrentLength(checkpointRopeLength);
            ropeController.RestoreSelectedRopeLength(checkpointSelectedRopeLength);
            weaveResource.RestoreThreads(checkpointWeaveThreads);
            foreach (CheckpointWeaveState state in checkpointWeaveStates)
            {
                if (state.Frame != null)
                {
                    state.Frame.RestoreWeave(state.IsCompleted);
                }
            }
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
