using UnityEngine;

namespace HimoHito
{
    /// <summary>
    /// Shrinks only the player's visible body as rope is spent.
    /// The player transform, Rigidbody2D, and Collider2D keep their original size.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RopeResource), typeof(SpriteRenderer))]
    public sealed class RopeBodyVisual : MonoBehaviour
    {
        private const string PlayerArtResourcePath = "Art/HimoHitoPlayer-v1";
        private const string WalkArtResourcePath = "Art/HimoHitoWalk-v1";
        private const string JumpArtResourcePath = "Art/HimoHitoJump-v1";
        private const int WalkColumns = 4;
        private const int WalkRows = 2;
        private const int WalkFrameCount = WalkColumns * WalkRows;
        private const int JumpColumns = 3;
        private const int JumpRows = 2;
        private const int JumpFrameCount = JumpColumns * JumpRows;
        private const float SpritePixelsPerUnit = 100f;

        private static Sprite[] cachedWalkFrames;
        private static Vector2 cachedWalkContentSize;
        private static Sprite[] cachedJumpFrames;
        private static Vector2 cachedJumpContentSize;

        [SerializeField, Range(0.2f, 1f)] private float minimumVisualScale = 0.65f;

        [Header("Walking animation")]
        [SerializeField, Min(0.01f)] private float minimumWalkSpeed = 0.35f;
        [SerializeField, Min(1f)] private float minimumWalkFramesPerSecond = 5f;
        [SerializeField, Min(1f)] private float maximumWalkFramesPerSecond = 9f;
        [SerializeField, Min(0.1f)] private float fullWalkAnimationSpeed = 6.5f;

        [Header("Jump animation")]
        [SerializeField, Min(0f)] private float launchFrameDuration = 0.08f;
        [SerializeField, Min(0.1f)] private float fastRiseSpeed = 4f;
        [SerializeField, Min(0f)] private float apexSpeed = 0.8f;
        [SerializeField, Min(0.1f)] private float fastFallSpeed = 4f;

        [Header("Landing squash")]
        [SerializeField, Min(0.05f)] private float landingDuration = 0.18f;
        [SerializeField, Min(0f)] private float minimumLandingSpeed = 1.5f;
        [SerializeField, Min(0.01f)] private float fullLandingSpeed = 10f;
        [SerializeField, Range(0f, 0.3f)] private float maximumHorizontalSquash = 0.10f;
        [SerializeField, Range(0f, 0.3f)] private float maximumVerticalSquash = 0.14f;

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
        private Sprite standingSprite;
        private Sprite[] walkingFrames;
        private Sprite[] jumpingFrames;
        private bool usesCharacterArt;
        private bool isWalking;
        private float currentBaseScale = 1f;
        private float walkFrameProgress;
        private float airborneElapsed;
        private float fastestFallSpeed;
        private float landingElapsed;
        private float landingStrength;
        private bool wasGrounded;

        public Vector2 RopeOrigin
        {
            get
            {
                if (usesCharacterArt && visualRenderer != null)
                {
                    Bounds visualBounds = visualRenderer.bounds;
                    return new Vector2(visualBounds.center.x, visualBounds.max.y);
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
            sourceRenderer = GetComponent<SpriteRenderer>();
            CreateVisualBody();
            UpdateRemainingLengthScale();
            landingElapsed = landingDuration;
            wasGrounded = playerMover != null && playerMover.IsGrounded;
            ApplyVisualScale();
        }

        private void LateUpdate()
        {
            UpdateLandingState();

            if (ropeController == null || !ropeController.IsAttached)
            {
                UpdateRemainingLengthScale();
            }

            UpdateCharacterAnimation();
            ApplyVisualScale();
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
                    0.9f / Mathf.Max(0.01f, spriteSize.x),
                    1.15f / Mathf.Max(0.01f, spriteSize.y));
                standingVisualBaseScale = visualBaseScale;
                standingSprite = playerArt;
                walkingFrames = LoadWalkFrames(out Vector2 walkContentSize);
                if (walkingFrames != null && walkContentSize.sqrMagnitude > 0f)
                {
                    walkingVisualBaseScale = new Vector2(
                        0.9f / Mathf.Max(0.01f, walkContentSize.x),
                        1.15f / Mathf.Max(0.01f, walkContentSize.y));
                }
                jumpingFrames = LoadJumpFrames(out Vector2 jumpContentSize);
                if (jumpingFrames != null && jumpContentSize.sqrMagnitude > 0f)
                {
                    jumpingVisualBaseScale = new Vector2(
                        0.9f / Mathf.Max(0.01f, jumpContentSize.x),
                        1.15f / Mathf.Max(0.01f, jumpContentSize.y));
                }
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
            bool shouldShowJump =
                usesCharacterArt &&
                jumpingFrames != null &&
                jumpingFrames.Length == JumpFrameCount &&
                playerMover != null &&
                !playerMover.IsGrounded &&
                (ropeController == null || !ropeController.IsAttached);

            if (shouldShowJump)
            {
                UpdateJumpAnimation();
                return;
            }

            airborneElapsed = 0f;
            UpdateWalkAnimation();
        }

        private void UpdateJumpAnimation()
        {
            if (visualRenderer == null || body == null)
            {
                return;
            }

            airborneElapsed += Time.deltaTime;
            float verticalSpeed = body.linearVelocity.y;
            int frameIndex;

            if (airborneElapsed <= launchFrameDuration)
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

            if (Mathf.Abs(body.linearVelocity.x) >= minimumWalkSpeed)
            {
                visualRenderer.flipX = body.linearVelocity.x < 0f;
            }

            visualRenderer.sprite = jumpingFrames[frameIndex];
            visualBaseScale = jumpingVisualBaseScale;
            isWalking = false;
            walkFrameProgress = 0f;
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
                    visualBaseScale = standingVisualBaseScale;
                }

                isWalking = false;
                walkFrameProgress = 0f;
                return;
            }

            isWalking = true;
            visualRenderer.flipX = body.linearVelocity.x < 0f;
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
                ref cachedJumpFrames,
                ref cachedJumpContentSize,
                out contentSize);
        }

        private static Sprite[] LoadAnimationFrames(
            string resourcePath,
            string animationName,
            int columns,
            int rows,
            ref Sprite[] cachedFrames,
            ref Vector2 cachedContentSize,
            out Vector2 contentSize)
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

            for (int frameIndex = 0; frameIndex < frameCount; frameIndex++)
            {
                int column = frameIndex % columns;
                int rowFromTop = frameIndex / columns;
                int rowFromBottom = rows - 1 - rowFromTop;
                int cellX = column * cellWidth;
                int cellY = rowFromBottom * cellHeight;

                FindFrameContentSize(
                    pixels,
                    source.width,
                    cellX,
                    cellY,
                    cellWidth,
                    cellHeight,
                    out int contentWidth,
                    out int contentHeight);
                maximumContentWidth = Mathf.Max(maximumContentWidth, contentWidth);
                maximumContentHeight = Mathf.Max(maximumContentHeight, contentHeight);

                Sprite frame = Sprite.Create(
                    transparentTexture,
                    new Rect(cellX, cellY, cellWidth, cellHeight),
                    new Vector2(0.5f, 0.5f),
                    SpritePixelsPerUnit,
                    0,
                    SpriteMeshType.FullRect);
                frame.name = $"{source.name} {animationName} {frameIndex + 1}";
                frame.hideFlags = HideFlags.HideAndDontSave;
                frames[frameIndex] = frame;
            }

            if (maximumContentWidth <= 0 || maximumContentHeight <= 0)
            {
                Debug.LogWarning(
                    $"{animationName} animation texture became empty: {resourcePath}");
                return null;
            }

            cachedFrames = frames;
            cachedContentSize = new Vector2(
                maximumContentWidth / SpritePixelsPerUnit,
                maximumContentHeight / SpritePixelsPerUnit);
            contentSize = cachedContentSize;
            return cachedFrames;
        }

        private static void FindFrameContentSize(
            Color32[] pixels,
            int textureWidth,
            int cellX,
            int cellY,
            int cellWidth,
            int cellHeight,
            out int contentWidth,
            out int contentHeight)
        {
            int minX = cellWidth;
            int minY = cellHeight;
            int maxX = -1;
            int maxY = -1;

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

            contentWidth = maxX >= minX ? maxX - minX + 1 : 0;
            contentHeight = maxY >= minY ? maxY - minY + 1 : 0;
        }

        private void UpdateRemainingLengthScale()
        {
            if (ropeResource == null)
            {
                return;
            }

            float remainingRatio = Mathf.Clamp01(ropeResource.NormalizedLength);
            currentBaseScale = Mathf.Lerp(minimumVisualScale, 1f, remainingRatio);
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
                    landingStrength = Mathf.Lerp(
                        0.55f,
                        1f,
                        Mathf.InverseLerp(
                            minimumLandingSpeed,
                            Mathf.Max(minimumLandingSpeed + 0.01f, fullLandingSpeed),
                            fastestFallSpeed));
                    landingElapsed = 0f;
                }

                fastestFallSpeed = 0f;
            }

            wasGrounded = isGrounded;
        }

        private void ApplyVisualScale()
        {
            if (visualTransform == null)
            {
                return;
            }

            Vector2 landingScale = CalculateLandingScale();
            visualTransform.localScale = new Vector3(
                visualBaseScale.x * currentBaseScale * landingScale.x,
                visualBaseScale.y * currentBaseScale * landingScale.y,
                1f);
        }

        private Vector2 CalculateLandingScale()
        {
            if (landingElapsed >= landingDuration)
            {
                return Vector2.one;
            }

            float normalizedTime = Mathf.Clamp01(landingElapsed / landingDuration);
            landingElapsed += Time.deltaTime;

            float squashAmount;
            if (normalizedTime < 0.42f)
            {
                squashAmount = Mathf.SmoothStep(0f, 1f, normalizedTime / 0.42f);
            }
            else
            {
                squashAmount = 1f - Mathf.SmoothStep(
                    0f,
                    1f,
                    (normalizedTime - 0.42f) / 0.58f);
            }

            return new Vector2(
                1f + maximumHorizontalSquash * landingStrength * squashAmount,
                1f - maximumVerticalSquash * landingStrength * squashAmount);
        }
    }
}
