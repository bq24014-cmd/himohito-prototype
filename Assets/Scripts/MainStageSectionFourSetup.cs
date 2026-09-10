using UnityEngine;

namespace HimoHito
{
    /// <summary>
    /// Builds main-stage section four from slide 23 of the 0829 manual.
    /// A length-5 permanent rope reaches the relay column, then a length-9
    /// swing from that column reaches the far Hook and the opposite bank.
    /// </summary>
    public static class MainStageSectionFourSetup
    {
        public const string IntermediateColumnName =
            "Main Section 4 Intermediate Column";
        public const string FarHookName =
            "Main Section 4 Far Hook";
        public const string BridgeStartMarkerName =
            "Main Section 4 Green Bridge Start Hook";
        public const string BridgeAnchorName =
            "Main Section 4 Green Bridge End Hook";
        private const string LegacyBridgeStartMarkerName =
            "Main Section 4 Bridge Start Marker";
        private const string LegacyBridgeAnchorName =
            "Main Section 4 Yellow Bridge Hook";
        public const string LandingName =
            "Main Section 4 Landing";

        // Section three's goal bank and this whole section share the +3 translation.
        public static readonly Vector2 StartEdgePosition =
            new Vector2(64.6f, -2.15f);
        public static readonly Vector2 IntermediateTopPosition =
            new Vector2(69.2f, -0.65f);
        public static readonly Vector2 IntermediateColumnPosition =
            new Vector2(69.2f, -5.65f);
        public static readonly Vector2 IntermediateColumnSize =
            new Vector2(1.8f, 10f);
        public static readonly Vector2 BridgeStartMarkerPosition =
            new Vector2(64.6f, -2.15f);
        public static readonly Vector2 BridgeStartMarkerSize =
            new Vector2(0.62f, 0.62f);
        public static readonly Vector2 BridgeAnchorPosition =
            new Vector2(68.3f, -0.65f);
        public static readonly Vector2 BridgeAnchorSize =
            new Vector2(0.62f, 0.62f);
        public const int BridgeRopeLength = 5;
        public static readonly Vector2 FarHookPosition =
            new Vector2(75.6f, 2.55f);
        public static readonly Vector2 HookSize =
            new Vector2(1.6f, 0.45f);
        public static readonly Vector2 LandingPosition =
            new Vector2(89.1f, -7.15f);
        public static readonly Vector2 LandingSize =
            new Vector2(6f, 10f);
        public static readonly Vector2 LandingRespawnPosition =
            new Vector2(87.1f, -1.45f);

        private static readonly Color TerrainColor =
            new Color(0.56f, 0.29f, 0.09f);
        private static readonly Color HookColor =
            new Color(0.298f, 0.765f, 1f);
        public static readonly Color BridgeAnchorColor =
            new Color(0.33f, 1f, 0.76f);

        public static bool ApplyCurrentScene()
        {
            bool changed = false;
            changed |= RenameLegacyObject(
                LegacyBridgeStartMarkerName,
                BridgeStartMarkerName);
            changed |= RenameLegacyObject(
                LegacyBridgeAnchorName,
                BridgeAnchorName);
            changed |= EnsureTerrain(
                IntermediateColumnName,
                IntermediateColumnPosition,
                IntermediateColumnSize);
            changed |= EnsureBridgeStartMarker();
            changed |= EnsureBridgeAnchor();
            changed |= EnsureHook(FarHookName, FarHookPosition);
            changed |= EnsureTerrain(
                LandingName,
                LandingPosition,
                LandingSize);

            GameObject landing = FindSceneObject(LandingName);
            if (landing != null)
            {
                if (!landing.TryGetComponent(
                        out MainStageCheckpoint checkpoint))
                {
                    checkpoint = landing.AddComponent<MainStageCheckpoint>();
                    changed = true;
                }

                checkpoint.Configure(5, LandingRespawnPosition, 45f);
            }

            return changed;
        }

        private static bool EnsureBridgeStartMarker()
        {
            GameObject marker = EnsureObject(
                BridgeStartMarkerName,
                BridgeStartMarkerPosition,
                BridgeStartMarkerSize,
                out bool changed);
            if (!marker.TryGetComponent(out HookPoint hookPoint))
            {
                hookPoint = marker.AddComponent<HookPoint>();
                changed = true;
            }
            changed |= hookPoint.ConfigureFixedAttachmentPoint(Vector2.zero);
            if (marker.TryGetComponent(out BoxCollider2D markerCollider) &&
                markerCollider.enabled)
            {
                markerCollider.enabled = false;
                changed = true;
            }
            if (!marker.TryGetComponent(out RopePlatformAnchor platformAnchor))
            {
                platformAnchor = marker.AddComponent<RopePlatformAnchor>();
                changed = true;
            }
            changed |= platformAnchor.Configure(
                BridgeRopeLength,
                BridgeAnchorName);
            marker.TryGetComponent(out SolidSprite visual);
            if (visual.Color != BridgeAnchorColor)
            {
                visual.Color = BridgeAnchorColor;
                changed = true;
            }
            return changed;
        }

        private static bool EnsureBridgeAnchor()
        {
            GameObject anchor = EnsureObject(
                BridgeAnchorName,
                BridgeAnchorPosition,
                BridgeAnchorSize,
                out bool changed);
            if (!anchor.TryGetComponent(out HookPoint hookPoint))
            {
                hookPoint = anchor.AddComponent<HookPoint>();
                changed = true;
            }
            changed |= hookPoint.ConfigureFixedAttachmentPoint(Vector2.zero);
            if (!anchor.TryGetComponent(
                    out RopePlatformAnchor platformAnchor))
            {
                platformAnchor = anchor.AddComponent<RopePlatformAnchor>();
                changed = true;
            }
            changed |= platformAnchor.Configure(
                BridgeRopeLength,
                BridgeStartMarkerName);
            if (anchor.TryGetComponent(out SolidSprite visual) &&
                visual.Color != BridgeAnchorColor)
            {
                visual.Color = BridgeAnchorColor;
                changed = true;
            }
            return changed;
        }

        private static bool RenameLegacyObject(
            string legacyName,
            string currentName)
        {
            GameObject current = FindSceneObject(currentName);
            if (current != null)
            {
                return false;
            }

            GameObject legacy = FindSceneObject(legacyName);
            if (legacy == null)
            {
                return false;
            }

            legacy.name = currentName;
            return true;
        }

        private static bool EnsureHook(string objectName, Vector2 position)
        {
            GameObject hook = EnsureObject(
                objectName,
                position,
                HookSize,
                out bool changed);
            if (!hook.TryGetComponent(out HookPoint hookPoint))
            {
                hookPoint = hook.AddComponent<HookPoint>();
                changed = true;
            }
            changed |= hookPoint.ConfigureFixedAttachmentPoint(Vector2.zero);

            if (hook.TryGetComponent(out SolidSprite visual) &&
                visual.Color != HookColor)
            {
                visual.Color = HookColor;
                changed = true;
            }
            return changed;
        }

        private static bool EnsureTerrain(
            string objectName,
            Vector2 position,
            Vector2 size)
        {
            GameObject terrain = EnsureObject(
                objectName,
                position,
                size,
                out bool changed);
            if (!terrain.TryGetComponent(out SolidSwingSurface _))
            {
                terrain.AddComponent<SolidSwingSurface>();
                changed = true;
            }
            if (terrain.TryGetComponent(out BoxCollider2D collider) &&
                collider.isTrigger)
            {
                collider.isTrigger = false;
                changed = true;
            }
            if (terrain.TryGetComponent(out SolidSprite visual) &&
                visual.Color != TerrainColor)
            {
                visual.Color = TerrainColor;
                changed = true;
            }
            return changed;
        }

        private static GameObject EnsureObject(
            string objectName,
            Vector2 position,
            Vector2 size,
            out bool changed)
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
            if (!target.TryGetComponent(out SolidSprite _))
            {
                target.AddComponent<SolidSprite>();
                changed = true;
            }
            if (!target.TryGetComponent(out BoxCollider2D collider))
            {
                collider = target.AddComponent<BoxCollider2D>();
                changed = true;
            }
            if (collider.size != Vector2.one)
            {
                collider.size = Vector2.one;
                changed = true;
            }
            return target;
        }

        private static GameObject FindSceneObject(string objectName)
        {
            return SceneObjectLookup.Find(objectName, "MainStage");
        }
    }
}
