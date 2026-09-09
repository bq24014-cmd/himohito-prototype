using UnityEngine;

namespace HimoHito
{
    /// <summary>
    /// Saves tutorial progress after the player lands.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public sealed class TutorialCheckpoint : MonoBehaviour
    {
        private const float MinimumTopContactNormal = 0.6f;
        private const float TopContactTolerance = 0.12f;

        [SerializeField, Range(2, 4)] private int sectionNumber = 2;
        [SerializeField] private Vector2 respawnPosition;
        [SerializeField, Range(1, 14)] private int startingRopeLength = 6;

        private Collider2D checkpointCollider;

        public int SectionNumber => sectionNumber;
        public Vector2 RespawnPosition => respawnPosition;

        public void Configure(int section, Vector2 position, int selectedLength)
        {
            sectionNumber = Mathf.Clamp(section, 2, 4);
            respawnPosition = position;
            startingRopeLength = Mathf.Clamp(selectedLength, 1, 14);
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            if (!IsPlayerLandingOnTop(collision))
            {
                return;
            }

            PrototypeRunController runController =
                collision.rigidbody.GetComponent<PrototypeRunController>();
            if (runController != null)
            {
                int previousSection = runController.CurrentTutorialSection;
                runController.TryReachTutorialSection(
                    sectionNumber,
                    respawnPosition,
                    startingRopeLength);
                if (runController.CurrentTutorialSection > previousSection)
                    SectionArrivalFeedback.Notify(collision.rigidbody.gameObject,
                        sectionNumber, respawnPosition, checkpointCollider);
            }
        }

        private bool IsPlayerLandingOnTop(Collision2D collision)
        {
            if (collision.rigidbody == null)
            {
                return false;
            }

            checkpointCollider ??= GetComponent<Collider2D>();
            float surfaceTop = checkpointCollider.bounds.max.y;
            if (collision.rigidbody.worldCenterOfMass.y <= surfaceTop)
            {
                return false;
            }

            Collider2D playerCollider =
                collision.rigidbody.GetComponent<Collider2D>();
            if (playerCollider != null &&
                playerCollider.bounds.min.y <
                surfaceTop - TopContactTolerance)
            {
                return false;
            }

            for (int index = 0; index < collision.contactCount; index++)
            {
                ContactPoint2D contact = collision.GetContact(index);
                if (contact.point.y >= surfaceTop - TopContactTolerance &&
                    Mathf.Abs(contact.normal.y) >= MinimumTopContactNormal)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
