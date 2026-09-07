using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HimoHito
{
    /// <summary>
    /// Applies the adopted visuals across the tutorial without changing colliders or physics.
    /// Gameplay objects receive the agreed toy palette and presentation.
    /// </summary>
    public static class TutorialFirstSectionVisuals
    {
        private const string BackgroundResourcePath =
            "Art/TutorialNightChildRoom-v1";
        private const string BackgroundName =
            "Tutorial Night Child Room Background";
        private const string FarBackgroundName =
            "Tutorial Far Child Room Background";
        private const string BlockResourcePath =
            "Art/TutorialBlockPlatform-v1";
        private const string RailResourcePath =
            "Art/TutorialRailPlatform-v1";
        private const string ToyBoxResourcePath =
            "Art/TutorialToyBoxGoal-v2";
        private const string HookVisualName = "Blue Toy Hook Visual";
        private const float HookVisualWorldDiameter = 0.72f;

        private static readonly Dictionary<string, Sprite> ProcessedSprites =
            new Dictionary<string, Sprite>();

        private static readonly Color PlayerColor =
            new Color(1f, 0.365f, 0.561f);
        private static readonly Color PlatformColor =
            new Color(1f, 0.706f, 0.235f);
        private static readonly Color HookColor =
            new Color(0.298f, 0.765f, 1f);
        private static int lastReloadRestoreFrame = -1;
        private static bool isRestoringAfterReload;

        public static bool Apply(GameObject player)
        {
            bool changed = false;
            changed |= ApplyColor(player, PlayerColor);
            changed |= ApplyColor(FindSceneObject("Start Ground"), PlatformColor);
            changed |= ApplyColor(FindSceneObject("Tutorial Landing"), PlatformColor);
            changed |= ApplyColor(FindSceneObject("Tutorial Hook"), HookColor);
            changed |= ApplyColor(
                FindSceneObject(TutorialSectionTwoSetup.LandingFloorName),
                PlatformColor);
            changed |= ApplyColor(
                FindSceneObject(TutorialSectionTwoSetup.HookName),
                HookColor);
            changed |= ApplyColor(
                FindSceneObject(TutorialSectionThreeSetup.LandingFloorName),
                PlatformColor);
            changed |= ApplyColor(
                FindSceneObject(TutorialSectionFourSetup.BeamName),
                PlatformColor);
            changed |= ApplyColor(
                FindSceneObject(TutorialSectionFourSetup.GoalFloorName),
                PlatformColor);
            changed |= ApplyColor(
                FindSceneObject(TutorialSectionFourSetup.CenterHookName),
                HookColor);
            changed |= ApplyColor(FindSceneObject("Landing 1"), PlatformColor);
            changed |= ApplyColor(FindSceneObject("Hook 1"), HookColor);
            changed |= ApplyColor(FindSceneObject("Hook 2"), HookColor);
            changed |= ApplyColor(FindSceneObject("Hook 3"), HookColor);
            changed |= ApplyColor(FindSceneObject("Goal / Landing 3"), PlatformColor);
            changed |= HimoHitoFarBackgroundLayer.Ensure(
                FarBackgroundName,
                40f,
                0.78f);
            changed |= EnsureBackground();
            changed |= EnsureToyVisual(
                "Start Ground",
                "Orange Block Platform Visual",
                BlockResourcePath,
                1);
            changed |= RemoveChild(
                FindSceneObject("Tutorial Landing"),
                "Blue Railway Platform Visual");
            changed |= EnsureToyVisual(
                "Tutorial Landing",
                "Orange Block Platform Visual",
                BlockResourcePath,
                1);
            changed |= EnsurePartDHookRingVisual("Tutorial Hook");
            changed |= EnsureToyVisual(
                TutorialSectionTwoSetup.LandingFloorName,
                "Orange Block Platform Visual",
                BlockResourcePath,
                1);
            changed |= EnsurePartDHookRingVisual(
                TutorialSectionTwoSetup.HookName);
            changed |= EnsureToyVisual(
                TutorialSectionThreeSetup.LandingFloorName,
                "Orange Block Platform Visual",
                BlockResourcePath,
                1);
            changed |= EnsureToyVisual(
                TutorialSectionFourSetup.BeamName,
                "Orange Block Platform Visual",
                BlockResourcePath,
                1);
            changed |= EnsureToyVisual(
                TutorialSectionFourSetup.GoalFloorName,
                "Orange Block Platform Visual",
                BlockResourcePath,
                1);
            changed |= EnsurePartDHookRingVisual(
                TutorialSectionFourSetup.CenterHookName);
            changed |= EnsureToyVisual(
                TutorialSectionFourSetup.GoalMarkerName,
                "Open Toy Box Goal Visual",
                ToyBoxResourcePath,
                1,
                true,
                true);
            changed |= EnsureToyVisual(
                "Landing 1",
                "Blue Railway Platform Visual",
                RailResourcePath,
                1);
            changed |= EnsurePartDHookRingVisual("Hook 1");
            changed |= EnsurePartDHookRingVisual("Hook 2");
            changed |= EnsurePartDHookRingVisual("Hook 3");
            changed |= EnsureToyVisual(
                "Goal / Landing 3",
                "Open Toy Box Goal Visual",
                ToyBoxResourcePath,
                1,
                true,
                true);
            changed |= RemoveLegacyGoalToyBlockSupports();
            changed |= EnsureFixedHookAttachmentPoint("Tutorial Hook");
            changed |= EnsureFixedHookAttachmentPoint(
                TutorialSectionTwoSetup.HookName);
            changed |= EnsureFixedHookAttachmentPoint(
                TutorialSectionFourSetup.CenterHookName);
            changed |= EnsureFixedHookAttachmentPoint("Hook 1");
            changed |= EnsureFixedHookAttachmentPoint("Hook 2");
            changed |= EnsureFixedHookAttachmentPoint("Hook 3");
            return changed;
        }

        public static void RestoreSceneVisualsAfterReload()
        {
            if (!Application.isPlaying ||
                SceneManager.GetActiveScene().name != "Tutorial" ||
                isRestoringAfterReload ||
                lastReloadRestoreFrame == Time.frameCount)
            {
                return;
            }

            lastReloadRestoreFrame = Time.frameCount;
            isRestoringAfterReload = true;
            try
            {
                Apply(FindSceneObject("Player"));
            }
            finally
            {
                isRestoringAfterReload = false;
            }
        }

        private static bool EnsurePartDHookRingVisual(string targetName)
        {
            GameObject target = FindSceneObject(targetName);
            if (target == null ||
                !target.TryGetComponent(out SpriteRenderer sourceRenderer))
            {
                return false;
            }

            bool changed = false;
            Transform visualTransform = target.transform.Find(HookVisualName);
            if (visualTransform == null)
            {
                GameObject visualObject = new GameObject(HookVisualName);
                visualTransform = visualObject.transform;
                visualTransform.SetParent(target.transform, false);
                changed = true;
            }
            if (!visualTransform.gameObject.activeSelf)
            {
                visualTransform.gameObject.SetActive(true);
                changed = true;
            }
            if (!visualTransform.TryGetComponent(out SpriteRenderer _))
            {
                visualTransform.gameObject.AddComponent<SpriteRenderer>();
                changed = true;
            }
            if (!visualTransform.TryGetComponent(
                    out HimoHitoHookRingVisual ringVisual))
            {
                ringVisual = visualTransform.gameObject.AddComponent<
                    HimoHitoHookRingVisual>();
                changed = true;
            }

            changed |= ringVisual.Configure(
                sourceRenderer.sortingLayerID,
                sourceRenderer.sortingOrder + 6,
                HookVisualWorldDiameter);
            if (sourceRenderer.enabled)
            {
                sourceRenderer.enabled = false;
                changed = true;
            }
            return changed;
        }

        private static bool RemoveChild(
            GameObject parent,
            string childName)
        {
            if (parent == null)
            {
                return false;
            }

            Transform child = parent.transform.Find(childName);
            if (child == null)
            {
                return false;
            }

            if (Application.isPlaying)
            {
                Object.Destroy(child.gameObject);
            }
            else
            {
                Object.DestroyImmediate(child.gameObject);
            }
            return true;
        }

        public static Sprite LoadProcessedToySprite(string resourcePath)
        {
            return GetProcessedSprite(resourcePath);
        }

        private static bool RemoveLegacyGoalToyBlockSupports()
        {
            GameObject goal = FindSceneObject("Goal / Landing 3");
            if (goal == null)
            {
                return false;
            }

            bool changed = false;
            for (int index = goal.transform.childCount - 1; index >= 0; index--)
            {
                Transform child = goal.transform.GetChild(index);
                if (!child.name.StartsWith(
                        "Toy Block Support ",
                        System.StringComparison.Ordinal))
                {
                    continue;
                }

                if (Application.isPlaying)
                {
                    Object.Destroy(child.gameObject);
                }
                else
                {
                    Object.DestroyImmediate(child.gameObject);
                }
                changed = true;
            }

            return changed;
        }

        private static bool EnsureToyVisual(
            string targetName,
            string visualName,
            string resourcePath,
            int sortingOrderOffset,
            bool preserveWorldAspect = false,
            bool alignBottom = false)
        {
            GameObject target = FindSceneObject(targetName);
            if (target == null ||
                !target.TryGetComponent(out SpriteRenderer sourceRenderer))
            {
                return false;
            }

            Sprite processedSprite = GetProcessedSprite(resourcePath);
            if (processedSprite == null)
            {
                return false;
            }

            bool changed = false;
            Transform visualTransform = target.transform.Find(visualName);
            if (visualTransform == null)
            {
                GameObject visualObject = new GameObject(visualName);
                visualTransform = visualObject.transform;
                visualTransform.SetParent(target.transform, false);
                changed = true;
            }

            if (visualTransform.localRotation != Quaternion.identity)
            {
                visualTransform.localRotation = Quaternion.identity;
                changed = true;
            }

            Vector2 spriteSize = processedSprite.bounds.size;
            Vector3 targetScale = new Vector3(
                1f / Mathf.Max(0.01f, spriteSize.x),
                1f / Mathf.Max(0.01f, spriteSize.y),
                1f);
            if (preserveWorldAspect)
            {
                Vector3 parentScale = target.transform.lossyScale;
                float parentWidth = Mathf.Max(0.01f, Mathf.Abs(parentScale.x));
                float parentHeight = Mathf.Max(0.01f, Mathf.Abs(parentScale.y));
                float worldScale = parentWidth / Mathf.Max(0.01f, spriteSize.x);
                targetScale.y = worldScale / parentHeight;
            }
            if (visualTransform.localScale != targetScale)
            {
                visualTransform.localScale = targetScale;
                changed = true;
            }

            Vector3 targetLocalPosition = Vector3.zero;
            if (alignBottom)
            {
                float parentHeight = Mathf.Max(
                    0.01f,
                    Mathf.Abs(target.transform.lossyScale.y));
                float visualWorldHeight =
                    spriteSize.y * targetScale.y * parentHeight;
                targetLocalPosition.y =
                    (visualWorldHeight - parentHeight) * 0.5f / parentHeight;
            }

            if (visualTransform.localPosition != targetLocalPosition)
            {
                visualTransform.localPosition = targetLocalPosition;
                changed = true;
            }

            if (!visualTransform.TryGetComponent(out SpriteRenderer renderer))
            {
                renderer = visualTransform.gameObject.AddComponent<SpriteRenderer>();
                changed = true;
            }

            if (renderer.sprite != processedSprite)
            {
                renderer.sprite = processedSprite;
                changed = true;
            }

            if (renderer.color != Color.white)
            {
                renderer.color = Color.white;
                changed = true;
            }

            if (renderer.sortingLayerID != sourceRenderer.sortingLayerID)
            {
                renderer.sortingLayerID = sourceRenderer.sortingLayerID;
                changed = true;
            }

            int targetOrder =
                sourceRenderer.sortingOrder + sortingOrderOffset;
            if (renderer.sortingOrder != targetOrder)
            {
                renderer.sortingOrder = targetOrder;
                changed = true;
            }

            if (sourceRenderer.enabled)
            {
                sourceRenderer.enabled = false;
                changed = true;
            }

            return changed;
        }

        private static bool EnsureFixedHookAttachmentPoint(string hookName)
        {
            GameObject hook = FindSceneObject(hookName);
            if (hook == null ||
                !hook.TryGetComponent(out HookPoint hookPoint))
            {
                return false;
            }

            // The adopted connector art has its circular eye at local center.
            return hookPoint.ConfigureFixedAttachmentPoint(Vector2.zero);
        }

        private static Sprite GetProcessedSprite(string resourcePath)
        {
            if (ProcessedSprites.TryGetValue(resourcePath, out Sprite cached) &&
                cached != null)
            {
                return cached;
            }

            Texture2D source = Resources.Load<Texture2D>(resourcePath);
            if (source == null)
            {
                Debug.LogWarning(
                    $"Tutorial toy texture was not found: {resourcePath}");
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
                    $"Tutorial toy texture is not readable: {resourcePath}\n{exception.Message}");
                return null;
            }

            int minX = source.width;
            int minY = source.height;
            int maxX = -1;
            int maxY = -1;

            for (int y = 0; y < source.height; y++)
            {
                for (int x = 0; x < source.width; x++)
                {
                    int index = y * source.width + x;
                    Color32 pixel = pixels[index];
                    if (pixel.a == 0)
                    {
                        continue;
                    }

                    byte highest = System.Math.Max(
                        pixel.r,
                        System.Math.Max(pixel.g, pixel.b));
                    byte lowest = System.Math.Min(
                        pixel.r,
                        System.Math.Min(pixel.g, pixel.b));

                    bool isNeutralLightBackground =
                        lowest >= 205 && highest - lowest <= 28;
                    if (isNeutralLightBackground)
                    {
                        pixel.a = 0;
                        pixels[index] = pixel;
                        continue;
                    }

                    minX = Mathf.Min(minX, x);
                    minY = Mathf.Min(minY, y);
                    maxX = Mathf.Max(maxX, x);
                    maxY = Mathf.Max(maxY, y);
                }
            }

            if (maxX < minX || maxY < minY)
            {
                Debug.LogWarning(
                    $"Tutorial toy texture became empty: {resourcePath}");
                return null;
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

            const int padding = 2;
            minX = Mathf.Max(0, minX - padding);
            minY = Mathf.Max(0, minY - padding);
            maxX = Mathf.Min(source.width - 1, maxX + padding);
            maxY = Mathf.Min(source.height - 1, maxY + padding);
            Rect spriteRect = new Rect(
                minX,
                minY,
                maxX - minX + 1,
                maxY - minY + 1);

            Sprite processed = Sprite.Create(
                transparentTexture,
                spriteRect,
                new Vector2(0.5f, 0.5f),
                100f,
                0,
                SpriteMeshType.FullRect);
            processed.name = $"{source.name} Processed";
            processed.hideFlags = HideFlags.HideAndDontSave;
            ProcessedSprites[resourcePath] = processed;
            return processed;
        }

        private static bool EnsureBackground()
        {
            Sprite backgroundSprite =
                Resources.Load<Sprite>(BackgroundResourcePath);
            if (backgroundSprite == null)
            {
                Debug.LogWarning(
                    $"Tutorial background was not found: {BackgroundResourcePath}");
                return false;
            }

            bool changed = false;
            GameObject background = FindSceneObject(BackgroundName);
            if (background == null)
            {
                background = new GameObject(BackgroundName);
                changed = true;
            }

            Vector3 targetPosition = new Vector3(-15f, 0f, 1f);
            if (background.transform.position != targetPosition)
            {
                background.transform.position = targetPosition;
                changed = true;
            }

            float width = Mathf.Max(0.01f, backgroundSprite.bounds.size.x);
            float scale = 40f / width;
            Vector3 targetScale = new Vector3(scale, scale, 1f);
            if (background.transform.localScale != targetScale)
            {
                background.transform.localScale = targetScale;
                changed = true;
            }

            if (!background.TryGetComponent(out SpriteRenderer renderer))
            {
                renderer = background.AddComponent<SpriteRenderer>();
                changed = true;
            }

            if (renderer.sprite != backgroundSprite)
            {
                renderer.sprite = backgroundSprite;
                changed = true;
            }

            Color foregroundTint = new Color(1f, 1f, 1f, 0.84f);
            if (renderer.color != foregroundTint)
            {
                renderer.color = foregroundTint;
                changed = true;
            }

            if (renderer.sortingOrder != -100)
            {
                renderer.sortingOrder = -100;
                changed = true;
            }

            if (!background.TryGetComponent(out TutorialBackgroundParallax _))
            {
                background.AddComponent<TutorialBackgroundParallax>();
                changed = true;
            }

            return changed;
        }

        private static bool ApplyColor(GameObject target, Color color)
        {
            if (target == null)
            {
                return false;
            }

            if (!target.TryGetComponent(out SolidSprite visual))
            {
                return false;
            }

            if (visual.Color == color)
            {
                return false;
            }

            visual.Color = color;
            return true;
        }

        private static GameObject FindSceneObject(string objectName)
        {
            return SceneObjectLookup.Find(objectName);
        }
    }
}
