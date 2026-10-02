using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using HimoHito;

namespace HimoHitoProof
{
    /// <summary>Isolated proof only. No gameplay, physics, input or camera mutations.</summary>
    [DefaultExecutionOrder(1000)]
    public sealed class Tutorial2p5DBackground : MonoBehaviour
    {
        public static bool ProofEnabled = true;
        public static bool RefineEnabled = true;
        public static Tutorial2p5DBackground Current { get; private set; }
        public bool Visible { get; private set; }
        public SpriteRenderer[] Layers { get; private set; }
        public readonly float[] FollowRates = { .99f, .92f, .78f };
        public readonly float[] Depths = { 7f, 5f, 3f };
        Camera view;
        PrototypeRunController run;
        SpriteRenderer[] baseline;
        bool[] baselineFlags;
        float originX;
        bool originReady;
        Sprite originalNear, refinedNear, floorSprite, shadowSprite;
        Texture2D shadowTexture;
        Material floorMaterial;
        public SpriteRenderer FloorStrip { get; private set; }
        SpriteRenderer[] contactShadows;
        Collider2D[] supports;
        public float EffectiveFollow(int index) => RefineEnabled && index == 2 ? .88f : FollowRates[index];

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset()
        {
            Current = null;
            ProofEnabled = !Array.Exists(Environment.GetCommandLineArgs(), a => a == "--tutorial2p5d-baseline");
            RefineEnabled = !Array.Exists(Environment.GetCommandLineArgs(), a => a == "--foreground-before");
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            SceneManager.sceneLoaded -= Loaded;
            SceneManager.sceneLoaded += Loaded;
            Install(SceneManager.GetActiveScene());
        }

        static void Loaded(Scene scene, LoadSceneMode mode) => Install(scene);
        static void Install(Scene scene)
        {
            if (scene.name != "Tutorial" || FindFirstObjectByType<Tutorial2p5DBackground>() != null) return;
            new GameObject("Tutorial Section 1 — 2.5D Proof ONLY").AddComponent<Tutorial2p5DBackground>();
        }

        void Start()
        {
            Current = this;
            view = Camera.main;
            run = FindFirstObjectByType<PrototypeRunController>();
            var root = GameObject.Find("Tutorial Night Child Room Background");
            if (root != null)
            {
                baseline = root.GetComponentsInChildren<SpriteRenderer>(true);
                baselineFlags = new bool[baseline.Length];
                for (int i = 0; i < baseline.Length; i++) baselineFlags[i] = baseline[i].forceRenderingOff;
            }
            else { baseline = new SpriteRenderer[0]; baselineFlags = new bool[0]; }
            Layers = new SpriteRenderer[3];
            string[] names = { "Far", "Mid", "Near" };
            for (int i = 0; i < 3; i++)
            {
                var sprite = Resources.Load<Sprite>("Proof/Tutorial2p5D/Tutorial2p5D_" + names[i]);
                if (sprite == null) throw new InvalidOperationException("Proof sprite missing: " + names[i]);
                var child = new GameObject("Proof " + names[i]);
                child.transform.SetParent(transform, false);
                var renderer = child.AddComponent<SpriteRenderer>();
                renderer.sprite = sprite;
                renderer.sortingOrder = -130 + i * 20;
                renderer.color = Color.white;
                renderer.enabled = false;
                Layers[i] = renderer;
            }
            originalNear = Layers[2].sprite;
            refinedNear = Resources.Load<Sprite>("Proof/Tutorial2p5D/Tutorial2p5D_NearRefined");
            if (refinedNear == null) throw new InvalidOperationException("Refined Near is missing");
            CreateRefinement();
            RefreshImmediate();
        }

        void CreateRefinement()
        {
            // Reuse the existing Mid texture's floor pixels, not a new room image.
            var mid = Layers[1].sprite;
            int floorPixels = Mathf.RoundToInt(mid.texture.height * .17f);
            floorSprite = Sprite.Create(mid.texture, new Rect(0, 0, mid.texture.width, floorPixels),
                new Vector2(.5f, .5f), mid.pixelsPerUnit, 0, SpriteMeshType.FullRect);
            var child = new GameObject("Refine — near floor strip (existing Mid pixels)");
            child.transform.SetParent(transform, false);
            FloorStrip = child.AddComponent<SpriteRenderer>();
            FloorStrip.sprite = floorSprite;
            FloorStrip.sortingOrder = -95;
            floorMaterial = new Material(Resources.Load<Shader>("Proof/FloorBlend"));
            FloorStrip.sharedMaterial = floorMaterial;
            FloorStrip.enabled = false;
            // Code-native soft shadow, same approach as existing PlayerContactShadow.
            // No collider, body, light, or gameplay-material change.
            shadowTexture = new Texture2D(128, 64, TextureFormat.RGBA32, false) {
                name = "Refine small projected support shadow", wrapMode = TextureWrapMode.Clamp
            };
            var colors = new Color[128 * 64];
            for (int y = 0; y < 64; y++) for (int x = 0; x < 128; x++)
            {
                float r = Mathf.Pow((x + .5f - 64f) / 64f, 2) + Mathf.Pow((y + .5f - 32f) / 32f, 2);
                float a = Mathf.Pow(Mathf.Clamp01(1 - r), 2) * .26f;
                colors[y * 128 + x] = new Color(.10f, .065f, .12f, a);
            }
            shadowTexture.SetPixels(colors); shadowTexture.Apply(false, true);
            shadowSprite = Sprite.Create(shadowTexture, new Rect(0, 0, 128, 64), new Vector2(.5f, .5f), 128);
            string[] names = { TutorialSectionOneSetup.StartFloorName, TutorialSectionOneSetup.LandingFloorName };
            contactShadows = new SpriteRenderer[2]; supports = new Collider2D[2];
            for (int i = 0; i < 2; i++)
            {
                var support = GameObject.Find(names[i]); supports[i] = support != null ? support.GetComponent<Collider2D>() : null;
                var obj = new GameObject("Refine — support contact shadow " + i); obj.transform.SetParent(transform, false);
                contactShadows[i] = obj.AddComponent<SpriteRenderer>();
                contactShadows[i].sprite = shadowSprite; contactShadows[i].sortingOrder = -94; contactShadows[i].enabled = false;
            }
        }

        void LateUpdate() => RefreshImmediate();

        public void RefreshImmediate()
        {
            if (Layers == null || view == null) return;
            bool show = ProofEnabled && run != null && run.CurrentTutorialSection == 1 &&
                run.Outcome == PrototypeRunController.RunOutcome.Playing &&
                !MainStagePreview.IsActive && !StageStartTransition.IsActive;
            if (show && !originReady) { originX = view.transform.position.x; originReady = true; }
            Visible = show;
            for (int i = 0; i < baseline.Length; i++)
                if (baseline[i] != null) baseline[i].forceRenderingOff = show || baselineFlags[i];
            float height = view.orthographicSize * 2f;
            float width = Mathf.Max(36f, height * view.aspect + 5f, (height + .8f) * (16f / 9f));
            float coverageMargin = Mathf.Max(0, (width - height * view.aspect) * .5f - .1f);
            Layers[2].sprite = RefineEnabled ? refinedNear : originalNear;
            for (int i = 0; i < Layers.Length; i++)
            {
                Layers[i].enabled = show;
                // Finite proof art: overscan and bounded offset, never mirror/wrap the toys.
                float relativeX = Mathf.Clamp(-(view.transform.position.x - originX) * (1 - EffectiveFollow(i)),
                    -coverageMargin, coverageMargin);
                Layers[i].transform.position = new Vector3(view.transform.position.x + relativeX,
                    view.transform.position.y + 1.1f, Depths[i]);
                float scale = width / Layers[i].sprite.bounds.size.x;
                Layers[i].transform.localScale = new Vector3(scale, scale, 1);
            }
            FloorStrip.enabled = show && RefineEnabled;
            FloorStrip.transform.position = new Vector3(Layers[2].transform.position.x,
                Layers[1].transform.position.y - Layers[1].bounds.size.y * .5f + floorSprite.bounds.size.y * Layers[1].transform.localScale.y * .5f, 3.1f);
            FloorStrip.transform.localScale = Layers[1].transform.localScale;
            for (int i = 0; i < contactShadows.Length; i++)
            {
                var s = contactShadows[i]; s.enabled = show && RefineEnabled && supports[i] != null;
                if (supports[i] == null) continue;
                var bounds = supports[i].bounds;
                // The artwork floor is a projected plane. This is a visual cast footprint,
                // not a new support surface; gameplay's actual support stays untouched.
                // Anchor the projection to the visible artwork floor, not the offscreen
                // physical bottom of the tall gameplay support. Never move that support.
                float projectedY = FloorStrip.bounds.min.y + FloorStrip.bounds.size.y * .32f;
                s.transform.position = new Vector3(bounds.center.x + .65f, projectedY, 3.05f);
                s.transform.localScale = new Vector3(bounds.size.x + 2f, 1.6f, 1);
            }
        }

        public bool BaselineRestored()
        {
            for (int i = 0; i < baseline.Length; i++)
                if (baseline[i] != null && baseline[i].forceRenderingOff != baselineFlags[i]) return false;
            return !Visible;
        }

        void OnDestroy()
        {
            if (baseline != null) for (int i = 0; i < baseline.Length; i++)
                if (baseline[i] != null) baseline[i].forceRenderingOff = baselineFlags[i];
            if (Current == this) Current = null;
            if (floorMaterial != null) Destroy(floorMaterial);
            if (floorSprite != null) Destroy(floorSprite);
            if (shadowSprite != null) Destroy(shadowSprite);
            if (shadowTexture != null) Destroy(shadowTexture);
        }
    }
}
