using UnityEngine;

namespace HimoHito
{
    /// <summary>
    /// Releases an attached rope when the player enters a main-stage hazard.
    /// The trigger is not a floor, so the detached player continues falling.
    /// </summary>
    public sealed class MainStageRopeHazard : MonoBehaviour
    {
        [SerializeField]
        private bool canBeBlockedByGeneratedRopePlatform;

        public void ConfigureRopePlatformBlocking(bool canBeBlocked)
        {
            canBeBlockedByGeneratedRopePlatform = canBeBlocked;
        }

        private void Awake()
        {
            Collider2D detectionArea = GetComponent<Collider2D>();
            if (detectionArea != null)
            {
                detectionArea.isTrigger = true;
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
            if (contactedRope == null || !contactedRope.IsAttached)
            {
                return;
            }

            if (canBeBlockedByGeneratedRopePlatform &&
                IsBlockedByGeneratedRopePlatform(contactedRope))
            {
                return;
            }

            contactedRope.DetachAndRefund();
        }

        private bool IsBlockedByGeneratedRopePlatform(
            RopeController contactedRope)
        {
            Vector2 lightPosition = transform.position;
            Vector2 playerPosition = contactedRope.transform.position;
            Vector2 toPlayer = playerPosition - lightPosition;
            float distanceToPlayer = toPlayer.magnitude;
            if (distanceToPlayer <= 0.01f)
            {
                return false;
            }

            RaycastHit2D[] hits = Physics2D.RaycastAll(
                lightPosition,
                toPlayer / distanceToPlayer,
                distanceToPlayer);
            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider == null || hit.collider.isTrigger)
                {
                    continue;
                }

                if (hit.collider.GetComponentInParent<GeneratedRopePlatform>() != null)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
