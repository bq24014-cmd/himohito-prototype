using UnityEngine;

namespace HimoHito
{
    /// <summary>
    /// Detaches an active rope when the player enters this hazard's trigger.
    /// Normal refund and momentum preservation remain owned by RopeController.
    /// </summary>
    public sealed class RopeReleaseHazard : MonoBehaviour
    {
        [SerializeField] private HookPoint affectedHook;

        private BoxCollider2D detectionArea;
        private RopeController ropeController;
        private Collider2D playerCollider;

        public HookPoint AffectedHook => affectedHook;

        public void Configure(HookPoint hook)
        {
            affectedHook = hook;
        }

        private void Awake()
        {
            CacheReferences();
        }

        private void FixedUpdate()
        {
            CacheReferences();
            if (detectionArea == null ||
                playerCollider == null ||
                ropeController == null ||
                !ShouldDetach(ropeController))
            {
                return;
            }

            // Attached motion can miss a one-frame trigger notification.
            // Checking the actual collider overlap keeps the hazard reliable.
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

        private void TryDetach(Collider2D other)
        {
            RopeController contactedRope = other.GetComponentInParent<RopeController>();
            if (!ShouldDetach(contactedRope))
            {
                return;
            }

            contactedRope.DetachAndRefund();
        }

        private bool ShouldDetach(RopeController contactedRope)
        {
            return contactedRope != null &&
                   contactedRope.IsAttached &&
                   affectedHook != null &&
                   contactedRope.ActiveHookPoint == affectedHook;
        }

        private void CacheReferences()
        {
            if (detectionArea == null)
            {
                foreach (BoxCollider2D collider in GetComponents<BoxCollider2D>())
                {
                    if (collider.isTrigger)
                    {
                        detectionArea = collider;
                        break;
                    }
                }
            }

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
