using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.SceneManagement;
using HimoHito;

// Diagnostic ONLY. Direct API calls are not OS-key or full-playthrough evidence.
[DefaultExecutionOrder(10001)]
public sealed class FullSystems : MonoBehaviour
{
    public static string Output;
    GameObject player;
    PlayerMover mover;
    Rigidbody2D body;
    RopeController rope;
    RopeResource resource;
    RopePlatformBuilder builder;
    MainStageRespawnOnFall run;
    float move, deadline, nextSample;
    int fails, tests, errors;
    readonly List<float> frames = new();
    bool finished;
    static object F(object o,string n)=>o?.GetType().GetField(n,BindingFlags.Instance|BindingFlags.NonPublic)?.GetValue(o);
    static void S(object o,string n,object v)=>o.GetType().GetField(n,BindingFlags.Instance|BindingFlags.NonPublic).SetValue(o,v);
    static object C(object o,string n,params object[] a)=>o.GetType().GetMethod(n,BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public).Invoke(o,a);
    void Log(string text) { File.AppendAllText(Path.Combine(Output,"checks.txt"),text+"\n");Debug.Log("FULL_QA "+text); }
    bool Check(bool ok,string text) { tests++;if(!ok)fails++;Log((ok?"PASS ":"FAIL ")+text);return ok; }
    void Bind()
    {
        player=FindFirstObjectByType<PlayerMover>().gameObject;mover=player.GetComponent<PlayerMover>();body=player.GetComponent<Rigidbody2D>();
        rope=player.GetComponent<RopeController>();resource=player.GetComponent<RopeResource>();builder=player.GetComponent<RopePlatformBuilder>();run=player.GetComponent<MainStageRespawnOnFall>();
    }
    [Serializable] sealed class Snapshot
    {
        public string label,scene;public int frame,section,platforms,removed,hooks,colliders,bgm,audioSources,visibleSprites;
        public float time,current,maximum,active,alpha;public int selected;public bool grounded,attached,simulated,moverEnabled,ropeEnabled,failure,hazardBlocked,hazardTrigger;
        public Vector2 position,velocity;public Vector3 camera;
    }
    void Record(string label)
    {
        var hazard=FindFirstObjectByType<PlatformOccludedLightHazard>();
        var s=new Snapshot {label=label,scene=SceneManager.GetActiveScene().name,frame=Time.frameCount,time=Time.time,section=run!=null?run.CurrentSection:player.GetComponent<PrototypeRunController>()?.CurrentTutorialSection??0,
            current=resource.CurrentLength,maximum=resource.MaximumLength,selected=rope.SelectedRopeLength,active=rope.ActiveRopeLength,platforms=builder.GeneratedPlatformCount,removed=builder.CaptureRemovedHooks().Length,
            position=body.position,velocity=body.linearVelocity,grounded=mover.IsGrounded,attached=rope.IsAttached,simulated=body.simulated,moverEnabled=mover.enabled,ropeEnabled=rope.enabled,
            failure=run!=null&&run.IsFailureVisible,bgm=FindObjectsByType<HimoHitoBackgroundMusic>(FindObjectsSortMode.None).Length,audioSources=FindObjectsByType<AudioSource>(FindObjectsSortMode.None).Length,
            hooks=FindObjectsByType<HookPoint>(FindObjectsSortMode.None).Length,colliders=FindObjectsByType<Collider2D>(FindObjectsSortMode.None).Length,camera=Camera.main!=null?Camera.main.transform.position:Vector3.zero,
            hazardBlocked=hazard!=null&&hazard.IsBlocked,hazardTrigger=hazard!=null&&hazard.GetComponent<Collider2D>().enabled,alpha=hazard!=null?hazard.GetComponent<SpriteRenderer>().color.a:0};
        foreach(var r in player.GetComponentsInChildren<SpriteRenderer>())if(r.enabled&&r.color.a>.05f)s.visibleSprites++;
        File.AppendAllText(Path.Combine(Output,"systems.jsonl"),JsonUtility.ToJson(s)+"\n");
    }
    void LateUpdate() {if(mover!=null)S(mover,"moveInput",move);}
    void Update()
    {
        if(finished)return;frames.Add(Time.unscaledDeltaTime*1000);
        if(Time.realtimeSinceStartup>=nextSample)
        {
            nextSample=Time.realtimeSinceStartup+1;
            File.AppendAllText(Path.Combine(Output,"performance.csv"),$"{Time.frameCount},{Time.realtimeSinceStartup:F3},{Time.unscaledDeltaTime*1000:F3},{Profiler.GetTotalAllocatedMemoryLong()},{Profiler.GetMonoUsedSizeLong()},{GC.CollectionCount(0)}\n");
        }
        if(deadline>0&&Time.realtimeSinceStartup>deadline){Log("HARNESS_TIMEOUT partial only");Finish(3);}
    }
    void OnLog(string message,string stack,LogType type)
    {
        if(type==LogType.Exception||type==LogType.Error){errors++;File.AppendAllText(Path.Combine(Output,"runtime_errors.txt"),message+"\n"+stack+"\n");}
    }
    public void Begin()
    {
        Directory.CreateDirectory(Output);Application.logMessageReceived+=OnLog;deadline=Time.realtimeSinceStartup+300;
        File.WriteAllText(Path.Combine(Output,"performance.csv"),"frame,realtime,frame_ms,total_allocated_bytes,mono_used_bytes,gen0_collections\n");
        StartCoroutine(Routine());
    }
    void Finish(int code=0)
    {
        if(finished)return;finished=true;move=0;Application.logMessageReceived-=OnLog;
        frames.Sort();float sum=0;foreach(float f in frames)sum+=f;
        File.WriteAllText(Path.Combine(Output,"result.txt"),$"tests={tests} failures={fails} runtimeErrors={errors} frames={frames.Count}\nmean_ms={(frames.Count>0?sum/frames.Count:0):F3} p95_ms={(frames.Count>0?frames[(int)((frames.Count-1)*.95f)]:0):F3} max_ms={(frames.Count>0?frames[^1]:0):F3}\nDIAGNOSTIC_EDITOR_BATCH_TIMINGS_NOT_PLAYER_FPS\n");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.Exit(code!=0?code:fails>0||errors>0?2:0);
#endif
    }
    void Position(Vector2 p)
    {
        Log("CONTROLLED_SETUP position="+p);move=0;rope.DetachAndRefund(false);body.simulated=true;body.position=p;body.transform.position=p;body.linearVelocity=Vector2.zero;body.angularVelocity=0;S(mover,"previousPhysicsPosition",p);Physics2D.SyncTransforms();
    }
    IEnumerator SkipPreview()
    {
        yield return new WaitForSecondsRealtime(.2f);
        var preview=Camera.main.GetComponent<MainStagePreview>();if(MainStagePreview.IsActive&&preview!=null)C(preview,"Advance",0f,true);
        yield return new WaitForSeconds(.4f);
    }
    IEnumerator EnterSection(int section)
    {
        MainStageCheckpoint cp=null;foreach(var c in FindObjectsByType<MainStageCheckpoint>(FindObjectsSortMode.None))if((int)F(c,"sectionNumber")==section)cp=c;
        if(!Check(cp!=null,"checkpoint object exists "+section))yield break;
        var floor=cp.GetComponent<Collider2D>();var p=(Vector2)F(cp,"respawnPosition");Position(new Vector2(p.x,floor.bounds.max.y+1.2f));
        yield return new WaitForSeconds(.8f);Check(run.CurrentSection==section,"controlled physical top landing captures "+section);Record("CONTROLLED_ENTRY_"+section);
    }
    IEnumerator Retry(string label)
    {
        move=0;C(run,"RestartFromCheckpoint");yield return new WaitForSecondsRealtime(1.05f);Record(label);
        Check(body.simulated&&mover.enabled&&rope.enabled&&!run.IsFailureVisible,"retry playable "+label);
    }
    IEnumerator Walk(float x,float limit=15)
    {
        move=1;float until=Time.time+limit;
        while(body.position.x<x&&Time.time<until&&!run.IsFailureVisible)yield return null;
        move=0;Check(body.position.x>=x-.1f,"physical walk target "+x+" actual "+body.position);yield return new WaitForSeconds(.15f);
    }
    IEnumerator Bridge(Vector2 start,Vector2 end,int length,string label)
    {
        Position(start+new Vector2(-.65f,.73f));yield return new WaitForSeconds(.3f);rope.RestoreSelectedRopeLength(length);Record(label+"_BEFORE_E");float before=resource.CurrentLength;
        bool attach=rope.TryAttach(end);Check(attach,label+" E attach");Record(label+"_AFTER_E");Check(resource.CurrentLength==before,label+" E no permanent spend");
        bool built=attach&&builder.TryBuildCurrentPlatform();Check(built,label+" Q build");Record(label+"_AFTER_Q");Check(!built||Mathf.Abs(resource.CurrentLength-(before-length))<.001f,label+" Q exact resource cost");yield return new WaitForSeconds(.45f);
    }
    IEnumerator Routine()
    {
        yield return null;Bind();yield return SkipPreview();Record("MAIN_START");
        var inventory=new List<string>();foreach(var c in FindObjectsByType<MainStageCheckpoint>(FindObjectsSortMode.None))inventory.Add($"CP {(int)F(c,"sectionNumber")} {c.name} respawn={F(c,"respawnPosition")} bounds={c.GetComponent<Collider2D>().bounds}");
        foreach(var h in FindObjectsByType<HookPoint>(FindObjectsSortMode.None))inventory.Add($"HOOK {h.name} {h.transform.position}");
        File.WriteAllLines(Path.Combine(Output,"inventory.txt"),inventory);
        int baseHooks=FindObjectsByType<HookPoint>(FindObjectsSortMode.None).Length,baseColliders=FindObjectsByType<Collider2D>(FindObjectsSortMode.None).Length;
        float initial=resource.CurrentLength;int initialSelected=rope.SelectedRopeLength;
        for(int i=1;i<=5;i++)
        {
            Position(new Vector2(body.position.x,-10));yield return new WaitForSecondsRealtime(.9f);Record("FALL_"+i);
            Check(run.IsFallFailure&&!body.simulated,"fall failure locks "+i);
            yield return new WaitForSecondsRealtime(.4f);Check(run.IsFallFailure,"failure waits for user "+i);
            yield return Retry("FALL_R_"+i);Check(resource.CurrentLength==initial&&rope.SelectedRopeLength==initialSelected,"fall no resource drift "+i);
            Check(FindObjectsByType<HookPoint>(FindObjectsSortMode.None).Length==baseHooks&&FindObjectsByType<Collider2D>(FindObjectsSortMode.None).Length==baseColliders,"fall no Hook/collider growth "+i);
        }
        for(int i=1;i<=3;i++)
        {
            resource.RestoreCurrentLength(0);yield return new WaitForSeconds(.1f);Record("EXHAUSTED_"+i);Check(run.IsRopeExhausted,"exhaustion entered "+i);
            yield return Retry("EXHAUSTED_R_"+i);Check(resource.CurrentLength==initial,"exhaustion restores rope "+i);
        }
        yield return EnterSection(2);yield return EnterSection(3);
        var stairs=FindFirstObjectByType<MainStageSectionThreeRecoveryStairs>();var sw=FindFirstObjectByType<MainStageSectionThreeRecoverySwitch>();
        Position(new Vector2(45.6f,-5.3f));yield return new WaitForSeconds(.35f);Check(run.CurrentSection==3,"low switch floor does not advance checkpoint4");
        if(sw!=null)C(sw,"TryActivate");yield return new WaitForSeconds(.2f);Check(stairs!=null&&stairs.IsRevealed,"physical proximity switch reveals stairs via F handler API");
        Record("STAIRS_REVEALED");yield return Retry("STAIRS_R");Check(stairs!=null&&!stairs.IsRevealed,"return checkpoint hides recovery stairs");
        yield return EnterSection(4);
        yield return Bridge(MainStageSectionFourSetup.StartEdgePosition,MainStageSectionFourSetup.BridgeAnchorPosition,5,"S4_BRIDGE");
        yield return Walk(68.5f,8);Check(mover.IsGrounded,"S4 bridge supports walking");Record("S4_SUPPORT");
        float oldRope=resource.CurrentLength;
        rope.RestoreSelectedRopeLength(9);
        bool swing=rope.TryAttach(MainStageSectionFourSetup.FarHookPosition);Check(swing,"S4 bridge supports E to swing hook");
        if(swing){move=1;yield return new WaitForSeconds(.35f);move=0;Vector2 v=body.linearVelocity;rope.DetachAndRefund(true);Check(Vector2.Distance(v,body.linearVelocity)<.001f,"detach preserves instantaneous momentum");Check(resource.CurrentLength==oldRope,"detach no permanent spend");Record("S4_DETACH");}
        yield return Retry("S4_R");
        yield return EnterSection(5);
        var hazard=FindFirstObjectByType<PlatformOccludedLightHazard>();Check(hazard!=null&&!hazard.IsBlocked,"S5 hazard unblocked without bridge");Record("S5_NO_BRIDGE");
        yield return Bridge(MainStageSectionFiveSetup.BridgeStartHookPosition,MainStageSectionFiveSetup.BridgeEndHookPosition,6,"S5_SHADOW");
        yield return new WaitForSeconds(.5f);Check(hazard!=null&&hazard.IsBlocked,"S5 authored bridge occludes light");Record("S5_OCCLUDED");
        for(int i=1;i<=3;i++)
        {
            yield return Retry("S5_BRIDGE_R_"+i);Check(builder.GeneratedPlatformCount==0&&!hazard.IsBlocked,"S5 retry removes unsaved shadow bridge "+i);
            Check(GameObject.Find(MainStageSectionFiveSetup.LeftShelfName).activeInHierarchy&&GameObject.Find(MainStageSectionFiveSetup.RightShelfName).activeInHierarchy,"S5 rail shelves active "+i);
            yield return Bridge(MainStageSectionFiveSetup.BridgeStartHookPosition,MainStageSectionFiveSetup.BridgeEndHookPosition,6,"S5_REBUILD_"+i);
        }
        yield return Retry("S5_FINAL_R");yield return EnterSection(6);
        yield return Bridge(MainStageSectionSixSetup.UpperBridgeStartHookPosition,MainStageSectionSixSetup.UpperBridgeEndHookPosition,14,"S6_UPPER");
        yield return Walk(131.6f,15);Record("S6_UPPER_END");
        yield return Retry("S6_R");
        Position(MainStageSectionSixSetup.UpperBridgeStartHookPosition+new Vector2(-.7f,.7f));yield return new WaitForSeconds(.3f);rope.RestoreSelectedRopeLength(9);
        bool first=rope.TryAttach(MainStageSectionSixSetup.LowerHookAPosition);Check(first,"S6 lower initial grounded E");move=1;
        float chainUntil=Time.time+15;while(first&&(Vector2.Distance(body.position,MainStageSectionSixSetup.LowerHookBPosition)>8.9f||body.linearVelocity.y<=0||body.position.x<128)&&Time.time<chainUntil&&!run.IsFailureVisible)yield return null;move=0;
        if(first)
        {
            rope.DetachAndRefund(true);Record("S6_AIR_DETACH");Check(rope.IsAirChainReconnectOpen,"S6 air-chain window opens after manual detach");
            bool next=rope.TryAttach(MainStageSectionSixSetup.LowerHookBPosition);Check(next,"S6 immediate sequential air reattach API");Record("S6_AIR_REATTACH");
            rope.DetachAndRefund(true);yield return new WaitForSecondsRealtime(1.6f);Check(!rope.IsAirChainReconnectOpen,"S6 1.5sec window expires");Record("S6_WINDOW_EXPIRED");
        }
        yield return Retry("S6_AFTER_CHAIN_R");yield return EnterSection(7);
        yield return Bridge(MainStageSectionSevenSetup.BridgeStartHookPosition,MainStageSectionSevenSetup.BridgeEndHookPosition,4,"S7_BRIDGE");
        yield return Walk(159.7f,8);S(mover,"jumpBufferTimer",.15f);move=1;yield return new WaitForSeconds(.7f);move=0;Record("S7_JUMP");
        yield return Retry("S7_R");yield return EnterSection(9);
        Check(run.CurrentSection==9,"KNOWN CONTENT STRUCTURE physical progression7 ->9");
        var center=GameObject.Find(MainStageSectionNineSetup.CenterHookName);int centerId=center.GetInstanceID();
        yield return Bridge(MainStageSectionNineSetup.LeftAnchorPosition,MainStageSectionNineSetup.CenterHookPosition,6,"S9_LEFT");
        yield return Walk(MainStageSectionNineSetup.CenterHookPosition.x-.65f,10);
        float beforeSecond=resource.CurrentLength;rope.RestoreSelectedRopeLength(6);Check(rope.TryAttach(MainStageSectionNineSetup.RightAnchorPosition),"S9 second E");Check(builder.TryBuildCurrentPlatform(),"S9 second Q");Record("S9_TWO_BRIDGES");
        S(rope,"keyboardAimDirection",(MainStageSectionNineSetup.CenterHookPosition-body.position).normalized);float beforeMerge=resource.CurrentLength;Check(builder.TryRemoveAimedHook(),"S9 F merge");yield return new WaitForSeconds(.8f);Check(resource.CurrentLength==beforeMerge,"F no extra spend");Record("S9_MERGED");
        move=1;float until=Time.time+12;while(run.CurrentSection<10&&!run.IsFailureVisible&&Time.time<until)yield return null;move=0;
        bool captured=Check(run.CurrentSection==10,"S9 merged bridge actual physical checkpoint10");Record("S9_CP10");
        if(captured)
        {
            var states=builder.CapturePlatformStates();float saved=resource.CurrentLength;int selected=rope.SelectedRopeLength;int expected=FindObjectsByType<Collider2D>(FindObjectsSortMode.None).Length;
            for(int i=1;i<=3;i++)
            {
                resource.RestoreCurrentLength(12);rope.RestoreSelectedRopeLength(3);yield return Retry("MERGED_R_"+i);
                var actual=builder.CapturePlatformStates();bool same=actual.Length==states.Length;
                for(int n=0;n<actual.Length&&n<states.Length;n++)same&=actual[n].Start==states[n].Start&&actual[n].End==states[n].End&&actual[n].RopeLength==states[n].RopeLength;
                Check(same&&actual.Length==1&&Mathf.Abs(actual[0].RopeLength-12)<.001f,"merged snapshot endpoints/length12 count1 R"+i);
                Check(center!=null&&center.GetInstanceID()==centerId&&!center.activeSelf&&builder.CaptureRemovedHooks().Length==1,"merged removed Hook identity inactive R"+i);
                Check(resource.CurrentLength==saved&&rope.SelectedRopeLength==selected,"merged resource checkpoint R"+i);
                Check(FindObjectsByType<Collider2D>(FindObjectsSortMode.None).Length==expected,"merged no collider growth R"+i);
            }
            rope.RestoreSelectedRopeLength(10);yield return Walk(198.35f,10);Record("S10_BEFORE_E");float r=resource.CurrentLength;
            Check(rope.TryAttach(MainStageSectionTenSetup.RightAnchorPosition)&&builder.TryBuildCurrentPlatform(),"S10 length10 bridge from real current position");Check(resource.CurrentLength==r-10,"S10 consumes10");Record("S10_BUILT");
            var goal=FindFirstObjectByType<MainStageGoalZone>();move=1;until=Time.time+20;while(!goal.IsCompleting&&!run.IsFailureVisible&&Time.time<until)yield return null;move=0;
            Check(goal.IsCompleting&&!body.simulated,"S10 actual chest clear starts / physics locked");Record("MAIN_CLEARING");yield return new WaitForSecondsRealtime(1.6f);
            Check(goal.IsClear,"Main clear state revealed");Check(StageProgress.IsCleared(StageCatalog.Entries[1]),"Main progress flag");Record("MAIN_CLEAR");
            if(goal.IsClear)
            {
                goal.ReturnToStageSelectionFromClear();yield return new WaitForSecondsRealtime(.7f);yield return null;yield return null;Bind();var t=player.GetComponent<PrototypeRunController>();
                Check(t!=null&&t.IsStageSelectionOpen,"Main clear return API reaches Stage Selection");Check(FindObjectsByType<HimoHitoBackgroundMusic>(FindObjectsSortMode.None).Length==1,"BGM singleton on clear return");
                if(t!=null)
                {
                    t.SelectStage(2);yield return null;Check(!t.StartSelectedStage()&&!string.IsNullOrEmpty(t.StageSelectionError),"upcoming stage refuses launch with message");
                    t.SelectStage(1);Check(string.IsNullOrEmpty(t.StageSelectionError),"new selection clears error");t.OpenMenuHelp();yield return null;
                    var overlay=FindFirstObjectByType<StageOverlayControls>();Check(overlay.IsHelpVisible&&Time.timeScale==0,"menu help pauses");overlay.CloseHelp();yield return null;yield return null;Check(Time.timeScale==1,"help close resumes");
                    Check(t.StartSelectedStage(),"menu main reselect transition starts");until=Time.realtimeSinceStartup+10;
                    while(StageStartTransition.IsActive&&Time.realtimeSinceStartup<until)yield return null;
                    Bind();yield return SkipPreview();Check(run!=null&&run.CurrentSection==1&&resource.CurrentLength==50&&builder.GeneratedPlatformCount==0,"fresh MainStage after clear and reselect");
                    Check(FindObjectsByType<HimoHitoBackgroundMusic>(FindObjectsSortMode.None).Length==1,"BGM singleton after menu reselect");Record("FRESH_MAIN_RESELECT");
                }
            }
        }
        Finish();
    }
}
