using UnityEngine;

namespace HimoHito
{
    /// <summary>
    /// Keeps the two-point rope-platform anchor visible as a round toy eye.
    /// The runtime sprite is reassigned whenever the object is enabled so an
    /// editor scene save cannot leave a textureless temporary sprite behind.
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class RopeAnchorRingVisual : MonoBehaviour
    {
        [SerializeField] private Color color =
            new Color(0.33f, 1f, 0.76f);
        [SerializeField] private int sortingLayerId;
        [SerializeField] private int sortingOrder = 6;

        private const int TextureSize = 64;
        private static Sprite sharedSprite;

        public bool Configure(
            Color targetColor,
            int targetSortingLayerId,
            int targetSortingOrder)
        {
            bool changed = color != targetColor ||
                           sortingLayerId != targetSortingLayerId ||
                           sortingOrder != targetSortingOrder;
            color = targetColor;
            sortingLayerId = targetSortingLayerId;
            sortingOrder = targetSortingOrder;
            Apply();
            return changed;
        }

        private void OnEnable()
        {
            Apply();
        }

        private void OnValidate()
        {
            Apply();
        }

        private void Apply()
        {
            SpriteRenderer renderer = GetComponent<SpriteRenderer>();
            renderer.sprite = GetSharedSprite();
            renderer.color = color;
            renderer.sortingLayerID = sortingLayerId;
            renderer.sortingOrder = sortingOrder;
            renderer.enabled = true;
        }

        private static Sprite GetSharedSprite()
        {
            if (sharedSprite != null && sharedSprite.texture != null)
            {
                return sharedSprite;
            }

            Texture2D texture = new Texture2D(
                TextureSize,
                TextureSize,
                TextureFormat.RGBA32,
                false)
            {
                name = "Runtime Rope Anchor Ring",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave
            };

            Vector2 center =
                new Vector2(TextureSize - 1, TextureSize - 1) * 0.5f;
            float radiusScale = TextureSize * 0.5f;
            for (int y = 0; y < TextureSize; y++)
            {
                for (int x = 0; x < TextureSize; x++)
                {
                    float radius = Vector2.Distance(
                        new Vector2(x, y),
                        center) / radiusScale;
                    float outerMask = 1f - SmoothRange(0.9f, 1f, radius);
                    float innerMask = SmoothRange(0.43f, 0.54f, radius);
                    float alpha = outerMask * innerMask;

                    float normalizedX = x / (float)(TextureSize - 1);
                    float normalizedY = y / (float)(TextureSize - 1);
                    float highlight = Mathf.Clamp01(
                        0.72f +
                        normalizedY * 0.20f -
                        normalizedX * 0.08f);
                    texture.SetPixel(
                        x,
                        y,
                        new Color(
                            highlight,
                            highlight,
                            highlight,
                            alpha));
                }
            }

            texture.Apply(false, false);
            sharedSprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, TextureSize, TextureSize),
                new Vector2(0.5f, 0.5f),
                TextureSize);
            sharedSprite.name = "Runtime Rope Anchor Ring";
            sharedSprite.hideFlags = HideFlags.HideAndDontSave;
            return sharedSprite;
        }

        private static float SmoothRange(
            float minimum,
            float maximum,
            float value)
        {
            float progress = Mathf.InverseLerp(minimum, maximum, value);
            return Mathf.SmoothStep(0f, 1f, progress);
        }
    }
}
