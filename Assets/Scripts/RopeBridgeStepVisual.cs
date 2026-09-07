using UnityEngine;

namespace HimoHito
{
    /// <summary>Footstep ripples in the yarn renderer only. The support curve is immutable.</summary>
    public sealed class RopeBridgeStepVisual : MonoBehaviour
    {
        private const float Duration = 0.34f;
        private LineRenderer line;
        private Vector3[] resting, drawing;
        private readonly Vector2[] centers = new Vector2[4];
        private readonly float[] times = new float[4];
        private int next;
        private bool active;

        public static void Play(GeneratedRopePlatform platform, Vector2 foot)
        {
            if (platform == null || platform.GetComponent<RopeBridgeReveal>() != null ||
                platform.GetComponent<RopeBridgeMergeVisual>() != null) return;
            if (!platform.TryGetComponent(out RopeBridgeStepVisual effect))
                effect = platform.gameObject.AddComponent<RopeBridgeStepVisual>();
            if (effect.line == null)
            {
                effect.line = platform.GetComponent<LineRenderer>();
                if (effect.line == null) return;
                effect.resting = new Vector3[effect.line.positionCount];
                effect.drawing = new Vector3[effect.line.positionCount];
                effect.line.GetPositions(effect.resting);
                for (int i = 0; i < effect.times.Length; i++)
                    effect.times[i] = float.NegativeInfinity;
            }
            effect.centers[effect.next] = foot;
            effect.times[effect.next] = Time.time;
            effect.next = (effect.next + 1) % effect.times.Length;
            effect.active = true;
        }

        private void LateUpdate()
        {
            if (!active || line == null) return;
            if (GetComponent<RopeBridgeReveal>() != null || GetComponent<RopeBridgeMergeVisual>() != null)
            {
                active = false; // The reveal/merge animation owns this renderer now.
                for (int i = 0; i < times.Length; i++) times[i] = float.NegativeInfinity;
                return;
            }
            bool any = false;
            for (int i = 0; i < resting.Length; i++)
            {
                float dip = 0f;
                Vector3 point = line.useWorldSpace ? resting[i] : transform.TransformPoint(resting[i]);
                for (int j = 0; j < times.Length; j++)
                {
                    float t = (Time.time - times[j]) / Duration;
                    if (t < 0f || t >= 1f) continue;
                    any = true;
                    float proximity = Mathf.Clamp01(1f - Vector2.Distance(point, centers[j]) / 1.1f);
                    float wave = Mathf.Sin(t * Mathf.PI);
                    dip += 0.035f * wave * wave * proximity * proximity;
                }
                // Fixed ends and a hard visual displacement cap avoid visible foot separation.
                float endFade = Mathf.Sin(i / (float)(resting.Length - 1) * Mathf.PI);
                if (i == 0 || i == resting.Length - 1) endFade = 0f;
                Vector3 offset = Vector3.down * Mathf.Min(0.04f, dip) * endFade;
                drawing[i] = resting[i] + (line.useWorldSpace ? offset : transform.InverseTransformVector(offset));
            }
            line.SetPositions(drawing);
            active = any;
        }

        private void OnDisable()
        {
            if (active && line != null && resting != null &&
                GetComponent<RopeBridgeMergeVisual>() == null)
                line.SetPositions(resting);
            active = false;
        }
    }
}
