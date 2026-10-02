using System;
using System.Collections;
using System.IO;
using System.Reflection;
using Unity.Profiling;
using UnityEngine;
using HimoHito;

// QA-only instrumentation in isolated builds, never a production source asset.
// Without the explicit flag it creates no object and handles no inputs.
public sealed class StandaloneFrameProbe : MonoBehaviour
{
    static string folder;
    static bool expectLayers;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Install()
    {
        var args = Environment.GetCommandLineArgs();
        expectLayers = Array.IndexOf(args, "--expect-2p5d") >= 0;
        for (int i = 0; i < args.Length - 1; i++) if (args[i] == "--background-perf-output")
        {
            folder = args[i + 1]; Directory.CreateDirectory(folder);
            // Both benchmark builds must continue while their helper window is hidden.
            // This flag is diagnostic-only and is never applied in normal Human play.
            Application.runInBackground = true;
            File.WriteAllText(Path.Combine(folder, "steps.txt"), "Probe installed; background execution enabled for both comparisons\n");
            new GameObject("Background QA frame probe").AddComponent<StandaloneFrameProbe>();
            return;
        }
    }
    IEnumerator Start()
    {
        File.AppendAllText(Path.Combine(folder, "steps.txt"), "Start scene=" + gameObject.scene.name + "\n");
        var run = FindFirstObjectByType<PrototypeRunController>();
        float until = Time.realtimeSinceStartup + 30;
        while (!run.CanUseTitleMenu() && Time.realtimeSinceStartup < until) yield return null;
        if (!run.CanUseTitleMenu()) { File.WriteAllText(Path.Combine(folder, "error.txt"), "Title unavailable"); Application.Quit(2); yield break; }
        run.BeginGameFromTitle(); yield return new WaitForSecondsRealtime(.3f);
        File.AppendAllText(Path.Combine(folder, "steps.txt"), "Title started\n");
        var preview = Camera.main.GetComponent<MainStagePreview>();
        if (preview != null && MainStagePreview.IsActive)
            preview.GetType().GetMethod("Advance", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(preview, new object[] { 0f, true });
        yield return new WaitForSecondsRealtime(2);
        if (run.Outcome != PrototypeRunController.RunOutcome.Playing)
        { File.WriteAllText(Path.Combine(folder, "error.txt"), "Tutorial did not start"); Application.Quit(2); yield break; }
        // Reflection keeps the identical probe compilable in the unmodified baseline.
        var layerType = typeof(PrototypeRunController).Assembly.GetType("HimoHito.TutorialCraftRoomLayers");
        UnityEngine.Object activeLayers = null;
        if (layerType != null) foreach (var candidate in Resources.FindObjectsOfTypeAll(layerType))
        {
            var component = candidate as MonoBehaviour;
            if (component != null && component.gameObject.scene.name == "Tutorial" && component.isActiveAndEnabled)
            { activeLayers = candidate; break; }
        }
        bool visible = activeLayers != null && (bool)layerType.GetProperty("Visible").GetValue(activeLayers);
        bool legacyHidden = activeLayers != null && (bool)layerType.GetMethod("LegacyHidden").Invoke(activeLayers, null);
        if (expectLayers && (!visible || !legacyHidden))
        { File.WriteAllText(Path.Combine(folder, "error.txt"), "Production 2.5D missing/hidden/duplicated"); Application.Quit(2); yield break; }
        // Do not add the disabled ScreenCapture module just for a QA probe.
        // Visual evidence is captured separately by the Editor-only harness.
        File.AppendAllText(Path.Combine(folder, "steps.txt"), "240 frame recording\n");
        File.WriteAllText(Path.Combine(folder, "frames.csv"), "frame,frame_ms,main_thread_ns,valid\n");
        using (var recorder = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "Main Thread", 1))
        {
            for (int i = 0; i < 240; i++)
            {
                yield return null;
                File.AppendAllText(Path.Combine(folder, "frames.csv"), i + "," + (Time.unscaledDeltaTime * 1000).ToString("F4") + "," + recorder.LastValue + "," + recorder.Valid + "\n");
            }
        }
        long textureBytes = 0;
        foreach (var texture in Resources.FindObjectsOfTypeAll<Texture2D>())
            textureBytes += UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(texture);
        File.WriteAllText(Path.Combine(folder, "result.txt"), "PASS\nscene=" + run.gameObject.scene.name + "\nsection=" + run.CurrentTutorialSection +
            "\nframes=240\ntextureBytes=" + textureBytes + "\nresolution=" + Screen.width + "x" + Screen.height +
            "\nvsync=" + QualitySettings.vSyncCount + "\ntargetFrameRate=" + Application.targetFrameRate +
            "\n2p5d_visible=" + visible + "\nlegacy_hidden=" + legacyHidden +
            "\nStationary Section1, real standalone frame timing, no recording clock. Main Thread includes waiting; not pure busy CPU.\n");
        Application.Quit(0);
    }
}
