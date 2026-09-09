using System.Collections.Generic;
using UnityEngine;

namespace HimoHito
{
    /// <summary>Small sewn brows follow the eyes of the live pose; resource and physics are read-only.</summary>
    [DisallowMultipleComponent, DefaultExecutionOrder(210)]
    public sealed class RopeResourceExpression : MonoBehaviour
    {
        private readonly List<SpriteRenderer> renderers = new();
        private readonly LineRenderer[] brows = new LineRenderer[2];
        private readonly Vector3[] points = new Vector3[9];
        private RopeBodyVisual artwork;
        private RopeResource resource;
        private Rigidbody2D body;
        private Material material;
        private float level;
        private int state;

        private void Awake()
        {
            artwork = GetComponent<RopeBodyVisual>();
            resource = GetComponent<RopeResource>();
            body = GetComponent<Rigidbody2D>();
        }

        private void LateUpdate() => Advance(Time.deltaTime);
        private void Advance(float dt)
        {
            if (artwork == null || !artwork.enabled || resource == null || body == null || !body.simulated)
            {
                Hide(); level = 0f; state = 0; return;
            }
            if (dt <= 0f) return;
            float ratio = resource.NormalizedLength;
            // Small hysteresis avoids toggling the expression at a boundary.
            if (state == 2 && ratio > .14f) state = ratio > .34f ? 0 : 1;
            else if (state == 1 && ratio > .34f) state = 0;
            if (ratio <= .10f) state = 2;
            else if (ratio <= .30f && state == 0) state = 1;
            level = Mathf.MoveTowards(level, state, dt / .35f);
            renderers.Clear(); artwork.CollectCharacterRenderers(renderers);
            SpriteRenderer face = renderers.Count > 0 ? renderers[0] : null;
            if (level <= .001f || face == null || !face.enabled || face.forceRenderingOff || face.sprite == null ||
                (face.sharedMaterial != null && face.sharedMaterial.HasProperty("_WeaveProgress")) ||
                !RopeFaceLandmarks.TryGet(face.sprite, out var eyes) || eyes.Length == 0)
            { Hide(); return; }
            EnsureBrows(face.transform);
            if (material == null) return;
            float worry = Mathf.Clamp01(level), strain = Mathf.Clamp01(level - 1f);
            for (int i = 0; i < brows.Length; i++)
            {
                LineRenderer line = brows[i];
                line.enabled = i < eyes.Length;
                if (!line.enabled) continue;
                if (line.transform.parent != face.transform) line.transform.SetParent(face.transform, false);
                var eye = eyes[i];
                float inner = i == 0 ? 1f : -1f;
                float tilt = Mathf.Lerp(.48f, -.55f, strain) * inner;
                float lift = Mathf.Max(eye.Height * .27f, eye.Width * .22f);
                for (int p = 0; p < points.Length; p++)
                {
                    float t = p / (points.Length - 1f), x = (t - .5f) * eye.Width * 1.05f;
                    float y = lift + x * tilt + Mathf.Sin(t * Mathf.PI) * eye.Width * .075f;
                    Vector3 point = eye.Top + new Vector2(x, y);
                    if (face.flipX) point.x = -point.x;
                    if (face.flipY) point.y = -point.y;
                    points[p] = point;
                }
                line.SetPositions(points);
                float width = face.transform.TransformVector(Vector3.up * eye.Width * .30f).magnitude;
                line.widthMultiplier = Mathf.Clamp(width, .012f, .025f);
                line.sortingLayerID = face.sortingLayerID; line.sortingOrder = face.sortingOrder + 1;
                Color ink = new Color(.24f, .055f, .18f, worry * face.color.a * .94f);
                line.startColor = line.endColor = ink;
            }
        }

        private void EnsureBrows(Transform parent)
        {
            if (material == null)
            {
                Shader shader = Shader.Find("Sprites/Default");
                if (shader == null) return;
                material = new Material(shader) { name = "Sewn Expression Brows", hideFlags = HideFlags.HideAndDontSave };
            }
            for (int i = 0; i < brows.Length; i++)
            {
                if (brows[i] != null) continue;
                var root = new GameObject("Resource Expression Brow " + i);
                root.transform.SetParent(parent, false);
                brows[i] = root.AddComponent<LineRenderer>();
                brows[i].sharedMaterial = material;
                brows[i].useWorldSpace = false;
                brows[i].positionCount = points.Length;
                brows[i].numCapVertices = brows[i].numCornerVertices = 4;
                brows[i].widthCurve = new AnimationCurve(new Keyframe(0f, .55f),
                    new Keyframe(.5f, 1f), new Keyframe(1f, .55f));
            }
        }

        private void Hide() { foreach (LineRenderer line in brows) if (line != null) line.enabled = false; }
        private void OnDisable() { Hide(); level = 0f; state = 0; }
        private void OnDestroy()
        {
            foreach (LineRenderer line in brows) if (line != null) Dispose(line.gameObject);
            Dispose(material);
        }
        private static void Dispose(Object value)
        {
            if (value == null) return;
            if (Application.isPlaying) Destroy(value); else DestroyImmediate(value);
        }
    }
}
