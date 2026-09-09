using System;
using System.Collections.Generic;
using UnityEngine;

namespace HimoHito
{
    /// <summary>
    /// Q permanently turns the connected rope into a sagging bridge. F removes
    /// the aimed hook when two generated bridges meet there and joins both arcs.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    [RequireComponent(typeof(Rigidbody2D), typeof(RopeResource), typeof(RopeController))]
    public sealed class RopePlatformBuilder : MonoBehaviour
    {
        private const float StandingClearance = 0.005f;
        private const float SharedEndpointTolerance = 0.35f;
        private const int MinimumSmoothCurveSegments = 40;

        [Serializable]
        public readonly struct PlatformState
        {
            public PlatformState(Vector2 start, Vector2 end, float ropeLength)
            {
                Start = start;
                End = end;
                RopeLength = ropeLength;
            }

            public Vector2 Start { get; }
            public Vector2 End { get; }
            public float RopeLength { get; }
        }

        [SerializeField, Min(0.05f)] private float platformWidth = 0.22f;
        [SerializeField, Range(5, 48)] private int curveSegments = 20;
        [SerializeField, Min(0.1f)] private float minimumPlatformLength = 1f;
        [SerializeField, Min(0.01f)] private float minimumRopeReserve = 1f;

        private readonly List<GameObject> generatedPlatforms = new();
        private readonly List<GameObject> removedHooks = new();
        private Rigidbody2D body;
        private Collider2D bodyCollider;
        private RopeResource ropeResource;
        private RopeController ropeController;
        private PrototypeAudioFeedback audioFeedback;

        public float CurrentPlatformCost =>
            IsPlatformBuildingUnlocked &&
            ropeController != null && ropeController.IsAttached
                ? GetCurrentPlatformCost()
                : 0f;
        public float MinimumRopeReserve => minimumRopeReserve;
        public int GeneratedPlatformCount => generatedPlatforms.Count;
        public bool IsPlatformBuildingUnlocked
        {
            get
            {
                PrototypeRunController tutorial = GetComponent<PrototypeRunController>();
                if (tutorial != null && gameObject.scene.name == "Tutorial")
                {
                    return tutorial.CurrentTutorialSection >= 3;
                }

                MainStageRespawnOnFall main = GetComponent<MainStageRespawnOnFall>();
                return main == null || main.CurrentSection >= 4;
            }
        }
        public bool CanBuildCurrentPlatform => CanBuild(out _, out _, out _);

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            bodyCollider = GetComponent<Collider2D>();
            ropeResource = GetComponent<RopeResource>();
            ropeController = GetComponent<RopeController>();
            audioFeedback = GetComponent<PrototypeAudioFeedback>();
            if (audioFeedback == null)
            {
                audioFeedback = gameObject.AddComponent<PrototypeAudioFeedback>();
            }
        }

        private void Update()
        {
            if (MainStagePreview.IsActive) return;
            // Failure presentation must not accept Q/F while simulation is stopped.
            if (body != null && !body.simulated) return;

            if (Input.GetKeyDown(KeyCode.Q) && IsPlatformBuildingUnlocked)
            {
                TryBuildCurrentPlatform();
            }

            if (Input.GetKeyDown(KeyCode.F) && IsHookRemovalUnlocked())
            {
                TryRemoveAimedHook();
            }
        }

        public bool TryBuildCurrentPlatform()
        {
            if (!CanBuild(out float ropeLength, out Vector2 start, out Vector2 end) ||
                !ropeController.CommitAttachedRopeAsPlatform(ropeLength))
            {
                return false;
            }

            GeneratedRopePlatform platform = CreatePlatform(start, end, ropeLength);
            RopeResourceGauge.NotifyPlatformBuilt(ropeResource);
            PlacePlayerOnPlatform(platform);
            if (TryGetComponent(out PlayerMover mover))
            {
                mover.RegisterGeneratedRopePlatformContact(platform);
            }
            audioFeedback?.PlayRopePlatformBuilt();
            RopeBuildFluff.Play(platform.GetComponent<LineRenderer>());
            RopeBridgeReveal.Play(platform.GetComponent<LineRenderer>(), transform.position);
            GetComponent<RopeBodyVisual>()?.PlayWeavePose();
            return true;
        }

        public bool TryRemoveAimedHook()
        {
            if (ropeController == null || ropeController.IsAttached ||
                !ropeController.TryResolveCurrentAimHook(out HookPoint hook, out Vector2 anchor))
            {
                return false;
            }

            List<GeneratedRopePlatform> joined = new(2);
            foreach (GameObject platformObject in generatedPlatforms)
            {
                if (platformObject != null &&
                    platformObject.TryGetComponent(out GeneratedRopePlatform platform) &&
                    platform.Touches(anchor, SharedEndpointTolerance))
                {
                    joined.Add(platform);
                }
            }

            if (joined.Count != 2)
            {
                return false;
            }

            Vector2 firstOuter = joined[0].GetOtherEndpoint(anchor);
            Vector2 secondOuter = joined[1].GetOtherEndpoint(anchor);
            float combinedLength = joined[0].RopeLength + joined[1].RopeLength;
            Vector2[] firstCurve = BuildSaggingCurve(firstOuter, anchor, joined[0].RopeLength);
            Vector2[] secondCurve = BuildSaggingCurve(anchor, secondOuter, joined[1].RopeLength);
            RemovePlatform(joined[0].gameObject);
            RemovePlatform(joined[1].gameObject);
            GeneratedRopePlatform merged = CreatePlatform(firstOuter, secondOuter, combinedLength);
            RopeBridgeMergeVisual.Play(merged.GetComponent<LineRenderer>(), firstCurve, secondCurve);
            if (!removedHooks.Contains(hook.gameObject))
            {
                removedHooks.Add(hook.gameObject);
            }
            hook.gameObject.SetActive(false);
            audioFeedback?.PlayRopePlatformsMerged();
            return true;
        }

        public PlatformState[] CapturePlatformStates()
        {
            List<PlatformState> states = new(generatedPlatforms.Count);
            foreach (GameObject platformObject in generatedPlatforms)
            {
                if (platformObject != null &&
                    platformObject.TryGetComponent(out GeneratedRopePlatform platform))
                {
                    states.Add(new PlatformState(
                        platform.Start,
                        platform.End,
                        platform.RopeLength));
                }
            }
            return states.ToArray();
        }

        public void RestorePlatformStates(IReadOnlyList<PlatformState> states)
        {
            ClearPlatforms();
            if (states == null)
            {
                return;
            }

            for (int i = 0; i < states.Count; i++)
            {
                CreatePlatform(states[i].Start, states[i].End, states[i].RopeLength);
            }
        }

        public bool EnsureAuthoredPlatform(
            Vector2 start,
            Vector2 end,
            float ropeLength)
        {
            if (HasPlatformBetween(start, end, ropeLength))
            {
                return false;
            }

            CreatePlatform(start, end, ropeLength);
            return true;
        }

        public bool HasPlatformBetween(
            Vector2 start,
            Vector2 end)
        {
            foreach (GameObject platformObject in generatedPlatforms)
            {
                if (platformObject != null &&
                    platformObject.TryGetComponent(
                        out GeneratedRopePlatform platform) &&
                    ConnectsEndpoints(platform, start, end))
                {
                    return true;
                }
            }

            return false;
        }

        public bool HasPlatformBetween(
            Vector2 start,
            Vector2 end,
            float ropeLength)
        {
            foreach (GameObject platformObject in generatedPlatforms)
            {
                if (platformObject == null ||
                    !platformObject.TryGetComponent(
                        out GeneratedRopePlatform platform))
                {
                    continue;
                }

                if (ConnectsEndpoints(platform, start, end) &&
                    Mathf.Abs(platform.RopeLength - ropeLength) <= 0.05f)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool ConnectsEndpoints(
            GeneratedRopePlatform platform,
            Vector2 start,
            Vector2 end)
        {
            bool sameDirection =
                Vector2.Distance(platform.Start, start) <= 0.1f &&
                Vector2.Distance(platform.End, end) <= 0.1f;
            bool reverseDirection =
                Vector2.Distance(platform.Start, end) <= 0.1f &&
                Vector2.Distance(platform.End, start) <= 0.1f;
            return sameDirection || reverseDirection;
        }

        public void ClearPlatforms()
        {
            RestoreRemovedHooks();
            for (int i = generatedPlatforms.Count - 1; i >= 0; i--)
            {
                RemovePlatform(generatedPlatforms[i]);
            }
        }

        private void RestoreRemovedHooks()
        {
            foreach (GameObject hook in removedHooks)
            {
                if (hook != null)
                {
                    hook.SetActive(true);
                }
            }
            removedHooks.Clear();
        }

        private bool CanBuild(
            out float ropeLength,
            out Vector2 start,
            out Vector2 end)
        {
            ropeLength = 0f;
            start = default;
            end = default;
            if (body == null || ropeResource == null || ropeController == null ||
                !IsPlatformBuildingUnlocked || !ropeController.IsAttached)
            {
                return false;
            }

            start = ropeController.AnchorPoint;
            end = body.position;
            ropeLength = ropeController.ActiveRopeLength;

            HookPoint activeHook = ropeController.ActiveHookPoint;
            RopePlatformAnchor platformAnchor = null;
            if (activeHook != null)
            {
                activeHook.TryGetComponent(out platformAnchor);
            }

            // Q is reserved for authored bridge Hooks. The chosen length does
            // not have to match the suggested solution; it only has to span
            // the actual distance between this Hook and its paired endpoint.
            if (platformAnchor == null ||
                !platformAnchor.TryGetPairedAnchor(
                    out Vector2 pairedAnchor))
            {
                return false;
            }

            Vector2 activeAnchor = ropeController.AnchorPoint;
            bool pairedAnchorIsCloserToPlayer =
                Vector2.Distance(body.position, pairedAnchor) <=
                Vector2.Distance(body.position, activeAnchor);
            start = pairedAnchorIsCloserToPlayer
                ? activeAnchor
                : pairedAnchor;
            end = pairedAnchorIsCloserToPlayer
                ? pairedAnchor
                : activeAnchor;

            float directDistance = Vector2.Distance(start, end);
            return ropeLength >= minimumPlatformLength &&
                   directDistance <= ropeLength + 0.05f &&
                   ropeResource.CurrentLength - ropeLength >= minimumRopeReserve;
        }

        private float GetCurrentPlatformCost()
        {
            return ropeController.ActiveRopeLength;
        }

        private bool IsHookRemovalUnlocked()
        {
            PrototypeRunController tutorial = GetComponent<PrototypeRunController>();
            if (tutorial != null && gameObject.scene.name == "Tutorial")
            {
                return tutorial.CurrentTutorialSection >= 4;
            }

            MainStageRespawnOnFall main = GetComponent<MainStageRespawnOnFall>();
            return main == null || main.CurrentSection >= 9;
        }

        private GeneratedRopePlatform CreatePlatform(
            Vector2 start,
            Vector2 end,
            float ropeLength)
        {
            GameObject platformObject = new GameObject(
                $"Generated Rope Platform {generatedPlatforms.Count + 1}");
            Vector2[] points = BuildSaggingCurve(start, end, ropeLength);

            LineRenderer line = platformObject.AddComponent<LineRenderer>();
            line.useWorldSpace = true;
            line.positionCount = points.Length;
            for (int i = 0; i < points.Length; i++)
            {
                line.SetPosition(i, points[i]);
            }
            line.startWidth = platformWidth;
            line.endWidth = platformWidth;
            line.startColor = ropeController.VisibleRopeColor;
            line.endColor = ropeController.VisibleRopeColor;
            line.sortingOrder = 2;

            Material material = null;
            Shader spriteShader = Shader.Find("Sprites/Default");
            if (spriteShader != null)
            {
                material = new Material(spriteShader)
                {
                    name = "Generated Rope Platform Material"
                };
                line.material = material;
                Texture2D yarn = YarnRopeTexture.Load();
                if (yarn != null)
                {
                    material.mainTexture = yarn;
                    line.textureMode = LineTextureMode.Tile;
                    float tileWorldLength = platformWidth * yarn.width / yarn.height;
                    line.textureScale = new Vector2(1f / Mathf.Max(0.01f, tileWorldLength), 1f);
                    Color tint = new Color(1f, 1f, 1f, ropeController.VisibleRopeColor.a);
                    line.startColor = tint;
                    line.endColor = tint;
                }
            }

            EdgeCollider2D edge = platformObject.AddComponent<EdgeCollider2D>();
            edge.edgeRadius = platformWidth * 0.5f;
            edge.points = points;

            GeneratedRopePlatform generated =
                platformObject.AddComponent<GeneratedRopePlatform>();
            generated.Configure(
                start,
                end,
                ropeLength,
                points,
                material,
                edge,
                bodyCollider,
                ropeController);
            generatedPlatforms.Add(platformObject);
            return generated;
        }

        private Vector2[] BuildSaggingCurve(Vector2 start, Vector2 end, float ropeLength)
        {
            int count = Mathf.Max(MinimumSmoothCurveSegments, curveSegments);
            Vector2[] points = new Vector2[count];
            float slack = Mathf.Max(0f, ropeLength - Vector2.Distance(start, end));
            for (int i = 0; i < count; i++)
            {
                float t = i / (float)(count - 1);
                points[i] = Vector2.Lerp(start, end, t) +
                    Vector2.down * (slack * 4f * t * (1f - t));
            }
            return points;
        }

        private void PlacePlayerOnPlatform(GeneratedRopePlatform platform)
        {
            Vector2 tangent = platform.GetEndTangent();
            Vector2 normal = new Vector2(-tangent.y, tangent.x).normalized;
            if (normal.y < 0f)
            {
                normal = -normal;
            }

            float support = 0.5f;
            if (bodyCollider != null)
            {
                Vector2 extents = bodyCollider.bounds.extents;
                support = Mathf.Abs(normal.x) * extents.x +
                          Mathf.Abs(normal.y) * extents.y;
            }

            body.position = platform.End + normal *
                (support + platformWidth * 0.5f + StandingClearance);
            body.linearVelocity = Vector2.zero;
            body.angularVelocity = 0f;
        }

        private void RemovePlatform(GameObject platformObject)
        {
            generatedPlatforms.Remove(platformObject);
            if (platformObject == null)
            {
                return;
            }

            platformObject.SetActive(false);
            Destroy(platformObject);
        }
    }

    public sealed class GeneratedRopePlatform : MonoBehaviour
    {
        private const float PlayerSupportDistance = 0.15f;

        private Material runtimeMaterial;
        private Collider2D platformCollider;
        private Collider2D playerCollider;
        private RopeController ropeController;
        private Vector2[] curvePoints;
        private bool isIgnoringPlayerCollision;
        private int observedAttachmentSequence;
        private bool preserveCollisionUntilSeparated;

        public Vector2 Start { get; private set; }
        public Vector2 End { get; private set; }
        public float RopeLength { get; private set; }

        public void Configure(
            Vector2 start,
            Vector2 end,
            float ropeLength,
            Vector2[] points,
            Material material,
            Collider2D generatedCollider,
            Collider2D playerBodyCollider,
            RopeController playerRopeController)
        {
            Start = start;
            End = end;
            RopeLength = ropeLength;
            curvePoints = points;
            runtimeMaterial = material;
            platformCollider = generatedCollider;
            playerCollider = playerBodyCollider;
            ropeController = playerRopeController;
            observedAttachmentSequence = ropeController != null
                ? ropeController.AttachmentSequence
                : 0;
            UpdatePlayerCollision();
        }

        public bool Touches(Vector2 point, float tolerance)
        {
            return Vector2.Distance(Start, point) <= tolerance ||
                   Vector2.Distance(End, point) <= tolerance;
        }

        public Vector2 GetOtherEndpoint(Vector2 sharedPoint)
        {
            return Vector2.Distance(Start, sharedPoint) <=
                   Vector2.Distance(End, sharedPoint)
                ? End
                : Start;
        }

        public Vector2 GetEndTangent()
        {
            if (curvePoints == null || curvePoints.Length < 2)
            {
                return (End - Start).normalized;
            }
            return (curvePoints[^1] - curvePoints[^2]).normalized;
        }

        public bool TryGetSurfaceTangent(
            Vector2 worldPosition,
            out Vector2 tangent)
        {
            tangent = default;
            if (curvePoints == null || curvePoints.Length < 2)
            {
                Vector2 direct = End - Start;
                if (direct.sqrMagnitude < 0.0001f)
                {
                    return false;
                }

                tangent = direct.normalized;
                return true;
            }

            int closestSegment = 0;
            float closestSegmentT = 0f;
            float closestDistanceSquared = float.PositiveInfinity;
            for (int i = 0; i < curvePoints.Length - 1; i++)
            {
                Vector2 start = curvePoints[i];
                Vector2 segment = curvePoints[i + 1] - start;
                float segmentLengthSquared = segment.sqrMagnitude;
                float segmentT = segmentLengthSquared > 0.0001f
                    ? Mathf.Clamp01(
                        Vector2.Dot(worldPosition - start, segment) /
                        segmentLengthSquared)
                    : 0f;
                Vector2 closestPoint = start + segment * segmentT;
                float distanceSquared =
                    (worldPosition - closestPoint).sqrMagnitude;
                if (distanceSquared < closestDistanceSquared)
                {
                    closestDistanceSquared = distanceSquared;
                    closestSegment = i;
                    closestSegmentT = segmentT;
                }
            }

            Vector2 firstTangent = GetCurvePointTangent(closestSegment);
            Vector2 secondTangent = GetCurvePointTangent(closestSegment + 1);
            tangent = Vector2.Lerp(
                firstTangent,
                secondTangent,
                closestSegmentT).normalized;
            return tangent.sqrMagnitude >= 0.0001f;
        }

        private Vector2 GetCurvePointTangent(int pointIndex)
        {
            int previous = Mathf.Max(0, pointIndex - 1);
            int next = Mathf.Min(curvePoints.Length - 1, pointIndex + 1);
            return (curvePoints[next] - curvePoints[previous]).normalized;
        }

        private void FixedUpdate()
        {
            UpdatePlayerCollision();
        }

        private void UpdatePlayerCollision()
        {
            if (platformCollider == null || playerCollider == null)
            {
                return;
            }

            bool isPlayerAttached =
                ropeController != null && ropeController.IsAttached;
            bool isBuildingPlatform =
                isPlayerAttached &&
                ropeController.ActiveHookPoint != null &&
                ropeController.ActiveHookPoint.TryGetComponent(
                    out RopePlatformAnchor _);
            bool isNewAttachment =
                isPlayerAttached && ropeController.AttachmentSequence !=
                observedAttachmentSequence;
            if (isNewAttachment)
            {
                // Keep the bridge solid when E is pressed while standing on it.
                // Once the player leaves it, the bridge becomes non-solid for
                // the rest of this attachment so it cannot obstruct the swing.
                observedAttachmentSequence = ropeController.AttachmentSequence;
                preserveCollisionUntilSeparated = IsSupportingPlayer();
            }
            else if (!isPlayerAttached)
            {
                preserveCollisionUntilSeparated = false;
            }
            else if (preserveCollisionUntilSeparated &&
                     !IsSupportingPlayer())
            {
                preserveCollisionUntilSeparated = false;
            }

            // Platform-anchor connections exist only for the immediate E -> Q
            // build operation. Every generated bridge must remain solid during
            // that preparation; otherwise the bridge the player is standing on
            // disappears before the second bridge can be committed.
            bool shouldIgnore =
                isPlayerAttached &&
                !isBuildingPlatform &&
                !preserveCollisionUntilSeparated;
            if (shouldIgnore == isIgnoringPlayerCollision)
            {
                return;
            }

            Physics2D.IgnoreCollision(platformCollider, playerCollider, shouldIgnore);
            isIgnoringPlayerCollision = shouldIgnore;
        }

        public bool IsSupportingPlayer()
        {
            if (platformCollider == null || playerCollider == null ||
                !platformCollider.enabled || !playerCollider.enabled)
            {
                return false;
            }

            ColliderDistance2D separation =
                playerCollider.Distance(platformCollider);
            if (!separation.isValid ||
                separation.distance > PlayerSupportDistance)
            {
                return false;
            }

            // pointB belongs to the bridge. It must be at or below the
            // player's centre; a nearby bridge above the player is not a floor.
            return separation.pointB.y <= playerCollider.bounds.center.y;
        }

        private void OnDestroy()
        {
            if (isIgnoringPlayerCollision &&
                platformCollider != null && playerCollider != null)
            {
                Physics2D.IgnoreCollision(platformCollider, playerCollider, false);
            }
            if (runtimeMaterial != null)
            {
                Destroy(runtimeMaterial);
            }
        }
    }
}
