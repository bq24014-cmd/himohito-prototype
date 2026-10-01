using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using HimoHito;
[InitializeOnLoad]
public static class CheckpointMergedBridgeFix
{
    const string Key="CheckpointMergedBridgeFix20260930";
    static CheckpointMergedBridgeFix() { EditorApplication.playModeStateChanged+=Changed; }
    public static void Run()
    {
        string[] args=Environment.GetCommandLineArgs(); string test="Merged", evidence=null;
        for(int i=0;i<args.Length-1;i++) { if(args[i]=="-fixCase") test=args[i+1]; if(args[i]=="-fixEvidence") evidence=args[i+1]; }
        if(evidence==null) throw new Exception("Missing evidence path");
        Directory.CreateDirectory(evidence);
        PlayerSettings.companyName="HimoHitoFullRegressionDiagnosticOnly";
        PlayerSettings.productName="FullRegression_20260930";
        EditorSceneManager.playModeStartScene=null;
        string scene=test=="Tutorial"?"Tutorial":"MainStage";
        EditorSceneManager.OpenScene("Assets/Scenes/"+scene+".unity");
        SessionState.SetString(Key,evidence); SessionState.SetString(Key+"Case",test);
        Debug.Log("FIX_RUN Unity="+Application.unityVersion+" scene="+scene+" test="+test);
        EditorApplication.EnterPlaymode();
    }
    static void Changed(PlayModeStateChange state)
    {
        if(state!=PlayModeStateChange.EnteredPlayMode) return;
        string evidence=SessionState.GetString(Key,""); if(evidence=="") return;
        FixTrace.Output=evidence; FixTrace.Test=SessionState.GetString(Key+"Case","");
        var driver=new GameObject("Diagnostic Fix Regression Driver");
        UnityEngine.Object.DontDestroyOnLoad(driver);
        driver.AddComponent<FixDriver>().Begin();
    }
}
