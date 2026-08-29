using UnityEngine;

namespace HimoHito
{
    /// <summary>
    /// Creates the agreed section-six graybox when it is not yet saved in MainStage.
    /// The editor builder uses the same method, so runtime and generated layouts match.
    /// </summary>
    public static class MainStageSectionSixSetup
    {
        public const string HookName = "Main Section 6 Hook";
        public const string FlashlightSpotName = "Main Section 6 Flashlight Spot";
        public const string LegacyHazardName = "Main Section 6 Rope Hazard";
        public const string LandingName = "Main Section 6 Landing";

        public static GameObject EnsureCreated()
        {
            GameObject hook = EnsureSolidObject(
                HookName,
                new Vector2(82f, 3.2f),
                new Vector2(1.6f, 0.45f),
                new Color(1f, 0.72f, 0.18f));
            if (!hook.TryGetComponent(out HookPoint _))
            {
                hook.AddComponent<HookPoint>();
            }

            GameObject legacyHazard = FindSceneObject(LegacyHazardName);
            if (legacyHazard != null && FindSceneObject(FlashlightSpotName) == null)
            {
                legacyHazard.name = FlashlightSpotName;
            }

            GameObject hazard = FindSceneObject(FlashlightSpotName);
            if (hazard == null)
            {
                hazard = new GameObject(FlashlightSpotName);
            }

            FlashlightSpotVisual.ConfigureSpot(
                hazard,
                new Vector2(83.2f, -4.8f),
                4.4f,
                new Color(1f, 1f, 1f, 0.72f));
            if (!hazard.TryGetComponent(out Rigidbody2D hazardBody))
            {
                hazardBody = hazard.AddComponent<Rigidbody2D>();
            }

            hazardBody.bodyType = RigidbodyType2D.Kinematic;
            hazardBody.gravityScale = 0f;
            if (!hazard.TryGetComponent(out MainStageRopeHazard ropeHazard))
            {
                ropeHazard = hazard.AddComponent<MainStageRopeHazard>();
            }
            ropeHazard.ConfigureRopePlatformBlocking(false);

            if (!hazard.TryGetComponent(out MainStageHorizontalMover mover))
            {
                mover = hazard.AddComponent<MainStageHorizontalMover>();
            }

            mover.Configure(79.2f, 87.2f, 8.9f, 3f, 0.6f);

            GameObject landing = EnsureSolidObject(
                LandingName,
                new Vector2(93.7f, -0.8f),
                new Vector2(8f, 0.7f),
                new Color(0.38f, 0.41f, 0.52f));
            if (!landing.TryGetComponent(out MainStageSectionTarget _))
            {
                landing.AddComponent<MainStageSectionTarget>();
            }

            return landing;
        }

        private static GameObject FindSceneObject(string name)
        {
            foreach (GameObject candidate in Resources.FindObjectsOfTypeAll<GameObject>())
            {
                if (candidate.scene.IsValid() && candidate.name == name)
                {
                    return candidate;
                }
            }

            return null;
        }

        private static GameObject EnsureSolidObject(
            string name,
            Vector2 position,
            Vector2 size,
            Color color)
        {
            GameObject gameObject = GameObject.Find(name);
            if (gameObject == null)
            {
                gameObject = new GameObject(name);
            }

            gameObject.transform.position = position;
            gameObject.transform.localScale = new Vector3(size.x, size.y, 1f);
            if (!gameObject.TryGetComponent(out SpriteRenderer _))
            {
                gameObject.AddComponent<SpriteRenderer>();
            }

            if (!gameObject.TryGetComponent(out SolidSprite visual))
            {
                visual = gameObject.AddComponent<SolidSprite>();
            }

            visual.Color = color;
            if (!gameObject.TryGetComponent(out BoxCollider2D collider))
            {
                collider = gameObject.AddComponent<BoxCollider2D>();
            }

            collider.size = Vector2.one;
            return gameObject;
        }
    }
}
