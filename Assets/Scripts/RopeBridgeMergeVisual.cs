using UnityEngine;

namespace HimoHito
{
    /// <summary>Settles the old two-bridge silhouette into the merged bridge; physics is unchanged.</summary>
    public sealed class RopeBridgeMergeVisual : MonoBehaviour
    {
        private const float Duration = 0.4f;
        private LineRenderer line;
        private GameObject guide;
        private Vector3[] finalPoints;
        private Vector3[] oldPoints;
        private Vector3[] drawing;
        private float elapsed;

        public static void Play(LineRenderer bridge, Vector2[] first, Vector2[] second)
        {
            if (bridge == null || bridge.positionCount < 2) return;
            RopeBridgeMergeVisual effect = bridge.gameObject.AddComponent<RopeBridgeMergeVisual>();
            effect.line = bridge;
            effect.finalPoints = new Vector3[bridge.positionCount];
            bridge.GetPositions(effect.finalPoints);
            effect.oldPoints = new Vector3[bridge.positionCount];
            effect.drawing = new Vector3[bridge.positionCount];
            Vector2 outerStart = first[0];
            Vector2 span = second[second.Length - 1] - outerStart;
            float split = Mathf.Clamp(Vector2.Dot(first[first.Length - 1] - outerStart, span) /
                Mathf.Max(0.0001f, span.sqrMagnitude), 0.01f, 0.99f);
            for (int i = 0; i < effect.oldPoints.Length; i++)
            {
                float t = i / (float)(effect.oldPoints.Length - 1);
                effect.oldPoints[i] = t <= split ? Sample(first, t / split)
                    : Sample(second, (t - split) / (1f - split));
            }

            // The faint final curve keeps the immediately available support visible.
            effect.guide = new GameObject("Merged Bridge Support Outline");
            effect.guide.transform.SetParent(bridge.transform, false);
            LineRenderer outline = effect.guide.AddComponent<LineRenderer>();
            outline.sharedMaterial = bridge.sharedMaterial;
            outline.useWorldSpace = bridge.useWorldSpace;
            outline.positionCount = effect.finalPoints.Length;
            outline.SetPositions(effect.finalPoints);
            outline.startWidth = bridge.startWidth;
            outline.endWidth = bridge.endWidth;
            outline.textureMode = bridge.textureMode;
            outline.textureScale = bridge.textureScale;
            outline.sortingLayerID = bridge.sortingLayerID;
            outline.sortingOrder = bridge.sortingOrder;
            Color tint = bridge.startColor;
            tint.a *= 0.25f;
            outline.startColor = outline.endColor = tint;
            bridge.SetPositions(effect.oldPoints);
        }

        private static Vector3 Sample(Vector2[] curve, float t)
        {
            float sample = Mathf.Clamp01(t) * (curve.Length - 1);
            int index = Mathf.Min(Mathf.FloorToInt(sample), curve.Length - 2);
            return Vector2.Lerp(curve[index], curve[index + 1], sample - index);
        }

        private void Update()
        {
            elapsed += Time.deltaTime;
            if (line == null || elapsed >= Duration)
            {
                Restore();
                Destroy(this);
                return;
            }
            float t = Mathf.SmoothStep(0f, 1f, elapsed / Duration);
            for (int i = 0; i < drawing.Length; i++)
                drawing[i] = Vector3.Lerp(oldPoints[i], finalPoints[i], t);
            line.SetPositions(drawing);
        }

        private void OnDisable() => Restore();

        private void Restore()
        {
            if (line != null && finalPoints != null) line.SetPositions(finalPoints);
            if (guide != null)
            {
                guide.SetActive(false);
                Destroy(guide);
                guide = null;
            }
            enabled = false;
        }
    }
}
