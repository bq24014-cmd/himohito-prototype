using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using HimoHito;

// Only installed into independent QA copies, never Production Assets.
[InitializeOnLoad]
public static class IntroEntry
{
    const string Key = "TutorialIntroFix20261002";
    static IntroEntry() { EditorApplication.playModeStateChanged += Changed; }
    static string Arg(string name)
    {
        var args = Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length - 1; i++) if (args[i] == name) return args[i + 1];
        throw new Exception("Missing " + name);
    }
    static void Settings()
    {
        PlayerSettings.companyName = "HimoHitoVerificationOnly";
        PlayerSettings.productName = "Tutorial2p5DIntroFix_20261002";
    }
    public static void Timeline() => Run("IntroTimelineQA");
    public static void Regression() => Run("IntegrationQA");
    static void Run(string type)
    {
        Settings(); string output = Arg("-introEvidence"); Directory.CreateDirectory(output);
        SessionState.SetString(Key, output); SessionState.SetString(Key + "Type", type);
        SessionState.SetBool(Key + "After", Arg("-introVariant") == "After");
        EditorSceneManager.playModeStartScene = null;
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        EditorApplication.EnterPlaymode();
    }
    static void Changed(PlayModeStateChange state)
    {
        if (state != PlayModeStateChange.EnteredPlayMode) return;
        string output = SessionState.GetString(Key, ""); if (output == "") return;
        string name = SessionState.GetString(Key + "Type", ""); SessionState.EraseString(Key);
        EditorSceneManager.LoadSceneInPlayMode("Assets/Scenes/Tutorial.unity", new LoadSceneParameters(LoadSceneMode.Single));
        var type = Type.GetType(name + ", Assembly-CSharp");
        if (type == null) throw new Exception("QA type unavailable: " + name);
        type.GetField("Output").SetValue(null, output);
        if (name == "IntroTimelineQA") type.GetField("ExpectedAfter").SetValue(null, SessionState.GetBool(Key + "After", false));
        var go = new GameObject("Isolated intro background QA"); UnityEngine.Object.DontDestroyOnLoad(go);
        type.GetMethod("Begin").Invoke(go.AddComponent(type), null);
    }
    public static void Build()
    {
        try {
            Settings(); string output = Arg("-introBuild");
            if (Directory.Exists(output)) throw new Exception("Refuse existing build output");
            Directory.CreateDirectory(output);
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
                scenes = StageCatalog.GetBuildScenePaths(), target = BuildTarget.StandaloneWindows64,
                locationPathName = Path.Combine(output, "HimoHitoIntroFix.exe"), options = BuildOptions.None });
            File.WriteAllText(Path.Combine(output, "BUILD_RESULT.txt"), report.summary.result + "\nbytes=" + report.summary.totalSize + "\nseconds=" + report.summary.totalTime.TotalSeconds);
            EditorApplication.Exit(report.summary.result == BuildResult.Succeeded ? 0 : 2);
        } catch (Exception ex) { Debug.LogException(ex); EditorApplication.Exit(2); }
    }
}
