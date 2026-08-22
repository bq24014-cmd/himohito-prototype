using UnityEngine;

namespace HimoHito
{
    /// <summary>
    /// Creates section ten: one large, forgiving final swing into the goal.
    /// </summary>
    public static class MainStageSectionTenSetup
    {
        public const string HookName = "Main Section 10 Final Hook";
        public const string GoalName = "Main Stage Goal";

        public static GameObject EnsureCreated()
        {
            GameObject sectionNineLanding =
                FindSceneObject(MainStageSectionNineSetup.LandingName);
            if (sectionNineLanding != null)
            {
                if (!sectionNineLanding.TryGetComponent(
                        out MainStageSectionTenCheckpoint checkpoint))
                {
                    checkpoint = sectionNineLanding.AddComponent<MainStageSectionTenCheckpoint>();
                }

                checkpoint.Configure(new Vector2(175.1f, 3.3f));
            }

            GameObject hook = EnsureSolidObject(
                HookName,
                new Vector2(182f, 10.5f),
                new Vector2(1.8f, 0.5f),
                new Color(1f, 0.72f, 0.18f));
            if (!hook.TryGetComponent(out HookPoint _))
            {
                hook.AddComponent<HookPoint>();
            }

            GameObject goal = EnsureSolidObject(
                GoalName,
                new Vector2(205.5f, 2.3f),
                new Vector2(36f, 0.7f),
                new Color(0.28f, 0.9f, 0.58f));
            if (!goal.TryGetComponent(out MainStageGoalZone _))
            {
                goal.AddComponent<MainStageGoalZone>();
            }

            return goal;
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
            collider.isTrigger = false;
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
