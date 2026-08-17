using UnityEngine;

namespace HimoHito
{
    /// <summary>
    /// Moves a kinematic obstacle slowly up and down so its safe window can be observed.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class MainStageVerticalMover : MonoBehaviour
    {
        [SerializeField] private float lowerY = -0.7f;
        [SerializeField] private float upperY = 7.3f;
        [SerializeField, Min(0.5f)] private float cycleDuration = 7.5f;

        private Rigidbody2D body;
        private float elapsed;

        public void Configure(float lower, float upper, float duration)
        {
            lowerY = Mathf.Min(lower, upper);
            upperY = Mathf.Max(lower, upper);
            cycleDuration = Mathf.Max(0.5f, duration);
        }

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            body.gravityScale = 0f;
            body.linearVelocity = Vector2.zero;
            body.angularVelocity = 0f;
            body.position = new Vector2(body.position.x, lowerY);
        }

        private void FixedUpdate()
        {
            elapsed += Time.fixedDeltaTime;
            float progress = Mathf.PingPong(elapsed * 2f / cycleDuration, 1f);
            float easedProgress = Mathf.SmoothStep(0f, 1f, progress);
            body.MovePosition(new Vector2(body.position.x, Mathf.Lerp(lowerY, upperY, easedProgress)));
        }
    }
}
