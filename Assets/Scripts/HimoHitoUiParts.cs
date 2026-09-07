using UnityEngine;

namespace HimoHito
{
    /// <summary>
    /// Draws the Part D UI sprite sheet. The generated source uses a white
    /// studio background, so it is keyed to transparency once at runtime.
    /// </summary>
    public static class HimoHitoUiParts
    {
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
