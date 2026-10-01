using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
[InitializeOnLoad]
public static class FullSystemsEntry
{
    const string Key="FullSystemsEvidence20260930";
    static FullSystemsEntry(){EditorApplication.playModeStateChanged+=Changed;}
    public static void Run()
    {
        string output=null;var args=Environment.GetCommandLineArgs();for(int i=0;i<args.Length-1;i++)if(args[i]=="-fullEvidence")output=args[i+1];
        if(output==null)throw new Exception("Missing evidence");Directory.CreateDirectory(output);
        PlayerSettings.companyName="HimoHitoFullRegressionDiagnosticOnly";PlayerSettings.productName="FullSystems_20260930";
        EditorSceneManager.playModeStartScene=null;EditorSceneManager.OpenScene("Assets/Scenes/MainStage.unity");SessionState.SetString(Key,output);EditorApplication.EnterPlaymode();
    }
    static void Changed(PlayModeStateChange state)
    {
        if(state!=PlayModeStateChange.EnteredPlayMode)return;string output=SessionState.GetString(Key,"");if(output=="")return;
        FullSystems.Output=output;var go=new GameObject("Full Regression Diagnostic Only");UnityEngine.Object.DontDestroyOnLoad(go);go.AddComponent<FullSystems>().Begin();
    }
}
