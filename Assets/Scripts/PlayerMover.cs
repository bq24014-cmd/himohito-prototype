using UnityEngine;

namespace HimoHito
{
    /// <summary>
    /// Reads player input in Update and applies physics in FixedUpdate.
    /// Coyote time and jump buffering make the simple square feel responsive.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
    public sealed class PlayerMover : MonoBehaviour
    {
        [Header("Horizontal movement")]
        [SerializeField] private float moveSpeed = 7f;
        [SerializeField] private float acceleration = 45f;

        [Header("Jump")]
        [SerializeField] private float jumpImpulse = 10f;
        [SerializeField] private float coyoteTime = 0.1f;
        [SerializeField] private float jumpBufferTime = 0.15f;
        [SerializeField] private float groundProbeDistance = 0.08f;

        private Rigidbody2D body;
        private BoxCollider2D bodyCollider;
        private float moveInput;
        private float coyoteTimer;
        private float jumpBufferTimer;

        public bool IsGrounded { get; private set; }

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            bodyCollider = GetComponent<BoxCollider2D>();
        }

        private void Update()
        {
            // Arrow keys are reserved for rope aiming, so movement uses A/D only.
            float left = Input.GetKey(KeyCode.A) ? -1f : 0f;
            float right = Input.GetKey(KeyCode.D) ? 1f : 0f;
            moveInput = left + right;

            if (Input.GetButtonDown("Jump"))
            {
                jumpBufferTimer = jumpBufferTime;
            }

            jumpBufferTimer -= Time.deltaTime;
        }

        private void FixedUpdate()
        {
            IsGrounded = CheckGrounded();
            coyoteTimer = IsGrounded ? coyoteTime : coyoteTimer - Time.fixedDeltaTime;

            float targetSpeed = moveInput * moveSpeed;
            float nextHorizontalSpeed = Mathf.MoveTowards(
                body.linearVelocity.x,
                targetSpeed,
                acceleration * Time.fixedDeltaTime);
            body.linearVelocity = new Vector2(nextHorizontalSpeed, body.linearVelocity.y);

            if (jumpBufferTimer > 0f && coyoteTimer > 0f)
            {
                body.AddForce(Vector2.up * jumpImpulse, ForceMode2D.Impulse);
                jumpBufferTimer = 0f;
                coyoteTimer = 0f;
            }
        }

        private bool CheckGrounded()
        {
            Bounds bounds = bodyCollider.bounds;
            RaycastHit2D[] hits = Physics2D.BoxCastAll(
                bounds.center,
                new Vector2(bounds.size.x * 0.8f, bounds.size.y * 0.9f),
                0f,
                Vector2.down,
                groundProbeDistance);

            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider != null && hit.collider != bodyCollider)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
