using UnityEngine;

namespace HimoHito
{
    /// <summary>Bounded, reusable world-space motes behind gameplay, without camera motion.</summary>
    [DisallowMultipleComponent, DefaultExecutionOrder(100)]
    public sealed class AmbientDustVisual : MonoBehaviour
    {
        private const int Count = 32;
        private readonly SpriteRenderer[] motes = new SpriteRenderer[Count];
        private readonly Vector2[] positions = new Vector2[Count];
        private readonly float[] age = new float[Count];
        private Camera view;
        private GameObject root;
        private Texture2D texture;
        private Sprite sprite;
        private Vector2 lastCenter;
        private bool placed;

        private void Awake()
        {
            view = GetComponent<Camera>();
            if (view == null) { enabled = false; return; }
            texture = new Texture2D(16, 16, TextureFormat.RGBA32, false)
                { name = "Soft Moonlit Dust", hideFlags = HideFlags.HideAndDontSave, wrapMode = TextureWrapMode.Clamp };
            var pixels = new Color[256];
            for (int y = 0; y < 16; y++) for (int x = 0; x < 16; x++)
            {
                float r = new Vector2((x - 7.5f) / 8f, (y - 7.5f) / 8f).sqrMagnitude;
                pixels[y * 16 + x] = new Color(1f, 1f, 1f, Mathf.Pow(Mathf.Clamp01(1f - r), 2f));
            }
            texture.SetPixels(pixels); texture.Apply(false, true);
            sprite = Sprite.Create(texture, new Rect(0, 0, 16, 16), new Vector2(.5f, .5f), 16f);
            sprite.hideFlags = HideFlags.HideAndDontSave;
            root = new GameObject("Moonlit Room Dust") { hideFlags = HideFlags.DontSave };
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(root, gameObject.scene);
            for (int i = 0; i < Count; i++)
            {
                var mote = new GameObject("Dust " + i);
                mote.transform.SetParent(root.transform, false);
                var renderer = mote.AddComponent<SpriteRenderer>();
                renderer.sprite = sprite; renderer.sortingOrder = -90;
                renderer.enabled = false;
                mote.transform.localScale = Vector3.one * Mathf.Lerp(.035f, .085f, Seed(i, 3));
                motes[i] = renderer;
            }
        }

        private static float Seed(int i, int salt) => Mathf.Repeat((i + 1) * (.618034f + salt * .137f), 1f);

        private void LateUpdate()
        {
            Advance(Time.deltaTime);
        }

        private void Advance(float deltaTime)
        {
            if (view == null || root == null) return;
            root.SetActive(view.enabled);
            if (!view.enabled || deltaTime <= 0f) return;
            Vector2 center = view.transform.position;
            float height = Mathf.Max(1f, view.orthographicSize);
            Vector2 extent = new Vector2(height * Mathf.Max(.5f, view.aspect), height) + Vector2.one;
            bool reset = !placed || Vector2.Distance(center, lastCenter) > 3f;
            for (int i = 0; i < Count; i++)
            {
                Vector2 p = positions[i];
                if (reset)
                {
                    p = center + new Vector2((Seed(i, 1) * 2f - 1f) * extent.x,
                        (Seed(i, 2) * 2f - 1f) * extent.y);
                    age[i] = 0f;
                }
                p += new Vector2(.018f + Seed(i, 4) * .025f, .025f + Seed(i, 5) * .04f) * deltaTime;
                if (Mathf.Abs(p.x - center.x) > extent.x || Mathf.Abs(p.y - center.y) > extent.y)
                {
                    p = center + new Vector2(Mathf.Repeat(p.x - center.x + extent.x, extent.x * 2f) - extent.x,
                        Mathf.Repeat(p.y - center.y + extent.y, extent.y * 2f) - extent.y);
                    age[i] = 0f;
                }
                age[i] += deltaTime;
                float border = Mathf.Clamp01(Mathf.Min(extent.x - Mathf.Abs(p.x - center.x),
                    extent.y - Mathf.Abs(p.y - center.y)) / 1.5f);
                float alpha = (.16f + Seed(i, 6) * .12f) * Mathf.SmoothStep(0f, 1f, age[i] / 1.4f) * border;
                motes[i].color = new Color(.82f, .86f, 1f, alpha);
                motes[i].transform.position = new Vector3(p.x, p.y, .5f);
                motes[i].enabled = true;
                positions[i] = p;
            }
            placed = true; lastCenter = center;
        }

        private void OnDisable() { if (root != null) root.SetActive(false); }
        private void OnDestroy()
        {
            Dispose(root); Dispose(sprite); Dispose(texture);
        }
        private static void Dispose(Object item)
        {
            if (item == null) return;
            if (Application.isPlaying) Destroy(item); else DestroyImmediate(item);
        }
    }
}
