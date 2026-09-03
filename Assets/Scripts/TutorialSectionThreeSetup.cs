using UnityEngine;

namespace HimoHito
{
    /// <summary>
    /// Builds tutorial T3 and exposes its authored terrain-to-terrain bridge.
    /// </summary>
    public static class TutorialSectionThreeSetup
    {
        public const string LandingFloorName = "Tutorial T3 Landing";
        public const string BridgeStartHookName =
            "Tutorial T3 Green Bridge Start Hook";
        public const string BridgeEndHookName =
            "Tutorial T3 Green Bridge End Hook";
        public const int RequiredRopeLength = 7;
        public const int NextSectionStartingRopeLength = 6;
        public const float StartingRopeAmount = 20f;

        public static readonly Vector2 LandingFloorPosition =
            new Vector2(39f, -4.65f);
        public static readonly Vector2 LandingFloorSize =
            new Vector2(6f, 10f);
        public static readonly Vector2 LandingRespawnPosition =
            new Vector2(39f, 0.95f);
        public static readonly Vector2 LeftBridgeEndpoint =
            new Vector2(30f, 0.35f);
        public static readonly Vector2 RightBridgeEndpoint =
            new Vector2(36f, 0.35f);

        private const string BridgeAnchorVisualName =
            "Green Rope Anchor Ring Visual";
        private static readonly Vector2 BridgeHookSize =
            new Vector2(0.62f, 0.62f);
        private static readonly Color TerrainColor =
            new Color(0.96f, 0.55f, 0.18f);
        private static readonly Color BridgeHookColor =
            new Color(0.33f, 1f, 0.76f);

        public static bool ApplyCurrentScene()
        {
            bool changed = EnsureTerrain(out GameObject landing);
            changed |= EnsureBridgeHook(
                BridgeStartHookName,
                LeftBridgeEndpoint,
                BridgeEndHookName,
                false);
            changed |= EnsureBridgeHook(
                BridgeEndHookName,
                RightBridgeEndpoint,
                BridgeStartHookName,
                true);
            if (!landing.TryGetComponent(out TutorialCheckpoint checkpoint))
            {
                checkpoint = landing.AddComponent<TutorialCheckpoint>();
                changed = true;
            }
            checkpoint.Configure(
                4,
                LandingRespawnPosition,
                NextSectionStartingRopeLength);
            return changed;
        }

        public static GameObject EnsureCreated()
        {
            ApplyCurrentScene();
            return FindSceneObject(LandingFloorName);
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
            changed |= anchor.Configure(
                RequiredRopeLength,
                pairedHookName);

            BoxCollider2D collider = hook.GetComponent<BoxCollider2D>();
            if (collider.enabled != canAttach)
            {
                collider.enabled = canAttach;
                changed = true;
            }
            changed |= EnsureBridgeAnchorRingVisual(hook);
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

        private static bool EnsureTerrain(out GameObject terrain)
        {
            terrain = EnsureObject(
                LandingFloorName,
                LandingFloorPosition,
                LandingFloorSize,
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
