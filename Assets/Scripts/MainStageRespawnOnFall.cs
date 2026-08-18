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
        [SerializeField] private bool startFromCurrentSectionForDevelopment;
        [SerializeField] private Vector2 developmentStartPosition =
            new Vector2(155.1f, 3.3f);
        [SerializeField, Min(1f)] private float developmentRopeLength = 28f;
        [SerializeField, Min(1)] private int developmentSectionNumber = 10;
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
        public bool HasReachedSectionEight { get; private set; }
        public bool HasReachedSectionNine { get; private set; }
        public bool HasReachedSectionTen { get; private set; }
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
            if (!startFromCurrentSectionForDevelopment)
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
            checkpointWeaveThreads = weaveResource.CurrentThreads;
            if (sectionSevenWeaveFrame != null)
            {
                checkpointWeaveCompleted = sectionSevenWeaveFrame.IsCompleted;
            }
            checkpointCanUseSectionSevenBridge = CanUseSectionSevenBridge;
        }

        private void ApplyDevelopmentStart()
        {
            if (!startFromCurrentSectionForDevelopment)
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
            weaveResource.RestoreThreads(developmentWeaveThreads);
            CanUseSectionSevenBridge = developmentCanUseSectionSevenBridge;
            HasReachedMidpoint = developmentSectionNumber >= 6;
            HasReachedSectionEight = developmentSectionNumber >= 8;
            HasReachedSectionNine = developmentSectionNumber >= 9;
            HasReachedSectionTen = developmentSectionNumber >= 10;
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
