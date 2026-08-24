using System;
using System.Collections.Generic;
using UnityEngine;

namespace HimoHito
{
    /// <summary>
    /// Turns the currently attached rope into a permanent walkable platform with Q.
    /// The distance from the hook to the player is permanently removed from the rope resource.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    [RequireComponent(typeof(Rigidbody2D), typeof(RopeResource), typeof(RopeController))]
    public sealed class RopePlatformBuilder : MonoBehaviour
    {
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
        [SerializeField, Min(0f)] private float standingClearance = 0.05f;

        private readonly List<GameObject> generatedPlatforms = new();
        private Rigidbody2D body;
        private Collider2D bodyCollider;
        private RopeResource ropeResource;
        private RopeController ropeController;

        public float CurrentPlatformCost => ropeController != null && ropeController.IsAttached
            ? Vector2.Distance(ropeController.AnchorPoint, body.position)
            : 0f;
        public float MinimumRopeReserve => minimumRopeReserve;
        public int GeneratedPlatformCount => generatedPlatforms.Count;
        public bool CanBuildCurrentPlatform => CanBuild(out _);

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            bodyCollider = GetComponent<Collider2D>();
            ropeResource = GetComponent<RopeResource>();
            ropeController = GetComponent<RopeController>();
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
            if (!CanBuild(out float cost))
            {
                return false;
            }

            Vector2 start = ropeController.AnchorPoint;
            Vector2 end = body.position;
            if (!ropeController.CommitAttachedRopeAsPlatform(cost))
            {
                return false;
            }

            GeneratedRopePlatform generatedPlatform = CreatePlatform(start, end);
            PlacePlayerOnPlatform(end);
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
            cost = 0f;
            if (body == null ||
                ropeResource == null ||
                ropeController == null ||
                !ropeController.IsAttached)
            {
                return false;
            }

            cost = CurrentPlatformCost;
            if (cost < minimumPlatformLength ||
                cost > ropeController.ActiveRopeLength + 0.05f)
            {
                return false;
            }

            float lengthBeforeAttachment =
                ropeResource.CurrentLength + ropeController.ActiveRopeLength;
            return lengthBeforeAttachment - cost >= minimumRopeReserve;
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
            generated.Configure(start, end, line.material);
            generatedPlatforms.Add(platform);
            return generated;
        }

        private void PlacePlayerOnPlatform(Vector2 end)
        {
            float playerHalfHeight = bodyCollider != null
                ? bodyCollider.bounds.extents.y
                : 0.5f;
            body.position = end + Vector2.up *
                (playerHalfHeight + platformWidth * 0.5f + standingClearance);
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

        public Vector2 Start { get; private set; }
        public Vector2 End { get; private set; }

        public void Configure(Vector2 start, Vector2 end, Material material)
        {
            Start = start;
            End = end;
            runtimeMaterial = material;
        }

        private void OnDestroy()
        {
            if (runtimeMaterial != null)
            {
                Destroy(runtimeMaterial);
            }
        }
    }
}
