using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using HimoHito;

[InitializeOnLoad]
public static class IntegrationEntry
{
    const string Key = "TutorialProduction2p5D20261001";
    static IntegrationEntry() { EditorApplication.playModeStateChanged += Changed; }
    static string Arg(string key)
    {
        var args = Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length - 1; i++) if (args[i] == key) return args[i + 1];
        throw new Exception("Missing argument " + key);
    }
    static void Settings()
    {
        PlayerSettings.companyName = "HimoHitoVerificationOnly";
        PlayerSettings.productName = "Tutorial2p5DProductionIntegration_20261001";
    }
    public static void Build()
    {
        try {
            Settings(); string output = Arg("-integrationBuild");
            if (Directory.Exists(output)) throw new Exception("Refuse existing build " + output);
            Directory.CreateDirectory(output);
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
                scenes = StageCatalog.GetBuildScenePaths(), target = BuildTarget.StandaloneWindows64,
                locationPathName = Path.Combine(output, "HimoHitoProduction2p5D.exe"), options = BuildOptions.None });
            File.WriteAllText(Path.Combine(output, "BUILD_RESULT.txt"), report.summary.result + "\nbytes=" + report.summary.totalSize + "\nseconds=" + report.summary.totalTime.TotalSeconds);
            EditorApplication.Exit(report.summary.result == BuildResult.Succeeded ? 0 : 2);
        } catch (Exception ex) { Debug.LogException(ex); EditorApplication.Exit(2); }
    }
    public static void Run()
    {
        Settings(); string output = Arg("-integrationEvidence"); Directory.CreateDirectory(output);
        EditorSceneManager.playModeStartScene = null;
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        SessionState.SetString(Key, output); EditorApplication.EnterPlaymode();
    }
    static void Changed(PlayModeStateChange state)
    {
        if (state != PlayModeStateChange.EnteredPlayMode) return;
        string output = SessionState.GetString(Key, ""); if (output == "") return; SessionState.EraseString(Key);
        EditorSceneManager.LoadSceneInPlayMode("Assets/Scenes/Tutorial.unity", new LoadSceneParameters(LoadSceneMode.Single));
        var type = Type.GetType("IntegrationQA, Assembly-CSharp");
        if (type == null) throw new Exception("Diagnostic unavailable in this copy");
        type.GetField("Output").SetValue(null, output);
        var go = new GameObject("Production background regression — editor only");
        UnityEngine.Object.DontDestroyOnLoad(go);
        var qa = go.AddComponent(type);
        type.GetMethod("Begin").Invoke(qa, null);
    }
}
