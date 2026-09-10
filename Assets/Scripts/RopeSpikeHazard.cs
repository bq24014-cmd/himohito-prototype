using UnityEngine;

namespace HimoHito
{
    /// <summary>Spike contact cuts an active rope and lets the player fall.</summary>
    [RequireComponent(typeof(Collider2D))]
    public sealed class RopeSpikeHazard : MonoBehaviour
    {
        private Collider2D detectionArea;
        private RopeController playerRope;
        private Collider2D playerCollider;

        private void Awake()
        {
            CacheReferences();
        }

        private void FixedUpdate()
        {
            CacheReferences();
            if (detectionArea == null ||
                !detectionArea.enabled ||
                playerCollider == null ||
                !playerCollider.enabled ||
                playerRope == null ||
                !playerRope.IsAttached)
            {
                return;
            }

            // A fast pendulum can cross a small trigger between callbacks.
            // Confirm the physical overlap every physics step as a fallback.
            if (Physics2D.Distance(detectionArea, playerCollider).isOverlapped)
            {
                Cut(playerCollider);
            }
        }

        private void OnTriggerEnter2D(Collider2D other) => Cut(other);
        private void OnTriggerStay2D(Collider2D other) => Cut(other);

        private void Cut(Collider2D other)
        {
            RopeController rope = other.GetComponentInParent<RopeController>();
            if (rope == null && other.attachedRigidbody != null)
            {
                rope = other.attachedRigidbody.GetComponent<RopeController>();
            }
            if (rope != null && rope.IsAttached)
            {
                CacheReferences();
                Vector2 point = other.bounds.center;
                if (detectionArea != null)
                {
                    ColliderDistance2D contact = Physics2D.Distance(detectionArea, other);
                    point = contact.isValid ? contact.pointA : detectionArea.ClosestPoint(point);
                }
                Rigidbody2D body = rope.GetComponent<Rigidbody2D>();
                Vector2 velocity = body != null ? body.linearVelocity : Vector2.zero;
                // Preserve the existing detach/refund first. Presentation adds no force.
                rope.DetachAndRefund();
                if (body != null && body.simulated && Time.timeScale > 0f)
                {
                    GetComponent<ToySpikeVisual>()?.PlayContact(point, velocity);
                    RopeLandingFluff.PlaySpikeContact(other, point, velocity);
                }
            }
        }

        private void CacheReferences()
        {
            if (detectionArea == null || !detectionArea.enabled)
            {
                foreach (Collider2D candidate in GetComponents<Collider2D>())
                {
                    if (candidate.enabled && candidate.isTrigger)
                    {
                        detectionArea = candidate;
                        break;
                    }
                }
            }

            if (playerRope == null)
            {
                playerRope = FindFirstObjectByType<RopeController>();
            }
            if (playerRope != null && playerCollider == null)
            {
                playerCollider = playerRope.GetComponent<Collider2D>();
            }
        }
    }
}
