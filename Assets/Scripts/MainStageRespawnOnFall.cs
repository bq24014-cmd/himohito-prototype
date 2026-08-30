using UnityEngine;

namespace HimoHito
{
    /// <summary>
    /// Restarts from the latest main-stage checkpoint and restores its resources.
    /// </summary>
    [DefaultExecutionOrder(-200)]
    [RequireComponent(typeof(Rigidbody2D), typeof(RopeResource), typeof(RopeController))]
    [RequireComponent(typeof(RopePlatformBuilder), typeof(PlayerMover))]
    public sealed class MainStageRespawnOnFall : MonoBehaviour
    {
        [SerializeField] private float fallThreshold = -9f;
        [SerializeField, Min(0.01f)] private float minimumUsableRopeLength = 1f;
        [SerializeField] private bool startFromCurrentSectionForDevelopment = true;
        [SerializeField] private Vector2 developmentStartPosition =
            new Vector2(144.5f, 0.15f);
        [SerializeField, Min(1f)] private float developmentRopeLength = 50f;
        [SerializeField, Min(1)] private int developmentSectionNumber = 8;

        private Rigidbody2D body;
        private RopeResource ropeResource;
        private RopeController ropeController;
        private RopePlatformBuilder platformBuilder;
        private PlayerMover playerMover;
        private MainStageGoalZone goalZone;
        private Vector2 checkpointPosition;
        private float checkpointRopeLength;
        private int checkpointSelectedRopeLength;
        private RopePlatformBuilder.PlatformState[] checkpointPlatformStates;

        public bool HasReachedMidpoint { get; private set; }
        public bool HasReachedSectionEight { get; private set; }
        public bool HasReachedSectionNine { get; private set; }
        public bool HasReachedSectionTen { get; private set; }
        public bool IsRopeExhausted { get; private set; }

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            ropeResource = GetComponent<RopeResource>();
            ropeController = GetComponent<RopeController>();
            platformBuilder = GetComponent<RopePlatformBuilder>();
            if (platformBuilder == null)
            {
                platformBuilder = gameObject.AddComponent<RopePlatformBuilder>();
            }
            playerMover = GetComponent<PlayerMover>();
            goalZone = FindFirstObjectByType<MainStageGoalZone>();
            checkpointPosition = body.position;

            ApplyDevelopmentStart();

            CaptureCheckpointState();
        }

        private void Start()
        {
            if (!ShouldApplyDevelopmentStart)
            {
                return;
            }

            // Apply once more after every Awake so scene initialization cannot
            // move the player back before the preview camera begins.
            ApplyDevelopmentStart();
            CaptureCheckpointState();
        }

        private void Update()
        {
            if (goalZone != null && goalZone.IsClear)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                RestartFromCheckpoint();
                return;
            }

            if (IsRopeExhausted)
            {
                return;
            }

            if (transform.position.y < fallThreshold)
            {
                RestartFromCheckpoint();
                return;
            }

            if (!ropeController.IsAttached &&
                ropeResource.CurrentLength < minimumUsableRopeLength)
            {
                EnterRopeExhaustedState();
            }
        }

        public bool TryReachMidpoint(Vector2 respawnPosition)
        {
            if (ropeController.IsAttached)
            {
                return false;
            }

            if (HasReachedMidpoint)
            {
                return true;
            }

            checkpointPosition = respawnPosition;
            CaptureCheckpointState();
            HasReachedMidpoint = true;
            return true;
        }

        public bool TryStartSectionEight(Vector2 respawnPosition)
        {
            if (ropeController.IsAttached)
            {
                return false;
            }

            if (HasReachedSectionEight)
            {
                return true;
            }

            checkpointPosition = respawnPosition;
            HasReachedSectionEight = true;
            CaptureCheckpointState();
            return true;
        }

        public bool TryStartSectionNine(Vector2 respawnPosition)
        {
            if (ropeController.IsAttached)
            {
                return false;
            }

            if (HasReachedSectionNine)
            {
                return true;
            }

            checkpointPosition = respawnPosition;
            HasReachedSectionNine = true;
            CaptureCheckpointState();
            return true;
        }

        public bool TryStartSectionTen(Vector2 respawnPosition)
        {
            if (ropeController.IsAttached)
            {
                return false;
            }

            if (HasReachedSectionTen)
            {
                return true;
            }

            checkpointPosition = respawnPosition;
            HasReachedSectionTen = true;
            CaptureCheckpointState();
            return true;
        }

        private void CaptureCheckpointState()
        {
            checkpointRopeLength = ropeResource.CurrentLength;
            checkpointSelectedRopeLength = ropeController.SelectedRopeLength;
            checkpointPlatformStates = platformBuilder.CapturePlatformStates();
        }

        private void ApplyDevelopmentStart()
        {
            if (!ShouldApplyDevelopmentStart)
            {
                return;
            }

            checkpointPosition = developmentStartPosition;
            transform.position = developmentStartPosition;
            body.position = developmentStartPosition;
            body.linearVelocity = Vector2.zero;
            body.angularVelocity = 0f;
            ropeResource.RestoreCurrentLength(developmentRopeLength);
            ropeController.RestoreSelectedRopeLength(
                Mathf.Min(ropeController.SelectedRopeLength, Mathf.FloorToInt(developmentRopeLength)));
            platformBuilder.ClearPlatforms();
            HasReachedMidpoint = developmentSectionNumber >= 6;
            HasReachedSectionEight = developmentSectionNumber >= 8;
            HasReachedSectionNine = developmentSectionNumber >= 8;
            HasReachedSectionTen = developmentSectionNumber >= 9;
        }

        private bool ShouldApplyDevelopmentStart =>
            startFromCurrentSectionForDevelopment;

        private void RestoreCheckpointState()
        {
            ropeResource.RestoreCurrentLength(checkpointRopeLength);
            ropeController.RestoreSelectedRopeLength(checkpointSelectedRopeLength);
            platformBuilder.RestorePlatformStates(checkpointPlatformStates);
        }

        private void EnterRopeExhaustedState()
        {
            ropeController.DetachAndRefund();
            body.linearVelocity = Vector2.zero;
            body.angularVelocity = 0f;
            body.simulated = false;
            playerMover.enabled = false;
            ropeController.enabled = false;
            IsRopeExhausted = true;
        }

        private void RestartFromCheckpoint()
        {
            body.simulated = true;
            ropeController.DetachAndRefund();
            RestoreCheckpointState();
            body.position = checkpointPosition;
            transform.position = checkpointPosition;
            body.linearVelocity = Vector2.zero;
            body.angularVelocity = 0f;
            playerMover.enabled = true;
            ropeController.enabled = true;
            IsRopeExhausted = false;
        }
    }
}
