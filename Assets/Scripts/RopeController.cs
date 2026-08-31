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

        [SerializeField, Min(1f)] private float maximumShotDistance = 14f;
        [SerializeField, Min(0.01f)] private float ropeWidth = 0.16f;
        [SerializeField, Min(0.01f)] private float minimumRopeWidth = 0.06f;
        [SerializeField, Range(3, 32)] private int ropeVisualSegments = 14;
        [SerializeField] private Color ropeColor = new Color(0.95f, 0.82f, 0.35f);
        [SerializeField, Min(10f)] private float aimRotationSpeed = 120f;
        [SerializeField, Min(1)] private int minimumSelectableRopeLength = 1;
        [SerializeField, Min(0f)] private float lengthSelectionRepeatDelay = 0.35f;
        [SerializeField, Min(0.01f)] private float lengthSelectionRepeatInterval = 0.1f;

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
        private Vector2 anchorPoint;
        private HookPoint activeHookPoint;
        private Vector2 keyboardAimDirection = new Vector2(1f, 1f).normalized;
        private float activeRopeLength;
        private float nextLengthIncreaseTime;
        private float nextLengthDecreaseTime;
        private int selectedRopeLength = 1;

        public bool IsAttached => ropeJoint != null && ropeJoint.enabled;
        public Vector2 AnchorPoint => anchorPoint;
        public HookPoint ActiveHookPoint => activeHookPoint;
        public Vector2 KeyboardAimDirection => keyboardAimDirection;
        public float ReleaseRefundRate => 1f;
        public int SelectedRopeLength => selectedRopeLength;
        public int MaximumSelectableRopeLength => GetMaximumSelectableRopeLength();
        public float ActiveRopeLength => activeRopeLength;
        public Color VisibleRopeColor => GetVisibleRopeColor();

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
            DetachAndRefund();
        }

        private void OnDestroy()
        {
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
            minimumSelectableRopeLength = Mathf.Clamp(
                minimumSelectableRopeLength,
                1,
                Mathf.FloorToInt(maximumShotDistance));
        }

        public bool TryAttach(Vector2 worldTarget)
        {
            if (IsAttached || (playerMover != null && !playerMover.IsGrounded))
            {
                return false;
            }

            if (!TryResolveAttachmentPoint(
                    worldTarget,
                    out Vector2 resolvedAnchor,
                    out HookPoint hookPoint))
            {
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
            ropeJoint.distance = selectedLength;
            ropeJoint.enabled = true;
            lineRenderer.enabled = true;
            audioFeedback.PlayHookAttached();
            return true;
        }

        public bool TryResolveCurrentAimAnchor(out Vector2 resolvedAnchor)
        {
            Vector2 worldTarget =
                body.position + keyboardAimDirection * maximumShotDistance;
            if (TryResolveAttachmentPoint(
                    worldTarget,
                    out resolvedAnchor,
                    out _))
            {
                return true;
            }

            float freePlatformLength = Mathf.Min(
                SelectedRopeLength,
                maximumShotDistance);
            resolvedAnchor =
                body.position + keyboardAimDirection * freePlatformLength;
            return freePlatformLength > 0f;
        }

        public void DetachAndRefund(bool playReleaseSound = false)
        {
            if (!IsAttached)
            {
                return;
            }

            // Disabling the joint must not erase the velocity built up by the pendulum.
            Vector2 preservedVelocity = body.linearVelocity;
            float preservedAngularVelocity = body.angularVelocity;
            ropeJoint.enabled = false;
            activeHookPoint = null;
            body.linearVelocity = preservedVelocity;
            body.angularVelocity = preservedAngularVelocity;
            lineRenderer.enabled = false;
            activeRopeLength = 0f;
            ClampSelectedRopeLength();
            if (playReleaseSound)
            {
                audioFeedback.PlayRopeReleased(preservedVelocity.magnitude);
            }
        }

        public bool CommitAttachedRopeAsPlatform(float permanentCost)
        {
            if (!IsAttached || permanentCost <= 0f ||
                !ropeResource.TrySpend(permanentCost))
            {
                return false;
            }

            ropeJoint.enabled = false;
            activeHookPoint = null;
            lineRenderer.enabled = false;
            activeRopeLength = 0f;
            ClampSelectedRopeLength();
            return true;
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
            float searchDistance = Mathf.Min(
                SelectedRopeLength,
                maximumShotDistance);
            RaycastHit2D[] hits = Physics2D.RaycastAll(
                origin,
                keyboardAimDirection,
                searchDistance);
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
                if (Vector2.Distance(origin, candidateAnchor) <=
                    searchDistance + 0.01f)
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

        private bool TryResolveAttachmentPoint(
            Vector2 worldTarget,
            out Vector2 resolvedAnchor,
            out HookPoint hookPoint)
        {
            resolvedAnchor = default;
            hookPoint = null;

            Vector2 origin = body.position;
            Vector2 offset = worldTarget - origin;
            if (offset.sqrMagnitude < 0.01f)
            {
                return false;
            }

            float shotDistance = Mathf.Min(SelectedRopeLength, maximumShotDistance);
            RaycastHit2D[] hits = Physics2D.RaycastAll(
                origin,
                offset.normalized,
                shotDistance);
            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider == null ||
                    hit.collider == bodyCollider ||
                    hit.collider.isTrigger)
                {
                    continue;
                }

                HookPoint candidateHook =
                    hit.collider.GetComponentInParent<HookPoint>();
                Vector2 candidateAnchor = candidateHook != null
                    ? candidateHook.GetAttachmentPoint(hit.point)
                    : hit.point;
                if (Vector2.Distance(origin, candidateAnchor) >
                    shotDistance + 0.01f)
                {
                    continue;
                }

                resolvedAnchor = candidateAnchor;
                hookPoint = candidateHook;
                return true;
            }

            return false;
        }

        private void UpdateSelectedRopeLength()
        {
            PrototypeRunController tutorial = GetComponent<PrototypeRunController>();
            if (tutorial != null &&
                gameObject.scene.name == "Tutorial" &&
                tutorial.CurrentTutorialSection < 2)
            {
                selectedRopeLength = 6;
                ResetLengthSelectionRepeat();
                return;
            }

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
                ChangeSelectedRopeLength(amount);
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

            ChangeSelectedRopeLength(amount);
            nextRepeatTime = Time.unscaledTime + lengthSelectionRepeatInterval;
        }

        private void ChangeSelectedRopeLength(int amount)
        {
            selectedRopeLength = Mathf.Clamp(
                selectedRopeLength + amount,
                minimumSelectableRopeLength,
                GetMaximumSelectableRopeLength());
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

        private void DrawAimGuide()
        {
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
            lineRenderer.positionCount = ropeVisualSegments;
            for (int i = 0; i < ropeVisualSegments; i++)
            {
                float t = i / (float)(ropeVisualSegments - 1);
                lineRenderer.SetPosition(i, GetAttachedRopePoint(t));
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
