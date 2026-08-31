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
        [SerializeField, Min(0f)] private float attachedRopeSlackThreshold = 0.1f;

        [Header("Jump")]
        [SerializeField] private float jumpImpulse = 10f;
        [SerializeField, Range(0f, 1f)] private float jumpHorizontalSpeedRetention = 0.6923077f;
        [SerializeField] private float coyoteTime = 0.1f;
        [SerializeField] private float jumpBufferTime = 0.15f;
        [SerializeField] private float groundProbeDistance = 0.08f;

        [Header("Generated rope platform")]
        [SerializeField, Min(0f)] private float ropePlatformContactGraceTime = 0.15f;

        [Header("Solid swing collision")]
        [SerializeField, Min(0f)] private float solidSurfaceSkin = 0.01f;
        [SerializeField, Min(0.1f)] private float maximumCollisionSweepDistance = 2f;

        private Rigidbody2D body;
        private BoxCollider2D bodyCollider;
        private RopeController ropeController;
        private PhysicsMaterial2D movementMaterial;
        private GeneratedRopePlatform groundedRopePlatform;
        private GeneratedRopePlatform recentRopePlatform;
        private float lastRopePlatformContactTime = float.NegativeInfinity;
        private float moveInput;
        private float coyoteTimer;
        private float jumpBufferTimer;
        private Vector2 previousPhysicsPosition;
        private bool hasPreviousPhysicsPosition;

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
            previousPhysicsPosition = body.position;
            hasPreviousPhysicsPosition = true;
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
            PreventSolidSurfaceTunneling();
            IsGrounded = CheckGrounded();
            coyoteTimer = IsGrounded ? coyoteTime : coyoteTimer - Time.fixedDeltaTime;

            bool isSwinging = ropeController != null && ropeController.IsAttached;
            if (isSwinging)
            {
                if (ShouldUseAttachmentLaunchControl())
                {
                    ApplyAttachmentLaunchControl();
                }
                else
                {
                    ApplySwingControl();
                }
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
                ClearRecentRopePlatform();
                jumpBufferTimer = 0f;
                coyoteTimer = 0f;
            }

            previousPhysicsPosition = body.position;
            hasPreviousPhysicsPosition = true;
        }

        private void PreventSolidSurfaceTunneling()
        {
            if (!hasPreviousPhysicsPosition)
            {
                previousPhysicsPosition = body.position;
                hasPreviousPhysicsPosition = true;
                return;
            }

            Vector2 displacement = body.position - previousPhysicsPosition;
            float distance = displacement.magnitude;
            if (distance <= Mathf.Epsilon || distance > maximumCollisionSweepDistance)
            {
                previousPhysicsPosition = body.position;
                return;
            }

            Vector2 direction = displacement / distance;
            Vector2 centerOffset =
                (Vector2)bodyCollider.bounds.center - body.position;
            RaycastHit2D[] hits = Physics2D.BoxCastAll(
                previousPhysicsPosition + centerOffset,
                bodyCollider.bounds.size,
                0f,
                direction,
                distance);

            RaycastHit2D closestSolidHit = default;
            bool foundSolidHit = false;
            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider == null ||
                    !hit.collider.TryGetComponent(out SolidSwingSurface _) ||
                    hit.distance <= solidSurfaceSkin)
                {
                    continue;
                }

                if (!foundSolidHit || hit.distance < closestSolidHit.distance)
                {
                    closestSolidHit = hit;
                    foundSolidHit = true;
                }
            }

            if (!foundSolidHit)
            {
                return;
            }

            float safeDistance = Mathf.Max(
                0f,
                closestSolidHit.distance - solidSurfaceSkin);
            body.position = previousPhysicsPosition + direction * safeDistance;

            float speedIntoSurface =
                Vector2.Dot(body.linearVelocity, closestSolidHit.normal);
            if (speedIntoSurface < 0f)
            {
                body.linearVelocity -= closestSolidHit.normal * speedIntoSurface;
            }
        }

        public void RegisterGeneratedRopePlatformContact(
            GeneratedRopePlatform platform)
        {
            if (platform == null)
            {
                return;
            }

            recentRopePlatform = platform;
            lastRopePlatformContactTime = Time.fixedTime;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            RememberSupportingRopePlatform(collision);
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            RememberSupportingRopePlatform(collision);
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
                if (overlap == null || overlap == bodyCollider)
                {
                    continue;
                }

                if (overlap.TryGetComponent(out OneWayRailPlatform oneWayRail) &&
                    !oneWayRail.CanSupport(bodyCollider))
                {
                    continue;
                }

                isGrounded = true;
                if (overlap.TryGetComponent(
                        out GeneratedRopePlatform generatedRopePlatform))
                {
                    groundedRopePlatform = generatedRopePlatform;
                    RegisterGeneratedRopePlatformContact(generatedRopePlatform);
                }
            }

            if (!isGrounded && TryGetRecentRopePlatform(out GeneratedRopePlatform recent))
            {
                groundedRopePlatform = recent;
                isGrounded = true;
            }

            return isGrounded;
        }

        private bool ShouldUseAttachmentLaunchControl()
        {
            if (ropeController == null)
            {
                return false;
            }

            // A lower Hook can begin below the player. At that angle the
            // regular energy limiter intentionally stops further pumping,
            // which would also prevent the player from leaving the bank.
            // Keep world-left/right launch control until the player has
            // passed below the anchor and a normal pendulum arc exists.
            if (IsGrounded ||
                body.position.y >= ropeController.AnchorPoint.y)
            {
                return true;
            }

            float anchorDistance = Vector2.Distance(
                body.position,
                ropeController.AnchorPoint);
            float remainingSlack =
                ropeController.ActiveRopeLength - anchorDistance;
            return remainingSlack > attachedRopeSlackThreshold;
        }

        private void ApplyAttachmentLaunchControl()
        {
            body.linearDamping = IsGrounded ? 0f : swingLinearDamping;
            if (Mathf.Approximately(moveInput, 0f))
            {
                return;
            }

            Vector2 radiusDirection =
                (body.position - ropeController.AnchorPoint).normalized;
            Vector2 tangent =
                new Vector2(-radiusDirection.y, radiusDirection.x);
            float horizontalProjection = Vector2.Dot(
                Vector2.right * moveInput,
                tangent);
            if (Mathf.Abs(horizontalProjection) < 0.001f)
            {
                return;
            }

            float speedInRequestedDirection =
                body.linearVelocity.x * moveInput;
            if (speedInRequestedDirection >= moveSpeed)
            {
                return;
            }

            Vector2 requestedTangent =
                tangent * Mathf.Sign(horizontalProjection);

            // horizontalProjection selects which of the two tangent directions
            // matches A/D.  Do not multiply the force by that projection again:
            // at the lower Hook the tangent has only a small horizontal part,
            // and applying the factor twice leaves less force than floor
            // friction, making both keys appear unresponsive.
            body.AddForce(
                requestedTangent * acceleration,
                ForceMode2D.Force);
        }

        private void RememberSupportingRopePlatform(Collision2D collision)
        {
            if (collision == null)
            {
                return;
            }

            GeneratedRopePlatform platform =
                collision.gameObject.GetComponent<GeneratedRopePlatform>();
            if (platform == null && collision.collider != null)
            {
                platform = collision.collider.GetComponent<GeneratedRopePlatform>();
            }
            if (platform == null)
            {
                return;
            }

            float playerCenterY = bodyCollider.bounds.center.y;
            for (int i = 0; i < collision.contactCount; i++)
            {
                ContactPoint2D contact = collision.GetContact(i);
                if (contact.point.y <= playerCenterY)
                {
                    RegisterGeneratedRopePlatformContact(platform);
                    return;
                }
            }
        }

        private bool TryGetRecentRopePlatform(out GeneratedRopePlatform platform)
        {
            platform = recentRopePlatform;
            if (platform == null || body.linearVelocity.y > 0.5f)
            {
                return false;
            }

            return Time.fixedTime - lastRopePlatformContactTime <=
                ropePlatformContactGraceTime;
        }

        private void ClearRecentRopePlatform()
        {
            groundedRopePlatform = null;
            recentRopePlatform = null;
            lastRopePlatformContactTime = float.NegativeInfinity;
        }
    }
}
