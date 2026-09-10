using System.Collections.Generic;
using UnityEngine;

namespace HimoHito
{
    /// <summary>
    /// Two quiet, camera-relative craft-room layers. Only background renderers
    /// move; the camera, gameplay transforms and physics are never changed.
    /// </summary>
    [ExecuteAlways, DefaultExecutionOrder(110), DisallowMultipleComponent]
    public sealed class HimoHitoCraftRoomBackground : MonoBehaviour
    {
        private const string FarResource = "Art/HimoHitoCraftRoomFar-v1";
        private const string MidResource = "Art/HimoHitoCraftRoomMid-v1";
        private const float AuthoredTileWidth = 44f;
        private const float FarFollow = 0.985f;
        private const float MidFollow = 0.93f;
        private const int TileCount = 3;
        private static readonly Color MidTint = new Color(0.78f, 0.82f, 0.94f, 1f);
        private static readonly HashSet<HimoHitoCraftRoomBackground> CacheUsers =
            new HashSet<HimoHitoCraftRoomBackground>();
        private static Texture2D keyedSource;
        private static Texture2D keyedTexture;
        private static Sprite keyedMidSprite;
#if UNITY_EDITOR
        private static Hash128 keyedContentHash;
#endif

        [SerializeField, HideInInspector] private string legacyFarName;

        private readonly SpriteRenderer[] farTiles = new SpriteRenderer[TileCount];
        private readonly SpriteRenderer[] midTiles = new SpriteRenderer[TileCount];
        private Sprite farSprite;
        private Sprite midSprite;
        private Camera lastCamera;
        private Vector3 lastCameraPosition;
        private float lastViewHeight;
        private float lastAspect;
        private bool layoutValid;

        public static bool Ensure(string legacyNearName, string legacyFarName)
        {
            // Validate both resources before hiding any already-working art.
            Sprite far = Resources.Load<Sprite>(FarResource);
            Sprite mid = far != null ? GetKeyedMidSprite() : null;
            if (far == null || mid == null)
            {
                GameObject existing = SceneObjectLookup.Find(legacyNearName);
                return existing != null &&
                    existing.TryGetComponent(out HimoHitoCraftRoomBackground previous) &&
                    previous.RestoreLegacy(legacyFarName);
            }

            bool changed = false;
            GameObject root = SceneObjectLookup.Find(legacyNearName);
            if (root == null)
            {
                root = new GameObject(legacyNearName);
                changed = true;
            }
            if (!root.activeSelf) { root.SetActive(true); changed = true; }

            // Stop the old UV animation first so it releases its runtime material.
            if (root.TryGetComponent(out HangingDecorSway sway) && sway.enabled)
            { sway.enabled = false; changed = true; }
            if (root.TryGetComponent(out TutorialBackgroundParallax parallax) && parallax.enabled)
            { parallax.enabled = false; changed = true; }
            if (root.TryGetComponent(out SpriteRenderer oldRenderer) && oldRenderer.enabled)
            { oldRenderer.enabled = false; changed = true; }
            GameObject oldFar = SceneObjectLookup.Find(legacyFarName);
            if (oldFar != null && oldFar.activeSelf)
            { oldFar.SetActive(false); changed = true; }

            Transform rootTransform = root.transform;
            if (rootTransform.position != Vector3.zero)
            { rootTransform.position = Vector3.zero; changed = true; }
            if (rootTransform.rotation != Quaternion.identity)
            { rootTransform.rotation = Quaternion.identity; changed = true; }
            if (rootTransform.localScale != Vector3.one)
            { rootTransform.localScale = Vector3.one; changed = true; }
            if (!root.TryGetComponent(out HimoHitoCraftRoomBackground background))
            {
                background = root.AddComponent<HimoHitoCraftRoomBackground>();
                changed = true;
            }
            if (!background.enabled) { background.enabled = true; changed = true; }
            if (background.legacyFarName != legacyFarName)
            { background.legacyFarName = legacyFarName; changed = true; }
            background.farSprite = far;
            background.midSprite = mid;
            CacheUsers.Add(background);
            changed |= background.EnsureTiles();
            changed |= background.RefreshLayout(true);
            return changed;
        }

        private void OnEnable()
        {
            farSprite = Resources.Load<Sprite>(FarResource);
            midSprite = farSprite != null ? GetKeyedMidSprite() : null;
            if (farSprite == null || midSprite == null)
            {
                RestoreLegacy(legacyFarName);
                return;
            }
            CacheUsers.Add(this);
            EnsureTiles();
            RefreshLayout(true);
        }

        private void LateUpdate() => RefreshLayout(false);

        private void OnDestroy()
        {
            CacheUsers.Remove(this);
            if (CacheUsers.Count == 0) ReleaseCachedArt(!Application.isPlaying);
        }

        private bool RestoreLegacy(string farName)
        {
            // A saved, already-upgraded scene also needs a fallback, not only
            // the first upgrade. Keep the original sprite as the recovery source.
            if (!TryGetComponent(out SpriteRenderer original) || original.sprite == null)
                return false;
            bool changed = false;
            farSprite = midSprite = null;
            layoutValid = false;
            for (int i = 0; i < TileCount; i++)
            {
                Transform farTile = transform.Find("Craft Far Tile " + i);
                Transform midTile = transform.Find("Craft Mid Tile " + i);
                if (farTile != null && farTile.gameObject.activeSelf)
                { farTile.gameObject.SetActive(false); changed = true; }
                if (midTile != null && midTile.gameObject.activeSelf)
                { midTile.gameObject.SetActive(false); changed = true; }
            }
            CacheUsers.Remove(this);
            if (CacheUsers.Count == 0) ReleaseCachedArt(!Application.isPlaying);

            bool isTutorial = gameObject.name == "Tutorial Night Child Room Background";
            Camera view = Camera.main;
            float cameraX = view != null ? view.transform.position.x : isTutorial ? -15f : -6f;
            Vector3 position = new Vector3(cameraX, 0f, 1f);
            float scale = (isTutorial ? 40f : 60f) / Mathf.Max(0.01f, original.sprite.bounds.size.x);
            Vector3 uniformScale = new Vector3(scale, scale, 1f);
            if (transform.position != position) { transform.position = position; changed = true; }
            if (transform.rotation != Quaternion.identity)
            { transform.rotation = Quaternion.identity; changed = true; }
            if (transform.localScale != uniformScale)
            { transform.localScale = uniformScale; changed = true; }
            if (!original.enabled) { original.enabled = true; changed = true; }
            if (original.forceRenderingOff) { original.forceRenderingOff = false; changed = true; }
            Color originalTint = new Color(1f, 1f, 1f, 0.84f);
            if (original.color != originalTint) { original.color = originalTint; changed = true; }
            if (TryGetComponent(out TutorialBackgroundParallax parallax) && !parallax.enabled)
            {
                parallax.Configure(isTutorial ? 0.9f : 0.97f);
                parallax.enabled = true;
                changed = true;
            }
            if (TryGetComponent(out HangingDecorSway sway) && !sway.enabled)
            { sway.enabled = true; changed = true; }
            if (string.IsNullOrEmpty(farName))
                farName = isTutorial ? "Tutorial Far Child Room Background" : "Main Stage Far Child Room Background";
            GameObject oldFar = SceneObjectLookup.Find(farName);
            if (oldFar != null && !oldFar.activeSelf)
            {
                if (oldFar.TryGetComponent(out TutorialBackgroundParallax farParallax))
                    farParallax.Configure(0.78f);
                oldFar.SetActive(true);
                changed = true;
            }
            return changed;
        }

        private bool EnsureTiles()
        {
            bool changed = false;
            for (int i = 0; i < TileCount; i++)
            {
                changed |= EnsureTile("Craft Far Tile " + i, farSprite,
                    Color.white, -120, ref farTiles[i]);
                changed |= EnsureTile("Craft Mid Tile " + i, midSprite,
                    MidTint, -100, ref midTiles[i]);
            }
            return changed;
        }

        private bool EnsureTile(string tileName, Sprite sprite, Color tint,
            int sortingOrder, ref SpriteRenderer renderer)
        {
            bool changed = false;
            Transform tile = transform.Find(tileName);
            if (tile == null)
            {
                tile = new GameObject(tileName).transform;
                tile.SetParent(transform, false);
                changed = true;
            }
            if (!tile.gameObject.activeSelf)
            { tile.gameObject.SetActive(true); changed = true; }
            if (!tile.TryGetComponent(out renderer))
            { renderer = tile.gameObject.AddComponent<SpriteRenderer>(); changed = true; }
            if (!renderer.enabled) { renderer.enabled = true; changed = true; }
            if (renderer.forceRenderingOff) { renderer.forceRenderingOff = false; changed = true; }
            if (renderer.sprite != sprite) { renderer.sprite = sprite; changed = true; }
            if (renderer.color != tint) { renderer.color = tint; changed = true; }
            if (renderer.sortingOrder != sortingOrder)
            { renderer.sortingOrder = sortingOrder; changed = true; }
            if (tile.localRotation != Quaternion.identity)
            { tile.localRotation = Quaternion.identity; changed = true; }
            return changed;
        }

        private bool RefreshLayout(bool force)
        {
            Camera view = Camera.main;
            if (view == null || !view.orthographic || farSprite == null || midSprite == null)
                return false;
            Vector3 cameraPosition = view.transform.position;
            float viewHeight = view.orthographicSize * 2f;
            float aspect = Mathf.Max(0.01f, view.aspect);
            if (!force && layoutValid && lastCamera == view &&
                lastCameraPosition == cameraPosition && lastViewHeight == viewHeight && lastAspect == aspect)
                return false;

            bool changed = LayoutLayer(farTiles, farSprite, FarFollow, 2f,
                cameraPosition, viewHeight, aspect);
            changed |= LayoutLayer(midTiles, midSprite, MidFollow, 1f,
                cameraPosition, viewHeight, aspect);
            lastCamera = view;
            lastCameraPosition = cameraPosition;
            lastViewHeight = viewHeight;
            lastAspect = aspect;
            layoutValid = true;
            return changed;
        }

        private static bool LayoutLayer(SpriteRenderer[] tiles, Sprite sprite,
            float follow, float depth, Vector3 cameraPosition, float viewHeight, float aspect)
        {
            Vector2 sourceSize = sprite.bounds.size;
            float sourceAspect = sourceSize.x / Mathf.Max(0.01f, sourceSize.y);
            // Uniform scaling preserves the full canvas. Three tiles cover even
            // ultrawide/resized views, with no stretching or finite-stage edges.
            float tileWidth = Mathf.Max(AuthoredTileWidth,
                (viewHeight + 0.5f) * sourceAspect, viewHeight * aspect + 0.5f);
            float scale = tileWidth / Mathf.Max(0.01f, sourceSize.x);
            float phase = cameraPosition.x * follow;
            int centerIndex = Mathf.FloorToInt((cameraPosition.x - phase) / tileWidth + 0.5f);
            bool changed = false;
            for (int i = 0; i < TileCount; i++)
            {
                SpriteRenderer renderer = tiles[i];
                if (renderer == null) continue;
                int tileIndex = centerIndex + i - 1;
                Vector3 position = new Vector3(phase + tileIndex * tileWidth, cameraPosition.y, depth);
                Vector3 uniformScale = new Vector3(scale, scale, 1f);
                bool mirrored = (tileIndex & 1) != 0;
                if (renderer.transform.localPosition != position)
                { renderer.transform.localPosition = position; changed = true; }
                if (renderer.transform.localScale != uniformScale)
                { renderer.transform.localScale = uniformScale; changed = true; }
                // Global parity, not pool slot, keeps touching source edges
                // identical when tiles recycle after a tour or checkpoint jump.
                if (renderer.flipX != mirrored)
                { renderer.flipX = mirrored; changed = true; }
            }
            return changed;
        }

        private static Sprite GetKeyedMidSprite()
        {
            Texture2D source = Resources.Load<Texture2D>(MidResource);
            if (source == null) return null;
#if UNITY_EDITOR
            // Reimport can update the same native Texture instance. Its asset
            // dependency hash also tracks changed image bytes/import settings.
            Hash128 contentHash = UnityEditor.AssetDatabase.GetAssetDependencyHash(
                UnityEditor.AssetDatabase.GetAssetPath(source));
            if (keyedMidSprite != null && keyedSource == source && keyedContentHash == contentHash)
                return keyedMidSprite;
#else
            if (keyedMidSprite != null && keyedSource == source) return keyedMidSprite;
#endif
            Color32[] pixels;
            try { pixels = source.GetPixels32(); }
            catch (UnityException exception)
            {
                Debug.LogWarning("Craft-room middle art must be readable: " + exception.Message);
                return null;
            }
            RemoveWhiteMatte(pixels, source.width, source.height);
            Texture2D texture = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false)
            {
                name = source.name + " Keyed Full Canvas",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave
            };
            texture.SetPixels32(pixels);
            texture.Apply(false, true);
            // Do not crop to the toys: blank space is authored layer alignment.
            Sprite replacement = Sprite.Create(texture, new Rect(0, 0, source.width, source.height),
                new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
            replacement.name = source.name + " Keyed Full Canvas";
            replacement.hideFlags = HideFlags.HideAndDontSave;
            Sprite previousSprite = keyedMidSprite;
            Texture2D previousTexture = keyedTexture;
            keyedMidSprite = replacement;
            keyedTexture = texture;
            keyedSource = source;
#if UNITY_EDITOR
            keyedContentHash = contentHash;
#endif
            // All live scenes share the cache. Rebind every borrower before
            // destroying the old native objects during an editor reimport.
            foreach (HimoHitoCraftRoomBackground owner in CacheUsers)
            {
                if (owner == null) continue;
                owner.midSprite = replacement;
                owner.layoutValid = false;
                foreach (SpriteRenderer tile in owner.midTiles)
                    if (tile != null) tile.sprite = replacement;
            }
            DestroyOwnedArt(previousSprite, !Application.isPlaying);
            DestroyOwnedArt(previousTexture, !Application.isPlaying);
            return keyedMidSprite;
        }

        private static void RemoveWhiteMatte(Color32[] pixels, int width, int height)
        {
            // First retain the established key. Distances below are frozen
            // before antialias correction, so it cannot erode farther inward.
            byte[] distanceFromWhite = new byte[pixels.Length];
            for (int i = 0; i < pixels.Length; i++)
            {
                Color32 pixel = pixels[i];
                byte low = System.Math.Min(pixel.r, System.Math.Min(pixel.g, pixel.b));
                byte high = System.Math.Max(pixel.r, System.Math.Max(pixel.g, pixel.b));
                if (low >= 228 && high - low <= 15) pixel.a = 0;
                pixels[i] = pixel;
                distanceFromWhite[i] = pixel.a == 0 ? (byte)0 : (byte)3;
            }
            for (int y = 0; y < height; y++) for (int x = 0; x < width; x++)
            {
                int index = y * width + x;
                if (pixels[index].a == 0) continue;
                for (int dy = -2; dy <= 2; dy++) for (int dx = -2; dx <= 2; dx++)
                {
                    int nx = x + dx, ny = y + dy;
                    if (nx < 0 || nx >= width || ny < 0 || ny >= height ||
                        pixels[ny * width + nx].a != 0) continue;
                    int distance = Mathf.Max(Mathf.Abs(dx), Mathf.Abs(dy));
                    if (distance < distanceFromWhite[index])
                        distanceFromWhite[index] = (byte)distance;
                }
            }

            for (int y = 0; y < height; y++) for (int x = 0; x < width; x++)
            {
                int index = y * width + x;
                if (distanceFromWhite[index] == 0 || distanceFromWhite[index] > 2) continue;
                Color32 pixel = pixels[index];
                Vector3 color = new Vector3(pixel.r, pixel.g, pixel.b) / 255f;
                Vector3 fromWhite = Vector3.one - color;
                float bestScore = float.PositiveInfinity;
                float bestAlpha = 1f;
                // Only unchanged interior pixels can supply the original yarn/
                // wood color. Match C = alpha * F + (1-alpha) * white locally.
                for (int dy = -4; dy <= 4; dy++) for (int dx = -4; dx <= 4; dx++)
                {
                    int nx = x + dx, ny = y + dy;
                    if (nx < 0 || nx >= width || ny < 0 || ny >= height) continue;
                    int neighbour = ny * width + nx;
                    if (distanceFromWhite[neighbour] < 3) continue;
                    Color32 sample = pixels[neighbour];
                    Vector3 reference = new Vector3(sample.r, sample.g, sample.b) / 255f;
                    Vector3 referenceFromWhite = Vector3.one - reference;
                    float alpha = Vector3.Dot(fromWhite, referenceFromWhite) /
                        Mathf.Max(0.001f, referenceFromWhite.sqrMagnitude);
                    if (alpha < 0.04f || alpha > 0.985f) continue;
                    float error = (fromWhite - referenceFromWhite * alpha).sqrMagnitude;
                    if (error > 0.003f) continue;
                    float score = error + (dx * dx + dy * dy) * 0.0004f;
                    if (score >= bestScore) continue;
                    bestScore = score;
                    bestAlpha = alpha;
                }
                if (bestAlpha >= 1f) continue;
                Vector3 unmatted = (color - Vector3.one * (1f - bestAlpha)) / bestAlpha;
                pixel.r = (byte)Mathf.RoundToInt(Mathf.Clamp01(unmatted.x) * 255f);
                pixel.g = (byte)Mathf.RoundToInt(Mathf.Clamp01(unmatted.y) * 255f);
                pixel.b = (byte)Mathf.RoundToInt(Mathf.Clamp01(unmatted.z) * 255f);
                pixel.a = (byte)Mathf.RoundToInt(pixel.a * bestAlpha);
                pixels[index] = pixel;
            }

            // Bilinear filtering interpolates RGB even at alpha zero. Bleed
            // nearby real edge colors into only the two transparent outer pixels,
            // keeping alpha zero, instead of letting white reappear when scaled.
            for (int y = 0; y < height; y++) for (int x = 0; x < width; x++)
            {
                int index = y * width + x;
                if (distanceFromWhite[index] != 0) continue;
                int nearestDistance = int.MaxValue;
                Color32 nearest = pixels[index];
                for (int dy = -2; dy <= 2; dy++) for (int dx = -2; dx <= 2; dx++)
                {
                    int nx = x + dx, ny = y + dy;
                    if (nx < 0 || nx >= width || ny < 0 || ny >= height) continue;
                    Color32 sample = pixels[ny * width + nx];
                    if (sample.a == 0) continue;
                    int distance = dx * dx + dy * dy;
                    if (distance >= nearestDistance) continue;
                    nearestDistance = distance;
                    nearest = sample;
                }
                nearest.a = 0;
                pixels[index] = nearest;
            }
        }

        private static void ReleaseCachedArt(bool immediately)
        {
            Sprite sprite = keyedMidSprite;
            Texture2D texture = keyedTexture;
            keyedMidSprite = null;
            keyedTexture = null;
            keyedSource = null;
            DestroyOwnedArt(sprite, immediately);
            DestroyOwnedArt(texture, immediately);
        }

        private static void DestroyOwnedArt(Object owned, bool immediately)
        {
            if (owned == null) return;
            if (immediately) DestroyImmediate(owned);
            else Destroy(owned);
        }

#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
        private static void RegisterEditorCleanup()
        {
            UnityEditor.AssemblyReloadEvents.beforeAssemblyReload -= BeforeEditorReload;
            UnityEditor.AssemblyReloadEvents.beforeAssemblyReload += BeforeEditorReload;
            UnityEditor.EditorApplication.quitting -= BeforeEditorReload;
            UnityEditor.EditorApplication.quitting += BeforeEditorReload;
        }

        private static void BeforeEditorReload()
        {
            // HideAndDontSave resources are not reclaimed by scene unloading.
            // Clear native references while the old static registry still exists.
            foreach (HimoHitoCraftRoomBackground owner in CacheUsers)
            {
                if (owner == null) continue;
                owner.midSprite = null;
                owner.layoutValid = false;
                foreach (SpriteRenderer tile in owner.midTiles)
                    if (tile != null) tile.sprite = null;
            }
            CacheUsers.Clear();
            ReleaseCachedArt(true);
        }
#endif
    }
}
