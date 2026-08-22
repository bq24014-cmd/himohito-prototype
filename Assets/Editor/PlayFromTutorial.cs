using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace HimoHitoEditor
{
    /// <summary>
    /// Selects whether Play starts from the full tutorial or the active section experiment.
    /// </summary>
    [InitializeOnLoad]
    public static class PlayFromTutorial
    {
        private const string TutorialScenePath = "Assets/Scenes/Tutorial.unity";
        private const string MainStageScenePath = "Assets/Scenes/MainStage.unity";

        static PlayFromTutorial()
        {
            EditorApplication.delayCall += ConfigureSectionSevenExperimentStartScene;
        }

        [MenuItem("HimoHito/Play From Tutorial")]
        public static void ConfigurePlayStartScene()
        {
            ConfigurePlayStartScene(TutorialScenePath, "Tutorial");
        }

        [MenuItem("HimoHito/Play From Section 7 Experiment")]
        public static void ConfigureSectionSevenExperimentStartScene()
        {
            ConfigurePlayStartScene(MainStageScenePath, "MainStage section 7 experiment");
        }

        private static void ConfigurePlayStartScene(string scenePath, string description)
        {
            SceneAsset scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);

            if (scene == null)
            {
                Debug.LogWarning(
                    $"Play start scene was not found: {scenePath}");
                return;
            }

            EditorSceneManager.playModeStartScene = scene;
            Debug.Log($"Play starts from {description}.");
        }
    }
}
