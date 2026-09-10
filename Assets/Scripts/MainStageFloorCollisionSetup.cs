using UnityEngine;
using UnityEngine.SceneManagement;

namespace HimoHito
{
    /// <summary>
    /// Applies the 0829 manual's C-2 rule: authored terrain is solid from every
    /// direction. Generated rope bridges keep their own collision behaviour.
    /// </summary>
    public static class MainStageFloorCollisionSetup
    {
        private const string MainStageSceneName = "MainStage";
        private const string MainObjectPrefix = "Main ";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void RegisterSceneSetup()
        {
            // Also run after Tutorial -> MainStage and scene reloads. Unsubscribe
            // first so Enter Play Mode without domain reload cannot double-register.
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == MainStageSceneName && scene == SceneManager.GetActiveScene())
            {
                ApplyCurrentScene();
            }
        }

        public static bool ApplyCurrentScene()
        {
            bool changed = EnsureSectionTwoCheckpoint();
            changed |= MainStageSectionTwoSetup.ApplyCurrentScene();
            changed |= MainStageSectionThreeSetup.ApplyCurrentScene();
            changed |= MainStageSectionFourSetup.ApplyCurrentScene();
            changed |= MainStageSectionFiveSetup.ApplyCurrentScene();
            changed |= MainStageSectionSixSetup.ApplyCurrentScene();
            changed |= MainStageSectionSevenSetup.ApplyCurrentScene();
            changed |= MainStageSectionNineSetup.ApplyCurrentScene();
            changed |= MainStageSectionTenSetup.ApplyCurrentScene();
            foreach (GameObject candidate in
                     Resources.FindObjectsOfTypeAll<GameObject>())
            {
                if (!candidate.scene.IsValid() ||
                    candidate.scene.name != MainStageSceneName ||
                    !candidate.name.StartsWith(
                        MainObjectPrefix,
                        System.StringComparison.Ordinal) ||
                    !candidate.TryGetComponent(out BoxCollider2D floorCollider) ||
                    floorCollider.isTrigger ||
                    candidate.TryGetComponent(out RopeResource _) ||
                    candidate.TryGetComponent(out HookPoint _) ||
                    candidate.TryGetComponent(
                        out SwingPassThroughRailPlatform _))
                {
                    continue;
                }

                if (!candidate.TryGetComponent(out SolidSprite _))
                {
                    continue;
                }

                changed |= EnsureSolid(candidate);
            }

            RopeResource player = Object.FindFirstObjectByType<RopeResource>();
            if (player != null)
            {
                changed |= MainStageVisuals.Apply(player.gameObject);
            }

            return changed;
        }

        private static bool EnsureSectionTwoCheckpoint()
        {
            GameObject landing = SceneObjectLookup.Find("Main Landing 1", MainStageSceneName);
            if (landing == null || !landing.TryGetComponent(out Collider2D floor) ||
                landing.TryGetComponent(out MainStageCheckpoint _))
            {
                return false;
            }

            // Repair the authored opening, without rebuilding or moving its bank.
            Bounds bounds = floor.bounds;
            landing.AddComponent<MainStageCheckpoint>().Configure(2,
                new Vector2(bounds.center.x, bounds.max.y + 0.7f), 50f);
            return true;
        }

        private static bool EnsureSolid(GameObject floor)
        {
            bool changed = false;
            if (floor.TryGetComponent(out OneWayRailPlatform oneWayRail))
            {
                if (Application.isPlaying)
                {
                    Object.Destroy(oneWayRail);
                }
                else
                {
                    Object.DestroyImmediate(oneWayRail);
                }
                changed = true;
            }

            if (floor.TryGetComponent(out PlatformEffector2D effector) &&
                effector.enabled)
            {
                effector.enabled = false;
                changed = true;
            }

            if (!floor.TryGetComponent(out SolidSwingSurface _))
            {
                floor.AddComponent<SolidSwingSurface>();
                changed = true;
            }

            return changed;
        }

    }
}
