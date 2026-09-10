using UnityEngine;

namespace HimoHito
{
    /// <summary>
    /// Shows a normal rope Hook as a yarn-wrapped craft ring without
    /// changing the parent HookPoint, collider, position, or attachment point.
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class HimoHitoHookRingVisual : MonoBehaviour
    {
        private const string CraftResourcePath = "Art/HimoHitoCraftHookBlue-v1";
        [SerializeField] private int sortingLayerId;
        [SerializeField] private int sortingOrder = 6;
        [SerializeField, Min(0.1f)] private float worldDiameter = 0.72f;

        public bool Configure(
            int targetSortingLayerId,
            int targetSortingOrder,
            float targetWorldDiameter)
        {
            bool changed = sortingLayerId != targetSortingLayerId ||
                           sortingOrder != targetSortingOrder ||
                           !Mathf.Approximately(
                               worldDiameter,
                               targetWorldDiameter);
            sortingLayerId = targetSortingLayerId;
            sortingOrder = targetSortingOrder;
            worldDiameter = Mathf.Max(0.1f, targetWorldDiameter);
            Apply();
            return changed;
        }

        private void OnEnable()
        {
            Apply();
        }

        private void OnValidate()
        {
            if (TryGetComponent(out SpriteRenderer renderer))
            {
                renderer.sortingLayerID = sortingLayerId;
                renderer.sortingOrder = sortingOrder;
            }
        }

        private void Apply()
        {
            SpriteRenderer renderer = GetComponent<SpriteRenderer>();
            Sprite sprite = Resources.Load<Texture2D>(CraftResourcePath) != null
                ? TutorialFirstSectionVisuals.LoadProcessedToySprite(CraftResourcePath, true) : null;
            if (sprite == null) sprite = HimoHitoUiParts.ConnectorSprite;
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

            if (sprite == null)
            {
                return;
            }

            Vector2 spriteSize = sprite.bounds.size;
            Vector3 parentScale = transform.parent != null
                ? transform.parent.lossyScale
                : Vector3.one;
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = new Vector3(
                worldDiameter /
                    (Mathf.Max(0.01f, spriteSize.x) *
                     Mathf.Max(0.01f, Mathf.Abs(parentScale.x))),
                worldDiameter /
                    (Mathf.Max(0.01f, spriteSize.y) *
                     Mathf.Max(0.01f, Mathf.Abs(parentScale.y))),
                1f);
            HookWoodMountVisual.Ensure(gameObject);
        }
    }
}
