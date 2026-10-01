using System;
using System.Collections;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using HimoHito;

// Diagnostic-only controller, installed ONLY in the separate QA copy.
// No production source seams; cached-input/physics checks are NOT OS keyboard tests.
[DefaultExecutionOrder(10001)]
public sealed class CleanFocused : MonoBehaviour
{
    public static string Output;
    PlayerMover mover; Rigidbody2D body; RopeController rope; RopePlatformBuilder builder;
    GameObject player; float move, deadline; int failures, tests, errors; bool finished;
    static object F(object o, string n) => o.GetType().GetField(n, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(o);
    static void S(object o, string n, object v) => o.GetType().GetField(n, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(o, v);
    static object C(object o, string n, params object[] a) => o.GetType().GetMethod(n, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).Invoke(o, a);
    void Log(string s) { File.AppendAllText(Path.Combine(Output, "checks.txt"), s + "\n"); Debug.Log("CLEAN_FOCUSED " + s); }
    void Check(bool ok, string s) { tests++; if (!ok) failures++; Log((ok ? "PASS " : "FAIL ") + s); }
    public void Begin() { Directory.CreateDirectory(Output); deadline = Time.realtimeSinceStartup + 180; Application.logMessageReceived += OnLog; StartCoroutine(Run()); }
    void Update() { if (!finished && Time.realtimeSinceStartup > deadline) { Check(false, "harness timeout"); Finish(); } }
    void LateUpdate() { if (mover != null && body.simulated) S(mover, "moveInput", move); }
    void OnLog(string message, string stack, LogType type)
    {
        if (type == LogType.Exception || type == LogType.Error) errors++;
        if (type == LogType.Warning || type == LogType.Exception || type == LogType.Error)
            File.AppendAllText(Path.Combine(Output, "runtime_messages.txt"), type + " " + message + "\n" + stack + "\n");
    }
    void Bind() { mover = FindFirstObjectByType<PlayerMover>(); player = mover.gameObject; body = player.GetComponent<Rigidbody2D>(); rope = player.GetComponent<RopeController>(); builder = player.GetComponent<RopePlatformBuilder>(); }
    IEnumerator Skip() { yield return new WaitForSecondsRealtime(.2f); var p = Camera.main.GetComponent<MainStagePreview>(); if (p != null && MainStagePreview.IsActive) C(p, "Advance", 0f, true); yield return new WaitForSeconds(.3f); }
    void Position(Vector2 p) { move = 0; rope.DetachAndRefund(false); body.simulated = true; body.position = p; body.transform.position = p; body.linearVelocity = Vector2.zero; S(mover, "previousPhysicsPosition", p); Physics2D.SyncTransforms(); Log("CONTROLLED_SETUP " + p); }
    void Capture(string name, Vector2? center = null, float? size = null)
    {
        var cam = Camera.main; var old = cam.transform.position; float oldSize = cam.orthographicSize;
        if (center.HasValue) cam.transform.position = new Vector3(center.Value.x, center.Value.y, old.z);
        if (size.HasValue) cam.orthographicSize = size.Value;
        var rt = new RenderTexture(1600, 900, 24); var tex = new Texture2D(1600, 900, TextureFormat.RGB24, false);
        var prior = cam.targetTexture; var active = RenderTexture.active; cam.targetTexture = rt; cam.Render(); RenderTexture.active = rt;
        tex.ReadPixels(new Rect(0, 0, 1600, 900), 0, 0); tex.Apply();
        // HEAD intentionally has no ImageConversion package. PPM needs no extra module.
        var pixels = tex.GetPixels32(); var rgb = new byte[1600 * 900 * 3];
        for (int y = 0; y < 900; y++) for (int x = 0; x < 1600; x++)
        { var p = pixels[(899 - y) * 1600 + x]; int k = (y * 1600 + x) * 3; rgb[k] = p.r; rgb[k + 1] = p.g; rgb[k + 2] = p.b; }
        using (var file = File.Create(Path.Combine(Output, name + ".ppm")))
        { var header = System.Text.Encoding.ASCII.GetBytes("P6\n1600 900\n255\n"); file.Write(header, 0, header.Length); file.Write(rgb, 0, rgb.Length); }
        cam.targetTexture = prior; RenderTexture.active = active; cam.transform.position = old; cam.orthographicSize = oldSize; Destroy(rt); Destroy(tex);
    }
    IEnumerator ClearInput(string label)
    {
        yield return new WaitForSecondsRealtime(.8f);
        var audio = player.GetComponent<PrototypeAudioFeedback>(); var foot = F(audio, "footstepAudioSource") as AudioSource;
        Vector2 position = body.position;
        Check(!body.simulated && mover.enabled && foot != null, label + " Clear physics stopped / audio source present");
        int playing = 0, dirty = 0;
        for (int i = 0; i < 20; i++)
        {
            // Inject stale A/D-equivalent cached movement then invoke unmodified guards.
            S(mover, "moveInput", i % 2 == 0 ? -1f : 1f); S(mover, "jumpBufferTimer", .15f); S(mover, "footstepTimer", -.1f);
            C(mover, "Update"); C(mover, "FixedUpdate");
            if (mover.MovementInput != 0 || (float)F(mover, "footstepTimer") != 0 || (float)F(mover, "jumpBufferTimer") != 0) dirty++;
            yield return new WaitForSecondsRealtime(.08f); if (foot != null && foot.isPlaying) playing++;
        }
        Check(dirty == 0, label + " 20 cached A/D trials clear movement/jump/footstep timers");
        Check(playing == 0, label + " footstep AudioSource silent in 20 trials");
        Check(!body.simulated && Vector2.Distance(position, body.position) < .001f, label + " remains locked");
        Log("NOT_OS_KEY_TEST " + label + " uses unmodified Update/FixedUpdate guards");
    }
    IEnumerator Walk(float x)
    {
        move = 1; float until = Time.time + 10;
        while (body.position.x < x && Time.time < until) yield return null;
        move = 0; yield return new WaitForSeconds(.15f); Check(body.position.x >= x - .1f, "S9 physical walk " + x + " actual=" + body.position);
    }
    void Sweep(string label)
    {
        var right = GameObject.Find(MainStageSectionNineSetup.RightAnchorName).GetComponent<HookPoint>();
        string path = Path.Combine(Output, "aim_" + label + ".csv"); File.WriteAllText(path, "angle,resolved,right_ray,center_ray,available,highlighted,currentAim_F\n");
        int rightRay = 0, selected = 0, stolen = 0, mismatch = 0;
        for (int i = 0; i <= 720; i++)
        {
            float angle = -100 + i * .25f; Vector2 dir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad)); S(rope, "keyboardAimDirection", dir);
            bool rayRight = false, rayCenter = false;
            foreach (var hit in Physics2D.RaycastAll(body.position, dir, Mathf.Min(14, rope.SelectedRopeLength + .5f)))
            { var h = hit.collider.GetComponentInParent<HookPoint>(); rayRight |= h == right; rayCenter |= h != null && h.name == MainStageSectionNineSetup.CenterHookName; }
            object[] args = { body.position + dir * 14, Vector2.zero, null, false }; bool found = (bool)C(rope, "TryResolveAttachmentPoint", args); var resolved = args[2] as HookPoint;
            object[] avail = { body.position + dir * 14, Vector2.zero, null }; bool available = (bool)C(rope, "TryResolveAvailableAttachment", avail);
            C(rope, "UpdateAimHookFeedback"); var highlighted = F(rope, "highlightedHook") as HookPoint;
            rope.TryResolveCurrentAimHook(out var current, out _); // F removal target; intentionally not an E-target assertion.
            if (rayRight) rightRay++; if (found && resolved == right) selected++;
            if (rayRight && rayCenter && resolved != null && resolved.name == MainStageSectionNineSetup.CenterHookName) stolen++;
            if (resolved == right && available && highlighted != right) mismatch++;
            File.AppendAllText(path, $"{angle:F2},{resolved?.name ?? "none"},{rayRight},{rayCenter},{available},{highlighted?.name ?? "none"},{current?.name ?? "none"}\n");
        }
        Log($"SWEEP {label} position={body.position} rightRay={rightRay} rightSelected={selected} centerStealsRight={stolen} mismatch={mismatch}");
        if (label == "left")
            Check(rightRay > 0 && selected == 0, "S9 out-of-reach left boundary matches approved baseline");
        else Check(rightRay > 0 && selected > 0 && stolen == 0 && mismatch == 0, "S9 right Hook resolver/highlight " + label);
    }
    IEnumerator Run()
    {
        yield return null; Bind(); var tutorial = player.GetComponent<PrototypeRunController>();
        // Entry loads the scene during EnteredPlayMode; Start may run this same frame.
        while (!tutorial.CanUseTitleMenu()) yield return null;
        Check(tutorial.IsStageSelectionOpen && !body.simulated, "Stage Selection runtime state / movement locked");
        tutorial.BeginGameFromTitle(); yield return Skip();
        Check(tutorial.Outcome == PrototypeRunController.RunOutcome.Playing, "Tutorial starts after menu frame guard");
        foreach (var sign in FindObjectsByType<TutorialSignInscription>(FindObjectsSortMode.None))
        {
            int section = (int)F(sign, "section"); var board = sign.GetComponent<SpriteRenderer>(); var texts = sign.GetComponentsInChildren<TextMesh>();
            Position(new Vector2(sign.transform.position.x + .8f, 1.1f)); yield return new WaitForSeconds(.3f); S(sign, "animationTime", 0f); C(sign, "AdvanceAnimation", 0f);
            Capture("sign_" + section + "_world"); Capture("sign_" + section + "_detail", board.bounds.center, board.bounds.size.y * .65f);
            Check(texts.Length > 0, "Tutorial sign keys present " + section);
            foreach (var text in texts) { Log($"SIGN_TEXT section={section} key={text.text} color={text.color}"); Check(((Color32)text.color).Equals((Color32)new Color(.27f, .10f, .055f, 1f)), "Tutorial dark ink maintained " + section + "/" + text.text); }
        }
#if UNITY_EDITOR
        string guide = File.ReadAllText(Path.Combine(Application.dataPath, "Scripts/TutorialSectionGuide.cs"));
        Check(guide.Contains("99 → 92") && !guide.Contains("99 → 93"), "Tutorial sign3 99→92 committed source");
#endif
        Check(Resources.Load<AudioClip>("Audio/YasashiiOdori") != null, "FULL_REPRO_DEPENDENCY BGM clip available");
        var music = FindFirstObjectByType<HimoHitoBackgroundMusic>(); var musicSource = music != null ? music.GetComponent<AudioSource>() : null;
        Check(musicSource != null && musicSource.clip != null && musicSource.loop && musicSource.isPlaying, "BGM AudioSource clip loaded / loop / playing");
        if (musicSource != null) Log($"BGM_RUNTIME clip={musicSource.clip?.name} samples={musicSource.clip?.samples} channels={musicSource.clip?.channels} frequency={musicSource.clip?.frequency} timeSamples={musicSource.timeSamples}");
        tutorial.TryReachTutorialSection(4, new Vector2(40, 1.1f), 99); Position(new Vector2(TutorialSectionFourSetup.GoalMarkerPosition.x - .3f, 1.4f)); yield return new WaitForSecondsRealtime(4.5f);
        Check(tutorial.Outcome == PrototypeRunController.RunOutcome.Clear, "Tutorial real GoalZone enters Clear"); Capture("tutorial_clear"); yield return ClearInput("Tutorial");
        SceneManager.LoadScene("Assets/Scenes/MainStage.unity"); yield return null; yield return null; Bind(); yield return Skip();
        MainStageCheckpoint cp = null; foreach (var c in FindObjectsByType<MainStageCheckpoint>(FindObjectsSortMode.None)) if ((int)F(c, "sectionNumber") == 9) cp = c;
        var floor = cp.GetComponent<Collider2D>(); var p = (Vector2)F(cp, "respawnPosition"); Position(new Vector2(p.x, floor.bounds.max.y + 1.2f)); yield return new WaitForSeconds(.8f);
        Check(player.GetComponent<MainStageRespawnOnFall>().CurrentSection == 9, "S9 real checkpoint captured");
        Position(MainStageSectionNineSetup.LeftAnchorPosition + new Vector2(-.65f, .73f)); yield return new WaitForSeconds(.3f); rope.RestoreSelectedRopeLength(6);
        Check(rope.TryAttach(MainStageSectionNineSetup.CenterHookPosition) && builder.TryBuildCurrentPlatform(), "S9 first bridge"); yield return new WaitForSeconds(.45f);
        yield return Walk(184.55f); Position(new Vector2(184.67f, -.51f)); Sweep("left");
        yield return Walk(184.85f); Position(new Vector2(184.90f, -.40f)); Sweep("near");
        yield return Walk(185.05f); Position(new Vector2(185.06f, -.33f)); Sweep("center");
        rope.DetachAndRefund(false); builder.ClearPlatforms(); Position(new Vector2(MainStageSectionTenSetup.GoalMarkerPosition.x - .3f, -.8f)); yield return new WaitForSecondsRealtime(3f);
        Check(FindFirstObjectByType<MainStageGoalZone>().IsClear, "Main real GoalZone enters Clear"); Capture("main_clear"); yield return ClearInput("MainStage"); Finish();
    }
    void Finish()
    {
        if (finished) return; finished = true; move = 0; Application.logMessageReceived -= OnLog;
        File.WriteAllText(Path.Combine(Output, "result.txt"), $"tests={tests} failures={failures} runtimeErrors={errors}\nUnmodified production source. Controlled diagnostic, NOT OS keyboard / human playthrough.\n");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.Exit(failures > 0 || errors > 0 ? 2 : 0);
#endif
    }
}
