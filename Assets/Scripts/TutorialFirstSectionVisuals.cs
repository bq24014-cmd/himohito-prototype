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

        private static readonly Dictionary<(string, bool), Sprite> ProcessedSprites =
            new Dictionary<(string, bool), Sprite>();

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
            changed |= HimoHitoCraftRoomBackground.Ensure(
                BackgroundName, FarBackgroundName);
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
            changed |= GoalChestPresentation.Ensure(
                FindSceneObject(TutorialSectionFourSetup.GoalMarkerName),
                FindSceneObject(TutorialSectionFourSetup.GoalFloorName));
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
            changed |= EnsureSpikeVisual(TutorialSectionTwoSetup.LeftSpikeName);
            changed |= EnsureSpikeVisual(TutorialSectionTwoSetup.CenterSpikeName);
            changed |= EnsureSpikeVisual(TutorialSectionTwoSetup.RightSpikeName);
            return changed;
        }

        private static bool EnsureSpikeVisual(string objectName)
        {
            GameObject target = FindSceneObject(objectName);
            if (target == null) return false;
            bool added = !target.TryGetComponent(out ToySpikeVisual visual);
            if (added) visual = target.AddComponent<ToySpikeVisual>();
            return visual.Refresh() || added;
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

        public static Sprite LoadProcessedToySprite(
            string resourcePath, bool useMipMaps = false)
        {
            return GetProcessedSprite(resourcePath, useMipMaps);
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

            if (resourcePath == BlockResourcePath &&
                CraftWoodPlatformVisual.TryEnsure(target, out bool woodChanged)) return woodChanged;

            bool changed = false;
            if (resourcePath == RailResourcePath && targetName == "Landing 1")
            {
                if (!target.activeInHierarchy) return false;
                if (CraftRailPlatformVisual.TryEnsure(target, out changed, allowTutorialLegacy: true)) return changed;
            }

            Sprite processedSprite = GetProcessedSprite(resourcePath);
            if (processedSprite == null)
            {
                return changed;
            }

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
                if (targetName == TutorialSectionFourSetup.GoalMarkerName)
                {
                    // Seat the artwork on the visible wood, not the marker's
                    // bounding box. Keep the goal collider and marker untouched.
                    float floorTop = TutorialSectionFourSetup.GoalFloorPosition.y +
                        TutorialSectionFourSetup.GoalFloorSize.y * 0.5f;
                    const float woodSurfaceInset = 0.16f;
                    targetLocalPosition = target.transform.InverseTransformPoint(
                        new Vector3(target.transform.position.x,
                            floorTop - woodSurfaceInset + visualWorldHeight * 0.5f,
                            target.transform.position.z));
                }
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

            if (resourcePath == RailResourcePath && renderer.drawMode != SpriteDrawMode.Simple)
            {
                renderer.drawMode = SpriteDrawMode.Simple;
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

            if (resourcePath == BlockResourcePath)
                changed |= WoodenPlatformDepthVisual.Ensure(target);
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

        private static Sprite GetProcessedSprite(
            string resourcePath, bool useMipMaps = false)
        {
            var cacheKey = (resourcePath, useMipMaps);
            if (ProcessedSprites.TryGetValue(cacheKey, out Sprite cached) &&
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
                useMipMaps);
            transparentTexture.name = $"{source.name} Transparent";
            transparentTexture.filterMode = useMipMaps
                ? FilterMode.Trilinear : FilterMode.Bilinear;
            transparentTexture.wrapMode = TextureWrapMode.Clamp;
            transparentTexture.hideFlags = HideFlags.HideAndDontSave;
            if (useMipMaps) SetAlphaWeightedMipMaps(transparentTexture, pixels);
            else transparentTexture.SetPixels32(pixels);
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
            if (resourcePath == "Art/HimoHitoPlayer-v1")
                RopeFaceLandmarks.Register(processed, pixels, source.width);
            ProcessedSprites[cacheKey] = processed;
            return processed;
        }

        private static void SetAlphaWeightedMipMaps(Texture2D texture, Color32[] pixels)
        {
            int width = texture.width, height = texture.height;
            for (int level = 0; level < texture.mipmapCount; level++)
            {
                // RGB outside the silhouette must also be safe for bilinear sampling.
                BleedTransparentEdges(pixels, width, height);
                texture.SetPixels32(pixels, level);
                if (level == texture.mipmapCount - 1) break;

                int nextWidth = Mathf.Max(1, width / 2);
                int nextHeight = Mathf.Max(1, height / 2);
                var next = new Color32[nextWidth * nextHeight];
                for (int y = 0; y < nextHeight; y++)
                {
                    float bottom = (float)y * height / nextHeight;
                    float top = (float)(y + 1) * height / nextHeight;
                    for (int x = 0; x < nextWidth; x++)
                    {
                        float left = (float)x * width / nextWidth;
                        float right = (float)(x + 1) * width / nextWidth;
                        float alpha = 0f, red = 0f, green = 0f, blue = 0f;
                        // Area weights include every edge pixel of odd/NPOT dimensions.
                        for (int sy = Mathf.FloorToInt(bottom); sy < Mathf.CeilToInt(top); sy++)
                        for (int sx = Mathf.FloorToInt(left); sx < Mathf.CeilToInt(right); sx++)
                        {
                            float area = (Mathf.Min(right, sx + 1) - Mathf.Max(left, sx)) *
                                (Mathf.Min(top, sy + 1) - Mathf.Max(bottom, sy));
                            Color32 sample = pixels[sy * width + sx];
                            float weight = sample.a * area;
                            alpha += weight;
                            red += sample.r * weight;
                            green += sample.g * weight;
                            blue += sample.b * weight;
                        }
                        if (alpha > 0f)
                            next[y * nextWidth + x] = new Color32(
                                (byte)Mathf.RoundToInt(red / alpha),
                                (byte)Mathf.RoundToInt(green / alpha),
                                (byte)Mathf.RoundToInt(blue / alpha),
                                (byte)Mathf.RoundToInt(alpha / ((right - left) * (top - bottom))));
                    }
                }
                pixels = next;
                width = nextWidth;
                height = nextHeight;
            }
        }

        private static void BleedTransparentEdges(Color32[] pixels, int width, int height)
        {
            for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                int index = y * width + x;
                if (pixels[index].a != 0) continue;
                float alpha = 0f, red = 0f, green = 0f, blue = 0f;
                for (int sy = Mathf.Max(0, y - 1); sy <= Mathf.Min(height - 1, y + 1); sy++)
                for (int sx = Mathf.Max(0, x - 1); sx <= Mathf.Min(width - 1, x + 1); sx++)
                {
                    Color32 sample = pixels[sy * width + sx];
                    alpha += sample.a;
                    red += sample.r * sample.a;
                    green += sample.g * sample.a;
                    blue += sample.b * sample.a;
                }
                pixels[index] = alpha > 0f
                    ? new Color32((byte)Mathf.RoundToInt(red / alpha),
                        (byte)Mathf.RoundToInt(green / alpha), (byte)Mathf.RoundToInt(blue / alpha), 0)
                    : new Color32(0, 0, 0, 0);
            }
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
