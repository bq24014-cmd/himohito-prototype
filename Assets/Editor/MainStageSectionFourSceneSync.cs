using HimoHito;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace HimoHitoEditor
{
    /// <summary>Keeps the saved MainStage scene aligned with section four.</summary>
    [InitializeOnLoad]
    public static class MainStageSectionFourSceneSync
    {
        static MainStageSectionFourSceneSync()
        {
            EditorSceneManager.sceneOpened += OnSceneOpened;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            EditorApplication.delayCall += SyncActiveScene;
        }

        private static void OnSceneOpened(Scene scene, OpenSceneMode mode)
        {
            Sync(scene);
        }

        private static void SyncActiveScene()
        {
            Sync(SceneManager.GetActiveScene());
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredEditMode)
            {
                EditorApplication.delayCall += SyncActiveScene;
            }
        }

        private static void Sync(Scene scene)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode ||
                !scene.IsValid() ||
                scene.name != "MainStage")
            {
                return;
            }

            if (MainStageFloorCollisionSetup.ApplyCurrentScene())
            {
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }
        }
    }
}
