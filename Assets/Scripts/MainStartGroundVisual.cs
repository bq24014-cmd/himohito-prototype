using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HimoHito
{
    /// <summary>Renderer-only craft stack for the existing MainStage starting floor.</summary>
    [DefaultExecutionOrder(21000), DisallowMultipleComponent]
    public sealed class MainStartGroundVisual : MonoBehaviour
    {
        const string ChildName = "Main Start Ground Craft Stack Visual";
        readonly Dictionary<Renderer, bool> originalVisibility = new Dictionary<Renderer, bool>();
        GameObject child;
        Mesh mesh;
        Material material;
        MeshRenderer display;
        bool ready, unavailable;

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
                if (root.GetComponent<MainStartGroundVisual>() != null) return;
            var owner = new GameObject("Main Start Ground Craft Presentation");
            SceneManager.MoveGameObjectToScene(owner, scene);
            owner.AddComponent<MainStartGroundVisual>();
        }

        void OnEnable()
        {
            if (gameObject.scene.name != "MainStage") { enabled = false; return; }
            unavailable = false;
        }

        void LateUpdate()
        {
            if (!ready && !unavailable) TryPrepare();
            if (ready) ApplyVisibility();
        }

        void TryPrepare()
        {
            GameObject target = null;
            foreach (var root in gameObject.scene.GetRootGameObjects())
                if (root.name == "Main Start Ground") target = root;
            if (target == null || target.transform.Find("Craft Wood Body") == null) return;
            var t = target.transform;
            var box = target.GetComponent<BoxCollider2D>();
            var body = target.GetComponent<Rigidbody2D>();
            var source = target.GetComponent<SpriteRenderer>();
            if (box == null || body == null || source == null || !box.enabled || box.isTrigger ||
                box.size != Vector2.one || box.offset != Vector2.zero || body.bodyType != RigidbodyType2D.Static ||
                Vector3.Distance(t.position, new Vector3(-7.25f, -9.85f, 0)) > .001f ||
                Vector3.Distance(t.lossyScale, new Vector3(8, 10, 1)) > .001f ||
                Quaternion.Angle(t.rotation, Quaternion.identity) > .001f)
            { Fail("Unexpected starting floor contract; original visual retained."); return; }

            var texture = Resources.Load<Texture2D>("Art/MainStartGround/Start");
            var shader = Shader.Find("Sprites/Default");
            if (texture == null || shader == null || !texture.isReadable)
            { Fail("Missing readable craft stack art or shader; original visual retained."); return; }

            // Use the approved rectangular crop. No upper-band UV deformation.
            var pixels = texture.GetPixels32();
            int bottom = texture.height, top = -1, left = 0, right = texture.width - 1;
            var rows = new int[texture.height];
            for (int y = 0; y < texture.height; y++)
                for (int x = 0; x < texture.width; x++)
                    if (pixels[y * texture.width + x].a > 128) rows[y]++;
            for (int y = 0; y < texture.height; y++)
                if (rows[y] >= texture.width * .02f) bottom = Math.Min(bottom, y);
            for (int y = texture.height - 1; y >= bottom; y--)
            {
                if (rows[y] < texture.width * .90f) continue;
                left = 0; right = texture.width - 1;
                while (left < texture.width && pixels[y * texture.width + left].a <= 128) left++;
                while (right > left && pixels[y * texture.width + right].a <= 128) right--;
                bool solid = true;
                for (int x = left + 1; x < right; x++)
                    if (pixels[y * texture.width + x].a <= 128) { solid = false; break; }
                if (solid) { top = y; break; }
            }
            if (top < bottom) { Fail("Missing solid walking edge; original visual retained."); return; }

            child = new GameObject(ChildName) { layer = target.layer };
            child.transform.SetParent(t, false);
            mesh = new Mesh { name = "Main starting floor craft rectangle" };
            mesh.vertices = new[] { new Vector3(-.5f, -.5f), new Vector3(.5f, -.5f), new Vector3(.5f, .5f), new Vector3(-.5f, .5f) };
            float u0 = (left + .5f) / texture.width, u1 = (right + .5f) / texture.width;
            float v0 = (bottom + .5f) / texture.height, v1 = (top + .5f) / texture.height;
            mesh.uv = new[] { new Vector2(u0, v0), new Vector2(u1, v0), new Vector2(u1, v1), new Vector2(u0, v1) };
            mesh.colors = new[] { Color.white, Color.white, Color.white, Color.white };
            mesh.triangles = new[] { 0, 2, 1, 0, 3, 2 };
            mesh.RecalculateBounds();
            child.AddComponent<MeshFilter>().sharedMesh = mesh;
            display = child.AddComponent<MeshRenderer>();
            display.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            display.receiveShadows = false;
            display.sortingLayerID = source.sortingLayerID;
            display.sortingOrder = source.sortingOrder + 3;
            material = new Material(shader) { name = "Main starting floor craft art", mainTexture = texture };
            display.sharedMaterial = material;
            foreach (var renderer in target.GetComponentsInChildren<Renderer>(true))
            {
                string name = renderer.gameObject.name;
                if (renderer.gameObject == target || name == "Craft Wood Body" || name == "Wood Surface Shading" ||
                    name == "Orange Block Platform Visual" || name == "Terrain Top Edge" ||
                    name.StartsWith("Orange Block Platform Tile ", StringComparison.Ordinal))
                    originalVisibility.Add(renderer, renderer.forceRenderingOff);
            }
            ready = true;
            ApplyVisibility();
        }

        void ApplyVisibility()
        {
            foreach (var entry in originalVisibility)
                if (entry.Key != null) entry.Key.forceRenderingOff = isActiveAndEnabled || entry.Value;
            if (display != null) display.enabled = isActiveAndEnabled;
        }

        void Fail(string message) { unavailable = true; Debug.LogError(message, this); }
        void OnDisable() { ReleaseVisuals(); }
        void OnDestroy() { ReleaseVisuals(); }
        void ReleaseVisuals()
        {
            ready = false;
            foreach (var entry in originalVisibility)
                if (entry.Key != null) entry.Key.forceRenderingOff = entry.Value;
            if (display != null) display.enabled = false;
            ReleaseOwned(child); ReleaseOwned(mesh); ReleaseOwned(material);
            originalVisibility.Clear(); child = null; mesh = null; material = null; display = null;
        }
        static void ReleaseOwned(UnityEngine.Object owned)
        {
            if (owned == null) return;
            if (Application.isPlaying) Destroy(owned); else DestroyImmediate(owned);
        }
    }
}
