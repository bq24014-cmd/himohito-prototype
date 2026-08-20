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

        private Rigidbody2D body;
        private float elapsed;

        public void Configure(float left, float right, float duration)
        {
            leftX = Mathf.Min(left, right);
            rightX = Mathf.Max(left, right);
            cycleDuration = Mathf.Max(0.5f, duration);
            if (body != null)
            {
                body.position = new Vector2(leftX, body.position.y);
            }
        }

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            body.gravityScale = 0f;
            body.linearVelocity = Vector2.zero;
            body.angularVelocity = 0f;
            body.position = new Vector2(leftX, body.position.y);
        }

        private void FixedUpdate()
        {
            elapsed += Time.fixedDeltaTime;
            float progress = Mathf.PingPong(elapsed * 2f / cycleDuration, 1f);
            float easedProgress = Mathf.SmoothStep(0f, 1f, progress);
            body.MovePosition(new Vector2(
                Mathf.Lerp(leftX, rightX, easedProgress),
                body.position.y));
        }
    }
}
