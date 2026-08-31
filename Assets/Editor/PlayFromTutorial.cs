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
        private const string FloorCollisionExperimentScenePath =
            "Assets/Scenes/FloorCollisionExperiment.unity";

        static PlayFromTutorial()
        {
            EditorApplication.delayCall +=
                ConfigureSectionFourDevelopmentStartScene;
        }

        [MenuItem("HimoHito/Play From Tutorial")]
        public static void ConfigurePlayStartScene()
        {
            ConfigurePlayStartScene(TutorialScenePath, "Tutorial");
        }

        [MenuItem("HimoHito/Play From Main Stage Midpoint")]
        public static void ConfigureMidpointDevelopmentStartScene()
        {
            ConfigurePlayStartScene(MainStageScenePath, "MainStage midpoint development");
        }

        [MenuItem("HimoHito/Play From Main Stage Beginning")]
        public static void ConfigureMainStageBeginningStartScene()
        {
            ConfigurePlayStartScene(MainStageScenePath, "MainStage beginning");
        }

        [MenuItem("HimoHito/Play From Main Stage Section 4")]
        public static void ConfigureSectionFourDevelopmentStartScene()
        {
            ConfigurePlayStartScene(
                MainStageScenePath,
                "本編第4区間の開発開始地点");
        }

        [MenuItem("HimoHito/Play From Section 7 Experiment")]
        public static void ConfigureSectionSevenExperimentStartScene()
        {
            ConfigurePlayStartScene(MainStageScenePath, "MainStage section 7 experiment");
        }

        [MenuItem("HimoHito/Play From Floor Collision Experiment")]
        public static void ConfigureFloorCollisionExperimentStartScene()
        {
            ConfigurePlayStartScene(
                FloorCollisionExperimentScenePath,
                "floor collision experiment");
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
            Debug.Log($"Play開始地点：{description}");
        }
    }
}
