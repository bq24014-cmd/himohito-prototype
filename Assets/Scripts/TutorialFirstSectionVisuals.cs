using System.Collections.Generic;
using UnityEngine;

namespace HimoHito
{
    /// <summary>
    /// Applies the adopted first-section visuals without changing colliders or physics.
    /// Only tutorial section one receives the agreed gameplay palette entries.
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

        private static readonly Dictionary<string, Sprite> ProcessedSprites =
            new Dictionary<string, Sprite>();

        private static readonly Color PlayerColor =
            new Color(1f, 0.365f, 0.561f);
        private static readonly Color PlatformColor =
            new Color(1f, 0.706f, 0.235f);
        private static readonly Color HookColor =
            new Color(0.298f, 0.765f, 1f);

        public static bool Apply(GameObject player)
        {
            bool changed = false;
            changed |= ApplyColor(player, PlayerColor);
            changed |= ApplyColor(FindSceneObject("Start Ground"), PlatformColor);
            changed |= ApplyColor(FindSceneObject("Tutorial Landing"), PlatformColor);
            changed |= ApplyColor(FindSceneObject("Tutorial Hook"), HookColor);
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
            changed |= EnsureTutorialHookAttachmentPoint();
            return changed;
        }

        private static bool EnsureToyVisual(
            string targetName,
            string visualName,
            string resourcePath,
            int sortingOrderOffset)
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

            if (visualTransform.localPosition != Vector3.zero)
            {
                visualTransform.localPosition = Vector3.zero;
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
            if (visualTransform.localScale != targetScale)
            {
                visualTransform.localScale = targetScale;
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

        private static bool EnsureTutorialHookAttachmentPoint()
        {
            GameObject tutorialHook = FindSceneObject("Tutorial Hook");
            if (tutorialHook == null ||
                !tutorialHook.TryGetComponent(out HookPoint hookPoint))
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
