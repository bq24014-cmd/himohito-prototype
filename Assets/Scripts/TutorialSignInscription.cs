using UnityEngine;

namespace HimoHito
{
    /// <summary>Small painted control diagrams on the existing wooden sign, not a second UI.</summary>
    [ExecuteAlways, DisallowMultipleComponent]
    public sealed class TutorialSignInscription : MonoBehaviour
    {
        [SerializeField, Range(1, 4)] private int section = 1;
        private Transform drawing;
        private Material paint, yarn;
        private int drawnSection;
        private float worldWidth;
        private int sortingLayer, sortingOrder;
        private static readonly Color Cream = new Color(1f, .91f, .68f);
        private static readonly Color Ink = new Color(.27f, .10f, .055f, .85f);
        private static readonly Color Blue = new Color(.20f, .71f, .94f);
        private static readonly Color Green = new Color(.30f, .93f, .69f);

        public bool Configure(int value)
        {
            value = Mathf.Clamp(value, 1, 4);
            bool changed = section != value || drawing == null;
            section = value;
            if (changed) Rebuild();
            return changed;
        }

        private void OnEnable() => Rebuild();
        private void Update()
        {
            if (drawing == null || drawnSection != section) Rebuild();
        }

        private void Rebuild()
        {
            Clear();
            if (!TryGetComponent(out SpriteRenderer board) || board.sprite == null) return;
            Shader shader = Shader.Find("Sprites/Default");
            if (shader == null) return;
            paint = new Material(shader) { name = "Sign Painted Marks", hideFlags = HideFlags.HideAndDontSave };
            yarn = new Material(shader) { name = "Sign Yarn Diagram", hideFlags = HideFlags.HideAndDontSave };
            yarn.mainTexture = YarnRopeTexture.Load();
            Bounds bounds = board.sprite.bounds;
            worldWidth = bounds.size.x * Mathf.Abs(transform.lossyScale.x);
            sortingLayer = board.sortingLayerID;
            sortingOrder = board.sortingOrder + 1;
            drawing = new GameObject("Painted Tutorial Controls") { hideFlags = HideFlags.HideAndDontSave }.transform;
            drawing.SetParent(transform, false);
            drawing.localPosition = new Vector3(bounds.min.x, bounds.min.y, -.01f);
            drawing.localScale = new Vector3(bounds.size.x, bounds.size.y, 1f);
            drawnSection = section;

            // Coordinates are normalized to the original square artwork, inside the board face.
            switch (section)
            {
                case 1:
                    Vector2 swingHead = PlayerPicture(new Vector2(.255f, .54f), .125f);
                    Curve(swingHead, new Vector2(.34f, .72f), new Vector2(.42f, .80f), true);
                    Ring(new Vector2(.42f, .80f), Blue);
                    Arrow(new Vector2(.27f, .50f), new Vector2(.51f, .50f));
                    Key("E", new Vector2(.72f, .69f), .15f, .23f);
                    break;
                case 2:
                    LengthChoicePicture();
                    Key("W", new Vector2(.72f, .76f), .11f, .22f);
                    Key("S", new Vector2(.72f, .61f), .11f, .22f);
                    break;
                case 3:
                    Shelf(.20f, .76f, -.055f);
                    Shelf(.55f, .76f, .055f);
                    Curve(new Vector2(.20f, .76f), new Vector2(.375f, .48f), new Vector2(.55f, .76f), true);
                    Ring(new Vector2(.20f, .76f), Green);
                    Ring(new Vector2(.55f, .76f), Green);
                    PlayerPicture(new Vector2(.375f, .625f), .125f);
                    Key("Q", new Vector2(.72f, .69f), .15f, .23f);
                    break;
                case 4:
                    Curve(new Vector2(.20f, .76f), new Vector2(.27f, .65f), new Vector2(.375f, .81f), false);
                    Curve(new Vector2(.375f, .81f), new Vector2(.48f, .65f), new Vector2(.55f, .76f), false);
                    Curve(new Vector2(.20f, .76f), new Vector2(.375f, .40f), new Vector2(.55f, .76f), true);
                    Ring(new Vector2(.375f, .81f), Blue);
                    Ring(new Vector2(.20f, .76f), Green);
                    Ring(new Vector2(.55f, .76f), Green);
                    Arrow(new Vector2(.375f, .72f), new Vector2(.375f, .62f));
                    PlayerPicture(new Vector2(.46f, .625f), .115f);
                    Key("F", new Vector2(.72f, .69f), .15f, .23f);
                    break;
            }
        }

        private void LengthChoicePicture()
        {
            Color danger = new Color(1f, .29f, .34f);
            // Feet trajectories, not two pre-built bridges. The longer swing reaches the spikes.
            Stroke(new[] { new Vector3(.15f, .69f), new Vector3(.235f, .69f),
                new Vector3(.235f, .515f) }, .013f, Cream, false, "Left Bank");
            Stroke(new[] { new Vector3(.56f, .515f), new Vector3(.56f, .69f),
                new Vector3(.615f, .69f) }, .013f, Cream, false, "Right Bank");
            DashedPath(.58f, Green, "Safe Swing Path");
            DashedPath(.37f, danger, "Too Long Swing Path");
            for (int i = 0; i < 3; i++)
            {
                float x = .345f + i * .045f;
                Stroke(new[] { new Vector3(x - .018f, .49f), new Vector3(x, .55f),
                    new Vector3(x + .018f, .49f), new Vector3(x - .018f, .49f) },
                    .011f, danger, false, "Valley Spike");
            }
            Stroke(new[] { new Vector3(.555f, .748f), new Vector3(.570f, .731f),
                new Vector3(.600f, .769f) }, .010f, Green, false, "Safe Check");
            Stroke(new[] { new Vector3(.50f, .545f), new Vector3(.526f, .515f) },
                .010f, danger, false, "Too Long Cross");
            Stroke(new[] { new Vector3(.50f, .515f), new Vector3(.526f, .545f) },
                .010f, danger, false, "Too Long Cross");
            Vector2 head = PlayerPicture(new Vector2(.395f, .635f), .115f);
            Curve(head, Vector2.Lerp(head, new Vector2(.395f, .838f), .5f),
                new Vector2(.395f, .838f), true);
            Ring(new Vector2(.395f, .838f), Blue);
        }

        private void DashedPath(float controlY, Color color, string label)
        {
            Vector2 a = new Vector2(.235f, .69f), b = new Vector2(.395f, controlY), c = new Vector2(.56f, .69f);
            for (int i = 0; i < 18; i += 2)
            {
                float t0 = i / 18f, t1 = (i + 1f) / 18f;
                Vector2 p0 = (1f - t0) * (1f - t0) * a + 2f * (1f - t0) * t0 * b + t0 * t0 * c;
                Vector2 p1 = (1f - t1) * (1f - t1) * a + 2f * (1f - t1) * t1 * b + t1 * t1 * c;
                Stroke(new Vector3[] { p0, p1 }, .006f, color, false, label);
            }
        }

        private Vector2 PlayerPicture(Vector2 feet, float height)
        {
            Sprite sprite = TutorialFirstSectionVisuals.LoadProcessedToySprite("Art/HimoHitoPlayer-v1");
            if (sprite == null) return feet + Vector2.up * height;
            var child = new GameObject("Small Player Picture") { hideFlags = HideFlags.HideAndDontSave };
            child.transform.SetParent(drawing, false);
            var renderer = child.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sharedMaterial = paint;
            renderer.sortingLayerID = sortingLayer;
            renderer.sortingOrder = sortingOrder + 3;
            Bounds bounds = sprite.bounds;
            // Fit the original art uniformly. No opaque backing, no independent X/Y stretch.
            float scale = height / Mathf.Max(.001f, bounds.size.y);
            child.transform.localScale = Vector3.one * scale;
            child.transform.localPosition = (Vector3)feet -
                new Vector3(bounds.center.x, bounds.min.y, 0f) * scale;
            return feet + Vector2.up * (height * .94f);
        }

        private void Curve(Vector2 a, Vector2 b, Vector2 c, bool active)
        {
            var points = new Vector3[25];
            for (int i = 0; i < points.Length; i++)
            {
                float t = i / (points.Length - 1f);
                points[i] = (1f - t) * (1f - t) * a + 2f * (1f - t) * t * b + t * t * c;
            }
            Stroke(points, active ? .018f : .013f,
                active ? Color.white : new Color(.62f, .47f, .61f), active);
        }

        private void Ring(Vector2 center, Color color)
        {
            var points = new Vector3[33];
            for (int i = 0; i < points.Length; i++)
            {
                float angle = i / (points.Length - 1f) * Mathf.PI * 2f;
                points[i] = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * .029f;
            }
            Stroke(points, .014f, color, false);
        }

        private void Shelf(float x, float y, float direction)
        {
            Stroke(new[] { new Vector3(x + direction, y), new Vector3(x, y) }, .017f, Cream, false);
        }

        private void Arrow(Vector2 from, Vector2 to)
        {
            Vector2 direction = (to - from).normalized;
            Vector2 cross = new Vector2(-direction.y, direction.x);
            Stroke(new Vector3[] { from, to, to - direction * .028f + cross * .022f,
                to, to - direction * .028f - cross * .022f }, .009f, Cream, false);
        }

        private void Stroke(Vector3[] points, float width, Color color, bool textured, string label = null)
        {
            MakeLine(label != null ? label + " Outline" : "Paint Outline", points, width + .006f, Ink, paint, sortingOrder);
            MakeLine(label ?? (textured ? "Yarn Picture" : "Paint Picture"), points, width, color,
                textured && yarn.mainTexture != null ? yarn : paint, sortingOrder + 1);
        }

        private void MakeLine(string label, Vector3[] points, float width, Color color, Material material, int order)
        {
            var child = new GameObject(label) { hideFlags = HideFlags.HideAndDontSave };
            child.transform.SetParent(drawing, false);
            var line = child.AddComponent<LineRenderer>();
            line.useWorldSpace = false;
            line.sharedMaterial = material;
            line.positionCount = points.Length;
            line.SetPositions(points);
            line.startWidth = line.endWidth = width * worldWidth;
            line.startColor = line.endColor = color;
            line.numCapVertices = line.numCornerVertices = 3;
            line.sortingLayerID = sortingLayer;
            line.sortingOrder = order;
            line.textureMode = LineTextureMode.Tile;
            if (material.mainTexture != null)
            {
                // Local vertices are in sprite-normalized units; keep yarn twists small and consistent.
                Texture texture = material.mainTexture;
                line.textureScale = new Vector2(1f / Mathf.Max(.001f, width * texture.width / texture.height), 1f);
            }
        }

        private void Key(string value, Vector2 center, float height, float maxWidth)
        {
            Font font = Resources.Load<Font>("Fonts/MPlusRounded1c-Bold");
            if (font == null) return;
            var child = new GameObject("Operation Key " + value) { hideFlags = HideFlags.HideAndDontSave };
            child.transform.SetParent(drawing, false);
            var text = child.AddComponent<TextMesh>();
            text.font = font;
            text.fontSize = 64;
            text.characterSize = 1f;
            text.anchor = TextAnchor.MiddleCenter;
            text.alignment = TextAlignment.Center;
            text.color = Cream;
            text.text = value;
            var renderer = child.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = font.material;
            renderer.sortingLayerID = sortingLayer;
            renderer.sortingOrder = sortingOrder + 2;
            Bounds glyph = renderer.localBounds;
            float scale = Mathf.Min(height / Mathf.Max(.001f, glyph.size.y), maxWidth / Mathf.Max(.001f, glyph.size.x));
            child.transform.localScale = Vector3.one * scale;
            child.transform.localPosition = (Vector3)center - glyph.center * scale;
        }

        private void Clear()
        {
            if (drawing != null)
            {
                drawing.gameObject.SetActive(false);
                Dispose(drawing.gameObject);
                drawing = null;
            }
            Dispose(paint); Dispose(yarn);
            paint = yarn = null;
        }

        private static void Dispose(Object value)
        {
            if (value == null) return;
            if (Application.isPlaying) Destroy(value);
            else DestroyImmediate(value);
        }

        private void OnDisable() => Clear();
        private void OnDestroy() => Clear();
    }
}
