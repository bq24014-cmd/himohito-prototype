using UnityEngine;

namespace HimoHito
{
    /// <summary>
    /// Shrinks only the player's visible body as rope is spent.
    /// The player transform, Rigidbody2D, and Collider2D keep their original size.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RopeResource), typeof(SpriteRenderer))]
    public sealed class RopeBodyVisual : MonoBehaviour
    {
        [SerializeField, Range(0.2f, 1f)] private float minimumVisualScale = 0.65f;

        [Header("Landing squash")]
        [SerializeField, Min(0.05f)] private float landingDuration = 0.18f;
        [SerializeField, Min(0f)] private float minimumLandingSpeed = 1.5f;
        [SerializeField, Min(0.01f)] private float fullLandingSpeed = 10f;
        [SerializeField, Range(0f, 0.3f)] private float maximumHorizontalSquash = 0.10f;
        [SerializeField, Range(0f, 0.3f)] private float maximumVerticalSquash = 0.14f;

        private RopeResource ropeResource;
        private RopeController ropeController;
        private PlayerMover playerMover;
        private Rigidbody2D body;
        private SpriteRenderer sourceRenderer;
        private Transform visualTransform;
        private float currentBaseScale = 1f;
        private float fastestFallSpeed;
        private float landingElapsed;
        private float landingStrength;
        private bool wasGrounded;

        private void Awake()
        {
            ropeResource = GetComponent<RopeResource>();
            ropeController = GetComponent<RopeController>();
            playerMover = GetComponent<PlayerMover>();
            body = GetComponent<Rigidbody2D>();
            sourceRenderer = GetComponent<SpriteRenderer>();
            CreateVisualBody();
            UpdateRemainingLengthScale();
            landingElapsed = landingDuration;
            wasGrounded = playerMover != null && playerMover.IsGrounded;
            ApplyVisualScale();
        }

        private void LateUpdate()
        {
            UpdateLandingState();

            if (ropeController == null || !ropeController.IsAttached)
            {
                UpdateRemainingLengthScale();
            }

            ApplyVisualScale();
        }

        private void OnDestroy()
        {
            if (sourceRenderer != null)
            {
                sourceRenderer.enabled = true;
            }
        }

        private void CreateVisualBody()
        {
            GameObject visualObject = new GameObject("Rope Body Visual");
            visualObject.layer = gameObject.layer;
            visualTransform = visualObject.transform;
            visualTransform.SetParent(transform, false);

            SpriteRenderer visualRenderer = visualObject.AddComponent<SpriteRenderer>();
            visualRenderer.sortingLayerID = sourceRenderer.sortingLayerID;
            visualRenderer.sortingOrder = sourceRenderer.sortingOrder;
            visualRenderer.maskInteraction = sourceRenderer.maskInteraction;

            SolidSprite solidSprite = visualObject.AddComponent<SolidSprite>();
            solidSprite.Color = sourceRenderer.color;

            sourceRenderer.enabled = false;
        }

        private void UpdateRemainingLengthScale()
        {
            if (ropeResource == null)
            {
                return;
            }

            float remainingRatio = Mathf.Clamp01(ropeResource.NormalizedLength);
            currentBaseScale = Mathf.Lerp(minimumVisualScale, 1f, remainingRatio);
        }

        private void UpdateLandingState()
        {
            if (playerMover == null || body == null)
            {
                return;
            }

            bool isGrounded = playerMover.IsGrounded;
            if (!isGrounded)
            {
                fastestFallSpeed = Mathf.Max(fastestFallSpeed, -body.linearVelocity.y);
            }
            else if (!wasGrounded)
            {
                if (fastestFallSpeed >= minimumLandingSpeed)
                {
                    landingStrength = Mathf.Lerp(
                        0.55f,
                        1f,
                        Mathf.InverseLerp(
                            minimumLandingSpeed,
                            Mathf.Max(minimumLandingSpeed + 0.01f, fullLandingSpeed),
                            fastestFallSpeed));
                    landingElapsed = 0f;
                }

                fastestFallSpeed = 0f;
            }

            wasGrounded = isGrounded;
        }

        private void ApplyVisualScale()
        {
            if (visualTransform == null)
            {
                return;
            }

            Vector2 landingScale = CalculateLandingScale();
            visualTransform.localScale = new Vector3(
                currentBaseScale * landingScale.x,
                currentBaseScale * landingScale.y,
                1f);
        }

        private Vector2 CalculateLandingScale()
        {
            if (landingElapsed >= landingDuration)
            {
                return Vector2.one;
            }

            float normalizedTime = Mathf.Clamp01(landingElapsed / landingDuration);
            landingElapsed += Time.deltaTime;

            float squashAmount;
            if (normalizedTime < 0.42f)
            {
                squashAmount = Mathf.SmoothStep(0f, 1f, normalizedTime / 0.42f);
            }
            else
            {
                squashAmount = 1f - Mathf.SmoothStep(
                    0f,
                    1f,
                    (normalizedTime - 0.42f) / 0.58f);
            }

            return new Vector2(
                1f + maximumHorizontalSquash * landingStrength * squashAmount,
                1f - maximumVerticalSquash * landingStrength * squashAmount);
        }
    }
}
