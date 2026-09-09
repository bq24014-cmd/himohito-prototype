using UnityEngine;

namespace HimoHito
{
    /// <summary>Local foot pressure and ripples in the yarn renderer only; support stays immutable.</summary>
    public sealed class RopeBridgeStepVisual : MonoBehaviour
    {
        private const float Duration = 0.34f;
        private LineRenderer line;
        private Vector3[] resting, drawing;
        private readonly Vector2[] centers = new Vector2[4];
        private readonly float[] times = new float[4];
        private int next;
        private bool active;
        private AnimationCurve restingWidth, pressedWidth;
        private Keyframe[] widthKeys;
        private float[] distances;
        private float widthMultiplier;
        private Vector2 pressureFoot, requestedFoot;
        private float pressure, lastPressureTime = float.NegativeInfinity;

        // Called only for grounded support; never changes the physics curve or contact state.
        public static void Press(GeneratedRopePlatform platform, Vector2 foot)
        {
            RopeBridgeStepVisual effect = Prepare(platform);
            if (effect == null) return;
            if (effect.pressure <= 0f) effect.pressureFoot = foot;
            effect.requestedFoot = foot;
            effect.lastPressureTime = Time.time;
            effect.active = true;
        }

        public static void Play(GeneratedRopePlatform platform, Vector2 foot)
        {
            RopeBridgeStepVisual effect = Prepare(platform);
            if (effect == null) return;
            effect.centers[effect.next] = foot;
            effect.times[effect.next] = Time.time;
            effect.next = (effect.next + 1) % effect.times.Length;
            effect.active = true;
        }

        private static RopeBridgeStepVisual Prepare(GeneratedRopePlatform platform)
        {
            if (platform == null || platform.GetComponent<RopeBridgeReveal>() != null ||
                platform.GetComponent<RopeBridgeMergeVisual>() != null) return null;
            if (!platform.TryGetComponent(out RopeBridgeStepVisual effect))
                effect = platform.gameObject.AddComponent<RopeBridgeStepVisual>();
            if (!effect.isActiveAndEnabled) return null;
            if (effect.resting == null)
            {
                effect.line = platform.GetComponent<LineRenderer>();
                if (effect.line == null || effect.line.positionCount < 2) return null;
                effect.resting = new Vector3[effect.line.positionCount];
                effect.drawing = new Vector3[effect.line.positionCount];
                effect.line.GetPositions(effect.resting);
                effect.restingWidth = effect.line.widthCurve;
                effect.widthMultiplier = effect.line.widthMultiplier;
                effect.pressedWidth = new AnimationCurve();
                effect.widthKeys = new Keyframe[effect.resting.Length];
                effect.distances = new float[effect.resting.Length];
                float length = 0f;
                for (int i = 1; i < effect.resting.Length; i++)
                {
                    length += Vector3.Distance(effect.resting[i - 1], effect.resting[i]);
                    effect.distances[i] = length;
                }
                for (int i = 0; i < effect.distances.Length; i++)
                    effect.distances[i] = length > .0001f ? effect.distances[i] / length : i / (float)(effect.distances.Length - 1);
                for (int i = 0; i < effect.times.Length; i++)
                    effect.times[i] = float.NegativeInfinity;
            }
            return effect;
        }

        private void LateUpdate()
        {
            Advance(Time.time, Time.deltaTime);
        }

        private void Advance(float now, float deltaTime)
        {
            if (!active || line == null) return;
            if (GetComponent<RopeBridgeReveal>() != null || GetComponent<RopeBridgeMergeVisual>() != null)
            {
                active = false; // The reveal/merge animation owns this renderer now.
                for (int i = 0; i < times.Length; i++) times[i] = float.NegativeInfinity;
                line.widthCurve = restingWidth;
                line.widthMultiplier = widthMultiplier;
                resting = null; // Recapture the completed curve on the next actual step.
                pressure = 0f;
                lastPressureTime = float.NegativeInfinity;
                return;
            }
            if (deltaTime <= 0f) return;
            if (line.positionCount != resting.Length) { OnDisable(); return; }
            bool isPressed = now - lastPressureTime <= Mathf.Max(.06f, Time.fixedDeltaTime * 2.5f);
            pressure = Mathf.MoveTowards(pressure, isPressed ? 1f : 0f, deltaTime / (isPressed ? .12f : .28f));
            if (isPressed) pressureFoot = Vector2.Lerp(pressureFoot, requestedFoot, 1f - Mathf.Exp(-22f * deltaTime));
            float load = Mathf.SmoothStep(0f, 1f, pressure);
            bool any = pressure > 0f;
            for (int i = 0; i < resting.Length; i++)
            {
                float dip = 0f;
                Vector3 point = line.useWorldSpace ? resting[i] : transform.TransformPoint(resting[i]);
                for (int j = 0; j < times.Length; j++)
                {
                    float t = (now - times[j]) / Duration;
                    if (t < 0f || t >= 1f) continue;
                    any = true;
                    float proximity = Mathf.Clamp01(1f - Vector2.Distance(point, centers[j]) / 1.1f);
                    float wave = Mathf.Sin(t * Mathf.PI);
                    dip += 0.035f * wave * wave * proximity * proximity;
                }
                // Fixed ends and a hard visual displacement cap avoid visible foot separation.
                float endFade = Mathf.Sin(i / (float)(resting.Length - 1) * Mathf.PI);
                if (i == 0 || i == resting.Length - 1) endFade = 0f;
                float localPressure = Mathf.Clamp01(1f - Vector2.Distance(point, pressureFoot) / .85f);
                localPressure = localPressure * localPressure * load * endFade;
                dip += .025f * localPressure;
                Vector3 offset = Vector3.down * Mathf.Min(0.04f, dip) * endFade;
                drawing[i] = resting[i] + (line.useWorldSpace ? offset : transform.InverseTransformVector(offset));
                widthKeys[i] = new Keyframe(distances[i], restingWidth.Evaluate(distances[i]) * (1f - .14f * localPressure));
            }
            line.SetPositions(drawing);
            pressedWidth.keys = widthKeys;
            line.widthCurve = any ? pressedWidth : restingWidth;
            line.widthMultiplier = widthMultiplier;
            active = any;
        }

        private void OnDisable()
        {
            if (active && line != null && resting != null &&
                line.positionCount == resting.Length &&
                GetComponent<RopeBridgeReveal>() == null && GetComponent<RopeBridgeMergeVisual>() == null)
            {
                line.SetPositions(resting);
                line.widthCurve = restingWidth;
                line.widthMultiplier = widthMultiplier;
            }
            active = false;
            pressure = 0f;
            lastPressureTime = float.NegativeInfinity;
            resting = null;
        }
    }
}
