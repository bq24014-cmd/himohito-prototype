using UnityEngine;

namespace HimoHito
{
    /// <summary>
    /// Builds section six from slide 25 of the 0829 stage manual.
    /// The upper route spends rope on a safe bridge, while the lower
    /// route crosses three Hooks without permanently spending rope.
    /// </summary>
    public static class MainStageSectionSixSetup
    {
        // Older section-six objects kept only so existing scenes can be
        // cleaned up without losing their serialized references.
        public const string HookName = "Main Section 6 Hook";
        public const string FlashlightSpotName =
            "Main Section 6 Flashlight Spot";
        public const string LegacyHazardName =
            "Main Section 6 Rope Hazard";
        public const string LandingName = "Main Section 6 Landing";

        public const string UpperBridgeStartHookName =
            "Main S06 Green Upper Bridge Start Hook";
        public const string UpperBridgeEndHookName =
            "Main S06 Green Upper Bridge End Hook";
        public const string UpperShelfAName = "Main S06 Upper Left";
        public const string UpperShelfBName = "Main S06 Upper Right";
        public const string UpperShelfCName = "Main S06 Upper Final Step";
        public const string LowerHookAName = "Main S06 Lower Hook A";
        public const string LowerHookBName = "Main S06 Lower Hook B";
        public const string LowerHookCName = "Main S06 Lower Hook C";
        public const string MergeName = "Main S06 Merge";

        public const int UpperBridgeRopeLength = 14;
        public const int LowerRouteRopeLength = 8;

        // Section five's right bank ends at X=114.1 with a top of Y=-2.15.
        // Only this start anchor remains on the bank; the remaining route is
        // raised three units to create the intended vertical separation.
        public static readonly Vector2 UpperBridgeStartHookPosition =
            new Vector2(114.1f, -2.15f);
        public static readonly Vector2 UpperBridgeEndHookPosition =
            new Vector2(126.2f, 3.45f);
        public static readonly Vector2 BridgeHookSize =
            new Vector2(0.62f, 0.62f);

        public static readonly Vector2 UpperShelfAPosition =
            new Vector2(127.7f, 3.1f);
        public static readonly Vector2 UpperShelfASize =
            new Vector2(3f, 0.7f);
        public static readonly Vector2 UpperShelfBPosition =
            new Vector2(131.4f, 4.4f);
        public static readonly Vector2 UpperShelfBSize =
            new Vector2(2.6f, 0.7f);
        public static readonly Vector2 UpperShelfCPosition =
            new Vector2(135f, 3.75f);
        public static readonly Vector2 UpperShelfCSize =
            new Vector2(2.6f, 0.7f);

        public static readonly Vector2 LowerHookAPosition =
            new Vector2(121.4f, 0.5f);
        public static readonly Vector2 LowerHookBPosition =
            new Vector2(128.8f, 2.9f);
        public static readonly Vector2 LowerHookCPosition =
            new Vector2(135.6f, 3.1f);
        public static readonly Vector2 NormalHookSize =
            new Vector2(1.6f, 0.45f);

        public static readonly Vector2 MergePosition =
            new Vector2(142.1f, -7.15f);
        public static readonly Vector2 MergeSize =
            new Vector2(8f, 10f);
        public static readonly Vector2 MergeRespawnPosition =
            new Vector2(138.9f, -1.45f);

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
                UpperBridgeStartHookName,
                UpperBridgeStartHookPosition,
                UpperBridgeEndHookName,
                false);
            changed |= EnsureBridgeHook(
                UpperBridgeEndHookName,
                UpperBridgeEndHookPosition,
                UpperBridgeStartHookName,
                true);

            changed |= EnsureTerrain(
                UpperShelfAName,
                UpperShelfAPosition,
                UpperShelfASize);
            changed |= EnsureTerrain(
                UpperShelfBName,
                UpperShelfBPosition,
                UpperShelfBSize);
            changed |= EnsureTerrain(
                UpperShelfCName,
                UpperShelfCPosition,
                UpperShelfCSize);

            changed |= EnsureHook(LowerHookAName, LowerHookAPosition);
            changed |= EnsureHook(LowerHookBName, LowerHookBPosition);
            changed |= EnsureHook(LowerHookCName, LowerHookCPosition);

            changed |= EnsureTerrain(MergeName, MergePosition, MergeSize);
            GameObject merge = FindSceneObject(MergeName);
            if (merge != null)
            {
                if (!merge.TryGetComponent(out MainStageCheckpoint checkpoint))
                {
                    checkpoint = merge.AddComponent<MainStageCheckpoint>();
                    changed = true;
                }

                // 25 is only a safety lower bound. The lower route arrives
                // with 39, so its fourteen-rope advantage remains after merging.
                checkpoint.Configure(7, MergeRespawnPosition, 25f);
            }

            return changed;
        }

        public static GameObject EnsureCreated()
        {
            ApplyCurrentScene();
            return FindSceneObject(MergeName);
        }

        private static bool DisableLegacyObjects()
        {
            bool changed = false;
            string[] legacyNames =
            {
                HookName,
                FlashlightSpotName,
                LegacyHazardName,
                LandingName,
                "Main S06 Safety Floor",
                "Main S06 Return Step",
                MainStageSectionSevenSetup.HookName,
                MainStageSectionSevenSetup.FinalHookName,
                MainStageSectionSevenSetup.LandingName,
                MainStageSectionNineSetup.HookName,
                MainStageSectionNineSetup.FlashlightSpotName,
                MainStageSectionNineSetup.FlashlightSourceName,
                MainStageSectionNineSetup.LandingName
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

            if (!hook.TryGetComponent(out RopePlatformAnchor platformAnchor))
            {
                platformAnchor = hook.AddComponent<RopePlatformAnchor>();
                changed = true;
            }
            changed |= platformAnchor.Configure(
                UpperBridgeRopeLength,
                pairedHookName);

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
                if (Application.isPlaying)
                {
                    Object.Destroy(platformAnchor);
                }
                else
                {
                    Object.DestroyImmediate(platformAnchor);
                }
                changed = true;
            }

            if (hook.TryGetComponent(out BoxCollider2D collider) &&
                (!collider.enabled || collider.isTrigger))
            {
                collider.enabled = true;
                collider.isTrigger = false;
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
    }
}
