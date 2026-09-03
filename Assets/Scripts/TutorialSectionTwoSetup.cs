using UnityEngine;

namespace HimoHito
{
    /// <summary>
    /// Builds tutorial T2 so rope lengths six and seven clear the spikes,
    /// while the initial length eight drops the player into them.
    /// </summary>
    public static class TutorialSectionTwoSetup
    {
        public const string HookName = "Tutorial T2 Hook";
        public const string LandingFloorName = "Tutorial T2 Landing";
        public const string LeftSpikeName = "Tutorial T2 Spike Left";
        public const string CenterSpikeName = "Tutorial T2 Spike Center";
        public const string RightSpikeName = "Tutorial T2 Spike Right";
        public const int NextSectionStartingRopeLength = 7;

        public static readonly Vector2 HookPosition =
            new Vector2(18f, 5.5f);
        public static readonly Vector2 LandingFloorPosition =
            new Vector2(27f, -4.65f);
        public static readonly Vector2 LandingFloorSize =
            new Vector2(6f, 10f);
        public static readonly Vector2 LandingRespawnPosition =
            new Vector2(27f, 0.95f);

        private static readonly Vector2 HookSize =
            new Vector2(1.6f, 0.45f);
        private static readonly Vector2 SpikeSize =
            new Vector2(0.55f, 0.6f);
        private static readonly Color TerrainColor =
            new Color(0.96f, 0.55f, 0.18f);
        private static readonly Color HookColor =
            new Color(0.30f, 0.76f, 1f);
        private static readonly Color SpikeColor =
            new Color(1f, 0.18f, 0.25f);

        public static bool ApplyCurrentScene()
        {
            bool changed = EnsureHook();
            changed |= EnsureTerrain(out GameObject landing);
            changed |= EnsureSpike(
                LeftSpikeName,
                new Vector2(17.35f, -2.65f));
            changed |= EnsureSpike(
                CenterSpikeName,
                new Vector2(18f, -2.65f));
            changed |= EnsureSpike(
                RightSpikeName,
                new Vector2(18.65f, -2.65f));

            if (!landing.TryGetComponent(out TutorialCheckpoint checkpoint))
            {
                checkpoint = landing.AddComponent<TutorialCheckpoint>();
                changed = true;
            }
            checkpoint.Configure(
                3,
                LandingRespawnPosition,
                NextSectionStartingRopeLength);
            return changed;
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
            if (hook.TryGetComponent(out BoxCollider2D collider))
            {
                if (!collider.enabled)
                {
                    collider.enabled = true;
                    changed = true;
                }
                if (!collider.isTrigger)
                {
                    collider.isTrigger = true;
                    changed = true;
                }
            }
            return changed;
        }

        private static bool EnsureTerrain(out GameObject terrain)
        {
            terrain = EnsureObject(
                LandingFloorName,
                LandingFloorPosition,
                LandingFloorSize,
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

        private static bool EnsureSpike(
            string objectName,
            Vector2 position)
        {
            GameObject spike = EnsureObject(
                objectName,
                position,
                SpikeSize,
                SpikeColor,
                out bool changed);
            BoxCollider2D collider = spike.GetComponent<BoxCollider2D>();
            Vector2 triggerSize = new Vector2(0.86f, 0.9f);
            Vector2 triggerOffset = new Vector2(0f, -0.03f);
            if (collider.size != triggerSize)
            {
                collider.size = triggerSize;
                changed = true;
            }
            if (collider.offset != triggerOffset)
            {
                collider.offset = triggerOffset;
                changed = true;
            }
            if (!collider.isTrigger)
            {
                collider.isTrigger = true;
                changed = true;
            }
            if (!spike.TryGetComponent(out RopeSpikeHazard _))
            {
                spike.AddComponent<RopeSpikeHazard>();
                changed = true;
            }
            if (!spike.TryGetComponent(out ToySpikeVisual toyVisual))
            {
                toyVisual = spike.AddComponent<ToySpikeVisual>();
                changed = true;
            }
            changed |= toyVisual.Refresh();
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
            return SceneObjectLookup.Find(objectName, "Tutorial");
        }
    }
}
