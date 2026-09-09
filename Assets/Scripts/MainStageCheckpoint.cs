using UnityEngine;

namespace HimoHito
{
    /// <summary>
    /// Marks the start of a section and applies its resource lower bound.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public sealed class MainStageCheckpoint : MonoBehaviour
    {
        private const float MinimumTopContactNormal = 0.6f;
        private const float TopContactTolerance = 0.12f;

        [SerializeField, Range(2, 10)] private int sectionNumber = 2;
        [SerializeField] private Vector2 respawnPosition;
        [SerializeField, Min(0f)] private float minimumRopeAfterCheckpoint;
        private bool reached;
        private Collider2D checkpointCollider;

        public void Configure(int section, Vector2 position, float lowerBound)
        {
            sectionNumber = Mathf.Clamp(section, 2, 10);
            respawnPosition = position;
            minimumRopeAfterCheckpoint = Mathf.Max(0f, lowerBound);
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            if (reached || !IsPlayerLandingOnTop(collision))
            {
                return;
            }

            MainStageRespawnOnFall respawn =
                collision.rigidbody.GetComponent<MainStageRespawnOnFall>();
            if (respawn != null && respawn.TryReachSection(
                    sectionNumber,
                    respawnPosition,
                    minimumRopeAfterCheckpoint))
            {
                reached = true;
                SectionArrivalFeedback.Notify(collision.rigidbody.gameObject,
                    sectionNumber, respawnPosition, checkpointCollider);
            }
        }

        private bool IsPlayerLandingOnTop(Collision2D collision)
        {
            if (collision.rigidbody == null || !collision.rigidbody.simulated ||
                MainStagePreview.IsActive)
                return false;

            checkpointCollider ??= GetComponent<Collider2D>();
            Bounds floor = checkpointCollider.bounds;
            // A copied/stale checkpoint on a relay must not unlock a distant bank.
            if (respawnPosition.x < floor.min.x || respawnPosition.x > floor.max.x)
                return false;

            if (collision.rigidbody.worldCenterOfMass.y <= floor.max.y)
                return false;

            Collider2D playerCollider = collision.rigidbody.GetComponent<Collider2D>();
            if (playerCollider == null ||
                playerCollider.bounds.min.y < floor.max.y - TopContactTolerance)
                return false;

            for (int index = 0; index < collision.contactCount; index++)
            {
                ContactPoint2D contact = collision.GetContact(index);
                if (contact.point.x >= floor.min.x - TopContactTolerance &&
                    contact.point.x <= floor.max.x + TopContactTolerance &&
                    contact.point.y >= floor.max.y - TopContactTolerance &&
                    Mathf.Abs(contact.normal.y) >= MinimumTopContactNormal)
                    return true;
            }
            return false;
        }
    }
}
