using UnityEngine;

namespace HimoHito
{
    /// <summary>
    /// Creates section seven: a swing corridor where both overly short and
    /// overly long ropes are risky.
    /// </summary>
    public static class MainStageSectionSevenSetup
    {
        public const string HookName = "Main Section 7 Hook";
        public const string UpperHazardName = "Main Section 7 Upper Rope Hazard";
        public const string LowerHazardName = "Main Section 7 Lower Rope Hazard";
        public const string LandingName = "Main Section 7 Landing";

        public static GameObject EnsureCreated()
        {
            GameObject hook = EnsureSolidObject(
                HookName,
                new Vector2(102.5f, 4.2f),
                new Vector2(1.6f, 0.45f),
                new Color(1f, 0.72f, 0.18f));
            if (!hook.TryGetComponent(out HookPoint _))
            {
                hook.AddComponent<HookPoint>();
            }

            EnsureHazard(
                UpperHazardName,
                new Vector2(107f, -1.1f),
                new Vector2(5f, 1.2f));
            EnsureHazard(
                LowerHazardName,
                new Vector2(107f, -5.4f),
                new Vector2(5f, 1f));

            GameObject landing = EnsureSolidObject(
                LandingName,
                new Vector2(114f, -0.8f),
                new Vector2(8f, 0.7f),
                new Color(0.38f, 0.41f, 0.52f));
            if (!landing.TryGetComponent(out MainStageSectionTarget _))
            {
                landing.AddComponent<MainStageSectionTarget>();
            }

            return landing;
        }

        private static void EnsureHazard(string name, Vector2 position, Vector2 size)
        {
            GameObject hazard = EnsureSolidObject(
                name,
                position,
                size,
                new Color(1f, 0.28f, 0.32f));
            hazard.GetComponent<BoxCollider2D>().isTrigger = true;
            if (!hazard.TryGetComponent(out MainStageRopeHazard _))
            {
                hazard.AddComponent<MainStageRopeHazard>();
            }
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
