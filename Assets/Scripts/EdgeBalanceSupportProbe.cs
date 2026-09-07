using UnityEngine;

namespace HimoHito
{
    /// <summary>Read-only floor probes for presentation; never changes grounding.</summary>
    internal sealed class EdgeBalanceSupportProbe
    {
        private readonly RaycastHit2D[] hits = new RaycastHit2D[16];

        public int FindOpenSide(Collider2D player)
        {
            if (player == null || !player.enabled) return 0;
            Bounds bounds = player.bounds;
            float y = bounds.min.y + .12f;
            if (!TryFindSupport(player, new Vector2(bounds.center.x, y), .32f, out Collider2D center)) return 0;
            // A continuous sagging rope can fall outside a vertical side probe.
            // Its walking ripple already communicates balance; do not call it
            // a ledge. Bank-side probes below still recognise adjacent ropes.
            if (center == null || center.GetComponent<GeneratedRopePlatform>() != null) return 0;
            bool left = TryFindSupport(player, new Vector2(bounds.min.x - .14f, y), .72f, out _);
            bool right = TryFindSupport(player, new Vector2(bounds.max.x + .14f, y), .72f, out _);
            return EdgeBalancePoseState.ClassifySupport(true, left, right);
        }

        private bool TryFindSupport(Collider2D player, Vector2 origin, float distance, out Collider2D support)
        {
            support = null;
            ContactFilter2D filter = new ContactFilter2D();
            filter.SetLayerMask(Physics2D.GetLayerCollisionMask(player.gameObject.layer));
            filter.useTriggers = false;
            int count = Physics2D.Raycast(origin, Vector2.down, filter, hits, distance);
            // Conservatively suppress the reaction if a dense scene fills the
            // fixed buffer; missing a visual is better than inventing a cliff.
            if (count == hits.Length) return true;
            for (int i = 0; i < count; i++)
            {
                RaycastHit2D hit = hits[i];
                Collider2D other = hit.collider;
                if (other == null || other == player || other.isTrigger || hit.fraction <= 0f ||
                    other.attachedRigidbody == player.attachedRigidbody || hit.normal.y < .55f ||
                    Physics2D.GetIgnoreCollision(player, other)) continue;
                // Do not call OneWayRailPlatform.CanSupport: it configures the
                // effector. A visual probe must only read the current state.
                if (other.GetComponent<OneWayRailPlatform>() != null &&
                    player.bounds.min.y < other.bounds.max.y - .12f) continue;
                support = other;
                return true;
            }
            return false;
        }
    }
}
