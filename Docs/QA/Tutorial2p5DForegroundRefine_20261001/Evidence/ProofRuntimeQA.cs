#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using HimoHito;
using HimoHitoProof;

// Only the isolated editor diagnostic. NOT a human keyboard playthrough.
[DefaultExecutionOrder(10001)]
public sealed class ProofRuntimeQA : MonoBehaviour
{
    public static string Output;
    Rigidbody2D body; PlayerMover mover; RopeController rope; PrototypeRunController run;
    float move, deadline; bool finished; int tests, failures, errors, editorSearchErrors;
    static void Set(object o, string n, object value) => o.GetType().GetField(n, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(o, value);
    static object Call(object o, string n, params object[] args) => o.GetType().GetMethod(n, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Invoke(o, args);
    void Log(string text) { File.AppendAllText(Path.Combine(Output, "checks.txt"), text + "\n"); Debug.Log("PROOF_QA " + text); }
    void Check(bool pass, string text) { tests++; if (!pass) failures++; Log((pass ? "PASS " : "FAIL ") + text); }
    public void Begin() { deadline = Time.realtimeSinceStartup + 180; Application.logMessageReceived += OnLog; StartCoroutine(Run()); }
    void OnLog(string text, string stack, LogType type)
    {
        if (type == LogType.Error || type == LogType.Exception)
        {
            // Unity's initial empty search-index exception is an Editor-startup issue,
            // recorded separately, not silently counted as a gameplay exception.
            bool searchStartup = stack.Contains("UnityEditor.Search.SearchDatabase");
            if (searchStartup) editorSearchErrors++; else errors++;
            File.AppendAllText(Path.Combine(Output, searchStartup ? "editor_search_errors.txt" : "errors.txt"), text + "\n" + stack + "\n");
        }
    }
    void Update() { if (!finished && Time.realtimeSinceStartup > deadline) { Check(false, "Timeout"); Finish(); } }
    void LateUpdate() { if (mover != null && body.simulated) Set(mover, "moveInput", move); }
    void Bind() { mover = FindFirstObjectByType<PlayerMover>(); body = mover.GetComponent<Rigidbody2D>(); rope = mover.GetComponent<RopeController>(); run = mover.GetComponent<PrototypeRunController>(); }
    string State()
    {
        var result = new StringBuilder();
        result.Append(body.position.ToString("R")).Append(body.linearVelocity.ToString("R"))
            .Append(body.rotation).Append(body.angularVelocity).Append(body.simulated)
            .Append(rope.IsAttached).Append(rope.SelectedRopeLength).Append(run.CurrentTutorialSection)
            .Append(Camera.main.transform.position.ToString("R")).Append(Camera.main.orthographicSize);
        foreach (var c in FindObjectsByType<Collider2D>(FindObjectsSortMode.InstanceID))
            result.Append(UnityEditor.EditorJsonUtility.ToJson(c)).Append(c.transform.position.ToString("R")).Append(c.transform.lossyScale.ToString("R"));
        foreach (var j in FindObjectsByType<Joint2D>(FindObjectsSortMode.InstanceID)) result.Append(UnityEditor.EditorJsonUtility.ToJson(j));
        return result.ToString();
    }
    void Capture(string name)
    {
        const int w = 1600, h = 900;
        var cam = Camera.main; var rt = new RenderTexture(w, h, 24); var tex = new Texture2D(w, h, TextureFormat.RGB24, false);
        var oldTarget = cam.targetTexture; var oldActive = RenderTexture.active;
        cam.targetTexture = rt; cam.Render(); RenderTexture.active = rt;
        tex.ReadPixels(new Rect(0, 0, w, h), 0, 0); tex.Apply();
        var pixels = tex.GetPixels32(); var bytes = new byte[w * h * 3];
        for (int y = 0; y < h; y++) for (int x = 0; x < w; x++)
        { var p = pixels[(h - 1 - y) * w + x]; int k = (y * w + x) * 3; bytes[k] = p.r; bytes[k + 1] = p.g; bytes[k + 2] = p.b; }
        using (var file = File.Create(Path.Combine(Output, name + ".ppm")))
        { var header = Encoding.ASCII.GetBytes($"P6\n{w} {h}\n255\n"); file.Write(header, 0, header.Length); file.Write(bytes, 0, bytes.Length); }
        cam.targetTexture = oldTarget; RenderTexture.active = oldActive; Destroy(rt); Destroy(tex);
    }
    void Pair(string label)
    {
        var proof = Tutorial2p5DBackground.Current; string original = State();
        Tutorial2p5DBackground.RefineEnabled = false; proof.RefreshImmediate(); Capture(label + "_Before");
        Check(State() == original && proof.Visible && !proof.FloorStrip.enabled, label + " previous P1 / physics-camera unchanged");
        Tutorial2p5DBackground.RefineEnabled = true; proof.RefreshImmediate(); Capture(label + "_After");
        Check(State() == original && proof.Visible && proof.FloorStrip.enabled, label + " refine active / physics-camera unchanged");
    }
    void Frame(string label)
    {
        Capture(label);
        var p = Tutorial2p5DBackground.Current;
        string row = label + "," + Time.time.ToString("F4") + "," + body.position.x.ToString("F4") + "," + body.position.y.ToString("F4") + "," + Camera.main.transform.position.x.ToString("F4");
        foreach (var layer in p.Layers) row += "," + layer.transform.position.x.ToString("F4");
        File.AppendAllText(Path.Combine(Output, "motion.csv"), row + "\n");
    }
    IEnumerator Walk(float target)
    {
        move = Mathf.Sign(target - body.position.x); float limit = Time.time + 3;
        while ((target - body.position.x) * move > .08f && Time.time < limit) yield return null;
        move = 0; yield return new WaitForSeconds(.35f);
        Check(Mathf.Abs(body.position.x - target) < .6f && run.Outcome == PrototypeRunController.RunOutcome.Playing, "Physical walk to " + target + " actual=" + body.position);
    }
    void Performance()
    {
        var proof = Tutorial2p5DBackground.Current; var cam = Camera.main;
        var rt = new RenderTexture(1600, 900, 24); var prior = cam.targetTexture; cam.targetTexture = rt;
        File.WriteAllText(Path.Combine(Output, "render_timing.csv"), "mode,sample,render_cpu_ms\n");
        for (int n = 0; n < 40; n++)
        {
            Tutorial2p5DBackground.RefineEnabled = n % 2 == 1; proof.RefreshImmediate();
            var timer = System.Diagnostics.Stopwatch.StartNew(); cam.Render(); timer.Stop();
            if (n >= 10) File.AppendAllText(Path.Combine(Output, "render_timing.csv"), (n % 2 == 1 ? "proof" : "baseline") + "," + n + "," + timer.Elapsed.TotalMilliseconds.ToString("F4") + "\n");
        }
        cam.targetTexture = prior; Destroy(rt); Tutorial2p5DBackground.RefineEnabled = true; proof.RefreshImmediate();
        long bytes = 0;
        foreach (string name in new[] { "Far", "Mid", "Near", "NearRefined" })
            bytes += UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(Resources.Load<Sprite>("Proof/Tutorial2p5D/Tutorial2p5D_" + name).texture);
        Log("FOUR_LOADED_PROOF_TEXTURE_RUNTIME_BYTES " + bytes + " (comparison build retains both Near variants; not frame rate / GPU benchmark)");
    }
    IEnumerator Run()
    {
        File.WriteAllText(Path.Combine(Output, "motion.csv"), "frame,time,player_x,player_y,camera_x,far_x,mid_x,near_x\n");
        yield return null; Bind();
        while (!run.CanUseTitleMenu()) yield return null;
        Check(run.IsStageSelectionOpen && !body.simulated && !Tutorial2p5DBackground.Current.Visible, "Stage Selection unchanged / proof hidden");
        run.BeginGameFromTitle(); yield return new WaitForSecondsRealtime(.25f);
        var preview = Camera.main.GetComponent<MainStagePreview>();
        if (preview != null && MainStagePreview.IsActive) Call(preview, "Advance", 0f, true);
        yield return new WaitForSeconds(.8f);
        Check(run.Outcome == PrototypeRunController.RunOutcome.Playing && run.CurrentTutorialSection == 1, "Tutorial Section 1 starts");
        var proof = Tutorial2p5DBackground.Current;
        Check(proof.Visible && proof.Layers.Length == 3, "Three independent background sprite planes active");
        Check(proof.GetComponentsInChildren<Collider2D>().Length == 0 && proof.GetComponentsInChildren<Rigidbody2D>().Length == 0, "Proof owns no collider or rigidbody");
        Pair("01_Standing");
        Vector3 startCam = Camera.main.transform.position;
        float[] start = new float[3]; for (int i = 0; i < 3; i++) start[i] = proof.Layers[i].transform.position.x - startCam.x;
        yield return Walk(-1.45f);
        Pair("02_Right");
        float deltaCam = Camera.main.transform.position.x - startCam.x;
        Check(deltaCam > 1f, "Unmodified gameplay camera follows physical walk");
        for (int i = 0; i < 3; i++)
        {
            float actual = proof.Layers[i].transform.position.x - Camera.main.transform.position.x - start[i];
            Check(Mathf.Abs(actual + deltaCam * (1 - proof.EffectiveFollow(i))) < .03f, "Layer " + i + " independent parallax delta=" + actual);
            Check(Mathf.Abs(proof.Layers[i].transform.position.z - proof.Depths[i]) < .001f, "Layer " + i + " depth=" + proof.Depths[i]);
        }
        Check(Mathf.Abs(proof.FloorStrip.transform.position.x - proof.Layers[2].transform.position.x) < .001f, "Near-floor and props share horizontal phase");
        Check(proof.Layers[0].color == Color.white && proof.Layers[1].color == Color.white, "Far / wall / curtain / shelf tint unchanged");
        Vector2 hook = GameObject.Find(TutorialSectionOneSetup.HookName).transform.position;
        Set(rope, "keyboardAimDirection", (hook - body.position).normalized);
        yield return null; yield return new WaitForSeconds(.2f);
        Check(rope.TryResolveCurrentAimHook(out var resolved, out _) && resolved != null, "Hook aiming resolves existing target");
        Pair("03_Aim");
        float priorY = body.position.y; Set(mover, "jumpBufferTimer", .15f);
        yield return new WaitForSeconds(.16f);
        Check(body.position.y > priorY + .3f && !mover.IsGrounded, "Jump through unchanged PlayerMover physics");
        Pair("04_Jump");
        float groundedDeadline = Time.time + 3;
        while (!mover.IsGrounded && Time.time < groundedDeadline) yield return null;
        Check(mover.IsGrounded, "Normal jump lands before ground-only rope attachment");
        Check(rope.TryAttach(hook), "Existing RopeController Hook attach succeeds");
        yield return new WaitForSeconds(.2f); Check(rope.IsAttached, "Existing DistanceJoint rope remains attached"); Pair("05_Attached");
        move = -1;
        for (int n = 0; n < 12; n++) { yield return new WaitForSeconds(.1f); Frame("Motion_" + n.ToString("D3")); }
        move = 1;
        for (int n = 12; n < 30; n++) { yield return new WaitForSeconds(.1f); Frame("Motion_" + n.ToString("D3")); }
        move = 0; rope.DetachAndRefund(false);
        // Controlled scope checks, not claims of human completion or route traversal.
        run.TryReachTutorialSection(2, TutorialSectionOneSetup.LandingRespawnPosition, 8);
        proof.RefreshImmediate();
        Check(run.CurrentTutorialSection == 2 && proof.BaselineRestored(), "Section 2: old background fully restored");
        Capture("06_Section2_ScopeOnly");
        // Re-enter a fresh Tutorial to time the background renderer only.
        SceneManager.LoadScene("Assets/Scenes/Tutorial.unity"); yield return null; yield return null; Bind();
        while (!run.CanUseTitleMenu()) yield return null;
        run.BeginGameFromTitle(); yield return new WaitForSecondsRealtime(.25f);
        preview = Camera.main.GetComponent<MainStagePreview>();
        if (preview != null && MainStagePreview.IsActive) Call(preview, "Advance", 0f, true);
        yield return new WaitForSeconds(.5f); Performance();
        SceneManager.LoadScene("Assets/Scenes/MainStage.unity"); yield return null; yield return null;
        Check(FindFirstObjectByType<Tutorial2p5DBackground>() == null, "MainStage: no proof component installed");
        Check(FindFirstObjectByType<HimoHitoCraftRoomBackground>() != null, "MainStage: existing background remains");
        Finish();
    }
    void Finish()
    {
        if (finished) return; finished = true; move = 0; Application.logMessageReceived -= OnLog;
        File.WriteAllText(Path.Combine(Output, "result.txt"), $"tests={tests}\nfailures={failures}\nruntimeErrors={errors}\neditorSearchStartupErrors={editorSearchErrors}\nControlled cached-input diagnostic, NOT OS-keyboard or Human Review.\n");
        UnityEditor.EditorApplication.Exit(failures == 0 && errors == 0 ? 0 : 2);
    }
}
#endif
