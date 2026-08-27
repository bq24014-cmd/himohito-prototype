using UnityEngine;
using UnityEngine.SceneManagement;

namespace HimoHito
{
    /// <summary>
    /// Adds the section-two length window. The solid board sits below Hook 2
    /// and immediately before Landing 2 so rope length changes the route.
    /// </summary>
    public static class MainStageSectionTwoSetup
    {
        public const string BoardName = "Main Section 2 Length Window Board";

        private static readonly Vector2 BoardPosition =
            new Vector2(20f, -3.4f);
        private static readonly Vector2 BoardSize =
            new Vector2(2.4f, 0.5f);

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
            GameObject board = FindSceneObject(BoardName);
            if (board == null)
            {
                board = new GameObject(BoardName);
            }

            board.SetActive(true);
            board.transform.position = BoardPosition;
            board.transform.localScale = new Vector3(
                BoardSize.x,
                BoardSize.y,
                1f);

            if (!board.TryGetComponent(out SpriteRenderer _))
            {
                board.AddComponent<SpriteRenderer>();
            }

            if (!board.TryGetComponent(out SolidSprite visual))
            {
                visual = board.AddComponent<SolidSprite>();
            }
            visual.Color = new Color(1f, 0.706f, 0.235f);

            if (!board.TryGetComponent(out BoxCollider2D collider))
            {
                collider = board.AddComponent<BoxCollider2D>();
            }
            collider.size = Vector2.one;
            collider.isTrigger = false;

            if (!board.TryGetComponent(out SolidSwingSurface _))
            {
                board.AddComponent<SolidSwingSurface>();
            }

            return board;
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
