using UnityEngine;

namespace HimoHito
{
    /// <summary>
    /// Builds section eight from slide 27 of the 0829 stage manual. The
    /// generated rope platform must sag through the three-unit shaft mouth
    /// without touching the spikes, then lead to the low passage on the right.
    /// </summary>
    public static class MainStageSectionEightSetup
    {
        public const string LeftAnchorName = "Main S08 Green Left Rim";
        public const string RightAnchorName = "Main S08 Green Right Rim";
        public const string RightRimName = "Main S08 Right Rim";
        public const string LeftShaftLipName = "Main S08 Left Shaft Lip";
        public const string RightShaftLipName = "Main S08 Right Shaft Lip";
        public const string ExitFloorName = "Main S08 Exit Floor";
        public const string SpikeNamePrefix = "Main S08 Pit Spike";

        private const string LegacyDepthGateName =
            "Main S08 Wooden Depth Gate";

        public const int MinimumBuildLength = 5;
        public const int MaximumBuildLength = 14;
        public const int MinimumCorrectLength = 7;
        public const int MaximumCorrectLength = 8;
        public const float ShaftWidth = 5f;

        private const int SpikeCount = 4;
        private const float RimHeight = -2.15f;
        private const float SpikeCenterY = -6.35f;

        public static readonly Vector2 LeftAnchorPosition =
            new Vector2(175f, RimHeight);
        public static readonly Vector2 RightAnchorPosition =
            new Vector2(180f, RimHeight);
        public static readonly Vector2 RightRimPosition =
            new Vector2(185f, -2.5f);
        public static readonly Vector2 RightRimSize =
            new Vector2(10f, 0.7f);
        public static readonly Vector2 LeftShaftLipPosition =
            new Vector2(175.5f, -3.65f);
        public static readonly Vector2 RightShaftLipPosition =
            new Vector2(179.5f, -3.65f);
        public static readonly Vector2 ShaftLipSize =
            new Vector2(1f, 0.3f);
        public static readonly Vector2 ExitFloorPosition =
            new Vector2(184.1f, -10.85f);
        public static readonly Vector2 ExitFloorSize =
            new Vector2(10f, 10f);
        public static readonly Vector2 ExitRespawnPosition =
            new Vector2(180.1f, -5.15f);
        public static readonly Vector2 BridgeHookSize =
            new Vector2(0.62f, 0.62f);
        public static readonly Vector2 SpikeSize =
            new Vector2(0.7f, 0.8f);

        private static readonly Color TerrainColor =
            new Color(0.56f, 0.29f, 0.09f);
        private static readonly Color BridgeHookColor =
            new Color(0.33f, 1f, 0.76f);
        private static readonly Color SpikeColor =
            new Color(1f, 0.18f, 0.25f);

        public static bool ApplyCurrentScene()
        {
            bool changed = RemoveLegacyObject(LegacyDepthGateName);
            changed |= EnsureBridgeHook(
                LeftAnchorName,
                LeftAnchorPosition,
                RightAnchorName,
                false);
            changed |= EnsureBridgeHook(
                RightAnchorName,
                RightAnchorPosition,
                LeftAnchorName,
                true);

            changed |= EnsureTerrain(
                RightRimName,
                RightRimPosition,
                RightRimSize,
                out _);
            changed |= EnsureShaftLip(
                LeftShaftLipName,
                LeftShaftLipPosition,
                ShaftLipSize);
            changed |= EnsureShaftLip(
                RightShaftLipName,
                RightShaftLipPosition,
                ShaftLipSize);

            changed |= EnsureTerrain(
                ExitFloorName,
                ExitFloorPosition,
                ExitFloorSize,
                out GameObject exitFloor);
            if (!exitFloor.TryGetComponent(out MainStageCheckpoint checkpoint))
            {
                checkpoint = exitFloor.AddComponent<MainStageCheckpoint>();
                changed = true;
            }
            checkpoint.Configure(9, ExitRespawnPosition, 0f);

            float firstSpikeX = LeftAnchorPosition.x + 1.3f;
            float lastSpikeX = RightAnchorPosition.x - 1.3f;
            float spikeStep = (lastSpikeX - firstSpikeX) /
                Mathf.Max(1, SpikeCount - 1);
            for (int i = 0; i < SpikeCount; i++)
            {
                changed |= EnsureSpike(
                    $"{SpikeNamePrefix} {i + 1}",
                    new Vector2(firstSpikeX + spikeStep * i, SpikeCenterY));
            }

            return changed;
        }

        public static GameObject EnsureCreated()
        {
            ApplyCurrentScene();
            return FindSceneObject(ExitFloorName);
        }

        public static bool IsCorrectLength(float ropeLength)
        {
            return ropeLength >= MinimumCorrectLength - 0.05f &&
                   ropeLength <= MaximumCorrectLength + 0.05f;
        }

        public static float CalculatePlatformBottomY(float ropeLength)
        {
            float slack = Mathf.Max(0f, ropeLength - ShaftWidth);
            return RimHeight - slack;
        }

        public static bool TryGetShaftPlatformLength(
            RopePlatformBuilder platformBuilder,
            out float ropeLength)
        {
            ropeLength = 0f;
            if (platformBuilder == null)
            {
                return false;
            }

            RopePlatformBuilder.PlatformState[] states =
                platformBuilder.CapturePlatformStates();
            for (int i = states.Length - 1; i >= 0; i--)
            {
                RopePlatformBuilder.PlatformState state = states[i];
                bool sameDirection =
                    Vector2.Distance(state.Start, LeftAnchorPosition) <= 0.15f &&
                    Vector2.Distance(state.End, RightAnchorPosition) <= 0.15f;
                bool reverseDirection =
                    Vector2.Distance(state.Start, RightAnchorPosition) <= 0.15f &&
                    Vector2.Distance(state.End, LeftAnchorPosition) <= 0.15f;
                if (!sameDirection && !reverseDirection)
                {
                    continue;
                }

                ropeLength = state.RopeLength;
                return true;
            }
            return false;
        }

        private static bool EnsureBridgeHook(
            string objectName,
            Vector2 position,
            string pairedHookName,
            bool canAttach)
        {
            GameObject hook = EnsureObject(
                objectName,
                position,
                BridgeHookSize,
                BridgeHookColor,
                out bool changed);
            if (!hook.TryGetComponent(out HookPoint hookPoint))
            {
                hookPoint = hook.AddComponent<HookPoint>();
                changed = true;
            }
            changed |= hookPoint.ConfigureFixedAttachmentPoint(Vector2.zero);

            if (!hook.TryGetComponent(out RopePlatformAnchor anchor))
            {
                anchor = hook.AddComponent<RopePlatformAnchor>();
                changed = true;
            }
            changed |= anchor.ConfigureRange(
                MinimumBuildLength,
                MaximumBuildLength,
                pairedHookName);

            if (hook.TryGetComponent(out BoxCollider2D collider) &&
                collider.enabled != canAttach)
            {
                collider.enabled = canAttach;
                changed = true;
            }
            return changed;
        }

        private static bool EnsureTerrain(
            string objectName,
            Vector2 position,
            Vector2 size,
            out GameObject terrain)
        {
            terrain = EnsureObject(
                objectName,
                position,
                size,
                TerrainColor,
                out bool changed);
            if (!terrain.TryGetComponent(out SolidSwingSurface _))
            {
                terrain.AddComponent<SolidSwingSurface>();
                changed = true;
            }
            if (terrain.TryGetComponent(out BoxCollider2D collider) &&
                (!collider.enabled || collider.isTrigger))
            {
                collider.enabled = true;
                collider.isTrigger = false;
                changed = true;
            }
            return changed;
        }

        private static bool EnsureShaftLip(
            string objectName,
            Vector2 position,
            Vector2 size)
        {
            GameObject lip = EnsureObject(
                objectName,
                position,
                size,
                TerrainColor,
                out bool changed);
            if (lip.TryGetComponent(out BoxCollider2D collider) &&
                collider.enabled)
            {
                // The slide uses these ledges to show the three-unit mouth.
                // A solid box edge catches the player's rear corner while the
                // sagging path descends diagonally, so they remain visual only.
                collider.enabled = false;
                changed = true;
            }
            return changed;
        }

        private static bool EnsureSpike(string objectName, Vector2 position)
        {
            GameObject spike = EnsureObject(
                objectName,
                position,
                SpikeSize,
                SpikeColor,
                out bool changed,
                false);
            if (spike.TryGetComponent(out BoxCollider2D collider))
            {
                Vector2 targetSize = new Vector2(0.86f, 0.9f);
                Vector2 targetOffset = new Vector2(0f, -0.03f);
                if (collider.size != targetSize)
                {
                    collider.size = targetSize;
                    changed = true;
                }
                if (collider.offset != targetOffset)
                {
                    collider.offset = targetOffset;
                    changed = true;
                }
                if (!collider.enabled || !collider.isTrigger)
                {
                    collider.enabled = true;
                    collider.isTrigger = true;
                    changed = true;
                }
            }
            if (!spike.TryGetComponent(out RopeSpikeHazard _))
            {
                spike.AddComponent<RopeSpikeHazard>();
                changed = true;
            }
            if (!spike.TryGetComponent(out SectionEightPitHazard _))
            {
                spike.AddComponent<SectionEightPitHazard>();
                changed = true;
            }
            if (!spike.TryGetComponent(out ToySpikeVisual spikeVisual))
            {
                spikeVisual = spike.AddComponent<ToySpikeVisual>();
                changed = true;
            }
            changed |= spikeVisual.Refresh();
            return changed;
        }

        private static GameObject EnsureObject(
            string objectName,
            Vector2 position,
            Vector2 size,
            Color color,
            out bool changed,
            bool resetColliderSize = true)
        {
            changed = false;
            GameObject target = FindSceneObject(objectName);
            if (target == null)
            {
                target = new GameObject(objectName);
                changed = true;
            }
            if (!target.activeSelf)
            {
                target.SetActive(true);
                changed = true;
            }

            Vector3 targetPosition = new Vector3(position.x, position.y, 0f);
            Vector3 targetScale = new Vector3(size.x, size.y, 1f);
            if (target.transform.position != targetPosition)
            {
                target.transform.position = targetPosition;
                changed = true;
            }
            if (target.transform.localScale != targetScale)
            {
                target.transform.localScale = targetScale;
                changed = true;
            }
            if (!target.TryGetComponent(out SpriteRenderer _))
            {
                target.AddComponent<SpriteRenderer>();
                changed = true;
            }
            if (!target.TryGetComponent(out SolidSprite visual))
            {
                visual = target.AddComponent<SolidSprite>();
                changed = true;
            }
            if (visual.Color != color)
            {
                visual.Color = color;
                changed = true;
            }
            if (!target.TryGetComponent(out BoxCollider2D collider))
            {
                collider = target.AddComponent<BoxCollider2D>();
                changed = true;
            }
            if (resetColliderSize && collider.size != Vector2.one)
            {
                collider.size = Vector2.one;
                changed = true;
            }
            return target;
        }

        private static GameObject FindSceneObject(string objectName)
        {
            foreach (GameObject candidate in
                     Resources.FindObjectsOfTypeAll<GameObject>())
            {
                if (candidate.scene.IsValid() &&
                    candidate.name == objectName)
                {
                    return candidate;
                }
            }
            return null;
        }

        private static bool RemoveLegacyObject(string objectName)
        {
            GameObject legacy = FindSceneObject(objectName);
            if (legacy == null)
            {
                return false;
            }

            legacy.SetActive(false);
            if (Application.isPlaying)
            {
                Object.Destroy(legacy);
            }
            else
            {
                Object.DestroyImmediate(legacy);
            }
            return true;
        }
    }
}
