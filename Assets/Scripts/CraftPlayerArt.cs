using System.Collections.Generic;
using UnityEngine;

namespace HimoHito
{
    /// <summary>
    /// Chooses replacement artwork without changing pose timing, sprite registration,
    /// or physics. Existing resources remain available when a replacement is absent.
    /// </summary>
    public static class CraftPlayerArt
    {
        private static readonly Dictionary<Sprite, float> StandingHeadWidths = new();
        private static readonly Dictionary<Sprite, Vector2> StandingCrownPoints = new();
        private static readonly Dictionary<string, string> CraftResources = new()
        {
            { "Art/HimoHitoPlayer-v1", "Art/HimoHitoCraftPlayer-v1" },
            { "Art/HimoHitoWalk-v3", "Art/HimoHitoCraftWalk-v1" },
            { "Art/HimoHitoJump-v2", "Art/HimoHitoCraftJump-v1" },
            { "Art/HimoHitoLanding-v1", "Art/HimoHitoCraftLanding-v1" },
            { "Art/HimoHitoSwing-v2", "Art/HimoHitoCraftSwing-v1" },
            { "Art/HimoHitoWalkTransitions-v1", "Art/HimoHitoCraftWalkTransitions-v1" },
            { "Art/HimoHitoIdle-v1", "Art/HimoHitoCraftIdle-v1" },
            { "Art/HimoHitoAim-v1", "Art/HimoHitoCraftAim-v1" },
            { "Art/HimoHitoEdgeBalance-v1", "Art/HimoHitoCraftEdgeBalance-v1" },
            { "Art/HimoHitoWeave-v1", "Art/HimoHitoCraftWeave-v1" },
            { "Art/HimoHitoGoal-v1", "Art/HimoHitoCraftGoal-v1" },
            { "Art/HimoHitoWallPush-v1", "Art/HimoHitoCraftWallPush-v1" },
            { "Art/HimoHitoSwitchPress-v1", "Art/HimoHitoCraftSwitchPress-v1" }
        };

        public static Sprite LoadStandingSprite(string legacyResourcePath)
        {
            Texture2D craft = LoadCraftTexture(legacyResourcePath, 1, 1);
            if (craft != null)
            {
                Sprite sprite = TutorialFirstSectionVisuals.LoadProcessedToySprite(
                    CraftResources[legacyResourcePath], true);
                if (sprite != null)
                {
                    // The legacy loader registers eyes only for its original player path.
                    // Keep expression overlays registered to the new visible eyes as well.
                    if (!StandingHeadWidths.ContainsKey(sprite))
                    {
                        Color32[] pixels = craft.GetPixels32();
                        for (int i = 0; i < pixels.Length; i++)
                            if (!IsArtworkPixel(pixels[i])) pixels[i].a = 0;
                        RopeFaceLandmarks.Register(sprite, pixels, craft.width);
                        StandingHeadWidths[sprite] = MeasureHeadWidth(sprite, pixels, craft.width);
                        StandingCrownPoints[sprite] = MeasureCrownPoint(sprite, pixels, craft.width);
                    }
                    return sprite;
                }
            }
            return TutorialFirstSectionVisuals.LoadProcessedToySprite(legacyResourcePath);
        }

        public static Texture2D LoadAnimationTexture(string legacyResourcePath, int columns, int rows)
        {
            // RopeBodyVisual still performs its original per-cell head, sole, neck,
            // face and crown measurements on the selected texture.
            Texture2D craft = LoadCraftTexture(legacyResourcePath, columns, rows);
            return craft != null ? craft : Resources.Load<Texture2D>(legacyResourcePath);
        }

        public static bool IsCraftSprite(Sprite sprite) => sprite != null && sprite.texture != null &&
            sprite.texture.name.StartsWith("HimoHitoCraft", System.StringComparison.Ordinal);

        public static bool TryGetStandingHeadWidth(Sprite sprite, out float width)
        {
            width = 0f;
            return sprite != null && StandingHeadWidths.TryGetValue(sprite, out width) && width > 0f;
        }

        /// <summary>The visible yarn tip in sprite-local units, shared by both sign diagrams.</summary>
        public static bool TryGetStandingCrownPoint(Sprite sprite, out Vector2 point)
        {
            point = default;
            return sprite != null && StandingCrownPoints.TryGetValue(sprite, out point);
        }

        private static Vector2 MeasureCrownPoint(Sprite sprite, Color32[] pixels, int stride)
        {
            Rect rect = sprite.rect;
            int left = Mathf.RoundToInt(rect.xMin), right = Mathf.RoundToInt(rect.xMax);
            for (int y = Mathf.RoundToInt(rect.yMax) - 1; y >= Mathf.RoundToInt(rect.yMin); y--)
            {
                float sumX = 0f;
                int count = 0;
                for (int x = left; x < right; x++)
                {
                    if (pixels[y * stride + x].a == 0) continue;
                    sumX += x + .5f;
                    count++;
                }
                if (count > 0)
                    return (new Vector2(sumX / count - rect.x, y + .5f - rect.y) - sprite.pivot) /
                        sprite.pixelsPerUnit;
            }
            return new Vector2(sprite.bounds.center.x, sprite.bounds.max.y);
        }

        private static float MeasureHeadWidth(Sprite sprite, Color32[] pixels, int stride)
        {
            Rect rect = sprite.rect;
            int left = Mathf.RoundToInt(rect.xMin), right = Mathf.RoundToInt(rect.xMax);
            int bottom = Mathf.RoundToInt(rect.yMin), top = Mathf.RoundToInt(rect.yMax);
            int minY = top, maxY = bottom - 1;
            for (int y = bottom; y < top; y++)
            for (int x = left; x < right; x++)
            {
                if (pixels[y * stride + x].a == 0) continue;
                minY = Mathf.Min(minY, y);
                maxY = Mathf.Max(maxY, y);
            }
            if (maxY < minY) return 0f;
            // Match the animation loader: trailing feet/yarn must not widen the head.
            int headBottom = minY + Mathf.FloorToInt((maxY - minY + 1) * .40f);
            int minX = right, maxX = left - 1;
            for (int y = headBottom; y <= maxY; y++)
            for (int x = left; x < right; x++)
            {
                if (pixels[y * stride + x].a == 0) continue;
                minX = Mathf.Min(minX, x);
                maxX = Mathf.Max(maxX, x);
            }
            return maxX >= minX ? (maxX - minX + 1) / sprite.pixelsPerUnit : 0f;
        }

        private static Texture2D LoadCraftTexture(string legacyResourcePath, int columns, int rows)
        {
            if (!CraftResources.TryGetValue(legacyResourcePath, out string path)) return null;
            Texture2D texture = Resources.Load<Texture2D>(path);
            if (texture == null) return null;
            if (!texture.isReadable || columns < 1 || rows < 1 ||
                texture.width % columns != 0 || texture.height % rows != 0 ||
                !EveryCellHasArtwork(texture, columns, rows))
            {
                Debug.LogWarning($"Craft player artwork is unreadable or not a complete {columns}x{rows} sheet: " +
                    $"{path}. Using {legacyResourcePath} instead.");
                return null;
            }
            return texture;
        }

        private static bool EveryCellHasArtwork(Texture2D texture, int columns, int rows)
        {
            Color32[] pixels = texture.GetPixels32();
            int cellWidth = texture.width / columns, cellHeight = texture.height / rows;
            for (int row = 0; row < rows; row++)
            for (int column = 0; column < columns; column++)
            {
                bool found = false;
                for (int y = row * cellHeight; y < (row + 1) * cellHeight && !found; y++)
                for (int x = column * cellWidth; x < (column + 1) * cellWidth; x++)
                {
                    if (!IsArtworkPixel(pixels[y * texture.width + x])) continue;
                    found = true;
                    break;
                }
                if (!found) return false;
            }
            return true;
        }

        private static bool IsArtworkPixel(Color32 pixel)
        {
            int high = System.Math.Max(pixel.r, System.Math.Max(pixel.g, pixel.b));
            int low = System.Math.Min(pixel.r, System.Math.Min(pixel.g, pixel.b));
            // Same white-background key as both existing player loaders.
            return pixel.a > 0 && !(low >= 205 && high - low <= 28);
        }
    }
}
