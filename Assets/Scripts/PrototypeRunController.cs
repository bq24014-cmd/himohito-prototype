using UnityEngine;

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

        [SerializeField] private float fallThreshold = -9f;
        [SerializeField, Min(0f)] private float fallRespawnDelay = 0.5f;
        [SerializeField, Min(0.01f)] private float minimumUsableRopeLength = 1f;

        private Rigidbody2D body;
        private RopeResource ropeResource;
        private RopeController ropeController;
        private WeaveResource weaveResource;
        private PlayerMover playerMover;
        private Vector2 startPosition;
        private float automaticRespawnTimer;
        private bool startRequested;

        public RunOutcome Outcome { get; private set; } = RunOutcome.WaitingToStart;
        public bool IsAutomaticRespawnPending { get; private set; }

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
        }

        private void Update()
        {
            if (Outcome == RunOutcome.WaitingToStart)
            {
                UpdateStartScreen();
                return;
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                Restart();
                return;
            }

            if (IsAutomaticRespawnPending)
            {
                automaticRespawnTimer -= Time.unscaledDeltaTime;
                if (automaticRespawnTimer <= 0f)
                {
                    Restart();
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
                Finish(RunOutcome.Failed);
                IsAutomaticRespawnPending = true;
                automaticRespawnTimer = fallRespawnDelay;
                return;
            }

            if (cannotUseRope)
            {
                Finish(RunOutcome.Failed);
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

        private void Finish(RunOutcome outcome)
        {
            ropeController.DetachAndRefund();
            body.linearVelocity = Vector2.zero;
            body.angularVelocity = 0f;
            body.simulated = false;
            Outcome = outcome;
        }

        private void Restart()
        {
            body.simulated = true;
            ropeController.DetachAndRefund();
            ropeResource.ResetToMaximum();
            weaveResource.ResetThreads();
            foreach (WeaveFrame weaveFrame in FindObjectsByType<WeaveFrame>(FindObjectsSortMode.None))
            {
                weaveFrame.ResetWeave();
            }
            body.position = startPosition;
            body.linearVelocity = Vector2.zero;
            body.angularVelocity = 0f;
            automaticRespawnTimer = 0f;
            IsAutomaticRespawnPending = false;
            Outcome = RunOutcome.Playing;
        }
    }
}
