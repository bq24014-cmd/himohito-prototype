using UnityEditor;

namespace HimoHitoEditor
{
    /// <summary>Rebuilds the tutorial and the implemented main-stage sections.</summary>
    public static class StageManual0829Builder
    {
        [MenuItem("HimoHito/Rebuild Implemented Stages From 0829 Manual")]
        public static void RebuildAllStages()
        {
            PrototypeSceneBuilder.BuildPrototypeScene();
            MainStageSceneBuilder.BuildMainStageThroughCurrentSection();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
