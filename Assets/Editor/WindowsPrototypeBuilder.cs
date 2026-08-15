using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace HimoHitoEditor
{
    /// <summary>
    /// Creates a Windows build containing the tutorial and current main stage.
    /// Build output stays outside Git because it is reproducible from this project.
    /// </summary>
    public static class WindowsPrototypeBuilder
    {
        private const string TutorialScenePath = "Assets/Scenes/Tutorial.unity";
        private const string MainStageScenePath = "Assets/Scenes/MainStage.unity";
        private const string BuildFolder = "Builds/Windows";
        private const string ExecutableName = "HimoHitoPrototype.exe";

        [MenuItem("HimoHito/Build Windows Prototype")]
        public static void BuildWindowsPrototype()
        {
            string[] scenePaths = { TutorialScenePath, MainStageScenePath };
            foreach (string scenePath in scenePaths)
            {
                if (!File.Exists(scenePath))
                {
                    throw new FileNotFoundException(
                        $"Build scene was not found: {scenePath}",
                        scenePath);
                }
            }

            Directory.CreateDirectory(BuildFolder);
            string executablePath = Path.Combine(BuildFolder, ExecutableName);
            BuildPlayerOptions options = new BuildPlayerOptions
            {
                scenes = scenePaths,
                locationPathName = executablePath,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            BuildSummary summary = report.summary;
            if (summary.result != BuildResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Windows build failed: {summary.result}, " +
                    $"errors={summary.totalErrors}, warnings={summary.totalWarnings}");
            }

            Debug.Log(
                $"HimoHito Windows build created: {executablePath} " +
                $"({summary.totalSize / (1024f * 1024f):0.0} MB)");
        }

        public static void BuildFromCommandLine()
        {
            try
            {
                BuildWindowsPrototype();
                EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EditorApplication.Exit(1);
            }
        }
    }
}
