using System;
using System.Collections.Generic;
using UnityEngine;

namespace HimoHito
{
    /// <summary>
    /// Builds section nine from slide 28 of the 0829 stage manual. Two
    /// length-six platforms meet at the center Hook. Removing that Hook with
    /// F joins them into one deeper length-twelve path beneath the beam.
    /// </summary>
    public static class MainStageSectionNineSetup
    {
        public const string LeftAnchorName = "Main S09 Green Left Bank";
        public const string CenterHookName = "Main S09 Center Hook";
        public const string RightAnchorName = "Main S09 Green Right Bank";
        public const string BeamName = "Main S09 Hanging Beam";
        public const string GoalFloorName = "Main S09 Goal Floor";
        public const int PlatformRopeLength = 6;

        private const string SectionEightPrefix = "Main S08 ";

        public static readonly Vector2 LeftAnchorPosition =
            new Vector2(175f, -2.15f);
        public static readonly Vector2 CenterHookPosition =
            new Vector2(180.5f, -1.05f);
        public static readonly Vector2 RightAnchorPosition =
            new Vector2(186f, -2.15f);
        public static readonly Vector2 BeamPosition =
            new Vector2(182f, 1.55f);
        public static readonly Vector2 BeamSize =
            new Vector2(2f, 5f);
        public static readonly Vector2 GoalFloorPosition =
            new Vector2(190f, -7.15f);
        public static readonly Vector2 GoalFloorSize =
            new Vector2(8f, 10f);
        public static readonly Vector2 GoalRespawnPosition =
            new Vector2(187f, -1.45f);

        private static readonly Vector2 BridgeHookSize =
            new Vector2(0.62f, 0.62f);
        private static readonly Vector2 SwingHookSize =
            new Vector2(1.6f, 0.45f);
        private static readonly Color TerrainColor =
            new Color(0.56f, 0.29f, 0.09f);
        private static readonly Color BridgeHookColor =
            new Color(0.33f, 1f, 0.76f);
        private static readonly Color SwingHookColor =
            new Color(0.30f, 0.76f, 1f);

        public static bool ApplyCurrentScene()
        {
            bool changed = RemoveSectionEightObjects();
            changed |= EnsurePairedHook(
                LeftAnchorName,
                LeftAnchorPosition,
                BridgeHookSize,
                BridgeHookColor,
                CenterHookName,
                false);
            changed |= EnsurePairedHook(
                CenterHookName,
                CenterHookPosition,
                SwingHookSize,
                SwingHookColor,
                LeftAnchorName,
                true);
            changed |= EnsurePairedHook(
                RightAnchorName,
                RightAnchorPosition,
                BridgeHookSize,
                BridgeHookColor,
                CenterHookName,
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
            if (!goalFloor.TryGetComponent(out MainStageCheckpoint checkpoint))
            {
                checkpoint = goalFloor.AddComponent<MainStageCheckpoint>();
                changed = true;
            }
            checkpoint.Configure(
                10,
                GoalRespawnPosition,
                MainStageSectionTenSetup.MinimumRopeAtEntry);
            return changed;
        }

        public static GameObject EnsureCreated()
        {
            ApplyCurrentScene();
            return FindSceneObject(GoalFloorName);
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
            bool canAttach)
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

        private static bool RemoveSectionEightObjects()
        {
            List<GameObject> obsolete = new();
            foreach (GameObject candidate in
                     Resources.FindObjectsOfTypeAll<GameObject>())
            {
                if (candidate.scene.IsValid() &&
                    candidate.name.StartsWith(
                        SectionEightPrefix,
                        StringComparison.Ordinal))
                {
                    obsolete.Add(candidate);
                }
            }

            foreach (GameObject target in obsolete)
            {
                target.SetActive(false);
                if (Application.isPlaying)
                {
                    UnityEngine.Object.Destroy(target);
                }
                else
                {
                    UnityEngine.Object.DestroyImmediate(target);
                }
            }
            return obsolete.Count > 0;
        }

        private static GameObject FindSceneObject(string objectName)
        {
            return SceneObjectLookup.Find(objectName);
        }
    }
}
