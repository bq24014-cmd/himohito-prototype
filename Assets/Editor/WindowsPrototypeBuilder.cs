using System;
using System.IO;
using System.Text;
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
        private const string BuildFolder = "Builds/Windows";
        private const string RuntimeFolder = "Builds/Windows/Game";
        private const string ExecutableName = "HimoHitoPrototype.exe";
        private const string LauncherName = "START_HIMOHITO.bat";
        private const string ReadmeName = "README_起動方法.txt";

        [MenuItem("HimoHito/Build Windows Prototype")]
        public static void BuildWindowsPrototype()
        {
            ProjectPresentationSettings.Apply();

            string[] scenePaths = HimoHito.StageCatalog.GetBuildScenePaths();
            foreach (string scenePath in scenePaths)
            {
                if (!File.Exists(scenePath))
                {
                    throw new FileNotFoundException(
                        $"Build scene was not found: {scenePath}",
                        scenePath);
                }
            }

            if (Directory.Exists(BuildFolder))
            {
                Directory.Delete(BuildFolder, true);
            }
            Directory.CreateDirectory(RuntimeFolder);
            string executablePath = Path.Combine(RuntimeFolder, ExecutableName);
            BuildPlayerOptions options = new BuildPlayerOptions
            {
                scenes = scenePaths,
                locationPathName = executablePath,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.CleanBuildCache
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            BuildSummary summary = report.summary;
            if (summary.result != BuildResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Windows build failed: {summary.result}, " +
                    $"errors={summary.totalErrors}, warnings={summary.totalWarnings}");
            }

            WriteFriendLauncher();

            Debug.Log(
                $"HimoHito Windows build created: {executablePath} " +
                $"({summary.totalSize / (1024f * 1024f):0.0} MB)");
        }

        private static void WriteFriendLauncher()
        {
            string launcher =
                "@echo off\r\n" +
                "setlocal\r\n" +
                "set \"DEST=%PUBLIC%\\HimoHitoPrototype\"\r\n" +
                "for %%I in (\"%~dp0Game\\.\") do set \"SOURCE=%%~fI\"\r\n" +
                "for %%I in (\"%DEST%\\.\") do set \"DEST=%%~fI\"\r\n" +
                "if /I \"%SOURCE%\"==\"%DEST%\" goto launch\r\n" +
                "if not exist \"%DEST%\" mkdir \"%DEST%\"\r\n" +
                "robocopy \"%SOURCE%\" \"%DEST%\" /E /R:1 /W:1 /NFL /NDL /NJH /NJS /NP >nul\r\n" +
                ":launch\r\n" +
                "start \"\" \"%DEST%\\HimoHitoPrototype.exe\"\r\n" +
                "endlocal\r\n";
            File.WriteAllText(
                Path.Combine(BuildFolder, LauncherName),
                launcher,
                new UTF8Encoding(false));

            string readme =
                "ヒモヒト 起動方法\r\n\r\n" +
                "1. ZIPを右クリックして「すべて展開」してください。\r\n" +
                "2. START_HIMOHITO.batをダブルクリックしてください。\r\n" +
                "3. 初回だけゲーム一式を英数字のフォルダへコピーして起動します。\r\n\r\n" +
                "HimoHitoPrototype.exeを直接起動すると、日本語を含む保存場所では\r\n" +
                "チュートリアルから本編へ移る際にUnityが停止する場合があります。\r\n" +
                "必ずSTART_HIMOHITO.batから起動してください。\r\n";
            File.WriteAllText(
                Path.Combine(BuildFolder, ReadmeName),
                readme,
                new UTF8Encoding(true));
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
