using UnityEngine;

namespace HimoHito
{
    /// <summary>
    /// Builds the finale from slide 29 of the 0829 stage manual. The player
    /// spends length ten between two bank anchors and walks to the toy box.
    /// No swing Hook is placed above the valley.
    /// </summary>
    public static class MainStageSectionTenSetup
    {
        public const string LeftAnchorName = "Main S10 Green Left Bank";
        public const string RightAnchorName = "Main S10 Green Right Bank";
        public const string GoalFloorName = "Main S10 Goal Floor";
        public const string GoalMarkerName = "Main S10 Toy Box Goal";
        public const int BridgeRopeLength = 10;
        public const float MinimumRopeAtEntry = 11f;

        public static readonly Vector2 LeftAnchorPosition =
            new Vector2(194f, -2.15f);
        public static readonly Vector2 RightAnchorPosition =
            new Vector2(203f, -2.15f);
        public static readonly Vector2 GoalFloorPosition =
            new Vector2(210.5f, -7.15f);
        public static readonly Vector2 GoalFloorSize =
            new Vector2(15f, 10f);
        public static readonly Vector2 GoalMarkerPosition =
            new Vector2(214f, -0.95f);
        public static readonly Vector2 GoalMarkerSize =
            new Vector2(2.6f, 2.4f);

        private static readonly Vector2 AnchorSize =
            new Vector2(0.62f, 0.62f);
        private static readonly Color TerrainColor =
            new Color(0.56f, 0.29f, 0.09f);
        private static readonly Color BridgeHookColor =
            new Color(0.33f, 1f, 0.76f);
        private static readonly Color GoalMarkerColor =
            new Color(0.80f, 0.48f, 0.18f);

        public static bool ApplyCurrentScene()
        {
            bool changed = EnsurePairedAnchor(
                LeftAnchorName,
                LeftAnchorPosition,
                RightAnchorName,
                false);
            changed |= EnsurePairedAnchor(
                RightAnchorName,
                RightAnchorPosition,
                LeftAnchorName,
                true);
            changed |= EnsureTerrain(
                GoalFloorName,
                GoalFloorPosition,
                GoalFloorSize,
                out GameObject goalFloor);
            if (!goalFloor.TryGetComponent(out MainStageGoalZone _))
            {
                goalFloor.AddComponent<MainStageGoalZone>();
                changed = true;
            }
            changed |= EnsureGoalMarker();
            return changed;
        }

        public static GameObject EnsureCreated()
        {
            ApplyCurrentScene();
            return FindSceneObject(GoalMarkerName);
        }

        public static bool HasFinalPlatform(RopePlatformBuilder builder)
        {
            return builder != null && builder.HasPlatformBetween(
                LeftAnchorPosition,
                RightAnchorPosition);
        }

        private static bool EnsurePairedAnchor(
            string objectName,
            Vector2 position,
            string pairedAnchorName,
            bool canAttach)
        {
            GameObject hook = EnsureSolidObject(
                objectName,
                position,
                AnchorSize,
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
            changed |= anchor.Configure(BridgeRopeLength, pairedAnchorName);

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
            terrain = EnsureSolidObject(
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

        private static bool EnsureGoalMarker()
        {
            GameObject marker = FindSceneObject(GoalMarkerName);
            bool changed = false;
            if (marker == null)
            {
                marker = new GameObject(GoalMarkerName);
                changed = true;
            }
            if (!marker.activeSelf)
            {
                marker.SetActive(true);
                changed = true;
            }

            Vector3 targetPosition = new Vector3(
                GoalMarkerPosition.x,
                GoalMarkerPosition.y,
                0f);
            Vector3 targetScale = new Vector3(
                GoalMarkerSize.x,
                GoalMarkerSize.y,
                1f);
            if (marker.transform.position != targetPosition)
            {
                marker.transform.position = targetPosition;
                changed = true;
            }
            if (marker.transform.localScale != targetScale)
            {
                marker.transform.localScale = targetScale;
                changed = true;
            }
            if (!marker.TryGetComponent(out SpriteRenderer _))
            {
                marker.AddComponent<SpriteRenderer>();
                changed = true;
            }
            if (!marker.TryGetComponent(out SolidSprite visual))
            {
                visual = marker.AddComponent<SolidSprite>();
                changed = true;
            }
            if (visual.Color != GoalMarkerColor)
            {
                visual.Color = GoalMarkerColor;
                changed = true;
            }
            if (marker.TryGetComponent(out Collider2D collider))
            {
                if (Application.isPlaying)
                {
                    Object.Destroy(collider);
                }
                else
                {
                    Object.DestroyImmediate(collider);
                }
                changed = true;
            }
            return changed;
        }

        private static GameObject EnsureSolidObject(
            string objectName,
            Vector2 position,
            Vector2 size,
            Color color,
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
            if (collider.size != Vector2.one)
            {
                collider.size = Vector2.one;
                changed = true;
            }
            return target;
        }

        private static GameObject FindSceneObject(string objectName)
        {
            return SceneObjectLookup.Find(objectName);
        }
    }
}
