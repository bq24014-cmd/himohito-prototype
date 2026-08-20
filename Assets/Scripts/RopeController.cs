using UnityEngine;

namespace HimoHito
{
    /// <summary>
    /// Arrow keys aim the rope. Holding E attaches it and releasing E partially refunds it.
    /// Mouse input remains available as an optional alternative.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(DistanceJoint2D), typeof(LineRenderer))]
    [RequireComponent(typeof(RopeResource), typeof(WeaveResource))]
    public sealed class RopeController : MonoBehaviour
    {
        [SerializeField, Min(1f)] private float maximumShotDistance = 14f;
        [SerializeField, Min(0.01f)] private float ropeWidth = 0.16f;
        [SerializeField, Min(0.01f)] private float minimumRopeWidth = 0.06f;
        [SerializeField, Range(3, 32)] private int ropeVisualSegments = 14;
        [SerializeField] private Color ropeColor = new Color(0.95f, 0.82f, 0.35f);
        [SerializeField, Min(10f)] private float aimRotationSpeed = 120f;
        [SerializeField] private Color aimGuideColor = new Color(0.55f, 0.65f, 0.8f, 0.55f);
        [SerializeField, Range(0f, 1f)] private float releaseRefundRate = 0.7f;
        [SerializeField, Min(1)] private int minimumSelectableRopeLength = 1;
        [SerializeField, Min(0f)] private float lengthSelectionRepeatDelay = 0.35f;
        [SerializeField, Min(0.01f)] private float lengthSelectionRepeatInterval = 0.1f;

        private Rigidbody2D body;
        private Collider2D bodyCollider;
        private DistanceJoint2D ropeJoint;
        private LineRenderer lineRenderer;
        private RopeResource ropeResource;
        private WeaveResource weaveResource;
        private SpriteRenderer bodyRenderer;
        private Camera mainCamera;
        private Material runtimeMaterial;
        private Vector2 anchorPoint;
        private HookPoint activeHookPoint;
        private Vector2 keyboardAimDirection = new Vector2(1f, 1f).normalized;
        private float spentLength;
        private float nextLengthIncreaseTime;
        private float nextLengthDecreaseTime;
        private int selectedRopeLength = 1;

        public bool IsAttached => ropeJoint != null && ropeJoint.enabled;
        public Vector2 AnchorPoint => anchorPoint;
        public HookPoint ActiveHookPoint => activeHookPoint;
        public Vector2 KeyboardAimDirection => keyboardAimDirection;
        public float ReleaseRefundRate => releaseRefundRate;
        public int SelectedRopeLength => selectedRopeLength;
        public int MaximumSelectableRopeLength => GetMaximumSelectableRopeLength();
        public float ActiveRopeLength => spentLength;

        /// <summary>
        /// Checks the same curved rope that is drawn on screen against a box-shaped area.
        /// This keeps hazards tied to the visible rope instead of the player's collider.
        /// </summary>
        public bool IntersectsAttachedRope(BoxCollider2D area)
        {
            if (!IsAttached || area == null)
            {
                return false;
            }

            Vector2 previousPoint = GetAttachedRopePoint(0f);
            for (int i = 1; i < ropeVisualSegments; i++)
            {
                float t = i / (float)(ropeVisualSegments - 1);
                Vector2 currentPoint = GetAttachedRopePoint(t);
                if (SegmentIntersectsBox(previousPoint, currentPoint, area))
                {
                    return true;
                }

                previousPoint = currentPoint;
            }

            return false;
        }

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            bodyCollider = GetComponent<Collider2D>();
            ropeJoint = GetComponent<DistanceJoint2D>();
            lineRenderer = GetComponent<LineRenderer>();
            ropeResource = GetComponent<RopeResource>();
            weaveResource = GetComponent<WeaveResource>();
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
            bool aimChanged = UpdateKeyboardAim();

            if (Input.GetKeyDown(KeyCode.E) || (Input.GetKey(KeyCode.E) && aimChanged && !IsAttached))
            {
                Vector2 keyboardTarget = body.position + keyboardAimDirection * maximumShotDistance;
                TryAttach(keyboardTarget);
            }

            if (Input.GetKeyUp(KeyCode.E))
            {
                DetachAndRefund(true);
            }

            if (Input.GetMouseButtonDown(0))
            {
                TryAttachTowardCursor();
            }

            if (Input.GetMouseButtonUp(0))
            {
                DetachAndRefund(true);
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
            releaseRefundRate = Mathf.Clamp01(releaseRefundRate);
            lengthSelectionRepeatDelay = Mathf.Max(0f, lengthSelectionRepeatDelay);
            lengthSelectionRepeatInterval = Mathf.Max(0.01f, lengthSelectionRepeatInterval);
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

            Vector2 origin = body.position;
            Vector2 offset = worldTarget - origin;
            if (offset.sqrMagnitude < 0.01f)
            {
                return false;
            }

            float selectedLength = SelectedRopeLength;
            float shotDistance = Mathf.Min(selectedLength, maximumShotDistance);
            RaycastHit2D[] hits = Physics2D.RaycastAll(origin, offset.normalized, shotDistance);
            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider == null || hit.collider == bodyCollider)
                {
                    continue;
                }

                HookPoint hookPoint = hit.collider.GetComponentInParent<HookPoint>();
                if (hookPoint == null)
                {
                    continue;
                }

                if (!ropeResource.TrySpend(selectedLength))
                {
                    return false;
                }

                spentLength = selectedLength;
                anchorPoint = hit.point;
                activeHookPoint = hookPoint;
                ropeJoint.connectedBody = null;
                ropeJoint.connectedAnchor = anchorPoint;
                ropeJoint.distance = selectedLength;
                ropeJoint.enabled = true;
                lineRenderer.enabled = true;
                return true;
            }

            return false;
        }

        public void DetachAndRefund(bool awardWeaveThread = false)
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
            ropeResource.Refund(spentLength * releaseRefundRate);
            if (awardWeaveThread && weaveResource != null)
            {
                weaveResource.AddThread();
            }
            spentLength = 0f;
            ClampSelectedRopeLength();
        }

        public void RestoreSelectedRopeLength(int length)
        {
            selectedRopeLength = length;
            ClampSelectedRopeLength();
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
            lineRenderer.startColor = aimGuideColor;
            lineRenderer.endColor = aimGuideColor;
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
            Vector2 start = body.position;
            Vector2 end = anchorPoint;
            float directDistance = Vector2.Distance(start, end);
            float slack = Mathf.Max(0f, spentLength - directDistance);
            Vector2 point = Vector2.Lerp(start, end, t);
            return point + Vector2.down * (slack * 4f * t * (1f - t));
        }

        private static bool SegmentIntersectsBox(
            Vector2 worldStart,
            Vector2 worldEnd,
            BoxCollider2D area)
        {
            Vector2 start = area.transform.InverseTransformPoint(worldStart);
            Vector2 end = area.transform.InverseTransformPoint(worldEnd);
            Vector2 halfSize = area.size * 0.5f;
            Vector2 minimum = area.offset - halfSize;
            Vector2 maximum = area.offset + halfSize;
            Vector2 direction = end - start;
            float enter = 0f;
            float exit = 1f;

            return ClipSegmentAxis(start.x, direction.x, minimum.x, maximum.x, ref enter, ref exit) &&
                   ClipSegmentAxis(start.y, direction.y, minimum.y, maximum.y, ref enter, ref exit);
        }

        private static bool ClipSegmentAxis(
            float start,
            float direction,
            float minimum,
            float maximum,
            ref float enter,
            ref float exit)
        {
            if (Mathf.Approximately(direction, 0f))
            {
                return start >= minimum && start <= maximum;
            }

            float first = (minimum - start) / direction;
            float second = (maximum - start) / direction;
            if (first > second)
            {
                (first, second) = (second, first);
            }

            enter = Mathf.Max(enter, first);
            exit = Mathf.Min(exit, second);
            return enter <= exit;
        }

        private float GetVisibleRopeWidth()
        {
            if (ropeResource == null)
            {
                return ropeWidth;
            }

            float committedRemainingLength = ropeResource.CurrentLength;
            if (IsAttached)
            {
                committedRemainingLength += spentLength;
            }

            float remainingRatio = ropeResource.MaximumLength <= 0f
                ? 0f
                : committedRemainingLength / ropeResource.MaximumLength;
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
