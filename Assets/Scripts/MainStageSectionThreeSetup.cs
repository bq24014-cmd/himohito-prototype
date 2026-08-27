using UnityEngine;
using UnityEngine.SceneManagement;

namespace HimoHito
{
    /// <summary>
    /// Adds the upper choice for section three. The existing Main Hook 3 is the
    /// nearby lower choice; this hook asks for a longer rope but gives a higher
    /// pendulum pivot toward Landing 3.
    /// </summary>
    public static class MainStageSectionThreeSetup
    {
        public const string UpperHookName = "Main Hook 3 Upper";

        private static readonly Vector2 UpperHookPosition =
            new Vector2(36f, 4.8f);
        private static readonly Vector2 UpperHookSize =
            new Vector2(1.6f, 0.45f);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureAfterSceneLoad()
        {
            if (SceneManager.GetActiveScene().name != "MainStage")
            {
                return;
            }

            EnsureCreated();
            RopeResource player = Object.FindFirstObjectByType<RopeResource>();
            if (player != null)
            {
                MainStageVisuals.Apply(player.gameObject);
            }
        }

        public static GameObject EnsureCreated()
        {
            GameObject hook = FindSceneObject(UpperHookName);
            if (hook == null)
            {
                hook = new GameObject(UpperHookName);
            }

            hook.SetActive(true);
            hook.transform.position = UpperHookPosition;
            hook.transform.localScale = new Vector3(
                UpperHookSize.x,
                UpperHookSize.y,
                1f);

            if (!hook.TryGetComponent(out SpriteRenderer _))
            {
                hook.AddComponent<SpriteRenderer>();
            }

            if (!hook.TryGetComponent(out SolidSprite visual))
            {
                visual = hook.AddComponent<SolidSprite>();
            }
            visual.Color = new Color(0.298f, 0.765f, 1f);

            if (!hook.TryGetComponent(out BoxCollider2D collider))
            {
                collider = hook.AddComponent<BoxCollider2D>();
            }
            collider.size = Vector2.one;
            collider.isTrigger = false;

            if (!hook.TryGetComponent(out HookPoint hookPoint))
            {
                hookPoint = hook.AddComponent<HookPoint>();
            }
            hookPoint.ConfigureFixedAttachmentPoint(Vector2.zero);
            return hook;
        }

        private static GameObject FindSceneObject(string objectName)
        {
            foreach (GameObject candidate in
                     Resources.FindObjectsOfTypeAll<GameObject>())
            {
                if (candidate.scene.IsValid() &&
                    candidate.scene.name == "MainStage" &&
                    candidate.name == objectName)
                {
                    return candidate;
                }
            }

            return null;
        }
    }
}
