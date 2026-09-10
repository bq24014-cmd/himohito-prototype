using UnityEngine;

namespace HimoHito
{
    /// <summary>
    /// Display-only wood covering the existing collider rectangle. Texture
    /// density is measured in world units instead of stretching to each bank.
    /// </summary>
    [ExecuteAlways, DisallowMultipleComponent]
    public sealed class CraftWoodPlatformVisual : MonoBehaviour
    {
        private const string BodyName = "Craft Wood Body";
        private const string BlocksResource = "Art/HimoHitoCraftWoodBlocks-v1";
        private const string GrainResource = "Art/HimoHitoCraftWoodGrain-v1";
        public const float TextureWorldWidth = 4f;
        public const float ThinFloorWorldHeight = 1.2f;
        private BoxCollider2D floor;
        private SpriteRenderer source;
        private MeshRenderer display;
        private Mesh mesh;
        private Material material;
        private Texture2D blocksTexture, grainTexture, lastTexture;
        private Vector2 lastSize, lastOffset;
        private Vector3 lastScale;
        private int lastOrder, lastLayer;

        public static bool Ensure(GameObject target)
        {
            TryEnsure(target, out bool changed);
            return changed;
        }

        /// <summary>True means the new art is available, even when already up to date.</summary>
        public static bool TryEnsure(GameObject target, out bool changed)
        {
            changed = false;
            if (target == null || !target.TryGetComponent(out BoxCollider2D _) ||
                !target.TryGetComponent(out SpriteRenderer _)) return false;
            if (!TryLoadTextures(out Texture2D blocks, out Texture2D grain))
            {
                if (target.TryGetComponent(out CraftWoodPlatformVisual existing))
                    changed = existing.RestoreLegacy();
                return false;
            }
            if (!target.TryGetComponent(out CraftWoodPlatformVisual visual))
            {
                visual = target.AddComponent<CraftWoodPlatformVisual>();
                changed = true;
            }
            if (!visual.enabled) { visual.enabled = true; changed = true; }
            visual.blocksTexture = blocks;
            visual.grainTexture = grain;
            changed |= visual.Refresh();
            changed |= visual.HideLegacy();
            changed |= WoodenPlatformDepthVisual.Ensure(target);
            return true;
        }

        internal static bool TryLoadTextures(out Texture2D blocks, out Texture2D grain)
        {
            blocks = Resources.Load<Texture2D>(BlocksResource);
            grain = Resources.Load<Texture2D>(GrainResource);
            return blocks != null && grain != null;
        }

        private void OnEnable()
        {
            if (!TryLoadTextures(out blocksTexture, out grainTexture))
            { RestoreLegacy(); return; }
            Refresh();
            HideLegacy();
        }

        private void LateUpdate() => Refresh();

        private bool Refresh()
        {
            if (floor == null) floor = GetComponent<BoxCollider2D>();
            if (source == null) source = GetComponent<SpriteRenderer>();
            if (floor == null || source == null || !enabled) return false;
            if (blocksTexture == null || grainTexture == null)
            {
                if (!TryLoadTextures(out blocksTexture, out grainTexture)) return RestoreLegacy();
            }
            Vector3 scale = transform.lossyScale;
            float sx = Mathf.Max(0.001f, Mathf.Abs(scale.x));
            float sy = Mathf.Max(0.001f, Mathf.Abs(scale.y));
            float worldHeight = floor.size.y * sy;
            Texture2D texture = worldHeight < ThinFloorWorldHeight ? grainTexture : blocksTexture;
            if (mesh != null && material != null && display != null && display.enabled &&
                lastSize == floor.size && lastOffset == floor.offset && lastScale == scale &&
                lastTexture == texture && lastOrder == source.sortingOrder && lastLayer == source.sortingLayerID)
                return false;

            if (display == null)
            {
                Transform child = transform.Find(BodyName);
                if (child == null)
                {
                    child = new GameObject(BodyName).transform;
                    child.SetParent(transform, false);
                }
                child.gameObject.hideFlags = HideFlags.DontSave;
                child.gameObject.layer = gameObject.layer;
                if (!child.TryGetComponent(out display)) display = child.gameObject.AddComponent<MeshRenderer>();
                if (!child.TryGetComponent(out MeshFilter _)) child.gameObject.AddComponent<MeshFilter>();
                display.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                display.receiveShadows = false;
            }
            if (material == null)
            {
                Shader shader = Shader.Find("Sprites/Default");
                if (shader == null) return false;
                material = new Material(shader)
                { name = "Craft Wood Body Material", hideFlags = HideFlags.HideAndDontSave };
            }
            if (mesh == null) mesh = new Mesh
                { name = "Craft Wood Body Mesh", hideFlags = HideFlags.HideAndDontSave };
            float left = floor.offset.x - floor.size.x * 0.5f;
            float bottom = floor.offset.y - floor.size.y * 0.5f;
            float right = left + floor.size.x, top = bottom + floor.size.y;
            float tileHeight = texture == blocksTexture ? TextureWorldWidth :
                TextureWorldWidth * texture.height / Mathf.Max(1f, texture.width);
            float u = floor.size.x * sx / TextureWorldWidth;
            float v = worldHeight / Mathf.Max(0.01f, tileHeight);
            mesh.Clear();
            mesh.vertices = new[]
            {
                new Vector3(left, bottom), new Vector3(right, bottom),
                new Vector3(right, top), new Vector3(left, top)
            };
            // Keep the top at V=0: taller banks add wood below rather than
            // resizing blocks, stitches or grain at the walking surface.
            mesh.uv = new[] { new Vector2(0f, -v), new Vector2(u, -v), new Vector2(u, 0f), Vector2.zero };
            mesh.colors = new[] { Color.white, Color.white, Color.white, Color.white };
            mesh.triangles = new[] { 0, 2, 1, 0, 3, 2 };
            mesh.RecalculateBounds();
            material.mainTexture = texture;
            display.sharedMaterial = material;
            display.GetComponent<MeshFilter>().sharedMesh = mesh;
            display.sortingLayerID = source.sortingLayerID;
            display.sortingOrder = source.sortingOrder + 1;
            display.enabled = true;
            // Do not reset this child's transform or property block here.
            // Recovery-stair reveal owns its temporary rise and fade.
            lastSize = floor.size; lastOffset = floor.offset; lastScale = scale;
            lastTexture = texture; lastOrder = source.sortingOrder; lastLayer = source.sortingLayerID;
            HideLegacy();
            return true;
        }

        private bool HideLegacy()
        {
            bool changed = SetLegacyChildrenActive(false);
            if (source == null) source = GetComponent<SpriteRenderer>();
            if (source != null && source.enabled) { source.enabled = false; changed = true; }
            return changed;
        }

        private bool RestoreLegacy()
        {
            blocksTexture = grainTexture = null;
            bool changed = SetLegacyChildrenActive(true);
            if (display != null && display.enabled) { display.enabled = false; changed = true; }
            if (source == null) source = GetComponent<SpriteRenderer>();
            if (source != null && !source.enabled) { source.enabled = true; changed = true; }
            return changed;
        }

        private bool SetLegacyChildrenActive(bool active)
        {
            bool changed = false;
            foreach (Transform child in transform)
            {
                bool legacy = child.name == "Orange Block Platform Visual" ||
                    child.name == "Terrain Top Edge" || child.name.StartsWith("Orange Block Platform Tile ",
                        System.StringComparison.Ordinal);
                if (!legacy || child.gameObject.activeSelf == active) continue;
                child.gameObject.SetActive(active);
                changed = true;
            }
            return changed;
        }

        private void OnDisable()
        {
            // Disabling only this presentation must not leave an invisible
            // solid floor. Inactive recovery stairs stay completely hidden.
            if (gameObject.activeInHierarchy) RestoreLegacy();
            if (display != null) display.enabled = false;
            ReleaseResources();
        }

        private void OnDestroy()
        {
            // OnDisable normally restores it first; this is also safe for an
            // already-disabled component and only touches surviving floor art.
            if (gameObject.activeInHierarchy) RestoreLegacy();
            ReleaseResources();
            if (display != null) DestroyOwned(display.gameObject);
        }

        private void ReleaseResources()
        {
            if (display != null)
            {
                display.sharedMaterial = null;
                if (display.TryGetComponent(out MeshFilter filter)) filter.sharedMesh = null;
            }
            DestroyOwned(mesh); DestroyOwned(material);
            mesh = null; material = null; lastTexture = null;
        }

        private static void DestroyOwned(Object owned)
        {
            if (owned == null) return;
            if (Application.isPlaying) Destroy(owned);
            else DestroyImmediate(owned);
        }
    }
}
