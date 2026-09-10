using UnityEngine;

namespace HimoHito
{
    /// <summary>
    /// Draws the Part D UI sprite sheet. The generated source uses a white
    /// studio background, so it is keyed to transparency once at runtime.
    /// </summary>
    public static class HimoHitoUiParts
    {
        // Four text rows plus the 30px gauge, with room inside the felt stitching.
        public const float CompactHudHeight = 206f;

        private const string ResourcePath = "Art/HimoHitoUiParts-v1";
        private const float AtlasSourceWidth = 1881f;
        private const float AtlasSourceHeight = 836f;

        private static readonly RectInt GaugeRect =
            new RectInt(35, 319, 665, 179);
        private static readonly RectInt KnotRect =
            new RectInt(746, 307, 211, 205);
        private static readonly RectInt ConnectorRect =
            new RectInt(1011, 294, 231, 231);
        private static readonly RectInt ButtonRect =
            new RectInt(1295, 305, 373, 208);
        private static readonly RectInt FrayedEndRect =
            new RectInt(1724, 309, 102, 196);

        private static Texture2D processedAtlas;
        private static Sprite connectorSprite;
        private static Sprite woodMountSprite, mountingKnotSprite;
        private static Sprite craftWood, craftHud;
        private static GUIStyle hudAreaStyle;

        private static Sprite CraftSprite(ref Sprite cached, string path)
        {
            if (cached != null && cached.texture != null) return cached;
            if (Resources.Load<Texture2D>(path) == null) return null;
            return cached = TutorialFirstSectionVisuals.LoadProcessedToySprite(path, true);
        }

        // Only UI drawing uses these; the old atlas sprites remain unchanged for world props.
        private static Sprite CraftWood => CraftSprite(ref craftWood, "Art/HimoHitoCraftHookMount-v1");
        private static Sprite CraftHud => CraftSprite(ref craftHud, "Art/HimoHitoCraftHudPanel-v1");

        public static void BeginHudArea(Rect rect)
        {
            if (CraftHud == null) { GUILayout.BeginArea(rect, GUI.skin.box); return; }
            DrawSlicedSprite(rect, CraftHud, .10f, 12f);
            if (hudAreaStyle == null)
            {
                hudAreaStyle = new GUIStyle(GUI.skin.box);
                hudAreaStyle.normal.background = null;
                hudAreaStyle.padding = new RectOffset(14, 14, 12, 14);
            }
            GUILayout.BeginArea(rect, hudAreaStyle);
        }

        public static Sprite WoodMountSprite => AtlasSprite(ref woodMountSprite, ButtonRect, "Hook Wooden Mount");
        public static Sprite MountingKnotSprite => AtlasSprite(ref mountingKnotSprite, KnotRect, "Hook Mounting Knot");

        private static Sprite AtlasSprite(ref Sprite cached, RectInt region, string label)
        {
            EnsureAtlas();
            if (processedAtlas == null) return null;
            if (cached != null && cached.texture != null) return cached;
            float sx = processedAtlas.width / AtlasSourceWidth;
            float sy = processedAtlas.height / AtlasSourceHeight;
            var rect = new Rect(region.x * sx, (AtlasSourceHeight - region.yMax) * sy,
                region.width * sx, region.height * sy);
            cached = Sprite.Create(processedAtlas, rect, new Vector2(.5f, .5f), rect.height,
                0, SpriteMeshType.FullRect);
            cached.name = label;
            cached.hideFlags = HideFlags.HideAndDontSave;
            return cached;
        }

        public static bool IsAvailable
        {
            get
            {
                EnsureAtlas();
                return processedAtlas != null;
            }
        }

        public static Sprite ConnectorSprite
        {
            get
            {
                EnsureAtlas();
                if (processedAtlas == null)
                {
                    return null;
                }

                if (connectorSprite != null && connectorSprite.texture != null)
                {
                    return connectorSprite;
                }

                float scaleX = processedAtlas.width / AtlasSourceWidth;
                float scaleY = processedAtlas.height / AtlasSourceHeight;
                Rect spriteRect = new Rect(
                    ConnectorRect.x * scaleX,
                    (AtlasSourceHeight - ConnectorRect.yMax) * scaleY,
                    ConnectorRect.width * scaleX,
                    ConnectorRect.height * scaleY);
                connectorSprite = Sprite.Create(
                    processedAtlas,
                    spriteRect,
                    new Vector2(0.5f, 0.5f),
                    spriteRect.height,
                    0,
                    SpriteMeshType.FullRect);
                connectorSprite.name = "HimoHito Part D Connector Ring";
                connectorSprite.hideFlags = HideFlags.HideAndDontSave;
                return connectorSprite;
            }
        }

        public static void DrawGaugeFrame(Rect rect)
        {
            if (CraftWood != null && rect.width >= 40f && rect.height > 0f)
            {
                // Keep the existing x+20 / y+9 / height 12 track fully exposed.
                DrawSlicedSprite(new Rect(rect.x + 10f, rect.y + 1f, rect.width - 20f, 8f), CraftWood, .16f, 3f);
                DrawSlicedSprite(new Rect(rect.x + 10f, rect.y + 21f, rect.width - 20f, 8f), CraftWood, .16f, 3f);
                DrawSlicedSprite(new Rect(rect.x, rect.y + 1f, 20f, 28f), CraftWood, .16f, 5f);
                DrawSlicedSprite(new Rect(rect.xMax - 20f, rect.y + 1f, 20f, 28f), CraftWood, .16f, 5f);
                DrawKnot(new Rect(rect.x + 2f, rect.y + 6f, 12f, 18f));
                DrawKnot(new Rect(rect.xMax - 14f, rect.y + 6f, 12f, 18f));
                return;
            }
            if (!IsAvailable)
            {
                return;
            }

            float capWidth = Mathf.Min(rect.width * 0.24f, rect.height * 0.7f);
            Rect leftTarget = new Rect(rect.x, rect.y, capWidth, rect.height);
            Rect middleTarget = new Rect(
                rect.x + capWidth,
                rect.y,
                Mathf.Max(0f, rect.width - capWidth * 2f),
                rect.height);
            Rect rightTarget = new Rect(
                rect.xMax - capWidth,
                rect.y,
                capWidth,
                rect.height);

            const int leftSourceWidth = 115;
            const int rightSourceWidth = 104;
            DrawAtlasRegion(
                leftTarget,
                new RectInt(
                    GaugeRect.x,
                    GaugeRect.y,
                    leftSourceWidth,
                    GaugeRect.height));
            DrawAtlasRegion(
                middleTarget,
                new RectInt(
                    GaugeRect.x + leftSourceWidth,
                    GaugeRect.y,
                    GaugeRect.width - leftSourceWidth - rightSourceWidth,
                    GaugeRect.height));
            DrawAtlasRegion(
                rightTarget,
                new RectInt(
                    GaugeRect.xMax - rightSourceWidth,
                    GaugeRect.y,
                    rightSourceWidth,
                    GaugeRect.height));
        }

        public static void DrawWoodButton(Rect rect)
        {
            if (CraftWood != null)
            {
                DrawSlicedSprite(rect, CraftWood, .16f, Mathf.Min(12f, rect.height * .22f));
                return;
            }
            if (!IsAvailable)
            {
                return;
            }

            float capWidth = Mathf.Min(rect.width * 0.28f, rect.height * 0.46f);
            const int sourceCapWidth = 88;
            DrawAtlasRegion(
                new Rect(rect.x, rect.y, capWidth, rect.height),
                new RectInt(
                    ButtonRect.x,
                    ButtonRect.y,
                    sourceCapWidth,
                    ButtonRect.height));
            DrawAtlasRegion(
                new Rect(
                    rect.x + capWidth,
                    rect.y,
                    Mathf.Max(0f, rect.width - capWidth * 2f),
                    rect.height),
                new RectInt(
                    ButtonRect.x + sourceCapWidth,
                    ButtonRect.y,
                    ButtonRect.width - sourceCapWidth * 2,
                    ButtonRect.height));
            DrawAtlasRegion(
                new Rect(rect.xMax - capWidth, rect.y, capWidth, rect.height),
                new RectInt(
                    ButtonRect.xMax - sourceCapWidth,
                    ButtonRect.y,
                    sourceCapWidth,
                    ButtonRect.height));
        }

        public static void DrawWoodButtonLabel(
            Rect rect,
            string text,
            GUIStyle style)
        {
            DrawWoodButton(rect);

            GUIStyle buttonLabelStyle = new GUIStyle(style)
            {
                alignment = TextAnchor.MiddleCenter
            };
            buttonLabelStyle.normal.textColor = new Color(0.11f, 0.08f, 0.20f);
            GUI.Label(rect, text, buttonLabelStyle);
        }

        public static bool WoodButton(Rect rect, string text, GUIStyle style)
        {
            bool clicked = GUI.Button(rect, GUIContent.none, GUIStyle.none);
            Color previous = GUI.color;
            if (GUI.enabled && rect.Contains(Event.current.mousePosition))
                GUI.color = previous * new Color(1f, .9f, .8f, 1f);
            DrawWoodButtonLabel(rect, text, style);
            GUI.color = previous;
            return clicked;
        }

        public static void DrawKnot(Rect rect)
        {
            DrawAtlasRegion(rect, KnotRect);
        }

        public static void DrawFrayedEnd(Rect rect)
        {
            DrawAtlasRegion(rect, FrayedEndRect);
        }

        private static void DrawAtlasRegion(Rect target, RectInt sourceTopLeft)
        {
            EnsureAtlas();
            if (processedAtlas == null || target.width <= 0f || target.height <= 0f)
            {
                return;
            }

            Rect uv = new Rect(
                sourceTopLeft.x / AtlasSourceWidth,
                (AtlasSourceHeight - sourceTopLeft.yMax) / AtlasSourceHeight,
                sourceTopLeft.width / AtlasSourceWidth,
                sourceTopLeft.height / AtlasSourceHeight);
            GUI.DrawTextureWithTexCoords(target, processedAtlas, uv, true);
        }

        // Nine-slicing keeps the rounded corners intact without stretching them with long labels.
        private static void DrawSlicedSprite(Rect target, Sprite sprite, float borderRatio, float borderPixels)
        {
            if (target.width <= 0f || target.height <= 0f || sprite == null) return;
            Rect source = sprite.textureRect;
            float cut = Mathf.Min(source.width, source.height) * borderRatio;
            float border = Mathf.Min(borderPixels, Mathf.Min(target.width, target.height) * .5f);
            for (int row = 0; row < 3; row++)
            for (int column = 0; column < 3; column++)
            {
                float sx = column == 0 ? 0f : column == 1 ? cut : source.width - cut;
                float sy = row == 0 ? 0f : row == 1 ? cut : source.height - cut;
                float sw = column == 1 ? source.width - cut * 2f : cut;
                float sh = row == 1 ? source.height - cut * 2f : cut;
                float dx = column == 0 ? 0f : column == 1 ? border : target.width - border;
                float dy = row == 0 ? 0f : row == 1 ? border : target.height - border;
                float dw = column == 1 ? target.width - border * 2f : border;
                float dh = row == 1 ? target.height - border * 2f : border;
                if (dw <= 0f || dh <= 0f) continue;
                Rect uv = new Rect((source.x + sx) / sprite.texture.width,
                    (source.yMax - sy - sh) / sprite.texture.height,
                    sw / sprite.texture.width, sh / sprite.texture.height);
                GUI.DrawTextureWithTexCoords(new Rect(target.x + dx, target.y + dy, dw, dh),
                    sprite.texture, uv, true);
            }
        }

        private static void EnsureAtlas()
        {
            if (processedAtlas != null)
            {
                return;
            }

            Texture2D source = Resources.Load<Texture2D>(ResourcePath);
            if (source == null)
            {
                return;
            }

            RenderTexture previous = RenderTexture.active;
            RenderTexture temporary = RenderTexture.GetTemporary(
                source.width,
                source.height,
                0,
                RenderTextureFormat.ARGB32,
                RenderTextureReadWrite.sRGB);
            try
            {
                Graphics.Blit(source, temporary);
                RenderTexture.active = temporary;
                processedAtlas = new Texture2D(
                    source.width,
                    source.height,
                    TextureFormat.RGBA32,
                    false);
                processedAtlas.name = "HimoHito UI Parts Transparent";
                processedAtlas.hideFlags = HideFlags.HideAndDontSave;
                processedAtlas.ReadPixels(
                    new Rect(0f, 0f, source.width, source.height),
                    0,
                    0);

                Color32[] pixels = processedAtlas.GetPixels32();
                for (int index = 0; index < pixels.Length; index++)
                {
                    Color32 pixel = pixels[index];
                    int maximum = Mathf.Max(pixel.r, Mathf.Max(pixel.g, pixel.b));
                    int minimum = Mathf.Min(pixel.r, Mathf.Min(pixel.g, pixel.b));
                    bool isNeutralBackground =
                        maximum - minimum <= 22 && minimum >= 222;
                    if (isNeutralBackground)
                    {
                        pixel.a = 0;
                        pixels[index] = pixel;
                    }
                }

                processedAtlas.SetPixels32(pixels);
                processedAtlas.wrapMode = TextureWrapMode.Clamp;
                processedAtlas.filterMode = FilterMode.Bilinear;
                processedAtlas.Apply(false, true);
            }
            finally
            {
                RenderTexture.active = previous;
                RenderTexture.ReleaseTemporary(temporary);
            }
        }
    }
}
