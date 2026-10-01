using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class CleanEntry
{
    const string Key = "CleanReproduction20261001";
    static CleanEntry() { EditorApplication.playModeStateChanged += Changed; }
    public static void Run()
    {
        string output = null, mode = null;
        var args = Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (args[i] == "-cleanEvidence") output = args[i + 1];
            if (args[i] == "-cleanMode") mode = args[i + 1];
        }
        if (output == null || mode == null) throw new Exception("Missing clean QA arguments");
        Directory.CreateDirectory(output);
        PlayerSettings.companyName = "HimoHitoCleanDiagnosticOnly";
        PlayerSettings.productName = "CleanReproduction_20261001_" + mode;
        EditorSceneManager.playModeStartScene = null;
        // Never open the production scene in Edit Mode: SceneSync may serialize it.
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        SessionState.SetString(Key, output);
        SessionState.SetString(Key + "Mode", mode);
        EditorApplication.EnterPlaymode();
    }
    static void Changed(PlayModeStateChange state)
    {
        if (state != PlayModeStateChange.EnteredPlayMode) return;
        string output = SessionState.GetString(Key, ""), mode = SessionState.GetString(Key + "Mode", "");
        if (output == "") return;
        SessionState.EraseString(Key);
        EditorSceneManager.LoadSceneInPlayMode(mode == "Full" ? "Assets/Scenes/MainStage.unity" : "Assets/Scenes/Tutorial.unity",
            new LoadSceneParameters(LoadSceneMode.Single));
        var go = new GameObject("Clean HEAD Diagnostic Only");
        UnityEngine.Object.DontDestroyOnLoad(go);
        if (mode == "Full") { FullSystems.Output = output; go.AddComponent<FullSystems>().Begin(); }
        else { CleanFocused.Output = output; go.AddComponent<CleanFocused>().Begin(); }
    }
}
