using UnityEngine;

namespace HimoHito
{
    /// <summary>
    /// Creates section eight: a permanent rope platform must be placed as a
    /// shield before the player makes the main swing through the flashlight.
    /// </summary>
    public static class MainStageSectionNineSetup
    {
        public const string HookName = "Main Section 8 Hook";
        public const string FlashlightSpotName = "Main Section 8 Flashlight Spot";
        public const string LegacyHookName = "Main Section 9 Hook";
        public const string LegacyFlashlightSpotName = "Main Section 9 Flashlight Spot";
        public const string LegacyFlashlightBeamName = "Main Section 9 Flashlight Beam";
        public const string LegacyMovingHazardName = "Main Section 9 Moving Hazard";
        public const string LandingName = "Main Section 8 Landing";
        public const string LegacyLandingName = "Main Section 9 Landing";

        public static GameObject EnsureCreated()
        {
            RenameLegacyObject(LegacyHookName, HookName);
            RenameLegacyObject(LegacyFlashlightSpotName, FlashlightSpotName);
            RenameLegacyObject(LegacyLandingName, LandingName);

            GameObject sectionSevenLanding =
                FindSceneObject(MainStageSectionSevenSetup.LandingName);
            if (sectionSevenLanding != null)
            {
                if (!sectionSevenLanding.TryGetComponent(
                        out MainStageSectionNineCheckpoint checkpoint))
                {
                    checkpoint = sectionSevenLanding.AddComponent<MainStageSectionNineCheckpoint>();
                }

                checkpoint.Configure(new Vector2(136.5f, 0.15f));
            }

            GameObject hook = EnsureSolidObject(
                HookName,
                new Vector2(149.5f, 4.8f),
                new Vector2(1.6f, 0.45f),
                new Color(1f, 0.72f, 0.18f));
            if (!hook.TryGetComponent(out HookPoint _))
            {
                hook.AddComponent<HookPoint>();
            }

            GameObject legacyHazard = FindSceneObject(LegacyFlashlightBeamName) ??
                                      FindSceneObject(LegacyMovingHazardName);
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
                new Vector2(154f, 0f),
                6f,
                new Color(1f, 1f, 1f, 0.72f));
            if (!hazard.TryGetComponent(out Rigidbody2D hazardBody))
            {
                hazardBody = hazard.AddComponent<Rigidbody2D>();
            }

            hazardBody.bodyType = RigidbodyType2D.Kinematic;
            hazardBody.gravityScale = 0f;
            hazardBody.position = new Vector2(154f, 0f);
            if (!hazard.TryGetComponent(out MainStageRopeHazard ropeHazard))
            {
                ropeHazard = hazard.AddComponent<MainStageRopeHazard>();
            }
            ropeHazard.ConfigureRopePlatformBlocking(true);

            if (hazard.TryGetComponent(out MainStageVerticalMover mover))
            {
                mover.enabled = false;
            }

            GameObject landing = EnsureSolidObject(
                LandingName,
                new Vector2(164f, -0.5f),
                new Vector2(7f, 0.7f),
                new Color(0.38f, 0.41f, 0.52f));
            if (!landing.TryGetComponent(out MainStageSectionTarget _))
            {
                landing.AddComponent<MainStageSectionTarget>();
            }

            return landing;
        }

        private static GameObject EnsureSolidObject(
            string name,
            Vector2 position,
            Vector2 size,
            Color color)
        {
            GameObject gameObject = FindSceneObject(name);
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

        private static void RenameLegacyObject(string legacyName, string currentName)
        {
            if (FindSceneObject(currentName) != null)
            {
                return;
            }

            GameObject legacyObject = FindSceneObject(legacyName);
            if (legacyObject != null)
            {
                legacyObject.name = currentName;
            }
        }
    }
}
