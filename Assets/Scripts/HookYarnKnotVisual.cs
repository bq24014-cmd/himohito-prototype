using UnityEngine;

namespace HimoHito
{
    /// <summary>A tiny tied loop at the visible rope tip. Never modifies the hook or joint.</summary>
    [DefaultExecutionOrder(110)]
    public sealed class HookYarnKnotVisual : MonoBehaviour
    {
        private const float TieDuration = 0.20f;
        private const float UntieDuration = 0.12f;
        private const int Segments = 40;
        private LineRenderer line;
        private Material material;
        private Rigidbody2D owner;
        private RopeRetractVisual returningRope;
        private Vector3 anchor;
        private Vector3 across, along;
        private float elapsed, width, opacity;
        private float releaseVisible, releaseSize;
        private bool releasing, cancelled;

        public static HookYarnKnotVisual Play(LineRenderer source, Rigidbody2D player, Vector2 point)
        {
            if (source == null || !source.enabled || source.positionCount < 2 ||
                source.sharedMaterial == null || player == null || !player.simulated) return null;
            var root = new GameObject("Hook Yarn Knot Visual");
            root.transform.SetParent(player.transform, false);
            var effect = root.AddComponent<HookYarnKnotVisual>();
            effect.owner = player;
            effect.anchor = point;
            Vector3 origin = source.GetPosition(0);
            if (!source.useWorldSpace) origin = source.transform.TransformPoint(origin);
            effect.along = (origin - effect.anchor).normalized;
            if (effect.along.sqrMagnitude < 0.001f) effect.along = Vector3.down;
            effect.across = new Vector3(-effect.along.y, effect.along.x, 0f);
            effect.width = Mathf.Clamp(source.endWidth * 0.48f, 0.045f, 0.075f);
            effect.opacity = source.endColor.a;
            effect.material = new Material(source.sharedMaterial) { name = "Hook Knot Yarn Material" };
            effect.line = root.AddComponent<LineRenderer>();
            effect.line.sharedMaterial = effect.material;
            effect.line.useWorldSpace = true;
            effect.line.positionCount = Segments;
            effect.line.numCapVertices = 4;
            effect.line.numCornerVertices = 3;
            effect.line.textureMode = LineTextureMode.Tile;
            Texture texture = effect.material.mainTexture;
            float tileLength = texture != null ? effect.width * texture.width / texture.height : 1f;
            effect.line.textureScale = new Vector2(1f / Mathf.Max(0.01f, tileLength), 1f);
            effect.line.startColor = effect.line.endColor = source.endColor;
            effect.line.sortingLayerID = source.sortingLayerID;
            // In front of the existing blue/green ring, without hiding its rim.
            effect.line.sortingOrder = Mathf.Max(9, source.sortingOrder + 1);
            effect.Draw();
            return effect;
        }

        public void Release(RopeRetractVisual rope)
        {
            if (rope == null) { Cancel(); return; }
            float tied = Mathf.Clamp01(elapsed / TieDuration);
            releaseVisible = Mathf.SmoothStep(0f, 1f, tied * 1.65f);
            releaseSize = Mathf.Lerp(1.3f, 1f, Mathf.SmoothStep(0f, 1f, tied));
            returningRope = rope;
            releasing = true;
            elapsed = 0f;
        }

        public void Cancel()
        {
            if (cancelled) return;
            cancelled = true;
            if (line != null) line.enabled = false;
            Destroy(gameObject);
        }

        private void LateUpdate() => Advance(Time.deltaTime);

        private void Advance(float deltaTime)
        {
            if (cancelled) return;
            if (owner == null || !owner.simulated || !owner.gameObject.activeInHierarchy ||
                (releasing && returningRope == null)) { Cancel(); return; }
            if (deltaTime <= 0f) return;
            elapsed += deltaTime;
            if (releasing && elapsed >= UntieDuration) { Cancel(); return; }
            Draw();
        }

        private void Draw()
        {
            float tied = Mathf.Clamp01(elapsed / TieDuration);
            float undone = releasing ? Mathf.SmoothStep(0f, 1f, elapsed / UntieDuration) : 0f;
            float visible = releasing ? releaseVisible * (1f - undone) : Mathf.SmoothStep(0f, 1f, tied * 1.65f);
            float size = releasing ? Mathf.Lerp(releaseSize, 0.55f, undone) : Mathf.Lerp(1.3f, 1f, Mathf.SmoothStep(0f, 1f, tied));
            Vector3 center = releasing ? returningRope.TipPosition : anchor;
            line.enabled = visible > 0.001f;
            line.startWidth = line.endWidth = width;
            Color color = line.startColor;
            color.a = opacity * (1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.7f, 1f, undone)));
            line.startColor = line.endColor = color;
            for (int i = 0; i < Segments; i++)
            {
                float turn = visible * i / (Segments - 1f) * Mathf.PI * 2f;
                // One continuous figure-eight: the crossing tightens into a small yarn knot.
                Vector3 offset = across * (Mathf.Sin(turn) * 0.135f) +
                                 along * (Mathf.Sin(turn * 2f) * 0.075f);
                line.SetPosition(i, center + offset * size);
            }
        }

        private void OnDisable()
        {
            if (line != null) line.enabled = false;
        }

        private void OnDestroy()
        {
            if (material != null) Destroy(material);
        }
    }
}
