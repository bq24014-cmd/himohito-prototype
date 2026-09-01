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
        [SerializeField] private int sortingLayerId;
        [SerializeField] private int sortingOrder = 6;

        private const string ResourcePath =
            "Art/TutorialRopeAnchorRing-v1";
        private static Sprite sharedSprite;

        public bool Configure(
            int targetSortingLayerId,
            int targetSortingOrder)
        {
            bool changed = sortingLayerId != targetSortingLayerId ||
                           sortingOrder != targetSortingOrder;
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
            Sprite sprite = GetSharedSprite();
            renderer.sprite = sprite;
            renderer.color = Color.white;
            renderer.sortingLayerID = sortingLayerId;
            renderer.sortingOrder = sortingOrder;
            renderer.enabled = sprite != null;

            if (sprite != null)
            {
                Vector2 size = sprite.bounds.size;
                transform.localScale = new Vector3(
                    1f / Mathf.Max(0.01f, size.x),
                    1f / Mathf.Max(0.01f, size.y),
                    1f);
            }
        }

        private static Sprite GetSharedSprite()
        {
            if (sharedSprite != null && sharedSprite.texture != null)
            {
                return sharedSprite;
            }

            sharedSprite =
                TutorialFirstSectionVisuals.LoadProcessedToySprite(
                    ResourcePath);
            return sharedSprite;
        }
    }
}
