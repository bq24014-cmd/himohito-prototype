using UnityEngine;

namespace HimoHito
{
    /// <summary>Local foot pressure and ripples in the yarn renderer only; support stays immutable.</summary>
    [DefaultExecutionOrder(-60)] // Deform before the character follows the visible supporting yarn.
    public sealed class RopeBridgeStepVisual : MonoBehaviour
    {
        private const float Duration = 0.48f;
        private LineRenderer line;
        private Vector3[] resting, drawing;
        private readonly Vector2[] centers = new Vector2[4];
        private readonly float[] times = new float[4];
        private readonly float[] strengths = new float[4];
        private readonly bool[] rebounds = new bool[4];
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
            AddWave(platform, foot, .055f, false);
        }

        public static void PlayTakeoff(GeneratedRopePlatform platform, Vector2 foot)
        {
            RopeBridgeStepVisual effect = AddWave(platform, foot, .16f, true);
            if (effect != null) effect.lastPressureTime = float.NegativeInfinity;
        }

        public static void PlayLanding(GeneratedRopePlatform platform, Vector2 foot, float speed)
        {
            AddWave(platform, foot, Mathf.Lerp(.08f, .14f, Mathf.InverseLerp(3f, 12f, speed)), true);
        }

        private static RopeBridgeStepVisual AddWave(GeneratedRopePlatform platform, Vector2 foot,
            float strength, bool rebound)
        {
            RopeBridgeStepVisual effect = Prepare(platform);
            if (effect == null) return null;
            effect.centers[effect.next] = foot;
            effect.times[effect.next] = Time.time;
            effect.strengths[effect.next] = strength;
            effect.rebounds[effect.next] = rebound;
            effect.next = (effect.next + 1) % effect.times.Length;
            effect.active = true;
            return effect;
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
                System.Array.Copy(effect.resting, effect.drawing, effect.resting.Length);
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
            if (Time.timeScale <= 0f) return;
            Advance(Time.time, Time.deltaTime);
        }

        // Sample only the visual displacement, never alter the supporting EdgeCollider.
        public float GetVerticalOffset(Vector2 foot)
        {
            if (!active || resting == null || line == null ||
                GetComponent<RopeBridgeReveal>() != null || GetComponent<RopeBridgeMergeVisual>() != null) return 0f;
            float closest = float.PositiveInfinity, offset = 0f;
            for (int i = 0; i < resting.Length - 1; i++)
            {
                Vector3 a = line.useWorldSpace ? resting[i] : transform.TransformPoint(resting[i]);
                Vector3 b = line.useWorldSpace ? resting[i + 1] : transform.TransformPoint(resting[i + 1]);
                Vector2 span = b - a;
                float t = Mathf.Clamp01(Vector2.Dot(foot - (Vector2)a, span) / Mathf.Max(.0001f, span.sqrMagnitude));
                float distance = ((Vector2)Vector3.Lerp(a, b, t) - foot).sqrMagnitude;
                if (distance >= closest) continue;
                closest = distance;
                Vector3 delta = Vector3.Lerp(drawing[i] - resting[i], drawing[i + 1] - resting[i + 1], t);
                offset = (line.useWorldSpace ? delta : transform.TransformVector(delta)).y;
            }
            return offset;
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
            pressure = Mathf.MoveTowards(pressure, isPressed ? 1f : 0f, deltaTime / (isPressed ? .12f : .18f));
            if (isPressed) pressureFoot = Vector2.Lerp(pressureFoot, requestedFoot, 1f - Mathf.Exp(-22f * deltaTime));
            float load = Mathf.SmoothStep(0f, 1f, pressure);
            bool any = pressure > 0f;
            for (int i = 0; i < resting.Length; i++)
            {
                float dip = 0f;
                Vector3 point = line.useWorldSpace ? resting[i] : transform.TransformPoint(resting[i]);
                for (int j = 0; j < times.Length; j++)
                {
                    float distance = Vector2.Distance(point, centers[j]);
                    float age = now - times[j];
                    float duration = rebounds[j] ? .8f : Duration;
                    // The small travelling delay lets the bend spread out from the foot.
                    if (age >= 0f && age < duration + .3f) any = true;
                    float t = (age - Mathf.Min(.3f, distance / 5f)) / duration;
                    if (t < 0f || t >= 1f) continue;
                    float proximity = Mathf.Clamp01(1f - distance / (rebounds[j] ? 2.4f : 1.8f));
                    float wave = rebounds[j]
                        ? Mathf.Sin(t * Mathf.PI * 2f) * (1f - t) * (1f - t)
                        : Mathf.Sin(t * Mathf.PI);
                    dip += strengths[j] * wave * proximity * proximity;
                }
                // Fixed ends; the player artwork follows this dip while physics stays fixed.
                float endFade = Mathf.Sin(i / (float)(resting.Length - 1) * Mathf.PI);
                if (i == 0 || i == resting.Length - 1) endFade = 0f;
                float localPressure = Mathf.Clamp01(1f - Vector2.Distance(point, pressureFoot) / 1.6f);
                localPressure = localPressure * localPressure * load * endFade;
                dip += .10f * localPressure;
                Vector3 offset = Vector3.down * Mathf.Clamp(dip, -.065f, .18f) * endFade;
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
