using UnityEngine;

namespace HimoHito
{
    /// <summary>Spike contact cuts an active rope and lets the player fall.</summary>
    [RequireComponent(typeof(Collider2D))]
    public sealed class RopeSpikeHazard : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D other) => Cut(other);
        private void OnTriggerStay2D(Collider2D other) => Cut(other);

        private static void Cut(Collider2D other)
        {
            RopeController rope = other.GetComponentInParent<RopeController>();
            if (rope != null && rope.IsAttached)
            {
                rope.DetachAndRefund();
            }
        }
    }

    /// <summary>
    /// A flashlight hazard that is disabled when a generated rope platform lies
    /// between its source and the player. The visible spot fades to show success.
    /// </summary>
    [RequireComponent(typeof(Collider2D), typeof(SpriteRenderer))]
    public sealed class PlatformOccludedLightHazard : MonoBehaviour
    {
        [SerializeField] private Transform lightSource;
        [SerializeField, Range(0.02f, 1f)] private float blockedAlpha = 0.08f;
        [SerializeField, Range(0.02f, 1f)] private float activeAlpha = 0.62f;
        [SerializeField, Min(0.01f)] private float fadeDuration = 0.4f;

        private SpriteRenderer spotRenderer;
        private Collider2D triggerArea;
        private RopeController playerRope;
        private Collider2D playerCollider;
        private float currentAlpha;

        public void Configure(Transform source)
        {
            lightSource = source;
        }

        private void Awake()
        {
            spotRenderer = GetComponent<SpriteRenderer>();
            triggerArea = GetComponent<Collider2D>();
            playerRope = FindFirstObjectByType<RopeController>();
            if (playerRope != null)
            {
                playerCollider = playerRope.GetComponent<Collider2D>();
            }
            currentAlpha = activeAlpha;
        }

        private void FixedUpdate()
        {
            if (playerRope == null || playerCollider == null || lightSource == null)
            {
                return;
            }

            bool blocked = IsBlockedByGeneratedPlatform();
            float targetAlpha = blocked ? blockedAlpha : activeAlpha;
            currentAlpha = Mathf.MoveTowards(
                currentAlpha,
                targetAlpha,
                Time.fixedDeltaTime / Mathf.Max(0.01f, fadeDuration));
            Color color = spotRenderer.color;
            color.a = currentAlpha;
            spotRenderer.color = color;

            if (!blocked && playerRope.IsAttached &&
                Physics2D.Distance(triggerArea, playerCollider).isOverlapped)
            {
                playerRope.DetachAndRefund();
            }
        }

        private bool IsBlockedByGeneratedPlatform()
        {
            Vector2 source = lightSource.position;
            Vector2 target = playerRope.transform.position;
            RaycastHit2D[] hits = Physics2D.LinecastAll(source, target);
            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider == null || hit.collider == playerCollider)
                {
                    continue;
                }
                if (hit.collider.TryGetComponent(out GeneratedRopePlatform _))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
