using UnityEngine;

namespace HimoHito
{
    /// <summary>
    /// Connects the graybox into one complete run: play, clear or fail, then restart.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(RopeResource), typeof(RopeController))]
    public sealed class PrototypeRunController : MonoBehaviour
    {
        public enum RunOutcome
        {
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
        private Vector2 startPosition;
        private float automaticRespawnTimer;

        public RunOutcome Outcome { get; private set; } = RunOutcome.Playing;
        public bool IsAutomaticRespawnPending { get; private set; }

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            ropeResource = GetComponent<RopeResource>();
            ropeController = GetComponent<RopeController>();
            startPosition = body.position;
        }

        private void Update()
        {
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
            body.position = startPosition;
            body.linearVelocity = Vector2.zero;
            body.angularVelocity = 0f;
            automaticRespawnTimer = 0f;
            IsAutomaticRespawnPending = false;
            Outcome = RunOutcome.Playing;
        }
    }
}
