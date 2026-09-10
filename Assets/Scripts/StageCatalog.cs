using System;
using System.Collections.Generic;
using UnityEngine;

namespace HimoHito
{
    /// <summary>Ordered stage data shared by the menu and the Windows builder.</summary>
    public static class StageCatalog
    {
        [Serializable]
        public sealed class Entry
        {
            public string id;
            public string title;
            public string subtitle;
            public string description;
            public string mapArt;
            public string scenePath;
            public bool available;
        }

        [Serializable] private sealed class Data { public Entry[] stages; }
        public const string TitleScenePath = "Assets/Scenes/Tutorial.unity";
        private static Entry[] entries;
        public static IReadOnlyList<Entry> Entries => entries ??= Load();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetCache() => entries = null;

        private static Entry[] Load()
        {
            TextAsset asset = Resources.Load<TextAsset>("StageCatalog");
            if (asset == null) throw new InvalidOperationException("StageCatalog.json is missing.");
            Data data = JsonUtility.FromJson<Data>(asset.text);
            if (data?.stages == null || data.stages.Length == 0)
                throw new InvalidOperationException("StageCatalog requires at least one stage.");
            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (Entry entry in data.stages)
            {
                if (entry == null || string.IsNullOrWhiteSpace(entry.id) || !ids.Add(entry.id) ||
                    string.IsNullOrWhiteSpace(entry.title) || (entry.available &&
                    (string.IsNullOrWhiteSpace(entry.scenePath) ||
                     !entry.scenePath.StartsWith("Assets/", StringComparison.Ordinal) ||
                     !entry.scenePath.EndsWith(".unity", StringComparison.Ordinal))))
                    throw new InvalidOperationException("StageCatalog contains an invalid or duplicate entry.");
            }
            return data.stages;
        }

        public static string[] GetBuildScenePaths()
        {
            var paths = new List<string> { TitleScenePath };
            foreach (Entry entry in Load())
                if (entry.available && !paths.Contains(entry.scenePath)) paths.Add(entry.scenePath);
            return paths.ToArray();
        }
    }
}
