using UnityEngine;

namespace HimoHito
{
    /// <summary>
    /// Marks the run clear when the player reaches the goal object.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public sealed class GoalZone : MonoBehaviour
    {
        private const float MinimumTopContactNormal = 0.6f;
        private const float TopContactTolerance = 0.12f;

        private Collider2D goalCollider;

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
                if (GoalChestPresentation.ReadyToClear(collision, goalCollider,
                    TutorialSectionFourSetup.GoalMarkerName))
                    runController.MarkClear();
            }
        }

        private bool IsPlayerLandingOnTop(Collision2D collision)
        {
            if (collision.rigidbody == null)
            {
                return false;
            }

            goalCollider ??= GetComponent<Collider2D>();
            float surfaceTop = goalCollider.bounds.max.y;
            Collider2D playerCollider =
                collision.rigidbody.GetComponent<Collider2D>();
            if (collision.rigidbody.worldCenterOfMass.y <= surfaceTop ||
                (playerCollider != null &&
                 playerCollider.bounds.min.y <
                 surfaceTop - TopContactTolerance))
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
