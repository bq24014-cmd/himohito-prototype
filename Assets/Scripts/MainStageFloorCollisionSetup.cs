using UnityEngine;
using UnityEngine.SceneManagement;

namespace HimoHito
{
    /// <summary>
    /// Gives each visible main-stage floor the collision rule suggested by
    /// its toy material. Blue railway floors are one-way; the orange start
    /// floor and generated rope platforms are solid from every direction.
    /// </summary>
    public static class MainStageFloorCollisionSetup
    {
        private const string MainStageSceneName = "MainStage";
        private const string MainObjectPrefix = "Main ";
        private const string StartGroundName = "Main Start Ground";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void ApplyAfterSceneLoad()
        {
            if (SceneManager.GetActiveScene().name == MainStageSceneName)
            {
                ApplyCurrentScene();
            }
        }

        public static bool ApplyCurrentScene()
        {
            bool changed = false;
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
                    candidate.TryGetComponent(out HookPoint _))
                {
                    continue;
                }

                if (candidate.name == StartGroundName)
                {
                    changed |= EnsureSolid(candidate);
                    continue;
                }

                if (candidate.TryGetComponent(out SolidSwingSurface _))
                {
                    changed |= EnsureSolid(candidate);
                    continue;
                }

                if (!candidate.TryGetComponent(out SolidSprite _))
                {
                    continue;
                }

                changed |= EnsureOneWayRail(candidate);
            }

            return changed;
        }

        private static bool EnsureSolid(GameObject floor)
        {
            bool changed = false;
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

        private static bool EnsureOneWayRail(GameObject floor)
        {
            bool changed = false;
            if (floor.TryGetComponent(out SolidSwingSurface solidSurface))
            {
                if (Application.isPlaying)
                {
                    Object.Destroy(solidSurface);
                }
                else
                {
                    Object.DestroyImmediate(solidSurface);
                }
                changed = true;
            }

            if (!floor.TryGetComponent(out PlatformEffector2D _))
            {
                floor.AddComponent<PlatformEffector2D>();
                changed = true;
            }

            if (!floor.TryGetComponent(out OneWayRailPlatform _))
            {
                floor.AddComponent<OneWayRailPlatform>();
                changed = true;
            }

            return changed;
        }
    }
}
