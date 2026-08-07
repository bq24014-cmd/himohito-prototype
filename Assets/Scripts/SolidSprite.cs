using UnityEngine;

namespace HimoHito
{
    /// <summary>
    /// Creates a one-pixel white sprite at runtime and colors it.
    /// The prototype therefore needs no external art assets.
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class SolidSprite : MonoBehaviour
    {
        [SerializeField] private Color color = Color.white;

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
            Apply();
        }

        private void Apply()
        {
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            spriteRenderer.sprite = GetSharedSprite();
            spriteRenderer.color = color;
        }

        private static Sprite GetSharedSprite()
        {
            if (sharedSprite != null)
            {
                return sharedSprite;
            }

            Texture2D texture = new Texture2D(1, 1)
            {
                name = "PrototypeWhitePixel",
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave
            };
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();

            sharedSprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, 1f, 1f),
                new Vector2(0.5f, 0.5f),
                1f);
            sharedSprite.name = "PrototypeSquare";
            sharedSprite.hideFlags = HideFlags.HideAndDontSave;
            return sharedSprite;
        }
    }
}
