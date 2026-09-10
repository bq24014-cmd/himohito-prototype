using UnityEngine;

namespace HimoHito
{
    /// <summary>Reusable, drawing-only failed shot. Never attaches or spends rope.</summary>
    [DefaultExecutionOrder(100)]
    public sealed class RopeMissVisual : MonoBehaviour
    {
        private const float ExtendTime = .13f;
        private const float ReturnStart = .29f;
        private const float Duration = .56f;
        private const int Segments = 24;
        private RopeController controller;
        private Rigidbody2D owner;
        private RopeBodyVisual artwork;
        private LineRenderer line;
        private Material material;
        private Vector2 direction;
        private float length;
        private float elapsed = Duration;

        public static RopeMissVisual Create(RopeController controller, LineRenderer source,
            Rigidbody2D owner)
        {
            Shader shader = Shader.Find("Sprites/Default");
            if (shader == null || source == null || owner == null) return null;
            var visual = new GameObject("Missed Shot Yarn");
            visual.transform.SetParent(owner.transform, false);
            var effect = visual.AddComponent<RopeMissVisual>();
            effect.controller = controller;
            effect.owner = owner;
            effect.artwork = owner.GetComponent<RopeBodyVisual>();
            effect.material = new Material(shader)
            {
                name = "Missed Shot Yarn Material",
                hideFlags = HideFlags.HideAndDontSave,
                mainTexture = YarnRopeTexture.Load()
            };
            effect.line = visual.AddComponent<LineRenderer>();
            effect.line.sharedMaterial = effect.material;
            effect.line.useWorldSpace = true;
            effect.line.positionCount = Segments;
            effect.line.textureMode = LineTextureMode.Tile;
            effect.line.numCornerVertices = 3;
            effect.line.numCapVertices = 3;
            effect.line.sortingLayerID = source.sortingLayerID;
            effect.line.sortingOrder = source.sortingOrder;
            effect.line.enabled = false;
            return effect;
        }

        public void Play(Vector2 shot, float selectedLength, float width, Color color)
        {
            Cancel();
            if (!isActiveAndEnabled || controller == null || !controller.isActiveAndEnabled ||
                controller.IsAttached || owner == null || !owner.simulated || Time.timeScale <= 0f ||
                shot.sqrMagnitude < .0001f) return;
            // Deliberately short: a failed shot must not look like it reached a distant Hook.
            direction = shot.normalized;
            length = Mathf.Min(2.1f, Mathf.Min(selectedLength, shot.magnitude) * .75f);
            if (length <= .01f) return;
            line.startWidth = line.endWidth = width;
            bool textured = material.mainTexture != null;
            line.startColor = line.endColor = textured ? new Color(1f, 1f, 1f, color.a) : color;
            if (textured)
            {
                Texture texture = material.mainTexture;
                line.textureScale = new Vector2(1f / Mathf.Max(.01f,
                    width * texture.width / texture.height), 1f);
            }
            elapsed = 0f;
            Draw();
        }

        private void LateUpdate() => Advance(Time.deltaTime);

        private void Advance(float deltaTime)
        {
            if (elapsed >= Duration) return;
            if (owner == null || !owner.simulated || controller == null ||
                !controller.isActiveAndEnabled || controller.IsAttached)
            {
                Cancel();
                return;
            }
            // A pause can start this frame while deltaTime still contains the old step.
            if (Time.timeScale <= 0f) return;
            elapsed = Mathf.Min(Duration, elapsed + Mathf.Max(0f, deltaTime));
            if (elapsed >= Duration) { Cancel(); return; }
            Draw();
        }

        private void Draw()
        {
            float extension = Mathf.SmoothStep(0f, 1f, elapsed / ExtendTime);
            float droop = Mathf.SmoothStep(0f, 1f,
                (elapsed - ExtendTime) / (ReturnStart - ExtendTime));
            float returning = Mathf.SmoothStep(0f, 1f,
                (elapsed - ReturnStart) / (Duration - ReturnStart));
            Vector2 origin = artwork != null
                ? Vector2.Lerp(artwork.RopeOrigin, artwork.HeadReturnPoint, returning)
                : owner.position;
            float visible = extension * (1f - returning);
            line.enabled = visible > .001f;
            for (int i = 0; i < Segments; i++)
            {
                // Trim the curved tip toward the head instead of shrinking the entire shape.
                float t = visible * i / (Segments - 1f);
                Vector2 offset = direction * (length * t) +
                    Vector2.down * (length * .32f * droop * t * t * t);
                line.SetPosition(i, origin + offset);
            }
        }

        public void Cancel()
        {
            elapsed = Duration;
            if (line != null) line.enabled = false;
        }

        private void OnDisable() => Cancel();
        private void OnDestroy()
        {
            if (material != null) Destroy(material);
        }
    }
}
