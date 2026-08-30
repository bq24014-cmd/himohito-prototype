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

        [SerializeField, Range(0f, 1f)]
        private float blockedBrightnessMultiplier = 0.45f;

        [SerializeField, Range(0f, 1f)]
        private float blockedAlphaMultiplier = 0.22f;

        [SerializeField, Range(0f, 1f)]
        private float placementPreviewBrightnessMultiplier = 0.72f;

        [SerializeField, Range(0f, 1f)]
        private float placementPreviewAlphaMultiplier = 0.5f;

        [SerializeField, Min(0f)]
        private float placementPreviewMargin = 0.3f;

        [SerializeField, Min(0f)]
        private float visualTransitionSpeed = 14f;

        private SpriteRenderer spotRenderer;
        private Collider2D detectionArea;
        private RopeController playerRope;
        private RopePlatformBuilder platformBuilder;
        private Color visibleColor = Color.white;
        private bool isBlocked;
        private bool isPlacementPreview;

        public bool IsBlocked => isBlocked;
        public bool IsPlacementPreview => isPlacementPreview;

        public void ConfigureRopePlatformBlocking(bool canBeBlocked)
        {
            canBeBlockedByGeneratedRopePlatform = canBeBlocked;
        }

        private void Awake()
        {
            detectionArea = GetComponent<Collider2D>();
            if (detectionArea != null)
            {
                detectionArea.isTrigger = true;
            }

            playerRope = FindFirstObjectByType<RopeController>();
            platformBuilder = FindFirstObjectByType<RopePlatformBuilder>();

            spotRenderer = GetComponent<SpriteRenderer>();
            if (spotRenderer != null)
            {
                visibleColor = spotRenderer.color;
            }
        }

        private void Update()
        {
            UpdatePlacementPreview();
            UpdateBlockedVisual();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            TryDetach(other);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            TryDetach(other);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.GetComponentInParent<RopeController>() != null)
            {
                SetBlocked(false);
            }
        }

        private void OnDisable()
        {
            isPlacementPreview = false;
            SetBlocked(false, true);
        }

        private void TryDetach(Collider2D other)
        {
            RopeController contactedRope = other.GetComponentInParent<RopeController>();
            if (contactedRope == null)
            {
                return;
            }

            if (!contactedRope.IsAttached)
            {
                SetBlocked(false);
                return;
            }

            if (canBeBlockedByGeneratedRopePlatform &&
                HasGeneratedRopePlatformInLight())
            {
                SetBlocked(true);
                return;
            }

            SetBlocked(false);
            contactedRope.DetachAndRefund();
        }

        private void SetBlocked(bool blocked, bool applyImmediately = false)
        {
            isBlocked = canBeBlockedByGeneratedRopePlatform && blocked;
            if (applyImmediately && spotRenderer != null)
            {
                spotRenderer.color = GetTargetColor();
            }
        }

        private void UpdateBlockedVisual()
        {
            if (spotRenderer == null || !canBeBlockedByGeneratedRopePlatform)
            {
                return;
            }

            Color targetColor = GetTargetColor();
            float progress = 1f - Mathf.Exp(
                -visualTransitionSpeed * Time.deltaTime);
            spotRenderer.color = Color.Lerp(
                spotRenderer.color,
                targetColor,
                progress);
        }

        private void UpdatePlacementPreview()
        {
            isPlacementPreview = false;
            if (!canBeBlockedByGeneratedRopePlatform || isBlocked)
            {
                return;
            }

            if (playerRope == null)
            {
                playerRope = FindFirstObjectByType<RopeController>();
            }
            if (platformBuilder == null)
            {
                platformBuilder = FindFirstObjectByType<RopePlatformBuilder>();
            }

            if (HasGeneratedRopePlatformInLight())
            {
                isPlacementPreview = true;
                return;
            }

            if (playerRope == null ||
                playerRope.IsAttached ||
                platformBuilder == null ||
                !platformBuilder.CanBuildCurrentPlatform ||
                !playerRope.TryResolveCurrentAimAnchor(out Vector2 platformEnd))
            {
                return;
            }

            Vector2 platformStart = playerRope.transform.position;
            Vector2 segment = platformEnd - platformStart;
            float segmentLengthSquared = segment.sqrMagnitude;
            if (segmentLengthSquared <= 0.0001f)
            {
                return;
            }

            Vector2 lightPosition = transform.position;
            float progress = Mathf.Clamp01(
                Vector2.Dot(lightPosition - platformStart, segment) /
                segmentLengthSquared);
            Vector2 closestPoint = platformStart + segment * progress;
            float previewRadius = GetWorldDetectionRadius() +
                placementPreviewMargin;
            isPlacementPreview = progress > 0.1f &&
                Vector2.Distance(closestPoint, lightPosition) <= previewRadius;
        }

        private float GetWorldDetectionRadius()
        {
            if (detectionArea is not CircleCollider2D circle)
            {
                return 0f;
            }

            Vector3 scale = circle.transform.lossyScale;
            return circle.radius * Mathf.Max(
                Mathf.Abs(scale.x),
                Mathf.Abs(scale.y));
        }

        private Color GetTargetColor()
        {
            if (isBlocked)
            {
                return new Color(
                    visibleColor.r * blockedBrightnessMultiplier,
                    visibleColor.g * blockedBrightnessMultiplier,
                    visibleColor.b * blockedBrightnessMultiplier,
                    visibleColor.a * blockedAlphaMultiplier);
            }

            if (isPlacementPreview)
            {
                return new Color(
                    visibleColor.r * placementPreviewBrightnessMultiplier,
                    visibleColor.g * placementPreviewBrightnessMultiplier,
                    visibleColor.b * placementPreviewBrightnessMultiplier,
                    visibleColor.a * placementPreviewAlphaMultiplier);
            }

            return visibleColor;
        }

        private bool HasGeneratedRopePlatformInLight()
        {
            Vector2 lightPosition = transform.position;
            Collider2D[] hits = Physics2D.OverlapCircleAll(
                lightPosition,
                GetWorldDetectionRadius());
            foreach (Collider2D hit in hits)
            {
                if (hit == null || hit.isTrigger)
                {
                    continue;
                }

                if (hit.GetComponentInParent<GeneratedRopePlatform>() != null)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
