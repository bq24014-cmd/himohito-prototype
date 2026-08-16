using UnityEngine;

namespace HimoHito
{
    /// <summary>
    /// Restarts from the latest main-stage checkpoint and restores its resources.
    /// </summary>
    [DefaultExecutionOrder(-200)]
    [RequireComponent(typeof(Rigidbody2D), typeof(RopeResource), typeof(RopeController))]
    [RequireComponent(typeof(WeaveResource))]
    public sealed class MainStageRespawnOnFall : MonoBehaviour
    {
        [SerializeField] private float fallThreshold = -9f;
        [SerializeField] private bool startFromMidpointForDevelopment = true;
        [SerializeField] private Vector2 developmentStartPosition =
            new Vector2(69.7f, -1.35f);
        [SerializeField, Min(0)] private int developmentWeaveThreads = 3;
        [SerializeField] private bool developmentCanUseSectionSevenBridge = true;

        private Rigidbody2D body;
        private RopeResource ropeResource;
        private RopeController ropeController;
        private WeaveResource weaveResource;
        private Vector2 checkpointPosition;
        private float checkpointRopeLength;
        private int checkpointSelectedRopeLength;
        private int checkpointWeaveThreads;
        private WeaveFrame sectionSevenWeaveFrame;
        private bool checkpointWeaveCompleted;
        private bool checkpointCanUseSectionSevenBridge;

        public bool HasReachedMidpoint { get; private set; }
        public bool CanUseSectionSevenBridge { get; private set; }

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            ropeResource = GetComponent<RopeResource>();
            ropeController = GetComponent<RopeController>();
            weaveResource = GetComponent<WeaveResource>();
            sectionSevenWeaveFrame = FindFirstObjectByType<WeaveFrame>();
            checkpointPosition = body.position;

            ApplyDevelopmentStart();

            CaptureCheckpointState();
        }

        private void Start()
        {
            if (!startFromMidpointForDevelopment)
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
            if (transform.position.y >= fallThreshold)
            {
                return;
            }

            ropeController.DetachAndRefund();
            RestoreCheckpointState();
            body.position = checkpointPosition;
            body.linearVelocity = Vector2.zero;
            body.angularVelocity = 0f;
        }

        public bool TryReachMidpoint(
            Vector2 respawnPosition,
            int weaveThreads,
            bool grantsSectionSevenBridge)
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
            weaveResource.RestoreThreads(weaveThreads);
            CanUseSectionSevenBridge = grantsSectionSevenBridge;
            CaptureCheckpointState();
            HasReachedMidpoint = true;
            return true;
        }

        private void CaptureCheckpointState()
        {
            checkpointRopeLength = ropeResource.CurrentLength;
            checkpointSelectedRopeLength = ropeController.SelectedRopeLength;
            checkpointWeaveThreads = weaveResource.CurrentThreads;
            if (sectionSevenWeaveFrame != null)
            {
                checkpointWeaveCompleted = sectionSevenWeaveFrame.IsCompleted;
            }
            checkpointCanUseSectionSevenBridge = CanUseSectionSevenBridge;
        }

        private void ApplyDevelopmentStart()
        {
            if (!startFromMidpointForDevelopment)
            {
                return;
            }

            checkpointPosition = developmentStartPosition;
            transform.position = developmentStartPosition;
            body.position = developmentStartPosition;
            body.linearVelocity = Vector2.zero;
            body.angularVelocity = 0f;
            weaveResource.RestoreThreads(developmentWeaveThreads);
            CanUseSectionSevenBridge = developmentCanUseSectionSevenBridge;
            HasReachedMidpoint = true;
        }

        private void RestoreCheckpointState()
        {
            ropeResource.RestoreCurrentLength(checkpointRopeLength);
            ropeController.RestoreSelectedRopeLength(checkpointSelectedRopeLength);
            weaveResource.RestoreThreads(checkpointWeaveThreads);
            if (sectionSevenWeaveFrame != null)
            {
                sectionSevenWeaveFrame.RestoreWeave(checkpointWeaveCompleted);
            }
            CanUseSectionSevenBridge = checkpointCanUseSectionSevenBridge;
        }
    }
}
