using UnityEngine;

namespace HimoHito
{
    /// <summary>
    /// Left mouse press shoots a rope toward the cursor.
    /// Keeping the button held keeps the joint attached; releasing it refunds the rope.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(DistanceJoint2D), typeof(LineRenderer))]
    [RequireComponent(typeof(RopeResource))]
    public sealed class RopeController : MonoBehaviour
    {
        [SerializeField, Min(1f)] private float maximumShotDistance = 14f;
        [SerializeField, Min(0.01f)] private float ropeWidth = 0.08f;
        [SerializeField] private Color ropeColor = new Color(0.95f, 0.82f, 0.35f);

        private Rigidbody2D body;
        private Collider2D bodyCollider;
        private DistanceJoint2D ropeJoint;
        private LineRenderer lineRenderer;
        private RopeResource ropeResource;
        private Camera mainCamera;
        private Material runtimeMaterial;
        private Vector2 anchorPoint;
        private float spentLength;

        public bool IsAttached => ropeJoint != null && ropeJoint.enabled;
        public Vector2 AnchorPoint => anchorPoint;

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
                lineRenderer.enabled = false;
                return;
            }

            lineRenderer.enabled = true;
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

            ropeJoint.enabled = false;
            lineRenderer.enabled = false;
            ropeResource.Refund(spentLength);
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
    }
}
