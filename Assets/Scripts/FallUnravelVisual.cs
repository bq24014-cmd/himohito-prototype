using System.Collections.Generic;
using UnityEngine;

namespace HimoHito
{
    /// <summary>A frozen pose unknits into loose yarn after failure is already confirmed.</summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(210)]
    public sealed class FallUnravelVisual : MonoBehaviour
    {
        private const float Duration = .8f;
        private readonly List<SpriteRenderer> originals = new();
        private readonly List<bool> hiddenStates = new();
        private readonly List<SpriteRenderer> copies = new();
        private readonly LineRenderer[] strands = new LineRenderer[6];
        private GameObject presentation;
        private Material stitches, yarn;
        private Bounds bounds;
        private float elapsed;
        public bool IsPlaying { get; private set; }

        public static bool IsPlayingFor(GameObject player) =>
            player != null && player.TryGetComponent(out FallUnravelVisual effect) && effect.IsPlaying;

        public static void Play(GameObject player)
        {
            if (player == null) return;
            if (!player.TryGetComponent(out FallUnravelVisual effect))
                effect = player.AddComponent<FallUnravelVisual>();
            effect.Begin();
        }

        private void Begin()
        {
            Cancel();
            if (!isActiveAndEnabled || MainStagePreview.IsActive ||
                !TryGetComponent(out Rigidbody2D body) || body.simulated) return;
            Shader shader = Resources.Load<Shader>("RespawnWeave");
            Shader lineShader = Shader.Find("Sprites/Default");
            Texture2D texture = YarnRopeTexture.Load();
            if (shader == null || !shader.isSupported ||
                lineShader == null || texture == null) return;

            // A retry can fail before its reveal finishes; never snapshot a half-visible material.
            GetComponent<RespawnWeaveVisual>()?.Cancel();
            if (TryGetComponent(out RopeBodyVisual art)) art.CollectCharacterRenderers(originals);
            else if (TryGetComponent(out SpriteRenderer fallback)) originals.Add(fallback);
            originals.RemoveAll(r => r == null || !r.enabled || !r.gameObject.activeInHierarchy || r.sprite == null);
            if (originals.Count == 0) return;
            bounds = originals[0].bounds;
            foreach (SpriteRenderer renderer in originals) bounds.Encapsulate(renderer.bounds);

            // Keep the falling pose exactly where it was, even outside the viewport.
            // Clamping it into view makes a falling character appear to jump back upward.
            if (presentation == null)
                presentation = new GameObject("Fall Unravelling Presentation") { hideFlags = HideFlags.HideAndDontSave };
            presentation.SetActive(true);
            if (stitches == null) stitches = new Material(shader) { hideFlags = HideFlags.HideAndDontSave };
            if (yarn == null) yarn = new Material(lineShader) { mainTexture = texture, hideFlags = HideFlags.HideAndDontSave };
            stitches.SetVector("_WeaveBounds", new Vector4(bounds.min.x, bounds.min.y,
                Mathf.Max(.01f, bounds.size.x), Mathf.Max(.01f, bounds.size.y)));
            for (int i = 0; i < originals.Count; i++)
            {
                SpriteRenderer source = originals[i];
                if (i == copies.Count)
                {
                    var child = new GameObject("Frozen Fall Pose " + i);
                    child.transform.SetParent(presentation.transform, false);
                    copies.Add(child.AddComponent<SpriteRenderer>());
                }
                SpriteRenderer copy = copies[i];
                copy.gameObject.layer = source.gameObject.layer;
                copy.transform.SetPositionAndRotation(source.transform.position, source.transform.rotation);
                copy.transform.localScale = source.transform.lossyScale;
                copy.sprite = source.sprite;
                copy.color = source.color;
                copy.flipX = source.flipX;
                copy.flipY = source.flipY;
                copy.sortingLayerID = source.sortingLayerID;
                copy.sortingOrder = source.sortingOrder + 10;
                copy.sharedMaterial = stitches;
                copy.enabled = true;
                hiddenStates.Add(source.forceRenderingOff);
                source.forceRenderingOff = true;
            }
            for (int i = 0; i < strands.Length; i++)
            {
                if (strands[i] == null)
                {
                    var child = new GameObject("Loose Fall Yarn " + i);
                    child.transform.SetParent(presentation.transform, false);
                    strands[i] = child.AddComponent<LineRenderer>();
                }
                LineRenderer line = strands[i];
                line.gameObject.layer = originals[0].gameObject.layer;
                line.sharedMaterial = yarn;
                line.positionCount = 18;
                line.useWorldSpace = true;
                line.startWidth = line.endWidth = .035f;
                line.textureMode = LineTextureMode.Tile;
                line.textureScale = new Vector2(1f / Mathf.Max(.01f, .035f * texture.width / texture.height), 1f);
                line.numCapVertices = 2;
                line.sortingLayerID = copies[0].sortingLayerID;
                line.sortingOrder = copies[0].sortingOrder + 1;
            }
            elapsed = 0f;
            IsPlaying = true;
            UpdateArt(0f);
        }

        private void LateUpdate()
        {
            if (!IsPlaying) return;
            if (TryGetComponent(out Rigidbody2D body) && body.simulated) { Cancel(); return; }
            Advance(Time.deltaTime);
        }

        private void Advance(float deltaTime)
        {
            if (!IsPlaying) return;
            elapsed += Mathf.Max(0f, deltaTime);
            UpdateArt(Mathf.Clamp01(elapsed / Duration));
            if (elapsed < Duration) return;
            IsPlaying = false;
            presentation.SetActive(false);
            // Originals stay hidden until retry, so no single-frame reappearance before the failure UI.
        }

        private void UpdateArt(float progress)
        {
            float remaining = 1f - Mathf.Clamp01(progress / .8f);
            stitches.SetFloat("_WeaveProgress", remaining);
            for (int strand = 0; strand < strands.Length; strand++)
            {
                LineRenderer line = strands[strand];
                float age = Mathf.Clamp01((progress - strand * .055f) / .67f);
                line.enabled = age > 0f && age < 1f;
                float width = Mathf.Min(bounds.size.x * .8f, .65f);
                float x = bounds.center.x + (strand / 5f - .5f) * width;
                float alpha = Mathf.SmoothStep(0f, 1f, age / .12f) * (1f - Mathf.SmoothStep(0f, 1f, (age - .65f) / .35f));
                line.startColor = line.endColor = new Color(1, 1, 1, alpha);
                float length = Mathf.Lerp(.04f, .65f, Mathf.SmoothStep(0f, 1f, age));
                Vector3 start = new Vector3(x, Mathf.Lerp(bounds.min.y, bounds.max.y, remaining), bounds.center.z);
                for (int point = 0; point < line.positionCount; point++)
                {
                    float t = point / (float)(line.positionCount - 1);
                    line.SetPosition(point, start + new Vector3(
                        Mathf.Sin(t * Mathf.PI * 2f + strand * 1.7f + age * 2f) * .10f * t + (strand / 5f - .5f) * age * t * .3f,
                        -t * length - age * age * .12f, 0));
                }
            }
        }

        public void Cancel()
        {
            IsPlaying = false;
            for (int i = 0; i < hiddenStates.Count; i++)
                if (originals[i] != null) originals[i].forceRenderingOff = hiddenStates[i];
            originals.Clear();
            hiddenStates.Clear();
            foreach (SpriteRenderer copy in copies) if (copy != null) copy.enabled = false;
            if (presentation != null) presentation.SetActive(false);
        }

        private void OnDisable() => Cancel();
        private void OnDestroy()
        {
            Cancel();
            Dispose(presentation);
            Dispose(stitches);
            Dispose(yarn);
        }
        private static void Dispose(Object item)
        {
            if (item == null) return;
            if (Application.isPlaying) Destroy(item);
            else DestroyImmediate(item);
        }
    }
}
