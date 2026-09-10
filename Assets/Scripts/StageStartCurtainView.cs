using UnityEngine;

namespace HimoHito
{
    /// <summary>Draws one yarn-pulled sheet; the caller owns input, timing and scene loading.</summary>
    internal sealed class StageStartCurtainView
    {
        private const float ClothTileSize = 240f;
        private static readonly Color Indigo = new Color(.085f, .10f, .18f, 1f);
        private static readonly Color Stitch = new Color(.93f, .81f, .63f, .62f);
        private static readonly Color Rose = new Color(1f, .57f, .67f, 1f);
        private Texture2D cloth, yarn;

        public void Draw(float width, float height, Vector2 normalizedOrigin,
            float threadProgress, float coverProgress, float revealProgress, string stageTitle, bool loading)
        {
            if (width <= 0f || height <= 0f) return;
            threadProgress = Mathf.Clamp01(threadProgress);
            coverProgress = Mathf.Clamp01(coverProgress);
            revealProgress = Mathf.Clamp01(revealProgress);
            if (revealProgress >= 1f || (threadProgress <= 0f && coverProgress <= 0f && revealProgress <= 0f))
                return;

            EnsureResources();
            Matrix4x4 oldMatrix = GUI.matrix;
            Color oldColor = GUI.color;
            bool oldEnabled = GUI.enabled;
            int oldDepth = GUI.depth;
            try
            {
                GUI.matrix = Matrix4x4.identity;
                GUI.color = Color.white;
                GUI.enabled = true;
                GUI.depth = -10000;
                float scale = Mathf.Clamp(Mathf.Min(width / 1280f, height / 720f), .5f, 2f);
                float margin = 40f * scale;
                Vector2 origin = new Vector2(Mathf.Clamp01(normalizedOrigin.x) * width,
                    Mathf.Clamp01(normalizedOrigin.y) * height);
                Vector2 feedEnd = new Vector2(-margin, origin.y - 3f * scale);
                Vector2 pullEnd = new Vector2(width + margin, origin.y - 3f * scale);

                if (coverProgress <= 0f && revealProgress <= 0f)
                {
                    DrawYarn(origin, feedEnd, threadProgress, 5f * scale, 3f * scale);
                    DrawYarn(origin, pullEnd, threadProgress, 5f * scale, 3f * scale);
                    DrawKnot(origin, 25f * scale * Mathf.SmoothStep(0f, 1f, threadProgress));
                    return;
                }

                // The same oversize sheet crosses the viewport without exposing an aspect-ratio border.
                float sheetWidth = width + margin * 2f;
                float left = revealProgress > 0f
                    ? Mathf.Lerp(-margin, width + margin, revealProgress)
                    : Mathf.Lerp(-sheetWidth - margin, -margin, coverProgress);
                Rect sheet = new Rect(left, -margin, sheetWidth, height + margin * 2f);
                float hem = 24f * scale;
                Vector2 knot = new Vector2(sheet.xMax - hem * .45f, origin.y);
                if (revealProgress <= 0f)
                {
                    // Keep the exact completed thread pose until the moving cloth naturally hides it.
                    DrawYarn(origin, feedEnd, 1f, 5f * scale, 3f * scale);
                    DrawYarn(origin, pullEnd, 1f, 5f * scale, 3f * scale);
                    DrawKnot(origin, 25f * scale);
                }

                DrawCloth(sheet, Color.white);
                DrawHem(new Rect(sheet.x, sheet.y, hem, sheet.height), false, scale);
                DrawHem(new Rect(sheet.xMax - hem, sheet.y, hem, sheet.height), true, scale);
                if (revealProgress <= 0f) DrawKnot(knot, 32f * scale);
                // Deliberately no pulsing loading indicator or text: the cloth remains still while covered.
            }
            finally
            {
                GUI.matrix = oldMatrix;
                GUI.color = oldColor;
                GUI.enabled = oldEnabled;
                GUI.depth = oldDepth;
            }
        }

        private void EnsureResources()
        {
            if (cloth == null)
            {
                cloth = Resources.Load<Texture2D>("Art/HimoHitoStageCurtainCloth-v1");
                if (cloth != null) cloth.wrapMode = TextureWrapMode.Repeat;
            }
            if (yarn == null) yarn = YarnRopeTexture.Load();
        }

        private void DrawCloth(Rect rect, Color tint)
        {
            // An opaque backing also guarantees coverage if the optional art is missing or has alpha.
            GUI.color = Indigo;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            if (cloth == null) return;
            GUI.color = tint;
            GUI.DrawTextureWithTexCoords(rect, cloth,
                new Rect(0f, 0f, rect.width / ClothTileSize, rect.height / ClothTileSize));
        }

        private void DrawHem(Rect hem, bool leading, float scale)
        {
            DrawCloth(hem, new Color(.76f, .80f, .90f, 1f));
            float crease = leading ? hem.x + 3f * scale : hem.xMax - 5f * scale;
            GUI.color = new Color(.02f, .03f, .08f, .36f);
            GUI.DrawTexture(new Rect(crease, hem.y, 2f * scale, hem.height), Texture2D.whiteTexture);
            GUI.color = new Color(.64f, .68f, .81f, .11f);
            GUI.DrawTexture(new Rect(hem.center.x, hem.y, 3f * scale, hem.height), Texture2D.whiteTexture);
            float seam = leading ? hem.x + 8f * scale : hem.xMax - 10f * scale;
            GUI.color = Stitch;
            for (float y = hem.y; y < hem.yMax; y += 17f * scale)
                GUI.DrawTexture(new Rect(seam, y, 1.4f * scale, 5f * scale), Texture2D.whiteTexture);
        }

        private void DrawYarn(Vector2 from, Vector2 to, float progress, float thickness, float sag)
        {
            const int segments = 24;
            Vector2 previous = from;
            float distance = 0f;
            for (int i = 1; i <= segments; i++)
            {
                float t = progress * i / segments;
                Vector2 next = Vector2.Lerp(from, to, t) + Vector2.up * (4f * t * (1f - t) * sag);
                Vector2 delta = next - previous;
                if (delta.sqrMagnitude > .001f)
                {
                    Matrix4x4 matrix = GUI.matrix;
                    GUIUtility.RotateAroundPivot(Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg, previous);
                    GUI.color = yarn != null ? Color.white : Rose;
                    float tile = yarn != null ? thickness * yarn.width / yarn.height : 16f;
                    GUI.DrawTextureWithTexCoords(new Rect(previous.x, previous.y - thickness / 2f,
                        delta.magnitude + .5f, thickness), yarn != null ? yarn : Texture2D.whiteTexture,
                        new Rect(distance / tile, 0f, (delta.magnitude + .5f) / tile, 1f));
                    GUI.matrix = matrix;
                    distance += delta.magnitude;
                }
                previous = next;
            }
        }

        private static void DrawKnot(Vector2 center, float size)
        {
            Sprite sprite = HimoHitoUiParts.MountingKnotSprite;
            if (sprite == null) return;
            Rect source = sprite.textureRect;
            float width = size * source.width / source.height;
            GUI.color = Color.white;
            GUI.DrawTextureWithTexCoords(new Rect(center.x - width / 2f, center.y - size / 2f, width, size),
                sprite.texture, new Rect(source.x / sprite.texture.width, source.y / sprite.texture.height,
                    source.width / sprite.texture.width, source.height / sprite.texture.height));
        }
    }
}
