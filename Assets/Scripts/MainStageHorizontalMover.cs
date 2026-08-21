using UnityEngine;

namespace HimoHito
{
    /// <summary>
    /// Moves a kinematic flashlight spot slowly from side to side.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class MainStageHorizontalMover : MonoBehaviour
    {
        [SerializeField] private float leftX = 79.2f;
        [SerializeField] private float rightX = 87.2f;
        [SerializeField, Min(0.5f)] private float cycleDuration = 8.9f;
        [SerializeField, Min(0f)] private float hiddenDuration = 3f;

        private Rigidbody2D body;
        private SpriteRenderer spotRenderer;
        private CircleCollider2D detectionArea;
        private float elapsed;
        private bool isLightVisible = true;

        public void Configure(
            float left,
            float right,
            float movementDuration,
            float offDuration)
        {
            leftX = Mathf.Min(left, right);
            rightX = Mathf.Max(left, right);
            cycleDuration = Mathf.Max(0.5f, movementDuration);
            hiddenDuration = Mathf.Max(0f, offDuration);
            elapsed = 0f;
            if (body != null)
            {
                body.position = new Vector2(leftX, body.position.y);
            }
            SetLightVisible(true);
        }

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            body.gravityScale = 0f;
            body.linearVelocity = Vector2.zero;
            body.angularVelocity = 0f;
            body.position = new Vector2(leftX, body.position.y);
            spotRenderer = GetComponent<SpriteRenderer>();
            detectionArea = GetComponent<CircleCollider2D>();
            SetLightVisible(true);
        }

        private void FixedUpdate()
        {
            elapsed += Time.fixedDeltaTime;
            float totalDuration = cycleDuration + hiddenDuration;
            float phase = totalDuration > 0f ? elapsed % totalDuration : 0f;
            bool shouldBeVisible = phase < cycleDuration;
            SetLightVisible(shouldBeVisible);
            if (!shouldBeVisible)
            {
                body.MovePosition(new Vector2(leftX, body.position.y));
                return;
            }

            float progress = Mathf.PingPong(phase * 2f / cycleDuration, 1f);
            float easedProgress = Mathf.SmoothStep(0f, 1f, progress);
            body.MovePosition(new Vector2(
                Mathf.Lerp(leftX, rightX, easedProgress),
                body.position.y));
        }

        private void SetLightVisible(bool visible)
        {
            if (isLightVisible == visible &&
                spotRenderer != null &&
                detectionArea != null)
            {
                return;
            }

            isLightVisible = visible;
            if (spotRenderer != null)
            {
                spotRenderer.enabled = visible;
            }

            if (detectionArea != null)
            {
                detectionArea.enabled = visible;
            }
        }
    }
}
