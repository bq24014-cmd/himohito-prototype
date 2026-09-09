using UnityEngine;

namespace HimoHito
{
    /// <summary>
    /// Keeps the adopted tutorial background covering the camera while retaining
    /// a small amount of horizontal parallax. The source image is not stretched
    /// or repeated.
    /// </summary>
    [DefaultExecutionOrder(100)]
    [DisallowMultipleComponent]
    public sealed class TutorialBackgroundParallax : MonoBehaviour
    {
        [SerializeField, Range(0f, 1f)] private float horizontalFollow = 0.9f;

        private Transform cameraTransform;
        private Camera viewCamera;
        private SpriteRenderer backgroundRenderer;
        private Vector3 backgroundStartPosition;
        private float cameraStartX;
        private bool isInitialized;

        private void Start()
        {
            TryInitialize();
            if (TryGetComponent(out SpriteRenderer renderer))
                HangingDecorSway.Ensure(renderer);
        }

        public void Configure(float followAmount)
        {
            horizontalFollow = Mathf.Clamp01(followAmount);
            isInitialized = false;
        }

        private void LateUpdate()
        {
            if (!isInitialized && !TryInitialize())
            {
                return;
            }

            float cameraTravel = cameraTransform.position.x - cameraStartX;
            float nextX = backgroundStartPosition.x + cameraTravel * horizontalFollow;
            // The opening tour reaches the far end immediately. Keep the near artwork
            // covering that view without stretching it or changing normal play's parallax.
            if (MainStagePreview.IsActive && viewCamera != null && viewCamera.orthographic &&
                backgroundRenderer != null && backgroundRenderer.sprite != null)
            {
                Bounds bounds = backgroundRenderer.bounds;
                float margin = Mathf.Max(0f, bounds.extents.x -
                    viewCamera.orthographicSize * viewCamera.aspect - 0.05f);
                float centerOffset = bounds.center.x - transform.position.x;
                nextX = Mathf.Clamp(nextX + centerOffset,
                    cameraTransform.position.x - margin,
                    cameraTransform.position.x + margin) - centerOffset;
            }
            transform.position = new Vector3(
                nextX,
                backgroundStartPosition.y,
                backgroundStartPosition.z);
        }

        private bool TryInitialize()
        {
            Camera targetCamera = Camera.main;
            if (targetCamera == null)
            {
                targetCamera = FindFirstObjectByType<Camera>();
            }

            if (targetCamera == null)
            {
                return false;
            }

            cameraTransform = targetCamera.transform;
            viewCamera = targetCamera;
            TryGetComponent(out backgroundRenderer);
            transform.position = new Vector3(
                cameraTransform.position.x,
                transform.position.y,
                transform.position.z);
            backgroundStartPosition = transform.position;
            cameraStartX = cameraTransform.position.x;
            isInitialized = true;
            return true;
        }
    }
}
