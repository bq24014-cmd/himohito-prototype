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
        private Vector3 backgroundStartPosition;
        private float cameraStartX;
        private bool isInitialized;

        private void Start()
        {
            TryInitialize();
        }

        private void LateUpdate()
        {
            if (!isInitialized && !TryInitialize())
            {
                return;
            }

            float cameraTravel = cameraTransform.position.x - cameraStartX;
            transform.position = new Vector3(
                backgroundStartPosition.x + cameraTravel * horizontalFollow,
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
            backgroundStartPosition = transform.position;
            cameraStartX = cameraTransform.position.x;
            isInitialized = true;
            return true;
        }
    }
}
