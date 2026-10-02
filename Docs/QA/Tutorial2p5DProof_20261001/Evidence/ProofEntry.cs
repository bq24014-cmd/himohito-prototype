using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;
using HimoHito;

[InitializeOnLoad]
public static class ProofEntry
{
    const string Key = "Tutorial2p5DProof20261001";
    static ProofEntry() { EditorApplication.playModeStateChanged += Changed; }
    static string Arg(string key)
    {
        string[] args = Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length - 1; i++) if (args[i] == key) return args[i + 1];
        throw new Exception("Missing " + key);
    }
    static void Import()
    {
        foreach (string name in new[] { "Far", "Mid", "Near" })
        {
            string path = "Assets/Resources/Proof/Tutorial2p5D/Tutorial2p5D_" + name + ".png";
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 100;
            importer.alphaIsTransparency = true;
            importer.sRGBTexture = true;
            importer.mipmapEnabled = false;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.filterMode = FilterMode.Bilinear;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.maxTextureSize = 2048;
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            settings.spriteAlignment = (int)SpriteAlignment.Center;
            importer.SetTextureSettings(settings);
            importer.SaveAndReimport();
        }
        PlayerSettings.companyName = "HimoHitoProofOnly";
        PlayerSettings.productName = "Tutorial2p5DProof_20261001";
    }
    public static void Run()
    {
        Import();
        string output = Arg("-proofEvidence");
        Directory.CreateDirectory(output);
        EditorSceneManager.playModeStartScene = null;
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        SessionState.SetString(Key, output);
        EditorApplication.EnterPlaymode();
    }
    static void Changed(PlayModeStateChange state)
    {
        if (state != PlayModeStateChange.EnteredPlayMode) return;
        string output = SessionState.GetString(Key, "");
        if (output == "") return;
        SessionState.EraseString(Key);
        EditorSceneManager.LoadSceneInPlayMode("Assets/Scenes/Tutorial.unity", new LoadSceneParameters(LoadSceneMode.Single));
        var go = new GameObject("Isolated 2.5D controlled diagnostic ONLY");
        UnityEngine.Object.DontDestroyOnLoad(go);
        ProofRuntimeQA.Output = output;
        go.AddComponent<ProofRuntimeQA>().Begin();
    }
    public static void Build()
    {
        try
        {
            Import();
            string output = Arg("-proofBuild");
            if (Directory.Exists(output)) throw new Exception("Refuse to overwrite existing build: " + output);
            Directory.CreateDirectory(output);
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
                scenes = StageCatalog.GetBuildScenePaths(), target = BuildTarget.StandaloneWindows64,
                locationPathName = Path.Combine(output, "HimoHito2p5DProof.exe"), options = BuildOptions.None
            });
            File.WriteAllText(Path.Combine(output, "BUILD_RESULT.txt"),
                report.summary.result + "\nbytes=" + report.summary.totalSize + "\nseconds=" + report.summary.totalTime.TotalSeconds);
            EditorApplication.Exit(report.summary.result == BuildResult.Succeeded ? 0 : 2);
        }
        catch (Exception ex) { Debug.LogException(ex); EditorApplication.Exit(2); }
    }
}
