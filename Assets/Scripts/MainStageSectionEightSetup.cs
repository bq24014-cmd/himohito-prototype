using UnityEngine;

namespace HimoHito
{
    /// <summary>
    /// Creates section eight: two visible swings that ask the player to reserve
    /// enough rope for the second move instead of optimizing only the first.
    /// </summary>
    public static class MainStageSectionEightSetup
    {
        public const string FirstHookName = "Main Section 8 Hook A";
        public const string PlanningLandingName = "Main Section 8 Planning Landing";
        public const string SecondHookName = "Main Section 8 Hook B";
        public const string FinalLandingName = "Main Section 8 Landing";

        public static GameObject EnsureCreated()
        {
            GameObject sectionSevenLanding =
                FindSceneObject(MainStageSectionSevenSetup.LandingName);
            if (sectionSevenLanding != null)
            {
                if (!sectionSevenLanding.TryGetComponent(
                        out MainStageSectionEightCheckpoint checkpoint))
                {
                    checkpoint = sectionSevenLanding.AddComponent<MainStageSectionEightCheckpoint>();
                }

                checkpoint.Configure(new Vector2(116.5f, 0.15f));
            }

            EnsureHook(
                FirstHookName,
                new Vector2(121.5f, 4.3f));

            EnsureSolidObject(
                PlanningLandingName,
                new Vector2(127f, -0.2f),
                new Vector2(6f, 0.7f),
                new Color(0.38f, 0.41f, 0.52f));

            EnsureHook(
                SecondHookName,
                new Vector2(129f, 8.5f));

            GameObject finalLanding = EnsureSolidObject(
                FinalLandingName,
                new Vector2(137f, 2f),
                new Vector2(7f, 0.7f),
                new Color(0.38f, 0.41f, 0.52f));
            if (!finalLanding.TryGetComponent(out MainStageSectionTarget _))
            {
                finalLanding.AddComponent<MainStageSectionTarget>();
            }

            return finalLanding;
        }

        private static void EnsureHook(string name, Vector2 position)
        {
            GameObject hook = EnsureSolidObject(
                name,
                position,
                new Vector2(1.6f, 0.45f),
                new Color(1f, 0.72f, 0.18f));
            if (!hook.TryGetComponent(out HookPoint _))
            {
                hook.AddComponent<HookPoint>();
            }
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
