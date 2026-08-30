using UnityEngine;

namespace HimoHito
{
    /// <summary>
    /// Releases an attached rope when the player enters a main-stage hazard.
    /// Section eight can instead test whether a generated rope platform lies
    /// between a visible light source and the player.
    /// </summary>
    public sealed class MainStageRopeHazard : MonoBehaviour
    {
        [SerializeField]
        private bool canBeBlockedByGeneratedRopePlatform;

        [SerializeField, Min(0.05f)]
        private float shadowDetectionRadius = 0.65f;

        [SerializeField, Min(0.1f)]
        private float minimumShieldExtension = 1.25f;

        [SerializeField, Min(1f)]
        private float maximumShadowCasterDistance = 14f;

        [SerializeField, Min(1f)]
        private float shadowVisualLength = 8f;

        [SerializeField, Min(0.05f)]
        private float shadowStartWidth = 0.65f;

        [SerializeField, Min(0.05f)]
        private float shadowEndWidth = 1.8f;

        [SerializeField]
        private Color previewShadowColor = new Color(0.18f, 0.12f, 0.3f, 0.28f);

        [SerializeField]
        private Color placedShadowColor = new Color(0.1f, 0.06f, 0.2f, 0.42f);

        [SerializeField]
        private Color safeShadowColor = new Color(0.06f, 0.035f, 0.14f, 0.62f);

        private SpriteRenderer spotRenderer;
        private Collider2D detectionArea;
        private RopeController playerRope;
        private RopePlatformBuilder platformBuilder;
        private Transform shadowSource;
        private Color visibleColor = Color.white;
        private bool isBlocked;
        private bool isPlacementPreview;
        private Vector2 previewStart;
        private Vector2 previewEnd;
        private GameObject shadowVisualObject;
        private LineRenderer shadowRenderer;
        private Material shadowMaterial;

        public bool IsBlocked => isBlocked;
        public bool IsPlacementPreview => isPlacementPreview;

        public void ConfigureRopePlatformBlocking(bool canBeBlocked)
        {
            canBeBlockedByGeneratedRopePlatform = canBeBlocked;
        }

        public void ConfigureShadowSource(Transform source)
        {
            shadowSource = source;
        }

        private void Awake()
        {
            detectionArea = GetComponent<Collider2D>();
            if (detectionArea != null)
            {
                detectionArea.isTrigger = true;
            }

            playerRope = FindFirstObjectByType<RopeController>();
            platformBuilder = FindFirstObjectByType<RopePlatformBuilder>();

            spotRenderer = GetComponent<SpriteRenderer>();
            if (spotRenderer != null)
            {
                visibleColor = spotRenderer.color;
            }

            EnsureShadowRenderer();
        }

        private void Update()
        {
            UpdatePlacementPreview();
            UpdateShadowVisual();

            // The flashlight remains on. Only the shadow corridor becomes dark.
            if (spotRenderer != null && spotRenderer.color != visibleColor)
            {
                spotRenderer.color = visibleColor;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            TryDetach(other);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            TryDetach(other);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.GetComponentInParent<RopeController>() != null)
            {
                SetBlocked(false);
            }
        }

        private void OnDisable()
        {
            isPlacementPreview = false;
            SetBlocked(false);
            if (shadowRenderer != null)
            {
                shadowRenderer.enabled = false;
            }

            if (spotRenderer != null)
            {
                spotRenderer.color = visibleColor;
            }
        }

        private void OnDestroy()
        {
            if (shadowVisualObject != null)
            {
                Destroy(shadowVisualObject);
            }

            if (shadowMaterial != null)
            {
                Destroy(shadowMaterial);
            }
        }

        private void TryDetach(Collider2D other)
        {
            RopeController contactedRope = other.GetComponentInParent<RopeController>();
            if (contactedRope == null)
            {
                return;
            }

            if (!contactedRope.IsAttached)
            {
                SetBlocked(false);
                return;
            }

            if (canBeBlockedByGeneratedRopePlatform &&
                TryFindBlockingPlatform(
                    contactedRope.transform.position,
                    out _))
            {
                SetBlocked(true);
                return;
            }

            SetBlocked(false);
            contactedRope.DetachAndRefund();
        }

        private void SetBlocked(bool blocked)
        {
            isBlocked = canBeBlockedByGeneratedRopePlatform && blocked;
        }

        private void UpdatePlacementPreview()
        {
            isPlacementPreview = false;
            if (!canBeBlockedByGeneratedRopePlatform || shadowSource == null)
            {
                return;
            }

            if (playerRope == null)
            {
                playerRope = FindFirstObjectByType<RopeController>();
            }
            if (platformBuilder == null)
            {
                platformBuilder = FindFirstObjectByType<RopePlatformBuilder>();
            }

            if (playerRope == null ||
                playerRope.IsAttached ||
                platformBuilder == null ||
                !platformBuilder.CanBuildCurrentPlatform ||
                !playerRope.TryResolveCurrentAimAnchor(out Vector2 platformEnd))
            {
                return;
            }

            previewStart = playerRope.transform.position;
            previewEnd = platformEnd;
            isPlacementPreview = PlatformExtendsTowardSource(
                previewStart,
                previewEnd,
                previewStart);
        }

        private bool TryFindBlockingPlatform(
            Vector2 playerPosition,
            out GeneratedRopePlatform blockingPlatform)
        {
            blockingPlatform = null;
            if (shadowSource == null)
            {
                return false;
            }

            Vector2 sourcePosition = shadowSource.position;
            Vector2 sourceToPlayer = playerPosition - sourcePosition;
            float distance = sourceToPlayer.magnitude;
            if (distance <= 0.01f)
            {
                return false;
            }

            RaycastHit2D[] hits = Physics2D.CircleCastAll(
                sourcePosition,
                shadowDetectionRadius,
                sourceToPlayer / distance,
                distance);
            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider == null || hit.collider.isTrigger)
                {
                    continue;
                }

                GeneratedRopePlatform generated =
                    hit.collider.GetComponentInParent<GeneratedRopePlatform>();
                if (generated == null ||
                    !PlatformExtendsTowardSource(
                        generated.Start,
                        generated.End,
                        playerPosition))
                {
                    continue;
                }

                blockingPlatform = generated;
                return true;
            }

            return false;
        }

        private bool PlatformExtendsTowardSource(
            Vector2 start,
            Vector2 end,
            Vector2 playerPosition)
        {
            if (shadowSource == null)
            {
                return false;
            }

            Vector2 playerToSource =
                (Vector2)shadowSource.position - playerPosition;
            if (playerToSource.sqrMagnitude <= 0.0001f)
            {
                return false;
            }

            playerToSource.Normalize();
            float startExtension = Vector2.Dot(
                start - playerPosition,
                playerToSource);
            float endExtension = Vector2.Dot(
                end - playerPosition,
                playerToSource);
            float maximumExtension = Mathf.Max(startExtension, endExtension);
            if (maximumExtension < minimumShieldExtension)
            {
                return false;
            }

            Vector2 furthestPoint = startExtension >= endExtension ? start : end;
            Vector2 playerToFurthest = furthestPoint - playerPosition;
            float lateralOffset = Mathf.Abs(
                playerToFurthest.x * playerToSource.y -
                playerToFurthest.y * playerToSource.x);
            float permittedOffset = shadowDetectionRadius +
                maximumExtension * 0.22f;
            return lateralOffset <= permittedOffset;
        }

        private void UpdateShadowVisual()
        {
            EnsureShadowRenderer();
            if (shadowRenderer == null ||
                !canBeBlockedByGeneratedRopePlatform ||
                shadowSource == null)
            {
                if (shadowRenderer != null)
                {
                    shadowRenderer.enabled = false;
                }
                return;
            }

            if (TryFindNearestShadowCaster(out GeneratedRopePlatform caster))
            {
                DrawShadow(
                    caster.Start,
                    caster.End,
                    isBlocked ? safeShadowColor : placedShadowColor);
                return;
            }

            if (isPlacementPreview)
            {
                DrawShadow(previewStart, previewEnd, previewShadowColor);
                return;
            }

            shadowRenderer.enabled = false;
        }

        private bool TryFindNearestShadowCaster(
            out GeneratedRopePlatform nearestCaster)
        {
            nearestCaster = null;
            Vector2 sourcePosition = shadowSource.position;
            float nearestDistanceSquared =
                maximumShadowCasterDistance * maximumShadowCasterDistance;

            GeneratedRopePlatform[] platforms =
                FindObjectsByType<GeneratedRopePlatform>(
                    FindObjectsSortMode.None);
            foreach (GeneratedRopePlatform platform in platforms)
            {
                if (platform == null)
                {
                    continue;
                }

                Vector2 midpoint = (platform.Start + platform.End) * 0.5f;
                float distanceSquared =
                    (midpoint - sourcePosition).sqrMagnitude;
                if (distanceSquared > nearestDistanceSquared)
                {
                    continue;
                }

                nearestDistanceSquared = distanceSquared;
                nearestCaster = platform;
            }

            return nearestCaster != null;
        }

        private void DrawShadow(
            Vector2 platformStart,
            Vector2 platformEnd,
            Color color)
        {
            Vector2 midpoint = (platformStart + platformEnd) * 0.5f;
            Vector2 awayFromSource =
                midpoint - (Vector2)shadowSource.position;
            if (awayFromSource.sqrMagnitude <= 0.0001f)
            {
                awayFromSource = Vector2.left;
            }
            else
            {
                awayFromSource.Normalize();
            }

            Vector2 shadowStart = midpoint + awayFromSource * 0.12f;
            Vector2 shadowEnd =
                shadowStart + awayFromSource * shadowVisualLength;

            shadowRenderer.enabled = true;
            shadowRenderer.startWidth = shadowStartWidth;
            shadowRenderer.endWidth = shadowEndWidth;
            shadowRenderer.startColor = color;
            shadowRenderer.endColor = new Color(
                color.r,
                color.g,
                color.b,
                color.a * 0.58f);
            shadowRenderer.SetPosition(0, shadowStart);
            shadowRenderer.SetPosition(1, shadowEnd);
        }

        private void EnsureShadowRenderer()
        {
            if (shadowRenderer != null)
            {
                return;
            }

            shadowVisualObject = new GameObject(
                "Main Section 8 Rope Shadow Corridor");
            shadowRenderer = shadowVisualObject.AddComponent<LineRenderer>();
            shadowRenderer.useWorldSpace = true;
            shadowRenderer.positionCount = 2;
            shadowRenderer.numCapVertices = 8;
            shadowRenderer.numCornerVertices = 4;
            shadowRenderer.sortingOrder = 5;
            shadowRenderer.enabled = false;

            Shader spriteShader = Shader.Find("Sprites/Default");
            if (spriteShader != null)
            {
                shadowMaterial = new Material(spriteShader)
                {
                    name = "Section 8 Rope Shadow Material"
                };
                shadowRenderer.material = shadowMaterial;
            }
        }
    }
}
