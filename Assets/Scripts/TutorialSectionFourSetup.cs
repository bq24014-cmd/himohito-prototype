using UnityEngine;

namespace HimoHito
{
    /// <summary>
    /// Builds the tutorial finale. Two length-six platforms meet at the
    /// center Hook; removing it with F joins them into one deeper bridge.
    /// </summary>
    public static class TutorialSectionFourSetup
    {
        public const string LeftAnchorName =
            "Tutorial T4 Green Left Bank";
        public const string CenterHookName =
            "Tutorial T4 Center Hook";
        public const string RightAnchorName =
            "Tutorial T4 Green Right Bank";
        public const string BeamName = "Tutorial T4 Hanging Beam";
        public const string GoalFloorName = "Tutorial T4 Goal Floor";
        public const string GoalMarkerName = "Tutorial T4 Toy Box Goal";
        public const int PlatformRopeLength = 6;

        public static readonly Vector2 LeftAnchorPosition =
            new Vector2(42f, 0.35f);
        public static readonly Vector2 CenterHookPosition =
            new Vector2(47f, 1.55f);
        public static readonly Vector2 RightAnchorPosition =
            new Vector2(52f, 0.35f);
        public static readonly Vector2 BeamPosition =
            new Vector2(48.5f, 3.30f);
        public static readonly Vector2 BeamSize =
            new Vector2(2f, 5f);
        public static readonly Vector2 GoalFloorPosition =
            new Vector2(55f, -4.65f);
        public static readonly Vector2 GoalFloorSize =
            new Vector2(6f, 10f);
        public static readonly Vector2 GoalMarkerPosition =
            new Vector2(55f, 1.55f);
        public static readonly Vector2 GoalMarkerSize =
            new Vector2(2.4f, 2.2f);

        private const string BridgeAnchorVisualName =
            "Green Rope Anchor Ring Visual";
        private static readonly Vector2 BridgeHookSize =
            new Vector2(0.62f, 0.62f);
        private static readonly Vector2 CenterHookSize =
            new Vector2(1.6f, 0.45f);
        private static readonly Color TerrainColor =
            new Color(0.96f, 0.55f, 0.18f);
        private static readonly Color BridgeHookColor =
            new Color(0.33f, 1f, 0.76f);
        private static readonly Color SwingHookColor =
            new Color(0.30f, 0.76f, 1f);
        private static readonly Color GoalMarkerColor =
            new Color(0.80f, 0.48f, 0.18f);

        public static bool ApplyCurrentScene()
        {
            bool changed = EnsurePairedHook(
                LeftAnchorName,
                LeftAnchorPosition,
                BridgeHookSize,
                BridgeHookColor,
                CenterHookName,
                false,
                true);
            changed |= EnsurePairedHook(
                CenterHookName,
                CenterHookPosition,
                CenterHookSize,
                SwingHookColor,
                LeftAnchorName,
                true,
                false);
            changed |= EnsurePairedHook(
                RightAnchorName,
                RightAnchorPosition,
                BridgeHookSize,
                BridgeHookColor,
                CenterHookName,
                true,
                true);
            changed |= EnsureTerrain(
                BeamName,
                BeamPosition,
                BeamSize,
                out _);
            changed |= EnsureTerrain(
                GoalFloorName,
                GoalFloorPosition,
                GoalFloorSize,
                out GameObject goalFloor);
            if (!goalFloor.TryGetComponent(out GoalZone _))
            {
                goalFloor.AddComponent<GoalZone>();
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

        public static bool HasLeftPlatform(RopePlatformBuilder builder)
        {
            return builder != null && builder.HasPlatformBetween(
                LeftAnchorPosition,
                CenterHookPosition,
                PlatformRopeLength);
        }

        public static bool HasRightPlatform(RopePlatformBuilder builder)
        {
            return builder != null && builder.HasPlatformBetween(
                CenterHookPosition,
                RightAnchorPosition,
                PlatformRopeLength);
        }

        public static bool HasMergedPlatform(RopePlatformBuilder builder)
        {
            return builder != null && builder.HasPlatformBetween(
                LeftAnchorPosition,
                RightAnchorPosition,
                PlatformRopeLength * 2f);
        }

        private static bool EnsurePairedHook(
            string objectName,
            Vector2 position,
            Vector2 size,
            Color color,
            string pairedHookName,
            bool canAttach,
            bool useRingVisual)
        {
            GameObject hook = EnsureObject(
                objectName,
                position,
                size,
                color,
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
            changed |= anchor.Configure(PlatformRopeLength, pairedHookName);

            BoxCollider2D collider = hook.GetComponent<BoxCollider2D>();
            if (collider.enabled != canAttach)
            {
                collider.enabled = canAttach;
                changed = true;
            }
            if (useRingVisual)
            {
                changed |= EnsureBridgeAnchorRingVisual(hook);
            }
            return changed;
        }

        private static bool EnsureBridgeAnchorRingVisual(GameObject hook)
        {
            SpriteRenderer sourceRenderer = hook.GetComponent<SpriteRenderer>();
            bool changed = false;
            Transform visualTransform =
                hook.transform.Find(BridgeAnchorVisualName);
            if (visualTransform == null)
            {
                GameObject visualObject =
                    new GameObject(BridgeAnchorVisualName);
                visualTransform = visualObject.transform;
                visualTransform.SetParent(hook.transform, false);
                changed = true;
            }
            if (visualTransform.localPosition != Vector3.zero)
            {
                visualTransform.localPosition = Vector3.zero;
                changed = true;
            }
            if (visualTransform.localRotation != Quaternion.identity)
            {
                visualTransform.localRotation = Quaternion.identity;
                changed = true;
            }
            if (!visualTransform.TryGetComponent(out SpriteRenderer _))
            {
                visualTransform.gameObject.AddComponent<SpriteRenderer>();
                changed = true;
            }
            if (!visualTransform.TryGetComponent(
                    out RopeAnchorRingVisual ringVisual))
            {
                ringVisual = visualTransform.gameObject.AddComponent<
                    RopeAnchorRingVisual>();
                changed = true;
            }
            changed |= ringVisual.Configure(
                sourceRenderer.sortingLayerID,
                sourceRenderer.sortingOrder + 6);
            if (sourceRenderer.enabled)
            {
                sourceRenderer.enabled = false;
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
            BoxCollider2D collider = terrain.GetComponent<BoxCollider2D>();
            if (!collider.enabled || collider.isTrigger)
            {
                collider.enabled = true;
                collider.isTrigger = false;
                changed = true;
            }
            if (!terrain.TryGetComponent(out Rigidbody2D body))
            {
                body = terrain.AddComponent<Rigidbody2D>();
                changed = true;
            }
            if (body.bodyType != RigidbodyType2D.Static)
            {
                body.bodyType = RigidbodyType2D.Static;
                changed = true;
            }
            if (!terrain.TryGetComponent(out SolidSwingSurface _))
            {
                terrain.AddComponent<SolidSwingSurface>();
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
            if (marker.TryGetComponent(out Collider2D markerCollider))
            {
                if (Application.isPlaying)
                {
                    Object.Destroy(markerCollider);
                }
                else
                {
                    Object.DestroyImmediate(markerCollider);
                }
                changed = true;
            }
            return changed;
        }

        private static GameObject EnsureObject(
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
