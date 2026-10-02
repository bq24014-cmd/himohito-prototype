#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using HimoHito;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Isolated verification project only. Production never installs this entry.
[InitializeOnLoad]
public static class TerrainProductionEntry
{
    const string Key = "TutorialTerrainHybridProduction20261003";
    static TerrainProductionEntry() { EditorApplication.playModeStateChanged += Changed; }
    static string Arg(string key)
    {
        string[] args = Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length - 1; i++) if (args[i] == key) return args[i + 1];
        throw new Exception("Missing argument " + key);
    }
    public static void Run()
    {
        string output = Arg("-terrainEvidence");
        if (Directory.Exists(output)) throw new Exception("Refuse existing QA output " + output);
        Directory.CreateDirectory(output); AssetDatabase.Refresh();
        EditorSceneManager.playModeStartScene = null;
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        SessionState.SetString(Key, output); EditorApplication.EnterPlaymode();
    }
    static void Changed(PlayModeStateChange state)
    {
        if (state != PlayModeStateChange.EnteredPlayMode) return;
        string output = SessionState.GetString(Key, ""); if (output == "") return; SessionState.EraseString(Key);
        EditorSceneManager.LoadSceneInPlayMode("Assets/Scenes/Tutorial.unity", new LoadSceneParameters(LoadSceneMode.Single));
        Type type = Type.GetType("TerrainProductionQA, Assembly-CSharp");
        if (type == null) throw new Exception("Missing isolated TerrainProductionQA");
        type.GetField("Output").SetValue(null, output);
        var go = new GameObject("Tutorial all-terrain isolated automated QA only");
        UnityEngine.Object.DontDestroyOnLoad(go); type.GetMethod("Begin").Invoke(go.AddComponent(type), null);
    }
    [Serializable]
    sealed class PlayerAssemblyAudit
    {
        public bool passed;
        public string playerAssembly;
        public string sha256;
        public string cecilAssembly;
        public int inspectedTypeCount;
        public string[] failures;
        public string[] terrainMembers;
        public string[] forbiddenTypesFound;
        public string[] legitimateProbeTypes;
        public string scope = "Actual built Windows player Assembly-CSharp.dll, not Editor/Library assembly; no type loading or executing from player assembly";
    }
    static PropertyInfo FindProperty(Type type, string name)
    {
        // Cecil definitions can hide covariant reference properties. Choose
        // the most-derived declaration instead of ambiguous GetProperty(name).
        for (Type current = type; current != null; current = current.BaseType)
        {
            var property = current.GetProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            if (property != null) return property;
        }
        return null;
    }
    static object Property(object obj, string name) => FindProperty(obj.GetType(), name).GetValue(obj);
    static string Name(object obj) => (string)Property(obj, "Name");
    static void GatherTypes(IEnumerable types, List<object> result)
    {
        foreach (object type in types) { result.Add(type); GatherTypes((IEnumerable)Property(type, "NestedTypes"), result); }
    }
    static bool AuditPlayerAssembly(string output, string reportDirectory = null)
    {
        string path = Path.Combine(output, "HimoHitoTutorialTerrainHybridProduction_Data", "Managed", "Assembly-CSharp.dll");
        var audit = new PlayerAssemblyAudit { playerAssembly = path };
        var failures = new List<string>(); var members = new List<string>(); var forbidden = new List<string>(); var probes = new List<string>();
        object assembly = null;
        try
        {
            if (!File.Exists(path)) throw new FileNotFoundException("Built managed player Assembly-CSharp.dll is required for artifact audit", path);
            using (var hash = SHA256.Create()) using (var stream = File.OpenRead(path)) audit.sha256 = BitConverter.ToString(hash.ComputeHash(stream)).Replace("-", "");
            // Reflection avoids adding a Cecil compile dependency to QA Assets.
            // Unity ships this metadata reader; the player assembly is parsed,
            // never Assembly.Load'ed or executed in the Editor process.
            audit.cecilAssembly = Path.Combine(EditorApplication.applicationContentsPath, "Managed", "Unity.Cecil.dll");
            Assembly cecil = Assembly.LoadFrom(audit.cecilAssembly);
            Type definition = cecil.GetType("Mono.Cecil.AssemblyDefinition", true);
            assembly = definition.GetMethod("ReadAssembly", new[] { typeof(string) }).Invoke(null, new object[] { path });
            object module = Property(assembly, "MainModule"); var types = new List<object>();
            GatherTypes((IEnumerable)Property(module, "Types"), types); audit.inspectedTypeCount = types.Count;
            var forbiddenNames = new HashSet<string>(StringComparer.Ordinal)
            {
                "AllTerrainReview", "TerrainProductionReviewRetired", "TerrainProductionQA", "TerrainProductionQaFacade", "TerrainProductionEntry",
                "AllTerrainQA", "AllTerrainEntry", "ShapeProofQA", "ShapeProofEntry", "ShapeVisualProof", "Probe", "StandaloneFrameProbe",
                "IntroTimelineQA", "IntroEntry", "IntegrationQA", "IntegrationEntry", "ReferenceTrial"
            };
            object terrain = null;
            foreach (object type in types)
            {
                string name = Name(type), fullName = (string)Property(type, "FullName");
                if (forbiddenNames.Contains(name)) forbidden.Add(fullName);
                if (name.IndexOf("Probe", StringComparison.OrdinalIgnoreCase) >= 0 && !forbiddenNames.Contains(name)) probes.Add(fullName);
                if (fullName == "HimoHito.TutorialTerrainHybridVisual") terrain = type;
            }
            if (forbidden.Count > 0) failures.Add("QA/proof types present in compiled player assembly");
            if (terrain == null) failures.Add("Production terrain component missing from compiled player");
            else
            {
                var removed = new HashSet<string>(new[] { "Current", "Ready", "HybridEnabled", "TargetCount", "TextureCropInfo", "SetHybrid", "GetTarget", "GetVisualBounds" }, StringComparer.Ordinal);
                foreach (string kind in new[] { "Fields", "Properties", "Methods" }) foreach (object member in (IEnumerable)Property(terrain, kind))
                {
                    string name = Name(member); members.Add(kind + ":" + name);
                    string simple = name.StartsWith("get_", StringComparison.Ordinal) || name.StartsWith("set_", StringComparison.Ordinal) ? name.Substring(4) : name;
                    if (removed.Contains(simple)) failures.Add("Removed proof API survives compilation: " + kind + ":" + name);
                    foreach (string removedName in removed) if (name.Contains("<" + removedName + ">")) failures.Add("Removed proof property backing field survives: " + name);
                    if (kind == "Fields" && (bool)Property(member, "IsPublic")) failures.Add("Production terrain declares public field: " + name);
                    if (kind == "Methods")
                    {
                        if ((bool)Property(member, "IsPublic") && !(bool)Property(member, "IsConstructor")) failures.Add("Production terrain declares public method: " + name);
                        if (name == "OnGUI") failures.Add("Production terrain contains OnGUI debug UI");
                        if (!(bool)Property(member, "HasBody")) continue;
                        foreach (object instruction in (IEnumerable)Property(Property(member, "Body"), "Instructions"))
                        {
                            object operand = Property(instruction, "Operand"); if (operand == null) continue;
                            PropertyInfo declaring = FindProperty(operand.GetType(), "DeclaringType"); if (declaring == null) continue;
                            object declaringType = declaring.GetValue(operand); if (declaringType == null) continue;
                            string owner = Convert.ToString(Property(declaringType, "FullName"));
                            if (owner == "UnityEngine.Input" || owner.StartsWith("UnityEngine.InputSystem.", StringComparison.Ordinal))
                                failures.Add("Production terrain contains input call, forbidden for F7/debug-free presentation: " + name + " / " + operand);
                        }
                    }
                }
            }
            foreach (object reference in (IEnumerable)Property(module, "AssemblyReferences"))
                if (Name(reference).StartsWith("UnityEditor", StringComparison.Ordinal)) failures.Add("Built player references UnityEditor: " + Name(reference));
        }
        catch (Exception error) { failures.Add("Audit exception: " + error); }
        finally { (assembly as IDisposable)?.Dispose(); }
        audit.failures = failures.ToArray(); audit.terrainMembers = members.ToArray(); audit.forbiddenTypesFound = forbidden.ToArray(); audit.legitimateProbeTypes = probes.ToArray();
        audit.passed = failures.Count == 0;
        File.WriteAllText(Path.Combine(reportDirectory ?? output, "PLAYER_ASSEMBLY_AUDIT.json"), JsonUtility.ToJson(audit, true));
        return audit.passed;
    }
    public static void AuditExistingBuild()
    {
        try
        {
            string report = Arg("-terrainAudit");
            if (Directory.Exists(report)) throw new Exception("Refuse existing artifact audit report " + report);
            Directory.CreateDirectory(report);
            EditorApplication.Exit(AuditPlayerAssembly(Arg("-terrainBuild"), report) ? 0 : 2);
        }
        catch (Exception ex) { Debug.LogException(ex); EditorApplication.Exit(2); }
    }
    public static void Build()
    {
        try
        {
            AssetDatabase.Refresh(); string output = Arg("-terrainBuild");
            if (Directory.Exists(output)) throw new Exception("Refuse existing build output " + output);
            Directory.CreateDirectory(output);
            PlayerSettings.companyName = "HimoHitoVerificationOnly";
            PlayerSettings.productName = "TutorialTerrainHybridProduction_20261003";
            var result = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = StageCatalog.GetBuildScenePaths(), target = BuildTarget.StandaloneWindows64,
                locationPathName = Path.Combine(output, "HimoHitoTutorialTerrainHybridProduction.exe"), options = BuildOptions.None
            });
            File.WriteAllText(Path.Combine(output, "BUILD_RESULT.txt"), result.summary.result + "\nbytes=" + result.summary.totalSize + "\nseconds=" + result.summary.totalTime.TotalSeconds);
            bool artifactPass = result.summary.result == BuildResult.Succeeded && AuditPlayerAssembly(output);
            File.AppendAllText(Path.Combine(output, "BUILD_RESULT.txt"), "\nplayerAssemblyAudit=" + (artifactPass ? "PASS" : "FAIL") + "\n");
            EditorApplication.Exit(artifactPass ? 0 : 2);
        }
        catch (Exception ex) { Debug.LogException(ex); EditorApplication.Exit(2); }
    }
}
#endif
