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
        private const string CraftResourcePath = "Art/HimoHitoCraftHookGreen-v1";

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
            // Spriteの差し替えをOnValidate中に行うと、Unityが
            // OnSpriteTilingPropertyChangeを送れず警告を出す。
            // Sprite本体はOnEnable/Configureで設定し、ここでは
            // Inspectorで編集可能な描画順だけを反映する。
            if (TryGetComponent(out SpriteRenderer renderer))
            {
                renderer.sortingLayerID = sortingLayerId;
                renderer.sortingOrder = sortingOrder;
            }
        }

        private void Apply()
        {
            SpriteRenderer renderer = GetComponent<SpriteRenderer>();
            Sprite sprite = GetSharedSprite();
            if (renderer.sprite != sprite)
            {
                renderer.sprite = sprite;
            }
            if (renderer.color != Color.white)
            {
                renderer.color = Color.white;
            }
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
                HookWoodMountVisual.Ensure(gameObject);
            }
        }

        private static Sprite GetSharedSprite()
        {
            // The common loader already caches sprites by resource path. Resolve
            // the preferred path on Apply so later asset imports replace fallback art.
            Sprite sprite = Resources.Load<Texture2D>(CraftResourcePath) != null
                ? TutorialFirstSectionVisuals.LoadProcessedToySprite(CraftResourcePath, true) : null;
            return sprite != null ? sprite :
                TutorialFirstSectionVisuals.LoadProcessedToySprite(ResourcePath);
        }
    }
}
