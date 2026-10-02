using UnityEngine;

namespace HimoHito
{
    /// <summary>
    /// Approved Tutorial Far/Mid/Near presentation, owned by the existing
    /// craft-room background. Only background renderers move; camera and
    /// gameplay objects are read-only. No trial bootstrap or input handling.
    /// </summary>
    [DefaultExecutionOrder(1000), DisallowMultipleComponent]
    public sealed class TutorialCraftRoomLayers : MonoBehaviour
    {
        const string ResourceRoot = "Art/Tutorial2p5D/Tutorial2p5D_";
        const float OriginX = -4f;
        static readonly float[] FollowRates = { .99f, .92f, .88f };
        static readonly float[] Depths = { 7f, 5f, 3f };
        public SpriteRenderer[] Layers { get; private set; }
        public SpriteRenderer[] FloorTiles { get; private set; }
        public bool Visible { get; private set; }
        public float Follow(int layer) => FollowRates[layer];
        Camera view;
        PrototypeRunController run;
        SpriteRenderer[] legacy;
        bool[] legacyOff;
        SpriteRenderer[,] guards;
        SpriteRenderer[] shadows;
        Collider2D[] supports;
        Sprite floorSprite, shadowSprite;
        Texture2D shadowTexture;
        Material floorMaterial, edgeMaterial;

        public static bool Ensure(GameObject root)
        {
            if (!Application.isPlaying || root.scene.name != "Tutorial") return false;
            if (root.TryGetComponent(out TutorialCraftRoomLayers existing))
            {
                if (existing.enabled) return false;
                existing.enabled = true; return true;
            }
            // Retain the already-working background if an asset is missing.
            foreach (string name in new[] { "Far", "Mid", "NearRefined" })
                if (Resources.Load<Sprite>(ResourceRoot + name) == null)
                { Debug.LogWarning("Tutorial 2.5D asset missing: " + name); return false; }
            if (Resources.Load<Shader>("TutorialBackgroundFloor") == null ||
                Resources.Load<Shader>("TutorialBackgroundEdge") == null)
            { Debug.LogWarning("Tutorial 2.5D background shaders missing"); return false; }
            root.AddComponent<TutorialCraftRoomLayers>(); return true;
        }
        SpriteRenderer Make(string name, Sprite sprite, int order)
        {
            var go = new GameObject(name); go.transform.SetParent(transform, false);
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite; renderer.sortingOrder = order; renderer.enabled = false;
            return renderer;
        }
        void Start()
        {
            view = Camera.main; run = FindFirstObjectByType<PrototypeRunController>();
            legacy = GetComponentsInChildren<SpriteRenderer>(true);
            legacyOff = new bool[legacy.Length];
            for (int i = 0; i < legacy.Length; i++) legacyOff[i] = legacy[i].forceRenderingOff;
            Layers = new SpriteRenderer[3]; guards = new SpriteRenderer[2, 2];
            edgeMaterial = new Material(Resources.Load<Shader>("TutorialBackgroundEdge"));
            string[] names = { "Far", "Mid", "NearRefined" };
            for (int i = 0; i < 3; i++)
            {
                var sprite = Resources.Load<Sprite>(ResourceRoot + names[i]);
                Layers[i] = Make("Tutorial 2.5D " + names[i], sprite, -130 + i * 20);
                if (i == 1) Layers[i].sharedMaterial = edgeMaterial;
                if (i < 2) for (int side = 0; side < 2; side++)
                {
                    guards[i, side] = Make("Tutorial 2.5D " + names[i] + " edge " + side, sprite, -130 + i * 20);
                    if (i == 1) guards[i, side].sharedMaterial = edgeMaterial;
                }
            }
            var mid = Layers[1].sprite;
            floorSprite = Sprite.Create(mid.texture, new Rect(0, 0, mid.texture.width, Mathf.RoundToInt(mid.texture.height * .17f)),
                new Vector2(.5f, .5f), mid.pixelsPerUnit, 0, SpriteMeshType.FullRect);
            floorMaterial = new Material(Resources.Load<Shader>("TutorialBackgroundFloor"));
            FloorTiles = new SpriteRenderer[3];
            for (int i = 0; i < 3; i++)
            {
                FloorTiles[i] = Make("Tutorial 2.5D floor strip " + i, floorSprite, -95);
                FloorTiles[i].sharedMaterial = floorMaterial;
            }
            shadowTexture = new Texture2D(128, 64, TextureFormat.RGBA32, false)
                { wrapMode = TextureWrapMode.Clamp, name = "Tutorial projected support shadow" };
            var colors = new Color[128 * 64];
            for (int y = 0; y < 64; y++) for (int x = 0; x < 128; x++)
            {
                float radius = Mathf.Pow((x + .5f - 64) / 64, 2) + Mathf.Pow((y + .5f - 32) / 32, 2);
                colors[y * 128 + x] = new Color(.10f, .065f, .12f, Mathf.Pow(Mathf.Clamp01(1 - radius), 2) * .26f);
            }
            shadowTexture.SetPixels(colors); shadowTexture.Apply(false, true);
            shadowSprite = Sprite.Create(shadowTexture, new Rect(0, 0, 128, 64), new Vector2(.5f, .5f), 128);
            string[] terrain = { TutorialSectionOneSetup.StartFloorName, TutorialSectionOneSetup.LandingFloorName,
                TutorialSectionTwoSetup.LandingFloorName, TutorialSectionThreeSetup.LandingFloorName, TutorialSectionFourSetup.GoalFloorName };
            supports = new Collider2D[terrain.Length]; shadows = new SpriteRenderer[terrain.Length];
            for (int i = 0; i < terrain.Length; i++)
            {
                supports[i] = GameObject.Find(terrain[i])?.GetComponent<Collider2D>();
                shadows[i] = Make("Tutorial support projection " + terrain[i], shadowSprite, -94);
            }
            Refresh();
        }
        void LateUpdate() => Refresh();
        public void Refresh()
        {
            if (Layers == null || view == null || run == null) return;
            bool show = enabled && run.Outcome != PrototypeRunController.RunOutcome.WaitingToStart;
            Visible = show;
            for (int i = 0; i < legacy.Length; i++) if (legacy[i] != null) legacy[i].forceRenderingOff = show || legacyOff[i];
            float h = view.orthographicSize * 2;
            float width = Mathf.Max(36, h * view.aspect + 5, (h + .8f) * (16f / 9f));
            float travel = view.transform.position.x - OriginX;
            for (int i = 0; i < 3; i++)
            {
                var layer = Layers[i]; layer.enabled = show;
                layer.transform.position = new Vector3(view.transform.position.x - travel * (1 - FollowRates[i]), view.transform.position.y + 1.1f, Depths[i]);
                float scale = width / layer.sprite.bounds.size.x;
                layer.transform.localScale = new Vector3(scale, scale, 1);
                if (i < 2) for (int side = 0; side < 2; side++)
                {
                    var guard = guards[i, side]; guard.enabled = show; guard.flipX = true;
                    guard.transform.localScale = layer.transform.localScale;
                    guard.transform.position = layer.transform.position + Vector3.right * width * (side == 0 ? -1 : 1);
                }
            }
            float floorY = Layers[1].transform.position.y - Layers[1].bounds.size.y * .5f + floorSprite.bounds.size.y * Layers[1].transform.localScale.y * .5f;
            for (int i = 0; i < FloorTiles.Length; i++)
            {
                var floor = FloorTiles[i]; floor.enabled = show; floor.flipX = i != 1;
                floor.transform.localScale = Layers[1].transform.localScale;
                floor.transform.position = new Vector3(Layers[2].transform.position.x + (i - 1) * width, floorY, 3.1f);
            }
            for (int i = 0; i < shadows.Length; i++)
            {
                var shadow = shadows[i]; shadow.enabled = show && supports[i] != null;
                if (supports[i] == null) continue;
                var bounds = supports[i].bounds;
                float projectedY = FloorTiles[1].bounds.min.y + FloorTiles[1].bounds.size.y * .32f;
                shadow.transform.position = new Vector3(bounds.center.x + .65f, projectedY, 3.05f);
                shadow.transform.localScale = new Vector3(bounds.size.x + 2, 1.6f, 1);
            }
        }
        public bool LegacyRestored()
        {
            if (legacy != null) for (int i = 0; i < legacy.Length; i++)
                if (legacy[i] != null && legacy[i].forceRenderingOff != legacyOff[i]) return false;
            return !Visible;
        }
        public bool LegacyHidden()
        {
            if (!Visible || legacy == null) return false;
            foreach (var renderer in legacy) if (renderer != null && renderer.enabled && !renderer.forceRenderingOff) return false;
            return true;
        }
        void RestoreLegacy()
        {
            if (legacy != null) for (int i = 0; i < legacy.Length; i++)
                if (legacy[i] != null) legacy[i].forceRenderingOff = legacyOff[i];
            foreach (var renderer in GetComponentsInChildren<SpriteRenderer>(true))
                if (renderer != null && (renderer.gameObject.name.StartsWith("Tutorial 2.5D") ||
                    renderer.gameObject.name.StartsWith("Tutorial support projection"))) renderer.enabled = false;
            Visible = false;
        }
        void OnDisable() => RestoreLegacy();
        void OnDestroy()
        {
            RestoreLegacy();
            if (floorMaterial != null) Destroy(floorMaterial);
            if (edgeMaterial != null) Destroy(edgeMaterial);
            if (floorSprite != null) Destroy(floorSprite);
            if (shadowSprite != null) Destroy(shadowSprite);
            if (shadowTexture != null) Destroy(shadowTexture);
        }
    }
}
