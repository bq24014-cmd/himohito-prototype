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
        private const string WalkArtResourcePath = "Art/HimoHitoWalk-v3";
        private const string JumpArtResourcePath = "Art/HimoHitoJump-v2";
        private const string LandingArtResourcePath = "Art/HimoHitoLanding-v1";
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
        [SerializeField, Min(0.05f)] private float landingDuration = 0.30f;
        [SerializeField, Min(0f)] private float minimumLandingSpeed = 1.5f;
        [SerializeField, Min(0.01f)] private float fullLandingSpeed = 10f;

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
        private Vector3 standingRopeLocalPoint;
        private Sprite standingSprite;
        private Sprite[] walkingFrames;
        private Sprite[] jumpingFrames;
        private Sprite[] landingFrames;
        private bool usesCharacterArt;
        private bool isWalking;
        private bool isJumpingVisually;
        private float currentBaseScale = 1f;
        private float walkFrameProgress;
        private float airborneElapsed;
        private float fastestFallSpeed;
        private float landingElapsed;
        private float landingStrength;
        private float currentVisualAngle;
        private float visualAngleVelocity;
        private bool wasGrounded;
        private bool wasRopeAttached;
        private bool swingFacingLeft;
        private bool isReturningFromSwing;

        public Vector2 RopeOrigin
        {
            get
            {
                if (usesCharacterArt && visualRenderer != null)
                {
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
            bool isAttached = ropeController != null && ropeController.IsAttached;
            bool isGrounded = playerMover == null || playerMover.IsGrounded;
            UpdateSwingState(isAttached);

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

            if (shouldShowSwing)
            {
                UpdateSwingAnimation();
                return;
            }

            UpdateUprightVisualRotation(isAttached);

            if (shouldShowLanding)
            {
                UpdateLandingAnimation();
                return;
            }

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

            UpdateFacingFromHorizontalVelocity(body.linearVelocity.x);

            visualRenderer.sprite = jumpingFrames[frameIndex];
            visualTransform.localPosition = Vector3.zero;
            visualBaseScale = jumpingVisualBaseScale;
            isWalking = false;
            walkFrameProgress = 0f;
        }

        private void UpdateSwingAnimation()
        {
            if (visualRenderer == null || body == null || ropeController == null)
            {
                return;
            }

            Vector2 bodyToAnchor =
                ropeController.AnchorPoint - (Vector2)transform.position;
            if (bodyToAnchor.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            Vector2 bodyToAnchorDirection = bodyToAnchor.normalized;
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

            visualRenderer.sprite = standingSprite;
            visualTransform.localPosition = Vector3.zero;
            visualBaseScale = standingVisualBaseScale;
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
            int frameIndex = Mathf.Min(
                LandingFrameCount - 1,
                Mathf.FloorToInt(normalizedTime * LandingFrameCount));

            // A lighter landing stops at the bent-knee pose instead of using the
            // deepest compression drawing. Strong falls keep the complete motion.
            if (frameIndex == 2 && landingStrength < 0.8f)
            {
                frameIndex = 1;
            }

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
            if (isAttached && !wasRopeAttached)
            {
                swingFacingLeft = visualRenderer != null && visualRenderer.flipX;
                isReturningFromSwing = false;
                visualAngleVelocity = 0f;
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

                Sprite frame = Sprite.Create(
                    transparentTexture,
                    new Rect(cellX, cellY, cellWidth, cellHeight),
                    framePivot,
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

            visualTransform.localScale = new Vector3(
                visualBaseScale.x * currentBaseScale,
                visualBaseScale.y * currentBaseScale,
                1f);
        }
    }
}
