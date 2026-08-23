using System.Collections.Generic;
using UnityEngine;

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
        private const string BlockResourcePath =
            "Art/TutorialBlockPlatform-v1";
        private const string RailResourcePath =
            "Art/TutorialRailPlatform-v1";
        private const string HookResourcePath =
            "Art/TutorialHookConnector-v1";
        private const string ToyBoxResourcePath =
            "Art/TutorialToyBoxGoal-v2";

        private static readonly Dictionary<string, Sprite> ProcessedSprites =
            new Dictionary<string, Sprite>();

        private static readonly Color PlayerColor =
            new Color(1f, 0.365f, 0.561f);
        private static readonly Color PlatformColor =
            new Color(1f, 0.706f, 0.235f);
        private static readonly Color HookColor =
            new Color(0.298f, 0.765f, 1f);
        private static readonly Color ToyBlockYellow =
            new Color(1f, 0.824f, 0.2f);

        public static bool Apply(GameObject player)
        {
            bool changed = false;
            changed |= ApplyColor(player, PlayerColor);
            changed |= ApplyColor(FindSceneObject("Start Ground"), PlatformColor);
            changed |= ApplyColor(FindSceneObject("Tutorial Landing"), PlatformColor);
            changed |= ApplyColor(FindSceneObject("Tutorial Hook"), HookColor);
            changed |= ApplyColor(FindSceneObject("Landing 1"), PlatformColor);
            changed |= ApplyColor(FindSceneObject("Hook 1"), HookColor);
            changed |= ApplyColor(FindSceneObject("Hook 2"), HookColor);
            changed |= ApplyColor(FindSceneObject("Hook 3"), HookColor);
            changed |= ApplyColor(FindSceneObject("Goal / Landing 3"), PlatformColor);
            changed |= EnsureBackground();
            changed |= EnsureToyVisual(
                "Start Ground",
                "Orange Block Platform Visual",
                BlockResourcePath,
                1);
            changed |= EnsureToyVisual(
                "Tutorial Landing",
                "Blue Railway Platform Visual",
                RailResourcePath,
                1);
            changed |= EnsureToyVisual(
                "Tutorial Hook",
                "Blue Toy Hook Visual",
                HookResourcePath,
                6);
            changed |= EnsureToyVisual(
                "Landing 1",
                "Blue Railway Platform Visual",
                RailResourcePath,
                1);
            changed |= EnsureToyVisual(
                "Hook 1",
                "Blue Toy Hook Visual",
                HookResourcePath,
                6);
            changed |= EnsureToyVisual(
                "Hook 2",
                "Blue Toy Hook Visual",
                HookResourcePath,
                6);
            changed |= EnsureToyVisual(
                "Hook 3",
                "Blue Toy Hook Visual",
                HookResourcePath,
                6);
            changed |= EnsureToyVisual(
                "Goal / Landing 3",
                "Open Toy Box Goal Visual",
                ToyBoxResourcePath,
                1,
                true,
                true,
                new Vector2(3.6f, 1.9f));
            changed |= EnsureGoalToyBlockTower();
            changed |= EnsureFixedHookAttachmentPoint("Tutorial Hook");
            changed |= EnsureFixedHookAttachmentPoint("Hook 1");
            changed |= EnsureFixedHookAttachmentPoint("Hook 2");
            changed |= EnsureFixedHookAttachmentPoint("Hook 3");
            return changed;
        }

        private static bool EnsureToyVisual(
            string targetName,
            string visualName,
            string resourcePath,
            int sortingOrderOffset,
            bool preserveWorldAspect = false,
            bool alignBottom = false,
            Vector2? targetWorldSize = null)
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
            if (targetWorldSize.HasValue)
            {
                Vector3 parentScale = target.transform.lossyScale;
                Vector2 worldSize = targetWorldSize.Value;
                targetScale.x = worldSize.x /
                    Mathf.Max(0.01f, spriteSize.x * Mathf.Abs(parentScale.x));
                targetScale.y = worldSize.y /
                    Mathf.Max(0.01f, spriteSize.y * Mathf.Abs(parentScale.y));
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

        private static bool EnsureGoalToyBlockTower()
        {
            GameObject goal = FindSceneObject("Goal / Landing 3");
            if (goal == null ||
                !goal.TryGetComponent(out SpriteRenderer sourceRenderer))
            {
                return false;
            }

            // The room floor in the adopted background sits at Y=-4.85.
            // These blocks only explain why the elevated toy box is there;
            // they deliberately have no colliders and cannot create a shortcut.
            const float floorTop = -4.85f;
            const float goalBottom = -0.9f;
            const float blockWidth = 0.88f;
            const float gap = 0.08f;
            const int blockCount = 4;
            float blockHeight =
                (goalBottom - floorTop - gap * (blockCount - 1)) /
                blockCount;

            Color[] leftColors =
            {
                PlatformColor,
                HookColor,
                ToyBlockYellow,
                PlatformColor
            };
            Color[] rightColors =
            {
                HookColor,
                ToyBlockYellow,
                PlatformColor,
                HookColor
            };

            bool changed = false;
            for (int index = 0; index < blockCount; index++)
            {
                float centerY = floorTop + blockHeight * 0.5f +
                    index * (blockHeight + gap);
                changed |= EnsureDecorativeBlock(
                    goal,
                    sourceRenderer,
                    $"Toy Block Support Left {index + 1}",
                    new Vector2(goal.transform.position.x - 1.15f, centerY),
                    new Vector2(blockWidth, blockHeight),
                    leftColors[index]);
                changed |= EnsureDecorativeBlock(
                    goal,
                    sourceRenderer,
                    $"Toy Block Support Right {index + 1}",
                    new Vector2(goal.transform.position.x + 1.15f, centerY),
                    new Vector2(blockWidth, blockHeight),
                    rightColors[index]);
            }

            return changed;
        }

        private static bool EnsureDecorativeBlock(
            GameObject goal,
            SpriteRenderer sourceRenderer,
            string blockName,
            Vector2 worldPosition,
            Vector2 worldSize,
            Color color)
        {
            bool changed = false;
            Transform blockTransform = goal.transform.Find(blockName);
            if (blockTransform == null)
            {
                GameObject blockObject = new GameObject(blockName);
                blockTransform = blockObject.transform;
                blockTransform.SetParent(goal.transform, false);
                changed = true;
            }

            Vector3 targetPosition =
                new Vector3(worldPosition.x, worldPosition.y, goal.transform.position.z);
            if (blockTransform.position != targetPosition)
            {
                blockTransform.position = targetPosition;
                changed = true;
            }

            if (blockTransform.rotation != Quaternion.identity)
            {
                blockTransform.rotation = Quaternion.identity;
                changed = true;
            }

            Vector3 parentScale = goal.transform.lossyScale;
            Vector3 targetScale = new Vector3(
                worldSize.x / Mathf.Max(0.01f, Mathf.Abs(parentScale.x)),
                worldSize.y / Mathf.Max(0.01f, Mathf.Abs(parentScale.y)),
                1f);
            if (blockTransform.localScale != targetScale)
            {
                blockTransform.localScale = targetScale;
                changed = true;
            }

            if (!blockTransform.TryGetComponent(out SolidSprite solidSprite))
            {
                solidSprite = blockTransform.gameObject.AddComponent<SolidSprite>();
                changed = true;
            }
            if (solidSprite.Color != color)
            {
                solidSprite.Color = color;
                changed = true;
            }

            SpriteRenderer renderer = blockTransform.GetComponent<SpriteRenderer>();
            if (renderer.sortingLayerID != sourceRenderer.sortingLayerID)
            {
                renderer.sortingLayerID = sourceRenderer.sortingLayerID;
                changed = true;
            }
            if (renderer.sortingOrder != sourceRenderer.sortingOrder)
            {
                renderer.sortingOrder = sourceRenderer.sortingOrder;
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

            if (renderer.color != Color.white)
            {
                renderer.color = Color.white;
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
            foreach (GameObject candidate in
                     Resources.FindObjectsOfTypeAll<GameObject>())
            {
                if (candidate.scene.IsValid() && candidate.name == objectName)
                {
                    return candidate;
                }
            }

            return null;
        }
    }
}
