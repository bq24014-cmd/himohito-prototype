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
        public const string HazardName = "Main Section 6 Rope Hazard";
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

            GameObject hazard = EnsureSolidObject(
                HazardName,
                new Vector2(85.5f, -7.1f),
                new Vector2(13f, 0.65f),
                new Color(1f, 0.28f, 0.32f));
            hazard.GetComponent<BoxCollider2D>().isTrigger = true;
            if (!hazard.TryGetComponent(out MainStageRopeHazard _))
            {
                hazard.AddComponent<MainStageRopeHazard>();
            }

            GameObject landing = EnsureSolidObject(
                LandingName,
                new Vector2(96f, -0.8f),
                new Vector2(8f, 0.7f),
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
