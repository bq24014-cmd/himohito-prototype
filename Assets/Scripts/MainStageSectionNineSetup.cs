using UnityEngine;

namespace HimoHito
{
    /// <summary>
    /// Creates section nine: a slow vertical obstacle whose safe timing is observed
    /// before starting the swing.
    /// </summary>
    public static class MainStageSectionNineSetup
    {
        public const string HookName = "Main Section 9 Hook";
        public const string MovingHazardName = "Main Section 9 Moving Hazard";
        public const string LandingName = "Main Section 9 Landing";

        public static GameObject EnsureCreated()
        {
            GameObject sectionEightLanding =
                FindSceneObject(MainStageSectionEightSetup.FinalLandingName);
            if (sectionEightLanding != null)
            {
                if (!sectionEightLanding.TryGetComponent(
                        out MainStageSectionNineCheckpoint checkpoint))
                {
                    checkpoint = sectionEightLanding.AddComponent<MainStageSectionNineCheckpoint>();
                }

                checkpoint.Configure(new Vector2(139.5f, 2.95f));
            }

            GameObject hook = EnsureSolidObject(
                HookName,
                new Vector2(145.5f, 8.5f),
                new Vector2(1.6f, 0.45f),
                new Color(1f, 0.72f, 0.18f));
            if (!hook.TryGetComponent(out HookPoint _))
            {
                hook.AddComponent<HookPoint>();
            }

            GameObject hazard = EnsureSolidObject(
                MovingHazardName,
                new Vector2(150f, 0.8f),
                new Vector2(3.2f, 0.8f),
                new Color(1f, 0.28f, 0.32f));
            BoxCollider2D hazardCollider = hazard.GetComponent<BoxCollider2D>();
            hazardCollider.isTrigger = true;
            if (!hazard.TryGetComponent(out Rigidbody2D hazardBody))
            {
                hazardBody = hazard.AddComponent<Rigidbody2D>();
            }

            hazardBody.bodyType = RigidbodyType2D.Kinematic;
            hazardBody.gravityScale = 0f;
            if (!hazard.TryGetComponent(out MainStageRopeHazard _))
            {
                hazard.AddComponent<MainStageRopeHazard>();
            }

            if (!hazard.TryGetComponent(out MainStageVerticalMover mover))
            {
                mover = hazard.AddComponent<MainStageVerticalMover>();
            }

            mover.Configure(0.8f, 5.8f, 4.8f);

            GameObject landing = EnsureSolidObject(
                LandingName,
                new Vector2(154.8f, 2.3f),
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
    }
}
