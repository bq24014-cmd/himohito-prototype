using UnityEngine;

namespace HimoHito
{
    /// <summary>Spike contact cuts an active rope and lets the player fall.</summary>
    [RequireComponent(typeof(Collider2D))]
    public sealed class RopeSpikeHazard : MonoBehaviour
    {
        private Collider2D detectionArea;
        private RopeController playerRope;
        private Collider2D playerCollider;

        private void Awake()
        {
            CacheReferences();
        }

        private void FixedUpdate()
        {
            CacheReferences();
            if (detectionArea == null ||
                !detectionArea.enabled ||
                playerCollider == null ||
                !playerCollider.enabled ||
                playerRope == null ||
                !playerRope.IsAttached)
            {
                return;
            }

            // A fast pendulum can cross a small trigger between callbacks.
            // Confirm the physical overlap every physics step as a fallback.
            if (Physics2D.Distance(detectionArea, playerCollider).isOverlapped)
            {
                playerRope.DetachAndRefund();
            }
        }

        private void OnTriggerEnter2D(Collider2D other) => Cut(other);
        private void OnTriggerStay2D(Collider2D other) => Cut(other);

        private static void Cut(Collider2D other)
        {
            RopeController rope = other.GetComponentInParent<RopeController>();
            if (rope == null && other.attachedRigidbody != null)
            {
                rope = other.attachedRigidbody.GetComponent<RopeController>();
            }
            if (rope != null && rope.IsAttached)
            {
                rope.DetachAndRefund();
            }
        }

        private void CacheReferences()
        {
            if (detectionArea == null || !detectionArea.enabled)
            {
                foreach (Collider2D candidate in GetComponents<Collider2D>())
                {
                    if (candidate.enabled && candidate.isTrigger)
                    {
                        detectionArea = candidate;
                        break;
                    }
                }
            }

            if (playerRope == null)
            {
                playerRope = FindFirstObjectByType<RopeController>();
            }
            if (playerRope != null && playerCollider == null)
            {
                playerCollider = playerRope.GetComponent<Collider2D>();
            }
        }
    }

    /// <summary>
    /// Draws a spike as a painted wooden triangle so the hazard belongs in
    /// the night-time toy room instead of looking like a prototype rectangle.
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class ToySpikeVisual : MonoBehaviour
    {
        private const string FaceName = "Wooden Toy Spike Face";
        private const string PaintedSpikeResourcePath = "Art/WoodenToySpike-v1";
        private const int TextureSize = 64;
        private static Sprite sharedSprite;

        public bool Refresh()
        {
            bool changed = false;
            SpriteRenderer sourceRenderer = GetComponent<SpriteRenderer>();
            if (sourceRenderer.enabled)
            {
                sourceRenderer.enabled = false;
                changed = true;
            }

            Transform face = transform.Find(FaceName);
            if (face == null)
            {
                GameObject faceObject = new GameObject(FaceName);
                face = faceObject.transform;
                face.SetParent(transform, false);
                changed = true;
            }

            if (face.localPosition != Vector3.zero)
            {
                face.localPosition = Vector3.zero;
                changed = true;
            }
            if (face.localRotation != Quaternion.identity)
            {
                face.localRotation = Quaternion.identity;
                changed = true;
            }
            Sprite targetSprite = GetSharedSprite();
            // Fit the new artwork inside the same local hazard bounds; never resize the collider.
            Vector3 visualScale = new Vector3(1f / targetSprite.bounds.size.x,
                1f / targetSprite.bounds.size.y, 1f);
            if (face.localScale != visualScale)
            {
                face.localScale = visualScale;
                changed = true;
            }

            if (!face.TryGetComponent(out SpriteRenderer faceRenderer))
            {
                faceRenderer = face.gameObject.AddComponent<SpriteRenderer>();
                changed = true;
            }

            if (faceRenderer.sprite != targetSprite)
            {
                faceRenderer.sprite = targetSprite;
                changed = true;
            }
            if (faceRenderer.color != Color.white)
            {
                faceRenderer.color = Color.white;
                changed = true;
            }
            if (faceRenderer.sortingLayerID != sourceRenderer.sortingLayerID)
            {
                faceRenderer.sortingLayerID = sourceRenderer.sortingLayerID;
                changed = true;
            }
            if (faceRenderer.sortingOrder != sourceRenderer.sortingOrder + 2)
            {
                faceRenderer.sortingOrder = sourceRenderer.sortingOrder + 2;
                changed = true;
            }
            return changed;
        }

        private void OnEnable()
        {
            Refresh();
        }

        private static Sprite GetSharedSprite()
        {
            Sprite paintedSprite = TutorialFirstSectionVisuals.LoadProcessedToySprite(
                PaintedSpikeResourcePath);
            if (paintedSprite != null) return paintedSprite;

            // Retain a visible hazard if the art has not finished importing.
            if (sharedSprite != null)
            {
                return sharedSprite;
            }

            Color clear = new Color(0f, 0f, 0f, 0f);
            Color litWood = new Color(0.95f, 0.36f, 0.29f, 1f);
            Color warmWood = new Color(0.78f, 0.18f, 0.20f, 1f);
            Color shadedWood = new Color(0.49f, 0.10f, 0.16f, 1f);
            Color rim = new Color(1f, 0.58f, 0.38f, 1f);
            Color[] pixels = new Color[TextureSize * TextureSize];

            for (int y = 0; y < TextureSize; y++)
            {
                float vertical = (y + 0.5f) / TextureSize;
                float halfWidth = 0.47f * (1f - vertical);
                for (int x = 0; x < TextureSize; x++)
                {
                    float horizontal = (x + 0.5f) / TextureSize - 0.5f;
                    Color color = clear;
                    if (vertical >= 0.04f &&
                        Mathf.Abs(horizontal) <= halfWidth)
                    {
                        float edgeDistance = halfWidth - Mathf.Abs(horizontal);
                        if (edgeDistance < 0.025f || vertical < 0.075f)
                        {
                            color = shadedWood;
                        }
                        else if (horizontal < -0.08f)
                        {
                            color = litWood;
                        }
                        else if (horizontal > 0.18f)
                        {
                            color = shadedWood;
                        }
                        else
                        {
                            color = warmWood;
                        }

                        if (horizontal < -0.13f &&
                            edgeDistance > 0.055f &&
                            vertical > 0.18f &&
                            vertical < 0.82f)
                        {
                            color = Color.Lerp(color, rim, 0.55f);
                        }
                    }
                    pixels[y * TextureSize + x] = color;
                }
            }

            Texture2D texture = new Texture2D(
                TextureSize,
                TextureSize,
                TextureFormat.RGBA32,
                false)
            {
                name = "WoodenToySpikeTexture",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave
            };
            texture.SetPixels(pixels);
            texture.Apply();

            sharedSprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, TextureSize, TextureSize),
                new Vector2(0.5f, 0.5f),
                TextureSize);
            sharedSprite.name = "WoodenToySpike";
            sharedSprite.hideFlags = HideFlags.HideAndDontSave;
            return sharedSprite;
        }
    }

    /// <summary>
    /// A flashlight hazard that is disabled when a generated rope platform lies
    /// between its source and the player. The visible spot fades to show success.
    /// </summary>
    [RequireComponent(typeof(Collider2D), typeof(SpriteRenderer))]
    public sealed class PlatformOccludedLightHazard : MonoBehaviour
    {
        [SerializeField] private Transform lightSource;
        [SerializeField, Range(0.02f, 1f)] private float blockedAlpha = 0.08f;
        [SerializeField, Range(0.02f, 1f)] private float activeAlpha = 0.62f;
        [SerializeField, Min(0.01f)] private float fadeDuration = 0.4f;

        private SpriteRenderer spotRenderer;
        private Collider2D triggerArea;
        private RopeController playerRope;
        private Collider2D playerCollider;
        private float currentAlpha;

        public bool IsBlocked { get; private set; }

        public void Configure(Transform source)
        {
            lightSource = source;
        }

        private void Awake()
        {
            spotRenderer = GetComponent<SpriteRenderer>();
            triggerArea = GetComponent<Collider2D>();
            playerRope = FindFirstObjectByType<RopeController>();
            if (playerRope != null)
            {
                playerCollider = playerRope.GetComponent<Collider2D>();
            }
            currentAlpha = activeAlpha;
        }

        private void FixedUpdate()
        {
            if (playerRope == null || playerCollider == null || lightSource == null)
            {
                return;
            }

            IsBlocked = IsBlockedByGeneratedPlatform();
            float targetAlpha = IsBlocked ? blockedAlpha : activeAlpha;
            currentAlpha = Mathf.MoveTowards(
                currentAlpha,
                targetAlpha,
                Time.fixedDeltaTime / Mathf.Max(0.01f, fadeDuration));
            Color color = spotRenderer.color;
            color.a = currentAlpha;
            spotRenderer.color = color;

            if (!IsBlocked && playerRope.IsAttached &&
                Physics2D.Distance(triggerArea, playerCollider).isOverlapped)
            {
                playerRope.DetachAndRefund();
            }
        }

        private bool IsBlockedByGeneratedPlatform()
        {
            Vector2 source = lightSource.position;
            Vector2 target = transform.position;
            RaycastHit2D[] hits = Physics2D.LinecastAll(source, target);
            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider == null || hit.collider == playerCollider)
                {
                    continue;
                }
                if (hit.collider.TryGetComponent(out GeneratedRopePlatform _))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
