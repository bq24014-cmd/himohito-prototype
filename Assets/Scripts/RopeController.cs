using UnityEngine;

namespace HimoHito
{
    /// <summary>
    /// Arrow keys aim the rope. E toggles attachment and detachment.
    /// Mouse input remains available as an optional alternative.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody2D), typeof(DistanceJoint2D), typeof(LineRenderer))]
    [RequireComponent(typeof(RopeResource))]
    public sealed class RopeController : MonoBehaviour
    {
        private static readonly Color AimGuideColor =
            new Color(1f, 0.72f, 0.80f, 0.55f);
        private const float PlatformHookTargetingGrace = 0.5f;
        private const float RopeRevealDuration = 0.13f;
        private float ropeRevealStartedAt;
        private bool hookArrivalPending;
        private LineRenderer aimHookOutline;
        private Material aimHookMaterial;
        private AnimationCurve unavailableAimWidth;
        private readonly AnimationCurve availableAimWidth = AnimationCurve.Constant(0f, 1f, 1f);

        [SerializeField, Min(1f)] private float maximumShotDistance = 14f;
        [SerializeField, Min(0.01f)] private float ropeWidth = 0.16f;
        [SerializeField, Min(0.01f)] private float minimumRopeWidth = 0.06f;
        [SerializeField, Range(3, 32)] private int ropeVisualSegments = 14;
        [SerializeField] private Color ropeColor = new Color(0.95f, 0.82f, 0.35f);
        [SerializeField, Min(10f)] private float aimRotationSpeed = 120f;
        [SerializeField, Min(1)] private int minimumSelectableRopeLength = 1;
        [SerializeField, Min(0f)] private float lengthSelectionRepeatDelay = 0.35f;
        [SerializeField, Min(0.01f)] private float lengthSelectionRepeatInterval = 0.1f;
        [SerializeField, Min(0.1f)] private float airChainReconnectWindow = 1.5f;

        private Rigidbody2D body;
        private Collider2D bodyCollider;
        private DistanceJoint2D ropeJoint;
        private LineRenderer lineRenderer;
        private RopeResource ropeResource;
        private PlayerMover playerMover;
        private RopeBodyVisual ropeBodyVisual;
        private PrototypeAudioFeedback audioFeedback;
        private SpriteRenderer bodyRenderer;
        private Camera mainCamera;
        private Material runtimeMaterial;
        private Texture2D yarnTexture;
        private Vector2 anchorPoint;
        private HookPoint activeHookPoint;
        private Vector2 keyboardAimDirection = new Vector2(1f, 1f).normalized;
        private float activeRopeLength;
        private float nextLengthIncreaseTime;
        private float nextLengthDecreaseTime;
        private string pendingAirChainGroup;
        private int pendingAirChainOrder;
        private float airChainReconnectExpiresAt = float.NegativeInfinity;
        private int selectedRopeLength = 1;
        private RigidbodyConstraints2D constraintsBeforePlatformBuildHold;
        private bool isPlatformBuildHoldActive;

        public bool IsAttached => ropeJoint != null && ropeJoint.enabled;
        public Vector2 AnchorPoint => anchorPoint;
        public HookPoint ActiveHookPoint => activeHookPoint;
        public Vector2 KeyboardAimDirection => keyboardAimDirection;
        public float ReleaseRefundRate => 1f;
        public int SelectedRopeLength => selectedRopeLength;
        public int MaximumSelectableRopeLength => GetMaximumSelectableRopeLength();
        public float ActiveRopeLength => activeRopeLength;
        public int AttachmentSequence { get; private set; }
        public Color VisibleRopeColor => GetVisibleRopeColor();
        public bool IsAirChainReconnectOpen =>
            !IsAttached &&
            !string.IsNullOrEmpty(pendingAirChainGroup) &&
            Time.unscaledTime <= airChainReconnectExpiresAt;

        private void Awake()
        {
            RopeController[] controllers = GetComponents<RopeController>();
            if (controllers.Length > 0 && controllers[0] != this)
            {
                Debug.LogWarning(
                    "Duplicate RopeController was disabled to prevent one E press " +
                    "from attaching and detaching in the same frame.",
                    this);
                enabled = false;
                return;
            }

            body = GetComponent<Rigidbody2D>();
            bodyCollider = GetComponent<Collider2D>();
            ropeJoint = GetComponent<DistanceJoint2D>();
            lineRenderer = GetComponent<LineRenderer>();
            ropeResource = GetComponent<RopeResource>();
            playerMover = GetComponent<PlayerMover>();
            ropeBodyVisual = GetComponent<RopeBodyVisual>();
            audioFeedback = GetComponent<PrototypeAudioFeedback>();
            if (audioFeedback == null)
            {
                audioFeedback = gameObject.AddComponent<PrototypeAudioFeedback>();
            }
            bodyRenderer = GetComponent<SpriteRenderer>();
            mainCamera = Camera.main;

            ropeJoint.enabled = false;
            ropeJoint.autoConfigureConnectedAnchor = false;
            ropeJoint.autoConfigureDistance = false;
            ropeJoint.maxDistanceOnly = true;
            ropeJoint.enableCollision = false;

            lineRenderer.enabled = false;
            lineRenderer.positionCount = ropeVisualSegments;
            lineRenderer.useWorldSpace = true;
            float visibleRopeWidth = GetVisibleRopeWidth();
            lineRenderer.startWidth = visibleRopeWidth;
            lineRenderer.endWidth = visibleRopeWidth;
            Color visibleRopeColor = GetVisibleRopeColor();
            lineRenderer.startColor = visibleRopeColor;
            lineRenderer.endColor = visibleRopeColor;

            Shader spriteShader = Shader.Find("Sprites/Default");
            if (spriteShader != null)
            {
                runtimeMaterial = new Material(spriteShader)
                {
                    name = "Runtime Rope Material",
                    hideFlags = HideFlags.HideAndDontSave
                };
                lineRenderer.material = runtimeMaterial;
                yarnTexture = YarnRopeTexture.Load();
            }
        }

        private void Update()
        {
            UpdateSelectedRopeLength();
            UpdateKeyboardAim();

            if (Input.GetKeyDown(KeyCode.E))
            {
                if (IsAttached)
                {
                    DetachAndRefund(playReleaseSound: true);
                }
                else
                {
                    Vector2 keyboardTarget =
                        body.position + keyboardAimDirection * maximumShotDistance;
                    TryAttach(keyboardTarget);
                }
            }

            if (Input.GetMouseButtonDown(0))
            {
                if (IsAttached)
                {
                    DetachAndRefund(playReleaseSound: true);
                }
                else
                {
                    TryAttachTowardCursor();
                }
            }
        }

        private void LateUpdate()
        {
            UpdateAimHookFeedback();
            if (!IsAttached)
            {
                DrawAimGuide();
                return;
            }

            lineRenderer.enabled = true;
            float visibleRopeWidth = GetVisibleRopeWidth();
            lineRenderer.startWidth = visibleRopeWidth;
            lineRenderer.endWidth = visibleRopeWidth;
            Color visibleRopeColor = GetVisibleRopeColor();
            lineRenderer.startColor = visibleRopeColor;
            lineRenderer.endColor = visibleRopeColor;
            DrawAttachedRope();
        }

        private void OnDisable()
        {
            if (aimHookOutline != null) aimHookOutline.enabled = false;
            DetachAndRefund();
        }

        private void OnDestroy()
        {
            if (aimHookMaterial != null) Destroy(aimHookMaterial);
            if (runtimeMaterial != null)
            {
                Destroy(runtimeMaterial);
            }
        }

        private void OnValidate()
        {
            maximumShotDistance = Mathf.Max(1f, maximumShotDistance);
            ropeWidth = Mathf.Max(0.01f, ropeWidth);
            minimumRopeWidth = Mathf.Clamp(minimumRopeWidth, 0.01f, ropeWidth);
            ropeVisualSegments = Mathf.Clamp(ropeVisualSegments, 3, 32);
            lengthSelectionRepeatDelay = Mathf.Max(0f, lengthSelectionRepeatDelay);
            lengthSelectionRepeatInterval = Mathf.Max(0.01f, lengthSelectionRepeatInterval);
            airChainReconnectWindow = Mathf.Max(0.1f, airChainReconnectWindow);
            minimumSelectableRopeLength = Mathf.Clamp(
                minimumSelectableRopeLength,
                1,
                Mathf.FloorToInt(maximumShotDistance));
        }

        public bool TryAttach(Vector2 worldTarget)
        {
            if (IsAttached)
            {
                return false;
            }

            audioFeedback?.PlayRopeShot();

            bool foundTarget = TryResolveAvailableAttachment(worldTarget,
                out Vector2 resolvedAnchor, out HookPoint hookPoint);
            if (!foundTarget)
            {
                audioFeedback?.PlayRopeAttachMiss();
                return false;
            }

            // Attaching and swinging are free. Rope is permanently consumed only
            // when Q converts the active rope into a platform.
            float selectedLength = SelectedRopeLength;
            activeRopeLength = selectedLength;
            anchorPoint = resolvedAnchor;
            activeHookPoint = hookPoint;
            ropeJoint.connectedBody = null;
            ropeJoint.connectedAnchor = anchorPoint;
            float jointDistance = selectedLength;
            bool isPlatformBuildAttachment =
                hookPoint != null &&
                hookPoint.TryGetComponent(out RopePlatformAnchor _);
            if (isPlatformBuildAttachment)
            {
                // A platform Hook is followed by Q, not by a swing. The player
                // stands about half a collider outside the paired bank Hook, so
                // do not let that targeting margin pull them off the edge.
                jointDistance = Mathf.Max(
                    jointDistance,
                    Vector2.Distance(body.position, anchorPoint));
            }
            ropeJoint.distance = jointDistance;
            ropeJoint.enabled = true;
            if (isPlatformBuildAttachment)
            {
                BeginPlatformBuildHold();
            }
            AttachmentSequence++;
            lineRenderer.enabled = true;
            ClearAirChainReconnectWindow();
            // The joint is already active. Only presentation travels to the Hook.
            ropeRevealStartedAt = Time.time;
            hookArrivalPending = true;
            DrawAttachedRope();
            return true;
        }

        public void DetachAndRefund(bool playReleaseSound = false)
        {
            hookArrivalPending = false;
            if (!playReleaseSound) ropeBodyVisual?.CancelActionPoses();
            if (!IsAttached)
            {
                return;
            }

            // Disabling the joint must not erase the velocity built up by the pendulum.
            Vector2 preservedVelocity = isPlatformBuildHoldActive
                ? Vector2.zero
                : body.linearVelocity;
            float preservedAngularVelocity = isPlatformBuildHoldActive
                ? 0f
                : body.angularVelocity;
            HookPoint releasedHook = activeHookPoint;
            // Manual release only: copy the artwork before hiding the live rope.
            if (playReleaseSound) RopeRetractVisual.Play(lineRenderer, body);
            ropeJoint.enabled = false;
            EndPlatformBuildHold();
            activeHookPoint = null;
            body.linearVelocity = preservedVelocity;
            body.angularVelocity = preservedAngularVelocity;
            lineRenderer.enabled = false;
            activeRopeLength = 0f;
            OpenAirChainReconnectWindow(releasedHook);
            ClampSelectedRopeLength();
            if (playReleaseSound)
            {
                // The joint and momentum have already been handled. Only the
                // visible body follows through; reattachment remains immediate.
                ropeBodyVisual?.PlaySwingReleasePose();
                audioFeedback.PlayRopeReleased(preservedVelocity.magnitude);
            }
        }

        private void OpenAirChainReconnectWindow(HookPoint releasedHook)
        {
            if (releasedHook == null || !releasedHook.IsAirChainStep)
            {
                ClearAirChainReconnectWindow();
                return;
            }

            pendingAirChainGroup = releasedHook.AirChainGroup;
            pendingAirChainOrder = releasedHook.AirChainOrder;
            airChainReconnectExpiresAt =
                Time.unscaledTime + airChainReconnectWindow;
        }

        private bool CanReconnectAirChainTo(HookPoint targetHook)
        {
            return targetHook != null &&
                   targetHook.IsAirChainStep &&
                   Time.unscaledTime <= airChainReconnectExpiresAt &&
                   targetHook.AirChainGroup == pendingAirChainGroup &&
                   targetHook.AirChainOrder == pendingAirChainOrder + 1;
        }

        private bool TryResolveNextAirChainHook(
            out Vector2 resolvedAnchor,
            out HookPoint hookPoint)
        {
            resolvedAnchor = default;
            hookPoint = null;
            if (!IsAirChainReconnectOpen)
            {
                return false;
            }

            float shotDistance = Mathf.Min(
                SelectedRopeLength,
                maximumShotDistance);
            foreach (HookPoint candidate in
                     Object.FindObjectsByType<HookPoint>(
                         FindObjectsSortMode.None))
            {
                if (!CanReconnectAirChainTo(candidate))
                {
                    continue;
                }

                Vector2 candidateAnchor = candidate.GetAttachmentPoint(
                    candidate.transform.position);
                if (Vector2.Distance(body.position, candidateAnchor) >
                    shotDistance + 0.01f)
                {
                    continue;
                }

                resolvedAnchor = candidateAnchor;
                hookPoint = candidate;
                return true;
            }

            return false;
        }

        private void ClearAirChainReconnectWindow()
        {
            pendingAirChainGroup = null;
            pendingAirChainOrder = 0;
            airChainReconnectExpiresAt = float.NegativeInfinity;
        }

        public bool CommitAttachedRopeAsPlatform(float permanentCost)
        {
            if (!IsAttached || permanentCost <= 0f ||
                !ropeResource.TrySpend(permanentCost))
            {
                return false;
            }

            hookArrivalPending = false;
            ropeBodyVisual?.CancelActionPoses();
            ropeJoint.enabled = false;
            EndPlatformBuildHold();
            activeHookPoint = null;
            lineRenderer.enabled = false;
            activeRopeLength = 0f;
            ClampSelectedRopeLength();
            return true;
        }

        private void BeginPlatformBuildHold()
        {
            if (isPlatformBuildHoldActive || body == null)
            {
                return;
            }

            // A RopePlatformAnchor is a construction target. It must not switch
            // the player into a pendulum while they are preparing Q at a bridge
            // edge, so hold the successful E position until Q or detach.
            constraintsBeforePlatformBuildHold = body.constraints;
            body.linearVelocity = Vector2.zero;
            body.angularVelocity = 0f;
            body.constraints =
                constraintsBeforePlatformBuildHold |
                RigidbodyConstraints2D.FreezePositionX |
                RigidbodyConstraints2D.FreezePositionY;
            isPlatformBuildHoldActive = true;
        }

        private void EndPlatformBuildHold()
        {
            if (!isPlatformBuildHoldActive || body == null)
            {
                return;
            }

            body.constraints = constraintsBeforePlatformBuildHold;
            isPlatformBuildHoldActive = false;
        }

        public void RestoreSelectedRopeLength(int length)
        {
            selectedRopeLength = length;
            ClampSelectedRopeLength();
        }

        public bool TryResolveCurrentAimHook(
            out HookPoint hookPoint,
            out Vector2 resolvedAnchor)
        {
            Vector2 origin = body.position;
            float baseSearchDistance = Mathf.Min(
                SelectedRopeLength,
                maximumShotDistance);
            float hookSearchDistance = GetHookSearchDistance(
                baseSearchDistance);
            RaycastHit2D[] hits = Physics2D.RaycastAll(
                origin,
                keyboardAimDirection,
                hookSearchDistance);
            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider == null || hit.collider == bodyCollider)
                {
                    continue;
                }

                HookPoint candidate =
                    hit.collider.GetComponentInParent<HookPoint>();
                if (candidate == null)
                {
                    continue;
                }

                Vector2 candidateAnchor = candidate.GetAttachmentPoint(hit.point);
                float allowedDistance = CanUsePlatformHookTargetingGrace(
                    candidate,
                    baseSearchDistance)
                    ? hookSearchDistance
                    : baseSearchDistance;
                if (Vector2.Distance(origin, candidateAnchor) <=
                    allowedDistance + 0.01f)
                {
                    hookPoint = candidate;
                    resolvedAnchor = candidateAnchor;
                    return true;
                }
            }

            resolvedAnchor = default;
            hookPoint = null;
            return false;
        }

        private bool TryResolveAvailableAttachment(Vector2 worldTarget,
            out Vector2 resolvedAnchor, out HookPoint hookPoint)
        {
            bool found = IsAirChainReconnectOpen
                ? TryResolveNextAirChainHook(out resolvedAnchor, out hookPoint)
                : TryResolveAttachmentPoint(worldTarget, out resolvedAnchor, out hookPoint);
            return found && (playerMover == null || playerMover.IsGrounded ||
                CanReconnectAirChainTo(hookPoint));
        }

        private bool TryResolveAttachmentPoint(
            Vector2 worldTarget,
            out Vector2 resolvedAnchor,
            out HookPoint hookPoint,
            bool previewBeyondSelectedLength = false)
        {
            resolvedAnchor = default;
            hookPoint = null;

            Vector2 origin = body.position;
            Vector2 offset = worldTarget - origin;
            if (offset.sqrMagnitude < 0.01f)
            {
                return false;
            }

            float shotDistance = previewBeyondSelectedLength
                ? maximumShotDistance : Mathf.Min(SelectedRopeLength, maximumShotDistance);
            float hookSearchDistance = GetHookSearchDistance(shotDistance);
            RaycastHit2D[] hits = Physics2D.RaycastAll(
                origin,
                offset.normalized,
                hookSearchDistance);

            // Only authored HookPoints are valid rope targets. Floors, walls,
            // beams, generated rope platforms, and empty space must never become
            // implicit anchors; otherwise the visible Hooks lose their purpose
            // and terrain edges can create unstable attachment states.
            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider == null || hit.collider == bodyCollider)
                {
                    continue;
                }

                HookPoint candidateHook =
                    hit.collider.GetComponentInParent<HookPoint>();
                if (candidateHook == null)
                {
                    continue;
                }

                if (!CanAttachToHook(candidateHook))
                {
                    continue;
                }

                Vector2 candidateAnchor =
                    candidateHook.GetAttachmentPoint(hit.point);
                float allowedDistance =
                    CanUsePlatformHookTargetingGrace(
                        candidateHook,
                        shotDistance)
                        ? hookSearchDistance
                        : shotDistance;
                if (Vector2.Distance(origin, candidateAnchor) >
                    allowedDistance + 0.01f)
                {
                    continue;
                }

                resolvedAnchor = candidateAnchor;
                hookPoint = candidateHook;
                return true;
            }

            return false;
        }

        private bool CanAttachToHook(HookPoint candidateHook)
        {
            if (gameObject.scene.name != "Tutorial" ||
                candidateHook.name != TutorialSectionFourSetup.CenterHookName)
            {
                return true;
            }

            PrototypeRunController tutorial =
                GetComponent<PrototypeRunController>();
            RopePlatformBuilder platformBuilder =
                GetComponent<RopePlatformBuilder>();
            return tutorial == null ||
                tutorial.CurrentTutorialSection != 4 ||
                !TutorialSectionFourSetup.HasLeftPlatform(platformBuilder);
        }

        private float GetHookSearchDistance(float baseDistance)
        {
            return Mathf.Min(
                maximumShotDistance,
                baseDistance + PlatformHookTargetingGrace);
        }

        private static bool CanUsePlatformHookTargetingGrace(
            HookPoint hook,
            float ropeLength)
        {
            if (hook == null ||
                !hook.TryGetComponent(out RopePlatformAnchor platformAnchor) ||
                !platformAnchor.TryGetPairedAnchor(out Vector2 pairedAnchor))
            {
                return false;
            }

            Vector2 hookAnchor = hook.GetAttachmentPoint(
                hook.transform.position);
            return Vector2.Distance(hookAnchor, pairedAnchor) <=
                ropeLength + 0.05f;
        }

        private void UpdateSelectedRopeLength()
        {
            if (IsAttached)
            {
                ResetLengthSelectionRepeat();
                return;
            }

            ClampSelectedRopeLength();
            UpdateLengthSelectionKey(
                KeyCode.W,
                1,
                ref nextLengthIncreaseTime);
            UpdateLengthSelectionKey(
                KeyCode.S,
                -1,
                ref nextLengthDecreaseTime);
        }

        private void UpdateLengthSelectionKey(
            KeyCode key,
            int amount,
            ref float nextRepeatTime)
        {
            if (Input.GetKeyDown(key))
            {
                ChangeSelectedRopeLength(amount, true);
                nextRepeatTime = Time.unscaledTime + lengthSelectionRepeatDelay;
                return;
            }

            if (!Input.GetKey(key))
            {
                nextRepeatTime = 0f;
                return;
            }

            if (nextRepeatTime <= 0f || Time.unscaledTime < nextRepeatTime)
            {
                return;
            }

            ChangeSelectedRopeLength(amount, false);
            nextRepeatTime = Time.unscaledTime + lengthSelectionRepeatInterval;
        }

        private void ChangeSelectedRopeLength(
            int amount,
            bool playLimitSound)
        {
            int previousLength = selectedRopeLength;
            selectedRopeLength = Mathf.Clamp(
                selectedRopeLength + amount,
                minimumSelectableRopeLength,
                GetMaximumSelectableRopeLength());

            if (selectedRopeLength != previousLength)
            {
                audioFeedback?.PlayRopeLengthChanged(amount);
            }
            else if (playLimitSound)
            {
                audioFeedback?.PlayRopeLengthLimitReached();
            }
        }

        private void ResetLengthSelectionRepeat()
        {
            nextLengthIncreaseTime = 0f;
            nextLengthDecreaseTime = 0f;
        }

        private void ClampSelectedRopeLength()
        {
            selectedRopeLength = Mathf.Clamp(
                selectedRopeLength,
                minimumSelectableRopeLength,
                GetMaximumSelectableRopeLength());
        }

        private int GetMaximumSelectableRopeLength()
        {
            float availableLength = ropeResource != null
                ? ropeResource.CurrentLength
                : maximumShotDistance;
            int availableWholeUnits = Mathf.FloorToInt(
                Mathf.Min(maximumShotDistance, availableLength));
            return Mathf.Max(minimumSelectableRopeLength, availableWholeUnits);
        }

        private void TryAttachTowardCursor()
        {
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }

            if (mainCamera == null)
            {
                return;
            }

            Vector3 cursor = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            TryAttach(new Vector2(cursor.x, cursor.y));
        }

        private bool UpdateKeyboardAim()
        {
            bool changed = false;

            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                keyboardAimDirection = Vector2.up;
                changed = true;
            }

            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                keyboardAimDirection = Vector2.down;
                changed = true;
            }

            float rotation = 0f;
            if (Input.GetKey(KeyCode.LeftArrow)) rotation += aimRotationSpeed * Time.deltaTime;
            if (Input.GetKey(KeyCode.RightArrow)) rotation -= aimRotationSpeed * Time.deltaTime;

            if (!Mathf.Approximately(rotation, 0f))
            {
                keyboardAimDirection = Quaternion.Euler(0f, 0f, rotation) * keyboardAimDirection;
                keyboardAimDirection.Normalize();
                changed = true;
            }

            return changed;
        }

        private void UpdateAimHookFeedback()
        {
            if (aimHookOutline != null) aimHookOutline.enabled = false;
            if (IsAttached || Time.timeScale <= 0f || body == null) return;

            Vector2 target = body.position + keyboardAimDirection * maximumShotDistance;
            bool available = TryResolveAvailableAttachment(target, out Vector2 anchor,
                out HookPoint hook);
            if (!available && !TryResolveAttachmentPoint(target, out anchor, out hook,
                    previewBeyondSelectedLength: true)) return;

            if (aimHookOutline == null)
            {
                Shader shader = Shader.Find("Sprites/Default");
                if (shader == null) return;
                GameObject visual = new GameObject("Aim Hook Feedback");
                visual.transform.SetParent(transform, false);
                aimHookOutline = visual.AddComponent<LineRenderer>();
                aimHookMaterial = new Material(shader) { hideFlags = HideFlags.HideAndDontSave };
                aimHookOutline.sharedMaterial = aimHookMaterial;
                aimHookOutline.useWorldSpace = true;
                aimHookOutline.loop = true;
                aimHookOutline.positionCount = 65;
                aimHookOutline.sortingLayerID = lineRenderer.sortingLayerID;
                aimHookOutline.sortingOrder = lineRenderer.sortingOrder + 2;
                unavailableAimWidth = new AnimationCurve();
                for (int i = 0; i <= 64; i++)
                {
                    Keyframe key = new Keyframe(i / 64f, i % 8 < 5 ? 1f : 0f);
                    key.outTangent = float.PositiveInfinity;
                    unavailableAimWidth.AddKey(key);
                }
            }

            Color color = available ? new Color(1f, 0.91f, 0.65f, 0.8f)
                : new Color(1f, 0.64f, 0.72f, 0.5f);
            aimHookOutline.startColor = aimHookOutline.endColor = color;
            // A thin, complete ring means E can connect; a broken ring means it cannot.
            for (int i = 0; i <= 64; i++)
            {
                float t = i / 64f;
                float angle = t * Mathf.PI * 2f;
                float radius = 0.49f;
                aimHookOutline.SetPosition(i, new Vector3(anchor.x + Mathf.Cos(angle) * radius,
                    anchor.y + Mathf.Sin(angle) * radius, 0f));
            }
            aimHookOutline.widthCurve = available ? availableAimWidth : unavailableAimWidth;
            aimHookOutline.widthMultiplier = available ? 0.035f : 0.025f;
            aimHookOutline.enabled = true;
        }

        private void DrawAimGuide()
        {
            if (runtimeMaterial != null) runtimeMaterial.mainTexture = null;
            lineRenderer.textureMode = LineTextureMode.Stretch;
            lineRenderer.textureScale = Vector2.one;
            lineRenderer.enabled = true;
            lineRenderer.positionCount = 2;
            lineRenderer.startWidth = minimumRopeWidth * 0.6f;
            lineRenderer.endWidth = minimumRopeWidth * 0.6f;
            lineRenderer.startColor = AimGuideColor;
            lineRenderer.endColor = AimGuideColor;
            lineRenderer.SetPosition(0, body.position);
            lineRenderer.SetPosition(1, body.position + keyboardAimDirection * selectedRopeLength);
        }

        private void DrawAttachedRope()
        {
            if (runtimeMaterial != null && yarnTexture != null)
            {
                runtimeMaterial.mainTexture = yarnTexture;
                lineRenderer.textureMode = LineTextureMode.Tile;
                float tileWorldLength = GetVisibleRopeWidth() * yarnTexture.width / yarnTexture.height;
                lineRenderer.textureScale = new Vector2(1f / Mathf.Max(0.01f, tileWorldLength), 1f);
                // The artwork already carries the player's pink; do not tint it twice.
                Color tint = new Color(1f, 1f, 1f, GetVisibleRopeColor().a);
                lineRenderer.startColor = tint;
                lineRenderer.endColor = tint;
            }
            float progress = Mathf.Clamp01((Time.time - ropeRevealStartedAt) / RopeRevealDuration);
            float visibleFraction = Mathf.SmoothStep(0f, 1f, progress);
            lineRenderer.positionCount = ropeVisualSegments;
            for (int i = 0; i < ropeVisualSegments; i++)
            {
                float t = visibleFraction * i / (float)(ropeVisualSegments - 1);
                lineRenderer.SetPosition(i, GetAttachedRopePoint(t));
            }

            if (hookArrivalPending && progress >= 1f)
            {
                hookArrivalPending = false;
                audioFeedback?.PlayHookAttached();
                if (activeHookPoint != null)
                {
                    HimoHitoHookRingVisual blue = activeHookPoint.GetComponentInChildren<HimoHitoHookRingVisual>();
                    RopeAnchorRingVisual green = activeHookPoint.GetComponentInChildren<RopeAnchorRingVisual>();
                    Transform visual = blue != null ? blue.transform : green != null ? green.transform : null;
                    // Never animate the physical Hook or a renderer carrying a collider.
                    if (visual != null && visual != activeHookPoint.transform &&
                        visual.GetComponentInChildren<Collider2D>() == null)
                    {
                        HookArrivalVisualPulse pulse = visual.GetComponent<HookArrivalVisualPulse>();
                        if (pulse == null) pulse = visual.gameObject.AddComponent<HookArrivalVisualPulse>();
                        pulse.Play();
                    }
                }
            }
        }

        private Vector2 GetAttachedRopePoint(float t)
        {
            Vector2 physicsStart = body.position;
            Vector2 start = GetRopeVisualOrigin();
            Vector2 end = anchorPoint;
            float directDistance = Vector2.Distance(physicsStart, end);
            float slack = Mathf.Max(0f, activeRopeLength - directDistance);
            Vector2 point = Vector2.Lerp(start, end, t);
            return point + Vector2.down * (slack * 4f * t * (1f - t));
        }

        private Vector2 GetRopeVisualOrigin()
        {
            if (ropeBodyVisual == null)
            {
                ropeBodyVisual = GetComponent<RopeBodyVisual>();
            }

            return ropeBodyVisual != null
                ? ropeBodyVisual.RopeOrigin
                : body.position;
        }

        private float GetVisibleRopeWidth()
        {
            if (ropeResource == null)
            {
                return ropeWidth;
            }

            float remainingRatio = ropeResource.MaximumLength <= 0f
                ? 0f
                : ropeResource.CurrentLength / ropeResource.MaximumLength;
            return Mathf.Lerp(
                minimumRopeWidth,
                ropeWidth,
                Mathf.Clamp01(remainingRatio));
        }

        private Color GetVisibleRopeColor()
        {
            return bodyRenderer != null ? bodyRenderer.color : ropeColor;
        }
    }
}
