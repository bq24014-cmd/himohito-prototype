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

        [Header("Landing sound")]
        [SerializeField, Min(0f)] private float minimumLandingSoundSpeed = 1.25f;

        [Header("Footsteps")]
        [SerializeField, Min(0f)] private float minimumFootstepSpeed = 0.75f;
        [SerializeField, Min(0.05f)] private float slowFootstepInterval = 0.38f;
        [SerializeField, Min(0.05f)] private float fastFootstepInterval = 0.22f;

        [Header("Solid swing collision")]
        [SerializeField, Min(0f)] private float solidSurfaceSkin = 0.01f;
        [SerializeField, Min(0.1f)] private float maximumCollisionSweepDistance = 2f;

        private Rigidbody2D body;
        private BoxCollider2D bodyCollider;
        private RopeController ropeController;
        private PrototypeAudioFeedback audioFeedback;
        private PhysicsMaterial2D movementMaterial;
        private GeneratedRopePlatform groundedRopePlatform;
        private GeneratedRopePlatform recentRopePlatform;
        private float lastRopePlatformContactTime = float.NegativeInfinity;
        private float moveInput;
        private float coyoteTimer;
        private float jumpBufferTimer;
        private Vector2 previousPhysicsPosition;
        private bool hasPreviousPhysicsPosition;
        private float previousVerticalSpeed;
        private bool hasPreviousVerticalSpeed;
        private float footstepTimer;
        private float landingFluffAirTime;
        private readonly Collider2D[] ropeEndpointHits = new Collider2D[16];
        private readonly Collider2D[] stepClearanceHits = new Collider2D[16];
        private readonly ContactPoint2D[] ropeWalkingContacts = new ContactPoint2D[32];

        public bool IsGrounded { get; private set; }
        public float MovementInput => moveInput; // Read-only input for presentation.

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            bodyCollider = GetComponent<BoxCollider2D>();
            ropeController = GetComponent<RopeController>();
            audioFeedback = GetComponent<PrototypeAudioFeedback>();
            if (audioFeedback == null)
            {
                audioFeedback = gameObject.AddComponent<PrototypeAudioFeedback>();
            }

            movementMaterial = new PhysicsMaterial2D("Player Movement Material")
            {
                friction = 0.1f,
                bounciness = 0f,
                hideFlags = HideFlags.HideAndDontSave
            };
            bodyCollider.sharedMaterial = movementMaterial;
            // Keep the collider bounds aligned with the visible character.
            // BoxCollider2D edge radius expands its outer bounds and makes the
            // sprite appear to float above flat floors.
            bodyCollider.edgeRadius = 0f;
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
            // Presentation only: a restart teleport must not look like an impact.
            bool teleported = hasPreviousPhysicsPosition &&
                Vector2.Distance(body.position, previousPhysicsPosition) > maximumCollisionSweepDistance;
            float landingImpactSpeed = 0f;
            PreventSolidSurfaceTunneling();
            bool wasGrounded = IsGrounded;
            IsGrounded = CheckGrounded();
            if (hasPreviousVerticalSpeed &&
                !wasGrounded &&
                IsGrounded &&
                previousVerticalSpeed <= -minimumLandingSoundSpeed)
            {
                audioFeedback?.PlayPlayerLanded();
                if (landingFluffAirTime >= 0.08f)
                {
                    if (!teleported) landingImpactSpeed = -previousVerticalSpeed;
                    WoodenPlatformDepthVisual.NotifyLanding(bodyCollider, -previousVerticalSpeed);
                }
            }
            landingFluffAirTime = IsGrounded ? 0f : landingFluffAirTime + Time.fixedDeltaTime;
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
                TryStepOntoRopeEndpoint();
                ApplyGroundControl();
            }
            else
            {
                ApplyAirControl();
            }

            bool jumpedThisStep = false;
            if (jumpBufferTimer > 0f && coyoteTimer > 0f && !isSwinging)
            {
                float horizontalBrakeImpulse = -body.linearVelocity.x
                    * (1f - jumpHorizontalSpeedRetention)
                    * body.mass;
                Vector2 takeoffImpulse = new Vector2(horizontalBrakeImpulse, jumpImpulse);
                body.AddForce(takeoffImpulse, ForceMode2D.Impulse);
                audioFeedback?.PlayPlayerJumped();
                GetComponent<RopeBodyVisual>()?.PlayTakeoffElasticity();
                // Coyote-time jumps happen in the air, so do not invent a floor puff.
                if (IsGrounded) RopeLandingFluff.PlayTakeoff(bodyCollider);
                ClearRecentRopePlatform();
                jumpBufferTimer = 0f;
                coyoteTimer = 0f;
                jumpedThisStep = true;
            }

            // A buffered jump uses only the takeoff flecks, not two stacked bursts.
            if (!jumpedThisStep && landingImpactSpeed > 0f)
                RopeLandingFluff.Play(bodyCollider, landingImpactSpeed);

            if (IsGrounded && !isSwinging && !jumpedThisStep && body.simulated &&
                groundedRopePlatform != null && groundedRopePlatform.IsSupportingPlayer())
                RopeBridgeStepVisual.Press(groundedRopePlatform,
                    new Vector2(bodyCollider.bounds.center.x, bodyCollider.bounds.min.y));
            UpdateFootstepAudio(isSwinging, jumpedThisStep);

            previousPhysicsPosition = body.position;
            hasPreviousPhysicsPosition = true;
            previousVerticalSpeed = body.linearVelocity.y;
            hasPreviousVerticalSpeed = true;
        }

        private void UpdateFootstepAudio(bool isSwinging, bool jumpedThisStep)
        {
            float horizontalSpeed = Mathf.Abs(body.linearVelocity.x);
            bool isWalking =
                !isSwinging &&
                !jumpedThisStep &&
                IsGrounded &&
                Mathf.Abs(moveInput) > 0.01f &&
                horizontalSpeed >= minimumFootstepSpeed;
            if (!isWalking)
            {
                footstepTimer = 0f;
                return;
            }

            footstepTimer -= Time.fixedDeltaTime;
            if (footstepTimer > 0f)
            {
                return;
            }

            audioFeedback?.PlayPlayerFootstep();
            if (groundedRopePlatform == null)
                RopeLandingFluff.PlayWoodenStep(bodyCollider);
            RopeBridgeStepVisual.Play(groundedRopePlatform,
                new Vector2(bodyCollider.bounds.center.x, bodyCollider.bounds.min.y));
            float speedRatio = Mathf.InverseLerp(
                minimumFootstepSpeed,
                moveSpeed,
                horizontalSpeed);
            footstepTimer = Mathf.Lerp(
                slowFootstepInterval,
                fastFootstepInterval,
                speedRatio);
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

            if (groundedRopePlatform != null)
            {
                if (TryGetRopeWalkingContactTangent(out Vector2 contactTangent))
                    ApplyRopePlatformGroundControl(contactTangent);
                else
                    // A probe can see the next arc while the feet have not
                    // touched it. Do not project momentum onto that arc yet.
                    ApplyAirControl();
                return;
            }

            float targetSpeed = moveInput * moveSpeed;
            float speedChange = Mathf.Approximately(moveInput, 0f)
                ? deceleration
                : acceleration;
            float nextHorizontalSpeed = Mathf.MoveTowards(
                body.linearVelocity.x,
                targetSpeed,
                speedChange * Time.fixedDeltaTime);
            body.linearVelocity = new Vector2(nextHorizontalSpeed, body.linearVelocity.y);
        }

        private void TryStepOntoRopeEndpoint()
        {
            // Only take up the small lip made by a bridge's thickness at a
            // bank. This is not general stair climbing or an airborne snap.
            if (groundedRopePlatform != null || Mathf.Abs(moveInput) < 0.01f ||
                jumpBufferTimer > 0f || body.linearVelocity.y > 0.5f)
                return;

            const float maximumRise = 0.18f;
            const float clearance = 0.04f;
            Bounds bounds = bodyCollider.bounds;
            float direction = Mathf.Sign(moveInput);
            float frontX = bounds.center.x + bounds.extents.x * direction;
            float reach = Mathf.Clamp(Mathf.Abs(body.linearVelocity.x) *
                Time.fixedDeltaTime + 0.06f, 0.14f, 0.2f);
            ContactFilter2D filter = new ContactFilter2D();
            filter.SetLayerMask(Physics2D.GetLayerCollisionMask(gameObject.layer));
            filter.useTriggers = false;
            int count = Physics2D.OverlapBox(
                new Vector2(frontX + direction * reach * 0.5f, bounds.min.y),
                new Vector2(reach + 0.04f, maximumRise * 2f),
                0f, filter, ropeEndpointHits);
            if (count == ropeEndpointHits.Length) return;

            for (int i = 0; i < count; i++)
            {
                Collider2D hit = ropeEndpointHits[i];
                if (!CanUseGroundCollider(hit) ||
                    !(hit is EdgeCollider2D edge) ||
                    !hit.TryGetComponent(out GeneratedRopePlatform platform)) continue;

                Vector2 endpoint = direction > 0f
                    ? (platform.Start.x < platform.End.x ? platform.Start : platform.End)
                    : (platform.Start.x > platform.End.x ? platform.Start : platform.End);
                float gap = (endpoint.x - frontX) * direction;
                float rise = endpoint.y + edge.edgeRadius + clearance - bounds.min.y;
                // Endpoints must be at the bank's height. Do not use this to
                // reach an elevated hook or climb the middle of a bridge.
                if (gap < -0.04f || gap > reach + edge.edgeRadius ||
                    Mathf.Abs(endpoint.y - bounds.min.y) > 0.06f ||
                    rise <= clearance || rise > maximumRise) continue;

                // Keep walls/ceilings solid and never lift through an obstacle.
                Vector2 destination = (Vector2)bounds.center + Vector2.up * rise;
                int blockers = Physics2D.OverlapBox(destination,
                    (Vector2)bounds.size - Vector2.one * 0.01f,
                    0f, filter, stepClearanceHits);
                if (blockers == stepClearanceHits.Length) return;
                bool blocked = false;
                for (int j = 0; j < blockers; j++)
                {
                    if (CanUseGroundCollider(stepClearanceHits[j]))
                    { blocked = true; break; }
                }
                if (blocked) continue;

                body.position += Vector2.up * rise;
                return;
            }
        }

        private bool CanUseGroundCollider(Collider2D other)
        {
            return other != null && other != bodyCollider && !other.isTrigger &&
                other.attachedRigidbody != body &&
                other.GetComponentInParent<HookPoint>() == null &&
                !Physics2D.GetIgnoreLayerCollision(gameObject.layer, other.gameObject.layer) &&
                !Physics2D.GetIgnoreCollision(bodyCollider, other);
        }

        private bool TryGetRopeWalkingContactTangent(out Vector2 tangent)
        {
            tangent = default;
            int count = bodyCollider.GetContacts(ropeWalkingContacts);
            bool found = false;
            float bestSupport = float.NegativeInfinity;
            bool moving = Mathf.Abs(moveInput) > 0.01f;
            for (int i = 0; i < count; i++)
            {
                ContactPoint2D contact = ropeWalkingContacts[i];
                Collider2D other = contact.collider == bodyCollider
                    ? contact.otherCollider : contact.collider;
                Vector2 normal = contact.normal;
                if (!CanUseGroundCollider(other) || normal.y < 0.45f ||
                    contact.point.y > bodyCollider.bounds.center.y) continue;

                // At a bank or shared hook, several surfaces can support the
                // player's box at once. Choose the most uphill one in the
                // requested direction: walking along it cannot push down into
                // another support. At rest prefer the flattest actual support.
                Vector2 candidate = new Vector2(normal.y, -normal.x).normalized;
                float support = moving ? candidate.y / candidate.x * Mathf.Sign(moveInput) : normal.y;
                if (found && support <= bestSupport) continue;
                found = true;
                bestSupport = support;
                tangent = candidate;
            }
            return found;
        }

        private void ApplyRopePlatformGroundControl(Vector2 tangent)
        {
            if (tangent.sqrMagnitude < 0.0001f)
            {
                return;
            }

            tangent.Normalize();
            if (tangent.x < 0f)
            {
                tangent = -tangent;
            }

            float targetSurfaceSpeed = moveInput * moveSpeed;
            float speedChange = Mathf.Approximately(moveInput, 0f)
                ? deceleration
                : acceleration;
            float currentSurfaceSpeed =
                Vector2.Dot(body.linearVelocity, tangent);
            float nextSurfaceSpeed = Mathf.MoveTowards(
                currentSurfaceSpeed,
                targetSurfaceSpeed,
                speedChange * Time.fixedDeltaTime);

            // The bridge is curved. Keeping the previous world-space vertical
            // velocity would launch the player away from the rising half after
            // passing the lowest point. While grounded, retain only velocity
            // along the local bridge surface. Jumping is applied afterwards as
            // a separate upward impulse, so this does not weaken takeoff.
            body.linearVelocity = tangent * nextSurfaceSpeed;

            if (Mathf.Approximately(moveInput, 0f))
            {
                // Keep the existing friction and the part of gravity that
                // presses the player into the bridge. Cancel only gravity's
                // component along the local slope so an idle player behaves
                // like they are held by static friction instead of creeping
                // toward the sagging bridge's lowest point every physics step.
                Vector2 gravityForce =
                    Physics2D.gravity * body.gravityScale * body.mass;
                float forceAlongSurface =
                    Vector2.Dot(gravityForce, tangent);
                body.AddForce(
                    -tangent * forceAlongSurface,
                    ForceMode2D.Force);
            }
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
                // Hook triggers must remain visible to rope targeting, but
                // touching a ring is not ground contact. In particular, a
                // ring beside a bridge end must not suppress bridge support
                // and switch slope-following back to flat-floor movement.
                if (!CanUseGroundCollider(overlap))
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
            if (platform == null)
            {
                return false;
            }

            // On a rising bridge the supporting contact moves toward the
            // lower corner of the player's box. The narrow centre ground
            // probe can briefly miss that contact, and the surface-following
            // velocity legitimately has a positive Y component. Keep ground
            // control while the bridge is physically supporting the player;
            // once a jump separates the colliders, the upward-velocity guard
            // still releases the player into normal air control.
            if (platform.IsSupportingPlayer())
            {
                return true;
            }

            if (body.linearVelocity.y > 0.5f)
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
