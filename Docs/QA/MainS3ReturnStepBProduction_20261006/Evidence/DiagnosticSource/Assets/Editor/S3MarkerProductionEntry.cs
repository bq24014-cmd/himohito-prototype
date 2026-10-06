#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class S3MarkerProductionEntry
{
    const string Key = "S3MarkerProduction20261006";
    static S3MarkerProductionEntry() { EditorApplication.playModeStateChanged += Changed; }
    static string Arg(string key)
    {
        var args=Environment.GetCommandLineArgs();
        for(int i=0;i<args.Length-1;i++)if(args[i]==key)return args[i+1];
        throw new Exception("Missing "+key);
    }
    public static void Run()
    {
        string output=Arg("-s3ProductionOutput");
        if(Directory.Exists(output))throw new Exception("Refuse to overwrite proof evidence");
        Directory.CreateDirectory(output);
        SessionState.SetString(Key,output);
        EditorSceneManager.playModeStartScene=null;
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        EditorApplication.EnterPlaymode();
    }
    static void Changed(PlayModeStateChange state)
    {
        if(state==PlayModeStateChange.EnteredPlayMode)
        {
            string output=SessionState.GetString(Key,"");if(output=="")return;
            S3MarkerProductionQA.Output=output;
            S3MarkerProductionQA.EncodePng=t=>t.EncodeToPNG();
            EditorSceneManager.LoadSceneInPlayMode("Assets/Scenes/MainStage.unity",new LoadSceneParameters(LoadSceneMode.Single));
            var observer=new GameObject("Isolated S3 Step B automated proof observer");
            UnityEngine.Object.DontDestroyOnLoad(observer);observer.AddComponent<S3MarkerProductionQA>();
        }
        if(state==PlayModeStateChange.EnteredEditMode&&SessionState.GetString(Key,"")!="")
        {
            SessionState.EraseString(Key);EditorApplication.delayCall+=()=>EditorApplication.Exit(0);
        }
    }
}
#endif
