using UnityEngine;

namespace HimoHito
{
    /// <summary>Q-only visual reveal over a faint outline of the complete supporting bridge.</summary>
    public sealed class RopeBridgeReveal : MonoBehaviour
    {
        private const float Duration = 0.3f;
        private LineRenderer bridge;
        private LineRenderer growing;
        private Vector3[] points;
        private Color startColor;
        private Color endColor;
        private float elapsed;

        public static void Play(LineRenderer line, Vector3 playerPosition)
        {
            if (line == null || line.positionCount < 2) return;
            RopeBridgeReveal effect = line.gameObject.AddComponent<RopeBridgeReveal>();
            effect.bridge = line;
            effect.startColor = line.startColor;
            effect.endColor = line.endColor;
            effect.points = new Vector3[line.positionCount];
            line.GetPositions(effect.points);
            Vector3 first = line.useWorldSpace ? effect.points[0] : line.transform.TransformPoint(effect.points[0]);
            Vector3 last = line.useWorldSpace ? effect.points[effect.points.Length - 1]
                : line.transform.TransformPoint(effect.points[effect.points.Length - 1]);
            bool reverse = (playerPosition - last).sqrMagnitude < (playerPosition - first).sqrMagnitude;
            if (reverse) System.Array.Reverse(effect.points);

            GameObject artwork = new GameObject("Weaving Rope Artwork");
            artwork.transform.SetParent(line.transform, false);
            effect.growing = artwork.AddComponent<LineRenderer>();
            effect.growing.sharedMaterial = line.sharedMaterial;
            effect.growing.useWorldSpace = line.useWorldSpace;
            effect.growing.startWidth = reverse ? line.endWidth : line.startWidth;
            effect.growing.endWidth = reverse ? line.startWidth : line.endWidth;
            effect.growing.startColor = reverse ? line.endColor : line.startColor;
            effect.growing.endColor = reverse ? line.startColor : line.endColor;
            effect.growing.textureMode = line.textureMode;
            effect.growing.textureScale = line.textureScale;
            effect.growing.sortingLayerID = line.sortingLayerID;
            effect.growing.sortingOrder = line.sortingOrder + 1;
            Color faintStart = line.startColor;
            Color faintEnd = line.endColor;
            faintStart.a *= 0.25f;
            faintEnd.a *= 0.25f;
            line.startColor = faintStart;
            line.endColor = faintEnd;
            effect.Draw(0f);
        }

        private void Update()
        {
            elapsed += Time.deltaTime;
            if (bridge == null || elapsed >= Duration)
            {
                Restore();
                Destroy(this);
                return;
            }
            Draw(Mathf.SmoothStep(0f, 1f, elapsed / Duration));
        }

        private void Draw(float progress)
        {
            float sample = progress * (points.Length - 1);
            int index = Mathf.Min(Mathf.FloorToInt(sample), points.Length - 2);
            growing.positionCount = index + 2;
            for (int i = 0; i <= index; i++) growing.SetPosition(i, points[i]);
            growing.SetPosition(index + 1, Vector3.Lerp(points[index], points[index + 1], sample - index));
        }

        private void OnDisable() => Restore();

        private void Restore()
        {
            if (bridge != null)
            {
                bridge.startColor = startColor;
                bridge.endColor = endColor;
            }
            if (growing != null)
            {
                growing.enabled = false;
                Destroy(growing.gameObject);
                growing = null;
            }
            enabled = false;
        }
    }
}
