using UnityEngine;

namespace HimoHito
{
    /// <summary>
    /// Creates section seven: a gap that can be crossed either by spending
    /// weave threads on a bridge or by swinging with rope.
    /// </summary>
    public static class MainStageSectionSevenSetup
    {
        public const string HookName = "Main Section 7 Hook";
        public const string WeaveFrameName = "Main Section 7 Weave Frame";
        public const string WovenPlatformName = "Main Section 7 Woven Platform";
        public const string WeaveMarkerName = "Main Section 7 Weave Marker";
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

            GameObject wovenPlatform = EnsureSolidObject(
                WovenPlatformName,
                new Vector2(103.85f, -0.8f),
                new Vector2(12.3f, 0.7f),
                new Color(0.72f, 0.42f, 1f));

            GameObject weaveFrameObject = FindSceneObject(WeaveFrameName);
            if (weaveFrameObject == null)
            {
                weaveFrameObject = new GameObject(WeaveFrameName);
            }

            weaveFrameObject.transform.position = new Vector2(96.5f, 0f);
            weaveFrameObject.transform.localScale = new Vector3(2.5f, 3f, 1f);
            if (!weaveFrameObject.TryGetComponent(out BoxCollider2D frameTrigger))
            {
                frameTrigger = weaveFrameObject.AddComponent<BoxCollider2D>();
            }

            frameTrigger.isTrigger = true;
            frameTrigger.size = Vector2.one;
            if (!weaveFrameObject.TryGetComponent(out WeaveFrame weaveFrame))
            {
                weaveFrame = weaveFrameObject.AddComponent<WeaveFrame>();
            }

            weaveFrame.Configure(wovenPlatform, 3, true);

            GameObject marker = EnsureSolidObject(
                WeaveMarkerName,
                new Vector2(97.2f, 0.2f),
                new Vector2(0.25f, 1.6f),
                new Color(0.72f, 0.42f, 1f));
            marker.GetComponent<BoxCollider2D>().enabled = false;

            wovenPlatform.SetActive(weaveFrame.IsCompleted);

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
