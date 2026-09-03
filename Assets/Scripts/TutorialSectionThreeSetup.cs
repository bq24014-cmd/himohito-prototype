using UnityEngine;

namespace HimoHito
{
    /// <summary>
    /// Builds tutorial T3 and exposes its authored terrain-to-terrain bridge.
    /// </summary>
    public static class TutorialSectionThreeSetup
    {
        public const string LandingFloorName = "Tutorial T3 Landing";
        public const int RequiredRopeLength = 7;
        public const int NextSectionStartingRopeLength = 7;
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

        private const float EndpointSnapDistance = 1.5f;
        private static readonly Color TerrainColor =
            new Color(0.96f, 0.55f, 0.18f);

        public static bool ApplyCurrentScene()
        {
            bool changed = EnsureTerrain(out GameObject landing);
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

        public static bool TryGetAuthoredBridgeEndpoints(
            Vector2 playerPosition,
            Vector2 attachedPoint,
            float ropeLength,
            out Vector2 attachedEndpoint,
            out Vector2 playerEndpoint)
        {
            attachedEndpoint = default;
            playerEndpoint = default;
            if (Mathf.Abs(ropeLength - RequiredRopeLength) > 0.05f ||
                Vector2.Distance(
                    playerPosition,
                    LeftBridgeEndpoint) > EndpointSnapDistance ||
                Vector2.Distance(
                    attachedPoint,
                    RightBridgeEndpoint) > EndpointSnapDistance)
            {
                return false;
            }

            // Keep the attached side first and the player's side last. The
            // platform builder places the player back on the last endpoint.
            attachedEndpoint = RightBridgeEndpoint;
            playerEndpoint = LeftBridgeEndpoint;
            return true;
        }

        private static bool EnsureTerrain(out GameObject terrain)
        {
            terrain = FindSceneObject(LandingFloorName);
            bool changed = false;
            if (terrain == null)
            {
                terrain = new GameObject(LandingFloorName);
                changed = true;
            }
            if (!terrain.activeSelf)
            {
                terrain.SetActive(true);
                changed = true;
            }

            Vector3 targetPosition = new Vector3(
                LandingFloorPosition.x,
                LandingFloorPosition.y,
                0f);
            Vector3 targetScale = new Vector3(
                LandingFloorSize.x,
                LandingFloorSize.y,
                1f);
            if (terrain.transform.position != targetPosition)
            {
                terrain.transform.position = targetPosition;
                changed = true;
            }
            if (terrain.transform.localScale != targetScale)
            {
                terrain.transform.localScale = targetScale;
                changed = true;
            }
            if (!terrain.TryGetComponent(out SpriteRenderer _))
            {
                terrain.AddComponent<SpriteRenderer>();
                changed = true;
            }
            if (!terrain.TryGetComponent(out SolidSprite visual))
            {
                visual = terrain.AddComponent<SolidSprite>();
                changed = true;
            }
            if (visual.Color != TerrainColor)
            {
                visual.Color = TerrainColor;
                changed = true;
            }
            if (!terrain.TryGetComponent(out BoxCollider2D collider))
            {
                collider = terrain.AddComponent<BoxCollider2D>();
                changed = true;
            }
            if (collider.size != Vector2.one)
            {
                collider.size = Vector2.one;
                changed = true;
            }
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

        private static GameObject FindSceneObject(string objectName)
        {
            foreach (GameObject candidate in
                     Resources.FindObjectsOfTypeAll<GameObject>())
            {
                if (candidate.scene.IsValid() &&
                    candidate.scene.name == "Tutorial" &&
                    candidate.name == objectName)
                {
                    return candidate;
                }
            }
            return null;
        }
    }
}
