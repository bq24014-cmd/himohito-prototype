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

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset()
        {
            Current = null;
            ProofEnabled = !Array.Exists(Environment.GetCommandLineArgs(), a => a == "--tutorial2p5d-baseline");
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
            RefreshImmediate();
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
            for (int i = 0; i < Layers.Length; i++)
            {
                Layers[i].enabled = show;
                // Finite proof art: overscan and bounded offset, never mirror/wrap the toys.
                float relativeX = Mathf.Clamp(-(view.transform.position.x - originX) * (1 - FollowRates[i]),
                    -coverageMargin, coverageMargin);
                Layers[i].transform.position = new Vector3(view.transform.position.x + relativeX,
                    view.transform.position.y + 1.1f, Depths[i]);
                float scale = width / Layers[i].sprite.bounds.size.x;
                Layers[i].transform.localScale = new Vector3(scale, scale, 1);
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
        }
    }
}
