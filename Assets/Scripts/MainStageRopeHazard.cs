using UnityEngine;

namespace HimoHito
{
    /// <summary>
    /// Releases an attached rope when the player crosses a main-stage obstacle.
    /// The trigger is not a floor, so the detached player continues falling.
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public sealed class MainStageRopeHazard : MonoBehaviour
    {
        private BoxCollider2D detectionArea;
        private RopeController ropeController;
        private Collider2D playerCollider;

        private void Awake()
        {
            detectionArea = GetComponent<BoxCollider2D>();
            detectionArea.isTrigger = true;
            CachePlayer();
        }

        private void FixedUpdate()
        {
            CachePlayer();
            if (detectionArea == null ||
                ropeController == null ||
                playerCollider == null ||
                !ropeController.IsAttached)
            {
                return;
            }

            if (detectionArea.bounds.Intersects(playerCollider.bounds))
            {
                ropeController.DetachAndRefund();
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            TryDetach(other);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            TryDetach(other);
        }

        private static void TryDetach(Collider2D other)
        {
            RopeController contactedRope = other.GetComponentInParent<RopeController>();
            if (contactedRope != null && contactedRope.IsAttached)
            {
                contactedRope.DetachAndRefund();
            }
        }

        private void CachePlayer()
        {
            if (ropeController == null)
            {
                ropeController = FindFirstObjectByType<RopeController>();
            }

            if (ropeController != null && playerCollider == null)
            {
                playerCollider = ropeController.GetComponent<Collider2D>();
            }
        }
    }
}
