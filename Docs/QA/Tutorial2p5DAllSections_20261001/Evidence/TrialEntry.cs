using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;
using HimoHito;

[InitializeOnLoad]
public static class TrialEntry
{
    const string Key = "TutorialAll2p5D20261001";
    static TrialEntry() { EditorApplication.playModeStateChanged += Changed; }
    static string Arg(string key)
    {
        var args = Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length - 1; i++) if (args[i] == key) return args[i + 1];
        throw new Exception("Missing argument: " + key);
    }
    static void Import()
    {
        foreach (string name in new[] { "Far", "Mid", "Near", "NearRefined" })
        {
            var imp = (TextureImporter)AssetImporter.GetAtPath("Assets/Resources/Proof/Tutorial2p5D/Tutorial2p5D_" + name + ".png");
            imp.textureType = TextureImporterType.Sprite; imp.spriteImportMode = SpriteImportMode.Single;
            imp.spritePixelsPerUnit = 100; imp.alphaIsTransparency = true; imp.sRGBTexture = true;
            imp.mipmapEnabled = false; imp.wrapMode = TextureWrapMode.Clamp; imp.filterMode = FilterMode.Bilinear;
            imp.textureCompression = TextureImporterCompression.Uncompressed; imp.maxTextureSize = 2048;
            var settings = new TextureImporterSettings(); imp.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect; settings.spriteAlignment = (int)SpriteAlignment.Center;
            imp.SetTextureSettings(settings); imp.SaveAndReimport();
        }
        PlayerSettings.companyName = "HimoHitoProofOnly";
        PlayerSettings.productName = "Tutorial2p5DAllSections_20261001";
    }
    public static void Run()
    {
        Import(); string output = Arg("-trialEvidence"); Directory.CreateDirectory(output);
        EditorSceneManager.playModeStartScene = null;
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        SessionState.SetString(Key, output); EditorApplication.EnterPlaymode();
    }
    static void Changed(PlayModeStateChange state)
    {
        if (state != PlayModeStateChange.EnteredPlayMode) return;
        string output = SessionState.GetString(Key, ""); if (output == "") return; SessionState.EraseString(Key);
        EditorSceneManager.LoadSceneInPlayMode("Assets/Scenes/Tutorial.unity", new LoadSceneParameters(LoadSceneMode.Single));
        var go = new GameObject("All sections controlled diagnostic — NOT human keyboard");
        UnityEngine.Object.DontDestroyOnLoad(go); AllSectionsQA.Output = output; go.AddComponent<AllSectionsQA>().Begin();
    }
    public static void Build()
    {
        try
        {
            Import(); string output = Arg("-trialBuild");
            if (Directory.Exists(output)) throw new Exception("Refuse to overwrite build: " + output);
            Directory.CreateDirectory(output);
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
                scenes = StageCatalog.GetBuildScenePaths(), target = BuildTarget.StandaloneWindows64,
                locationPathName = Path.Combine(output, "HimoHitoTutorialAll2p5D.exe"), options = BuildOptions.None });
            File.WriteAllText(Path.Combine(output, "BUILD_RESULT.txt"), report.summary.result + "\nbytes=" + report.summary.totalSize + "\nseconds=" + report.summary.totalTime.TotalSeconds);
            EditorApplication.Exit(report.summary.result == BuildResult.Succeeded ? 0 : 2);
        }
        catch (Exception ex) { Debug.LogException(ex); EditorApplication.Exit(2); }
    }
}
