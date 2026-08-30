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

        [SerializeField, Min(0f)]
        private float visualTransitionSpeed = 14f;

        private SpriteRenderer spotRenderer;
        private Color visibleColor = Color.white;
        private bool isBlocked;

        public bool IsBlocked => isBlocked;

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

            spotRenderer = GetComponent<SpriteRenderer>();
            if (spotRenderer != null)
            {
                visibleColor = spotRenderer.color;
            }
        }

        private void Update()
        {
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
            SetBlocked(false, true);
        }

        private void TryDetach(Collider2D other)
        {
            RopeController contactedRope = other.GetComponentInParent<RopeController>();
            if (contactedRope == null || !contactedRope.IsAttached)
            {
                SetBlocked(false);
                return;
            }

            if (canBeBlockedByGeneratedRopePlatform &&
                IsBlockedByGeneratedRopePlatform(contactedRope))
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

        private Color GetTargetColor()
        {
            if (!isBlocked)
            {
                return visibleColor;
            }

            return new Color(
                visibleColor.r * blockedBrightnessMultiplier,
                visibleColor.g * blockedBrightnessMultiplier,
                visibleColor.b * blockedBrightnessMultiplier,
                visibleColor.a * blockedAlphaMultiplier);
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
