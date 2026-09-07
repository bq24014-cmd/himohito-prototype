using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace HimoHitoEditor
{
    /// <summary>Selects the scene Unity opens when Play starts.</summary>
    [InitializeOnLoad]
    public static class PlayStartSceneSelector
    {
        private const string TutorialScenePath = "Assets/Scenes/Tutorial.unity";
        private const string MainStageScenePath = "Assets/Scenes/MainStage.unity";

        static PlayStartSceneSelector()
        {
            EditorApplication.delayCall +=
                ConfigurePlayStartScene;
        }

        [MenuItem("HimoHito/Play From Tutorial")]
        public static void ConfigurePlayStartScene()
        {
            ConfigurePlayStartScene(TutorialScenePath, "Tutorial");
        }

        [MenuItem("HimoHito/Play From Main Stage Beginning")]
        public static void ConfigureMainStageBeginningStartScene()
        {
            ConfigurePlayStartScene(MainStageScenePath, "MainStage beginning");
        }

        [MenuItem("HimoHito/Play From Ending Preview")]
        public static void ConfigureEndingPreviewStartScene()
        {
            ConfigurePlayStartScene(MainStageScenePath, "ending preview");
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
