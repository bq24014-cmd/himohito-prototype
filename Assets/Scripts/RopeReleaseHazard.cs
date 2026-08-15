using UnityEngine;

namespace HimoHito
{
    /// <summary>
    /// Detaches an active rope when the player enters this hazard's trigger.
    /// Normal refund and momentum preservation remain owned by RopeController.
    /// </summary>
    public sealed class RopeReleaseHazard : MonoBehaviour
    {
        [SerializeField, Range(1, PrototypeRunController.TutorialSectionCount)]
        private int activeTutorialSection = 2;

        private BoxCollider2D detectionArea;
        private RopeController ropeController;
        private Collider2D playerCollider;
        private SpriteRenderer hazardRenderer;
        private Collider2D[] hazardColliders;
        private bool isHazardVisible = true;

        public int ActiveTutorialSection => activeTutorialSection;

        public void Configure(int tutorialSection)
        {
            activeTutorialSection = Mathf.Clamp(
                tutorialSection,
                1,
                PrototypeRunController.TutorialSectionCount);
        }

        private void Awake()
        {
            CacheReferences();
            UpdateVisibility();
        }

        private void Update()
        {
            CacheReferences();
            UpdateVisibility();
        }

        private void FixedUpdate()
        {
            CacheReferences();
            if (!isHazardVisible ||
                detectionArea == null ||
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
            if (contactedRope == null || !contactedRope.IsAttached)
            {
                return false;
            }

            PrototypeRunController runController =
                contactedRope.GetComponent<PrototypeRunController>();
            return runController != null &&
                   runController.Outcome == PrototypeRunController.RunOutcome.Playing &&
                   runController.CurrentTutorialSection == activeTutorialSection;
        }

        private void CacheReferences()
        {
            if (hazardRenderer == null)
            {
                hazardRenderer = GetComponent<SpriteRenderer>();
            }

            if (hazardColliders == null || hazardColliders.Length == 0)
            {
                hazardColliders = GetComponents<Collider2D>();
            }

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

        private void UpdateVisibility()
        {
            if (ropeController == null)
            {
                return;
            }

            PrototypeRunController runController =
                ropeController.GetComponent<PrototypeRunController>();
            if (runController == null)
            {
                return;
            }

            bool shouldBeVisible =
                runController.CurrentTutorialSection <= activeTutorialSection;
            if (isHazardVisible == shouldBeVisible)
            {
                return;
            }

            isHazardVisible = shouldBeVisible;
            if (hazardRenderer != null)
            {
                hazardRenderer.enabled = shouldBeVisible;
            }

            foreach (Collider2D hazardCollider in hazardColliders)
            {
                if (hazardCollider != null)
                {
                    hazardCollider.enabled = shouldBeVisible;
                }
            }
        }
    }
}
