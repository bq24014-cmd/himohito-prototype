using UnityEngine;

namespace HimoHito
{
    /// <summary>
    /// Follows the player's horizontal movement while keeping camera height and zoom fixed.
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

        private void Awake()
        {
            if (!TryGetComponent(out AudioListener _))
            {
                gameObject.AddComponent<AudioListener>();
            }

            fixedY = transform.position.y;
            fixedZ = transform.position.z;
            FindTargetIfNeeded();

            if (target != null)
            {
                respawnController = target.GetComponent<MainStageRespawnOnFall>();
                transform.position = new Vector3(
                    target.position.x + horizontalOffset,
                    ShouldFollowVertically() ? target.position.y : fixedY,
                    fixedZ);
            }
        }

        private void LateUpdate()
        {
            FindTargetIfNeeded();
            if (target == null)
            {
                return;
            }

            float desiredX = target.position.x + horizontalOffset;
            float nextX = smoothTime <= 0f
                ? desiredX
                : Mathf.SmoothDamp(
                    transform.position.x,
                    desiredX,
                    ref horizontalVelocity,
                    smoothTime);
            float desiredY = ShouldFollowVertically() ? target.position.y : fixedY;
            float nextY = verticalSmoothTime <= 0f
                ? desiredY
                : Mathf.SmoothDamp(
                    transform.position.y,
                    desiredY,
                    ref verticalVelocity,
                    verticalSmoothTime);

            transform.position = new Vector3(nextX, nextY, fixedZ);
        }

        private void FindTargetIfNeeded()
        {
            if (target != null)
            {
                return;
            }

            RopeResource playerRope = FindFirstObjectByType<RopeResource>();
            if (playerRope != null)
            {
                target = playerRope.transform;
                respawnController = target.GetComponent<MainStageRespawnOnFall>();
            }
        }

        private bool ShouldFollowVertically()
        {
            return followVerticalInSectionTen &&
                respawnController != null &&
                respawnController.HasReachedSectionTen;
        }
    }
}
