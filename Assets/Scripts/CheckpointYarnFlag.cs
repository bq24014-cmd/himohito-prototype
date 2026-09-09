using UnityEngine;

namespace HimoHito
{
    /// <summary>A small pennant knitted from the same yarn tile as the bridges.</summary>
    public sealed class CheckpointYarnFlag : MonoBehaviour
    {
        private const int Rows = 8;
        private const int Segments = 10;
        private readonly Vector3[] vertices = new Vector3[Rows * (Segments + 1) * 2];
        private Mesh cloth;
        private Material yarnMaterial;
        private Material poleMaterial;
        private LineRenderer pole;
        private LineRenderer highlight;
        private LineRenderer knot;
        private float elapsed;
        private float phase;

        public void Initialize(int section)
        {
            if (cloth != null) return;
            phase = section * 1.71f;
            Shader shader = Shader.Find("Sprites/Default");
            if (shader == null) return;
            Texture2D yarn = YarnRopeTexture.Load();
            yarnMaterial = new Material(shader)
            {
                name = "Checkpoint Yarn (Runtime)", hideFlags = HideFlags.HideAndDontSave,
                mainTexture = yarn, color = yarn != null ? Color.white : new Color(1f, .31f, .55f)
            };
            poleMaterial = new Material(shader)
            {
                name = "Checkpoint Wood (Runtime)", hideFlags = HideFlags.HideAndDontSave
            };
            pole = Line("Wooden pin", poleMaterial, .085f, new Color(.45f, .22f, .07f), 3);
            highlight = Line("Pin highlight", poleMaterial, .025f, new Color(.86f, .52f, .22f), 4);
            knot = Line("Yarn tie", yarnMaterial, .13f, Color.white, 5);
            var banner = new GameObject("Knitted pennant");
            banner.transform.SetParent(transform, false);
            banner.hideFlags = HideFlags.DontSave;
            cloth = new Mesh { name = "Checkpoint knitted rows", hideFlags = HideFlags.HideAndDontSave };
            cloth.MarkDynamic();
            var uv = new Vector2[vertices.Length];
            var triangles = new int[Rows * Segments * 6];
            for (int row = 0; row < Rows; row++)
            {
                float width = RowWidth(row);
                for (int column = 0; column <= Segments; column++)
                {
                    int v = (row * (Segments + 1) + column) * 2;
                    float x = column / (float)Segments;
                    uv[v] = new Vector2(x * width / .38f, 0f);
                    uv[v + 1] = new Vector2(x * width / .38f, 1f);
                    if (column == Segments) continue;
                    int t = (row * Segments + column) * 6;
                    triangles[t] = v; triangles[t + 1] = v + 1; triangles[t + 2] = v + 2;
                    triangles[t + 3] = v + 2; triangles[t + 4] = v + 1; triangles[t + 5] = v + 3;
                }
            }
            cloth.vertices = vertices;
            cloth.uv = uv;
            cloth.triangles = triangles;
            banner.AddComponent<MeshFilter>().sharedMesh = cloth;
            var renderer = banner.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = yarnMaterial;
            renderer.sortingOrder = 4;
            UpdateShape();
        }

        private LineRenderer Line(string name, Material material, float width, Color color, int order)
        {
            var go = new GameObject(name) { hideFlags = HideFlags.DontSave };
            go.transform.SetParent(transform, false);
            var line = go.AddComponent<LineRenderer>();
            line.sharedMaterial = material;
            line.useWorldSpace = false;
            line.positionCount = 2;
            line.startWidth = line.endWidth = width;
            line.startColor = line.endColor = color;
            line.numCapVertices = 4;
            line.sortingOrder = order;
            return line;
        }

        private static float RowWidth(int row) => Mathf.Lerp(.88f, .16f, row / (float)(Rows - 1));

        public void Advance(float deltaTime)
        {
            if (deltaTime <= 0f || cloth == null) return;
            elapsed += deltaTime;
            UpdateShape();
        }

        private void UpdateShape()
        {
            float rise = Mathf.SmoothStep(0f, 1f, elapsed / .6f);
            float unfold = Mathf.SmoothStep(0f, 1f, (elapsed - .12f) / .55f);
            pole.SetPosition(0, Vector3.zero);
            pole.SetPosition(1, Vector3.up * (1.42f * rise));
            highlight.SetPosition(0, new Vector3(-.014f, 0f));
            highlight.SetPosition(1, new Vector3(-.014f, 1.42f * rise));
            knot.SetPosition(0, new Vector3(-.07f, 1.30f * rise));
            knot.SetPosition(1, new Vector3(.07f, 1.30f * rise));
            for (int row = 0; row < Rows; row++)
            for (int column = 0; column <= Segments; column++)
            {
                float along = column / (float)Segments;
                float x = along * RowWidth(row) * unfold;
                float flutter = Mathf.Sin(elapsed * 1.8f - along * 3.2f + phase) * .025f * along * unfold;
                float y = (1.28f - row * .067f + flutter) * rise;
                int v = (row * (Segments + 1) + column) * 2;
                vertices[v] = new Vector3(x, y - .05f * rise);
                vertices[v + 1] = new Vector3(x, y + .05f * rise);
            }
            cloth.vertices = vertices;
            cloth.RecalculateBounds();
        }

        public void Release()
        {
            Dispose(cloth); Dispose(yarnMaterial); Dispose(poleMaterial);
            cloth = null; yarnMaterial = null; poleMaterial = null;
        }

        private static void Dispose(Object value)
        {
            if (value == null) return;
            if (Application.isPlaying) Destroy(value);
            else DestroyImmediate(value);
        }

        private void OnDestroy() => Release();
    }
}
