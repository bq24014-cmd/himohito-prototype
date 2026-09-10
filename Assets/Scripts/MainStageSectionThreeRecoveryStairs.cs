using UnityEngine;

namespace HimoHito
{
    /// <summary>
    /// Keeps the lower-route recovery stairs out of both valid swing paths.
    /// They become solid and visible only after the player presses the switch
    /// on the low dead end, where they are needed to prevent a soft lock.
    /// </summary>
    [DefaultExecutionOrder(220)]
    public sealed class MainStageSectionThreeRecoveryStairs : MonoBehaviour
    {
        private bool revealed;
        private float revealElapsed;
        private StepArt[] revealArt;
        private MaterialPropertyBlock fadeBlock;
        private static readonly string[] StepNames =
        {
            MainStageSectionThreeSetup.ReturnStepAName,
            MainStageSectionThreeSetup.ReturnStepBName,
            MainStageSectionThreeSetup.ReturnStepCName,
            MainStageSectionThreeSetup.ReturnStepDName
        };

        // Animate only the existing artwork; supporting colliders never move.
        private sealed class StepArt
        {
            public Renderer[] renderers;
            public Vector3[] positions;
            public Color[] colors;
            public bool[] hidden;
            public MaterialPropertyBlock[] blocks;
        }

        public bool IsRevealed => revealed;

        public void Configure()
        {
            if (Application.isPlaying && !revealed)
            {
                SetStairsActive(false);
            }
        }

        private void Awake()
        {
            Configure();
        }

        public void Reveal()
        {
            if (revealed)
            {
                return;
            }

            revealed = true;
            SetStairsActive(true);
            // Refresh once after activation, before capturing the final visual poses.
            RopeResource player = Object.FindFirstObjectByType<RopeResource>();
            if (player != null) MainStageVisuals.Apply(player.gameObject);
            if (Application.isPlaying) BeginReveal();
        }

        public void Hide()
        {
            FinishReveal();
            if (!revealed)
            {
                SetStairsActive(false);
                return;
            }

            revealed = false;
            SetStairsActive(false);
        }

        private void BeginReveal()
        {
            FinishReveal();
            revealElapsed = 0f;
            fadeBlock ??= new MaterialPropertyBlock();
            revealArt = new StepArt[StepNames.Length];
            for (int i = 0; i < StepNames.Length; i++)
            {
                GameObject step = FindStep(StepNames[i]);
                if (step == null) continue;
                var art = new StepArt { renderers = step.GetComponentsInChildren<Renderer>() };
                int count = art.renderers.Length;
                art.positions = new Vector3[count];
                art.colors = new Color[count];
                art.hidden = new bool[count];
                art.blocks = new MaterialPropertyBlock[count];
                for (int r = 0; r < count; r++)
                {
                    Renderer renderer = art.renderers[r];
                    art.positions[r] = renderer.transform.localPosition;
                    art.hidden[r] = renderer.forceRenderingOff;
                    art.colors[r] = renderer is SpriteRenderer sprite ? sprite.color : Color.white;
                    art.blocks[r] = new MaterialPropertyBlock();
                    renderer.GetPropertyBlock(art.blocks[r]);
                }
                revealArt[i] = art;
            }
            ApplyReveal();
        }

        private void LateUpdate() => AdvanceReveal(Time.deltaTime);

        private void AdvanceReveal(float dt)
        {
            if (revealArt == null || dt <= 0f) return;
            revealElapsed += dt;
            if (revealElapsed >= .98f) { FinishReveal(); return; }
            ApplyReveal();
        }

        private void ApplyReveal()
        {
            for (int i = 0; i < revealArt.Length; i++)
            {
                StepArt art = revealArt[i];
                if (art == null) continue;
                float age = revealElapsed - .10f - i * .14f;
                float rise = Mathf.Clamp01(age / .30f);
                float offset = -.65f * Mathf.Pow(1f - rise, 3f);
                if (age > .30f)
                    offset = -.025f * Mathf.Sin(Mathf.PI * Mathf.Clamp01((age - .30f) / .16f));
                float alpha = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(age / .16f));
                for (int r = 0; r < art.renderers.Length; r++)
                {
                    Renderer renderer = art.renderers[r];
                    if (renderer == null) continue;
                    // The root owns physics. Keep a fallback root sprite unchanged.
                    if (renderer.transform.parent == null || renderer.GetComponent<Collider2D>() != null) continue;
                    renderer.transform.localPosition = art.positions[r] +
                        renderer.transform.parent.InverseTransformVector(Vector3.up * offset);
                    renderer.forceRenderingOff = art.hidden[r] || age <= 0f;
                    if (renderer is SpriteRenderer sprite)
                    {
                        Color color = art.colors[r]; color.a *= alpha; sprite.color = color;
                    }
                    else
                    {
                        renderer.GetPropertyBlock(fadeBlock);
                        fadeBlock.SetColor("_Color", new Color(1f, 1f, 1f, alpha));
                        renderer.SetPropertyBlock(fadeBlock);
                    }
                }
            }
        }

        private void FinishReveal()
        {
            if (revealArt == null) return;
            foreach (StepArt art in revealArt)
            {
                if (art == null) continue;
                for (int r = 0; r < art.renderers.Length; r++)
                {
                    Renderer renderer = art.renderers[r];
                    if (renderer == null) continue;
                    if (renderer.GetComponent<Collider2D>() == null)
                        renderer.transform.localPosition = art.positions[r];
                    renderer.forceRenderingOff = art.hidden[r];
                    if (renderer is SpriteRenderer sprite) sprite.color = art.colors[r];
                    else renderer.SetPropertyBlock(art.blocks[r]);
                }
            }
            revealArt = null;
        }

        private void OnDisable() => FinishReveal();

        private static void SetStairsActive(bool active)
        {
            foreach (string name in StepNames) SetActive(name, active);
        }

        private static void SetActive(string objectName, bool active)
        {
            GameObject step = FindStep(objectName);
            if (step != null) step.SetActive(active);
        }

        private static GameObject FindStep(string objectName)
        {
            foreach (GameObject candidate in
                     Resources.FindObjectsOfTypeAll<GameObject>())
            {
                if (candidate.scene.IsValid() &&
                    candidate.scene.name == "MainStage" &&
                    candidate.name == objectName)
                {
                    return candidate;
                }
            }
            return null;
        }
    }
}
