using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace HimoHitoEditor
{
    /// <summary>
    /// Keeps full-play checks starting from the tutorial even while another scene is open.
    /// </summary>
    [InitializeOnLoad]
    public static class PlayFromTutorial
    {
        private const string TutorialScenePath = "Assets/Scenes/Tutorial.unity";

        static PlayFromTutorial()
        {
            EditorApplication.delayCall += ConfigurePlayStartScene;
        }

        [MenuItem("HimoHito/Play From Tutorial")]
        public static void ConfigurePlayStartScene()
        {
            SceneAsset tutorialScene =
                AssetDatabase.LoadAssetAtPath<SceneAsset>(TutorialScenePath);

            if (tutorialScene == null)
            {
                Debug.LogWarning(
                    $"Tutorial scene was not found: {TutorialScenePath}");
                return;
            }

            EditorSceneManager.playModeStartScene = tutorialScene;
        }
    }
}
