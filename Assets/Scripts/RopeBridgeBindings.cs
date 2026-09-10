using UnityEngine;

namespace HimoHito
{
    /// <summary>Yarn wound around bridge-end rings. Never changes supporting geometry.</summary>
    [DisallowMultipleComponent, DefaultExecutionOrder(130)]
    public sealed class RopeBridgeBindings : MonoBehaviour
    {
        private const int Segments = 48;
        private const float TightenDuration = .22f;
        private const float ReleaseDuration = .4f;
        private LineRenderer bridge;
        private readonly LineRenderer[] wraps = new LineRenderer[2];
        private readonly float[] radii = { .30f, .30f };
        private float width, tieElapsed = 1f, tieDelay;
        private Color tint;
        private Release[] releases;
        private float releaseElapsed;
        private GeneratedRopePlatform platform;
        private readonly float[] loads = new float[2];

        private struct Release
        {
            public LineRenderer Line;
            public Vector3 Anchor, Direction;
            public float Radius, Tightness, AlongBridge, Load;
        }

        public static RopeBridgeBindings Ensure(LineRenderer source)
        {
            if (source == null || source.positionCount < 2) return null;
            if (!source.TryGetComponent(out RopeBridgeBindings effect))
                effect = source.gameObject.AddComponent<RopeBridgeBindings>();
            if (effect.bridge != null) return effect;
            effect.bridge = source;
            effect.platform = source.GetComponent<GeneratedRopePlatform>();
            effect.width = Mathf.Clamp(source.startWidth * .5f, .045f, .075f);
            effect.tint = source.startColor;
            // Only once when constructing a bridge, never during normal frame updates.
            HookPoint[] hooks = Object.FindObjectsByType<HookPoint>(FindObjectsSortMode.None);
            for (int i = 0; i < 2; i++)
            {
                Vector3 anchor = effect.Endpoint(i);
                foreach (HookPoint hook in hooks)
                {
                    if (Vector2.Distance(hook.GetAttachmentPoint(hook.transform.position), anchor) > .06f) continue;
                    var blue = hook.GetComponentInChildren<HimoHitoHookRingVisual>();
                    var green = hook.GetComponentInChildren<RopeAnchorRingVisual>();
                    var ring = blue != null ? blue.GetComponent<SpriteRenderer>() :
                        green != null ? green.GetComponent<SpriteRenderer>() : null;
                    if (ring != null)
                        effect.radii[i] = Mathf.Clamp(Mathf.Max(ring.bounds.extents.x, ring.bounds.extents.y), .15f, .6f);
                    break;
                }
                effect.wraps[i] = effect.NewLine(i == 0 ? "Start Hook Winding" : "End Hook Winding");
            }
            effect.Advance(0f);
            return effect;
        }

        public void BeginWeave(float revealDuration)
        {
            tieDelay = Mathf.Max(0f, revealDuration);
            tieElapsed = 0f;
            loads[0] = loads[1] = 0f;
            Advance(0f);
        }

        public void ReleaseCenter(RopeBridgeBindings first, RopeBridgeBindings second, Vector2 center)
        {
            ClearReleases();
            if (first == null || second == null) return;
            releases = new[] { Capture(first, center), Capture(second, center) };
            releaseElapsed = 0f;
            Advance(0f);
        }

        private Release Capture(RopeBridgeBindings old, Vector2 center)
        {
            int index = Vector2.Distance(old.Endpoint(0), center) <= Vector2.Distance(old.Endpoint(1), center) ? 0 : 1;
            Vector3 span = Endpoint(1) - Endpoint(0);
            return new Release
            {
                Line = NewLine("Unwinding Central Hook"),
                Anchor = old.Endpoint(index), Direction = old.Direction(index), Radius = old.radii[index],
                Tightness = Mathf.Clamp01((old.tieElapsed - old.tieDelay) / TightenDuration),
                Load = old.loads[index],
                AlongBridge = Mathf.Clamp01(Vector3.Dot((Vector3)center - Endpoint(0), span) / Mathf.Max(.0001f, span.sqrMagnitude))
            };
        }

        private void LateUpdate()
        {
            if (Time.timeScale > 0f) Advance(Time.deltaTime);
        }

        private void Advance(float delta)
        {
            if (bridge == null) return;
            tieElapsed = Mathf.Min(tieDelay + TightenDuration, tieElapsed + Mathf.Max(0f, delta));
            float tightness = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(tieDelay, tieDelay + TightenDuration, tieElapsed));
            Vector2 foot = default;
            bool supported = tightness >= 1f && releases == null && platform != null &&
                GetComponent<RopeBridgeReveal>() == null && GetComponent<RopeBridgeMergeVisual>() == null &&
                platform.TryGetSupportedFoot(out foot);
            Vector2 span = Endpoint(1) - Endpoint(0);
            float position = supported ? Mathf.Clamp01(Vector2.Dot(foot - (Vector2)Endpoint(0), span) /
                Mathf.Max(.0001f, span.sqrMagnitude)) : .5f;
            for (int i = 0; i < wraps.Length; i++)
            {
                float target = supported ? (i == 0 ? 1f - position : position) : 0f;
                loads[i] = Mathf.Lerp(loads[i], target, 1f - Mathf.Exp(-8f * Mathf.Max(0f, delta)));
                Draw(wraps[i], Endpoint(i), Direction(i), radii[i], tightness, tightness, 0f, loads[i]);
            }
            if (releases == null) return;
            releaseElapsed += Mathf.Max(0f, delta);
            float loosen = Mathf.SmoothStep(0f, 1f, releaseElapsed / ReleaseDuration);
            foreach (Release r in releases)
            {
                Vector3 target = Sample(r.AlongBridge);
                // The merge renderer already lowers this point. Follow it directly, without a second lag.
                Draw(r.Line, target, r.Direction,
                    r.Radius * (1f - loosen), r.Tightness, (1f - loosen) * r.Tightness, loosen,
                    r.Load * (1f - loosen));
            }
            if (releaseElapsed >= ReleaseDuration) ClearReleases();
        }

        private void Draw(LineRenderer line, Vector3 anchor, Vector3 along, float radius,
            float tightness, float alpha, float unwind, float load = 0f)
        {
            if (line == null) return;
            line.enabled = bridge.enabled && alpha > .001f;
            Color color = tint; color.a *= alpha;
            line.startColor = line.endColor = color;
            line.startWidth = line.endWidth = width * (1f - .10f * load);
            Vector3 across = new Vector3(-along.y, along.x, 0f);
            float looseSize = Mathf.Lerp(1.55f, 1f, tightness);
            float turns = 2.1f * (1f - unwind);
            for (int i = 0; i < Segments; i++)
            {
                float t = i / (Segments - 1f);
                float angle = t * Mathf.PI * 2f * turns;
                Vector3 offset = along * (radius * (.82f + .10f * load) + (t - .5f) * radius * .5f + Mathf.Cos(angle) * radius * .12f) +
                    across * (Mathf.Sin(angle) * radius * .36f * looseSize * (1f - .30f * load));
                line.SetPosition(i, anchor + offset);
            }
        }

        private Vector3 Point(int index)
        {
            Vector3 p = bridge.GetPosition(index);
            return bridge.useWorldSpace ? p : bridge.transform.TransformPoint(p);
        }
        private Vector3 Endpoint(int index) => Point(index == 0 ? 0 : bridge.positionCount - 1);
        private Vector3 Direction(int index)
        {
            Vector3 d = Point(index == 0 ? 1 : bridge.positionCount - 2) - Endpoint(index);
            return d.sqrMagnitude > .00001f ? d.normalized : Vector3.down;
        }
        private Vector3 Sample(float t)
        {
            float f = t * (bridge.positionCount - 1);
            int i = Mathf.Min(Mathf.FloorToInt(f), bridge.positionCount - 2);
            return Vector3.Lerp(Point(i), Point(i + 1), f - i);
        }

        private LineRenderer NewLine(string label)
        {
            var child = new GameObject(label);
            child.transform.SetParent(transform, false);
            var line = child.AddComponent<LineRenderer>();
            line.sharedMaterial = bridge.sharedMaterial;
            line.useWorldSpace = true;
            line.positionCount = Segments;
            line.startWidth = line.endWidth = width;
            line.textureMode = LineTextureMode.Tile;
            Texture texture = bridge.sharedMaterial != null ? bridge.sharedMaterial.mainTexture : null;
            if (texture != null) line.textureScale = new Vector2(1f / (width * texture.width / texture.height), 1f);
            line.sortingLayerID = bridge.sortingLayerID;
            line.sortingOrder = Mathf.Max(10, bridge.sortingOrder + 2);
            line.numCapVertices = line.numCornerVertices = 3;
            return line;
        }

        private void OnDisable()
        {
            loads[0] = loads[1] = 0f;
            foreach (var line in wraps) if (line != null) line.enabled = false;
            ClearReleases();
        }
        private void OnDestroy()
        {
            foreach (var line in wraps) if (line != null) Dispose(line.gameObject);
            ClearReleases();
        }
        private void ClearReleases()
        {
            if (releases == null) return;
            foreach (Release r in releases)
                if (r.Line != null) { r.Line.enabled = false; Dispose(r.Line.gameObject); }
            releases = null;
        }
        private static void Dispose(Object value)
        {
            if (Application.isPlaying) Destroy(value);
            else DestroyImmediate(value);
        }
    }
}
