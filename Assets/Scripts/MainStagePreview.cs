using System.Collections;
using UnityEngine;

namespace HimoHito
{
    /// <summary>
    /// Shows the currently built part of the main stage, then returns to the player.
    /// This component belongs only to MainStage and does not change tutorial flow.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    [RequireComponent(typeof(Camera))]
    public sealed class MainStagePreview : MonoBehaviour
    {
        [SerializeField] private Transform player;
        [SerializeField] private Transform previewTarget;
        [SerializeField, Min(0f)] private float holdDuration = 0.7f;
        [SerializeField, Min(0.01f)] private float returnDuration = 1.4f;
        [SerializeField, Min(0.01f)] private float returnSpeed = 38f;

        private HorizontalCameraFollow cameraFollow;
        private Rigidbody2D playerBody;
        private PlayerMover playerMover;
        private RopeController ropeController;
        private float fixedY;
        private float fixedZ;

        public bool IsPreviewing { get; private set; }

        public void Configure(Transform playerTransform, Transform targetTransform)
        {
            player = playerTransform;
            previewTarget = targetTransform;
        }

        private void Awake()
        {
            cameraFollow = GetComponent<HorizontalCameraFollow>();
            fixedY = transform.position.y;
            fixedZ = transform.position.z;

            if (player == null)
            {
                RopeResource resource = FindFirstObjectByType<RopeResource>();
                player = resource != null ? resource.transform : null;
            }

            if (player != null)
            {
                playerBody = player.GetComponent<Rigidbody2D>();
                playerMover = player.GetComponent<PlayerMover>();
                ropeController = player.GetComponent<RopeController>();
                MainStageRespawnOnFall respawnController =
                    player.GetComponent<MainStageRespawnOnFall>();
                if (respawnController != null && respawnController.HasReachedSectionTen)
                {
                    fixedY = player.position.y;
                }
            }
        }

        private IEnumerator Start()
        {
            if (player == null || previewTarget == null)
            {
                yield break;
            }

            IsPreviewing = true;
            if (cameraFollow != null)
            {
                cameraFollow.enabled = false;
            }

            if (playerBody != null)
            {
                playerBody.linearVelocity = Vector2.zero;
                playerBody.angularVelocity = 0f;
                playerBody.simulated = false;
            }

            if (playerMover != null)
            {
                playerMover.enabled = false;
            }

            if (ropeController != null)
            {
                ropeController.enabled = false;
            }

            Vector2 previewPosition = previewTarget.position;
            Vector2 returnPosition = new Vector2(player.position.x, fixedY);
            float distance = Vector2.Distance(previewPosition, returnPosition);
            float actualReturnDuration = Mathf.Max(returnDuration, distance / returnSpeed);
            SetCameraPosition(previewPosition);
            yield return new WaitForSecondsRealtime(holdDuration);

            float elapsed = 0f;
            while (elapsed < actualReturnDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float progress = Mathf.Clamp01(elapsed / actualReturnDuration);
                float easedProgress = Mathf.SmoothStep(0f, 1f, progress);
                SetCameraPosition(Vector2.Lerp(previewPosition, returnPosition, easedProgress));
                yield return null;
            }

            SetCameraPosition(returnPosition);
            if (playerBody != null)
            {
                playerBody.simulated = true;
            }

            if (playerMover != null)
            {
                playerMover.enabled = true;
            }

            if (ropeController != null)
            {
                ropeController.enabled = true;
            }

            if (cameraFollow != null)
            {
                cameraFollow.enabled = true;
            }

            IsPreviewing = false;
        }

        private void SetCameraPosition(Vector2 position)
        {
            transform.position = new Vector3(position.x, position.y, fixedZ);
        }
    }
}
