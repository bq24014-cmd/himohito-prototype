#if UNITY_EDITOR
using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using HimoHito;
using UnityEngine;
using UnityEngine.SceneManagement;

// Isolated Editor QA only. Entire type is excluded from standalone builds.
// The production terrain component exposes no comparison or QA API.
// Cached input/API automation uses existing physics and checkpoint APIs; it is
// not OS-keyboard or Human play. No body/camera fixture teleport is performed.
[DefaultExecutionOrder(10004)]
public sealed class TerrainProductionQA : MonoBehaviour
{
    public static string Output;
    PlayerMover mover; Rigidbody2D body; RopeController rope; RopeResource resource;
    RopePlatformBuilder builder; PrototypeRunController run; TutorialSectionGuide guide;
    TerrainProductionQaFacade visual; TutorialCraftRoomLayers layers;
    bool done, track, introTrack, introCommitted, introStart, introMiddle, introEnd, introPreviewSeen, introResumed;
    float move, deadline, nextTrace;
    int checks, failures, errors, searchErrors, transitions, captures, introFrames, introHidden, introDouble;
    int priorSection; Vector3 priorCamera; Vector3[] priorLayers;
    readonly string[] names = { TutorialSectionOneSetup.StartFloorName, TutorialSectionOneSetup.LandingFloorName,
        TutorialSectionTwoSetup.LandingFloorName, TutorialSectionThreeSetup.LandingFloorName, TutorialSectionFourSetup.GoalFloorName };
    static object Read(object o, string n) => o.GetType().GetField(n, BindingFlags.NonPublic | BindingFlags.Instance).GetValue(o);
    static void Set(object o, string n, object v) => o.GetType().GetField(n, BindingFlags.NonPublic | BindingFlags.Instance).SetValue(o, v);
    static object Call(object o, string n, params object[] args) => o.GetType().GetMethod(n, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Invoke(o, args);
    static string F(float v) => v.ToString("R", CultureInfo.InvariantCulture);
    static bool Drawn(Renderer r) => r != null && r.enabled && r.gameObject.activeInHierarchy && !r.forceRenderingOff;
    static int TerrainComponentCount() => FindObjectsByType<TutorialTerrainHybridVisual>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length;
    bool Playing => run != null && run.Outcome == PrototypeRunController.RunOutcome.Playing;
    void Log(string value) { File.AppendAllText(Path.Combine(Output, "checks.txt"), value + "\n"); Debug.Log("TERRAIN_PRODUCTION_QA " + value); }
    void Check(bool value, string label) { checks++; if (!value) failures++; Log((value ? "PASS " : "FAIL ") + label); }
    bool Required(bool value, string label) { Check(value, label); if (!value) Finish(); return value; }
    public void Begin()
    {
        Directory.CreateDirectory(Output); Application.runInBackground = true;
        Time.captureDeltaTime = 1f / 60f; deadline = Time.realtimeSinceStartup + 1200;
        Application.logMessageReceived += OnLog;
        File.WriteAllText(Path.Combine(Output, "captures.csv"), "label,frame,section,outcome,hybrid,player_x,player_y,grounded,attached,rope_selected,rope_remaining,bridges,cam_x,cam_y,cam_z,ortho,width,height\n");
        File.WriteAllText(Path.Combine(Output, "terrain_projection.csv"), "label,target,min_x,max_x,min_y,max_y,viewport_left,viewport_right,viewport_bottom,viewport_top\n");
        File.WriteAllText(Path.Combine(Output, "aim_resolution.csv"), "label,frame,expected_e_hook,raw_f_picker_found,raw_f_picker_hook,e_resolver_found,e_resolver_hook,feedback_available,feedback_hook,aim_x,aim_y,player_x,player_y,e_anchor_x,e_anchor_y,selected,remaining\n");
        File.WriteAllText(Path.Combine(Output, "route.csv"), "frame,time,section,outcome,player_x,player_y,velocity_x,velocity_y,grounded,attached,remaining,bridges,cam_x,cam_y,far_x,mid_x,near_x\n");
        File.WriteAllText(Path.Combine(Output, "intro.csv"), "frame,realtime,preview,transition,outcome,far,mid,near,legacy_hidden,body_simulated,mover_enabled,rope_enabled,cam_x,cam_y,ortho\n");
        StartCoroutine(Routine());
    }
    void OnLog(string value, string stack, LogType type)
    {
        if (type != LogType.Error && type != LogType.Exception) return;
        bool search = stack.Contains("UnityEditor.Search.SearchDatabase"); if (search) searchErrors++; else errors++;
        File.AppendAllText(Path.Combine(Output, search ? "editor_search_errors.txt" : "runtime_errors.txt"), value + "\n" + stack + "\n");
    }
    void Update() { if (!done && Time.realtimeSinceStartup > deadline) { Check(false, "1200 second QA timeout"); Finish(); } }
    void Bind()
    {
        mover = FindFirstObjectByType<PlayerMover>(); if (mover == null) return;
        body = mover.GetComponent<Rigidbody2D>(); rope = mover.GetComponent<RopeController>(); resource = mover.GetComponent<RopeResource>();
        builder = mover.GetComponent<RopePlatformBuilder>(); run = mover.GetComponent<PrototypeRunController>(); guide = mover.GetComponent<TutorialSectionGuide>();
        var component = FindFirstObjectByType<TutorialTerrainHybridVisual>();
        visual = component == null ? null : new TerrainProductionQaFacade(component); layers = FindFirstObjectByType<TutorialCraftRoomLayers>();
    }
    void LateUpdate()
    {
        if (done || mover == null) return;
        Set(mover, "moveInput", move);
        if (introTrack && run != null && layers != null && layers.Layers != null)
        {
            bool committed = run.Outcome != PrototypeRunController.RunOutcome.WaitingToStart;
            if (committed)
            {
                introFrames++; if (!BackgroundVisible()) introHidden++;
                if (layers.Visible && !layers.LegacyHidden()) introDouble++;
            }
            var cam = Camera.main;
            File.AppendAllText(Path.Combine(Output, "intro.csv"), string.Join(",", Time.frameCount, F(Time.realtimeSinceStartup),
                MainStagePreview.IsActive, StageStartTransition.IsActive, run.Outcome, Drawn(layers.Layers[0]), Drawn(layers.Layers[1]), Drawn(layers.Layers[2]),
                layers.LegacyHidden(), body.simulated, mover.enabled, rope.enabled, F(cam.transform.position.x), F(cam.transform.position.y), F(cam.orthographicSize)) + "\n");
            // Observe after the production background's LateUpdate, so the
            // first committed frame uses its actual final rendered state.
            if (committed && !introCommitted) { introCommitted = true; Pair("Intro_Committed"); }
            if (MainStagePreview.IsActive)
            {
                introPreviewSeen = true; var preview = cam.GetComponent<MainStagePreview>();
                float elapsed = (float)Read(preview, "elapsed"), total = (float)Read(preview, "holdDuration") + (float)Read(preview, "travelDuration");
                if (!introStart && !StageStartTransition.IsActive) { introStart = true; Pair("Intro_Start"); }
                if (!introMiddle && elapsed >= total * .5f) { introMiddle = true; Pair("Intro_Middle"); }
                // Last fifth of the natural tour; no timing mutation or skip.
                if (!introEnd && elapsed >= total * .8f) { introEnd = true; Pair("Intro_NearEnd"); }
            }
            if (introCommitted && introPreviewSeen && !MainStagePreview.IsActive && !StageStartTransition.IsActive && !introResumed)
            { introResumed = true; Pair("Intro_Resumed"); }
        }
        if (!track || !BackgroundVisible()) return;
        var position = Camera.main.transform.position;
        if (priorLayers != null && run.CurrentTutorialSection != priorSection)
        {
            float dx = position.x - priorCamera.x; bool continuous = true;
            for (int i = 0; i < 3; i++) continuous &= Mathf.Abs(layers.Layers[i].transform.position.x - priorLayers[i].x - layers.Follow(i) * dx) < .02f;
            Check(continuous, "Section " + priorSection + " -> " + run.CurrentTutorialSection + " background parallax phase unchanged"); transitions++;
        }
        priorCamera = position; priorSection = run.CurrentTutorialSection; priorLayers ??= new Vector3[3];
        for (int i = 0; i < 3; i++) priorLayers[i] = layers.Layers[i].transform.position;
        if (Time.time < nextTrace) return; nextTrace = Time.time + .1f;
        File.AppendAllText(Path.Combine(Output, "route.csv"), string.Join(",", Time.frameCount, F(Time.time), run.CurrentTutorialSection, run.Outcome,
            F(body.position.x), F(body.position.y), F(body.linearVelocity.x), F(body.linearVelocity.y), mover.IsGrounded, rope.IsAttached, F(resource.CurrentLength),
            builder.GeneratedPlatformCount, F(position.x), F(position.y), F(priorLayers[0].x), F(priorLayers[1].x), F(priorLayers[2].x)) + "\n");
    }
    bool BackgroundVisible() => layers != null && layers.Visible && layers.LegacyHidden() && layers.Layers != null &&
        Drawn(layers.Layers[0]) && Drawn(layers.Layers[1]) && Drawn(layers.Layers[2]);
    bool IsTerrainVisual(Transform t)
    {
        foreach (string n in names)
        {
            // During lifecycle disposal the presentation deliberately clears
            // its references, but original scene floors still exist.
            var target = visual.GetTarget(n) ?? GameObject.Find(n); if (target == null) continue;
            if (t == target.transform) return true;
            if (!t.IsChildOf(target.transform)) continue;
            // Do not accidentally exempt signs/hooks/other children just
            // because a floor is their parent. Only the renderer allowlist.
            string child = t.name;
            return child == "Tutorial Hybrid Terrain Visual" || child == "Craft Wood Body" || child == "Wood Surface Shading" ||
                child == "Orange Block Platform Visual" || child == "Terrain Top Edge" || child.StartsWith("Orange Block Platform Tile ", StringComparison.Ordinal);
        }
        return false;
    }
    string PhysicsState()
    {
        var s = new StringBuilder();
        s.Append(body.position.ToString("R")).Append(body.linearVelocity.ToString("R")).Append(F(body.rotation)).Append(F(body.angularVelocity)).Append(body.simulated)
            .Append(mover.enabled).Append(mover.IsGrounded).Append(rope.enabled).Append(rope.IsAttached).Append(rope.SelectedRopeLength).Append(resource.CurrentLength)
            .Append(run.CurrentTutorialSection).Append(run.Outcome).Append(Time.timeScale).Append(F(Time.fixedDeltaTime));
        s.Append(UnityEditor.EditorJsonUtility.ToJson(rope));
        foreach (var camera in FindObjectsByType<Camera>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID))
            s.Append(UnityEditor.EditorJsonUtility.ToJson(camera)).Append(camera.transform.position.ToString("R")).Append(camera.transform.rotation.ToString("R"));
        foreach (var c in FindObjectsByType<Collider2D>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID))
            s.Append(UnityEditor.EditorJsonUtility.ToJson(c)).Append(c.gameObject.activeSelf).Append(c.transform.position.ToString("R")).Append(c.transform.rotation.ToString("R")).Append(c.transform.lossyScale.ToString("R"));
        foreach (var r in FindObjectsByType<Rigidbody2D>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID))
            s.Append(UnityEditor.EditorJsonUtility.ToJson(r)).Append(r.position.ToString("R")).Append(r.linearVelocity.ToString("R"));
        foreach (var j in FindObjectsByType<Joint2D>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID)) s.Append(UnityEditor.EditorJsonUtility.ToJson(j));
        foreach (var h in FindObjectsByType<HookPoint>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID))
            s.Append(UnityEditor.EditorJsonUtility.ToJson(h)).Append(h.transform.position.ToString("R")).Append(h.gameObject.activeSelf);
        foreach (var b in builder.CapturePlatformStates()) s.Append(b.Start.ToString("R")).Append(b.End.ToString("R")).Append(b.RopeLength);
        foreach (var h in builder.CaptureRemovedHooks()) s.Append(h.GetInstanceID()).Append(h.activeSelf);
        return s.ToString();
    }
    string ExcludedVisualState()
    {
        var s = new StringBuilder(); s.Append(layers.enabled).Append(layers.Visible).Append(layers.LegacyHidden());
        foreach (var r in FindObjectsByType<Renderer>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID))
        {
            if (IsTerrainVisual(r.transform)) continue;
            s.Append(r.GetInstanceID()).Append(r.name).Append(r.enabled).Append(r.forceRenderingOff).Append(r.gameObject.activeSelf)
                .Append(r.transform.position.ToString("R")).Append(r.transform.rotation.ToString("R")).Append(r.transform.lossyScale.ToString("R"))
                .Append(r.sortingLayerID).Append(r.sortingOrder).Append(r.sharedMaterial == null ? 0 : r.sharedMaterial.GetInstanceID());
            if (r is SpriteRenderer sprite) s.Append(sprite.sprite == null ? 0 : sprite.sprite.GetInstanceID()).Append(sprite.color).Append(sprite.flipX).Append(sprite.flipY);
        }
        return s.ToString();
    }
    string TargetPhysics(string n)
    {
        var t = visual.GetTarget(n); var s = new StringBuilder();
        s.Append(t.transform.position.ToString("R")).Append(t.transform.rotation.ToString("R")).Append(t.transform.localScale.ToString("R"));
        foreach (var c in t.GetComponentsInChildren<Collider2D>(true)) s.Append(UnityEditor.EditorJsonUtility.ToJson(c));
        foreach (var r in t.GetComponentsInChildren<Rigidbody2D>(true)) s.Append(UnityEditor.EditorJsonUtility.ToJson(r));
        return s.ToString();
    }
    void AuditTargets()
    {
        Check(visual.Ready && visual.TargetCount == 5, "Only five audited Tutorial normal-terrain targets prepared");
        float[] x = { -4, 12, 27, 39, 59.5f };
        var audit = new StringBuilder(); audit.AppendLine("TextureCropInfo=" + visual.TextureCropInfo);
        for (int i = 0; i < names.Length; i++)
        {
            var t = visual.GetTarget(names[i]); if (!Required(t != null, "Audited target exists: " + names[i])) return;
            var c = t.GetComponent<BoxCollider2D>(); var size = i == 4 ? new Vector3(15, 10, 1) : new Vector3(6, 10, 1);
            Check(t.name == names[i] && t.scene.name == "Tutorial" && Vector3.Distance(t.transform.position, new Vector3(x[i], -4.65f, 0)) < .001f &&
                Vector3.Distance(t.transform.localScale, size) < .001f && Quaternion.Angle(t.transform.rotation, Quaternion.identity) < .001f,
                names[i] + " authored Transform unchanged");
            Check(c != null && c.enabled && !c.isTrigger && c.size == Vector2.one && c.offset == Vector2.zero, names[i] + " authored Collider unchanged");
            Check(t.GetComponentsInChildren<Collider2D>(true).Length == 1 && t.GetComponentsInChildren<Rigidbody2D>(true).Length == 1 &&
                t.GetComponent<Rigidbody2D>().bodyType == RigidbodyType2D.Static, names[i] + " one original static body/Collider; no visual physics");
            audit.AppendLine("\nTarget=" + t.name).AppendLine("Position=" + t.transform.position.ToString("R")).AppendLine("Scale=" + t.transform.localScale.ToString("R"))
                .AppendLine("ColliderBounds=" + c.bounds).AppendLine("VisualBounds=" + visual.GetVisualBounds(names[i]));
            foreach (var component in t.GetComponents<Component>()) audit.AppendLine(component.GetType().FullName + " " + UnityEditor.EditorJsonUtility.ToJson(component));
            foreach (var r in t.GetComponentsInChildren<Renderer>(true)) audit.AppendLine("Renderer=" + r.name + "; enabled=" + r.enabled + "; forceOff=" + r.forceRenderingOff + "; bounds=" + r.bounds);
        }
        File.WriteAllText(Path.Combine(Output, "terrain_runtime_audit.txt"), audit.ToString());
    }
    void Capture(string label, int width = 1920, int height = 1080)
    {
        // World-camera render. Screen-space Overlay UI is not represented.
        var cam = Camera.main; var originalTarget = cam.targetTexture; var active = RenderTexture.active;
        var rt = new RenderTexture(width, height, 24); var tex = new Texture2D(width, height, TextureFormat.RGB24, false);
        try
        {
            cam.targetTexture = rt; cam.Render(); RenderTexture.active = rt; tex.ReadPixels(new Rect(0, 0, width, height), 0, 0); tex.Apply();
            // Record the actual RenderTexture projection, not an inferred
            // screen aspect. World coordinates stay authoritative evidence.
            foreach (string n in names)
            {
                var bounds = visual.GetTarget(n).GetComponent<BoxCollider2D>().bounds;
                float left = float.PositiveInfinity, right = float.NegativeInfinity, bottom = float.PositiveInfinity, top = float.NegativeInfinity;
                foreach (Vector3 corner in new[] {
                    new Vector3(bounds.min.x, bounds.min.y, bounds.center.z), new Vector3(bounds.max.x, bounds.min.y, bounds.center.z),
                    new Vector3(bounds.max.x, bounds.max.y, bounds.center.z), new Vector3(bounds.min.x, bounds.max.y, bounds.center.z) })
                {
                    Vector3 point = cam.WorldToViewportPoint(corner);
                    left = Mathf.Min(left, point.x); right = Mathf.Max(right, point.x); bottom = Mathf.Min(bottom, point.y); top = Mathf.Max(top, point.y);
                }
                File.AppendAllText(Path.Combine(Output, "terrain_projection.csv"), string.Join(",", label, n, F(bounds.min.x), F(bounds.max.x), F(bounds.min.y), F(bounds.max.y),
                    F(left), F(right), F(bottom), F(top)) + "\n");
            }
            var colors = tex.GetPixels32(); var bytes = new byte[width * height * 3];
            for (int y = 0; y < height; y++) for (int x = 0; x < width; x++)
            { var p = colors[(height - 1 - y) * width + x]; int k = (y * width + x) * 3; bytes[k] = p.r; bytes[k + 1] = p.g; bytes[k + 2] = p.b; }
            using (var file = File.Create(Path.Combine(Output, label + ".ppm")))
            { byte[] header = Encoding.ASCII.GetBytes($"P6\n{width} {height}\n255\n"); file.Write(header, 0, header.Length); file.Write(bytes, 0, bytes.Length); }
        }
        finally { cam.targetTexture = originalTarget; RenderTexture.active = active; Destroy(rt); Destroy(tex); }
        var pcam = cam.transform.position; captures++;
        File.AppendAllText(Path.Combine(Output, "captures.csv"), string.Join(",", label, Time.frameCount, run.CurrentTutorialSection, run.Outcome, visual.HybridEnabled,
            F(body.position.x), F(body.position.y), mover.IsGrounded, rope.IsAttached, rope.SelectedRopeLength, F(resource.CurrentLength), builder.GeneratedPlatformCount,
            F(pcam.x), F(pcam.y), F(pcam.z), F(cam.orthographicSize), width, height) + "\n");
    }
    void Pair(string label)
    {
        string state = PhysicsState(), excluded = ExcludedVisualState(); bool previous = visual.HybridEnabled;
        var targets = new string[names.Length]; for (int i = 0; i < names.Length; i++) targets[i] = TargetPhysics(names[i]);
        foreach (bool on in new[] { false, true })
        {
            visual.SetHybrid(on);
            Check(state == PhysicsState(), label + " " + on + " preserves all Collider/RB/Joint/Hook/Rope/Camera/run/physics state");
            Check(excluded == ExcludedVisualState(), label + " " + on + " preserves background/Far/Mid/Near/shader/prop/sign/hook/bridge/player renderers");
            bool intact = true; for (int i = 0; i < names.Length; i++) intact &= targets[i] == TargetPhysics(names[i]);
            Check(intact, label + " " + on + " five original terrain Transform/Collider/RB identities unchanged");
            if (on) foreach (string n in names)
            {
                Bounds c = visual.GetTarget(n).GetComponent<BoxCollider2D>().bounds, v = visual.GetVisualBounds(n);
                Check(Mathf.Abs(v.max.y - c.max.y) < .015f && Mathf.Abs(v.min.y - c.min.y) < .02f &&
                    Mathf.Abs(v.min.x - c.min.x) < .02f && Mathf.Abs(v.max.x - c.max.x) < .02f, label + " " + n + " visual bounds/walk line exactly match authored Collider");
            }
            Capture(label + (on ? "_After" : "_Before"));
        }
        visual.SetHybrid(previous);
        Check(state == PhysicsState() && excluded == ExcludedVisualState(), label + " restores synchronous gameplay/background state");
        if (run.Outcome != PrototypeRunController.RunOutcome.WaitingToStart) Check(BackgroundVisible(), label + " adopted 2.5D background continuously visible");
    }
    IEnumerator NaturalIntro()
    {
        while (!done && !run.CanUseTitleMenu()) yield return null;
        Check(run.IsStageSelectionOpen && !layers.Visible && layers.LegacyRestored(), "Stage Selection retains adopted pre-start background contract");
        Pair("Intro_Menu"); run.SelectStage(0); introTrack = true;
        if (!Required(run.StartSelectedStage(), "Existing Tutorial StartSelectedStage curtain route accepted")) yield break;
        float until = Time.realtimeSinceStartup + 40;
        while (!done && !introResumed && Time.realtimeSinceStartup < until) yield return null;
        introTrack = false;
        if (!Required(introCommitted && introStart && introMiddle && introEnd && introPreviewSeen && introResumed && Playing && body.simulated && mover.enabled && rope.enabled,
            "Natural unskipped Intro completes all milestones and restores gameplay controls")) yield break;
        Check(introFrames > 0 && introHidden == 0 && introDouble == 0, "Intro Fix preserved: no hidden adopted background or legacy/new double draw after commit");
        yield return new WaitForSeconds(.8f); track = true;
    }
    IEnumerator FallRetry()
    {
        float available = resource.CurrentLength; int selected = rope.SelectedRopeLength; move = -1;
        float until = Time.time + 8;
        while (!done && Playing && body.position.x > -7.8f && Time.time < until) yield return null;
        move = 0; Set(mover, "moveInput", 0f);
        while (!done && Playing && Time.time < until) yield return null;
        if (!Required(run.Outcome == PrototypeRunController.RunOutcome.Failed && run.FailureReason == PrototypeRunController.RunFailureReason.Fell && !body.simulated,
            "Physical S1 floor-edge fall invokes existing failure path without fixture teleport")) yield break;
        Pair("S1_Failed");
        float recoverUntil = Time.realtimeSinceStartup + 8;
        while (!done && run.IsFallUnravelling && Time.realtimeSinceStartup < recoverUntil) yield return null;
        if (!Required(!run.IsFallUnravelling, "Existing fall-unravel effect finishes before normal R retry")) yield break;
        Call(run, "RestartFromCheckpoint"); yield return new WaitForSeconds(.9f);
        if (!Required(Playing && run.CurrentTutorialSection == 1 && mover.enabled && rope.enabled && body.simulated && mover.IsGrounded &&
            resource.CurrentLength == available && rope.SelectedRopeLength == selected && builder.GeneratedPlatformCount == 0,
            "Normal R failure retry restores S1 checkpoint/control/resource/bridges")) yield break;
        Pair("S1_Retry");
    }
    IEnumerator WalkTo(float x, float limit = 12, string walkCapture = null)
    {
        float start = body.position.x, until = Time.time + limit; bool captured = false;
        while (!done && Playing && Mathf.Abs(x - body.position.x) > .07f && Time.time < until)
        {
            move = Mathf.Clamp((x - body.position.x) * 2f, -1, 1);
            if (walkCapture != null && !captured && Mathf.Abs(body.position.x - start) > .12f && Mathf.Abs(body.linearVelocity.x) > .1f)
            { Pair(walkCapture); captured = true; }
            yield return null;
        }
        move = 0; Set(mover, "moveInput", 0f); yield return new WaitForSeconds(.2f);
        Required(Playing && Mathf.Abs(body.position.x - x) < .25f, "Physical Walk " + F(start) + " -> " + F(x) + "; actual=" + body.position);
        if (walkCapture != null) Check(captured, walkCapture + " image taken during actual horizontal movement");
    }
    IEnumerator Jump(string label)
    {
        if (!Required(mover.IsGrounded && !rope.IsAttached, label + " starts grounded/detached")) yield break;
        float y = body.position.y; Set(mover, "jumpBufferTimer", .15f); yield return new WaitForSeconds(.16f);
        Check(body.position.y > y + .3f && !mover.IsGrounded, label + " normal PlayerMover jump rises"); Pair(label);
        float until = Time.time + 3; while (!done && !mover.IsGrounded && Time.time < until) yield return null;
        Required(mover.IsGrounded && Mathf.Abs(body.position.y - y) < .08f, label + " lands on same authored upper face");
    }
    IEnumerator LessonStart(int section)
    {
        if (!Required(run.CurrentTutorialSection == section, "Section " + section + " reached through authored physical route")) yield break;
        float available = resource.CurrentLength; int selected = rope.SelectedRopeLength, bridges = builder.GeneratedPlatformCount;
        Call(run, "RestartFromCheckpoint"); yield return new WaitForSeconds(.9f);
        if (!Required(run.CurrentTutorialSection == section && Playing && body.simulated && mover.enabled && rope.enabled && resource.CurrentLength == available &&
            rope.SelectedRopeLength == selected && builder.GeneratedPlatformCount == bridges, "S" + section + " normal checkpoint R preserves resource/selection/bridge state")) yield break;
        Pair("S" + section + "_Standing"); float start = body.position.x;
        yield return WalkTo(start - .4f); if (done) yield break;
        yield return WalkTo(start + .4f, 12, "S" + section + "_Walk"); if (done) yield break;
        yield return WalkTo(start); if (done) yield break;
        yield return Jump("S" + section + "_Jump"); if (done) yield break;
        Call(guide, "OpenGuide", section); Vector3 camera = Camera.main.transform.position;
        Check(guide.IsVisible && Time.timeScale == 0 && !mover.enabled && !rope.enabled, "S" + section + " existing sign UI pause opens");
        yield return new WaitForSecondsRealtime(.12f);
        Check(BackgroundVisible() && Camera.main.transform.position == camera, "S" + section + " sign pause preserves adopted background/camera");
        Pair("S" + section + "_SignWorldOnly"); Call(guide, "CloseGuide");
        Check(!guide.IsVisible && Time.timeScale == 1 && mover.enabled && rope.enabled, "S" + section + " sign closes and resumes normally");
    }
    IEnumerator AimAttach(Vector2 hook, string expectedName, string label)
    {
        Set(rope, "keyboardAimDirection", (hook - body.position).normalized); yield return null;
        // TryResolveCurrentAimHook is the raw F-removal picker, NOT the E
        // candidate resolver. It intentionally still sees the S4 center after
        // the left bridge, while E/feedback skip that no-longer-attachable Hook.
        // Preserve both observations rather than weakening the expected E name.
        bool rawFound = rope.TryResolveCurrentAimHook(out var rawHook, out _);
        Vector2 keyboardTarget = body.position + rope.KeyboardAimDirection * (float)Read(rope, "maximumShotDistance");
        object[] resolverArgs = { keyboardTarget, Vector2.zero, null };
        bool candidateAvailable = (bool)Call(rope, "TryResolveAvailableAttachment", resolverArgs);
        Vector2 resolvedAnchor = (Vector2)resolverArgs[1]; var resolved = resolverArgs[2] as HookPoint;
        var highlighted = Read(rope, "highlightedHook") as HookPoint; bool feedback = (bool)Read(rope, "hasAvailableAimAnchor");
        File.AppendAllText(Path.Combine(Output, "aim_resolution.csv"), string.Join(",", label, Time.frameCount, expectedName, rawFound,
            rawHook == null ? "None" : rawHook.name, candidateAvailable, resolved == null ? "None" : resolved.name, feedback,
            highlighted == null ? "None" : highlighted.name, F(rope.KeyboardAimDirection.x), F(rope.KeyboardAimDirection.y), F(body.position.x), F(body.position.y),
            F(resolvedAnchor.x), F(resolvedAnchor.y), rope.SelectedRopeLength, F(resource.CurrentLength)) + "\n");
        Check(candidateAvailable && resolved != null && resolved.name == expectedName && Vector2.Distance(resolvedAnchor, hook) < .001f,
            label + " actual E candidate resolver selects intended Hook / anchor");
        Check(feedback && highlighted != null && highlighted.name == expectedName, label + " actual visual aim feedback highlights intended E Hook");
        if (label == "S4_Second" || label == "S4_Rebuilt_Second")
            Check(rawFound && rawHook != null && rawHook.name == TutorialSectionFourSetup.CenterHookName,
                label + " separate F picker correctly retains center Hook while E selects right endpoint");
        Pair(label + "_Aim"); float available = resource.CurrentLength; int length = rope.SelectedRopeLength;
        if (!Required(rope.TryAttach(keyboardTarget), label + " existing keyboard-direction E attach succeeds")) yield break;
        var joint = rope.GetComponent<DistanceJoint2D>();
        bool platform = rope.ActiveHookPoint != null && rope.ActiveHookPoint.GetComponent<RopePlatformAnchor>() != null;
        float distance = platform ? Mathf.Max(length, Vector2.Distance(body.position, rope.AnchorPoint)) : length;
        Check(rope.IsAttached && resource.CurrentLength == available && Mathf.Abs(rope.ActiveRopeLength - length) < .001f &&
            rope.ActiveHookPoint != null && rope.ActiveHookPoint.name == expectedName && Vector2.Distance(rope.AnchorPoint, hook) < .001f && joint != null &&
            joint.enabled && Vector2.Distance(joint.connectedAnchor, hook) < .001f && Mathf.Abs(joint.distance - distance) < .002f,
            label + " free E / selected rope / original Hook / DistanceJoint contract");
        Pair(label + "_Attached");
    }
    IEnumerator SwingToNext(Vector2 hook, string expectedName, float releaseX, int nextSection)
    {
        string label = "S" + (nextSection - 1); yield return AimAttach(hook, expectedName, label); if (done) yield break;
        move = 1; float until = Time.time + 15; bool released = false;
        while (!done && run.CurrentTutorialSection < nextSection && Time.time < until)
        {
            if (!released && body.position.x >= releaseX && body.linearVelocity.x > 2 && body.linearVelocity.y > 0)
            {
                Vector2 velocity = body.linearVelocity; float angular = body.angularVelocity, available = resource.CurrentLength;
                rope.DetachAndRefund(true); released = true;
                Check(!rope.IsAttached && resource.CurrentLength == available && (body.linearVelocity - velocity).sqrMagnitude < .000001f && body.angularVelocity == angular,
                    label + " E release preserves linear/angular momentum and rope resource");
                Pair(label + "_Released");
            }
            if (!Playing) { Required(false, label + " swing failed at " + body.position); yield break; }
            yield return null;
        }
        move = 0;
        if (!Required(run.CurrentTutorialSection == nextSection, label + " real swing/landing reaches checkpoint " + nextSection)) yield break;
        yield return new WaitForSeconds(.2f);
    }
    IEnumerator Build(Vector2 hook, string name, int length, string label)
    {
        rope.RestoreSelectedRopeLength(length); float available = resource.CurrentLength;
        yield return AimAttach(hook, name, label); if (done) yield break;
        if (!Required(builder.TryBuildCurrentPlatform(), label + " existing Q creates bridge")) yield break;
        Check(!rope.IsAttached && Mathf.Abs(resource.CurrentLength - (available - length)) < .001f, label + " Q consumes only selected rope length");
        yield return new WaitForSeconds(.2f); Pair(label + "_Bridge");
    }
    IEnumerator BuildMerge(string prefix)
    {
        yield return WalkTo(41.35f); if (done) yield break;
        yield return Build(TutorialSectionFourSetup.CenterHookPosition, TutorialSectionFourSetup.CenterHookName, 6, prefix + "_First"); if (done) yield break;
        yield return WalkTo(46.35f); if (done) yield break;
        yield return Build(TutorialSectionFourSetup.RightAnchorPosition, TutorialSectionFourSetup.RightAnchorName, 6, prefix + "_Second"); if (done) yield break;
        Pair(prefix + "_TwoBridges"); Set(rope, "keyboardAimDirection", (TutorialSectionFourSetup.CenterHookPosition - body.position).normalized);
        float available = resource.CurrentLength;
        if (!Required(builder.TryRemoveAimedHook(), prefix + " existing F merge succeeds")) yield break;
        yield return new WaitForSeconds(.8f);
        Check(resource.CurrentLength == available && builder.GeneratedPlatformCount == 2 && GameObject.Find(TutorialSectionFourSetup.CenterHookName) == null &&
            builder.HasPlatformBetween(TutorialSectionFourSetup.LeftAnchorPosition, TutorialSectionFourSetup.RightAnchorPosition), prefix + " merged bridge/center removal adds no resource cost");
        Pair(prefix + "_Merged");
    }
    IEnumerator WideLocal()
    {
        yield return WalkTo(53.5f); if (done) yield break;
        Check(mover.IsGrounded && run.CurrentTutorialSection == 4, "Wide normal floor reached through S4 merged bridge"); Pair("S4_WideStanding");
        yield return WalkTo(53.1f); if (done) yield break;
        yield return WalkTo(53.9f, 12, "S4_WideWalk"); if (done) yield break;
        yield return Jump("S4_WideJump"); if (done) yield break;
        rope.RestoreSelectedRopeLength(6);
        yield return AimAttach(TutorialSectionFourSetup.RightAnchorPosition, TutorialSectionFourSetup.RightAnchorName, "S4_Wide"); if (done) yield break;
        Vector2 velocity = body.linearVelocity; float available = resource.CurrentLength; rope.DetachAndRefund(true);
        Check(!rope.IsAttached && resource.CurrentLength == available && (body.linearVelocity - velocity).sqrMagnitude < .000001f, "Wide E release preserves momentum/resource");
    }
    string OriginalTerrainVisibility(GameObject[] targets)
    {
        var snapshot = new StringBuilder();
        foreach (var target in targets) foreach (var renderer in target.GetComponentsInChildren<Renderer>(true))
        {
            if (renderer.name == "Tutorial Hybrid Terrain Visual") continue;
            snapshot.Append(renderer.GetInstanceID()).Append(renderer.enabled).Append(renderer.forceRenderingOff).Append(renderer.gameObject.activeSelf);
        }
        return snapshot.ToString();
    }
    int OwnedChildren(GameObject[] targets, bool drawnOnly)
    {
        int count = 0;
        foreach (var target in targets) foreach (var renderer in target.GetComponentsInChildren<Renderer>(true))
            if (renderer.name == "Tutorial Hybrid Terrain Visual" && (!drawnOnly || Drawn(renderer))) count++;
        return count;
    }
    IEnumerator Lifecycle()
    {
        // Clear has already been reached; this is a presentation lifecycle
        // check, not an alternate route or a scene/save operation.
        var targets = new GameObject[names.Length]; for (int i = 0; i < names.Length; i++) targets[i] = visual.GetTarget(names[i]);
        visual.SetHybrid(false); string originalVisibility = OriginalTerrainVisibility(targets); visual.SetHybrid(true);
        string physics = PhysicsState(), excluded = ExcludedVisualState();
        Check(OwnedChildren(targets, true) == 5, "Lifecycle baseline: exactly five drawable Hybrid children");
        visual.enabled = false;
        Check(PhysicsState() == physics && ExcludedVisualState() == excluded, "Lifecycle disable synchronously preserves physics/camera/background/excluded renderers");
        Check(!visual.Ready && !visual.Instance.enabled && TerrainComponentCount() == 1 && visual.GetTarget(names[0]) == null,
            "Lifecycle disable clears private ready/owned references; one disabled production component remains");
        Check(OwnedChildren(targets, true) == 0 && OriginalTerrainVisibility(targets) == originalVisibility,
            "Lifecycle disable immediately hides owned renderers and restores original wood renderer flags");
        yield return null;
        Check(OwnedChildren(targets, false) == 0, "Lifecycle deferred disposal removes all five owned renderer children");
        physics = PhysicsState(); excluded = ExcludedVisualState(); visual.enabled = true;
        Check(PhysicsState() == physics && ExcludedVisualState() == excluded, "Lifecycle enable synchronously preserves physics/camera/background/excluded renderers");
        float until = Time.realtimeSinceStartup + 3;
        while (!done && !visual.Ready && Time.realtimeSinceStartup < until) yield return null;
        if (!Required(visual.Ready && visual.TargetCount == 5 && visual.HybridEnabled && visual.Instance.enabled && TerrainComponentCount() == 1,
            "Lifecycle re-enable prepares five targets with private ready and one production component")) yield break;
        Check(OwnedChildren(targets, false) == 5 && OwnedChildren(targets, true) == 5, "Lifecycle re-enable has exactly five owned renderers, no duplicates");
        bool originalPhysicsOnly = true;
        foreach (var target in targets) originalPhysicsOnly &= target.GetComponentsInChildren<Collider2D>(true).Length == 1 &&
            target.GetComponentsInChildren<Rigidbody2D>(true).Length == 1 && target.GetComponent<Rigidbody2D>().bodyType == RigidbodyType2D.Static;
        Check(originalPhysicsOnly, "Lifecycle re-enable adds no Collider/Rigidbody components");
        Pair("S4_LifecycleRestored");
    }
    IEnumerator Routine()
    {
        yield return null; yield return null; Bind();
        while (!done && (mover == null || visual == null || !visual.Ready || layers == null || layers.Layers == null)) { yield return null; Bind(); }
        if (done) yield break; visual.SetHybrid(true); AuditTargets(); if (done) yield break;
        var productionType = typeof(TutorialTerrainHybridVisual);
        const BindingFlags exposed = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
        bool noProofApi = true;
        foreach (string name in new[] { "Current", "Ready", "HybridEnabled", "TargetCount", "TextureCropInfo", "SetHybrid", "GetTarget", "GetVisualBounds" })
            noProofApi &= productionType.GetMember(name, exposed).Length == 0;
        Check(noProofApi && productionType.GetMethods(exposed).Length == 0 && productionType.GetProperties(exposed).Length == 0 && productionType.GetFields(exposed).Length == 0,
            "Production terrain class declares no public proof API/method/property/field");
        Check(TerrainComponentCount() == 1, "Tutorial has exactly one production terrain component");
        bool noOldReviewHelper = true;
        foreach (var behaviour in FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (behaviour != null && (behaviour.GetType().Name == "AllTerrainReview" || behaviour.GetType().Name == "ShapeVisualProof")) noOldReviewHelper = false;
        Check(noOldReviewHelper, "No former F7 review or shape-proof runtime helper is installed");
        Check(layers.GetComponentsInChildren<Collider2D>(true).Length == 0 && layers.GetComponentsInChildren<Rigidbody2D>(true).Length == 0,
            "Adopted 2.5D background remains visual-only");
        Check(layers.Layers[1].sharedMaterial.shader.name == "HimoHito/Tutorial Background Edge" &&
            layers.FloorTiles[1].sharedMaterial.shader.name == "HimoHito/Tutorial Background Floor", "Existing seam-fix shaders remain in use");
        yield return NaturalIntro(); if (done) yield break;
        yield return FallRetry(); if (done) yield break;
        yield return LessonStart(1); if (done) yield break;
        yield return WalkTo(-1.3f); if (done) yield break;
        yield return SwingToNext(TutorialSectionOneSetup.HookPosition, TutorialSectionOneSetup.HookName, 6, 2); if (done) yield break;
        yield return LessonStart(2); if (done) yield break;
        Check(resource.CurrentLength == 99 && rope.SelectedRopeLength == 6, "S2 checkpoint preserves 99 rope / selected 6");
        yield return WalkTo(14.7f); if (done) yield break;
        yield return SwingToNext(TutorialSectionTwoSetup.HookPosition, TutorialSectionTwoSetup.HookName, 22, 3); if (done) yield break;
        yield return LessonStart(3); if (done) yield break;
        yield return WalkTo(29.4f); if (done) yield break;
        yield return Build(TutorialSectionThreeSetup.RightBridgeEndpoint, TutorialSectionThreeSetup.BridgeEndHookName, 7, "S3"); if (done) yield break;
        yield return WalkTo(39f, 15, "S3_BridgeWalk"); if (done) yield break;
        if (!Required(run.CurrentTutorialSection == 4, "S3 actual bridge traversal reaches S4 checkpoint")) yield break;
        yield return LessonStart(4); if (done) yield break;
        Check(builder.GeneratedPlatformCount == 1 && builder.HasPlatformBetween(TutorialSectionThreeSetup.LeftBridgeEndpoint, TutorialSectionThreeSetup.RightBridgeEndpoint, 7) &&
            resource.CurrentLength == 92 && rope.SelectedRopeLength == 7, "S4 checkpoint R preserves S3 bridge / 92 resource / selected 7");
        yield return BuildMerge("S4"); if (done) yield break;
        Call(run, "RestartFromCheckpoint"); yield return new WaitForSeconds(.9f);
        Check(builder.GeneratedPlatformCount == 1 && builder.CaptureRemovedHooks().Length == 0 && GameObject.Find(TutorialSectionFourSetup.CenterHookName) != null &&
            resource.CurrentLength == 92 && rope.SelectedRopeLength == 7, "S4 post-merge R restores original checkpoint bridges/hooks/resource/selection");
        Pair("S4_Restored"); yield return BuildMerge("S4_Rebuilt"); if (done) yield break;
        yield return WideLocal(); if (done) yield break;
        Check(resource.CurrentLength == 80 && builder.GeneratedPlatformCount == 2, "Final route has only S3 bridge + merged S4 / rope 80");
        move = 1; float until = Time.time + 20;
        while (!done && Playing && Time.time < until) yield return null;
        move = 0; Set(mover, "moveInput", 0f);
        if (!Required(run.Outcome == PrototypeRunController.RunOutcome.Clearing && !body.simulated, "Physical goal arrival triggers original Clearing / physics lock")) yield break;
        Pair("S4_Clearing"); yield return new WaitForSecondsRealtime(1.6f);
        Check(run.Outcome == PrototypeRunController.RunOutcome.Clear && BackgroundVisible(), "Clear completed with unchanged adopted background"); Pair("S4_Clear");
        Check(transitions == 3, "All three real Tutorial section transitions observed"); track = false;
        yield return Lifecycle(); if (done) yield break;
        run.ReturnToStageSelectionFromClear(); yield return null; yield return null; yield return new WaitForSecondsRealtime(1.1f); Bind();
        Check(run.IsStageSelectionOpen && layers.LegacyRestored(), "Clear -> Stage Selection restores original background contract");
        SceneManager.LoadScene("Assets/Scenes/MainStage.unity"); yield return null; yield return null; yield return new WaitForSecondsRealtime(.5f);
        Check(TerrainComponentCount() == 0, "MainStage has no Hybrid manager / no production terrain application");
        Check(FindFirstObjectByType<TutorialCraftRoomLayers>() == null && FindFirstObjectByType<HimoHitoCraftRoomBackground>() != null, "MainStage original background / no Tutorial layer component");
        int hybridRenderers = 0; foreach (var r in FindObjectsByType<Renderer>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (r.name.IndexOf("Hybrid", StringComparison.OrdinalIgnoreCase) >= 0) hybridRenderers++;
        Check(hybridRenderers == 0, "MainStage contains zero Hybrid-named visual renderers; S6 proof not installed");
        Finish();
    }
    void Finish()
    {
        if (done) return; done = true; move = 0; track = introTrack = false;
        if (mover != null) Set(mover, "moveInput", 0f);
        Application.logMessageReceived -= OnLog; Time.captureDeltaTime = 0;
        File.WriteAllText(Path.Combine(Output, "result.txt"), "checks=" + checks + "\nfailures=" + failures + "\nruntimeErrors=" + errors +
            "\neditorSearchStartupErrors=" + searchErrors + "\nphysicalTransitions=" + transitions + "\nworldCameraCaptures=" + captures +
            "\nintroFrames=" + introFrames + "\nintroHiddenAfterCommit=" + introHidden + "\nintroDoubleDraw=" + introDouble +
            "\nControlled cached-input/API route with real forces/collisions; NOT OS keyboard/Human approval.\n" +
            "No fixture teleport or direct camera assignment; natural intro is not skipped. Existing R/checkpoint API performs its authored respawn.\n" +
            "Diagnostic capture clock 1/60 s; production fixedDeltaTime and physics unchanged. Screen-space Overlay UI is not included in world-camera images.\n" +
            "Before/After switches exist only in the isolated UNITY_EDITOR QA facade; production has no public proof API, F7, or renderer toggle.\n" +
            "MainStage read-only runtime negative check only; no S6 proof or reachability claim.\n");
        UnityEditor.EditorApplication.Exit(failures == 0 && errors == 0 ? 0 : 2);
    }
}
#endif
