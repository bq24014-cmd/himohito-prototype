#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using HimoHito;

namespace HimoHitoAllTrial
{
    // ISOLATED TRIAL: only background SpriteRenderers. Never writes gameplay/camera.
    [DefaultExecutionOrder(1000)]
    public sealed class ReferenceTrial : MonoBehaviour
    {
        public enum DisplayMode { Production, PreviousSectionOne, AllTutorial }
        public static DisplayMode Mode = DisplayMode.Production;
        public static ReferenceTrial Current { get; private set; }
        public SpriteRenderer[] Layers { get; private set; }
        public bool Visible { get; private set; }
        public readonly float[] Follow = { .99f, .92f, .88f };
        public readonly float[] Depth = { 7f, 5f, 3f };
        public SpriteRenderer[] FloorTiles { get; private set; }
        Camera view;
        PrototypeRunController run;
        SpriteRenderer[] baseline;
        bool[] originalOff;
        SpriteRenderer[,] guards;
        SpriteRenderer[] shadows;
        Collider2D[] supports;
        Sprite floorSprite, shadowSprite;
        Texture2D shadowTexture;
        Material floorMaterial;
        const float OriginX = -4f;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset()
        {
            Current = null;
            var args = Environment.GetCommandLineArgs();
            Mode = Array.Exists(args, a => a == "--tutorial2p5d-before") ? DisplayMode.PreviousSectionOne :
                Array.Exists(args, a => a == "--tutorial2p5d-production") ? DisplayMode.Production : DisplayMode.Production;
        }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            SceneManager.sceneLoaded -= Loaded; SceneManager.sceneLoaded += Loaded;
            Install(SceneManager.GetActiveScene());
        }
        static void Loaded(Scene scene, LoadSceneMode mode) => Install(scene);
        static void Install(Scene scene)
        {
            if (scene.name != "Tutorial" || FindFirstObjectByType<ReferenceTrial>() != null) return;
            new GameObject("Tutorial ALL — background trial only").AddComponent<ReferenceTrial>();
        }
        SpriteRenderer Make(string name, Sprite sprite, int order)
        {
            var go = new GameObject(name); go.transform.SetParent(transform, false);
            var r = go.AddComponent<SpriteRenderer>(); r.sprite = sprite; r.sortingOrder = order; r.enabled = false;
            return r;
        }
        void Start()
        {
            Current = this; view = Camera.main; run = FindFirstObjectByType<PrototypeRunController>();
            var root = GameObject.Find("Tutorial Night Child Room Background");
            baseline = root != null ? root.GetComponentsInChildren<SpriteRenderer>(true).Where(r => !r.name.StartsWith("Tutorial 2.5D") && !r.name.StartsWith("Tutorial support projection")).ToArray() : new SpriteRenderer[0];
            originalOff = new bool[baseline.Length];
            for (int i = 0; i < baseline.Length; i++) originalOff[i] = baseline[i].forceRenderingOff;
            Layers = new SpriteRenderer[3]; guards = new SpriteRenderer[2, 2];
            string[] names = { "Far", "Mid", "NearRefined" };
            for (int i = 0; i < 3; i++)
            {
                var sprite = Resources.Load<Sprite>("Art/Tutorial2p5D/Tutorial2p5D_" + names[i]);
                if (sprite == null) throw new InvalidOperationException("Approved sprite missing: " + names[i]);
                Layers[i] = Make("Trial " + names[i], sprite, -130 + i * 20);
                if (i < 2) for (int side = 0; side < 2; side++)
                    guards[i, side] = Make("Trial " + names[i] + " edge continuation " + side, sprite, -130 + i * 20);
            }
            // Near has no repeated toys: the authored groups slide out of view naturally.
            // Opaque room/sky neighbours cover the edge instead of clamping parallax.
            var mid = Layers[1].sprite;
            floorSprite = Sprite.Create(mid.texture, new Rect(0, 0, mid.texture.width, Mathf.RoundToInt(mid.texture.height * .17f)),
                new Vector2(.5f, .5f), mid.pixelsPerUnit, 0, SpriteMeshType.FullRect);
            floorMaterial = new Material(Resources.Load<Shader>("LegacyFloor"));
            FloorTiles = new SpriteRenderer[3];
            for (int i = 0; i < 3; i++)
            {
                FloorTiles[i] = Make("Trial near-floor strip " + i, floorSprite, -95);
                FloorTiles[i].sharedMaterial = floorMaterial;
            }
            shadowTexture = new Texture2D(128, 64, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, name = "Trial projected support shadow" };
            var colors = new Color[128 * 64];
            for (int y = 0; y < 64; y++) for (int x = 0; x < 128; x++)
            {
                float r = Mathf.Pow((x + .5f - 64) / 64, 2) + Mathf.Pow((y + .5f - 32) / 32, 2);
                colors[y * 128 + x] = new Color(.10f, .065f, .12f, Mathf.Pow(Mathf.Clamp01(1 - r), 2) * .26f);
            }
            shadowTexture.SetPixels(colors); shadowTexture.Apply(false, true);
            shadowSprite = Sprite.Create(shadowTexture, new Rect(0, 0, 128, 64), new Vector2(.5f, .5f), 128);
            string[] terrain = { TutorialSectionOneSetup.StartFloorName, TutorialSectionOneSetup.LandingFloorName,
                TutorialSectionTwoSetup.LandingFloorName, TutorialSectionThreeSetup.LandingFloorName, TutorialSectionFourSetup.GoalFloorName };
            supports = new Collider2D[terrain.Length]; shadows = new SpriteRenderer[terrain.Length];
            for (int i = 0; i < terrain.Length; i++)
            {
                supports[i] = GameObject.Find(terrain[i])?.GetComponent<Collider2D>();
                shadows[i] = Make("Trial support projection " + terrain[i], shadowSprite, -94);
            }
            Refresh();
        }
        // Diagnostic reference must not overwrite the production owner's
        // legacy visibility flags when it is not the active comparison mode.
        void LateUpdate() { if (Mode != DisplayMode.Production) Refresh(); }
        public void Refresh()
        {
            if (Layers == null || view == null || run == null) return;
            bool all = Mode == DisplayMode.AllTutorial;
            bool previous = Mode == DisplayMode.PreviousSectionOne && run.CurrentTutorialSection == 1 &&
                run.Outcome == PrototypeRunController.RunOutcome.Playing;
            bool show = (all ? run.Outcome != PrototypeRunController.RunOutcome.WaitingToStart : previous) &&
                !MainStagePreview.IsActive && !StageStartTransition.IsActive;
            Visible = show;
            for (int i = 0; i < baseline.Length; i++) if (baseline[i] != null) baseline[i].forceRenderingOff = show || originalOff[i];
            float h = view.orthographicSize * 2;
            float w = Mathf.Max(36, h * view.aspect + 5, (h + .8f) * (16f / 9f));
            float margin = Mathf.Max(0, (w - h * view.aspect) * .5f - .1f);
            float travel = view.transform.position.x - OriginX;
            for (int i = 0; i < 3; i++)
            {
                var layer = Layers[i]; layer.enabled = show;
                float shift = -travel * (1 - Follow[i]);
                if (!all) shift = Mathf.Clamp(shift, -margin, margin);
                layer.transform.position = new Vector3(view.transform.position.x + shift, view.transform.position.y + 1.1f, Depth[i]);
                float scale = w / layer.sprite.bounds.size.x;
                layer.transform.localScale = new Vector3(scale, scale, 1);
                if (i < 2) for (int side = 0; side < 2; side++)
                {
                    var guard = guards[i, side]; guard.enabled = show && all;
                    guard.flipX = true;
                    guard.transform.localScale = layer.transform.localScale;
                    guard.transform.position = layer.transform.position + Vector3.right * w * (side == 0 ? -1 : 1);
                }
            }
            float floorY = Layers[1].transform.position.y - Layers[1].bounds.size.y * .5f + floorSprite.bounds.size.y * Layers[1].transform.localScale.y * .5f;
            for (int i = 0; i < FloorTiles.Length; i++)
            {
                var f = FloorTiles[i]; f.enabled = show && (i == 1 || all);
                f.flipX = i != 1; f.transform.localScale = Layers[1].transform.localScale;
                f.transform.position = new Vector3(Layers[2].transform.position.x + (i - 1) * w, floorY, 3.1f);
            }
            for (int i = 0; i < shadows.Length; i++)
            {
                var s = shadows[i]; s.enabled = show && supports[i] != null && (all || i < 2);
                if (supports[i] == null) continue;
                var b = supports[i].bounds;
                float projectedY = FloorTiles[1].bounds.min.y + FloorTiles[1].bounds.size.y * .32f;
                s.transform.position = new Vector3(b.center.x + .65f, projectedY, 3.05f);
                s.transform.localScale = new Vector3(b.size.x + 2, 1.6f, 1);
            }
        }
        public bool Restored()
        {
            for (int i = 0; i < baseline.Length; i++) if (baseline[i] != null && baseline[i].forceRenderingOff != originalOff[i]) return false;
            return !Visible;
        }
        void OnDestroy()
        {
            if (baseline != null) for (int i = 0; i < baseline.Length; i++) if (baseline[i] != null) baseline[i].forceRenderingOff = originalOff[i];
            if (Current == this) Current = null;
            if (floorMaterial != null) Destroy(floorMaterial);
            if (floorSprite != null) Destroy(floorSprite);
            if (shadowSprite != null) Destroy(shadowSprite);
            if (shadowTexture != null) Destroy(shadowTexture);
        }
    }
}
#endif
