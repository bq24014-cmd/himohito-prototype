using UnityEditor;

namespace HimoHitoEditor
{
    /// <summary>Rebuilds both playable scenes from the final 2026-08-29 manual.</summary>
    public static class StageManual0829Builder
    {
        [MenuItem("HimoHito/Rebuild All Stages From 0829 Manual")]
        public static void RebuildAllStages()
        {
            PrototypeSceneBuilder.BuildPrototypeScene();
            MainStageSceneBuilder.BuildMainStageThroughSectionTen();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
