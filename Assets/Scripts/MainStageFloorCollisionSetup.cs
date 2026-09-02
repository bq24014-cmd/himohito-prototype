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
            bool changed = MainStageSectionTwoSetup.ApplyCurrentScene();
            changed |= MainStageSectionThreeSetup.ApplyCurrentScene();
            changed |= MainStageSectionFourSetup.ApplyCurrentScene();
            changed |= MainStageSectionFiveSetup.ApplyCurrentScene();
            changed |= MainStageSectionSixSetup.ApplyCurrentScene();
            changed |= MainStageSectionSevenSetup.ApplyCurrentScene();
            changed |= MainStageSectionEightSetup.ApplyCurrentScene();
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
