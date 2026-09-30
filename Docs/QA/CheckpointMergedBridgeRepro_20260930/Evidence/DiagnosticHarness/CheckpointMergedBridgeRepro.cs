using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using HimoHito;
[InitializeOnLoad]
public static class CheckpointMergedBridgeRepro
{
    const string Key="HimoHitoCheckpointRepro20260930";
    static CheckpointMergedBridgeRepro() { EditorApplication.playModeStateChanged+=Changed; }
    public static void Run()
    {
        string[] args=Environment.GetCommandLineArgs(); string scene="Tutorial", evidence=null;
        for(int i=0;i<args.Length-1;i++) { if(args[i]=="-reproScene") scene=args[i+1]; if(args[i]=="-reproEvidence") evidence=args[i+1]; }
        if(evidence==null) throw new Exception("Missing evidence directory");
        Directory.CreateDirectory(evidence);
        PlayerSettings.companyName="HimoHitoCheckpointDiagnosticOnly";
        PlayerSettings.productName="CheckpointRepro_20260930";
        EditorSceneManager.playModeStartScene=null;
        EditorSceneManager.OpenScene("Assets/Scenes/"+scene+".unity");
        SessionState.SetString(Key,evidence);
        Debug.Log("REPRO_RUN Unity="+Application.unityVersion+" scene="+scene+" setup=controlled relocation; not full manual play");
        EditorApplication.EnterPlaymode();
    }
    static void Changed(PlayModeStateChange state)
    {
        if(state!=PlayModeStateChange.EnteredPlayMode) return;
        string evidence=SessionState.GetString(Key,""); if(evidence=="") return;
        ReproTrace.Output=evidence;
        var host=new GameObject("Diagnostic Reproduction Driver");
        host.AddComponent<ReproDriver>().Begin();
    }
}
