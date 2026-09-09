using UnityEngine;

namespace HimoHito
{
    /// <summary>
    /// Follows horizontal position at a fixed zoom, without speed-based framing.
    /// Reads physics only; the authored camera height is preserved outside section ten.
    /// </summary>
    [RequireComponent(typeof(Camera), typeof(AudioListener))]
    public sealed class HorizontalCameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField, Min(0f)] private float smoothTime = 0.18f;
        [SerializeField] private float horizontalOffset = 0f;
        [SerializeField, Min(0f)] private float verticalSmoothTime = 0.28f;
        [SerializeField] private bool followVerticalInSectionTen = true;
        private float horizontalVelocity;
        private float verticalVelocity;
        private float fixedY;
        private float fixedZ;
        private MainStageRespawnOnFall respawnController;
        private Camera gameplayCamera;
        private Rigidbody2D targetBody;
        private PlayerMover targetMover;
        private Transform cachedTarget;
        private float baseOrthographicSize;
        private Vector2 previousTargetPosition;
        private bool skipFollowAfterReset;

        private void Awake()
        {
            if (!TryGetComponent(out AudioListener _))
            {
                gameObject.AddComponent<AudioListener>();
            }

            fixedY = transform.position.y;
            fixedZ = transform.position.z;
            gameplayCamera = GetComponent<Camera>();
            baseOrthographicSize = gameplayCamera.orthographicSize;
            ResetFraming();
            if (Application.isPlaying && !TryGetComponent(out AmbientDustVisual _))
                gameObject.AddComponent<AmbientDustVisual>();
        }

        // All player Awake methods have now placed their starting position.
        private void Start() => ResetFraming();

        // The main-stage overview temporarily owns the camera while disabled.
        private void OnEnable()
        {
            if (gameplayCamera != null) ResetFraming();
        }

        public void ResetFraming()
        {
            if (gameplayCamera == null) return;
            FindTargetIfNeeded();
            horizontalVelocity = verticalVelocity = 0f;
            gameplayCamera.orthographicSize = baseOrthographicSize;
            if (target == null) return;
            Vector2 position = targetBody != null ? targetBody.position : (Vector2)target.position;
            previousTargetPosition = position;
            skipFollowAfterReset = true;
            transform.position = new Vector3(position.x + horizontalOffset,
                ShouldFollowVertically() ? position.y : fixedY, fixedZ);
        }

        private void LateUpdate()
        {
            UpdateFraming(Time.deltaTime);
        }

        private void UpdateFraming(float deltaTime)
        {
            FindTargetIfNeeded();
            // Title, failure, clear and preview disable simulation/control.
            // Tab, signs and pause stop scaled time: do not let framing drift.
            if (target == null || deltaTime <= 0f ||
                (targetBody != null && !targetBody.simulated) ||
                (targetMover != null && !targetMover.enabled))
            {
                return;
            }

            // Rigidbody interpolation may still expose the pre-restart
            // Transform for this frame. Keep the reset's exact body position.
            if (skipFollowAfterReset)
            {
                skipFollowAfterReset = false;
                return;
            }

            Vector2 velocity = targetBody != null ? targetBody.linearVelocity : Vector2.zero;
            Vector2 position = targetBody != null ? targetBody.position : (Vector2)target.position;
            float teleportDistance = Mathf.Max(3f, velocity.magnitude * deltaTime * 3f + 0.5f);
            if (Vector2.Distance(position, previousTargetPosition) > teleportDistance)
            {
                ResetFraming();
                skipFollowAfterReset = false;
                return;
            }
            previousTargetPosition = position;
            float desiredX = target.position.x + horizontalOffset;
            float nextX = smoothTime <= 0f
                ? desiredX
                : Mathf.SmoothDamp(
                    transform.position.x,
                    desiredX,
                    ref horizontalVelocity,
                    smoothTime, Mathf.Infinity, deltaTime);
            float desiredY = ShouldFollowVertically() ? target.position.y : fixedY;
            float nextY = verticalSmoothTime <= 0f
                ? desiredY
                : Mathf.SmoothDamp(
                    transform.position.y,
                    desiredY,
                    ref verticalVelocity,
                    verticalSmoothTime, Mathf.Infinity, deltaTime);

            transform.position = new Vector3(nextX, nextY, fixedZ);
            if (gameplayCamera.orthographic)
                gameplayCamera.orthographicSize = baseOrthographicSize;
        }

        private void FindTargetIfNeeded()
        {
            if (target == null)
            {
                RopeResource playerRope = FindFirstObjectByType<RopeResource>();
                if (playerRope != null) target = playerRope.transform;
            }
            if (target == null || target == cachedTarget) return;
            cachedTarget = target;
            targetBody = target.GetComponent<Rigidbody2D>();
            targetMover = target.GetComponent<PlayerMover>();
            respawnController = target.GetComponent<MainStageRespawnOnFall>();
        }

        private bool ShouldFollowVertically()
        {
            return followVerticalInSectionTen &&
                respawnController != null &&
                respawnController.HasReachedSectionTen;
        }
    }
}
