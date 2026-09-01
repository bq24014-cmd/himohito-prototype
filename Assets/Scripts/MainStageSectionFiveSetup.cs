using UnityEngine;

namespace HimoHito
{
    /// <summary>
    /// Builds section five from slide 24 of the 0829 stage manual.
    /// A length-six bridge between the green shelf Hooks blocks the light,
    /// then the central blue Hook provides the final swing to the right bank.
    /// </summary>
    public static class MainStageSectionFiveSetup
    {
        public const string LeftShelfName = "Main S05 Left Shelf";
        public const string RightShelfName = "Main S05 Right Shelf";
        public const string BridgeStartHookName =
            "Main Section 5 Green Shadow Start Hook";
        public const string BridgeEndHookName =
            "Main Section 5 Green Shadow End Hook";
        public const string LightMountName = "Main S05 Flashlight Source";
        public const string LightSpotName = "Main S05 Flashlight Spot";
        public const string CentralHookName = "Main S05 Central Hook";
        public const string LandingName = "Main S05 Landing";

        public const int BridgeRopeLength = 6;
        public const int SwingRopeLength = 7;

        // Section four's landing top is Y=-2.15. The shelf tops are 1.5 higher.
        public static readonly Vector2 LeftShelfPosition =
            new Vector2(85.6f, -0.95f);
        public static readonly Vector2 RightShelfPosition =
            new Vector2(93f, -0.95f);
        public static readonly Vector2 ShelfSize = new Vector2(3f, 0.6f);
        public static readonly Vector2 BridgeStartHookPosition =
            new Vector2(87.1f, -0.65f);
        public static readonly Vector2 BridgeEndHookPosition =
            new Vector2(91.5f, -0.65f);
        public static readonly Vector2 BridgeHookSize =
            new Vector2(0.62f, 0.62f);
        public static readonly Vector2 CentralHookPosition =
            new Vector2(97f, 2.45f);
        public static readonly Vector2 LightMountPosition =
            new Vector2(89.3f, 2.85f);
        public static readonly Vector2 LightSpotPosition =
            new Vector2(89.3f, -3.15f);
        public const float LightSpotDiameter = 4f;
        public static readonly Vector2 LandingPosition =
            new Vector2(106.1f, -7.15f);
        public static readonly Vector2 LandingSize = new Vector2(8f, 10f);
        public static readonly Vector2 LandingRespawnPosition =
            new Vector2(103.4f, -1.45f);

        private static readonly Color ShelfColor =
            new Color(0.56f, 0.29f, 0.09f);
        private static readonly Color HookColor =
            new Color(0.298f, 0.765f, 1f);
        private static readonly Color BridgeHookColor =
            new Color(0.33f, 1f, 0.76f);
        private static readonly Color MountColor =
            new Color(0.42f, 0.24f, 0.12f);
        private static readonly Color LightColor =
            new Color(1f, 0.82f, 0.56f, 0.62f);

        public static bool ApplyCurrentScene()
        {
            bool changed = false;
            changed |= DisableLegacySectionSixObjects();
            changed |= EnsureTerrain(
                LeftShelfName,
                LeftShelfPosition,
                ShelfSize);
            changed |= EnsureTerrain(
                RightShelfName,
                RightShelfPosition,
                ShelfSize);
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

            GameObject centralHook = EnsureObject(
                CentralHookName,
                CentralHookPosition,
                new Vector2(1.6f, 0.45f),
                HookColor,
                out bool centralHookChanged);
            changed |= centralHookChanged;
            if (!centralHook.TryGetComponent(out HookPoint centralHookPoint))
            {
                centralHookPoint = centralHook.AddComponent<HookPoint>();
                changed = true;
            }
            changed |= centralHookPoint.ConfigureFixedAttachmentPoint(Vector2.zero);

            changed |= EnsureLightMount();
            GameObject lightMount = FindSceneObject(LightMountName);
            changed |= EnsureLightSpot(
                lightMount != null
                    ? lightMount.transform
                    : centralHook.transform);
            changed |= EnsureTerrain(
                LandingName,
                LandingPosition,
                LandingSize);

            GameObject landing = FindSceneObject(LandingName);
            if (landing != null)
            {
                if (!landing.TryGetComponent(out MainStageCheckpoint checkpoint))
                {
                    checkpoint = landing.AddComponent<MainStageCheckpoint>();
                    changed = true;
                }
                checkpoint.Configure(6, LandingRespawnPosition, 34f);
            }

            return changed;
        }

        private static bool DisableLegacySectionSixObjects()
        {
            bool changed = false;
            string[] legacyNames =
            {
                MainStageSectionSixSetup.HookName,
                MainStageSectionSixSetup.FlashlightSpotName,
                MainStageSectionSixSetup.LandingName
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

        private static bool EnsureTerrain(
            string objectName,
            Vector2 position,
            Vector2 size)
        {
            GameObject terrain = EnsureObject(
                objectName,
                position,
                size,
                ShelfColor,
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
            if (!hook.TryGetComponent(out RopePlatformAnchor platformAnchor))
            {
                platformAnchor = hook.AddComponent<RopePlatformAnchor>();
                changed = true;
            }
            changed |= platformAnchor.Configure(
                BridgeRopeLength,
                pairedHookName);
            if (hook.TryGetComponent(out BoxCollider2D collider) &&
                collider.enabled != canAttach)
            {
                collider.enabled = canAttach;
                changed = true;
            }
            return changed;
        }

        private static bool EnsureLightMount()
        {
            GameObject mount = EnsureObject(
                LightMountName,
                LightMountPosition,
                new Vector2(1.2f, 0.8f),
                MountColor,
                out bool changed);
            if (mount.TryGetComponent(out BoxCollider2D collider) &&
                collider.enabled)
            {
                collider.enabled = false;
                changed = true;
            }
            return changed;
        }

        private static bool EnsureLightSpot(Transform source)
        {
            bool changed = false;
            GameObject spot = FindSceneObject(LightSpotName);
            if (spot == null)
            {
                spot = new GameObject(LightSpotName);
                changed = true;
            }

            Vector3 targetPosition = new Vector3(
                LightSpotPosition.x,
                LightSpotPosition.y,
                0f);
            Vector3 targetScale = new Vector3(
                LightSpotDiameter,
                LightSpotDiameter,
                1f);
            changed |= spot.transform.position != targetPosition ||
                       spot.transform.localScale != targetScale;
            FlashlightSpotVisual.ConfigureSpot(
                spot,
                LightSpotPosition,
                LightSpotDiameter,
                LightColor);
            if (!spot.TryGetComponent(out PlatformOccludedLightHazard hazard))
            {
                hazard = spot.AddComponent<PlatformOccludedLightHazard>();
                changed = true;
            }
            hazard.Configure(source);
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
            foreach (GameObject candidate in
                     Resources.FindObjectsOfTypeAll<GameObject>())
            {
                if (candidate.scene.IsValid() &&
                    candidate.scene.name == "MainStage" &&
                    candidate.name == objectName)
                {
                    return candidate;
                }
            }
            return null;
        }
    }
}
