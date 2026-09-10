using System.Collections.Generic;
using UnityEngine;

namespace HimoHito
{
    /// <summary>Thin display-only bevels and shading over the existing wood artwork.</summary>
    [ExecuteAlways, DisallowMultipleComponent]
    public sealed class WoodenPlatformDepthVisual : MonoBehaviour
    {
        private const string ChildName = "Wood Surface Shading";
        private Material sharedMaterial;
        private Material grainMaterial;
        private Texture2D grainTexture;
        private Texture2D lastGrainTexture;
        private bool lastHadGrain;
        private Mesh mesh;
        private MeshRenderer display;
        private BoxCollider2D floor;
        private SpriteRenderer source;
        private Vector2 lastSize, lastOffset;
        private Vector3 lastScale;
        private int lastOrder, lastLayer;
        private Vector3[] restingVertices, landingVertices;
        private float landingElapsed = 1f, landingStrength;
        private static readonly RaycastHit2D[] LandingHits = new RaycastHit2D[12];
        public const float SurfaceInset = 0.045f;

        public static bool Ensure(GameObject target)
        {
            if (target == null || !target.TryGetComponent(out BoxCollider2D _) ||
                !target.TryGetComponent(out SpriteRenderer _)) return false;
            bool added = !target.TryGetComponent(out WoodenPlatformDepthVisual visual);
            if (added) visual = target.AddComponent<WoodenPlatformDepthVisual>();
            // Explicit refresh is also the asset-reimport/fallback recovery path.
            visual.grainTexture = CraftWoodPlatformVisual.TryLoadTextures(out _, out Texture2D loadedGrain)
                ? loadedGrain : null;
            return visual.Refresh() || added;
        }

        private void OnEnable() => Refresh();
        private void LateUpdate()
        {
            Refresh();
            if (!Application.isPlaying || Time.deltaTime <= 0f) return;
            AdvanceLanding(Time.deltaTime);
        }
        private void OnDisable()
        {
            landingElapsed = 1f;
            if (mesh != null && restingVertices != null) mesh.vertices = restingVertices;
            if (display != null) display.enabled = false;
            ReleaseResources();
        }

        public static void NotifyLanding(Collider2D player, float speed)
        {
            if (player == null || speed < 1.25f) return;
            var filter = new ContactFilter2D();
            filter.SetLayerMask(Physics2D.GetLayerCollisionMask(player.gameObject.layer));
            filter.useTriggers = false;
            Bounds b = player.bounds;
            int count = Physics2D.Raycast(new Vector2(b.center.x, b.min.y + .12f),
                Vector2.down, filter, LandingHits, .3f);
            if (count == LandingHits.Length) return;
            for (int i = 0; i < count; i++)
            {
                var hit = LandingHits[i];
                if (hit.collider == null || hit.collider == player || hit.collider.attachedRigidbody == player.attachedRigidbody ||
                    hit.fraction <= 0f || hit.normal.y < .9f || Physics2D.GetIgnoreCollision(player, hit.collider)) continue;
                if (hit.collider.TryGetComponent(out WoodenPlatformDepthVisual visual) && visual.isActiveAndEnabled)
                    visual.PlayLanding(speed);
                return; // A rope/rail above a wooden floor occludes it.
            }
        }

        public void PlayLanding(float speed)
        {
            if (floor == null || !floor.enabled || floor.isTrigger || mesh == null ||
                Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.z, 0f)) > .1f) return;
            landingElapsed = 0f;
            landingStrength = Mathf.Lerp(.025f, .055f, Mathf.InverseLerp(2f, 14f, speed));
        }

        private void AdvanceLanding(float dt)
        {
            if (dt <= 0f || landingElapsed >= .28f || restingVertices == null || mesh == null) return;
            landingElapsed = Mathf.Min(.28f, landingElapsed + dt);
            float dip = Mathf.Sin(Mathf.PI * landingElapsed / .28f) * landingStrength;
            float sy = Mathf.Max(.001f, Mathf.Abs(transform.lossyScale.y));
            float top = floor.offset.y + floor.size.y * .5f;
            for (int i = 0; i < restingVertices.Length; i++)
            {
                Vector3 p = restingVertices[i];
                float weight = Mathf.Clamp01(1f - (top - p.y) * sy / .65f);
                p.y -= dip * weight / sy;
                landingVertices[i] = p;
            }
            // Only the decorative upper surface deforms; all colliders and roots stay fixed.
            mesh.vertices = landingVertices;
            mesh.RecalculateBounds();
        }

        public bool Refresh()
        {
            if (floor == null) floor = GetComponent<BoxCollider2D>();
            if (source == null) source = GetComponent<SpriteRenderer>();
            if (floor == null || source == null) return false;
            bool visible = enabled && floor.enabled && !floor.isTrigger;
            if (display != null) display.enabled = visible;
            if (!visible) return false;
            Vector3 scale = transform.lossyScale;
            if (grainTexture == null)
                grainTexture = CraftWoodPlatformVisual.TryLoadTextures(out _, out Texture2D loadedGrain)
                    ? loadedGrain : null;
            if (mesh != null && display != null && lastSize == floor.size &&
                lastOffset == floor.offset && lastScale == scale &&
                lastOrder == source.sortingOrder && lastLayer == source.sortingLayerID &&
                lastHadGrain == (grainTexture != null) && lastGrainTexture == grainTexture) return false;

            if (display == null)
            {
                Transform child = transform.Find(ChildName);
                if (child == null)
                {
                    child = new GameObject(ChildName).transform;
                    child.SetParent(transform, false);
                }
                child.gameObject.hideFlags = HideFlags.DontSave;
                child.gameObject.layer = gameObject.layer;
                child.localPosition = Vector3.zero;
                child.localRotation = Quaternion.identity;
                child.localScale = Vector3.one;
                if (!child.TryGetComponent(out display)) display = child.gameObject.AddComponent<MeshRenderer>();
                if (!child.TryGetComponent(out MeshFilter _)) child.gameObject.AddComponent<MeshFilter>();
                display.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                display.receiveShadows = false;
            }
            if (sharedMaterial == null)
            {
                Shader shader = Shader.Find("Sprites/Default");
                if (shader == null) return false;
                sharedMaterial = new Material(shader) { name = "Wood Shading Material", hideFlags = HideFlags.HideAndDontSave };
            }
            if (grainTexture != null)
            {
                if (grainMaterial == null) grainMaterial = new Material(sharedMaterial.shader)
                    { name = "Craft Wood Top Material", hideFlags = HideFlags.HideAndDontSave };
                grainMaterial.mainTexture = grainTexture;
                display.sharedMaterials = new[] { sharedMaterial, grainMaterial };
            }
            else display.sharedMaterials = new[] { sharedMaterial };
            display.sortingLayerID = source.sortingLayerID;
            display.sortingOrder = source.sortingOrder + 2;
            display.enabled = visible;
            if (mesh == null) mesh = new Mesh { name = "Wood Bevel Mesh", hideFlags = HideFlags.HideAndDontSave };
            BuildMesh(scale);
            display.GetComponent<MeshFilter>().sharedMesh = mesh;
            lastSize = floor.size; lastOffset = floor.offset; lastScale = scale;
            lastOrder = source.sortingOrder; lastLayer = source.sortingLayerID;
            lastGrainTexture = grainTexture; lastHadGrain = grainTexture != null;
            return true;
        }

        private void BuildMesh(Vector3 scale)
        {
            float sx = Mathf.Max(.001f, Mathf.Abs(scale.x)), sy = Mathf.Max(.001f, Mathf.Abs(scale.y));
            float left = floor.offset.x - floor.size.x * .5f, right = left + floor.size.x;
            float bottom = floor.offset.y - floor.size.y * .5f, top = bottom + floor.size.y;
            float depth = Mathf.Min(.25f / sy, floor.size.y * .22f);
            float back = top - Mathf.Min(SurfaceInset / sy, depth * .2f), front = back - depth;
            float edge = Mathf.Min(.12f / sx, floor.size.x * .08f);
            var vertices = new List<Vector3>(); var colors = new List<Color>();
            var uv = new List<Vector2>();
            var triangles = new List<int>(); var topTriangles = new List<int>();
            float grainWorldHeight = grainTexture != null ?
                CraftWoodPlatformVisual.TextureWorldWidth * grainTexture.height / Mathf.Max(1f, grainTexture.width) : 1f;
            void Quad(float x0, float y0, float x1, float y1, Color lower, Color upper, bool textured = false)
            {
                int first = vertices.Count;
                vertices.Add(new Vector3(x0,y0)); vertices.Add(new Vector3(x1,y0));
                vertices.Add(new Vector3(x1,y1)); vertices.Add(new Vector3(x0,y1));
                colors.Add(lower); colors.Add(lower); colors.Add(upper); colors.Add(upper);
                for (int i = first; i < first + 4; i++)
                    uv.Add(new Vector2((vertices[i].x - left) * sx / CraftWoodPlatformVisual.TextureWorldWidth,
                        (vertices[i].y - back) * sy / Mathf.Max(.01f, grainWorldHeight)));
                List<int> indices = textured && grainTexture != null ? topTriangles : triangles;
                indices.Add(first); indices.Add(first+2); indices.Add(first+1);
                indices.Add(first); indices.Add(first+3); indices.Add(first+2);
            }
            // Transparent face shading preserves the original grain, knots and seams.
            Quad(left,bottom,right,front, new Color(.12f,.055f,.02f,.48f), new Color(.12f,.055f,.02f,0f));
            Quad(right-edge,bottom,right,front, new Color(.14f,.065f,.025f,.45f), new Color(.14f,.065f,.025f,.26f));
            Quad(left,bottom,left+edge*.4f,front, new Color(1f,.70f,.33f,.08f), new Color(1f,.70f,.33f,.22f));
            // A shallow horizontal top, bounded by the existing collider silhouette.
            Quad(left,front,right,back,
                grainTexture != null ? new Color(.74f,.65f,.49f,1f) : new Color(.77f,.39f,.13f,1f),
                grainTexture != null ? new Color(1f,.96f,.83f,1f) : new Color(1f,.77f,.39f,1f), true);
            Quad(left,front-.045f/sy,right,front, new Color(.29f,.13f,.04f,.68f), new Color(.55f,.25f,.055f,.75f));
            Quad(left,back-.025f/sy,right,back, new Color(1f,.86f,.57f,.30f), new Color(1f,.89f,.62f,.82f));
            // Subtle lengthwise wood streaks on the new top, not a flat orange bar.
            for (int row=0; grainTexture == null && row<3;row++)
            {
                float y = front + depth * (.2f + row * .23f);
                float inset = floor.size.x * (.025f + row*.035f);
                Quad(left+inset,y,right-inset*.7f,y+.012f/sy,
                    new Color(.44f,.22f,.055f,.24f),new Color(.44f,.22f,.055f,.24f));
            }
            float underside = Mathf.Min(.4f/sy, floor.size.y*.12f);
            Quad(left,bottom,right,bottom+underside, new Color(.10f,.04f,.018f,.45f),new Color(.10f,.04f,.018f,0f));
            mesh.Clear(); mesh.SetVertices(vertices); mesh.SetColors(colors); mesh.SetUVs(0, uv);
            mesh.subMeshCount = grainTexture != null ? 2 : 1;
            mesh.SetTriangles(triangles,0);
            if (grainTexture != null) mesh.SetTriangles(topTriangles,1);
            mesh.RecalculateBounds();
            restingVertices = mesh.vertices;
            landingVertices = new Vector3[restingVertices.Length];
            landingElapsed = 1f;
        }

        private void OnDestroy()
        {
            ReleaseResources();
            if (display != null) { if (Application.isPlaying) Destroy(display.gameObject); else DestroyImmediate(display.gameObject); }
        }

        private void ReleaseResources()
        {
            if (display != null)
            {
                display.sharedMaterials = System.Array.Empty<Material>();
                if (display.TryGetComponent(out MeshFilter filter)) filter.sharedMesh = null;
            }
            DestroyOwned(mesh); DestroyOwned(sharedMaterial); DestroyOwned(grainMaterial);
            mesh = null; sharedMaterial = null; grainMaterial = null; grainTexture = null;
            lastGrainTexture = null; lastHadGrain = false;
            restingVertices = landingVertices = null;
        }

        private static void DestroyOwned(Object owned)
        {
            if (owned == null) return;
            if (Application.isPlaying) Destroy(owned);
            else DestroyImmediate(owned);
        }
    }
}
