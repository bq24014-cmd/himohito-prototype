using UnityEngine;

namespace HimoHito
{
    /// <summary>Feet-registered miniature artwork only. The caller owns its path and clock.</summary>
    internal static class StageDioramaPlayerArt
    {
        internal const float MaximumVisibleHeight = 31f;
        private const int FrameCount = 8;
        private const int RestFrame = 1; // Feet-together pose on the same registered walk sheet.
        private const float FramesPerSecond = 6f;
        private static readonly Sprite[] WalkFrames = new Sprite[FrameCount];
        private static Sprite fallbackStanding;
        private static float contentHeight;
        private static bool attemptedLoad;

        public static void Draw(Vector2 feet, float visibleHeight, float walkTime,
            bool facingRight, float activity)
        {
            if (!Finite(feet.x) || !Finite(feet.y) || !Finite(visibleHeight) || visibleHeight <= 0f) return;
            EnsureResources();
            Sprite rest = WalkFrames[RestFrame] != null ? WalkFrames[RestFrame] : fallbackStanding;
            if (rest == null || contentHeight <= 0f) return;

            float scale = Mathf.Min(visibleHeight, MaximumVisibleHeight) / contentHeight;
            float blend = Mathf.SmoothStep(0f, 1f, Finite(activity) ? Mathf.Clamp01(activity) : 0f);
            Sprite walking = WalkFrames[FrameIndex(walkTime)];
            if (walking == null) blend = 0f;
            Matrix4x4 oldMatrix = GUI.matrix;
            Color oldColor = GUI.color;
            try
            {
                // Reflect around the registered feet, not the padded cell's centre.
                if (!facingRight)
                    GUI.matrix = oldMatrix * Matrix4x4.TRS(new Vector3(feet.x, feet.y, 0f),
                        Quaternion.identity, new Vector3(-1f, 1f, 1f)) *
                        Matrix4x4.Translate(new Vector3(-feet.x, -feet.y, 0f));
                if (walking == rest) blend = 0f; // Do not dim a neutral pose by drawing it twice.
                DrawFrame(rest, feet, scale, 1f - blend, oldColor, rest != fallbackStanding);
                if (blend > 0f) DrawFrame(walking, feet, scale, blend, oldColor, true);
            }
            finally
            {
                GUI.matrix = oldMatrix;
                GUI.color = oldColor;
            }
        }

        internal static int FrameIndex(float walkTime) => Finite(walkTime)
            ? (int)(System.Math.Floor(System.Math.Max(0d, walkTime) * FramesPerSecond) % FrameCount)
            : 0;

        // Full animation cells include transparent padding. Their pivot, not their rectangle bottom,
        // is the sole. One common content-height scale keeps every frame the same physical size.
        internal static Rect RectAtFeet(Sprite sprite, Vector2 feet, float scale, bool soleRegistered = true)
        {
            if (sprite == null || scale <= 0f) return new Rect(feet, Vector2.zero);
            float pixelScale = scale / sprite.pixelsPerUnit;
            float pivotY = soleRegistered ? sprite.pivot.y : 0f;
            return new Rect(feet.x - sprite.pivot.x * pixelScale,
                feet.y - (sprite.rect.height - pivotY) * pixelScale,
                sprite.rect.width * pixelScale, sprite.rect.height * pixelScale);
        }

        private static void DrawFrame(Sprite sprite, Vector2 feet, float scale, float opacity,
            Color tint, bool soleRegistered)
        {
            if (sprite == null || opacity <= 0f) return;
            Texture2D texture = sprite.texture;
            Rect source = sprite.textureRect;
            GUI.color = new Color(tint.r, tint.g, tint.b, tint.a * opacity);
            GUI.DrawTextureWithTexCoords(RectAtFeet(sprite, feet, scale, soleRegistered), texture,
                new Rect(source.x / texture.width, source.y / texture.height,
                    source.width / texture.width, source.height / texture.height), true);
        }

        private static void EnsureResources()
        {
            if (attemptedLoad) return;
            attemptedLoad = true;
            bool complete = true;
            for (int i = 0; i < FrameCount; i++)
            {
                WalkFrames[i] = RopeBodyVisual.GetDioramaWalkFrame(i, out Vector2 size);
                if (WalkFrames[i] == null) { complete = false; break; }
                contentHeight = size.y;
            }
            if (complete && contentHeight > 0f) return;
            System.Array.Clear(WalkFrames, 0, WalkFrames.Length);
            fallbackStanding = CraftPlayerArt.LoadStandingSprite("Art/HimoHitoPlayer-v1");
            contentHeight = fallbackStanding != null ? fallbackStanding.bounds.size.y : 0f;
        }

        private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
