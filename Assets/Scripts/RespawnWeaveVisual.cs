using System.Collections.Generic;
using UnityEngine;

namespace HimoHito
{
    /// <summary>Reveals live character art stitch by stitch; owns no gameplay state.</summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(200)]
    public sealed class RespawnWeaveVisual : MonoBehaviour
    {
        private const float Duration = 0.8f;
        private readonly List<SpriteRenderer> renderers = new();
        private readonly List<Material> originalMaterials = new();
        private readonly LineRenderer[] strands = new LineRenderer[2];
        private Material revealMaterial;
        private Material yarnMaterial;
        private Rigidbody2D body;
        private PlayerMover mover;
        private float elapsed;
        private bool playing;
        private static readonly int ProgressId = Shader.PropertyToID("_WeaveProgress");
        private static readonly int BoundsId = Shader.PropertyToID("_WeaveBounds");

        public static void Play(GameObject player)
        {
            if (player == null) return;
            if (!player.TryGetComponent(out RespawnWeaveVisual effect))
                effect = player.AddComponent<RespawnWeaveVisual>();
            effect.Begin();
        }

        private void Begin()
        {
            GetComponent<FallUnravelVisual>()?.Cancel();
            Restore();
            if (!isActiveAndEnabled) return;
            body = GetComponent<Rigidbody2D>();
            mover = GetComponent<PlayerMover>();
            if (body == null || !body.simulated || MainStagePreview.IsActive) return;
            Shader shader = Resources.Load<Shader>("RespawnWeave");
            if (shader == null || !shader.isSupported) return;
            if (TryGetComponent(out RopeBodyVisual art)) art.CollectCharacterRenderers(renderers);
            else if (TryGetComponent(out SpriteRenderer fallback)) renderers.Add(fallback);
            if (renderers.Count == 0) return;
            if (revealMaterial == null)
                revealMaterial = new Material(shader) { name = "Player Respawn Stitches", hideFlags = HideFlags.HideAndDontSave };
            foreach (SpriteRenderer renderer in renderers)
            {
                originalMaterials.Add(renderer.sharedMaterial);
                renderer.sharedMaterial = revealMaterial;
            }
            elapsed = 0f;
            playing = true;
            UpdateArt(0f);
        }

        private void LateUpdate()
        {
            if (!playing) return;
            if (body == null || !body.simulated || (mover != null && !mover.enabled) || MainStagePreview.IsActive)
            {
                Restore();
                return;
            }
            Advance(Time.deltaTime);
        }

        private void Advance(float deltaTime)
        {
            if (!playing) return;
            elapsed += Mathf.Max(0f, deltaTime);
            if (elapsed >= Duration) { Restore(); return; }
            // Bounds follow the current pose after RopeBodyVisual.LateUpdate, including movement.
            UpdateArt(Mathf.Clamp01(elapsed / Duration));
        }

        private void UpdateArt(float progress)
        {
            bool found = false;
            Bounds bounds = default;
            SpriteRenderer front = null;
            foreach (SpriteRenderer renderer in renderers)
            {
                if (renderer == null || !renderer.enabled || renderer.sprite == null) continue;
                if (!found) { bounds = renderer.bounds; front = renderer; found = true; }
                else bounds.Encapsulate(renderer.bounds);
            }
            if (!found) return;
            revealMaterial.SetFloat(ProgressId, progress);
            revealMaterial.SetVector(BoundsId, new Vector4(bounds.min.x, bounds.min.y,
                Mathf.Max(.01f, bounds.size.x), Mathf.Max(.01f, bounds.size.y)));
            EnsureStrands();
            float tipY = Mathf.Lerp(bounds.min.y, bounds.max.y, progress);
            float row = progress * 10f;
            float across = Mathf.Repeat(row, 1f);
            if (Mathf.FloorToInt(row) % 2 != 0) across = 1f - across;
            float tipX = Mathf.Lerp(bounds.min.x, bounds.max.x, across);
            float alpha = 1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(.72f, 1f, progress));
            for (int side = 0; side < strands.Length; side++)
            {
                LineRenderer line = strands[side];
                if (line == null) continue;
                line.enabled = true;
                line.sortingLayerID = front.sortingLayerID;
                line.sortingOrder = front.sortingOrder + 1;
                line.startColor = line.endColor = new Color(1f, 1f, 1f, alpha * .85f);
                float sign = side == 0 ? -1f : 1f;
                Vector3 tip = new Vector3(tipX, tipY, bounds.center.z);
                Vector3 start = tip + new Vector3(sign * .5f * (1f - progress), -.13f, 0f);
                for (int i = 0; i < line.positionCount; i++)
                {
                    float t = i / (float)(line.positionCount - 1);
                    Vector3 point = Vector3.Lerp(start, tip, t);
                    point.y += Mathf.Sin(t * Mathf.PI * 2f) * .035f * (1f - progress);
                    line.SetPosition(i, point);
                }
            }
        }

        private void EnsureStrands()
        {
            if (strands[0] != null) return;
            Texture2D yarn = YarnRopeTexture.Load();
            Shader shader = Shader.Find("Sprites/Default");
            if (yarn == null || shader == null) return;
            yarnMaterial = new Material(shader) { name = "Respawn Gathering Yarn", mainTexture = yarn, hideFlags = HideFlags.HideAndDontSave };
            for (int i = 0; i < strands.Length; i++)
            {
                var child = new GameObject("Respawn Gathering Strand " + i);
                child.layer = gameObject.layer;
                child.transform.SetParent(transform, false);
                var line = child.AddComponent<LineRenderer>();
                line.sharedMaterial = yarnMaterial;
                line.positionCount = 12;
                line.useWorldSpace = true;
                line.startWidth = line.endWidth = .035f;
                line.textureMode = LineTextureMode.Tile;
                float tileLength = .035f * yarn.width / Mathf.Max(1f, yarn.height);
                line.textureScale = new Vector2(1f / Mathf.Max(.01f, tileLength), 1f);
                line.numCapVertices = 2;
                strands[i] = line;
            }
        }

        public void Cancel() => Restore();

        private void Restore()
        {
            for (int i = 0; i < originalMaterials.Count; i++)
                if (renderers[i] != null && renderers[i].sharedMaterial == revealMaterial)
                    renderers[i].sharedMaterial = originalMaterials[i];
            renderers.Clear();
            originalMaterials.Clear();
            foreach (LineRenderer line in strands) if (line != null) line.enabled = false;
            playing = false;
        }

        private void OnDisable() => Restore();
        private void OnDestroy()
        {
            Restore();
            foreach (LineRenderer line in strands) if (line != null) Dispose(line.gameObject);
            if (revealMaterial != null) Dispose(revealMaterial);
            if (yarnMaterial != null) Dispose(yarnMaterial);
        }
        private static void Dispose(Object item)
        {
            if (Application.isPlaying) Destroy(item);
            else DestroyImmediate(item);
        }
    }
}
