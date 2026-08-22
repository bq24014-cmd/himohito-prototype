using UnityEngine;

namespace HimoHito
{
    /// <summary>
    /// Applies the first visual trial without changing colliders or physics.
    /// Only tutorial section one receives the agreed gameplay palette entries.
    /// </summary>
    public static class TutorialFirstSectionVisuals
    {
        private const string BackgroundResourcePath =
            "Art/TutorialNightChildRoom-v1";
        private const string BackgroundName =
            "Tutorial Night Child Room Background";

        private static readonly Color PlayerColor =
            new Color(1f, 0.365f, 0.561f);
        private static readonly Color PlatformColor =
            new Color(1f, 0.706f, 0.235f);
        private static readonly Color HookColor =
            new Color(0.298f, 0.765f, 1f);

        public static void Apply(GameObject player)
        {
            ApplyColor(player, PlayerColor);
            ApplyColor(FindSceneObject("Start Ground"), PlatformColor);
            ApplyColor(FindSceneObject("Tutorial Landing"), PlatformColor);
            ApplyColor(FindSceneObject("Tutorial Hook"), HookColor);
            EnsureBackground();
        }

        private static void EnsureBackground()
        {
            Sprite backgroundSprite =
                Resources.Load<Sprite>(BackgroundResourcePath);
            if (backgroundSprite == null)
            {
                Debug.LogWarning(
                    $"Tutorial background was not found: {BackgroundResourcePath}");
                return;
            }

            GameObject background = FindSceneObject(BackgroundName);
            if (background == null)
            {
                background = new GameObject(BackgroundName);
            }

            background.transform.position = new Vector3(-15f, 0f, 1f);
            float width = Mathf.Max(0.01f, backgroundSprite.bounds.size.x);
            float scale = 40f / width;
            background.transform.localScale = new Vector3(scale, scale, 1f);

            if (!background.TryGetComponent(out SpriteRenderer renderer))
            {
                renderer = background.AddComponent<SpriteRenderer>();
            }

            renderer.sprite = backgroundSprite;
            renderer.color = Color.white;
            renderer.sortingOrder = -100;
        }

        private static void ApplyColor(GameObject target, Color color)
        {
            if (target == null)
            {
                return;
            }

            if (!target.TryGetComponent(out SolidSprite visual))
            {
                return;
            }

            visual.Color = color;
        }

        private static GameObject FindSceneObject(string objectName)
        {
            foreach (GameObject candidate in
                     Resources.FindObjectsOfTypeAll<GameObject>())
            {
                if (candidate.scene.IsValid() && candidate.name == objectName)
                {
                    return candidate;
                }
            }

            return null;
        }
    }
}
