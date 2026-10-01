using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using HimoHito;

// Diagnostic-only input seam in the isolated copy. NOT an OS keyboard test.
public static class HumanProbeInput
{
    public static float Move;
    public static bool Jump;
    public static int Reads, Footsteps;
    public static bool GetKey(KeyCode key) { Reads++; return key==KeyCode.A?Move<0:key==KeyCode.D&&Move>0; }
    public static bool GetButtonDown(string name) { Reads++;bool result=Jump;Jump=false;return result; }
}
public sealed class HumanProbe : MonoBehaviour
{
    public static string Output, Mode;
    PlayerMover mover; Rigidbody2D body; RopeController rope; RopePlatformBuilder builder;
    GameObject player; int failures; float deadline;
    static object F(object o,string n)=>o?.GetType().GetField(n,BindingFlags.Instance|BindingFlags.NonPublic)?.GetValue(o);
    static void S(object o,string n,object v)=>o.GetType().GetField(n,BindingFlags.Instance|BindingFlags.NonPublic).SetValue(o,v);
    static object C(object o,string n,params object[] a)=>o.GetType().GetMethod(n,BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public).Invoke(o,a);
    void Log(string s){File.AppendAllText(Path.Combine(Output,"checks.txt"),s+"\n");Debug.Log("HUMAN_FIX_PROBE "+s);}
    void Check(bool ok,string s){if(!ok)failures++;Log((ok?"PASS ":"FAIL ")+s);}
    public void Begin(){Directory.CreateDirectory(Output);deadline=Time.realtimeSinceStartup+120;Application.logMessageReceived+=OnLog;StartCoroutine(Run());}
    void Update(){if(Time.realtimeSinceStartup>deadline){Log("TIMEOUT");Finish();}}
    void OnLog(string message,string stack,LogType type){if(type==LogType.Exception||type==LogType.Error||type==LogType.Warning)File.AppendAllText(Path.Combine(Output,"runtime_messages.txt"),type+" "+message+"\n"+stack+"\n");}
    void Bind(){mover=FindFirstObjectByType<PlayerMover>();player=mover.gameObject;body=player.GetComponent<Rigidbody2D>();rope=player.GetComponent<RopeController>();builder=player.GetComponent<RopePlatformBuilder>();}
    IEnumerator Skip(){yield return new WaitForSecondsRealtime(.2f);var preview=Camera.main.GetComponent<MainStagePreview>();if(preview!=null&&MainStagePreview.IsActive)C(preview,"Advance",0f,true);yield return new WaitForSeconds(.3f);}
    void Position(Vector2 p){HumanProbeInput.Move=0;rope.DetachAndRefund(false);body.simulated=true;body.position=p;body.transform.position=p;body.linearVelocity=Vector2.zero;S(mover,"previousPhysicsPosition",p);Physics2D.SyncTransforms();Log("CONTROLLED_SETUP "+p);}
    void Capture(string name,Vector2? center=null,float? size=null)
    {
        var cam=Camera.main;Vector3 old=cam.transform.position;float oldSize=cam.orthographicSize;
        if(center.HasValue)cam.transform.position=new Vector3(center.Value.x,center.Value.y,old.z);
        if(size.HasValue)cam.orthographicSize=size.Value;
        var rt=new RenderTexture(1600,900,24);var tex=new Texture2D(1600,900,TextureFormat.RGB24,false);
        var prior=cam.targetTexture;var active=RenderTexture.active;cam.targetTexture=rt;cam.Render();RenderTexture.active=rt;
        tex.ReadPixels(new Rect(0,0,1600,900),0,0);tex.Apply();File.WriteAllBytes(Path.Combine(Output,name+".png"),tex.EncodeToPNG());
        cam.targetTexture=prior;RenderTexture.active=active;cam.transform.position=old;cam.orthographicSize=oldSize;Destroy(rt);Destroy(tex);
    }
    IEnumerator FootstepClear(string scene)
    {
        HumanProbeInput.Move=0;int before=HumanProbeInput.Footsteps;int reads=HumanProbeInput.Reads;
        Vector2 position=body.position;bool simulated=body.simulated;bool enabled=mover.enabled;
        for(int i=0;i<20;i++)
        {
            HumanProbeInput.Move=i%2==0?-1:1;HumanProbeInput.Jump=true;
            yield return new WaitForSecondsRealtime(.30f);
            Log($"CLEAR_INPUT {scene} trial={i+1} key={(i%2==0?"A":"D")} simulated={body.simulated} move={mover.MovementInput} footstepDelta={HumanProbeInput.Footsteps-before} jumpBuffer={F(mover,"jumpBufferTimer")}");
            HumanProbeInput.Move=0;yield return new WaitForSecondsRealtime(.10f);
        }
        int count=HumanProbeInput.Footsteps-before,readCount=HumanProbeInput.Reads-reads;
        Log($"CLEAR_RESULT {scene} trials=20 actualAudioPlayOneShotCalls={count} inputReads={readCount} simulated={simulated} moverEnabled={enabled} positionDelta={Vector2.Distance(position,body.position)}");
        Check(count==0,scene+" clear footstep zero");Check(readCount==0,scene+" clear skips movement/jump input reads");
        Check(!body.simulated&&Vector2.Distance(position,body.position)<.001f,scene+" clear remains locked");
    }
    IEnumerator Walk(float x)
    {
        HumanProbeInput.Move=1;float until=Time.time+12;
        while(body.position.x<x&&Time.time<until)yield return null;
        HumanProbeInput.Move=0;yield return new WaitForSeconds(.15f);
        Check(body.position.x>=x-.1f,"physical walk target="+x+" position="+body.position);
    }
    void Sweep(string label)
    {
        if(Mode=="After")
        {
            // Replay exact positions physically reached in BEFORE, avoiding frame-rate drift.
            string baseline=Path.Combine(Directory.GetParent(Output).FullName,"Before","angles_"+label+".csv");
            var row=File.ReadAllLines(baseline)[1].Split(',');
            var position=new Vector2(float.Parse(row[1],System.Globalization.CultureInfo.InvariantCulture),float.Parse(row[2],System.Globalization.CultureInfo.InvariantCulture));
            body.position=position;body.transform.position=position;body.linearVelocity=Vector2.zero;S(mover,"previousPhysicsPosition",position);Physics2D.SyncTransforms();Log("BEFORE_POSITION_REPLAY "+position);
        }
        var right=GameObject.Find(MainStageSectionNineSetup.RightAnchorName).GetComponent<HookPoint>();
        string path=Path.Combine(Output,"angles_"+label+".csv");
        File.WriteAllText(path,"angle,x,y,selected,right_distance,resolved,available,highlight,currentAim,ordered_hits\n");
        int rightHits=0,rightSelected=0,centerSelected=0,competition=0;
        float min=999,max=-999;float centerMin=999,centerMax=-999;float hitMin=999,hitMax=-999;
        for(int i=0;i<=720;i++)
        {
            float angle=-100+i*.25f;Vector2 dir=new Vector2(Mathf.Cos(angle*Mathf.Deg2Rad),Mathf.Sin(angle*Mathf.Deg2Rad));S(rope,"keyboardAimDirection",dir);
            var hits=Physics2D.RaycastAll(body.position,dir,Mathf.Min(14,rope.SelectedRopeLength+.5f));
            var list=new List<string>();bool rayRight=false,rayCenter=false;
            foreach(var hit in hits){var hook=hit.collider.GetComponentInParent<HookPoint>();list.Add(hit.collider.name+"@"+hit.distance.ToString("F4")+"/"+(hook!=null?hook.name:"none"));rayRight|=hook==right;rayCenter|=hook!=null&&hook.name==MainStageSectionNineSetup.CenterHookName;}
            if(rayRight){rightHits++;hitMin=Mathf.Min(hitMin,angle);hitMax=Mathf.Max(hitMax,angle);}
            object[] args={body.position+dir*14,Vector2.zero,null,false};bool found=(bool)C(rope,"TryResolveAttachmentPoint",args);
            var resolved=args[2] as HookPoint;object[] availableArgs={body.position+dir*14,Vector2.zero,null};bool available=(bool)C(rope,"TryResolveAvailableAttachment",availableArgs);
            C(rope,"UpdateAimHookFeedback");var highlighted=F(rope,"highlightedHook") as HookPoint;
            rope.TryResolveCurrentAimHook(out var current,out _);
            if(found&&resolved==right){rightSelected++;min=Mathf.Min(min,angle);max=Mathf.Max(max,angle);}
            if(resolved!=null&&resolved.name==MainStageSectionNineSetup.CenterHookName){centerSelected++;centerMin=Mathf.Min(centerMin,angle);centerMax=Mathf.Max(centerMax,angle);if(rayRight&&rayCenter)competition++;}
            File.AppendAllText(path,$"{angle:F2},{body.position.x:F5},{body.position.y:F5},{rope.SelectedRopeLength},{Vector2.Distance(body.position,MainStageSectionNineSetup.RightAnchorPosition):F5},{resolved?.name??"none"},{available},{highlighted?.name??"none"},{current?.name??"none"},\"{string.Join("|",list)}\"\n");
        }
        Log($"SWEEP {label} position={body.position} selected={rope.SelectedRopeLength} rightRaySamples={rightHits} rightRayRange=[{hitMin},{hitMax}] rightSelectedSamples={rightSelected} rightSelectedRange=[{min},{max}] centerSamples={centerSelected} centerRange=[{centerMin},{centerMax}] centerStealsRightSamples={competition}");
        S(rope,"keyboardAimDirection",(MainStageSectionNineSetup.RightAnchorPosition-body.position).normalized);C(rope,"UpdateAimHookFeedback");Capture("s9_"+label);
    }
    IEnumerator Run()
    {
        yield return null;Bind();var tutorial=player.GetComponent<PrototypeRunController>();tutorial.BeginGameFromTitle();yield return Skip();
        foreach(var sign in FindObjectsByType<TutorialSignInscription>(FindObjectsSortMode.None))
        {
            int section=(int)F(sign,"section");var board=sign.GetComponent<SpriteRenderer>();var texts=sign.GetComponentsInChildren<TextMesh>();
            S(sign,"animationTime",0f);C(sign,"AdvanceAnimation",0f);
            Log("SIGN "+section+" sprite="+board.sprite.name+" bounds="+board.bounds);
            foreach(var text in texts)Log($"SIGN_TEXT section={section} renderer={text.name} color={text.color} fontSize={text.fontSize} characterSize={text.characterSize} position={text.transform.position} scale={text.transform.lossyScale}");
            Position(new Vector2(sign.transform.position.x+.8f,1.1f));yield return new WaitForSeconds(.3f);
            S(sign,"animationTime",0f);C(sign,"AdvanceAnimation",0f);
            Capture("sign_"+section+"_world");Capture("sign_"+section+"_detail",board.bounds.center,board.bounds.size.y*.65f);
            Check(texts.Length>0,"sign key exists "+section);
        }
        if(Mode=="Signs"){Finish();yield break;}
        // Focused Clear reproduction: controlled final-floor landing uses the real GoalZone.
        tutorial.TryReachTutorialSection(4,new Vector2(40,1.1f),99);
        Position(new Vector2(TutorialSectionFourSetup.GoalMarkerPosition.x-.3f,1.4f));yield return new WaitForSecondsRealtime(4.5f);
        Check(tutorial.Outcome==PrototypeRunController.RunOutcome.Clear,"Tutorial physical goal enters Clear");
        Capture("tutorial_clear");yield return FootstepClear("Tutorial");
        SceneManager.LoadScene("Assets/Scenes/MainStage.unity");yield return null;yield return null;Bind();yield return Skip();
        int stepsBefore=HumanProbeInput.Footsteps;HumanProbeInput.Move=1;yield return new WaitForSeconds(.8f);HumanProbeInput.Move=0;
        Check(HumanProbeInput.Footsteps>stepsBefore,"normal MainStage actual footstep playback");Log("NORMAL_FOOTSTEPS "+(HumanProbeInput.Footsteps-stepsBefore));
        MainStageCheckpoint cp=null;foreach(var c in FindObjectsByType<MainStageCheckpoint>(FindObjectsSortMode.None))if((int)F(c,"sectionNumber")==9)cp=c;
        var floor=cp.GetComponent<Collider2D>();var respawn=(Vector2)F(cp,"respawnPosition");Position(new Vector2(respawn.x,floor.bounds.max.y+1.2f));yield return new WaitForSeconds(.8f);
        Check(player.GetComponent<MainStageRespawnOnFall>().CurrentSection==9,"physical checkpoint9");
        Position(MainStageSectionNineSetup.LeftAnchorPosition+new Vector2(-.65f,.73f));yield return new WaitForSeconds(.3f);rope.RestoreSelectedRopeLength(6);
        Check(rope.TryAttach(MainStageSectionNineSetup.CenterHookPosition)&&builder.TryBuildCurrentPlatform(),"S9 first bridge E/Q");yield return new WaitForSeconds(.45f);
        yield return Walk(184.55f);Sweep("left_of_center");yield return Walk(184.85f);Sweep("near_center");yield return Walk(185.05f);Sweep("at_center");
        // Separate Clear reproduction on authored final floor; no claim of a full run here.
        rope.DetachAndRefund(false);builder.ClearPlatforms();
        Position(new Vector2(MainStageSectionTenSetup.GoalMarkerPosition.x-.3f,-.8f));yield return new WaitForSecondsRealtime(3f);
        var goal=FindFirstObjectByType<MainStageGoalZone>();Check(goal.IsClear,"Main physical goal enters Clear");Capture("main_clear");yield return FootstepClear("MainStage");
        Finish();
    }
    void Finish(){deadline=float.PositiveInfinity;HumanProbeInput.Move=0;Application.logMessageReceived-=OnLog;File.WriteAllText(Path.Combine(Output,"result.txt"),$"failures={failures}\nDiagnostic input seam; NOT OS keypresses.\n");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.Exit(failures>0?2:0);
#endif
    }
}
