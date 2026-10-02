using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEngine;
using HimoHito;

// Explicit-flag QA only. No instance exists in ordinary Human review play.
[DefaultExecutionOrder(10002)]
public sealed class IntroTimelineQA : MonoBehaviour
{
    public static string Output;
    public static bool ExpectedAfter;
    PrototypeRunController run; TutorialCraftRoomLayers layers; PlayerMover mover; Rigidbody2D body;
    Camera view; bool started, committed, introSeen, ending, resumed, done;
    float deadline, began, nextCapture, move, inputBegan; int checks, failures, errors, samples, series;
    int hiddenAfterCommit, doubleDraw, maxOwned, maxLegacy; bool wasPreview;
    static object Read(object obj, string name) => obj.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(obj);
    static string F(float value) => value.ToString("R", CultureInfo.InvariantCulture);
    static bool Drawn(SpriteRenderer r) => r != null && r.enabled && r.gameObject.activeInHierarchy && !r.forceRenderingOff;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Install()
    {
        var args = Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length - 1; i++) if (args[i] == "--intro-qa-output")
        {
            Output = args[i + 1]; ExpectedAfter = true; Directory.CreateDirectory(Output);
            Application.runInBackground = true;
            var qa = new GameObject("Standalone intro verification flag only").AddComponent<IntroTimelineQA>();
            UnityEngine.Object.DontDestroyOnLoad(qa.gameObject); qa.Begin(); return;
        }
    }
    public void Begin()
    {
        Directory.CreateDirectory(Output); deadline = Time.realtimeSinceStartup + 90;
        Application.logMessageReceived += OnLog;
        File.WriteAllText(Path.Combine(Output, "frames.csv"), "label,frame,realtime,preview_elapsed,preview_total,preview_active,curtain_active,curtain_phase,outcome,far,mid,near,legacy_drawable,owned_drawable,camera_x,camera_y,camera_z,orthographic_size,body_simulated,mover_enabled,rope_enabled,player_x,player_y,unscaled_dt\n");
        StartCoroutine(StartRoute());
    }
    IEnumerator StartRoute()
    {
        yield return null; yield return null;
        run = FindFirstObjectByType<PrototypeRunController>();
        layers = FindFirstObjectByType<TutorialCraftRoomLayers>();
        mover = run.GetComponent<PlayerMover>(); body = run.GetComponent<Rigidbody2D>(); view = Camera.main;
        while (!run.CanUseTitleMenu()) yield return null;
        Check(layers != null && !layers.Visible && layers.LegacyRestored(), "Menu retains legacy background, new layers hidden");
        Capture("Menu_Contract", 1600, 900); Record("Menu_Contract");
        began = Time.realtimeSinceStartup;
        Check(run.StartSelectedStage(), "Real StartSelectedStage curtain route accepted; no intro skip");
        started = true;
    }
    void OnLog(string text, string stack, LogType type)
    {
        if (type != LogType.Error && type != LogType.Exception) return;
        if (stack.Contains("UnityEditor.Search.SearchDatabase")) return;
        errors++; File.AppendAllText(Path.Combine(Output, "runtime_errors.txt"), text + "\n" + stack + "\n");
    }
    void Check(bool pass, string text)
    {
        checks++; if (!pass) failures++;
        File.AppendAllText(Path.Combine(Output, "checks.txt"), (pass ? "PASS " : "FAIL ") + text + "\n");
    }
    bool AllVisible() => layers.Visible && Drawn(layers.Layers[0]) && Drawn(layers.Layers[1]) && Drawn(layers.Layers[2]);
    int LegacyDrawn()
    {
        int n = 0;
        foreach (var r in layers.GetComponentsInChildren<SpriteRenderer>(true))
            if (!r.name.StartsWith("Tutorial 2.5D") && !r.name.StartsWith("Tutorial support projection") && Drawn(r)) n++;
        return n;
    }
    int OwnedDrawn()
    {
        int n = 0;
        foreach (var r in layers.GetComponentsInChildren<SpriteRenderer>(true))
            if ((r.name.StartsWith("Tutorial 2.5D") || r.name.StartsWith("Tutorial support projection")) && Drawn(r)) n++;
        return n;
    }
    void Point(string label)
    {
        Record(label); Capture(label, 1600, 900);
        bool shouldShow = ExpectedAfter || (!MainStagePreview.IsActive && !StageStartTransition.IsActive);
        Check(AllVisible() == shouldShow && ((LegacyDrawn() == 0) == shouldShow), label + " expected layer/legacy visibility");
    }
    void LateUpdate()
    {
        if (done) return;
        if (Time.realtimeSinceStartup > deadline) { Check(false, "90s timeout"); Finish(); return; }
        if (!started || layers.Layers == null) return;
        var preview = view.GetComponent<MainStagePreview>();
        bool active = MainStagePreview.IsActive;
        bool playing = run.Outcome != PrototypeRunController.RunOutcome.WaitingToStart;
        if (playing)
        {
            samples++; if (!AllVisible()) hiddenAfterCommit++;
            int old = LegacyDrawn(), owned = OwnedDrawn();
            if (old > 0 && owned > 0) doubleDraw++;
            maxOwned = Mathf.Max(maxOwned, owned); maxLegacy = Mathf.Max(maxLegacy, old);
        }
        Record("Frame");
        if (playing && !committed) { committed = true; Point("A_TutorialCommitted"); }
        if (committed && !StageStartTransition.IsActive && active && !introSeen)
        { introSeen = true; Point("B_IntroStart"); }
        if (introSeen && active)
        {
            float elapsed = (float)Read(preview, "elapsed");
            float total = (float)Read(preview, "holdDuration") + (float)Read(preview, "travelDuration");
            if (!ending && elapsed >= total * .97f) { ending = true; Point("D_IntroNearEnd"); }
            if (!midCaptured && elapsed >= total * .5f) { midCaptured = true; Point("C_IntroMiddle"); }
        }
        if (committed && Time.realtimeSinceStartup >= nextCapture && !resumed)
        {
            nextCapture = Time.realtimeSinceStartup + .2f;
            string label = "Sequence_" + (series++).ToString("D3"); Capture(label, 960, 540); Record(label);
        }
        if (wasPreview && !active && committed && !resumed)
        {
            resumed = true; Point("E_GameplayResumed");
            Check(body.simulated && mover.enabled && run.GetComponent<RopeController>().enabled, "Normal camera intro restored control states");
            inputBegan = Time.realtimeSinceStartup; move = 1f;
        }
        if (resumed)
        {
            mover.GetType().GetField("moveInput", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(mover, move);
            if (Time.realtimeSinceStartup >= inputBegan + .35f)
            {
                move = 0; mover.GetType().GetField("moveInput", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(mover, 0f);
                Point("F_AfterMovement"); Check(body.position.x > TutorialSectionOneSetup.StartRespawnPosition.x + .03f, "Real cached move input advances player after intro");
                Check(committed && introSeen && midCaptured && ending && resumed, "All six milestones captured");
                Check(doubleDraw == 0, "No simultaneous legacy/new drawable renderers in any observed frame");
                Check(ExpectedAfter ? hiddenAfterCommit == 0 : hiddenAfterCommit > 0, "Expected AFTER continuity / BEFORE failure reproduced");
                Finish();
            }
        }
        wasPreview = active;
    }
    bool midCaptured;
    void Record(string label)
    {
        var preview = view.GetComponent<MainStagePreview>(); var transition = FindFirstObjectByType<StageStartTransition>();
        float elapsed = preview == null ? 0 : (float)Read(preview, "elapsed");
        float total = preview == null ? 0 : (float)Read(preview, "holdDuration") + (float)Read(preview, "travelDuration");
        var pos = view.transform.position;
        File.AppendAllText(Path.Combine(Output, "frames.csv"), string.Join(",", label, Time.frameCount,
            F(Time.realtimeSinceStartup - began), F(elapsed), F(total), MainStagePreview.IsActive, StageStartTransition.IsActive,
            transition == null ? "None" : Read(transition, "phase").ToString(), run.Outcome,
            Drawn(layers.Layers[0]), Drawn(layers.Layers[1]), Drawn(layers.Layers[2]), LegacyDrawn(), OwnedDrawn(),
            F(pos.x), F(pos.y), F(pos.z), F(view.orthographicSize), body.simulated, mover.enabled,
            run.GetComponent<RopeController>().enabled, F(body.position.x), F(body.position.y), F(Time.unscaledDeltaTime)) + "\n");
    }
    void Capture(string label, int w, int h)
    {
        // World-camera render, not a UI screen capture. Restore target exactly.
        var rt = new RenderTexture(w, h, 24); var tex = new Texture2D(w, h, TextureFormat.RGB24, false);
        var target = view.targetTexture; var prior = RenderTexture.active;
        try {
            view.targetTexture = rt; view.Render(); RenderTexture.active = rt;
            tex.ReadPixels(new Rect(0, 0, w, h), 0, 0); tex.Apply();
            var pixels = tex.GetPixels32(); var bytes = new byte[w * h * 3];
            for (int y = 0; y < h; y++) for (int x = 0; x < w; x++)
            { var c = pixels[(h - 1 - y) * w + x]; int k = (y * w + x) * 3; bytes[k] = c.r; bytes[k + 1] = c.g; bytes[k + 2] = c.b; }
            using (var f = File.Create(Path.Combine(Output, label + ".ppm")))
            { byte[] header = Encoding.ASCII.GetBytes($"P6\n{w} {h}\n255\n"); f.Write(header, 0, header.Length); f.Write(bytes, 0, bytes.Length); }
        } finally { view.targetTexture = target; RenderTexture.active = prior; Destroy(rt); Destroy(tex); }
    }
    void Finish()
    {
        if (done) return; done = true; Application.logMessageReceived -= OnLog;
        File.WriteAllText(Path.Combine(Output, "result.txt"), $"checks={checks}\nfailures={failures}\nruntimeErrors={errors}\nplayingFrames={samples}\nhiddenAfterCommit={hiddenAfterCommit}\ndoubleDrawFrames={doubleDraw}\nmaxOwnedDrawable={maxOwned}\nmaxLegacyDrawable={maxLegacy}\nsequenceFrames={series}\nvariant={(ExpectedAfter ? "After" : "Before")}\nNatural existing curtain + unskipped camera intro. Cached-input movement is automated QA, not Human keyboard approval.\n");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.Exit(failures == 0 && errors == 0 ? 0 : 2);
#else
        Application.Quit(failures == 0 && errors == 0 ? 0 : 2);
#endif
    }
}
