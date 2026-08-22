using UnityEngine;

namespace HimoHito
{
    /// <summary>
    /// Creates section seven: turn the first rope into a walkable platform,
    /// move to its endpoint, then attach a second rope to cross the gap.
    /// </summary>
    public static class MainStageSectionSevenSetup
    {
        public const string HookName = "Main Section 7 Hook";
        public const string FinalHookName = "Main Section 7 Final Hook";
        public const string WeaveFrameName = "Main Section 7 Weave Frame";
        public const string LegacyWovenPlatformName = "Main Section 7 Woven Platform";
        public const string WeaveMarkerName = "Main Section 7 Weave Marker";
        public const string LandingName = "Main Section 7 Landing";

        public static GameObject EnsureCreated()
        {
            RemoveLegacyGuide(WeaveFrameName);
            RemoveLegacyGuide(WeaveMarkerName);

            GameObject hook = EnsureSolidObject(
                HookName,
                new Vector2(102.5f, 3.2f),
                new Vector2(1.6f, 0.45f),
                new Color(1f, 0.72f, 0.18f));
            if (!hook.TryGetComponent(out HookPoint _))
            {
                hook.AddComponent<HookPoint>();
            }

            GameObject finalHook = FindSceneObject(FinalHookName);
            if (finalHook == null)
            {
                finalHook = FindSceneObject(LegacyWovenPlatformName);
                if (finalHook != null)
                {
                    finalHook.name = FinalHookName;
                }
            }

            finalHook = EnsureSolidObject(
                FinalHookName,
                new Vector2(120.5f, 3.5f),
                new Vector2(1.6f, 0.45f),
                new Color(1f, 0.72f, 0.18f));
            if (!finalHook.TryGetComponent(out HookPoint _))
            {
                finalHook.AddComponent<HookPoint>();
            }

            GameObject landing = EnsureSolidObject(
                LandingName,
                new Vector2(134f, -0.8f),
                new Vector2(8f, 0.7f),
                new Color(0.38f, 0.41f, 0.52f));
            if (!landing.TryGetComponent(out MainStageSectionTarget _))
            {
                landing.AddComponent<MainStageSectionTarget>();
            }

            return landing;
        }

        private static void RemoveLegacyGuide(string objectName)
        {
            GameObject legacyGuide = FindSceneObject(objectName);
            if (legacyGuide == null)
            {
                return;
            }

            legacyGuide.SetActive(false);
            if (Application.isPlaying)
            {
                Object.Destroy(legacyGuide);
            }
            else
            {
                Object.DestroyImmediate(legacyGuide);
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

            gameObject.SetActive(true);
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
