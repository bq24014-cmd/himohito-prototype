using UnityEngine;

namespace HimoHito
{
    /// <summary>
    /// Opens the right-hand passage only when the shaft bridge has the depth
    /// produced by length seven or eight. The tall wooden panel also prevents
    /// a normal jump or a shallow bridge from skipping the length choice.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public sealed class SectionEightDepthGate : MonoBehaviour
    {
        [SerializeField, Min(0.1f)] private float openingRise = 9f;
        [SerializeField, Min(0.1f)] private float movementSpeed = 18f;

        private Collider2D gateCollider;
        private RopePlatformBuilder platformBuilder;
        private Vector3 closedPosition;
        private bool isOpen;

        private void Awake()
        {
            gateCollider = GetComponent<Collider2D>();
            platformBuilder = FindFirstObjectByType<RopePlatformBuilder>();
            closedPosition = transform.position;
        }

        private void FixedUpdate()
        {
            if (platformBuilder == null)
            {
                platformBuilder = FindFirstObjectByType<RopePlatformBuilder>();
            }

            bool shouldOpen =
                MainStageSectionEightSetup.TryGetShaftPlatformLength(
                    platformBuilder,
                    out float ropeLength) &&
                MainStageSectionEightSetup.IsCorrectLength(ropeLength);
            if (shouldOpen != isOpen)
            {
                isOpen = shouldOpen;
                if (isOpen && gateCollider != null)
                {
                    gateCollider.enabled = false;
                }
            }

            Vector3 targetPosition = closedPosition +
                (isOpen ? Vector3.up * openingRise : Vector3.zero);
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPosition,
                movementSpeed * Time.fixedDeltaTime);

            if (!isOpen && gateCollider != null &&
                Vector3.SqrMagnitude(transform.position - closedPosition) <=
                    0.0001f)
            {
                gateCollider.enabled = true;
            }
        }
    }
}
