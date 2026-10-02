#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using HimoHito;
using HimoHitoAllTrial;
using Unity.Profiling;

// Reuses the previously passing Tutorial route. Actual forces/collisions, no teleport.
[DefaultExecutionOrder(10001)]
public sealed class IntegrationQA : MonoBehaviour
{
    public static string Output;
    PlayerMover mover; Rigidbody2D body; RopeController rope; RopeResource resource;
    RopePlatformBuilder builder; PrototypeRunController run; TutorialSectionGuide guide;
    float move, deadline, nextMotion; int checks, failures, errors, searchErrors, motionIndex;
    bool done, track; Vector3 priorCam; Vector3[] priorLayers; int priorSection; int transitions;
    static void Set(object o, string n, object v) => o.GetType().GetField(n, BindingFlags.NonPublic | BindingFlags.Instance).SetValue(o, v);
    static object Call(object o, string n, params object[] args) => o.GetType().GetMethod(n, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Invoke(o, args);
    void Log(string text) { File.AppendAllText(Path.Combine(Output, "checks.txt"), text + "\n"); Debug.Log("INTEGRATION_QA " + text); }
    void Check(bool pass, string text) { checks++; if (!pass) failures++; Log((pass ? "PASS " : "FAIL ") + text); }
    bool Required(bool pass, string text) { Check(pass, text); if (!pass) Finish(); return pass; }
    public void Begin()
    {
        // Diagnostic recording clock only: expensive capture IO must not advance
        // several physics steps before the cached input can be sampled again.
        // Production fixedDeltaTime, physics and standalone input are unchanged.
        Time.captureDeltaTime = 1f / 60f;
        deadline = Time.realtimeSinceStartup + 600;
        Application.logMessageReceived += OnLog; StartCoroutine(Run());
    }
    void OnLog(string text, string stack, LogType type)
    {
        if (type != LogType.Error && type != LogType.Exception) return;
        bool startup = stack.Contains("UnityEditor.Search.SearchDatabase");
        if (startup) searchErrors++; else errors++;
        File.AppendAllText(Path.Combine(Output, startup ? "editor_search_errors.txt" : "runtime_errors.txt"), text + "\n" + stack + "\n");
    }
    void Update() { if (!done && Time.realtimeSinceStartup > deadline) { Check(false, "Timeout"); Finish(); } }
    void LateUpdate()
    {
        if (mover != null && !done) Set(mover, "moveInput", move);
        var p = FindFirstObjectByType<TutorialCraftRoomLayers>();
        if (!track || done || p == null || !p.Visible) return;
        var cam = Camera.main.transform.position;
        if (priorLayers != null && run.CurrentTutorialSection != priorSection)
        {
            float dx = cam.x - priorCam.x; bool continuous = true;
            for (int i = 0; i < 3; i++) continuous &= Mathf.Abs(p.Layers[i].transform.position.x - priorLayers[i].x - p.Follow(i) * dx) < .02f;
            Check(continuous, "Actual section " + priorSection + "->" + run.CurrentTutorialSection + " preserves background phase"); transitions++;
        }
        priorCam = cam; priorSection = run.CurrentTutorialSection;
        priorLayers ??= new Vector3[3];
        for (int i = 0; i < 3; i++) priorLayers[i] = p.Layers[i].transform.position;
        if (Time.time >= nextMotion && run.Outcome == PrototypeRunController.RunOutcome.Playing)
        {
            nextMotion = Time.time + .8f;
            string label = "Motion_" + (motionIndex++).ToString("D3"); Capture(label, 640, 360);
            File.AppendAllText(Path.Combine(Output, "motion.csv"), label + "," + Time.time.ToString("F4") + "," + run.CurrentTutorialSection + "," + body.position.x.ToString("F4") + "," + body.position.y.ToString("F4") + "," + cam.x.ToString("F4") + "," + priorLayers[0].x.ToString("F4") + "," + priorLayers[1].x.ToString("F4") + "," + priorLayers[2].x.ToString("F4") + "\n");
        }
    }
    void Bind()
    {
        mover = FindFirstObjectByType<PlayerMover>(); body = mover.GetComponent<Rigidbody2D>(); rope = mover.GetComponent<RopeController>();
        resource = mover.GetComponent<RopeResource>(); builder = mover.GetComponent<RopePlatformBuilder>(); run = mover.GetComponent<PrototypeRunController>(); guide = mover.GetComponent<TutorialSectionGuide>();
    }
    string State()
    {
        var s = new StringBuilder();
        s.Append(body.position.ToString("R")).Append(body.linearVelocity.ToString("R")).Append(body.rotation).Append(body.angularVelocity).Append(body.simulated)
            .Append(mover.enabled).Append(rope.enabled).Append(rope.IsAttached).Append(rope.SelectedRopeLength).Append(resource.CurrentLength)
            .Append(run.CurrentTutorialSection).Append(run.Outcome).Append(Camera.main.transform.position.ToString("R")).Append(Camera.main.orthographicSize).Append(Time.timeScale);
        foreach (var c in FindObjectsByType<Collider2D>(FindObjectsSortMode.InstanceID)) s.Append(UnityEditor.EditorJsonUtility.ToJson(c)).Append(c.transform.position.ToString("R")).Append(c.transform.lossyScale.ToString("R"));
        foreach (var j in FindObjectsByType<Joint2D>(FindObjectsSortMode.InstanceID)) s.Append(UnityEditor.EditorJsonUtility.ToJson(j));
        foreach (var b in builder.CapturePlatformStates()) s.Append(b.Start.ToString("R")).Append(b.End.ToString("R")).Append(b.RopeLength);
        foreach (var h in builder.CaptureRemovedHooks()) s.Append(h.GetInstanceID()).Append(h.activeSelf);
        return s.ToString();
    }
    void Capture(string label, int w = 1600, int h = 900)
    {
        var cam = Camera.main; var rt = new RenderTexture(w, h, 24); var tex = new Texture2D(w, h, TextureFormat.RGB24, false);
        var priorTarget = cam.targetTexture; var priorActive = RenderTexture.active;
        cam.targetTexture = rt; cam.Render(); RenderTexture.active = rt; tex.ReadPixels(new Rect(0, 0, w, h), 0, 0); tex.Apply();
        var pixels = tex.GetPixels32(); var bytes = new byte[w * h * 3];
        for (int y = 0; y < h; y++) for (int x = 0; x < w; x++)
        { var c = pixels[(h - 1 - y) * w + x]; int k = (y * w + x) * 3; bytes[k] = c.r; bytes[k + 1] = c.g; bytes[k + 2] = c.b; }
        using (var f = File.Create(Path.Combine(Output, label + ".ppm")))
        { var header = Encoding.ASCII.GetBytes($"P6\n{w} {h}\n255\n"); f.Write(header, 0, header.Length); f.Write(bytes, 0, bytes.Length); }
        cam.targetTexture = priorTarget; RenderTexture.active = priorActive; Destroy(rt); Destroy(tex);
    }
    void Pair(string label)
    {
        var p = FindFirstObjectByType<TutorialCraftRoomLayers>(); string initial = State();
        p.enabled = false; ReferenceTrial.Mode = ReferenceTrial.DisplayMode.AllTutorial; ReferenceTrial.Current.Refresh(); Capture(label + "_Before");
        Check(State() == initial, label + " T1 Before physics/camera/rope unchanged");
        ReferenceTrial.Mode = ReferenceTrial.DisplayMode.Production; ReferenceTrial.Current.Refresh(); p.enabled = true; p.Refresh(); Capture(label + "_After");
        Check(State() == initial && p.Visible && p.LegacyHidden(), label + " Production After state unchanged / legacy renderers hidden");
        if (label == "S4_Clear") SeamEvidence(p);
    }
    void SeamEvidence(TutorialCraftRoomLayers p)
    {
        var children = p.GetComponentsInChildren<SpriteRenderer>();
        var enabled = new bool[children.Length];
        for (int i = 0; i < children.Length; i++) enabled[i] = children[i].enabled;
        foreach (string layer in new[] { "Far", "Mid", "NearRefined" })
        {
            for (int i = 0; i < children.Length; i++) children[i].enabled = enabled[i] && children[i].name.StartsWith("Tutorial 2.5D " + layer);
            Capture("Goal_" + layer + "_Only");
        }
        for (int i = 0; i < children.Length; i++) children[i].enabled = enabled[i];
        var cam = Camera.main; var prior = cam.targetTexture; var target = new RenderTexture(1600, 900, 24);
        cam.targetTexture = target;
        float edge = p.Layers[1].bounds.max.x, floorEdge = p.FloorTiles[1].bounds.max.x;
        Vector3 screen = cam.WorldToViewportPoint(new Vector3(edge, cam.transform.position.y, 5));
        Vector3 floorScreen = cam.WorldToViewportPoint(new Vector3(floorEdge, cam.transform.position.y, 3.1f));
        File.WriteAllText(Path.Combine(Output, "seam_position.txt"), "capture=1600x900\nmid_edge_world_x=" + edge +
            "\nmid_edge_viewport_x=" + screen.x + "\nfloor_edge_world_x=" + floorEdge + "\nfloor_edge_viewport_x=" + floorScreen.x +
            "\nfloor_edge_pixel_x=" + floorScreen.x * 1600 + "\nsource_right_column=1919 completely transparent\n" +
            "Coordinates use the actual 1600x900 capture projection; inspect both Mid and floor boundaries.\n");
        cam.targetTexture = prior; Destroy(target);
        Check(p.Layers[1].sharedMaterial.shader.name == "HimoHito/Tutorial Background Edge", "Mid edge-clamp shader active");
        Check(p.FloorTiles[1].sharedMaterial.shader.name == "HimoHito/Tutorial Background Floor", "Floor edge-clamp / sprite flip shader active");
    }
    IEnumerator WalkTo(float x, float limit = 10)
    {
        float oldX = body.position.x; float until = Time.time + limit;
        while (Mathf.Abs(x - body.position.x) > .08f && Time.time < until && !done && run.Outcome == PrototypeRunController.RunOutcome.Playing)
        {
            move = Mathf.Clamp((x - body.position.x) * 2f, -1f, 1f);
            yield return null;
        }
        move = 0; Set(mover, "moveInput", 0f);
        if (!Required(Mathf.Abs(body.position.x - x) < .6f && run.Outcome == PrototypeRunController.RunOutcome.Playing, "Walk " + oldX + " -> " + x + " actual=" + body.position)) yield break;
        yield return new WaitForSeconds(.2f);
    }
    IEnumerator LessonStart(int section)
    {
        if (!Required(run.CurrentTutorialSection == section, "Section " + section + " entered through authored route")) yield break;
        float length = resource.CurrentLength, selected = rope.SelectedRopeLength;
        int platforms = builder.GeneratedPlatformCount;
        Call(run, "RestartFromCheckpoint"); yield return new WaitForSeconds(.9f);
        if (!Required(run.CurrentTutorialSection == section && run.Outcome == PrototypeRunController.RunOutcome.Playing &&
            body.simulated && mover.enabled && rope.enabled && resource.CurrentLength == length &&
            rope.SelectedRopeLength == selected && builder.GeneratedPlatformCount == platforms,
            "Section " + section + " R normal checkpoint restoration / same resource and bridge state")) yield break;
        Pair("S" + section + "_Standing");
        float startX = body.position.x;
        yield return WalkTo(startX - .35f); if (done) yield break;
        yield return WalkTo(startX); if (done) yield break;
        Pair("S" + section + "_Walk");
        float priorY = body.position.y; Set(mover, "jumpBufferTimer", .15f); yield return new WaitForSeconds(.16f);
        Check(body.position.y > priorY + .3f, "Section " + section + " normal jump through PlayerMover"); Pair("S" + section + "_Jump");
        float until = Time.time + 3; while (!mover.IsGrounded && Time.time < until && !done) yield return null;
        if (!Required(mover.IsGrounded, "Section " + section + " jump lands")) yield break;
        Call(guide, "OpenGuide", section); var cam = Camera.main.transform.position;
        Check(guide.IsVisible && Time.timeScale == 0 && !mover.enabled && !rope.enabled, "Section " + section + " sign opens through existing UI path");
        yield return new WaitForSecondsRealtime(.12f); FindFirstObjectByType<TutorialCraftRoomLayers>().Refresh();
        Check(FindFirstObjectByType<TutorialCraftRoomLayers>().Visible && Camera.main.transform.position == cam, "Section " + section + " sign pause preserves background/camera");
        Capture("S" + section + "_SignWorldOnly"); Call(guide, "CloseGuide");
        Check(!guide.IsVisible && Time.timeScale == 1 && mover.enabled && rope.enabled, "Section " + section + " sign closes/resumes");
    }
    IEnumerator SwingToNext(Vector2 hook, float releaseX, int nextSection)
    {
        Set(rope, "keyboardAimDirection", (hook - body.position).normalized); yield return null;
        Check(rope.TryResolveCurrentAimHook(out var h, out _) && h != null, "S" + (nextSection - 1) + " Hook aim resolves");
        if (!Required(rope.TryAttach(hook), "S" + (nextSection - 1) + " E attach")) yield break;
        Pair("S" + (nextSection - 1) + "_Attached"); move = 1; float until = Time.time + 15; bool released = false;
        while (run.CurrentTutorialSection < nextSection && Time.time < until && !done)
        {
            if (!released && body.position.x >= releaseX && body.linearVelocity.x > 2 && body.linearVelocity.y > 0) { rope.DetachAndRefund(true); released = true; }
            if (run.Outcome != PrototypeRunController.RunOutcome.Playing) { Required(false, "Swing failed at " + body.position); yield break; }
            yield return null;
        }
        move = 0; if (!Required(run.CurrentTutorialSection == nextSection, "Physical swing landing checkpoint " + nextSection)) yield break;
        yield return new WaitForSeconds(.2f);
    }
    IEnumerator BuildMerge()
    {
        rope.RestoreSelectedRopeLength(6); yield return WalkTo(41.35f); if (done) yield break;
        if (!Required(rope.TryAttach(TutorialSectionFourSetup.CenterHookPosition) && builder.TryBuildCurrentPlatform(), "S4 first E/Q")) yield break;
        Pair("S4_FirstBridge"); yield return WalkTo(TutorialSectionFourSetup.CenterHookPosition.x - .65f); if (done) yield break;
        Set(rope, "keyboardAimDirection", (TutorialSectionFourSetup.RightAnchorPosition - body.position).normalized); yield return null;
        Check(rope.TryResolveCurrentAimHook(out var h, out _) && h != null, "S4 second right Hook aim");
        if (!Required(rope.TryAttach(TutorialSectionFourSetup.RightAnchorPosition) && builder.TryBuildCurrentPlatform(), "S4 second E/Q")) yield break;
        Pair("S4_TwoBridges"); Set(rope, "keyboardAimDirection", (TutorialSectionFourSetup.CenterHookPosition - body.position).normalized);
        if (!Required(builder.TryRemoveAimedHook(), "S4 F merge")) yield break;
        yield return new WaitForSeconds(.8f); Pair("S4_Merged");
        Check(builder.GeneratedPlatformCount == 2 && !GameObject.Find(TutorialSectionFourSetup.CenterHookName) && resource.CurrentLength == 80, "S3 bridge + merged S4, center inactive, rope80");
    }
    void Performance()
    {
        var cam = Camera.main; var p = FindFirstObjectByType<TutorialCraftRoomLayers>(); var rt = new RenderTexture(1600, 900, 24); var prior = cam.targetTexture; cam.targetTexture = rt;
        File.WriteAllText(Path.Combine(Output, "render_timing.csv"), "mode,sample,render_cpu_ms\n");
        for (int n = 0; n < 40; n++)
        {
            p.enabled = n % 2 != 0; p.Refresh();
            var timer = System.Diagnostics.Stopwatch.StartNew(); cam.Render(); timer.Stop();
            if (n >= 10) File.AppendAllText(Path.Combine(Output, "render_timing.csv"), (n % 2 == 0 ? "before" : "all") + "," + n + "," + timer.Elapsed.TotalMilliseconds.ToString("F4") + "\n");
        }
        cam.targetTexture = prior; Destroy(rt); p.enabled = true; p.Refresh();
        long bytes = 0; foreach (var layer in p.Layers) bytes += UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(layer.sprite.texture);
        Log("THREE_ACTIVE_TEXTURE_RUNTIME_BYTES " + bytes + "; helpers reuse Mid and a small 128x64 shadow; not total process/GPU memory");
    }
    IEnumerator FramePerformance()
    {
        var p = FindFirstObjectByType<TutorialCraftRoomLayers>();
        Time.captureDeltaTime = 0;
        File.WriteAllText(Path.Combine(Output, "frame_timing.csv"), "mode,sample,frame_ms,main_thread_ns,valid\n");
        foreach (bool production in new[] { false, true })
        {
            p.enabled = production; p.Refresh();
            yield return new WaitForSecondsRealtime(1);
            using (var recorder = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "Main Thread", 1))
            {
                for (int i = 0; i < 120; i++)
                {
                    yield return null;
                    File.AppendAllText(Path.Combine(Output, "frame_timing.csv"), (production ? "production" : "original") + "," + i + "," +
                        (Time.unscaledDeltaTime * 1000).ToString("F4") + "," + recorder.LastValue + "," + recorder.Valid + "\n");
                }
            }
        }
        p.enabled = true; p.Refresh(); Time.captureDeltaTime = 1f / 60f;
    }
    IEnumerator Run()
    {
        File.WriteAllText(Path.Combine(Output, "motion.csv"), "frame,time,section,player_x,player_y,camera_x,far_x,mid_x,near_x\n");
        yield return null; yield return null; Bind(); while (!run.CanUseTitleMenu()) yield return null;
        Check(run.IsStageSelectionOpen && !FindFirstObjectByType<TutorialCraftRoomLayers>().Visible, "Stage Selection remains original");
        run.BeginGameFromTitle(); yield return new WaitForSecondsRealtime(.25f);
        var preview = Camera.main.GetComponent<MainStagePreview>(); if (preview != null && MainStagePreview.IsActive) Call(preview, "Advance", 0f, true);
        yield return new WaitForSeconds(.8f); track = true;
        Check(FindFirstObjectByType<TutorialCraftRoomLayers>().GetComponentsInChildren<Collider2D>().Length == 0 && FindFirstObjectByType<TutorialCraftRoomLayers>().GetComponentsInChildren<Rigidbody2D>().Length == 0, "Background has no physics components");
        yield return LessonStart(1); if (done) yield break;
        yield return WalkTo(-1.3f); if (done) yield break;
        yield return SwingToNext(TutorialSectionOneSetup.HookPosition, 6, 2); if (done) yield break;
        Call(run, "RestartFromCheckpoint"); yield return new WaitForSeconds(.9f);
        Check(run.CurrentTutorialSection == 2 && resource.CurrentLength == 99 && rope.SelectedRopeLength == 6, "S2 checkpoint R restores section/resource/selection");
        yield return LessonStart(2); if (done) yield break;
        yield return WalkTo(14.7f); if (done) yield break;
        yield return SwingToNext(TutorialSectionTwoSetup.HookPosition, 22, 3); if (done) yield break;
        yield return LessonStart(3); if (done) yield break;
        rope.RestoreSelectedRopeLength(7); yield return WalkTo(29.4f); if (done) yield break;
        Set(rope, "keyboardAimDirection", (TutorialSectionThreeSetup.RightBridgeEndpoint - body.position).normalized); yield return null;
        Check(rope.TryResolveCurrentAimHook(out var target, out _) && target != null, "S3 right green Hook aim");
        if (!Required(rope.TryAttach(TutorialSectionThreeSetup.RightBridgeEndpoint), "S3 E")) yield break;
        Pair("S3_Attached"); if (!Required(builder.TryBuildCurrentPlatform(), "S3 Q length7")) yield break;
        Pair("S3_Bridge"); move = 1; float until = Time.time + 10;
        while (run.CurrentTutorialSection < 4 && Time.time < until && !done) yield return null;
        move = 0; if (!Required(run.CurrentTutorialSection == 4, "S3 physical bridge walk reaches S4")) yield break;
        Call(run, "RestartFromCheckpoint"); yield return new WaitForSeconds(.9f);
        Check(builder.GeneratedPlatformCount == 1 && builder.HasPlatformBetween(TutorialSectionThreeSetup.LeftBridgeEndpoint, TutorialSectionThreeSetup.RightBridgeEndpoint, 7) && resource.CurrentLength == 92 && rope.SelectedRopeLength == 7, "S4 R preserves S3 bridge / rope92 / selected7");
        yield return LessonStart(4); if (done) yield break;
        yield return BuildMerge(); if (done) yield break;
        Call(run, "RestartFromCheckpoint"); yield return new WaitForSeconds(.9f);
        Check(builder.GeneratedPlatformCount == 1 && builder.CaptureRemovedHooks().Length == 0 && GameObject.Find(TutorialSectionFourSetup.CenterHookName) != null && resource.CurrentLength == 92, "S4 R restores pre-merge checkpoint / center / resource");
        Pair("S4_Restored"); yield return BuildMerge(); if (done) yield break;
        move = 1; float clearDeadline = Time.time + 20;
        while (run.Outcome == PrototypeRunController.RunOutcome.Playing && Time.time < clearDeadline && !done) yield return null;
        move = 0; if (!Required(run.Outcome == PrototypeRunController.RunOutcome.Clearing && !body.simulated, "Actual goal reached / Clearing physics locked")) yield break;
        Check(FindFirstObjectByType<TutorialCraftRoomLayers>().Visible, "Clearing keeps continuous room backing");
        yield return new WaitForSecondsRealtime(1.6f);
        Check(run.Outcome == PrototypeRunController.RunOutcome.Clear && FindFirstObjectByType<TutorialCraftRoomLayers>().Visible, "Clear reached / background remains");
        Pair("S4_Clear"); Check(transitions == 3, "All three actual Tutorial transitions observed"); track = false;
        Performance(); yield return FramePerformance(); run.ReturnToStageSelectionFromClear(); yield return new WaitForSecondsRealtime(1.1f); Bind();
        Check(run.IsStageSelectionOpen && FindFirstObjectByType<TutorialCraftRoomLayers>().LegacyRestored(), "Clear -> Stage Selection / original background restored");
        SceneManager.LoadScene("Assets/Scenes/MainStage.unity"); yield return null; yield return null;
        Check(FindFirstObjectByType<TutorialCraftRoomLayers>() == null && FindFirstObjectByType<HimoHitoCraftRoomBackground>() != null, "MainStage original / no trial component");
        Finish();
    }
    void Finish()
    {
        if (done) return; done = true; move = 0; track = false; Application.logMessageReceived -= OnLog;
        Time.captureDeltaTime = 0;
        File.WriteAllText(Path.Combine(Output, "result.txt"), $"checks={checks}\nfailures={failures}\nruntimeErrors={errors}\neditorSearchStartupErrors={searchErrors}\nphysicalTransitions={transitions}\nmotionFrames={motionIndex}\nControlled cached-input/API route with real physics, NOT OS keyboard or Human approval. No body teleport on route. Diagnostic capture clock 1/60s; production physics timestep unchanged.\n");
        UnityEditor.EditorApplication.Exit(failures == 0 && errors == 0 ? 0 : 2);
    }
}
#endif
