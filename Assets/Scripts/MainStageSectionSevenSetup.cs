using UnityEngine;

namespace HimoHito
{
    /// <summary>
    /// Builds section seven from slide 26 of the 0829 stage manual.
    /// The player must create the lower-to-middle bridge before the upper
    /// shelf makes the final swing Hook useful.
    /// </summary>
    public static class MainStageSectionSevenSetup
    {
        // Legacy names are retained so older serialized scenes can be cleaned.
        public const string HookName = "Main Section 7 Hook";
        public const string FinalHookName = "Main Section 7 Final Hook";
        public const string WeaveFrameName = "Main Section 7 Weave Frame";
        public const string LegacyWovenPlatformName =
            "Main Section 7 Woven Platform";
        public const string WeaveMarkerName = "Main Section 7 Weave Marker";
        public const string LandingName = "Main Section 7 Landing";

        public const string BridgeStartHookName =
            "Main S07 Green Lower Bridge Start Hook";
        public const string BridgeEndHookName =
            "Main S07 Green Middle Bridge End Hook";
        public const string MiddleShelfName = "Main S07 Middle Shelf";
        public const string UpperShelfName = "Main S07 Upper Step";
        public const string UpperHookName = "Main S07 Upper Hook";
        public const string GoalFloorName = "Main S07 Landing";

        public const int BridgeRopeLength = 4;
        public const int FinalSwingRopeLength = 5;

        // The merge floor from section six is the lower floor in the slide.
        public static readonly Vector2 BridgeStartHookPosition =
            new Vector2(150.1f, -2.15f);
        public static readonly Vector2 BridgeEndHookPosition =
            new Vector2(152.9f, -0.35f);
        public static readonly Vector2 MiddleShelfPosition =
            new Vector2(154.4f, -0.7f);
        public static readonly Vector2 MiddleShelfSize =
            new Vector2(3f, 0.7f);
        // Jump impulse 10 against gravity 9.81 * 2.8 rises 1.82 at the apex.
        // The player reaches the 1.1-wide horizontal gap after about 0.20 s,
        // where the rise is only about 1.46. A 1.2 step keeps collision margin.
        public static readonly Vector2 UpperShelfPosition =
            new Vector2(158.5f, 0.5f);
        public static readonly Vector2 UpperShelfSize =
            new Vector2(3f, 0.7f);
        public static readonly Vector2 UpperHookPosition =
            new Vector2(162.35f, 3.7f);
        public static readonly Vector2 GoalFloorPosition =
            new Vector2(171f, -7.15f);
        public static readonly Vector2 GoalFloorSize =
            new Vector2(8f, 10f);
        public static readonly Vector2 GoalRespawnPosition =
            new Vector2(167.8f, -1.45f);

        private static readonly Vector2 BridgeHookSize =
            new Vector2(0.62f, 0.62f);
        private static readonly Vector2 NormalHookSize =
            new Vector2(1.6f, 0.45f);
        private static readonly Color TerrainColor =
            new Color(0.56f, 0.29f, 0.09f);
        private static readonly Color HookColor =
            new Color(0.298f, 0.765f, 1f);
        private static readonly Color BridgeHookColor =
            new Color(0.33f, 1f, 0.76f);

        public static bool ApplyCurrentScene()
        {
            bool changed = DisableLegacyObjects();

            changed |= EnsureBridgeHook(
                BridgeStartHookName,
                BridgeStartHookPosition,
                BridgeEndHookName,
                false);
            changed |= EnsureBridgeHook(
                BridgeEndHookName,
                BridgeEndHookPosition,
                BridgeStartHookName,
                true);
            changed |= EnsureTerrain(
                MiddleShelfName,
                MiddleShelfPosition,
                MiddleShelfSize);
            changed |= EnsureTerrain(
                UpperShelfName,
                UpperShelfPosition,
                UpperShelfSize);
            changed |= EnsureHook(UpperHookName, UpperHookPosition);
            changed |= EnsureTerrain(
                GoalFloorName,
                GoalFloorPosition,
                GoalFloorSize);

            GameObject goalFloor = FindSceneObject(GoalFloorName);
            if (goalFloor != null)
            {
                if (!goalFloor.TryGetComponent(out MainStageCheckpoint checkpoint))
                {
                    checkpoint = goalFloor.AddComponent<MainStageCheckpoint>();
                    changed = true;
                }

                // Preserve the section-six route difference while guaranteeing
                // the slide's section-nine entry lower bound. Section eight
                // was removed, so this checkpoint advances directly to nine.
                checkpoint.Configure(9, GoalRespawnPosition, 23f);
            }

            return changed;
        }

        public static GameObject EnsureCreated()
        {
            ApplyCurrentScene();
            return FindSceneObject(GoalFloorName);
        }

        private static bool DisableLegacyObjects()
        {
            bool changed = false;
            string[] legacyNames =
            {
                HookName,
                FinalHookName,
                WeaveFrameName,
                LegacyWovenPlatformName,
                WeaveMarkerName,
                LandingName,
                "Main S07 Lower Start"
            };

            foreach (string legacyName in legacyNames)
            {
                GameObject legacy = FindSceneObject(legacyName);
                if (legacy != null && legacy.activeSelf)
                {
                    legacy.SetActive(false);
                    changed = true;
                }
            }

            return changed;
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
            changed |= anchor.Configure(BridgeRopeLength, pairedHookName);

            if (hook.TryGetComponent(out BoxCollider2D collider) &&
                collider.enabled != canAttach)
            {
                collider.enabled = canAttach;
                changed = true;
            }
            return changed;
        }

        private static bool EnsureHook(string objectName, Vector2 position)
        {
            GameObject hook = EnsureObject(
                objectName,
                position,
                NormalHookSize,
                HookColor,
                out bool changed);
            if (!hook.TryGetComponent(out HookPoint hookPoint))
            {
                hookPoint = hook.AddComponent<HookPoint>();
                changed = true;
            }
            changed |= hookPoint.ConfigureFixedAttachmentPoint(Vector2.zero);

            if (hook.TryGetComponent(out RopePlatformAnchor platformAnchor))
            {
                DestroyComponent(platformAnchor);
                changed = true;
            }
            if (hook.TryGetComponent(out BoxCollider2D collider) &&
                !collider.enabled)
            {
                collider.enabled = true;
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

        private static void DestroyComponent(Object component)
        {
            if (Application.isPlaying)
            {
                Object.Destroy(component);
            }
            else
            {
                Object.DestroyImmediate(component);
            }
        }

        private static GameObject FindSceneObject(string objectName)
        {
            return SceneObjectLookup.Find(objectName);
        }
    }
}
