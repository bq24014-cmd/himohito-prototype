using UnityEngine;

namespace HimoHito
{
    /// <summary>
    /// Applies the adopted first-section visuals without changing colliders or physics.
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

        public static bool Apply(GameObject player)
        {
            bool changed = false;
            changed |= ApplyColor(player, PlayerColor);
            changed |= ApplyColor(FindSceneObject("Start Ground"), PlatformColor);
            changed |= ApplyColor(FindSceneObject("Tutorial Landing"), PlatformColor);
            changed |= ApplyColor(FindSceneObject("Tutorial Hook"), HookColor);
            changed |= EnsureBackground();
            return changed;
        }

        private static bool EnsureBackground()
        {
            Sprite backgroundSprite =
                Resources.Load<Sprite>(BackgroundResourcePath);
            if (backgroundSprite == null)
            {
                Debug.LogWarning(
                    $"Tutorial background was not found: {BackgroundResourcePath}");
                return false;
            }

            bool changed = false;
            GameObject background = FindSceneObject(BackgroundName);
            if (background == null)
            {
                background = new GameObject(BackgroundName);
                changed = true;
            }

            Vector3 targetPosition = new Vector3(-15f, 0f, 1f);
            if (background.transform.position != targetPosition)
            {
                background.transform.position = targetPosition;
                changed = true;
            }

            float width = Mathf.Max(0.01f, backgroundSprite.bounds.size.x);
            float scale = 40f / width;
            Vector3 targetScale = new Vector3(scale, scale, 1f);
            if (background.transform.localScale != targetScale)
            {
                background.transform.localScale = targetScale;
                changed = true;
            }

            if (!background.TryGetComponent(out SpriteRenderer renderer))
            {
                renderer = background.AddComponent<SpriteRenderer>();
                changed = true;
            }

            if (renderer.sprite != backgroundSprite)
            {
                renderer.sprite = backgroundSprite;
                changed = true;
            }

            if (renderer.color != Color.white)
            {
                renderer.color = Color.white;
                changed = true;
            }

            if (renderer.sortingOrder != -100)
            {
                renderer.sortingOrder = -100;
                changed = true;
            }

            return changed;
        }

        private static bool ApplyColor(GameObject target, Color color)
        {
            if (target == null)
            {
                return false;
            }

            if (!target.TryGetComponent(out SolidSprite visual))
            {
                return false;
            }

            if (visual.Color == color)
            {
                return false;
            }

            visual.Color = color;
            return true;
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
