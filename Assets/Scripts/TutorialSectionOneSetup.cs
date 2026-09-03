using UnityEngine;

namespace HimoHito
{
    /// <summary>
    /// Keeps tutorial T1 aligned with the 0829 manual: one six-unit valley,
    /// one centered swing Hook, and no additional mechanic in the play space.
    /// </summary>
    public static class TutorialSectionOneSetup
    {
        public const string StartFloorName = "Start Ground";
        public const string HookName = "Tutorial Hook";
        public const string LandingFloorName = "Tutorial Landing";
        public const string LegacyFlashlightSpotName =
            "Tutorial Flashlight Spot";
        public const int StartingRopeLength = 6;
        public const int NextSectionStartingRopeLength = 8;

        private const float PlayerColliderWorldHeight = 1.2f;
        private const float StartGroundingInset = 0.03f;

        public static readonly Vector2 StartFloorPosition =
            new Vector2(-4f, -4.65f);
        public static readonly Vector2 StartFloorSize =
            new Vector2(6f, 10f);
        public static readonly Vector2 HookPosition =
            new Vector2(2f, 5.9f);
        public static readonly Vector2 LandingFloorPosition =
            new Vector2(12f, -4.65f);
        public static readonly Vector2 LandingFloorSize =
            new Vector2(6f, 10f);
        public static readonly Vector2 StartRespawnPosition =
            new Vector2(
                StartFloorPosition.x,
                StartFloorPosition.y + StartFloorSize.y * 0.5f +
                PlayerColliderWorldHeight * 0.5f - StartGroundingInset);
        public static readonly Vector2 LandingRespawnPosition =
            new Vector2(12f, 0.95f);

        private static readonly Vector2 HookSize =
            new Vector2(1.6f, 0.45f);
        private static readonly Color TerrainColor =
            new Color(0.96f, 0.55f, 0.18f);
        private static readonly Color HookColor =
            new Color(0.30f, 0.76f, 1f);

        public static bool ApplyCurrentScene()
        {
            bool changed = RemoveLegacyFlashlightSpot();
            changed |= EnsureTerrain(
                StartFloorName,
                StartFloorPosition,
                StartFloorSize,
                out _);
            changed |= EnsureHook();
            changed |= EnsureTerrain(
                LandingFloorName,
                LandingFloorPosition,
                LandingFloorSize,
                out GameObject landing);

            if (!landing.TryGetComponent(out TutorialCheckpoint checkpoint))
            {
                checkpoint = landing.AddComponent<TutorialCheckpoint>();
                changed = true;
            }
            checkpoint.Configure(
                2,
                LandingRespawnPosition,
                NextSectionStartingRopeLength);
            return changed;
        }

        private static bool RemoveLegacyFlashlightSpot()
        {
            GameObject spot = FindSceneObject(LegacyFlashlightSpotName);
            if (spot == null ||
                Vector2.Distance(
                    spot.transform.position,
                    new Vector2(-4.25f, -3.1f)) > 0.01f)
            {
                return false;
            }

            if (Application.isPlaying)
            {
                Object.Destroy(spot);
            }
            else
            {
                Object.DestroyImmediate(spot);
            }
            return true;
        }

        public static GameObject EnsureCreated()
        {
            ApplyCurrentScene();
            return FindSceneObject(LandingFloorName);
        }

        private static bool EnsureHook()
        {
            GameObject hook = EnsureObject(
                HookName,
                HookPosition,
                HookSize,
                HookColor,
                out bool changed);
            if (!hook.TryGetComponent(out HookPoint hookPoint))
            {
                hookPoint = hook.AddComponent<HookPoint>();
                changed = true;
            }
            changed |= hookPoint.ConfigureFixedAttachmentPoint(Vector2.zero);
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
            Vector2 size,
            out GameObject terrain)
        {
            terrain = EnsureObject(
                objectName,
                position,
                size,
                TerrainColor,
                out bool changed);
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
            return SceneObjectLookup.Find(objectName);
        }
    }
}
