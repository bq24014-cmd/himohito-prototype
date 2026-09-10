using UnityEngine;

namespace HimoHito
{
    /// <summary>Layered menu miniatures. No GameObjects, gameplay events, sounds or camera motion.</summary>
    internal static class StageSelectionDioramaArt
    {
        private const string ResourcePath = "Art/HimoHitoStageDiorama-v1";
        private static Sprite island;
        private static readonly Sprite[] Chests = new Sprite[5];
        private static readonly float[] BodyWidths = new float[5];
        private static Texture2D glow;
        private static bool attemptedLoad;

        public static void DrawPractice(Rect icon, Sprite art, StageSelectionDioramaState.Motion motion)
        {
            if (art == null) return;
            Rect source = art.rect;
            float scale = Mathf.Min(icon.width / source.width, icon.height / source.height);
            Rect fitted = new Rect(icon.center.x - source.width * scale * .5f,
                icon.yMax - source.height * scale, source.width * scale, source.height * scale);
            // Feet follow the upper surface of the pictured sagging bridge, not a straight line.
            Vector2 a = new Vector2(.29f, .53f), b = new Vector2(.49f, .63f), c = new Vector2(.685f, .407f);
            float t = motion.WalkPosition;
            Vector2 point = (1-t)*(1-t)*a + 2*(1-t)*t*b + t*t*c;
            Vector2 feet = new Vector2(fitted.x + fitted.width * point.x, fitted.y + fitted.height * point.y);
            StageDioramaPlayerArt.Draw(feet, 29f, (float)(motion.WalkTime % 1000),
                motion.FacingRight, motion.IsWalking ? motion.Activity : 0f);
        }

        public static bool DrawToybox(Rect icon, StageSelectionDioramaState.Motion motion)
        {
            EnsureResources();
            if (island == null) return false;
            for (int i = 0; i < Chests.Length; i++)
                if (Chests[i] == null || BodyWidths[i] <= 0f) return false;
            Color tint = GUI.color;
            try
            {
                // The base has its own fixed dimensions; opening a lid never rescales the island.
                float islandWidth = 156f;
                float pixelsToGui = islandWidth / island.rect.width;
                Vector2 baseFeet = new Vector2(icon.center.x, icon.yMax);
                DrawAtFeet(island, baseFeet, pixelsToGui);

                // Authored top-step landing, measured in the atlas's top-left pixel coordinates.
                // Keep the chest planted on the same top face in every pose.
                Vector2 chestFeet = new Vector2(icon.center.x + 27f, icon.yMax - 121f);
                const float chestBodyWidth = 66f;
                float pose = Mathf.Clamp01(motion.Openness) * (Chests.Length - 1);
                int first = Mathf.Min(Mathf.FloorToInt(pose), Chests.Length - 1);
                int next = Mathf.Min(first + 1, Chests.Length - 1);
                float blend = pose - first;
                DrawChest(first, chestFeet, chestBodyWidth, tint, 1f - blend);
                if (blend > 0f) DrawChest(next, chestFeet, chestBodyWidth, tint, blend);

                // Broad low-opacity warmth, steady while selected: no flash or pulsing spotlight.
                if (glow != null && motion.Openness > 0f)
                {
                    GUI.color = new Color(1f, .73f, .32f, tint.a * .20f * motion.Openness);
                    GUI.DrawTexture(new Rect(chestFeet.x - 27, chestFeet.y - 49, 54, 39), glow);
                }
                return true;
            }
            finally { GUI.color = tint; }
        }

        private static void DrawChest(int index, Vector2 feet, float width, Color tint, float opacity)
        {
            if (opacity <= 0f || Chests[index] == null || BodyWidths[index] <= 0f) return;
            GUI.color = new Color(tint.r, tint.g, tint.b, tint.a * opacity);
            DrawAtFeet(Chests[index], feet, width / BodyWidths[index]);
        }

        private static void DrawAtFeet(Sprite sprite, Vector2 feet, float pixelScale)
        {
            Rect source = sprite.rect;
            Rect target = new Rect(feet.x - sprite.pivot.x * pixelScale,
                feet.y - (source.height - sprite.pivot.y) * pixelScale,
                source.width * pixelScale, source.height * pixelScale);
            GUI.DrawTextureWithTexCoords(target, sprite.texture, new Rect(source.x / sprite.texture.width,
                source.y / sprite.texture.height, source.width / sprite.texture.width, source.height / sprite.texture.height));
        }

        private static void EnsureResources()
        {
            if (attemptedLoad) return;
            attemptedLoad = true;
            Texture2D source = Resources.Load<Texture2D>(ResourcePath);
            if (source == null || !source.isReadable) return;
            Color32[] pixels = source.GetPixels32();
            for (int i = 0; i < pixels.Length; i++)
            {
                Color32 p = pixels[i];
                int low = Mathf.Min(p.r, Mathf.Min(p.g, p.b)), high = Mathf.Max(p.r, Mathf.Max(p.g, p.b));
                if (low >= 205 && high - low <= 28) { p.a = 0; pixels[i] = p; }
            }
            Texture2D texture = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false) {
                name = source.name + " transparent layers", filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp, hideFlags = HideFlags.HideAndDontSave
            };
            texture.SetPixels32(pixels);
            texture.Apply(false, true);
            // The generated base is slightly wider than one cell; use the actual clear gutters.
            Rect[] regions = {
                new Rect(0, 0, 575, 512), new Rect(575, 0, 449, 512), new Rect(1024, 0, 512, 512),
                new Rect(0, 512, 512, 512), new Rect(512, 512, 512, 512), new Rect(1024, 512, 512, 512)
            };
            for (int i = 0; i < regions.Length; i++)
            {
                Rect region = regions[i];
                float sx = source.width / 1536f, sy = source.height / 1024f;
                int left = Mathf.RoundToInt(region.xMin * sx), right = Mathf.RoundToInt(region.xMax * sx);
                int bottom = source.height - Mathf.RoundToInt(region.yMax * sy);
                int top = source.height - Mathf.RoundToInt(region.yMin * sy);
                int minX = right, maxX = left, minY = top, maxY = bottom;
                for (int y = bottom; y < top; y++)
                    for (int x = left; x < right; x++)
                        if (pixels[y * source.width + x].a > 20)
                        { minX = Mathf.Min(minX, x); maxX = Mathf.Max(maxX, x); minY = Mathf.Min(minY, y); maxY = Mathf.Max(maxY, y); }
                if (minX > maxX || minY > maxY) continue;
                int bodyLeft = minX, bodyRight = maxX;
                if (i > 0)
                {
                    // Ignore the moving lid when measuring width/centre. Register the lower body.
                    bodyLeft = right; bodyRight = left;
                    int bodyTop = Mathf.Min(maxY, minY + Mathf.RoundToInt(145 * sy));
                    for (int y = minY; y <= bodyTop; y++)
                        for (int x = minX; x <= maxX; x++)
                            if (pixels[y * source.width + x].a > 128)
                            { bodyLeft = Mathf.Min(bodyLeft, x); bodyRight = Mathf.Max(bodyRight, x); }
                }
                float width = maxX - minX + 1, height = maxY - minY + 1;
                float pivotX = ((bodyLeft + bodyRight + 1) * .5f - minX) / width;
                Sprite part = Sprite.Create(texture, new Rect(minX, minY, width, height),
                    new Vector2(pivotX, 0), 100, 0, SpriteMeshType.FullRect);
                part.name = source.name + " part " + i;
                part.hideFlags = HideFlags.HideAndDontSave;
                if (i == 0) island = part;
                else { Chests[i - 1] = part; BodyWidths[i - 1] = bodyRight - bodyLeft + 1; }
            }
            glow = new Texture2D(64, 64, TextureFormat.RGBA32, false) {
                name = "Diorama quiet warmth", wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear, hideFlags = HideFlags.HideAndDontSave
            };
            var light = new Color[64 * 64];
            for (int y = 0; y < 64; y++)
                for (int x = 0; x < 64; x++)
                {
                    float a = Mathf.Clamp01(1f - Vector2.Distance(new Vector2(x, y), new Vector2(31.5f, 31.5f)) / 31.5f);
                    light[y * 64 + x] = new Color(1, 1, 1, a * a);
                }
            glow.SetPixels(light); glow.Apply(false, true);
        }
    }
}
