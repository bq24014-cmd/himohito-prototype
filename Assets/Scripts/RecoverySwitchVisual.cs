using UnityEngine;

namespace HimoHito
{
    /// <summary>Restores the craft-style switch artwork after scene/domain reloads.</summary>
    [ExecuteAlways]
    [RequireComponent(typeof(SpriteRenderer), typeof(SolidSprite))]
    public sealed class RecoverySwitchVisual : MonoBehaviour
    {
        // Match the visible wooden surface, below the collider's top edge.
        private const float SurfaceInset = 0.16f;
        private Transform artwork;
        private SpriteRenderer artworkRenderer;
        private float pressAmount, pressStart, pressTarget, pressElapsed = .24f;
        private bool poseInitialized;

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
            const string craftPath = "Art/HimoHitoCraftRecoverySwitch-v1";
            Sprite sprite = Resources.Load<Texture2D>(craftPath) != null
                ? TutorialFirstSectionVisuals.LoadProcessedToySprite(craftPath, true) : null;
            if (sprite == null)
                sprite = TutorialFirstSectionVisuals.LoadProcessedToySprite("Art/RecoverySwitch-v1");
            if (sprite == null)
            {
                legacy.enabled = true;
                return false;
            }

            artwork = transform.Find("Recovery Switch Artwork");
            bool changed = artwork == null || legacy.enabled;
            if (artwork == null)
            {
                artwork = new GameObject("Recovery Switch Artwork").transform;
                artwork.SetParent(transform, false);
            }
            if (!artwork.TryGetComponent(out SpriteRenderer renderer))
                renderer = artwork.gameObject.AddComponent<SpriteRenderer>();
            artworkRenderer = renderer;
            changed |= renderer.sprite != sprite || !renderer.enabled;
            renderer.sprite = sprite;
            renderer.enabled = true;
            artwork.gameObject.SetActive(true);
            // The original controller's green state remains the source of truth.
            Color state = GetComponent<SolidSprite>().Color;
            bool isOn = state.g > state.r;
            float target = isOn ? 1f : 0f;
            if (!poseInitialized || !Application.isPlaying)
            {
                pressAmount = pressTarget = target;
                pressElapsed = .24f;
                poseInitialized = true;
            }
            else if (pressTarget != target)
            {
                pressStart = pressAmount;
                pressTarget = target;
                pressElapsed = 0f;
            }
            renderer.sortingLayerID = legacy.sortingLayerID;
            renderer.sortingOrder = legacy.sortingOrder + 1;
            ApplyPressPose();
            legacy.enabled = false;
            return changed;
        }

        private void LateUpdate()
        {
            if (Application.isPlaying) AdvancePress(Time.deltaTime);
        }

        private void AdvancePress(float dt)
        {
            if (dt <= 0f || pressElapsed >= .24f) return;
            pressElapsed = Mathf.Min(.24f, pressElapsed + dt);
            pressAmount = Mathf.Lerp(pressStart, pressTarget,
                Mathf.SmoothStep(0f, 1f, pressElapsed / .24f));
            ApplyPressPose();
        }

        private void ApplyPressPose()
        {
            if (artwork == null || artworkRenderer == null || artworkRenderer.sprite == null) return;
            Sprite sprite = artworkRenderer.sprite;
            artworkRenderer.color = Color.Lerp(Color.white, new Color(.85f, .85f, .85f), pressAmount);
            float width = 1.1f;
            float height = width * sprite.bounds.size.y / sprite.bounds.size.x;
            height *= 1f - .25f * pressAmount;
            Vector3 parentScale = transform.lossyScale;
            artwork.localScale = new Vector3(
                width / sprite.bounds.size.x / Mathf.Max(0.001f, Mathf.Abs(parentScale.x)),
                height / sprite.bounds.size.y / Mathf.Max(0.001f, Mathf.Abs(parentScale.y)), 1f);
            artwork.localRotation = Quaternion.identity;
            float floorTop = MainStageSectionThreeSetup.LowDeadEndPosition.y +
                MainStageSectionThreeSetup.LowDeadEndSize.y * 0.5f;
            artwork.position = new Vector3(transform.position.x,
                floorTop - SurfaceInset + height * 0.5f, transform.position.z);
        }
    }
}
