using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HimoHito
{
    /// <summary>
    /// Tutorial-only terrain presentation. Owns renderer-only children, never
    /// the floor Transform, Collider, Rigidbody, gameplay or background.
    /// The explicit allowlist intentionally excludes beams and all MainStage floors.
    /// </summary>
    [DefaultExecutionOrder(20000), DisallowMultipleComponent]
    public sealed class TutorialTerrainHybridVisual : MonoBehaviour
    {
        const string ResourceRoot = "Art/TutorialTerrainHybrid/";
        const string ChildName = "Tutorial Hybrid Terrain Visual";

        sealed class Floor
        {
            public readonly string name, resource;
            public readonly float centerX, width;
            public readonly RectInt crop;
            public readonly Vector2Int textureSize;
            public GameObject target, child;
            public BoxCollider2D collider;
            public Texture2D texture;
            public MeshRenderer display;
            public Mesh mesh;
            public Material material;
            public readonly Dictionary<Renderer, bool> originalVisibility = new Dictionary<Renderer, bool>();
            public Floor(string name, string resource, float x, float width, int tw, int th, RectInt crop)
            { this.name=name; this.resource=resource; centerX=x; this.width=width; textureSize=new Vector2Int(tw,th); this.crop=crop; }
        }
        readonly List<Floor> floors = new List<Floor>
        {
            new Floor("Start Ground", "Start", -4, 6, 971,1619, new RectInt(72,64,827,1401)),
            new Floor("Tutorial Landing", "Landing1", 12, 6, 971,1619, new RectInt(62,92,848,1386)),
            new Floor("Tutorial T2 Landing", "BridgeBank", 27, 6, 972,1619, new RectInt(61,113,852,1355)),
            new Floor("Tutorial T3 Landing", "MergeBank", 39, 6, 971,1619, new RectInt(58,83,854,1375)),
            new Floor("Tutorial T4 Goal Floor", "Goal", 59.5f, 15, 1536,1024, new RectInt(47,58,1442,881))
        };
        bool ready;
        bool unavailable;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            SceneManager.sceneLoaded -= SceneLoaded;
        }
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
            if (!Application.isPlaying || scene.name != "Tutorial") return;
            foreach (var root in scene.GetRootGameObjects())
                if (root.GetComponent<TutorialTerrainHybridVisual>() != null) return;
            var owner = new GameObject("Tutorial Hybrid Terrain Presentation");
            SceneManager.MoveGameObjectToScene(owner, scene);
            owner.AddComponent<TutorialTerrainHybridVisual>();
        }
        void Awake()
        {
            if (gameObject.scene.name != "Tutorial") { enabled=false; return; }
        }
        void LateUpdate()
        {
            if (!ready && !unavailable) TryPrepare();
            if (ready) ApplyVisibility();
        }
        void TryPrepare()
        {
            if (gameObject.scene.name != "Tutorial") return;
            var roots = gameObject.scene.GetRootGameObjects();
            // Wait for existing Tutorial setup/art; never create or repair terrain.
            foreach (var f in floors)
            {
                foreach (var root in roots) if (root.name == f.name) { f.target=root; break; }
                if (f.target == null || f.target.transform.Find("Craft Wood Body") == null) return;
                if (!ValidFloor(f)) { Fail("Unexpected floor contract: " + f.name); return; }
            }
            Shader shader = Shader.Find("Sprites/Default");
            if (shader == null) { Fail("Missing Sprites/Default"); return; }
            // Validate every resource before hiding any original renderer.
            foreach (var f in floors)
            {
                f.texture = Resources.Load<Texture2D>(ResourceRoot + f.resource);
                if (f.texture == null || f.texture.width != f.textureSize.x || f.texture.height != f.textureSize.y)
                { Fail("Missing/changed terrain art: " + f.resource); return; }
            }
            foreach (var f in floors) CreateVisual(f, shader);
            ready = true;
            ApplyVisibility();
        }
        bool ValidFloor(Floor f)
        {
            var t=f.target.transform;
            return f.target.scene == gameObject.scene && f.target.TryGetComponent(out f.collider) &&
                f.target.TryGetComponent(out SpriteRenderer _) && f.target.TryGetComponent(out SolidSwingSurface _) &&
                f.target.TryGetComponent(out Rigidbody2D body) && body.bodyType == RigidbodyType2D.Static &&
                f.collider.enabled && !f.collider.isTrigger && f.collider.size == Vector2.one && f.collider.offset == Vector2.zero &&
                Vector3.Distance(t.position,new Vector3(f.centerX,-4.65f,0)) < .001f &&
                Vector3.Distance(t.lossyScale,new Vector3(f.width,10,1)) < .001f && Quaternion.Angle(t.rotation,Quaternion.identity) < .001f;
        }
        void Fail(string reason)
        {
            unavailable=true;
            Debug.LogWarning("Tutorial Hybrid terrain retains existing wood. " + reason, this);
        }
        void CreateVisual(Floor f, Shader shader)
        {
            f.child=new GameObject(ChildName) { layer=f.target.layer };
            f.child.transform.SetParent(f.target.transform,false);
            f.display=f.child.AddComponent<MeshRenderer>();
            f.display.enabled=false;
            f.display.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
            f.display.receiveShadows=false;
            var source=f.target.GetComponent<SpriteRenderer>();
            f.display.sortingLayerID=source.sortingLayerID;
            f.display.sortingOrder=source.sortingOrder+3;
            f.mesh=new Mesh { name="Tutorial terrain " + f.resource };
            f.child.AddComponent<MeshFilter>().sharedMesh=f.mesh;
            float l=f.collider.offset.x-f.collider.size.x*.5f,r=l+f.collider.size.x;
            float b=f.collider.offset.y-f.collider.size.y*.5f,t=b+f.collider.size.y;
            f.mesh.vertices=new[] { new Vector3(l,b),new Vector3(r,b),new Vector3(r,t),new Vector3(l,t) };
            float u0=(f.crop.x+.5f)/f.texture.width,u1=(f.crop.xMax-.5f)/f.texture.width;
            float v0=(f.crop.y+.5f)/f.texture.height,v1=(f.crop.yMax-.5f)/f.texture.height;
            f.mesh.uv=new[] { new Vector2(u0,v0),new Vector2(u1,v0),new Vector2(u1,v1),new Vector2(u0,v1) };
            f.mesh.colors=new[] { Color.white,Color.white,Color.white,Color.white };
            f.mesh.triangles=new[] { 0,2,1,0,3,2 }; f.mesh.RecalculateBounds();
            f.material=new Material(shader) { name="Tutorial terrain " + f.resource,mainTexture=f.texture };
            f.display.sharedMaterial=f.material;
            foreach (var renderer in f.target.GetComponentsInChildren<Renderer>(true))
            {
                string n=renderer.gameObject.name;
                bool owned=renderer.gameObject==f.target || n=="Craft Wood Body" || n=="Wood Surface Shading" ||
                    n=="Orange Block Platform Visual" || n=="Terrain Top Edge" || n.StartsWith("Orange Block Platform Tile ",StringComparison.Ordinal);
                if (owned) f.originalVisibility.Add(renderer,renderer.forceRenderingOff);
            }
        }
        void ApplyVisibility()
        {
            bool show=isActiveAndEnabled;
            foreach(var f in floors)
            {
                foreach(var entry in f.originalVisibility) if(entry.Key!=null) entry.Key.forceRenderingOff=show || entry.Value;
                if(f.display!=null) f.display.enabled=show;
            }
        }
        void OnEnable()
        {
            if (gameObject.scene.name != "Tutorial") { enabled=false; return; }
            unavailable=false;
            if(ready) ApplyVisibility();
        }
        void OnDisable()
        {
            ReleaseVisuals();
        }
        void OnDestroy()
        {
            ReleaseVisuals();
        }
        void ReleaseVisuals()
        {
            // Runtime references are not serialized across an assembly reload.
            // OnDisable releases ownership before that reload; re-enable builds
            // fresh children in LateUpdate. Only our children/meshes/materials
            // are ever destroyed.
            ready=false;
            foreach(var f in floors)
            {
                foreach(var entry in f.originalVisibility) if(entry.Key!=null) entry.Key.forceRenderingOff=entry.Value;
                if(f.display!=null) f.display.enabled=false;
                ReleaseOwned(f.child);
                ReleaseOwned(f.mesh);
                ReleaseOwned(f.material);
                f.originalVisibility.Clear();
                f.target=null; f.child=null; f.collider=null; f.texture=null;
                f.display=null; f.mesh=null; f.material=null;
            }
        }
        static void ReleaseOwned(UnityEngine.Object owned)
        {
            if(owned==null) return;
            if(Application.isPlaying) Destroy(owned);
            else DestroyImmediate(owned);
        }
    }
}
