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
        [SerializeField, Min(0f)] private float fadeDuration = 0.6f;

        private Rigidbody2D body;
        private SpriteRenderer spotRenderer;
        private CircleCollider2D detectionArea;
        private float elapsed;
        private float visibleAlpha = 0.72f;

        public void Configure(
            float left,
            float right,
            float movementDuration,
            float offDuration,
            float transitionDuration)
        {
            leftX = Mathf.Min(left, right);
            rightX = Mathf.Max(left, right);
            cycleDuration = Mathf.Max(0.5f, movementDuration);
            hiddenDuration = Mathf.Max(0f, offDuration);
            fadeDuration = Mathf.Max(0f, transitionDuration);
            elapsed = 0f;
            if (body != null)
            {
                body.position = new Vector2(leftX, body.position.y);
            }
            SetLightState(1f, true);
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
            if (spotRenderer != null)
            {
                visibleAlpha = spotRenderer.color.a;
            }
            SetLightState(1f, true);
        }

        private void FixedUpdate()
        {
            elapsed += Time.fixedDeltaTime;
            float totalDuration = cycleDuration + hiddenDuration;
            float phase = totalDuration > 0f ? elapsed % totalDuration : 0f;
            if (phase >= cycleDuration)
            {
                body.MovePosition(new Vector2(leftX, body.position.y));
                float restProgress = phase - cycleDuration;
                float transition = Mathf.Min(fadeDuration, hiddenDuration * 0.5f);
                float intensity = CalculateRestIntensity(restProgress, transition);
                SetLightState(intensity, false);
                return;
            }

            SetLightState(1f, true);
            float progress = Mathf.PingPong(phase * 2f / cycleDuration, 1f);
            float easedProgress = Mathf.SmoothStep(0f, 1f, progress);
            body.MovePosition(new Vector2(
                Mathf.Lerp(leftX, rightX, easedProgress),
                body.position.y));
        }

        private float CalculateRestIntensity(float restProgress, float transition)
        {
            if (transition <= 0f)
            {
                return 0f;
            }

            if (restProgress < transition)
            {
                float fadeOut = Mathf.SmoothStep(
                    0f,
                    1f,
                    restProgress / transition);
                return 1f - fadeOut;
            }

            float fadeInStart = hiddenDuration - transition;
            if (restProgress > fadeInStart)
            {
                return Mathf.SmoothStep(
                    0f,
                    1f,
                    (restProgress - fadeInStart) / transition);
            }

            return 0f;
        }

        private void SetLightState(float intensity, bool harmful)
        {
            float clampedIntensity = Mathf.Clamp01(intensity);
            if (spotRenderer != null)
            {
                Color color = spotRenderer.color;
                color.a = visibleAlpha * clampedIntensity;
                spotRenderer.color = color;
                spotRenderer.enabled = clampedIntensity > 0.001f;
            }

            if (detectionArea != null)
            {
                detectionArea.enabled = harmful;
            }
        }
    }
}
