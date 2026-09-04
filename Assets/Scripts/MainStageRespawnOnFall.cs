using UnityEngine;
using UnityEngine.Serialization;

namespace HimoHito
{
    /// <summary>
    /// Restarts the current main-stage section. Each checkpoint records the
    /// rope platforms that existed before it and guarantees the documented
    /// lower bound for the sections ahead.
    /// </summary>
    [DefaultExecutionOrder(-200)]
    [RequireComponent(typeof(Rigidbody2D), typeof(RopeResource), typeof(RopeController))]
    [RequireComponent(typeof(RopePlatformBuilder), typeof(PlayerMover))]
    public sealed class MainStageRespawnOnFall : MonoBehaviour
    {
        // Temporary development switch. Set this to false when full-stage
        // playtesting should begin from section one again.
        [SerializeField]
        [FormerlySerializedAs("startFromSectionSevenForDevelopment")]
        [FormerlySerializedAs("startFromSectionEightForDevelopment")]
        [FormerlySerializedAs("startFromSectionNineForDevelopment")]
        private bool startFromSectionTenForDevelopment;
        private const int SectionOneStartingRopeLength = 7;
        private const float SectionTenStartingRopeLength = 15f;

        [SerializeField] private float fallThreshold = -9f;
        [SerializeField, Min(0.01f)] private float minimumUsableRopeLength = 1f;

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

        public int CurrentSection { get; private set; } = 1;
        public int RefillCount { get; private set; }
        public bool HasReachedMidpoint => CurrentSection >= 6;
        public bool HasReachedSectionNine => CurrentSection >= 9;
        public bool HasReachedSectionTen => CurrentSection >= 10;
        public bool IsRopeExhausted { get; private set; }

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            ropeResource = GetComponent<RopeResource>();
            ropeController = GetComponent<RopeController>();
            platformBuilder = GetComponent<RopePlatformBuilder>();
            playerMover = GetComponent<PlayerMover>();
            goalZone = FindFirstObjectByType<MainStageGoalZone>();

            if (startFromSectionTenForDevelopment)
            {
                CurrentSection = 10;
                body.position =
                    MainStageSectionNineSetup.GoalRespawnPosition;
                ropeResource.RestoreCurrentLength(
                    SectionTenStartingRopeLength);
                ropeController.RestoreSelectedRopeLength(
                    MainStageSectionTenSetup.BridgeRopeLength);
            }
            else
            {
                CurrentSection = 1;
                ropeResource.ResetToMaximum();
                ropeController.RestoreSelectedRopeLength(
                    SectionOneStartingRopeLength);
            }

            checkpointPosition = body.position;
            CaptureCheckpointState();
        }

        private void Start()
        {
            goalZone ??= FindFirstObjectByType<MainStageGoalZone>();
            if (!startFromSectionTenForDevelopment)
            {
                return;
            }

            // Apply once more after every Awake has completed. This prevents
            // scene setup components from leaving development Play at an older
            // section checkpoint.
            CurrentSection = 10;
            body.position =
                MainStageSectionNineSetup.GoalRespawnPosition;
            body.linearVelocity = Vector2.zero;
            body.angularVelocity = 0f;
            ropeResource.RestoreCurrentLength(
                SectionTenStartingRopeLength);
            ropeController.RestoreSelectedRopeLength(
                MainStageSectionTenSetup.BridgeRopeLength);
            checkpointPosition = body.position;
            CaptureCheckpointState();
        }

        private void Update()
        {
            goalZone ??= FindFirstObjectByType<MainStageGoalZone>();
            if (goalZone != null && goalZone.IsClear)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.R) || transform.position.y < fallThreshold)
            {
                RestartFromCheckpoint();
                return;
            }

            if (!ropeController.IsAttached &&
                ropeResource.CurrentLength < minimumUsableRopeLength)
            {
                IsRopeExhausted = true;
            }
        }

        public bool TryReachSection(
            int sectionNumber,
            Vector2 respawnPosition,
            float minimumRopeAfterCheckpoint)
        {
            if (ropeController.IsAttached || sectionNumber <= CurrentSection)
            {
                return false;
            }

            CurrentSection = Mathf.Clamp(sectionNumber, 1, 10);
            checkpointPosition = respawnPosition;
            if (ropeResource.CurrentLength < minimumRopeAfterCheckpoint)
            {
                ropeResource.RestoreCurrentLength(minimumRopeAfterCheckpoint);
                RefillCount++;
            }
            if (CurrentSection == 7)
            {
                MainStageSectionSevenSetup.PrepareEntryPlatform(
                    platformBuilder,
                    ropeResource.CurrentLength);
            }
            CaptureCheckpointState();
            IsRopeExhausted = false;
            return true;
        }

        private void CaptureCheckpointState()
        {
            checkpointRopeLength = ropeResource.CurrentLength;
            checkpointSelectedRopeLength = ropeController.SelectedRopeLength;
            checkpointPlatformStates = platformBuilder.CapturePlatformStates();
        }

        private void RestartFromCheckpoint()
        {
            body.simulated = true;
            ropeController.DetachAndRefund();
            ropeResource.RestoreCurrentLength(checkpointRopeLength);
            ropeController.RestoreSelectedRopeLength(checkpointSelectedRopeLength);
            platformBuilder.RestorePlatformStates(checkpointPlatformStates);
            body.position = checkpointPosition;
            body.linearVelocity = Vector2.zero;
            body.angularVelocity = 0f;
            playerMover.enabled = true;
            ropeController.enabled = true;
            IsRopeExhausted = false;
        }
    }
}
