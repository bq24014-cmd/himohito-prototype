using HimoHito;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HimoHitoEditor
{
    /// <summary>Refresh artwork without rebuilding gameplay or moving its camera.</summary>
    [InitializeOnLoad]
    public static class StageVisualPreview
    {
        static StageVisualPreview()
        {
            EditorApplication.delayCall += Refresh;
            EditorSceneManager.sceneOpened += (_, __) => EditorApplication.delayCall += Refresh;
            EditorApplication.playModeStateChanged += state =>
            {
                if (state == PlayModeStateChange.EnteredEditMode)
                    EditorApplication.delayCall += Refresh;
            };
        }

        [MenuItem("HimoHito/Preview/Refresh Scene Visuals")]
        public static void Refresh()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling)
                return;
            Scene scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || !scene.isLoaded) return;
            bool changed;
            if (scene.name == "Tutorial")
                changed = TutorialFirstSectionVisuals.Apply(GameObject.Find("Player"));
            else if (scene.name == "MainStage")
                changed = MainStageVisuals.Apply(GameObject.Find("Main Player"));
            else return;
            if (changed) EditorSceneManager.MarkSceneDirty(scene);
            SceneView.RepaintAll();
            EditorApplication.QueuePlayerLoopUpdate();
        }

        [MenuItem("HimoHito/Preview/Show Goal Chest")]
        public static void ShowGoal()
        {
            Refresh();
            Scene scene = SceneManager.GetActiveScene();
            string name = scene.name == "Tutorial" ? TutorialSectionFourSetup.GoalMarkerName :
                scene.name == "MainStage" ? MainStageSectionTenSetup.GoalMarkerName : null;
            GameObject marker = name != null ? GameObject.Find(name) : null;
            if (marker == null)
            {
                Debug.LogWarning("TutorialまたはMainStageのシーンを開いてください。");
                return;
            }
            Selection.activeGameObject = marker;
            SceneView view = SceneView.lastActiveSceneView;
            if (view == null) view = EditorWindow.GetWindow<SceneView>();
            view.in2DMode = true;
            view.Frame(new Bounds(marker.transform.position + Vector3.down,
                new Vector3(10f, 7f, 1f)), false);
        }
    }
}
