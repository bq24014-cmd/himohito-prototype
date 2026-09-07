using UnityEngine;

namespace HimoHito
{
    /// <summary>Restores temporary toy artwork after scene/domain reloads.</summary>
    [ExecuteAlways]
    [RequireComponent(typeof(SpriteRenderer), typeof(SolidSprite))]
    public sealed class RecoverySwitchVisual : MonoBehaviour
    {
        // Match the visible wooden surface, below the collider's top edge.
        private const float SurfaceInset = 0.16f;

        public static bool Ensure(GameObject target)
        {
            if (target == null) return false;
            bool added = !target.TryGetComponent(out RecoverySwitchVisual visual);
            if (added) visual = target.AddComponent<RecoverySwitchVisual>();
            return visual.Refresh() || added;
        }

        private void OnEnable() { Refresh(); }
        private void Start() { Refresh(); }

        public bool Refresh()
        {
            SpriteRenderer legacy = GetComponent<SpriteRenderer>();
            Sprite sprite = TutorialFirstSectionVisuals.LoadProcessedToySprite(
                "Art/RecoverySwitch-v1");
            if (sprite == null)
            {
                legacy.enabled = true;
                return false;
            }

            Transform artwork = transform.Find("Recovery Switch Artwork");
            bool changed = artwork == null || legacy.enabled;
            if (artwork == null)
            {
                artwork = new GameObject("Recovery Switch Artwork").transform;
                artwork.SetParent(transform, false);
            }
            if (!artwork.TryGetComponent(out SpriteRenderer renderer))
                renderer = artwork.gameObject.AddComponent<SpriteRenderer>();
            changed |= renderer.sprite != sprite || !renderer.enabled;
            renderer.sprite = sprite;
            renderer.enabled = true;
            artwork.gameObject.SetActive(true);
            // The original controller's green state remains the source of truth.
            Color state = GetComponent<SolidSprite>().Color;
            bool isOn = state.g > state.r;
            renderer.color = isOn ? new Color(0.85f, 0.85f, 0.85f) : Color.white;
            renderer.sortingLayerID = legacy.sortingLayerID;
            renderer.sortingOrder = legacy.sortingOrder + 1;
            float width = 1.1f;
            float height = width * sprite.bounds.size.y / sprite.bounds.size.x;
            if (isOn) height *= 0.75f;
            Vector3 parentScale = transform.lossyScale;
            artwork.localScale = new Vector3(
                width / sprite.bounds.size.x / Mathf.Max(0.001f, Mathf.Abs(parentScale.x)),
                height / sprite.bounds.size.y / Mathf.Max(0.001f, Mathf.Abs(parentScale.y)), 1f);
            artwork.localRotation = Quaternion.identity;
            float floorTop = MainStageSectionThreeSetup.LowDeadEndPosition.y +
                MainStageSectionThreeSetup.LowDeadEndSize.y * 0.5f;
            artwork.position = new Vector3(transform.position.x,
                floorTop - SurfaceInset + height * 0.5f, transform.position.z);
            legacy.enabled = false;
            return changed;
        }
    }
}
