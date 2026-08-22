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
        [SerializeField] private float moveSpeed = 6.5f;
        [SerializeField] private float acceleration = 45f;
        [SerializeField] private float deceleration = 120f;

        [Header("Air control")]
        [SerializeField] private float airControlForce = 9f;
        [SerializeField] private float maximumAirSpeed = 10f;
        [SerializeField, Range(0f, 1f)] private float airLinearDamping = 0.05f;

        [Header("Pendulum control")]
        [SerializeField] private float swingPumpForce = 18f;
        [SerializeField] private float maximumSwingSpeed = 15f;
        [SerializeField, Range(30f, 89f)] private float maximumPumpedSwingAngle = 85f;
        [SerializeField, Range(0f, 1f)] private float swingLinearDamping = 0.12f;

        [Header("Jump")]
        [SerializeField] private float jumpImpulse = 10f;
        [SerializeField, Range(0f, 1f)] private float jumpHorizontalSpeedRetention = 0.6923077f;
        [SerializeField] private float coyoteTime = 0.1f;
        [SerializeField] private float jumpBufferTime = 0.15f;
        [SerializeField] private float groundProbeDistance = 0.08f;

        private Rigidbody2D body;
        private BoxCollider2D bodyCollider;
        private RopeController ropeController;
        private PhysicsMaterial2D movementMaterial;
        private GeneratedRopePlatform groundedRopePlatform;
        private float moveInput;
        private float coyoteTimer;
        private float jumpBufferTimer;

        public bool IsGrounded { get; private set; }

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            bodyCollider = GetComponent<BoxCollider2D>();
            ropeController = GetComponent<RopeController>();

            movementMaterial = new PhysicsMaterial2D("Player Movement Material")
            {
                friction = 0.1f,
                bounciness = 0f,
                hideFlags = HideFlags.HideAndDontSave
            };
            bodyCollider.sharedMaterial = movementMaterial;
        }

        private void OnDestroy()
        {
            if (movementMaterial != null)
            {
                Destroy(movementMaterial);
            }
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

            bool isSwinging = ropeController != null && ropeController.IsAttached;
            if (isSwinging)
            {
                ApplySwingControl();
            }
            else if (IsGrounded)
            {
                ApplyGroundControl();
            }
            else
            {
                ApplyAirControl();
            }

            if (jumpBufferTimer > 0f && coyoteTimer > 0f && !isSwinging)
            {
                float horizontalBrakeImpulse = -body.linearVelocity.x
                    * (1f - jumpHorizontalSpeedRetention)
                    * body.mass;
                Vector2 takeoffImpulse = new Vector2(horizontalBrakeImpulse, jumpImpulse);
                body.AddForce(takeoffImpulse, ForceMode2D.Impulse);
                jumpBufferTimer = 0f;
                coyoteTimer = 0f;
            }
        }

        private void ApplyGroundControl()
        {
            body.linearDamping = 0f;
            float targetSpeed = moveInput * moveSpeed;
            float speedChange = Mathf.Approximately(moveInput, 0f)
                ? deceleration
                : acceleration;
            float nextHorizontalSpeed = Mathf.MoveTowards(
                body.linearVelocity.x,
                targetSpeed,
                speedChange * Time.fixedDeltaTime);
            body.linearVelocity = new Vector2(nextHorizontalSpeed, body.linearVelocity.y);

            if (groundedRopePlatform != null && Mathf.Approximately(moveInput, 0f))
            {
                ApplyRopePlatformGrip();
            }
        }

        private void ApplyRopePlatformGrip()
        {
            Vector2 platformDirection =
                groundedRopePlatform.End - groundedRopePlatform.Start;
            if (platformDirection.sqrMagnitude < 0.0001f)
            {
                return;
            }

            Vector2 tangent = platformDirection.normalized;
            float slopeSpeed = Vector2.Dot(body.linearVelocity, tangent);
            body.linearVelocity -= tangent * slopeSpeed;

            Vector2 gravity = Physics2D.gravity * body.gravityScale;
            Vector2 gravityAlongSlope = tangent * Vector2.Dot(gravity, tangent);
            body.AddForce(-gravityAlongSlope * body.mass, ForceMode2D.Force);
        }

        private void ApplyAirControl()
        {
            body.linearDamping = airLinearDamping;
            if (Mathf.Approximately(moveInput, 0f))
            {
                return;
            }

            float speedInRequestedDirection = body.linearVelocity.x * moveInput;
            if (speedInRequestedDirection < maximumAirSpeed)
            {
                body.AddForce(Vector2.right * moveInput * airControlForce, ForceMode2D.Force);
            }
        }

        private void ApplySwingControl()
        {
            // Small damping represents air resistance without cancelling momentum.
            body.linearDamping = swingLinearDamping;
            if (Mathf.Approximately(moveInput, 0f))
            {
                return;
            }

            // The joint supplies rope tension. Input only adds force along the circle's tangent.
            // D pumps counter-clockwise; A pumps clockwise. This direction stays continuous
            // around the entire circle and avoids a force flip at the left/right extremes.
            Vector2 radiusDirection = (body.position - ropeController.AnchorPoint).normalized;
            Vector2 tangent = new Vector2(-radiusDirection.y, radiusDirection.x);

            float tangentialSpeed = Vector2.Dot(body.linearVelocity, tangent);
            float speedInRequestedDirection = tangentialSpeed * moveInput;
            float pumpScale = speedInRequestedDirection >= 0f
                ? CalculateSwingEnergyScale(radiusDirection, tangentialSpeed)
                : 1f;
            if (speedInRequestedDirection < maximumSwingSpeed && pumpScale > 0f)
            {
                body.AddForce(
                    tangent * moveInput * swingPumpForce * pumpScale,
                    ForceMode2D.Force);
            }
        }

        private float CalculateSwingEnergyScale(
            Vector2 radiusDirection,
            float tangentialSpeed)
        {
            float ropeLength = Mathf.Max(ropeController.ActiveRopeLength, 0.01f);
            float gravityAcceleration = Mathf.Abs(Physics2D.gravity.y * body.gravityScale);
            float heightAboveBottom = Mathf.Clamp(
                radiusDirection.y * ropeLength + ropeLength,
                0f,
                ropeLength * 2f);
            float currentEnergy =
                0.5f * tangentialSpeed * tangentialSpeed +
                gravityAcceleration * heightAboveBottom;

            float angleRadians = maximumPumpedSwingAngle * Mathf.Deg2Rad;
            float maximumHeight = ropeLength * (1f - Mathf.Cos(angleRadians));
            float energyLimit = gravityAcceleration * maximumHeight;
            float fadeStart = energyLimit * 0.75f;

            return 1f - Mathf.InverseLerp(fadeStart, energyLimit, currentEnergy);
        }

        private bool CheckGrounded()
        {
            groundedRopePlatform = null;
            Bounds bounds = bodyCollider.bounds;
            Vector2 probeCenter = new Vector2(
                bounds.center.x,
                bounds.min.y - groundProbeDistance * 0.5f);
            Vector2 probeSize = new Vector2(
                bounds.size.x * 0.8f,
                groundProbeDistance * 2f);
            Collider2D[] overlaps = Physics2D.OverlapBoxAll(probeCenter, probeSize, 0f);

            bool isGrounded = false;
            foreach (Collider2D overlap in overlaps)
            {
                if (overlap != null && overlap != bodyCollider)
                {
                    isGrounded = true;
                    if (overlap.TryGetComponent(
                            out GeneratedRopePlatform generatedRopePlatform))
                    {
                        groundedRopePlatform = generatedRopePlatform;
                    }
                }
            }

            return isGrounded;
        }
    }
}
