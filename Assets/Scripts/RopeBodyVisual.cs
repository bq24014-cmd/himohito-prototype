using UnityEngine;

namespace HimoHito
{
    /// <summary>
    /// Shrinks only the player's visible body as rope is spent.
    /// The player transform, Rigidbody2D, and Collider2D keep their original size.
    /// </summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-50)] // Pose/crown updates before RopeController draws its line.
    [RequireComponent(typeof(RopeResource), typeof(SpriteRenderer))]
    public sealed class RopeBodyVisual : MonoBehaviour
    {
        private const string PlayerArtResourcePath = "Art/HimoHitoPlayer-v1";
        private const string WalkArtResourcePath = "Art/HimoHitoWalk-v3";
        private const string JumpArtResourcePath = "Art/HimoHitoJump-v2";
        private const string LandingArtResourcePath = "Art/HimoHitoLanding-v1";
        private const string SwingArtResourcePath = "Art/HimoHitoSwing-v2";
        private const string GroundTransitionArtResourcePath = "Art/HimoHitoWalkTransitions-v1";
        private const string IdleArtResourcePath = "Art/HimoHitoIdle-v1";
        private const string AimArtResourcePath = "Art/HimoHitoAim-v1";
        private const string EdgeArtResourcePath = "Art/HimoHitoEdgeBalance-v1";
        private const string WeaveArtResourcePath = "Art/HimoHitoWeave-v1";
        private const string GoalArtResourcePath = "Art/HimoHitoGoal-v1";
        private const float GoalGestureDuration = 0.45f;
        private const int SwingFrameCount = 6;
        private const int WalkColumns = 4;
        private const int WalkRows = 2;
        private const int WalkFrameCount = WalkColumns * WalkRows;
        private const int JumpColumns = 3;
        private const int JumpRows = 2;
        private const int JumpFrameCount = JumpColumns * JumpRows;
        private const int LandingColumns = 3;
        private const int LandingRows = 2;
        private const int LandingFrameCount = LandingColumns * LandingRows;
        private const float SpritePixelsPerUnit = 100f;
        private const float CharacterVisualWidth = 0.9f;
        private const float CharacterVisualHeight = 1.15f;

        private static Sprite[] cachedWalkFrames;
        private static Vector2 cachedWalkContentSize;
        private static Sprite[] cachedJumpFrames;
        private static Vector2 cachedJumpContentSize;
        private static Sprite[] cachedLandingFrames;
        private static Vector2 cachedLandingContentSize;
        private static Sprite[] cachedSwingFrames;
        private static Vector2 cachedSwingContentSize;
        private static Sprite[] cachedGroundTransitionFrames;
        private static Vector2 cachedGroundTransitionContentSize;
        private static Sprite[] cachedIdleFrames;
        private static Vector2 cachedIdleContentSize;
        private static Sprite[] cachedAimFrames;
        private static Vector2 cachedAimContentSize;
        private static Sprite[] cachedEdgeFrames;
        private static Vector2 cachedEdgeContentSize;
        private static readonly System.Collections.Generic.Dictionary<Sprite, RopePoseParts> PoseParts =
            new System.Collections.Generic.Dictionary<Sprite, RopePoseParts>();
        private static Sprite[] cachedWeaveFrames;
        private static Vector2 cachedWeaveContentSize;
        private static Sprite[] cachedGoalFrames;
        private static Vector2 cachedGoalContentSize;
        private static readonly System.Collections.Generic.Dictionary<Sprite, Vector3> FrameCrownPoints =
            new System.Collections.Generic.Dictionary<Sprite, Vector3>();
        private static readonly System.Collections.Generic.Dictionary<Sprite, float> IdleFootOffsets =
            new System.Collections.Generic.Dictionary<Sprite, float>();
        private static readonly System.Collections.Generic.Dictionary<Sprite, float> FrameHeadWidths =
            new System.Collections.Generic.Dictionary<Sprite, float>();
        private static readonly System.Collections.Generic.Dictionary<Sprite, float> GroundFrameScale =
            new System.Collections.Generic.Dictionary<Sprite, float>();
        private static readonly System.Collections.Generic.Dictionary<Sprite, float> SwingFrameScale =
            new System.Collections.Generic.Dictionary<Sprite, float>();

        [SerializeField, Range(0.2f, 1f)] private float minimumVisualScale = 0.65f;

        [Header("Walking animation")]
        [SerializeField, Min(0.01f)] private float minimumWalkSpeed = 0.35f;
        [SerializeField, Min(1f)] private float minimumWalkFramesPerSecond = 5f;
        [SerializeField, Min(1f)] private float maximumWalkFramesPerSecond = 9f;
        [SerializeField, Min(0.1f)] private float fullWalkAnimationSpeed = 6.5f;

        [Header("Jump animation")]
        [SerializeField, Min(0f)] private float minimumJumpAnimationSpeed = 0.75f;
        [SerializeField, Min(0f)] private float launchFrameDuration = 0.08f;
        [SerializeField, Min(0.1f)] private float fastRiseSpeed = 4f;
        [SerializeField, Min(0f)] private float apexSpeed = 0.8f;
        [SerializeField, Min(0.1f)] private float fastFallSpeed = 4f;

        [Header("Swing visual rotation")]
        [SerializeField, Range(0f, 1f)]
        [Tooltip("How strongly the visible body follows the rope angle.")]
        private float ropeFollowAmount = 0.72f;

        [SerializeField, Min(0.01f)]
        [Tooltip("Seconds used to ease the visible body toward the rope angle.")]
        private float swingRotationSmoothTime = 0.09f;

        [SerializeField, Min(0.01f)]
        [Tooltip("Seconds used to ease the visible body upright after release.")]
        private float releaseRotationSmoothTime = 0.20f;

        [SerializeField, Range(0f, 90f)]
        [Tooltip("Maximum visible tilt. The Rigidbody2D and Collider2D never rotate.")]
        private float maximumVisualRotation = 78f;

        [SerializeField, Min(0f)]
        [Tooltip("Extra visual lag in degrees for each unit of tangential speed.")]
        private float inertiaLeanAmount = 0.65f;

        [SerializeField, Range(0f, 15f)]
        [Tooltip("Maximum extra visual lag caused by pendulum momentum.")]
        private float maximumInertiaLean = 7f;

        [SerializeField, Min(0f)]
        [Tooltip("Horizontal speed required before normal movement may change facing.")]
        private float facingDeadZone = 0.35f;

        [Header("Landing animation")]
        [SerializeField, Min(0.05f)] private float landingDuration = 0.20f;
        [SerializeField, Min(0f)] private float minimumLandingSpeed = 1.5f;

        private RopeResource ropeResource;
        private RopeController ropeController;
        private PlayerMover playerMover;
        private Rigidbody2D body;
        private SpriteRenderer sourceRenderer;
        private SpriteRenderer visualRenderer;
        private Transform visualTransform;
        private Vector2 visualBaseScale = Vector2.one;
        private Vector2 standingVisualBaseScale = Vector2.one;
        private Vector2 walkingVisualBaseScale = Vector2.one;
        private Vector2 jumpingVisualBaseScale = Vector2.one;
        private Vector2 landingVisualBaseScale = Vector2.one;
        private Vector2 swingingVisualBaseScale = Vector2.one;
        private Vector2 groundTransitionBaseScale = Vector2.one;
        private Vector2 transitionWalkScaleRatio = Vector2.one;
        private Vector2 idleVisualBaseScale = Vector2.one;
        private Vector2 aimVisualBaseScale = Vector2.one;
        private Vector2 weaveVisualBaseScale = Vector2.one;
        private Vector3 standingRopeLocalPoint;
        private Sprite standingSprite;
        private Sprite[] walkingFrames;
        private Sprite[] jumpingFrames;
        private Sprite[] landingFrames;
        private Sprite[] swingingFrames;
        private Sprite[] groundTransitionFrames;
        private Sprite[] idleFrames;
        private Sprite[] aimFrames;
        private readonly AimPoseState aimPose = new AimPoseState();
        private bool isAimPoseDisplayed;
        private Sprite[] edgeFrames;
        private bool edgeArtReady;
        private Vector2 edgeBodyBaseScale;
        private SpriteRenderer balanceBodyRenderer;
        private Collider2D edgePlayerCollider;
        private readonly EdgeBalancePoseState edgePose = new EdgeBalancePoseState();
        private readonly EdgeBalanceSupportProbe edgeProbe = new EdgeBalanceSupportProbe();
        private Sprite[] weaveFrames;
        private readonly WeavePoseState weavePose = new WeavePoseState();
        private bool isWeavePoseDisplayed;
        private Sprite[] goalFrames;
        private readonly GoalPoseState goalPose = new GoalPoseState();
        private Vector2 goalNeutralBaseScale;
        private double goalPoseStartedAt;
        private readonly IdlePoseState idlePose = new IdlePoseState();
        private bool isIdlePoseDisplayed;
        private readonly GroundMotionPoseState groundMotionPose = new GroundMotionPoseState();
        private bool groundTransitionWasDisplayed;
        private int groundTransitionSequence;
        private float groundTransitionStartWalkWeight;
        private float groundTransitionWalkWeight;
        private readonly RopeSwingPoseState swingPose = new RopeSwingPoseState();
        private bool isSwingPoseDisplayed;
        private readonly SwingAttachPoseState swingAttachPose = new SwingAttachPoseState();
        private Vector3 attachInitialCrown;
        private Vector2 attachInitialHeadScale;
        private int visualAttachmentSequence;
        private readonly SwingReleasePoseState swingReleasePose = new SwingReleasePoseState();
        private bool isSwingReleasePoseDisplayed;
        private Vector3 releaseHeadOrigin;
        private Vector2 releaseInitialHeadScale;
        private bool usesCharacterArt;
        private bool isWalking;
        private bool isJumpingVisually;
        private float currentBaseScale = 1f;
        private float walkFrameProgress;
        private float airborneElapsed;
        private float fastestFallSpeed;
        private float landingElapsed;
        private float currentVisualAngle;
        private float visualAngleVelocity;
        private bool wasGrounded;
        private bool wasRopeAttached;
        private bool swingFacingLeft;
        private bool isReturningFromSwing;

        private static readonly System.Collections.Generic.Dictionary<Sprite, Vector3> HeadPoints =
            new System.Collections.Generic.Dictionary<Sprite, Vector3>();
        private static readonly System.Collections.Generic.Dictionary<Sprite, Vector3> PoseHeadCenters =
            new System.Collections.Generic.Dictionary<Sprite, Vector3>();

        public Vector2 HeadReturnPoint
        {
            get
            {
                if (visualRenderer == null || visualRenderer.sprite == null) return transform.position;
                Sprite sprite = visualRenderer.sprite;
                if (!HeadPoints.TryGetValue(sprite, out Vector3 point))
                    point = new Vector3(sprite.bounds.center.x,
                        sprite.bounds.max.y - sprite.bounds.size.y * 0.16f, 0f);
                if (visualRenderer.flipX) point.x = -point.x;
                return visualTransform.TransformPoint(point);
            }
        }

        public Vector2 RopeOrigin
        {
            get
            {
                if (usesCharacterArt && visualRenderer != null)
                {
                    // Swing cells pivot at the drawn crown, not the padded cell
                    // centre. The line must use that same attachment point.
                    if (isSwingPoseDisplayed) return visualTransform.position;
                    if ((isIdlePoseDisplayed || isSwingReleasePoseDisplayed || isWeavePoseDisplayed || isAimPoseDisplayed) &&
                        FrameCrownPoints.TryGetValue(visualRenderer.sprite, out Vector3 crown))
                    {
                        if (visualRenderer.flipX) crown.x = -crown.x;
                        return visualTransform.TransformPoint(crown);
                    }
                    return visualTransform.TransformPoint(standingRopeLocalPoint);
                }

                return transform.position;
            }
        }

        private void Awake()
        {
            ropeResource = GetComponent<RopeResource>();
            ropeController = GetComponent<RopeController>();
            playerMover = GetComponent<PlayerMover>();
            body = GetComponent<Rigidbody2D>();
            edgePlayerCollider = GetComponent<Collider2D>();
            sourceRenderer = GetComponent<SpriteRenderer>();
            CreateVisualBody();
            UpdateRemainingLengthScale();
            landingElapsed = landingDuration;
            wasGrounded = playerMover != null && playerMover.IsGrounded;
            wasRopeAttached = ropeController != null && ropeController.IsAttached;
            swingFacingLeft = visualRenderer != null && visualRenderer.flipX;
            ApplyVisualScale();
        }

        private void LateUpdate()
        {
            PrepareSwingAttachment();
            if (visualTransform != null)
                visualTransform.localPosition -= elasticityOffset;
            elasticityOffset = Vector3.zero;
            UpdateLandingState();

            if (ropeController == null || !ropeController.IsAttached)
            {
                UpdateRemainingLengthScale();
            }

            UpdateCharacterAnimation();
            ApplyVisualScale();
            landingElapsed = Mathf.Min(
                landingDuration,
                landingElapsed + Time.deltaTime);
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

            visualRenderer = visualObject.AddComponent<SpriteRenderer>();
            visualRenderer.sortingLayerID = sourceRenderer.sortingLayerID;
            visualRenderer.sortingOrder = sourceRenderer.sortingOrder;
            visualRenderer.maskInteraction = sourceRenderer.maskInteraction;

            Sprite playerArt =
                TutorialFirstSectionVisuals.LoadProcessedToySprite(
                    PlayerArtResourcePath);
            if (playerArt != null)
            {
                visualRenderer.sprite = playerArt;
                visualRenderer.color = Color.white;
                Vector2 spriteSize = playerArt.bounds.size;
                visualBaseScale = new Vector2(
                    CharacterVisualWidth / Mathf.Max(0.01f, spriteSize.x),
                    CharacterVisualHeight / Mathf.Max(0.01f, spriteSize.y));
                standingVisualBaseScale = visualBaseScale;
                standingSprite = playerArt;
                standingRopeLocalPoint = new Vector3(
                    playerArt.bounds.center.x,
                    playerArt.bounds.max.y,
                    0f);
                walkingFrames = LoadWalkFrames(out Vector2 walkContentSize);
                if (walkingFrames != null && walkContentSize.sqrMagnitude > 0f)
                {
                    walkingVisualBaseScale = new Vector2(
                        CharacterVisualWidth / Mathf.Max(0.01f, walkContentSize.x),
                        CharacterVisualHeight / Mathf.Max(0.01f, walkContentSize.y));
                }
                jumpingFrames = LoadJumpFrames(out Vector2 jumpContentSize);
                if (jumpingFrames != null && jumpContentSize.sqrMagnitude > 0f)
                {
                    jumpingVisualBaseScale = new Vector2(
                        CharacterVisualWidth / Mathf.Max(0.01f, jumpContentSize.x),
                        CharacterVisualHeight / Mathf.Max(0.01f, jumpContentSize.y));
                }
                landingFrames = LoadLandingFrames(out Vector2 landingContentSize);
                if (landingFrames != null && landingContentSize.sqrMagnitude > 0f)
                {
                    landingVisualBaseScale = new Vector2(
                        CharacterVisualWidth / Mathf.Max(0.01f, landingContentSize.x),
                        CharacterVisualHeight / Mathf.Max(0.01f, landingContentSize.y));
                }
                swingingFrames = LoadAnimationFrames(
                    SwingArtResourcePath, "Swing", 3, 2, false,
                    ref cachedSwingFrames, ref cachedSwingContentSize,
                    out Vector2 swingContentSize, true);
                if (swingingFrames != null && swingContentSize.sqrMagnitude > 0f)
                {
                    swingingVisualBaseScale = new Vector2(
                        CharacterVisualWidth / swingContentSize.x,
                        CharacterVisualHeight / swingContentSize.y);
                }
                groundTransitionFrames = LoadAnimationFrames(
                    GroundTransitionArtResourcePath, "Ground transition", 3, 2, true,
                    ref cachedGroundTransitionFrames, ref cachedGroundTransitionContentSize,
                    out Vector2 groundContentSize, false, true);
                if (groundTransitionFrames != null && groundContentSize.sqrMagnitude > 0f)
                {
                    groundTransitionBaseScale = new Vector2(
                        CharacterVisualWidth / groundContentSize.x,
                        CharacterVisualHeight / groundContentSize.y);
                    if (walkingFrames != null && FrameHeadWidths.TryGetValue(walkingFrames[0], out float headWidth))
                    {
                        // Existing walking art has its own normalization. Ease
                        // into that size, rather than popping at the hand-off.
                        transitionWalkScaleRatio = new Vector2(
                            walkingVisualBaseScale.x * headWidth / CharacterVisualWidth,
                            walkingVisualBaseScale.y * headWidth /
                                (groundTransitionBaseScale.y * groundContentSize.x));
                    }
                }
                idleFrames = LoadAnimationFrames(
                    IdleArtResourcePath, "Idle", 3, 2, true,
                    ref cachedIdleFrames, ref cachedIdleContentSize,
                    out Vector2 idleContentSize, false, true, true);
                if (idleFrames != null && idleContentSize.sqrMagnitude > 0f)
                {
                    idleVisualBaseScale = new Vector2(
                        CharacterVisualWidth / idleContentSize.x,
                        CharacterVisualHeight / idleContentSize.y);
                }
                aimFrames = LoadAnimationFrames(
                    AimArtResourcePath, "Aim", 3, 2, true,
                    ref cachedAimFrames, ref cachedAimContentSize,
                    out Vector2 aimContentSize, false, true, true, true);
                if (aimFrames != null && aimContentSize.sqrMagnitude > 0f)
                {
                    aimVisualBaseScale = new Vector2(
                        CharacterVisualWidth / aimContentSize.x,
                        CharacterVisualHeight / aimContentSize.y);
                }
                weaveFrames = LoadAnimationFrames(
                    WeaveArtResourcePath, "Weave", 3, 2, true,
                    ref cachedWeaveFrames, ref cachedWeaveContentSize,
                    out Vector2 weaveContentSize, false, true, true);
                if (weaveFrames != null && weaveContentSize.sqrMagnitude > 0f)
                {
                    weaveVisualBaseScale = new Vector2(
                        CharacterVisualWidth / weaveContentSize.x,
                        CharacterVisualHeight / weaveContentSize.y);
                }
                goalFrames = LoadAnimationFrames(
                    GoalArtResourcePath, "Goal", 3, 2, true,
                    ref cachedGoalFrames, ref cachedGoalContentSize,
                    out _, false, true, true);
                edgeFrames = LoadAnimationFrames(
                    EdgeArtResourcePath, "Edge balance", 3, 2, true,
                    ref cachedEdgeFrames, ref cachedEdgeContentSize,
                    out _, false, true, true, true);
                edgeArtReady = HasPoseParts(aimFrames) && HasPoseParts(edgeFrames);
                if (edgeArtReady)
                {
                    // Register the new body's neutral neck height to the
                    // approved aim body once. Head proportions never change.
                    edgeBodyBaseScale = aimVisualBaseScale *
                        (FrameHeadWidths[aimFrames[5]] / FrameHeadWidths[edgeFrames[5]]);
                    edgeBodyBaseScale.y = aimVisualBaseScale.y *
                        PoseParts[aimFrames[5]].Neck.y / PoseParts[edgeFrames[5]].Neck.y;
                    GameObject bodyObject = new GameObject("Edge Balance Body Visual");
                    bodyObject.layer = gameObject.layer;
                    bodyObject.transform.SetParent(transform, false);
                    balanceBodyRenderer = bodyObject.AddComponent<SpriteRenderer>();
                    balanceBodyRenderer.sortingLayerID = visualRenderer.sortingLayerID;
                    balanceBodyRenderer.sortingOrder = visualRenderer.sortingOrder - 1;
                    balanceBodyRenderer.maskInteraction = visualRenderer.maskInteraction;
                    balanceBodyRenderer.enabled = false;
                }
                else Debug.LogWarning("Edge balance artwork could not be registered; normal player poses remain enabled.");
                usesCharacterArt = true;
            }
            else
            {
                SolidSprite solidSprite = visualObject.AddComponent<SolidSprite>();
                solidSprite.Color = sourceRenderer.color;
            }

            sourceRenderer.enabled = false;
        }

        private void UpdateCharacterAnimation()
        {
            // Goals already freeze physics and use a realtime clear delay.
            // Only this explicitly triggered finish follows that clock.
            if (TryShowGoalPose()) return;
            // Freeze all poses on a paused clock, including rotation smoothing.
            if (Application.isPlaying && Time.deltaTime <= 0f) return;
            if (balanceBodyRenderer != null) balanceBodyRenderer.enabled = false;
            bool isAttached = ropeController != null && ropeController.IsAttached;
            bool isGrounded = playerMover == null || playerMover.IsGrounded;
            UpdateSwingState(isAttached);
            isSwingPoseDisplayed = false;
            isIdlePoseDisplayed = false;
            isSwingReleasePoseDisplayed = false;
            isWeavePoseDisplayed = false;
            isAimPoseDisplayed = false;
            swingReleasePose.Advance(Time.deltaTime,
                playerMover != null && playerMover.enabled && body != null && body.simulated &&
                !isGrounded && !isAttached);

            if (isAttached || isGrounded)
            {
                isJumpingVisually = false;
            }
            else if (!isJumpingVisually && body != null)
            {
                isJumpingVisually =
                    Mathf.Abs(body.linearVelocity.y) >= minimumJumpAnimationSpeed;
            }

            bool shouldShowJump =
                usesCharacterArt &&
                jumpingFrames != null &&
                jumpingFrames.Length == JumpFrameCount &&
                isJumpingVisually;

            bool shouldShowSwing =
                usesCharacterArt &&
                standingSprite != null &&
                isAttached &&
                !isGrounded;

            bool shouldShowLanding =
                usesCharacterArt &&
                landingFrames != null &&
                landingFrames.Length == LandingFrameCount &&
                landingElapsed < landingDuration &&
                isGrounded &&
                !isAttached;

            weavePose.Advance(Time.deltaTime, CanShowWeavePose() && !shouldShowLanding);

            bool canShowGroundTransition = !weavePose.IsActive && playerMover != null && playerMover.enabled &&
                body != null && body.simulated && isGrounded && !isAttached &&
                !shouldShowJump && !shouldShowLanding && !isReturningFromSwing &&
                Time.time - takeoffStartedAt >= 0.10f;
            groundMotionPose.Advance(playerMover != null ? playerMover.MovementInput : 0f,
                body != null ? body.linearVelocity.x : 0f, Time.deltaTime, canShowGroundTransition);
            if (!canShowGroundTransition) groundTransitionWasDisplayed = false;
            bool canShowAim = canShowGroundTransition && !groundMotionPose.IsActive &&
                aimFrames != null && aimFrames.Length == 6 && ropeController != null && ropeController.enabled &&
                Mathf.Abs(playerMover.MovementInput) < 0.01f && body.linearVelocity.sqrMagnitude < 0.0225f &&
                !Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D) && !Input.GetButton("Jump") &&
                !Input.GetKey(KeyCode.R) && Time.time - softLandingStartedAt >= 0.4f;
            Vector2 aimDirection = ropeController != null ? ropeController.KeyboardAimDirection : Vector2.up;
            aimPose.Advance(aimDirection.x, aimDirection.y,
                visualRenderer != null && visualRenderer.flipX, Time.deltaTime, canShowAim);
            bool canShowEdge = canShowAim && edgeArtReady && aimPose.IsVisible;
            edgePose.Advance(canShowEdge ? edgeProbe.FindOpenSide(edgePlayerCollider) : 0,
                Time.deltaTime, canShowEdge);
            // Existing idle drawings look horizontally. Only use them when
            // they agree with the guide; never replace an up/down gaze on a timer.
            bool canShowIdle = canShowGroundTransition && !groundMotionPose.IsActive && !edgePose.IsVisible &&
                (!aimPose.IsVisible || aimPose.IsLookingForward) &&
                Mathf.Abs(playerMover.MovementInput) < 0.01f &&
                body.linearVelocity.sqrMagnitude < 0.0225f && !Input.anyKey &&
                Time.time - softLandingStartedAt >= 0.4f;
            idlePose.Advance(Time.deltaTime, canShowIdle);

            if (shouldShowSwing)
            {
                UpdateSwingAnimation();
                return;
            }

            swingPose.Reset();

            UpdateUprightVisualRotation(isAttached);

            if (shouldShowLanding)
            {
                UpdateLandingAnimation();
                return;
            }

            if (TryShowSwingRelease()) return;

            if (shouldShowJump)
            {
                UpdateJumpAnimation();
                return;
            }

            airborneElapsed = 0f;
            if (TryShowWeavePose()) return;
            if (TryShowGroundTransition()) return;
            if (TryShowEdgeBalance()) return;
            if (TryShowAimPose()) return;
            if (TryShowIdle()) return;
            UpdateWalkAnimation();
        }

        public void PlayGoalPose(float revealDuration, Transform chest)
        {
            if (!usesCharacterArt || goalFrames == null || goalFrames.Length != 6 ||
                visualRenderer == null || visualRenderer.sprite == null ||
                !isActiveAndEnabled || body == null || body.simulated) return;
            bool faceLeft = visualRenderer.flipX;
            if (chest != null && Mathf.Abs(chest.position.x - transform.position.x) > 0.05f)
                faceLeft = chest.position.x < transform.position.x;
            // Extra clear-screen delay holds the final pose; it must not turn
            // the six drawings into a slow, choppy celebration.
            if (!goalPose.Begin(Mathf.Min(revealDuration, GoalGestureDuration), faceLeft)) return;
            // Collision callbacks can run on the fixed-step clock. Use the
            // same realtime clock as the owner's WaitForSecondsRealtime.
            goalPoseStartedAt = Time.realtimeSinceStartupAsDouble;
            // Keep the head size already on screen, including the current rope
            // body tier. Do not enlarge a nearly unravelled character on clear.
            float previousHeadWidth = FrameHeadWidths.TryGetValue(visualRenderer.sprite, out float width)
                ? width : visualRenderer.sprite.bounds.size.x;
            goalNeutralBaseScale = visualBaseScale *
                (previousHeadWidth / FrameHeadWidths[goalFrames[5]]);
            swingReleasePose.Reset();
            weavePose.Reset();
            swingAttachPose.Reset();
            groundMotionPose.Reset();
            idlePose.Reset();
            aimPose.Reset();
            ClearEdgeBalance();
        }

        private bool TryShowGoalPose()
        {
            if (!goalPose.IsVisible) return false;
            goalPose.Sample((float)(Time.realtimeSinceStartupAsDouble - goalPoseStartedAt),
                isActiveAndEnabled && body != null && !body.simulated);
            if (!goalPose.IsVisible) return false;
            Sprite frame = goalFrames[goalPose.FrameIndex];
            if (balanceBodyRenderer != null) balanceBodyRenderer.enabled = false;
            visualRenderer.sprite = frame;
            visualRenderer.flipX = goalPose.FacingLeft;
            visualBaseScale = goalNeutralBaseScale * GroundFrameScale[frame];
            float footCorrection = IdleFootOffsets[frame] * goalNeutralBaseScale.x * currentBaseScale;
            if (goalPose.FacingLeft) footCorrection = -footCorrection;
            // Goal entry requires a top-surface contact. Keep the drawn feet at
            // the same floor height as walking, even at the smallest rope tier.
            visualTransform.localPosition = new Vector3(footCorrection,
                -CharacterVisualHeight * 0.5f, 0f);
            visualTransform.localRotation = Quaternion.identity;
            currentVisualAngle = visualAngleVelocity = 0f;
            isSwingPoseDisplayed = isSwingReleasePoseDisplayed = false;
            isIdlePoseDisplayed = isWeavePoseDisplayed = false;
            isAimPoseDisplayed = false;
            isWalking = isJumpingVisually = isReturningFromSwing = false;
            walkFrameProgress = 0f;
            return true;
        }

        public void PlayWeavePose()
        {
            // Called only after successful creation and placement, never on raw
            // Q input or checkpoint restoration. Airborne builds are not queued.
            weavePose.Begin(CanShowWeavePose() && !isSwingPoseDisplayed &&
                landingElapsed >= landingDuration && Mathf.Abs(currentVisualAngle) < 0.1f,
                visualRenderer != null && visualRenderer.flipX);
        }

        private bool CanShowWeavePose()
        {
            return usesCharacterArt && weaveFrames != null && weaveFrames.Length == 6 &&
                visualRenderer != null && isActiveAndEnabled && Time.timeScale > 0f &&
                playerMover != null && playerMover.enabled && playerMover.IsGrounded &&
                body != null && body.simulated && body.linearVelocity.sqrMagnitude < 0.1225f &&
                (ropeController == null || !ropeController.IsAttached) &&
                Mathf.Abs(playerMover.MovementInput) < 0.01f &&
                !Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D) &&
                !Input.GetButton("Jump") && !Input.GetKey(KeyCode.R) &&
                Time.time - takeoffStartedAt >= 0.1f;
        }

        private bool TryShowWeavePose()
        {
            if (!weavePose.IsActive) return false;
            Sprite frame = weaveFrames[weavePose.FrameIndex];
            visualRenderer.sprite = frame;
            visualRenderer.flipX = weavePose.FacingLeft;
            visualBaseScale = weaveVisualBaseScale * GroundFrameScale[frame];
            float footCorrection = IdleFootOffsets[frame] * weaveVisualBaseScale.x * currentBaseScale;
            if (weavePose.FacingLeft) footCorrection = -footCorrection;
            visualTransform.localPosition = new Vector3(footCorrection,
                -CharacterVisualHeight * 0.5f * currentBaseScale, 0f);
            visualTransform.localRotation = Quaternion.identity;
            isWeavePoseDisplayed = true;
            isWalking = false;
            walkFrameProgress = 0f;
            return true;
        }

        private void OnDisable() => CancelActionPoses();

        private static bool HasPoseParts(Sprite[] frames)
        {
            if (frames == null || frames.Length != 6) return false;
            foreach (Sprite frame in frames) if (!PoseParts.ContainsKey(frame)) return false;
            return true;
        }

        private void ClearEdgeBalance()
        {
            edgePose.Reset();
            if (balanceBodyRenderer != null) balanceBodyRenderer.enabled = false;
        }

        private bool TryShowEdgeBalance()
        {
            if (!edgePose.IsVisible || !aimPose.IsVisible || !edgeArtReady) return false;
            Sprite lowerFrame = edgeFrames[edgePose.FrameIndex];
            Sprite aimedFrame = aimFrames[aimPose.FrameIndex];
            RopePoseParts lower = PoseParts[lowerFrame];
            RopePoseParts upper = PoseParts[aimedFrame];
            bool edgeOnLeft = edgePose.EdgeSide < 0;
            Vector2 lowerScale = edgeBodyBaseScale * GroundFrameScale[lowerFrame] * currentBaseScale;
            float footCorrection = IdleFootOffsets[lowerFrame] * edgeBodyBaseScale.x * currentBaseScale;
            if (edgeOnLeft) footCorrection = -footCorrection;
            Vector3 feet = new Vector3(footCorrection, -CharacterVisualHeight * .5f * currentBaseScale, 0f);
            Transform lowerTransform = balanceBodyRenderer.transform;
            balanceBodyRenderer.sprite = lower.Body;
            balanceBodyRenderer.flipX = edgeOnLeft;
            balanceBodyRenderer.enabled = true;
            lowerTransform.localPosition = feet;
            lowerTransform.localRotation = Quaternion.identity;
            lowerTransform.localScale = new Vector3(lowerScale.x, lowerScale.y, 1f);

            // The body shifts away from the drop, independently of gaze. Keep
            // the existing aim head/tuft and attach it at the drawn neck joint.
            Vector3 neck = lower.Neck;
            if (edgeOnLeft) neck.x = -neck.x;
            visualRenderer.sprite = upper.Head;
            visualRenderer.flipX = aimPose.FacingLeft;
            visualBaseScale = aimVisualBaseScale * GroundFrameScale[aimedFrame];
            visualTransform.localPosition = feet + Vector3.Scale(neck, lowerTransform.localScale);
            visualTransform.localRotation = Quaternion.identity;
            isAimPoseDisplayed = true;
            isWalking = false;
            walkFrameProgress = 0f;
            return true;
        }

        private bool TryShowAimPose()
        {
            if (!aimPose.IsVisible) return false;
            // Idle may blink while looking forward, but its facing must also
            // follow the guide (including a changed aim after stopping).
            visualRenderer.flipX = aimPose.FacingLeft;
            if (aimPose.IsLookingForward && idlePose.IsVisible) return false;
            Sprite frame = aimFrames[aimPose.FrameIndex];
            visualRenderer.sprite = frame;
            visualBaseScale = aimVisualBaseScale * GroundFrameScale[frame];
            float footCorrection = IdleFootOffsets[frame] * aimVisualBaseScale.x * currentBaseScale;
            if (aimPose.FacingLeft) footCorrection = -footCorrection;
            visualTransform.localPosition = new Vector3(footCorrection,
                -CharacterVisualHeight * 0.5f * currentBaseScale, 0f);
            visualTransform.localRotation = Quaternion.identity;
            isAimPoseDisplayed = true;
            isWalking = false;
            walkFrameProgress = 0f;
            return true;
        }

        private bool TryShowIdle()
        {
            if (!idlePose.IsVisible || idleFrames == null || idleFrames.Length != 6 ||
                visualRenderer == null) return false;
            Sprite frame = idleFrames[idlePose.FrameIndex];
            visualRenderer.sprite = frame;
            visualBaseScale = idleVisualBaseScale * GroundFrameScale[frame];
            float footCorrection = IdleFootOffsets[frame] * idleVisualBaseScale.x * currentBaseScale;
            if (visualRenderer.flipX) footCorrection = -footCorrection;
            visualTransform.localPosition = new Vector3(footCorrection,
                -CharacterVisualHeight * 0.5f * currentBaseScale, 0f);
            // Pose changes are drawn into the cells. No procedural full-body
            // rocking, pulsing, or rotation is added to a planted character.
            visualTransform.localRotation = Quaternion.identity;
            isIdlePoseDisplayed = true;
            isWalking = false;
            walkFrameProgress = 0f;
            return true;
        }

        private bool TryShowGroundTransition()
        {
            if (!groundMotionPose.IsActive || groundTransitionFrames == null ||
                groundTransitionFrames.Length != 6 || visualRenderer == null)
            {
                groundTransitionWasDisplayed = false;
                return false;
            }

            if (!groundTransitionWasDisplayed || groundTransitionSequence != groundMotionPose.SequenceId)
            {
                groundTransitionStartWalkWeight = groundTransitionWasDisplayed
                    ? groundTransitionWalkWeight : isWalking ? 1f : 0f;
                groundTransitionSequence = groundMotionPose.SequenceId;
            }
            groundTransitionWasDisplayed = true;

            Sprite frame = groundTransitionFrames[groundMotionPose.FrameIndex];
            float blend = Mathf.SmoothStep(0f, 1f, groundMotionPose.Progress);
            float walkWeight = Mathf.Lerp(groundTransitionStartWalkWeight,
                groundMotionPose.IsStarting ? 1f : 0f, blend);
            groundTransitionWalkWeight = walkWeight;
            Vector2 scaleRatio = Vector2.Lerp(Vector2.one, transitionWalkScaleRatio, walkWeight);
            visualBaseScale = Vector2.Scale(groundTransitionBaseScale * GroundFrameScale[frame], scaleRatio);
            visualRenderer.sprite = frame;
            visualRenderer.flipX = groundMotionPose.FacingLeft;
            // Feet are the pivot for these six cells. Blend between the existing
            // idle and walk foot heights (including the current rope body size).
            float footHeight = Mathf.Lerp(-CharacterVisualHeight * 0.5f * currentBaseScale,
                -CharacterVisualHeight * 0.5f, walkWeight);
            visualTransform.localPosition = new Vector3(0f, footHeight, 0f);
            visualTransform.localRotation = Quaternion.Euler(0f, 0f,
                currentVisualAngle + groundMotionPose.LeanDegrees);
            isWalking = false;
            walkFrameProgress = 0f;
            return true;
        }

        public void PlaySwingReleasePose()
        {
            bool eligible = usesCharacterArt && isSwingPoseDisplayed &&
                visualRenderer != null && visualTransform != null &&
                swingingFrames != null && swingingFrames.Length == SwingFrameCount &&
                jumpingFrames != null && jumpingFrames.Length == JumpFrameCount &&
                playerMover != null && playerMover.enabled && !playerMover.IsGrounded &&
                body != null && body.simulated && Time.timeScale > 0f;
            swingReleasePose.Begin(swingPose.FrameIndex, swingFacingLeft, eligible);
            if (!swingReleasePose.IsActive) return;
            // Capture the currently drawn head in the player's local, unrotated
            // space. Changing pivot/sprite later must not teleport the head.
            Vector3 headPoint = PoseHeadCenters[visualRenderer.sprite];
            if (visualRenderer.flipX) headPoint.x = -headPoint.x;
            releaseHeadOrigin = Quaternion.Inverse(visualTransform.localRotation) *
                visualTransform.localPosition + Vector3.Scale(headPoint, visualTransform.localScale);
            releaseInitialHeadScale = new Vector2(visualTransform.localScale.x, visualTransform.localScale.y) *
                (FrameHeadWidths[visualRenderer.sprite] / Mathf.Max(0.01f, currentBaseScale));
            isJumpingVisually = true;
            airborneElapsed = 0f;
            takeoffStartedAt = float.NegativeInfinity;
        }

        public void CancelActionPoses()
        {
            swingReleasePose.Reset();
            weavePose.Reset();
            goalPose.Reset();
            aimPose.Reset();
            ClearEdgeBalance();
        }

        private bool TryShowSwingRelease()
        {
            if (!swingReleasePose.IsActive) return false;
            airborneElapsed += Time.deltaTime;
            bool faceLeft = swingReleasePose.FacingLeft;
            float blend = swingReleasePose.HandoffBlend;
            Sprite swingFrame = swingingFrames[swingReleasePose.FrameIndex];
            // An immediate E after attachment can interrupt a size handoff.
            // Retain that exact visible size instead of jumping to normal swing.
            Vector2 swingScale = Vector2.Lerp(releaseInitialHeadScale / FrameHeadWidths[swingFrame],
                swingingVisualBaseScale * SwingFrameScale[swingFrame], swingReleasePose.SettleBlend);
            Sprite frame = swingFrame;
            Vector3 targetHead = releaseHeadOrigin;
            visualBaseScale = swingScale;
            if (swingReleasePose.UseAirbornePose)
            {
                frame = jumpingFrames[SelectJumpFrame(body.linearVelocity.y, false)];
                // Match head width and retain its scale ratio when swapping, then
                // ease to the existing airborne size. No double-image fade.
                Vector2 matchedScale = swingScale * (FrameHeadWidths[swingFrame] / FrameHeadWidths[frame]);
                visualBaseScale = Vector2.Lerp(matchedScale, jumpingVisualBaseScale, blend);
                Vector3 airHead = PoseHeadCenters[frame];
                if (faceLeft) airHead.x = -airHead.x;
                targetHead = Vector3.Scale(airHead,
                    new Vector3(jumpingVisualBaseScale.x * currentBaseScale,
                        jumpingVisualBaseScale.y * currentBaseScale, 1f));
            }
            visualRenderer.sprite = frame;
            visualRenderer.flipX = faceLeft;
            Vector3 drawnHead = PoseHeadCenters[frame];
            if (faceLeft) drawnHead.x = -drawnHead.x;
            Vector3 scaledHead = Vector3.Scale(drawnHead,
                new Vector3(visualBaseScale.x * currentBaseScale,
                    visualBaseScale.y * currentBaseScale, 1f));
            visualTransform.localPosition = visualTransform.localRotation *
                (Vector3.Lerp(releaseHeadOrigin, targetHead, blend) - scaledHead);
            isSwingReleasePoseDisplayed = true;
            isWalking = false;
            walkFrameProgress = 0f;
            return true;
        }

        private void UpdateJumpAnimation()
        {
            if (visualRenderer == null || body == null)
            {
                return;
            }

            airborneElapsed += Time.deltaTime;
            int frameIndex = SelectJumpFrame(body.linearVelocity.y, airborneElapsed <= launchFrameDuration);
            UpdateFacingFromHorizontalVelocity(body.linearVelocity.x);

            visualRenderer.sprite = jumpingFrames[frameIndex];
            visualTransform.localPosition = Vector3.zero;
            visualBaseScale = jumpingVisualBaseScale;
            isWalking = false;
            walkFrameProgress = 0f;
        }

        private int SelectJumpFrame(float verticalSpeed, bool showLaunch)
        {
            int frameIndex;

            if (showLaunch)
            {
                frameIndex = 0;
            }
            else if (verticalSpeed >= fastRiseSpeed)
            {
                frameIndex = 1;
            }
            else if (verticalSpeed > apexSpeed)
            {
                frameIndex = 2;
            }
            else if (verticalSpeed >= -apexSpeed)
            {
                frameIndex = 3;
            }
            else if (verticalSpeed > -fastFallSpeed)
            {
                frameIndex = 4;
            }
            else
            {
                frameIndex = 5;
            }

            // The generated takeoff and fast-fall drawings compress the whole body.
            // Reuse the neighboring stable-proportion poses so only the legs and
            // loose yarn communicate the jump instead of making the body pulse.
            if (frameIndex == 0)
            {
                frameIndex = 1;
            }
            else if (frameIndex == 5)
            {
                frameIndex = 4;
            }

            return frameIndex;
        }

        private void PrepareSwingAttachment()
        {
            bool eligible = usesCharacterArt && visualRenderer != null && visualRenderer.sprite != null &&
                swingingFrames != null && swingingFrames.Length == SwingFrameCount &&
                playerMover != null && playerMover.enabled && !playerMover.IsGrounded &&
                body != null && body.simulated && ropeController != null && ropeController.IsAttached &&
                (ropeController.ActiveHookPoint == null ||
                    ropeController.ActiveHookPoint.GetComponent<RopePlatformAnchor>() == null);
            int sequence = ropeController != null ? ropeController.AttachmentSequence : 0;
            if (!swingAttachPose.Advance(sequence, eligible, Time.deltaTime)) return;

            // Capture before removing last frame's jump/landing visual offset.
            // All offsets are local to the player, so its real flight continues.
            Sprite previousFrame = visualRenderer.sprite;
            Vector3 crown = FrameCrownPoints.TryGetValue(previousFrame, out Vector3 frameCrown)
                ? frameCrown : standingRopeLocalPoint;
            if (visualRenderer.flipX) crown.x = -crown.x;
            attachInitialCrown = visualTransform.localPosition + visualTransform.localRotation *
                Vector3.Scale(crown, visualTransform.localScale);
            float headWidth = FrameHeadWidths.TryGetValue(previousFrame, out float width)
                ? width : previousFrame.bounds.size.x;
            attachInitialHeadScale = new Vector2(visualTransform.localScale.x, visualTransform.localScale.y) *
                (headWidth / Mathf.Max(0.01f, currentBaseScale));
            // Include any small existing ground pose lean when stepping off.
            currentVisualAngle = Mathf.DeltaAngle(0f, visualTransform.localEulerAngles.z);
        }

        private void UpdateSwingAnimation()
        {
            if (visualRenderer == null || body == null || ropeController == null)
            {
                return;
            }

            Vector2 bodyToAnchor =
                ropeController.AnchorPoint - (Vector2)transform.position;
            Vector2 bodyToAnchorDirection = bodyToAnchor.sqrMagnitude > 0.0001f
                ? bodyToAnchor.normalized : Vector2.up;
            Vector2 radialDirection = -bodyToAnchorDirection;
            Vector2 tangentDirection = new Vector2(
                -radialDirection.y,
                radialDirection.x);
            float tangentialSpeed = Vector2.Dot(
                body.linearVelocity,
                tangentDirection);
            float inertiaLean = Mathf.Clamp(
                -tangentialSpeed * inertiaLeanAmount,
                -maximumInertiaLean,
                maximumInertiaLean);
            float ropeAngle = Vector2.SignedAngle(
                Vector2.up,
                bodyToAnchorDirection);
            float targetVisualAngle = Mathf.Clamp(
                ropeAngle * ropeFollowAmount + inertiaLean,
                -maximumVisualRotation,
                maximumVisualRotation);

            currentVisualAngle = Mathf.SmoothDampAngle(
                currentVisualAngle,
                targetVisualAngle,
                ref visualAngleVelocity,
                swingRotationSmoothTime,
                Mathf.Infinity,
                Time.deltaTime);
            visualTransform.localRotation = Quaternion.Euler(
                0f,
                0f,
                currentVisualAngle);

            // Facing is captured on attachment and held through both apexes.
            // This prevents velocity sign changes from flipping the sprite.
            visualRenderer.flipX = swingFacingLeft;

            if (swingingFrames != null && swingingFrames.Length == SwingFrameCount)
            {
                int frame = swingPose.Advance(tangentialSpeed, swingFacingLeft, Time.deltaTime);
                visualRenderer.sprite = swingingFrames[frame];
                visualBaseScale = swingingVisualBaseScale * SwingFrameScale[swingingFrames[frame]];
                // Keep the crown in the same place the rotated standing drawing
                // used to occupy. Do not move the parent/joint/collider at all.
                Vector3 crownOffset = Vector3.Scale(standingRopeLocalPoint,
                    new Vector3(standingVisualBaseScale.x * currentBaseScale,
                        standingVisualBaseScale.y * currentBaseScale, 1f));
                visualTransform.localPosition = visualTransform.localRotation * crownOffset;
                if (swingAttachPose.IsActive)
                {
                    float blend = swingAttachPose.Blend;
                    Vector2 matchedScale = attachInitialHeadScale / FrameHeadWidths[swingingFrames[frame]];
                    visualBaseScale = Vector2.Lerp(matchedScale, visualBaseScale, blend);
                    // Swing sprites pivot at the yarn tip, so the drawn rope
                    // and the head share this interpolated attachment point.
                    visualTransform.localPosition = Vector3.Lerp(attachInitialCrown,
                        visualTransform.localPosition, blend);
                }
                isSwingPoseDisplayed = true;
            }
            else
            {
                visualRenderer.sprite = standingSprite;
                visualTransform.localPosition = Vector3.zero;
                visualBaseScale = standingVisualBaseScale;
            }
            isWalking = false;
            walkFrameProgress = 0f;
            airborneElapsed = 0f;
        }

        private void UpdateLandingAnimation()
        {
            if (visualRenderer == null || landingFrames == null)
            {
                return;
            }

            float normalizedTime = Mathf.Clamp01(
                landingElapsed / Mathf.Max(0.01f, landingDuration));
            int frameIndex = normalizedTime < 0.28f
                ? 0
                : normalizedTime < 0.66f
                    ? 1
                    : LandingFrameCount - 1;

            visualRenderer.sprite = landingFrames[frameIndex];
            visualTransform.localPosition = new Vector3(
                0f,
                -CharacterVisualHeight * 0.5f,
                0f);
            visualBaseScale = landingVisualBaseScale;
            isWalking = false;
            walkFrameProgress = 0f;
            airborneElapsed = 0f;
        }

        private void UpdateSwingState(bool isAttached)
        {
            int sequence = ropeController != null ? ropeController.AttachmentSequence : 0;
            if (isAttached && (!wasRopeAttached || sequence != visualAttachmentSequence))
            {
                visualAttachmentSequence = sequence;
                swingFacingLeft = visualRenderer != null && visualRenderer.flipX;
                isReturningFromSwing = false;
                visualAngleVelocity = 0f;
                swingPose.Reset();
            }
            else if (!isAttached && wasRopeAttached)
            {
                isReturningFromSwing = true;
                visualAngleVelocity = 0f;
            }

            wasRopeAttached = isAttached;
        }

        private void UpdateUprightVisualRotation(bool isAttached)
        {
            if (visualTransform == null)
            {
                return;
            }

            float smoothTime = isAttached
                ? swingRotationSmoothTime
                : releaseRotationSmoothTime;
            currentVisualAngle = Mathf.SmoothDampAngle(
                currentVisualAngle,
                0f,
                ref visualAngleVelocity,
                smoothTime,
                Mathf.Infinity,
                Time.deltaTime);

            if (Mathf.Abs(Mathf.DeltaAngle(currentVisualAngle, 0f)) < 0.1f &&
                Mathf.Abs(visualAngleVelocity) < 0.5f)
            {
                currentVisualAngle = 0f;
                visualAngleVelocity = 0f;
                if (!isAttached)
                {
                    isReturningFromSwing = false;
                }
            }

            visualTransform.localRotation = Quaternion.Euler(
                0f,
                0f,
                currentVisualAngle);
        }

        private void UpdateFacingFromHorizontalVelocity(float horizontalVelocity)
        {
            if (visualRenderer == null ||
                (ropeController != null && ropeController.IsAttached) ||
                isReturningFromSwing ||
                Mathf.Abs(horizontalVelocity) < facingDeadZone)
            {
                return;
            }

            visualRenderer.flipX = horizontalVelocity < 0f;
        }

        private void UpdateWalkAnimation()
        {
            if (!usesCharacterArt || visualRenderer == null || standingSprite == null)
            {
                return;
            }

            float horizontalSpeed = body != null
                ? Mathf.Abs(body.linearVelocity.x)
                : 0f;
            bool shouldWalk =
                walkingFrames != null &&
                walkingFrames.Length == WalkFrameCount &&
                playerMover != null &&
                playerMover.IsGrounded &&
                (ropeController == null || !ropeController.IsAttached) &&
                horizontalSpeed >= minimumWalkSpeed;

            if (!shouldWalk)
            {
                if (isWalking || visualRenderer.sprite != standingSprite)
                {
                    visualRenderer.sprite = standingSprite;
                    visualTransform.localPosition = Vector3.zero;
                    visualBaseScale = standingVisualBaseScale;
                }

                isWalking = false;
                walkFrameProgress = 0f;
                return;
            }

            isWalking = true;
            UpdateFacingFromHorizontalVelocity(body.linearVelocity.x);
            visualTransform.localPosition = new Vector3(
                0f,
                -CharacterVisualHeight * 0.5f,
                0f);
            visualBaseScale = walkingVisualBaseScale;

            float speedRatio = Mathf.InverseLerp(
                minimumWalkSpeed,
                Mathf.Max(minimumWalkSpeed + 0.01f, fullWalkAnimationSpeed),
                horizontalSpeed);
            float framesPerSecond = Mathf.Lerp(
                minimumWalkFramesPerSecond,
                maximumWalkFramesPerSecond,
                speedRatio);
            walkFrameProgress += Time.deltaTime * framesPerSecond;
            int frameIndex = Mathf.FloorToInt(walkFrameProgress) % walkingFrames.Length;
            visualRenderer.sprite = walkingFrames[frameIndex];
        }

        private static Sprite[] LoadWalkFrames(out Vector2 contentSize)
        {
            return LoadAnimationFrames(
                WalkArtResourcePath,
                "Walk",
                WalkColumns,
                WalkRows,
                true,
                ref cachedWalkFrames,
                ref cachedWalkContentSize,
                out contentSize);
        }

        private static Sprite[] LoadJumpFrames(out Vector2 contentSize)
        {
            return LoadAnimationFrames(
                JumpArtResourcePath,
                "Jump",
                JumpColumns,
                JumpRows,
                false,
                ref cachedJumpFrames,
                ref cachedJumpContentSize,
                out contentSize);
        }

        private static Sprite[] LoadLandingFrames(out Vector2 contentSize)
        {
            return LoadAnimationFrames(
                LandingArtResourcePath,
                "Landing",
                LandingColumns,
                LandingRows,
                true,
                ref cachedLandingFrames,
                ref cachedLandingContentSize,
                out contentSize);
        }

        private static Sprite[] LoadAnimationFrames(
            string resourcePath,
            string animationName,
            int columns,
            int rows,
            bool alignToFeet,
            ref Sprite[] cachedFrames,
            ref Vector2 cachedContentSize,
            out Vector2 contentSize,
            bool alignToCrown = false,
            bool alignHeadToFeet = false,
            bool registerIdleCrown = false,
            bool registerBodyParts = false)
        {
            int frameCount = columns * rows;
            if (cachedFrames != null && cachedFrames.Length == frameCount)
            {
                contentSize = cachedContentSize;
                return cachedFrames;
            }

            contentSize = Vector2.zero;
            Texture2D source = Resources.Load<Texture2D>(resourcePath);
            if (source == null)
            {
                Debug.LogWarning(
                    $"{animationName} animation texture was not found: {resourcePath}");
                return null;
            }

            if (source.width % columns != 0 || source.height % rows != 0)
            {
                Debug.LogWarning(
                    $"{animationName} animation texture must be a {columns}x{rows} grid: " +
                    $"{source.width}x{source.height}");
                return null;
            }

            Color32[] pixels;
            try
            {
                pixels = source.GetPixels32();
            }
            catch (UnityException exception)
            {
                Debug.LogWarning(
                    $"{animationName} animation texture is not readable: {resourcePath}\n" +
                    exception.Message);
                return null;
            }

            for (int index = 0; index < pixels.Length; index++)
            {
                Color32 pixel = pixels[index];
                byte highest = System.Math.Max(
                    pixel.r,
                    System.Math.Max(pixel.g, pixel.b));
                byte lowest = System.Math.Min(
                    pixel.r,
                    System.Math.Min(pixel.g, pixel.b));
                bool isNeutralLightBackground =
                    pixel.a > 0 && lowest >= 205 && highest - lowest <= 28;
                if (isNeutralLightBackground)
                {
                    pixel.a = 0;
                    pixels[index] = pixel;
                }
            }

            Texture2D transparentTexture = new Texture2D(
                source.width,
                source.height,
                TextureFormat.RGBA32,
                false);
            transparentTexture.name = $"{source.name} Transparent";
            transparentTexture.filterMode = FilterMode.Bilinear;
            transparentTexture.wrapMode = TextureWrapMode.Clamp;
            transparentTexture.hideFlags = HideFlags.HideAndDontSave;
            transparentTexture.SetPixels32(pixels);
            transparentTexture.Apply(false, true);

            int cellWidth = source.width / columns;
            int cellHeight = source.height / rows;
            int maximumContentWidth = 0;
            int maximumContentHeight = 0;
            Sprite[] frames = new Sprite[frameCount];
            bool normalizeHead = alignToCrown || alignHeadToFeet;
            int[] headWidths = normalizeHead ? new int[frameCount] : null;
            int[] contentHeights = normalizeHead ? new int[frameCount] : null;
            float[] idleFootCenters = registerIdleCrown ? new float[frameCount] : null;

            for (int frameIndex = 0; frameIndex < frameCount; frameIndex++)
            {
                int column = frameIndex % columns;
                int rowFromTop = frameIndex / columns;
                int rowFromBottom = rows - 1 - rowFromTop;
                int cellX = column * cellWidth;
                int cellY = rowFromBottom * cellHeight;

                FindFrameContentBounds(
                    pixels,
                    source.width,
                    cellX,
                    cellY,
                    cellWidth,
                    cellHeight,
                    out int minContentX,
                    out int minContentY,
                    out int maxContentX,
                    out int maxContentY);
                int contentWidth = maxContentX >= minContentX
                    ? maxContentX - minContentX + 1
                    : 0;
                int contentHeight = maxContentY >= minContentY
                    ? maxContentY - minContentY + 1
                    : 0;
                if (normalizeHead && (contentWidth <= 0 || contentHeight <= 0))
                {
                    Debug.LogWarning($"{animationName} animation has an empty cell: {frameIndex + 1}");
                    foreach (Sprite created in frames)
                    {
                        if (created == null) continue;
                        HeadPoints.Remove(created);
                        PoseHeadCenters.Remove(created);
                        FrameHeadWidths.Remove(created);
                        FrameCrownPoints.Remove(created);
                        Object.Destroy(created);
                    }
                    Object.Destroy(transparentTexture);
                    return null;
                }
                maximumContentWidth = Mathf.Max(maximumContentWidth, contentWidth);
                maximumContentHeight = Mathf.Max(maximumContentHeight, contentHeight);

                Vector2 framePivot = CalculateFramePivot(
                    cellWidth,
                    cellHeight,
                    minContentX,
                    minContentY,
                    maxContentX,
                    maxContentY,
                    alignToFeet);

                int headBottom = minContentY + Mathf.FloorToInt(contentHeight * 0.40f);
                FindFrameContentBounds(pixels, source.width, cellX, cellY + headBottom,
                    cellWidth, Mathf.Max(0, maxContentY - headBottom + 1),
                    out int headMinX, out _, out int headMaxX, out _);
                int measuredHeadWidth = Mathf.Max(1, headMaxX - headMinX + 1);
                float headCenterX = (minContentX + maxContentX + 1f) * 0.5f;
                if (normalizeHead)
                {
                    // Tails change the full silhouette's centre and width. Use
                    // the upper 60% (head/tuft), never the trailing legs, to size
                    // the pose. The upper 5% locates the yarn attachment tip.
                    headWidths[frameIndex] = measuredHeadWidth;
                    contentHeights[frameIndex] = contentHeight;
                    headCenterX = (headMinX + headMaxX + 1f) * 0.5f;
                }
                if (alignToCrown)
                {
                    int tipBottom = maxContentY - Mathf.Max(1, Mathf.FloorToInt(contentHeight * 0.05f));
                    FindFrameContentBounds(pixels, source.width, cellX, cellY + tipBottom,
                        cellWidth, maxContentY - tipBottom + 1,
                        out int tipMinX, out _, out int tipMaxX, out _);
                    framePivot = new Vector2((tipMinX + tipMaxX + 1f) * 0.5f / cellWidth,
                        (maxContentY + 0.5f) / cellHeight);
                }
                else if (alignHeadToFeet)
                {
                    // Ignore yarn tails below/beside the feet when registering
                    // contact. Only the central lower body is used for the sole.
                    int feetX = headMinX + Mathf.FloorToInt(measuredHeadWidth * 0.40f);
                    int feetWidth = Mathf.Max(1, Mathf.FloorToInt(measuredHeadWidth * 0.45f));
                    int feetHeight = Mathf.Max(1, Mathf.FloorToInt(contentHeight * 0.30f));
                    FindFrameContentBounds(pixels, source.width, cellX + feetX, cellY + minContentY,
                        feetWidth, feetHeight, out _, out int feetMinY, out int feetMaxX, out _);
                    int soleY = feetMaxX >= 0 ? minContentY + feetMinY : minContentY;
                    contentHeights[frameIndex] = maxContentY - soleY + 1;
                    framePivot = new Vector2(headCenterX / cellWidth, (soleY + 0.5f) / cellHeight);
                    if (registerIdleCrown)
                    {
                        // Register a small strip of the planted feet, excluding
                        // the trailing yarn. Neutral-to-pose X offsets are baked
                        // once, so the body can shift without sliding the soles.
                        int soleStripHeight = Mathf.Max(1, Mathf.FloorToInt(contentHeight * 0.06f)) + 1;
                        FindFrameContentBounds(pixels, source.width, cellX + feetX, cellY + soleY,
                            feetWidth, soleStripHeight,
                            out int soleMinX, out _, out int soleMaxX, out _);
                        idleFootCenters[frameIndex] =
                            (feetX + (soleMinX + soleMaxX + 1f) * 0.5f - headCenterX) / SpritePixelsPerUnit;
                    }
                }

                Sprite frame = Sprite.Create(
                    transparentTexture,
                    new Rect(cellX, cellY, cellWidth, cellHeight),
                    framePivot,
                    SpritePixelsPerUnit,
                    0,
                    SpriteMeshType.FullRect);
                frame.name = $"{source.name} {animationName} {frameIndex + 1}";
                frame.hideFlags = HideFlags.HideAndDontSave;
                // Keep an artwork-space crown for changing-pivot presentations
                // (idle and manual release), without changing normal rope physics.
                {
                    int tipBottom = maxContentY - Mathf.Max(1, Mathf.FloorToInt(contentHeight * 0.05f));
                    FindFrameContentBounds(pixels, source.width, cellX, cellY + tipBottom,
                        cellWidth, maxContentY - tipBottom + 1,
                        out int tipMinX, out _, out int tipMaxX, out _);
                    FrameCrownPoints[frame] = new Vector3(
                        ((tipMinX + tipMaxX + 1f) * 0.5f - framePivot.x * cellWidth) / SpritePixelsPerUnit,
                        (maxContentY + 0.5f - framePivot.y * cellHeight) / SpritePixelsPerUnit, 0f);
                }
                FrameHeadWidths[frame] = measuredHeadWidth / SpritePixelsPerUnit;
                // The old rope-return point intentionally sits near the tuft.
                // Pose matching instead uses the wide, round part of the head,
                // so a long yarn tail or a taller tuft cannot shift the face.
                int widestBandMinY = maxContentY;
                int widestBandMaxY = headBottom;
                for (int y = headBottom; y <= maxContentY; y++)
                {
                    FindFrameContentBounds(pixels, source.width, cellX, cellY + y,
                        cellWidth, 1, out int rowMinX, out _, out int rowMaxX, out _);
                    if (rowMaxX - rowMinX + 1 < measuredHeadWidth * 0.97f) continue;
                    widestBandMinY = Mathf.Min(widestBandMinY, y);
                    widestBandMaxY = Mathf.Max(widestBandMaxY, y);
                }
                float poseHeadY = widestBandMaxY >= widestBandMinY
                    ? (widestBandMinY + widestBandMaxY + 1f) * 0.5f
                    : maxContentY + 1f - contentHeight * 0.3f;
                PoseHeadCenters[frame] = new Vector3(
                    ((headMinX + headMaxX + 1f) * 0.5f - framePivot.x * cellWidth) / SpritePixelsPerUnit,
                    (poseHeadY - framePivot.y * cellHeight) / SpritePixelsPerUnit, 0f);
                // Use opaque artwork bounds, not the padded animation-sheet cell.
                HeadPoints[frame] = new Vector3(
                    (headCenterX - framePivot.x * cellWidth) / SpritePixelsPerUnit,
                    (maxContentY + 1f - contentHeight * 0.16f - framePivot.y * cellHeight) / SpritePixelsPerUnit,
                    0f);
                if (registerBodyParts)
                {
                    RopePoseParts parts = RopePoseParts.Create(frame, pixels, source.width, poseHeadY, measuredHeadWidth);
                    if (parts != null)
                    {
                        PoseParts[frame] = parts;
                        // Cropping changes the sprite pivot, not the intended
                        // physical rope anchor. Register visual head landmarks
                        // for E attachment/retraction and goal hand-offs.
                        FrameCrownPoints[parts.Head] = FrameCrownPoints[frame] - parts.Neck;
                        HeadPoints[parts.Head] = HeadPoints[frame] - parts.Neck;
                        PoseHeadCenters[parts.Head] = PoseHeadCenters[frame] - parts.Neck;
                        FrameHeadWidths[parts.Head] = FrameHeadWidths[frame];
                    }
                }
                frames[frameIndex] = frame;
            }

            if (maximumContentWidth <= 0 || maximumContentHeight <= 0)
            {
                Debug.LogWarning(
                    $"{animationName} animation texture became empty: {resourcePath}");
                return null;
            }

            cachedFrames = frames;
            if (normalizeHead)
            {
                // Neutral drawing defines the standing height. Every frame is
                // scaled by its head width, so bent knees do not stretch the body.
                int neutralFrame = alignHeadToFeet ? 5 : 2;
                maximumContentWidth = headWidths[neutralFrame];
                maximumContentHeight = contentHeights[neutralFrame];
                for (int i = 0; i < frameCount; i++)
                {
                    float ratio = (float)maximumContentWidth / headWidths[i];
                    if (alignToCrown) SwingFrameScale[frames[i]] = ratio;
                    else GroundFrameScale[frames[i]] = ratio;
                    if (registerIdleCrown)
                        IdleFootOffsets[frames[i]] = idleFootCenters[neutralFrame] - idleFootCenters[i] * ratio;
                }
            }
            cachedContentSize = new Vector2(
                maximumContentWidth / SpritePixelsPerUnit,
                maximumContentHeight / SpritePixelsPerUnit);
            contentSize = cachedContentSize;
            return cachedFrames;
        }

        private static Vector2 CalculateFramePivot(
            int cellWidth,
            int cellHeight,
            int minContentX,
            int minContentY,
            int maxContentX,
            int maxContentY,
            bool alignToFeet)
        {
            if (maxContentX < minContentX || maxContentY < minContentY)
            {
                return new Vector2(0.5f, 0.5f);
            }

            float contentCenterX = (minContentX + maxContentX + 1f) * 0.5f;
            float pivotY = alignToFeet
                ? minContentY + 0.5f
                : (minContentY + maxContentY + 1f) * 0.5f;
            return new Vector2(
                contentCenterX / cellWidth,
                pivotY / cellHeight);
        }

        private static void FindFrameContentBounds(
            Color32[] pixels,
            int textureWidth,
            int cellX,
            int cellY,
            int cellWidth,
            int cellHeight,
            out int minX,
            out int minY,
            out int maxX,
            out int maxY)
        {
            minX = cellWidth;
            minY = cellHeight;
            maxX = -1;
            maxY = -1;

            for (int y = 0; y < cellHeight; y++)
            {
                for (int x = 0; x < cellWidth; x++)
                {
                    Color32 pixel = pixels[(cellY + y) * textureWidth + cellX + x];
                    if (pixel.a == 0)
                    {
                        continue;
                    }

                    minX = Mathf.Min(minX, x);
                    minY = Mathf.Min(minY, y);
                    maxX = Mathf.Max(maxX, x);
                    maxY = Mathf.Max(maxY, y);
                }
            }

        }

        private void UpdateRemainingLengthScale()
        {
            if (ropeResource == null)
            {
                return;
            }

            float remainingRatio = Mathf.Clamp01(ropeResource.NormalizedLength);
            // Four readable body sizes keep the resource feedback visible while
            // avoiding a body that appears to pulse after every small spend.
            currentBaseScale = remainingRatio switch
            {
                >= 0.75f => 1f,
                >= 0.50f => Mathf.Lerp(minimumVisualScale, 1f, 0.66f),
                >= 0.25f => Mathf.Lerp(minimumVisualScale, 1f, 0.33f),
                _ => minimumVisualScale
            };
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
                    landingElapsed = 0f;
                    softLandingStartedAt = Time.time;
                    softLandingStrength = Mathf.Lerp(0.4f, 1f,
                        Mathf.InverseLerp(minimumLandingSpeed, 10f, fastestFallSpeed));
                    takeoffStartedAt = float.NegativeInfinity;
                }

                fastestFallSpeed = 0f;
            }

            wasGrounded = isGrounded;
        }

        private float takeoffStartedAt = float.NegativeInfinity;
        private float softLandingStartedAt = float.NegativeInfinity;
        private float softLandingStrength;
        private Vector3 elasticityOffset;

        public void PlayTakeoffElasticity()
        {
            weavePose.Reset();
            aimPose.Reset();
            ClearEdgeBalance();
            takeoffStartedAt = Time.time;
            softLandingStartedAt = float.NegativeInfinity;
        }

        private void ApplyVisualScale()
        {
            if (visualTransform == null)
            {
                return;
            }

            visualTransform.localScale = new Vector3(
                visualBaseScale.x * currentBaseScale,
                visualBaseScale.y * currentBaseScale,
                1f);
            // The six weave drawings already contain the knee bend. Do not add
            // whole-body squash on top of their registered, planted feet.
            if (isWeavePoseDisplayed || isAimPoseDisplayed || !Application.isPlaying || playerMover == null || !playerMover.enabled ||
                body == null || !body.simulated ||
                (ropeController != null && ropeController.IsAttached)) return;
            float stretch = 0f;
            float jumpTime = (Time.time - takeoffStartedAt) / 0.24f;
            if (jumpTime >= 0f && jumpTime < 1f)
                stretch = jumpTime < 0.25f
                    ? -0.06f * Mathf.Sin(jumpTime / 0.25f * Mathf.PI)
                    : 0.08f * Mathf.Sin((jumpTime - 0.25f) / 0.75f * Mathf.PI);
            float landTime = (Time.time - softLandingStartedAt) / 0.28f;
            if (playerMover.IsGrounded && landTime >= 0f && landTime < 1f)
            {
                float wave = Mathf.Sin(landTime * Mathf.PI);
                stretch = -0.10f * softLandingStrength * wave * wave;
            }
            Vector3 baseScale = visualTransform.localScale;
            visualTransform.localScale = new Vector3(baseScale.x / (1f + stretch),
                baseScale.y * (1f + stretch), baseScale.z);
            if (visualRenderer != null && visualRenderer.sprite != null)
            {
                // Preserve the drawn feet; never move the player's collider.
                float footOffset = visualRenderer.sprite.bounds.min.y *
                    (baseScale.y - visualTransform.localScale.y);
                elasticityOffset = Vector3.up * footOffset;
                visualTransform.localPosition += elasticityOffset;
            }
        }
    }
}
