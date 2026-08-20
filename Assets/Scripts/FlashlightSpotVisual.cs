using UnityEngine;

namespace HimoHito
{
    /// <summary>
    /// Draws a soft, circular flashlight spot without requiring an art asset.
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class FlashlightSpotVisual : MonoBehaviour
    {
        [SerializeField] private Color color = new Color(1f, 1f, 1f, 0.72f);

        private const int TextureSize = 64;
        private static Sprite sharedSprite;

        public Color Color
        {
            get => color;
            set
            {
                color = value;
                Apply();
            }
        }

        private void OnEnable()
        {
            Apply();
        }

        private void OnValidate()
        {
            if (TryGetComponent(out SpriteRenderer spriteRenderer))
            {
                spriteRenderer.color = color;
            }
        }

        private void Apply()
        {
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            spriteRenderer.sprite = GetSharedSprite();
            spriteRenderer.color = color;
            spriteRenderer.sortingOrder = 4;
        }

        private static Sprite GetSharedSprite()
        {
            if (sharedSprite != null)
            {
                return sharedSprite;
            }

            Texture2D texture = new Texture2D(TextureSize, TextureSize)
            {
                name = "Runtime Flashlight Spot",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave
            };

            Vector2 center = new Vector2(TextureSize - 1, TextureSize - 1) * 0.5f;
            float radius = TextureSize * 0.5f;
            for (int y = 0; y < TextureSize; y++)
            {
                for (int x = 0; x < TextureSize; x++)
                {
                    float normalizedDistance = Vector2.Distance(
                        new Vector2(x, y),
                        center) / radius;
                    float colorProgress = SmoothRange(
                        0.12f,
                        0.92f,
                        normalizedDistance);
                    Color centerColor = new Color(1f, 0.99f, 0.9f);
                    Color edgeColor = new Color(1f, 0.6f, 0.2f);
                    Color radialColor = Color.Lerp(centerColor, edgeColor, colorProgress);

                    float hotCenter = 1f - SmoothRange(
                        0.05f,
                        0.3f,
                        normalizedDistance);
                    float mainSpot = 1f - SmoothRange(
                        0.56f,
                        0.84f,
                        normalizedDistance);
                    float outerGlow = 1f - SmoothRange(
                        0.72f,
                        1f,
                        normalizedDistance);
                    float alpha = Mathf.Clamp01(
                        hotCenter * 0.4f +
                        mainSpot * 0.36f +
                        outerGlow * 0.24f);
                    texture.SetPixel(
                        x,
                        y,
                        new Color(radialColor.r, radialColor.g, radialColor.b, alpha));
                }
            }

            texture.Apply();
            sharedSprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, TextureSize, TextureSize),
                new Vector2(0.5f, 0.5f),
                TextureSize);
            sharedSprite.name = "Runtime Flashlight Spot";
            sharedSprite.hideFlags = HideFlags.HideAndDontSave;
            return sharedSprite;
        }

        private static float SmoothRange(float minimum, float maximum, float value)
        {
            float progress = Mathf.InverseLerp(minimum, maximum, value);
            return Mathf.SmoothStep(0f, 1f, progress);
        }
    }
}
