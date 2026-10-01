using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
[InitializeOnLoad]
public static class HumanProbeEntry
{
    const string Key="HumanFixProbe20260930";
    static HumanProbeEntry(){EditorApplication.playModeStateChanged+=Changed;}
    public static void Run(){string path=null;var args=Environment.GetCommandLineArgs();for(int i=0;i<args.Length-1;i++)if(args[i]=="-humanEvidence")path=args[i+1];if(path==null)throw new Exception("Missing evidence path");Directory.CreateDirectory(path);PlayerSettings.companyName="HimoHitoHumanFixDiagnosticOnly";PlayerSettings.productName="HumanFixProbe_20260930";EditorSceneManager.playModeStartScene=null;EditorSceneManager.OpenScene("Assets/Scenes/Tutorial.unity");SessionState.SetString(Key,path);EditorApplication.EnterPlaymode();}
    static void Changed(PlayModeStateChange state){if(state!=PlayModeStateChange.EnteredPlayMode)return;string path=SessionState.GetString(Key,"");if(path=="")return;HumanProbe.Output=path;HumanProbe.Mode="All";var args=Environment.GetCommandLineArgs();for(int i=0;i<args.Length-1;i++)if(args[i]=="-humanMode")HumanProbe.Mode=args[i+1];var go=new GameObject("Human Fix Diagnostic Only");UnityEngine.Object.DontDestroyOnLoad(go);go.AddComponent<HumanProbe>().Begin();}
}
