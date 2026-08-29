using System;
using System.Collections.Generic;
using UnityEngine;

namespace HimoHito
{
    /// <summary>
    /// Turns the currently attached rope into a permanent walkable platform with Q.
    /// The distance from the attachment point to the player is permanently removed
    /// from the rope resource. The attachment may be a Hook or another solid surface.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    [RequireComponent(typeof(Rigidbody2D), typeof(RopeResource), typeof(RopeController))]
    public sealed class RopePlatformBuilder : MonoBehaviour
    {
        private const float StandingClearance = 0.005f;
        private const float MainStagePlatformUnlockX = 96f;

        [Serializable]
        public readonly struct PlatformState
        {
            public PlatformState(Vector2 start, Vector2 end)
            {
                Start = start;
                End = end;
            }

            public Vector2 Start { get; }
            public Vector2 End { get; }
        }

        [SerializeField, Min(0.05f)] private float platformWidth = 0.22f;
        [SerializeField, Min(0.1f)] private float minimumPlatformLength = 1f;
        [SerializeField, Min(0.01f)] private float minimumRopeReserve = 1f;

        private readonly List<GameObject> generatedPlatforms = new();
        private Rigidbody2D body;
        private Collider2D bodyCollider;
        private RopeResource ropeResource;
        private RopeController ropeController;
        private MainStageRespawnOnFall mainStageRespawn;
        private bool mainStagePlatformBuildingUnlocked;

        public float CurrentPlatformCost => TryGetPlatformEndpoints(
            out Vector2 start,
            out Vector2 end)
                ? Vector2.Distance(start, end)
                : 0f;
        public float MinimumRopeReserve => minimumRopeReserve;
        public int GeneratedPlatformCount => generatedPlatforms.Count;
        public bool IsPlatformBuildingUnlocked =>
            EvaluatePlatformBuildingUnlocked();
        public bool CanBuildCurrentPlatform => CanBuild(out _);

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            bodyCollider = GetComponent<Collider2D>();
            ropeResource = GetComponent<RopeResource>();
            ropeController = GetComponent<RopeController>();
            mainStageRespawn = GetComponent<MainStageRespawnOnFall>();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                TryBuildCurrentPlatform();
            }
        }

        public bool TryBuildCurrentPlatform()
        {
            if (!CanBuild(out float cost, out Vector2 start, out Vector2 end))
            {
                return false;
            }

            if (ropeController.IsAttached)
            {
                if (!ropeController.CommitAttachedRopeAsPlatform(cost))
                {
                    return false;
                }
            }
            else if (!ropeResource.TrySpend(cost))
            {
                return false;
            }

            GeneratedRopePlatform generatedPlatform = CreatePlatform(start, end);
            PlacePlayerOnPlatform(start, end);
            if (TryGetComponent(out PlayerMover playerMover))
            {
                playerMover.RegisterGeneratedRopePlatformContact(generatedPlatform);
            }

            if (TryGetComponent(out PrototypeRunController tutorialRun) &&
                tutorialRun.CurrentTutorialSection == 3)
            {
                tutorialRun.TryReachTutorialSection(4, body.position);
            }
            return true;
        }

        public PlatformState[] CapturePlatformStates()
        {
            List<PlatformState> states = new(generatedPlatforms.Count);
            foreach (GameObject platform in generatedPlatforms)
            {
                if (platform == null ||
                    !platform.TryGetComponent(out GeneratedRopePlatform generated))
                {
                    continue;
                }

                states.Add(new PlatformState(generated.Start, generated.End));
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
                CreatePlatform(states[i].Start, states[i].End);
            }
        }

        public void ClearPlatforms()
        {
            foreach (GameObject platform in generatedPlatforms)
            {
                if (platform == null)
                {
                    continue;
                }

                platform.SetActive(false);
                Destroy(platform);
            }

            generatedPlatforms.Clear();
        }

        private bool CanBuild(out float cost)
        {
            return CanBuild(out cost, out _, out _);
        }

        private bool CanBuild(
            out float cost,
            out Vector2 start,
            out Vector2 end)
        {
            cost = 0f;
            start = default;
            end = default;
            if (body == null ||
                ropeResource == null ||
                ropeController == null ||
                !EvaluatePlatformBuildingUnlocked() ||
                !TryGetPlatformEndpoints(out start, out end))
            {
                return false;
            }

            cost = Vector2.Distance(start, end);
            if (cost < minimumPlatformLength)
            {
                return false;
            }

            float availableLength = ropeController.IsAttached
                ? ropeController.ActiveRopeLength
                : ropeController.SelectedRopeLength;
            if (cost > availableLength + 0.05f)
            {
                return false;
            }

            float lengthBeforeAttachment = ropeResource.CurrentLength;
            if (ropeController.IsAttached)
            {
                lengthBeforeAttachment += ropeController.ActiveRopeLength;
            }

            return lengthBeforeAttachment - cost >= minimumRopeReserve;
        }

        private bool EvaluatePlatformBuildingUnlocked()
        {
            if (mainStageRespawn == null)
            {
                return true;
            }

            if (!mainStagePlatformBuildingUnlocked &&
                body != null &&
                (body.position.x >= MainStagePlatformUnlockX ||
                 mainStageRespawn.HasReachedSectionEight))
            {
                mainStagePlatformBuildingUnlocked = true;
            }

            return mainStagePlatformBuildingUnlocked;
        }

        private bool TryGetPlatformEndpoints(
            out Vector2 start,
            out Vector2 end)
        {
            start = default;
            end = body != null ? body.position : default;
            if (body == null || ropeController == null)
            {
                return false;
            }

            if (ropeController.IsAttached)
            {
                start = ropeController.AnchorPoint;
                return true;
            }

            return ropeController.TryResolveCurrentAimAnchor(out start);
        }

        private GeneratedRopePlatform CreatePlatform(Vector2 start, Vector2 end)
        {
            GameObject platform = new GameObject(
                $"Generated Rope Platform {generatedPlatforms.Count + 1}");
            platform.transform.SetParent(null);

            LineRenderer line = platform.AddComponent<LineRenderer>();
            line.useWorldSpace = true;
            line.positionCount = 2;
            line.SetPosition(0, start);
            line.SetPosition(1, end);
            line.startWidth = platformWidth;
            line.endWidth = platformWidth;
            line.startColor = ropeController.VisibleRopeColor;
            line.endColor = ropeController.VisibleRopeColor;
            line.sortingOrder = 2;

            Shader spriteShader = Shader.Find("Sprites/Default");
            if (spriteShader != null)
            {
                Material material = new Material(spriteShader)
                {
                    name = "Generated Rope Platform Material"
                };
                line.material = material;
            }

            EdgeCollider2D edge = platform.AddComponent<EdgeCollider2D>();
            edge.edgeRadius = platformWidth * 0.5f;
            edge.points = new[] { start, end };

            GeneratedRopePlatform generated =
                platform.AddComponent<GeneratedRopePlatform>();
            generated.Configure(
                start,
                end,
                line.material,
                edge,
                bodyCollider,
                ropeController);
            generatedPlatforms.Add(platform);
            return generated;
        }

        private void PlacePlayerOnPlatform(Vector2 start, Vector2 end)
        {
            Vector2 platformDirection = end - start;
            Vector2 surfaceNormal = platformDirection.sqrMagnitude > 0.0001f
                ? new Vector2(-platformDirection.y, platformDirection.x).normalized
                : Vector2.up;
            if (surfaceNormal.y < 0f)
            {
                surfaceNormal = -surfaceNormal;
            }

            float playerSupportDistance = 0.5f;
            if (bodyCollider != null)
            {
                Vector2 extents = bodyCollider.bounds.extents;
                playerSupportDistance =
                    Mathf.Abs(surfaceNormal.x) * extents.x +
                    Mathf.Abs(surfaceNormal.y) * extents.y;
            }

            float surfaceDistance =
                playerSupportDistance + platformWidth * 0.5f + StandingClearance;
            body.position = end + surfaceNormal * surfaceDistance;
            body.linearVelocity = Vector2.zero;
            body.angularVelocity = 0f;
        }
    }

    /// <summary>
    /// Stores the endpoints of one generated platform for checkpoint snapshots.
    /// </summary>
    public sealed class GeneratedRopePlatform : MonoBehaviour
    {
        private Material runtimeMaterial;
        private Collider2D platformCollider;
        private Collider2D playerCollider;
        private RopeController ropeController;
        private bool isIgnoringPlayerCollision;

        public Vector2 Start { get; private set; }
        public Vector2 End { get; private set; }

        public void Configure(
            Vector2 start,
            Vector2 end,
            Material material,
            Collider2D generatedCollider,
            Collider2D playerBodyCollider,
            RopeController playerRopeController)
        {
            Start = start;
            End = end;
            runtimeMaterial = material;
            platformCollider = generatedCollider;
            playerCollider = playerBodyCollider;
            ropeController = playerRopeController;
            UpdatePlayerCollision();
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

            bool shouldIgnore = ropeController != null && ropeController.IsAttached;
            if (shouldIgnore == isIgnoringPlayerCollision)
            {
                return;
            }

            Physics2D.IgnoreCollision(
                platformCollider,
                playerCollider,
                shouldIgnore);
            isIgnoringPlayerCollision = shouldIgnore;
        }

        private void OnDestroy()
        {
            if (isIgnoringPlayerCollision &&
                platformCollider != null &&
                playerCollider != null)
            {
                Physics2D.IgnoreCollision(
                    platformCollider,
                    playerCollider,
                    false);
            }

            if (runtimeMaterial != null)
            {
                Destroy(runtimeMaterial);
            }
        }
    }
}
