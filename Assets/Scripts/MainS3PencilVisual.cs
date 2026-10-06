using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HimoHito
{
    /// <summary>Approved renderer-only pencil for the existing S3 Return Step A.</summary>
    [DefaultExecutionOrder(21020), DisallowMultipleComponent]
    public sealed class MainS3PencilVisual : MonoBehaviour
    {
        public const string TargetName = MainStageSectionThreeSetup.ReturnStepAName;
        const string OwnerName = "Main S3 Pencil Presentation";
        public const string SurfaceName = "Main S3 Return Step A Pencil Visual";
        public bool Ready { get; private set; }
        public MeshRenderer Surface { get; private set; }
        GameObject target, child;
        Mesh mesh;
        Material material;
        readonly Dictionary<Renderer, bool> originals = new Dictionary<Renderer, bool>();
        bool failed;

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
                if (root.GetComponent<MainS3PencilVisual>() != null) return;
            var owner = new GameObject(OwnerName);
            SceneManager.MoveGameObjectToScene(owner, scene);
            owner.AddComponent<MainS3PencilVisual>();
        }

        void OnEnable()
        {
            if (gameObject.scene.name != "MainStage") { enabled = false; return; }
            failed = false;
        }

        void LateUpdate()
        {
            if (!Ready && !failed) Prepare();
            if (Ready) RefreshVisibility();
        }

        void Prepare()
        {
            if (gameObject.scene.name != "MainStage") return;
            foreach (var root in gameObject.scene.GetRootGameObjects())
                if (root.name == TargetName) target = root;
            if (!target || !target.transform.Find("Craft Wood Body")) return;
            var t = target.transform;
            var box = target.GetComponent<BoxCollider2D>();
            var body = target.GetComponent<Rigidbody2D>();
            var source = target.GetComponent<SpriteRenderer>();
            if (!box || !body || !source || !box.enabled || box.isTrigger ||
                box.size != Vector2.one || box.offset != Vector2.zero ||
                body.bodyType != RigidbodyType2D.Static ||
                Vector3.Distance(t.position, new Vector3(42.8f, -11.05f, 0)) > .001f ||
                Vector3.Distance(t.lossyScale, new Vector3(1.8f, 10, 1)) > .001f ||
                Quaternion.Angle(t.rotation, Quaternion.identity) > .001f)
            { Fail("Unexpected Step A support; original retained."); return; }
            var wood = Resources.Load<Texture2D>("Art/MainS4Ruler/Wood");
            var end = Resources.Load<Texture2D>("Art/MainS4Ruler/EndGrain");
            var shader = Resources.Load<Shader>("MainS3Pencil");
            if (!wood || !end || !shader)
            { Fail("Missing S3 pencil materials; original retained."); return; }

            child = new GameObject(SurfaceName);
            child.transform.SetParent(t, false);
            float w = 1.8f, h = 10f;
            // The full, horizontal walking rim remains exactly on the collider.
            // Facets change only depth: this is not a stretched box or painted log.
            float[] xs = { 0, .035f, .12f, .28f, 1.47f, 1.68f, 1.765f, w };
            float[] ys = { 0, .045f, .11f, .25f, h - .35f, h - .23f, h - .12f, h - .04f, h };
            var vertices = new List<Vector3>();
            var uvs = new List<Vector2>();
            var faces = new List<Vector2>();
            var triangles = new List<int>();
            for (int r = 0; r < ys.Length; r++)
                for (int c = 0; c < xs.Length; c++)
                {
                    float x = xs[c], y = ys[r], top = h - y;
                    float left = 1 - Mathf.Clamp01(x / .28f);
                    float right = Mathf.Clamp01((x - 1.47f) / .33f);
                    float cap = 1 - Mathf.Clamp01(top / .35f);
                    float heel = 1 - Mathf.Clamp01(y / .25f);
                    float z = -.24f + .075f * left + .115f * right + .15f * cap + .055f * heel;
                    vertices.Add(new Vector3(-.5f + x / w, -.5f + y / h, z));
                    uvs.Add(new Vector2(.08f + y * 100f / wood.width, .12f + x * 100f / wood.height));
                    faces.Add(new Vector2(x, top));
                    if (r < ys.Length - 1 && c < xs.Length - 1)
                    {
                        int a = r * xs.Length + c, b = a + 1, d = a + xs.Length, e = d + 1;
                        triangles.Add(a); triangles.Add(d); triangles.Add(b);
                        triangles.Add(b); triangles.Add(d); triangles.Add(e);
                    }
                }
            mesh = new Mesh { name = "Jumbo hexagonal unsharpened colour pencil / fixed support" };
            mesh.SetVertices(vertices); mesh.SetUVs(0, uvs); mesh.SetUVs(1, faces);
            mesh.SetTriangles(triangles, 0); mesh.RecalculateNormals(); mesh.RecalculateBounds();
            material = new Material(shader) { name = "Desaturated blue pencil / exposed end grain", mainTexture = wood };
            material.SetTexture("_EndTex", end);
            material.SetColor("_Color", Color.white); // Existing Reveal owns this fade.
            child.AddComponent<MeshFilter>().sharedMesh = mesh;
            Surface = child.AddComponent<MeshRenderer>();
            Surface.sharedMaterial = material;
            Surface.sortingLayerID = source.sortingLayerID;
            Surface.sortingOrder = source.sortingOrder + 7;
            Surface.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            Surface.receiveShadows = false;
            Ready = true;
            RefreshVisibility();
        }

        void RefreshVisibility()
        {
            foreach (var renderer in target.GetComponentsInChildren<Renderer>(true))
            {
                if (renderer == Surface || !Legacy(renderer)) continue;
                if (!originals.ContainsKey(renderer)) originals.Add(renderer, renderer.forceRenderingOff);
                // Mask only Step A's legacy art. The existing stairs own the new
                // renderer's rise/fade, forceRenderingOff, property block and pose.
                renderer.forceRenderingOff = true;
            }
            Surface.enabled = isActiveAndEnabled;
        }

        bool Legacy(Renderer r)
        {
            string n = r.gameObject.name;
            return r.gameObject == target || n == "Craft Wood Body" || n == "Wood Surface Shading" ||
                n == "Orange Block Platform Visual" || n == "Blue Railway Platform Visual" ||
                n == "Terrain Top Edge" || n.StartsWith("Orange Block Platform Tile ");
        }

        void Fail(string message) { failed = true; Debug.LogWarning("MainS3PencilVisual: " + message, this); }
        void OnDisable() => Release();
        void OnDestroy() => Release();
        void Release()
        {
            foreach (var pair in originals) if (pair.Key) pair.Key.forceRenderingOff = pair.Value;
            originals.Clear();
            if (child) Destroy(child);
            if (mesh) Destroy(mesh);
            if (material) Destroy(material);
            child = null; mesh = null; material = null; target = null; Surface = null; Ready = false;
        }
    }
}
