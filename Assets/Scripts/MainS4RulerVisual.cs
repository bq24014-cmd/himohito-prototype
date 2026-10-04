using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HimoHito
{
    /// <summary>Approved wooden ruler presentation for the existing S4 column.</summary>
    [DefaultExecutionOrder(21010), DisallowMultipleComponent]
    public sealed class MainS4RulerVisual : MonoBehaviour
    {
        const string OwnerName = "Main S4 Ruler Presentation";
        const string ChildName = "Main S4 Wooden Ruler Visual";
        readonly Dictionary<Renderer, bool> originalVisibility = new Dictionary<Renderer, bool>();
        GameObject child;
        Mesh mesh;
        Material material;
        GameObject target;
        bool unavailable;

        public bool Ready { get; private set; }
        public MeshRenderer Surface { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() { SceneManager.sceneLoaded -= SceneLoaded; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            SceneManager.sceneLoaded -= SceneLoaded;
            SceneManager.sceneLoaded += SceneLoaded;
            EnsureScene(SceneManager.GetActiveScene());
        }

        static void SceneLoaded(Scene scene, LoadSceneMode mode) => EnsureScene(scene);

        static void EnsureScene(Scene scene)
        {
            if (!Application.isPlaying || scene.name != "MainStage") return;
            foreach (var root in scene.GetRootGameObjects())
                if (root.GetComponent<MainS4RulerVisual>() != null) return;
            var owner = new GameObject(OwnerName);
            SceneManager.MoveGameObjectToScene(owner, scene);
            owner.AddComponent<MainS4RulerVisual>();
        }

        void OnEnable()
        {
            if (gameObject.scene.name != "MainStage") { enabled = false; return; }
            unavailable = false;
        }

        void LateUpdate()
        {
            if (!Ready && !unavailable) TryPrepare();
            if (Ready) ApplyVisibility();
        }

        void TryPrepare()
        {
            foreach (var root in gameObject.scene.GetRootGameObjects())
                if (root.name == MainStageSectionFourSetup.IntermediateColumnName) target = root;
            if (target == null || target.transform.Find("Craft Wood Body") == null) return;
            var t = target.transform;
            var box = target.GetComponent<BoxCollider2D>();
            var body = target.GetComponent<Rigidbody2D>();
            var source = target.GetComponent<SpriteRenderer>();
            if (box == null || body == null || source == null || !box.enabled || box.isTrigger ||
                box.size != Vector2.one || box.offset != Vector2.zero || body.bodyType != RigidbodyType2D.Static ||
                Vector3.Distance(t.position, new Vector3(69.2f, -5.65f, 0)) > .001f ||
                Vector3.Distance(t.lossyScale, new Vector3(1.8f, 10, 1)) > .001f ||
                Quaternion.Angle(t.rotation, Quaternion.identity) > .001f)
            { Fail("Unexpected S4 column contract; original visual retained."); return; }

            var wood = Resources.Load<Texture2D>("Art/MainS4Ruler/Wood");
            var end = Resources.Load<Texture2D>("Art/MainS4Ruler/EndGrain");
            var marks = Resources.Load<Texture2D>("Art/MainS4Ruler/Marks");
            var shader = Resources.Load<Shader>("MainS4Ruler");
            if (!wood || !end || !marks || !shader)
            { Fail("Missing S4 ruler art or shader; original visual retained."); return; }

            child = new GameObject(ChildName);
            child.transform.SetParent(t, false);
            BuildApprovedSurface(box, source, wood, end, marks, shader);
            Ready = true;
            ApplyVisibility();
        }

        void BuildApprovedSurface(BoxCollider2D box, SpriteRenderer source,
            Texture2D wood, Texture2D end, Texture2D marks, Shader shader)
        {
            float sx = Mathf.Abs(target.transform.lossyScale.x);
            float sy = Mathf.Abs(target.transform.lossyScale.y);
            float w = box.size.x * sx, h = box.size.y * sy;
            float[] xs = { 0, .025f, .055f, .09f, w - .20f, w - .13f, w - .07f, w - .025f, w };
            float[] ys = { 0, .035f, .075f, .13f, h - .24f, h - .15f, h - .085f, h - .035f, h };
            var vertices = new List<Vector3>();
            var uv = new List<Vector2>();
            var face = new List<Vector2>();
            var tris = new List<int>();
            for (int row = 0; row < ys.Length; row++)
                for (int col = 0; col < xs.Length; col++)
                {
                    float x = xs[col], y = ys[row], top = h - y;
                    float left = 1 - Mathf.Clamp01(x / .09f);
                    float right = Mathf.Clamp01((x - (w - .20f)) / .20f);
                    float crown = 1 - Mathf.Sqrt(Mathf.Max(0,
                        1 - Mathf.Pow(1 - Mathf.Clamp01(top / .24f), 2)));
                    float heel = 1 - Mathf.Clamp01(y / .13f);
                    // The bevel stays inside the approved, unchanged XY support.
                    float z = -.22f + .05f * left * left + .13f * right * right +
                        .14f * crown + .06f * heel * heel;
                    vertices.Add(new Vector3(box.offset.x - box.size.x / 2 + x / sx,
                        box.offset.y - box.size.y / 2 + y / sy, z));
                    uv.Add(new Vector2(.12f + y * 160 / wood.width, .17f + x * 160 / wood.height));
                    face.Add(new Vector2(x, top));
                    if (row < ys.Length - 1 && col < xs.Length - 1)
                    {
                        int a = row * xs.Length + col, b = a + 1, c = a + xs.Length, d = c + 1;
                        tris.Add(a); tris.Add(c); tris.Add(b);
                        tris.Add(b); tris.Add(c); tris.Add(d);
                    }
                }
            mesh = new Mesh { name = "S4 thin wooden ruler, full unchanged support" };
            mesh.SetVertices(vertices); mesh.SetUVs(0, uv); mesh.SetUVs(1, face);
            mesh.SetTriangles(tris, 0); mesh.RecalculateNormals(); mesh.RecalculateBounds();
            material = new Material(shader)
            { name = "S4 C wood ruler / consistent 0-15 cm marks", mainTexture = wood };
            material.SetTexture("_EndTex", end); material.SetTexture("_MarkTex", marks);
            material.SetFloat("_Width", w); material.SetFloat("_Height", h);
            child.AddComponent<MeshFilter>().sharedMesh = mesh;
            Surface = child.AddComponent<MeshRenderer>();
            Surface.sharedMaterial = material;
            Surface.sortingLayerID = source.sortingLayerID;
            Surface.sortingOrder = source.sortingOrder + 7;
            Surface.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            Surface.receiveShadows = false;
        }

        void ApplyVisibility()
        {
            // Other presentation scripts refresh enabled every frame. Preserve their
            // state and mask only the known, legacy S4 surfaces with forceRenderingOff.
            foreach (var renderer in target.GetComponentsInChildren<Renderer>(true))
            {
                if (renderer == Surface || !IsLegacySurface(renderer)) continue;
                if (!originalVisibility.ContainsKey(renderer))
                    originalVisibility.Add(renderer, renderer.forceRenderingOff);
                renderer.forceRenderingOff = true;
            }
            if (Surface != null) Surface.enabled = isActiveAndEnabled;
        }

        bool IsLegacySurface(Renderer renderer)
        {
            string name = renderer.gameObject.name;
            return renderer.gameObject == target || name == "Craft Wood Body" ||
                name == "Wood Surface Shading" || name == "Orange Block Platform Visual" ||
                name == "Blue Railway Platform Visual" || name == "Terrain Top Edge" ||
                name.StartsWith("Orange Block Platform Tile ");
        }

        void Fail(string message)
        {
            unavailable = true;
            Debug.LogWarning("MainS4RulerVisual: " + message, this);
        }

        void OnDisable() => Release();
        void OnDestroy() => Release();

        void Release()
        {
            foreach (var pair in originalVisibility)
                if (pair.Key != null) pair.Key.forceRenderingOff = pair.Value;
            originalVisibility.Clear();
            if (child != null) Destroy(child);
            if (mesh != null) Destroy(mesh);
            if (material != null) Destroy(material);
            child = null; mesh = null; material = null; target = null; Surface = null;
            Ready = false;
        }
    }
}
