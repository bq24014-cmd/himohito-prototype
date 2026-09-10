using System;
using UnityEngine;

namespace HimoHito
{
    /// <summary>Persistent clear flags only; stage availability stays in StageCatalog.</summary>
    public static class StageProgress
    {
        // Stable IDs keep progress when the catalog is reordered or scene paths change.
        private const string ClearedKeyPrefix = "HimoHito.StageProgress.v1.Cleared.";

        public static bool IsCleared(StageCatalog.Entry stage)
        {
            return stage != null && stage.available &&
                !string.IsNullOrWhiteSpace(stage.id) &&
                !string.IsNullOrWhiteSpace(stage.scenePath) &&
                PlayerPrefs.GetInt(ClearedKeyPrefix + stage.id, 0) == 1;
        }

        public static void MarkSceneCleared(string scenePath)
        {
            if (string.IsNullOrWhiteSpace(scenePath)) return;

            bool changed = false;
            foreach (StageCatalog.Entry stage in StageCatalog.Entries)
            {
                if (stage == null || !stage.available ||
                    string.IsNullOrWhiteSpace(stage.id) ||
                    string.IsNullOrWhiteSpace(stage.scenePath) ||
                    !string.Equals(stage.scenePath, scenePath, StringComparison.Ordinal) ||
                    IsCleared(stage)) continue;

                PlayerPrefs.SetInt(ClearedKeyPrefix + stage.id, 1);
                changed = true;
            }

            if (changed) PlayerPrefs.Save();
        }
    }
}
