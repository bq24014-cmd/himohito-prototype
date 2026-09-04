using UnityEngine;
using UnityEngine.SceneManagement;

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
        private const int SectionOneStartingRopeLength = 7;
        private const float EndingPreviewRopeLength = 5f;

        [SerializeField]
        private bool startNearGoalForEndingPreview = true;

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

            CurrentSection = 1;
            ropeResource.ResetToMaximum();
            ropeController.RestoreSelectedRopeLength(
                SectionOneStartingRopeLength);

            if (startNearGoalForEndingPreview)
            {
                ApplyEndingPreviewStart();
            }

            checkpointPosition = body.position;
            CaptureCheckpointState();
        }

        private void Start()
        {
            if (!startNearGoalForEndingPreview)
            {
                return;
            }

            // Reapply after every Awake so authored section setup cannot leave
            // the player at an older checkpoint during the ending preview.
            ApplyEndingPreviewStart();
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

            if (IsRopeExhausted)
            {
                if (Input.GetKeyDown(KeyCode.R))
                {
                    RestartFromCheckpoint();
                }
                else if (Input.GetKeyDown(KeyCode.Escape))
                {
                    SceneManager.LoadScene("Tutorial");
                }
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
                EnterRopeExhaustedState();
            }
        }

        private void EnterRopeExhaustedState()
        {
            body.linearVelocity = Vector2.zero;
            body.angularVelocity = 0f;
            body.simulated = false;
            playerMover.enabled = false;
            ropeController.enabled = false;
            IsRopeExhausted = true;
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

        private void ApplyEndingPreviewStart()
        {
            CurrentSection = 10;
            body.position = MainStageSectionTenSetup.GoalMarkerPosition +
                new Vector2(-2f, 2f);
            body.linearVelocity = Vector2.zero;
            body.angularVelocity = 0f;
            ropeResource.RestoreCurrentLength(EndingPreviewRopeLength);
            ropeController.RestoreSelectedRopeLength(1);
            IsRopeExhausted = false;
        }

        private void RestartFromCheckpoint()
        {
            body.simulated = true;
            ropeController.DetachAndRefund();
            ropeResource.RestoreCurrentLength(checkpointRopeLength);
            ropeController.RestoreSelectedRopeLength(checkpointSelectedRopeLength);
            platformBuilder.RestorePlatformStates(checkpointPlatformStates);
            MainStageSectionFiveSetup.RestoreRailShelvesAfterRestart();
            body.position = checkpointPosition;
            body.linearVelocity = Vector2.zero;
            body.angularVelocity = 0f;
            playerMover.enabled = true;
            ropeController.enabled = true;
            IsRopeExhausted = false;
        }
    }
}
