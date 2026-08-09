using UnityEngine;

namespace HimoHito
{
    /// <summary>
    /// Arrow keys aim the rope. Holding E attaches it and releasing E partially refunds it.
    /// Mouse input remains available as an optional alternative.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(DistanceJoint2D), typeof(LineRenderer))]
    [RequireComponent(typeof(RopeResource))]
    public sealed class RopeController : MonoBehaviour
    {
        [SerializeField, Min(1f)] private float maximumShotDistance = 14f;
        [SerializeField, Min(0.01f)] private float ropeWidth = 0.08f;
        [SerializeField] private Color ropeColor = new Color(0.95f, 0.82f, 0.35f);
        [SerializeField, Min(0.5f)] private float aimGuideLength = 3f;
        [SerializeField, Min(10f)] private float aimRotationSpeed = 120f;
        [SerializeField] private Color aimGuideColor = new Color(0.55f, 0.65f, 0.8f, 0.55f);
        [SerializeField, Range(0f, 1f)] private float releaseRefundRate = 0.7f;

        private Rigidbody2D body;
        private Collider2D bodyCollider;
        private DistanceJoint2D ropeJoint;
        private LineRenderer lineRenderer;
        private RopeResource ropeResource;
        private Camera mainCamera;
        private Material runtimeMaterial;
        private Vector2 anchorPoint;
        private Vector2 keyboardAimDirection = new Vector2(1f, 1f).normalized;
        private float spentLength;

        public bool IsAttached => ropeJoint != null && ropeJoint.enabled;
        public Vector2 AnchorPoint => anchorPoint;
        public Vector2 KeyboardAimDirection => keyboardAimDirection;
        public float ReleaseRefundRate => releaseRefundRate;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            bodyCollider = GetComponent<Collider2D>();
            ropeJoint = GetComponent<DistanceJoint2D>();
            lineRenderer = GetComponent<LineRenderer>();
            ropeResource = GetComponent<RopeResource>();
            mainCamera = Camera.main;

            ropeJoint.enabled = false;
            ropeJoint.autoConfigureConnectedAnchor = false;
            ropeJoint.autoConfigureDistance = false;
            ropeJoint.maxDistanceOnly = true;
            ropeJoint.enableCollision = false;

            lineRenderer.enabled = false;
            lineRenderer.positionCount = 2;
            lineRenderer.useWorldSpace = true;
            lineRenderer.startWidth = ropeWidth;
            lineRenderer.endWidth = ropeWidth;
            lineRenderer.startColor = ropeColor;
            lineRenderer.endColor = ropeColor;

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
            bool aimChanged = UpdateKeyboardAim();

            if (Input.GetKeyDown(KeyCode.E) || (Input.GetKey(KeyCode.E) && aimChanged && !IsAttached))
            {
                Vector2 keyboardTarget = body.position + keyboardAimDirection * maximumShotDistance;
                TryAttach(keyboardTarget);
            }

            if (Input.GetKeyUp(KeyCode.E))
            {
                DetachAndRefund();
            }

            if (Input.GetMouseButtonDown(0))
            {
                TryAttachTowardCursor();
            }

            if (Input.GetMouseButtonUp(0))
            {
                DetachAndRefund();
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
            lineRenderer.startWidth = ropeWidth;
            lineRenderer.endWidth = ropeWidth;
            lineRenderer.startColor = ropeColor;
            lineRenderer.endColor = ropeColor;
            lineRenderer.SetPosition(0, body.position);
            lineRenderer.SetPosition(1, anchorPoint);
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
            releaseRefundRate = Mathf.Clamp01(releaseRefundRate);
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

            RaycastHit2D[] hits = Physics2D.RaycastAll(origin, offset.normalized, maximumShotDistance);
            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider == null || hit.collider == bodyCollider)
                {
                    continue;
                }

                if (hit.collider.GetComponentInParent<HookPoint>() == null)
                {
                    continue;
                }

                float requiredLength = Vector2.Distance(origin, hit.point);
                if (!ropeResource.TrySpend(requiredLength))
                {
                    return false;
                }

                spentLength = requiredLength;
                anchorPoint = hit.point;
                ropeJoint.connectedBody = null;
                ropeJoint.connectedAnchor = anchorPoint;
                ropeJoint.distance = requiredLength;
                ropeJoint.enabled = true;
                lineRenderer.enabled = true;
                return true;
            }

            return false;
        }

        public void DetachAndRefund()
        {
            if (!IsAttached)
            {
                return;
            }

            // Disabling the joint must not erase the velocity built up by the pendulum.
            Vector2 preservedVelocity = body.linearVelocity;
            float preservedAngularVelocity = body.angularVelocity;
            ropeJoint.enabled = false;
            body.linearVelocity = preservedVelocity;
            body.angularVelocity = preservedAngularVelocity;
            lineRenderer.enabled = false;
            ropeResource.Refund(spentLength * releaseRefundRate);
            spentLength = 0f;
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
            lineRenderer.startWidth = ropeWidth * 0.45f;
            lineRenderer.endWidth = ropeWidth * 0.45f;
            lineRenderer.startColor = aimGuideColor;
            lineRenderer.endColor = aimGuideColor;
            lineRenderer.SetPosition(0, body.position);
            lineRenderer.SetPosition(1, body.position + keyboardAimDirection * aimGuideLength);
        }
    }
}
