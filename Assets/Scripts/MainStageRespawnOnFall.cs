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

        private Rigidbody2D body;
        private RopeResource ropeResource;
        private RopeController ropeController;
        private WeaveResource weaveResource;
        private Vector2 checkpointPosition;
        private float checkpointRopeLength;
        private int checkpointSelectedRopeLength;
        private int checkpointWeaveThreads;

        public bool HasReachedMidpoint { get; private set; }

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            ropeResource = GetComponent<RopeResource>();
            ropeController = GetComponent<RopeController>();
            weaveResource = GetComponent<WeaveResource>();
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

        private void CaptureCheckpointState()
        {
            checkpointRopeLength = ropeResource.CurrentLength;
            checkpointSelectedRopeLength = ropeController.SelectedRopeLength;
            checkpointWeaveThreads = weaveResource.CurrentThreads;
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
            HasReachedMidpoint = true;
        }

        private void RestoreCheckpointState()
        {
            ropeResource.RestoreCurrentLength(checkpointRopeLength);
            ropeController.RestoreSelectedRopeLength(checkpointSelectedRopeLength);
            weaveResource.RestoreThreads(checkpointWeaveThreads);
        }
    }
}
