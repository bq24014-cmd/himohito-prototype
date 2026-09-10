using System.Collections.Generic;
using UnityEngine;

namespace HimoHito
{
    /// <summary>Tiled rail artwork fitted to the existing box; no collider or gameplay writes.</summary>
    [ExecuteAlways, DisallowMultipleComponent]
    public sealed class CraftRailPlatformVisual : MonoBehaviour
    {
        public const string VisualName = "Blue Railway Platform Visual";
        public const string ResourcePath = "Art/HimoHitoCraftRail-v1";
        public const float NativeHeight = .6f;
        public const float BodyNativeHeight = .54f;
        public const float TieNativeHeight = .34f;
        public const float TieNativePitch = 1f;
        public const float CapHeightRatio = .5f;
        private const string LegacyResourcePath = "Art/TutorialRailPlatform-v1";
        private const string TiePrefix = "Craft Rail Tie ";
        [SerializeField] private bool allowTutorialLegacy;
        private BoxCollider2D floor;
        private SpriteRenderer source, display;
        private Texture2D sourceTexture;
        private Sprite processed, tiled, tieSprite;
        private readonly List<SpriteRenderer> ties = new List<SpriteRenderer>();
        private int tieCount;
        private bool usingCraft;

        public static bool TryEnsure(GameObject target, out bool changed, bool allowTutorialLegacy = false)
        {
            changed = false;
            if (!Eligible(target, allowTutorialLegacy)) return false;
            if (Resources.Load<Texture2D>(ResourcePath) == null)
            {
                if (target.TryGetComponent(out CraftRailPlatformVisual existing)) changed = existing.RestoreLegacy();
                return false;
            }
            if (!target.TryGetComponent(out CraftRailPlatformVisual visual))
            {
                visual = target.AddComponent<CraftRailPlatformVisual>();
                changed = true;
            }
            visual.allowTutorialLegacy = allowTutorialLegacy;
            if (!visual.enabled) { visual.enabled = true; changed = true; }
            changed |= visual.Refresh();
            return visual.usingCraft;
        }

        private static bool Eligible(GameObject target, bool legacy)
            => target != null && target.activeInHierarchy && target.TryGetComponent(out BoxCollider2D _) &&
                target.TryGetComponent(out SpriteRenderer _) &&
                (target.TryGetComponent(out SwingPassThroughRailPlatform _) ||
                 target.TryGetComponent(out OneWayRailPlatform _) ||
                 (legacy && target.name == "Landing 1" && target.scene.name == "Tutorial"));

        public bool Refresh() => Refresh(true);
        private void OnEnable() => Refresh(true);
        private void LateUpdate() => Refresh(false);

        private bool Refresh(bool restoreVisibility)
        {
            if (!enabled || !Eligible(gameObject, allowTutorialLegacy)) return RestoreLegacy();
            if (floor == null) floor = GetComponent<BoxCollider2D>();
            if (source == null) source = GetComponent<SpriteRenderer>();
            if (!TryLoadParts()) return RestoreLegacy();
            DiscoverDisplayAndTies(true);
            // Hiding an existing rail belongs to gameplay. Only an explicit refresh restores visibility.
            if (!restoreVisibility && usingCraft && (!display.enabled || !display.gameObject.activeSelf))
                return SetTieVisibility(false);
            Vector3 parentScale = transform.lossyScale;
            float sx = Mathf.Max(.0001f, Mathf.Abs(parentScale.x));
            float sy = Mathf.Max(.0001f, Mathf.Abs(parentScale.y));
            float worldHeight = floor.size.y * sy;
            if (worldHeight <= .0001f || floor.size.x <= .0001f) return RestoreLegacy();
            float uniformScale = worldHeight / NativeHeight;
            Vector3 localScale = new Vector3(uniformScale / sx, uniformScale / sy, 1f);
            float bodyLift = (NativeHeight - BodyNativeHeight) * .5f;
            Vector3 localPosition = new Vector3(floor.offset.x, floor.offset.y + bodyLift * uniformScale / sy, 0f);
            float nativeWidth = floor.size.x * sx / uniformScale;
            Vector2 size = new Vector2(nativeWidth, BodyNativeHeight);
            float tieScale = Mathf.Min(1f, nativeWidth / tieSprite.bounds.size.x);
            float margin = NativeHeight * .5f + tieSprite.bounds.size.x * tieScale * .5f;
            int count = Mathf.Clamp(Mathf.FloorToInt((nativeWidth - margin * 2f) / TieNativePitch) + 1, 1, 128);
            Transform body = display.transform;
            if (usingCraft && display.sprite == tiled && display.drawMode == SpriteDrawMode.Tiled &&
                display.tileMode == SpriteTileMode.Continuous && display.size == size &&
                body.localPosition == localPosition && body.localRotation == Quaternion.identity &&
                body.localScale == localScale && display.sortingLayerID == source.sortingLayerID &&
                display.sortingOrder == source.sortingOrder + 1 && display.color == Color.white &&
                body.gameObject.layer == gameObject.layer && tieCount == count && ties.Count >= count &&
                !source.enabled && display.enabled && body.gameObject.activeSelf) return SetTieVisibility(true);

            body.localPosition = localPosition;
            body.localRotation = Quaternion.identity;
            body.localScale = localScale;
            body.gameObject.layer = gameObject.layer;
            body.gameObject.SetActive(true);
            display.sprite = tiled;
            display.drawMode = SpriteDrawMode.Tiled;
            display.tileMode = SpriteTileMode.Continuous;
            display.size = size;
            display.color = Color.white;
            display.sortingLayerID = source.sortingLayerID;
            display.sortingOrder = source.sortingOrder + 1;
            display.enabled = true;
            source.enabled = false;
            UpdateTies(count, tieScale, -NativeHeight * .5f - bodyLift);
            usingCraft = true;
            return true;
        }

        private bool TryLoadParts()
        {
            if (sourceTexture != null && processed != null && processed.texture != null && tiled != null && tieSprite != null)
                return true;
            sourceTexture = Resources.Load<Texture2D>(ResourcePath);
            processed = sourceTexture != null
                ? TutorialFirstSectionVisuals.LoadProcessedToySprite(ResourcePath, useMipMaps: true) : null;
            if (processed == null) return false;
            Color32[] pixels;
            try { pixels = sourceTexture.GetPixels32(); }
            catch (UnityException) { return false; }
            int split = sourceTexture.height - Mathf.RoundToInt(sourceTexture.height * .56f);
            if (!TryCrop(pixels, sourceTexture.width, split, sourceTexture.height, out Rect body) ||
                !TryCrop(pixels, sourceTexture.width, 0, split, out Rect tie)) return false;
            ReleaseOwnedSprites();
            float cap = Mathf.Min(body.height * CapHeightRatio, body.width * .45f);
            tiled = Sprite.Create(processed.texture, body, new Vector2(.5f, .5f), body.height / BodyNativeHeight,
                0, SpriteMeshType.FullRect, new Vector4(cap, 0f, cap, 0f), false);
            tiled.name = "Craft Rail Fixed Caps";
            tiled.hideFlags = HideFlags.HideAndDontSave;
            tieSprite = Sprite.Create(processed.texture, tie, new Vector2(.5f, 0f), tie.height / TieNativeHeight,
                0, SpriteMeshType.FullRect);
            tieSprite.name = "Craft Rail Whole Wooden Tie";
            tieSprite.hideFlags = HideFlags.HideAndDontSave;
            return true;
        }

        private void DiscoverDisplayAndTies(bool create)
        {
            if (display == null)
            {
                Transform child = transform.Find(VisualName);
                if (child == null && create)
                {
                    child = new GameObject(VisualName).transform;
                    child.SetParent(transform, false);
                }
                if (child == null) return;
                if (!child.TryGetComponent(out display) && create)
                    display = child.gameObject.AddComponent<SpriteRenderer>();
            }
            if (display == null || ties.Count > 0) return;
            foreach (Transform part in display.transform)
                if (part.name.StartsWith(TiePrefix, System.StringComparison.Ordinal) &&
                    part.TryGetComponent(out SpriteRenderer tie)) ties.Add(tie);
        }

        // Regions use source pixel coordinates; the shared processed texture keeps the full atlas extent.
        private static bool TryCrop(Color32[] pixels, int width, int bottom, int top, out Rect rect)
        {
            int minX = width, maxX = -1, minY = top, maxY = bottom - 1;
            for (int y = bottom; y < top; y++)
                for (int x = 0; x < width; x++)
                {
                    Color32 p = pixels[y * width + x];
                    int low = Mathf.Min(p.r, Mathf.Min(p.g, p.b)), high = Mathf.Max(p.r, Mathf.Max(p.g, p.b));
                    if (p.a == 0 || (low >= 205 && high - low <= 28)) continue;
                    minX = Mathf.Min(minX, x); maxX = Mathf.Max(maxX, x);
                    minY = Mathf.Min(minY, y); maxY = Mathf.Max(maxY, y);
                }
            rect = default;
            if (maxX < minX || maxY < minY) return false;
            minX = Mathf.Max(0, minX - 2); maxX = Mathf.Min(width - 1, maxX + 2);
            minY = Mathf.Max(bottom, minY - 2); maxY = Mathf.Min(top - 1, maxY + 2);
            rect = new Rect(minX, minY, maxX - minX + 1, maxY - minY + 1);
            return true;
        }

        private void UpdateTies(int count, float scale, float bottom)
        {
            while (ties.Count < count)
            {
                GameObject child = new GameObject(TiePrefix + ties.Count);
                child.transform.SetParent(display.transform, false);
                ties.Add(child.AddComponent<SpriteRenderer>());
            }
            tieCount = count;
            for (int i = 0; i < ties.Count; i++)
            {
                SpriteRenderer tie = ties[i];
                tie.gameObject.SetActive(i < count);
                tie.enabled = i < count;
                if (i >= count) continue;
                tie.transform.localPosition = new Vector3((i - (count - 1) * .5f) * TieNativePitch, bottom, 0f);
                tie.transform.localRotation = Quaternion.identity;
                tie.transform.localScale = new Vector3(scale, scale, 1f);
                tie.gameObject.layer = gameObject.layer;
                tie.sprite = tieSprite;
                tie.drawMode = SpriteDrawMode.Simple;
                tie.color = Color.white;
                tie.sortingLayerID = source.sortingLayerID;
                tie.sortingOrder = source.sortingOrder + 2;
            }
        }

        private bool SetTieVisibility(bool visible)
        {
            bool changed = false;
            for (int i = 0; i < ties.Count; i++)
                if (ties[i] != null && ties[i].enabled != (visible && i < tieCount))
                { ties[i].enabled = visible && i < tieCount; changed = true; }
            return changed;
        }

        private bool RestoreLegacy()
        {
            DiscoverDisplayAndTies(false);
            bool changed = usingCraft;
            usingCraft = false;
            changed |= SetTieVisibility(false);
            tieCount = 0;
            if (source == null) source = GetComponent<SpriteRenderer>();
            if (floor == null) floor = GetComponent<BoxCollider2D>();
            Sprite legacy = Resources.Load<Texture2D>(LegacyResourcePath) != null
                ? TutorialFirstSectionVisuals.LoadProcessedToySprite(LegacyResourcePath) : null;
            if (display != null)
            {
                if (display.drawMode != SpriteDrawMode.Simple) { display.drawMode = SpriteDrawMode.Simple; changed = true; }
                if (display.sprite != legacy) { display.sprite = legacy; changed = true; }
                if (legacy != null && floor != null)
                {
                    Vector3 scale = new Vector3(floor.size.x / Mathf.Max(.0001f, legacy.bounds.size.x),
                        floor.size.y / Mathf.Max(.0001f, legacy.bounds.size.y), 1f);
                    Vector3 position = new Vector3(floor.offset.x, floor.offset.y, 0f);
                    if (display.transform.localScale != scale) { display.transform.localScale = scale; changed = true; }
                    if (display.transform.localPosition != position) { display.transform.localPosition = position; changed = true; }
                    if (display.transform.localRotation != Quaternion.identity) { display.transform.localRotation = Quaternion.identity; changed = true; }
                }
                bool visible = legacy != null;
                if (display.enabled != visible) { display.enabled = visible; changed = true; }
            }
            if (source != null)
            {
                bool visible = legacy == null || display == null || !display.gameObject.activeSelf;
                if (visible && source.sprite == null && TryGetComponent(out SolidSprite fallback)) fallback.Color = fallback.Color;
                if (source.enabled != visible) { source.enabled = visible; changed = true; }
            }
            ReleaseSprites();
            return changed;
        }

        private void OnDisable()
        {
            if (gameObject.activeInHierarchy) RestoreLegacy();
            else ReleaseSprites();
        }
        private void OnDestroy()
        {
            if (gameObject.activeInHierarchy) RestoreLegacy();
            ReleaseSprites();
            foreach (SpriteRenderer tie in ties)
                if (tie != null) DestroyOwned(tie.gameObject);
            ties.Clear();
        }
        private void ReleaseSprites()
        {
            ReleaseOwnedSprites();
            processed = null;
            sourceTexture = null;
        }
        private void ReleaseOwnedSprites()
        {
            if (display != null && display.sprite == tiled) display.sprite = null;
            foreach (SpriteRenderer tie in ties)
                if (tie != null && tie.sprite == tieSprite) tie.sprite = null;
            DestroyOwned(tiled);
            DestroyOwned(tieSprite);
            tiled = null;
            tieSprite = null;
        }
        private static void DestroyOwned(Object value)
        {
            if (value == null) return;
            if (Application.isPlaying) Destroy(value); else DestroyImmediate(value);
        }
    }
}
