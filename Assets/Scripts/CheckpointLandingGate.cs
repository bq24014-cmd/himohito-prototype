using UnityEngine;

namespace HimoHito
{
    /// <summary>Shared read-only support check: brushing a wall or corner is not arrival.</summary>
    internal sealed class CheckpointLandingGate
    {
        private const float SettleTime = .12f;
        private const float SurfaceTolerance = .06f;
        private Rigidbody2D supportedBody;
        private float supportedSince, lastSupportedAt;

        public void Reset() { supportedBody = null; }

        public void Exit(Rigidbody2D body)
        {
            if (supportedBody == body) Reset();
        }

        public bool Evaluate(Collision2D collision, Collider2D floor, Vector2 respawn, float now)
        {
            if (!HasTopSupport(collision, floor, respawn) || MainStagePreview.IsActive)
            {
                Reset();
                return false;
            }
            Rigidbody2D body = collision.rigidbody;
            if (supportedBody != body || now < lastSupportedAt ||
                now - lastSupportedAt > Time.fixedDeltaTime * 1.5f)
            {
                supportedBody = body;
                supportedSince = now;
            }
            lastSupportedAt = now;
            return now - supportedSince >= SettleTime - .0001f;
        }

        private static bool HasTopSupport(Collision2D collision, Collider2D floor, Vector2 respawn)
        {
            Rigidbody2D body = collision.rigidbody;
            if (body == null || !body.simulated || floor == null || !floor.enabled || floor.isTrigger)
                return false;
            Bounds bounds = floor.bounds;
            // A relay with a stale copied checkpoint cannot save a different, distant landing.
            if (respawn.x < bounds.min.x || respawn.x > bounds.max.x) return false;
            Collider2D player = collision.collider;
            if (player == null || player.attachedRigidbody != body) return false;
            Bounds feet = player.bounds;
            float inset = Mathf.Min(.04f, bounds.size.x * .1f);
            if (feet.center.x < bounds.min.x + inset || feet.center.x > bounds.max.x - inset ||
                feet.min.y < bounds.max.y - SurfaceTolerance ||
                feet.min.y > bounds.max.y + SurfaceTolerance ||
                body.worldCenterOfMass.y <= bounds.max.y)
                return false;
            float floorSpeed = floor.attachedRigidbody != null ? floor.attachedRigidbody.linearVelocity.y : 0f;
            if (Mathf.Abs(body.linearVelocity.y - floorSpeed) > .5f) return false;

            for (int i = 0; i < collision.contactCount; i++)
            {
                ContactPoint2D contact = collision.GetContact(i);
                if (contact.point.x >= bounds.min.x && contact.point.x <= bounds.max.x &&
                    Mathf.Abs(contact.point.y - bounds.max.y) <= SurfaceTolerance &&
                    Mathf.Abs(contact.normal.y) >= .8f)
                    return true;
            }
            return false;
        }
    }
}
