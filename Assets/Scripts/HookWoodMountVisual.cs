using UnityEngine;

namespace HimoHito
{
    /// <summary>Display-only toy mounting plate. The ring remains the attachment target.</summary>
    [ExecuteAlways, DisallowMultipleComponent, RequireComponent(typeof(SpriteRenderer))]
    public sealed class HookWoodMountVisual : MonoBehaviour
    {
        private const string CraftResourcePath = "Art/HimoHitoCraftHookMount-v1";
        private Transform decoration;
        private SpriteRenderer source;
        private Sprite lastSprite;
        private Sprite lastWood;
        private int lastLayer, lastOrder;
        private Material yarn;

        public static void Ensure(GameObject ring)
        {
            if (!ring.TryGetComponent(out HookWoodMountVisual visual))
                visual = ring.AddComponent<HookWoodMountVisual>();
            visual.Refresh(true);
        }

        private void OnEnable() => Refresh(true);
        private void LateUpdate() => Refresh();

        private void Refresh(bool refreshArtwork = false)
        {
            if (source == null) source = GetComponent<SpriteRenderer>();
            if (source.sprite == null || !source.enabled)
            {
                if (decoration != null) decoration.gameObject.SetActive(false);
                return;
            }
            if (decoration == null || lastSprite != source.sprite ||
                lastLayer != source.sortingLayerID || lastOrder != source.sortingOrder ||
                (refreshArtwork && lastWood != GetWoodSprite()))
                Build();
            if (decoration != null && !decoration.gameObject.activeSelf)
                decoration.gameObject.SetActive(true);
        }

        private void Build()
        {
            Clear();
            Sprite wood = GetWoodSprite();
            Sprite knot = HimoHitoUiParts.MountingKnotSprite;
            if (wood == null || knot == null) return;
            lastSprite = source.sprite;
            lastWood = wood;
            lastLayer = source.sortingLayerID;
            lastOrder = source.sortingOrder;
            decoration = new GameObject("Wood And Yarn Hook Mount")
                { hideFlags = HideFlags.HideAndDontSave }.transform;
            decoration.SetParent(transform, false);
            Bounds bounds = source.sprite.bounds;
            decoration.localPosition = bounds.center;
            decoration.localScale = new Vector3(bounds.size.x, bounds.size.y, 1f);

            // Mount the upper rim; keep the central hole, lower rim and rope endpoint clear.
            Picture("Mount Soft Shadow", wood, new Vector2(.025f, .555f),
                new Vector2(1.65f, .60f), new Color(.20f, .10f, .08f, .38f), -4);
            Picture("Wooden Mount", wood, new Vector2(0f, .60f),
                new Vector2(1.65f, .60f), Color.white, -3);
            // Small dark wooden pins reuse the rounded knot silhouette, rather than new textures.
            Picture("Left Wooden Pin", knot, new Vector2(-.60f, .60f),
                Vector2.one * .115f, new Color(.30f, .18f, .10f), -2);
            Picture("Right Wooden Pin", knot, new Vector2(.60f, .60f),
                Vector2.one * .115f, new Color(.30f, .18f, .10f), -2);

            Shader shader = Shader.Find("Sprites/Default");
            if (shader != null)
            {
                yarn = new Material(shader) { name = "Hook Mount Yarn", hideFlags = HideFlags.HideAndDontSave };
                yarn.mainTexture = YarnRopeTexture.Load();
                Tail("Left Tie Tail", new[] { new Vector3(-.045f,.48f), new Vector3(-.12f,.40f),
                    new Vector3(-.18f,.31f), new Vector3(-.23f,.33f) });
                Tail("Right Tie Tail", new[] { new Vector3(.045f,.49f), new Vector3(.14f,.40f),
                    new Vector3(.22f,.36f), new Vector3(.27f,.40f) });
            }
            Picture("Pink Mounting Knot", knot, new Vector2(0f, .51f),
                Vector2.one * .30f, Color.white, 1);
        }

        private static Sprite GetWoodSprite()
        {
            Sprite sprite = Resources.Load<Texture2D>(CraftResourcePath) != null
                ? TutorialFirstSectionVisuals.LoadProcessedToySprite(CraftResourcePath, true) : null;
            return sprite != null ? sprite : HimoHitoUiParts.WoodMountSprite;
        }

        private void Picture(string label, Sprite sprite, Vector2 center, Vector2 size, Color color, int order)
        {
            var child = new GameObject(label) { hideFlags = HideFlags.HideAndDontSave };
            child.transform.SetParent(decoration, false);
            var renderer = child.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = color;
            renderer.sortingLayerID = lastLayer;
            renderer.sortingOrder = lastOrder + order;
            Bounds b = sprite.bounds;
            Vector3 scale = new Vector3(size.x / b.size.x, size.y / b.size.y, 1f);
            child.transform.localScale = scale;
            child.transform.localPosition = (Vector3)center - Vector3.Scale(b.center, scale);
        }

        private void Tail(string label, Vector3[] points)
        {
            var child = new GameObject(label) { hideFlags = HideFlags.HideAndDontSave };
            child.transform.SetParent(decoration, false);
            var line = child.AddComponent<LineRenderer>();
            line.sharedMaterial = yarn;
            line.useWorldSpace = false;
            line.positionCount = points.Length;
            line.SetPositions(points);
            line.startWidth = line.endWidth = .065f * Mathf.Abs(decoration.lossyScale.x);
            line.startColor = line.endColor = yarn.mainTexture != null ? Color.white : new Color(1f,.27f,.52f);
            line.numCapVertices = line.numCornerVertices = 3;
            line.sortingLayerID = lastLayer;
            line.sortingOrder = lastOrder + 1;
            line.textureMode = LineTextureMode.Tile;
            Texture texture = yarn.mainTexture;
            if (texture != null)
                line.textureScale = new Vector2(1f / (.065f * texture.width / texture.height), 1f);
        }

        private void OnDisable() => Clear();
        private void OnDestroy() => Clear();
        private void Clear()
        {
            if (decoration != null)
            {
                decoration.gameObject.SetActive(false);
                Dispose(decoration.gameObject);
                decoration = null;
            }
            Dispose(yarn);
            yarn = null;
        }
        private static void Dispose(Object value)
        {
            if (value == null) return;
            if (Application.isPlaying) Destroy(value);
            else DestroyImmediate(value);
        }
    }
}
