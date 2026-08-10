using UnityEngine;

namespace HimoHito
{
    /// <summary>
    /// Follows the player's horizontal movement while keeping camera height and zoom fixed.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public sealed class HorizontalCameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField, Min(0f)] private float smoothTime = 0.18f;
        [SerializeField] private float horizontalOffset;

        private float horizontalVelocity;
        private float fixedY;
        private float fixedZ;

        private void Awake()
        {
            fixedY = transform.position.y;
            fixedZ = transform.position.z;
            FindTargetIfNeeded();

            if (target != null)
            {
                transform.position = new Vector3(
                    target.position.x + horizontalOffset,
                    fixedY,
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

            transform.position = new Vector3(nextX, fixedY, fixedZ);
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
            }
        }
    }
}
