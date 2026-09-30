using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace HimoHito
{
    public static class FixTrace
    {
        public static string Output, Test;
        public static GameObject Center, Player;
        static int sequence;
        public static object Field(object o,string n) => o==null?null:o.GetType().GetField(n,BindingFlags.NonPublic|BindingFlags.Instance)?.GetValue(o);
        public static void Set(object o,string n,object v) => o.GetType().GetField(n,BindingFlags.NonPublic|BindingFlags.Instance).SetValue(o,v);
        public static object Call(object o,string n,params object[] a) => o.GetType().GetMethod(n,BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance).Invoke(o,a);
        [Serializable] public class Platform { public Vector2 start,end; public float ropeLength; }
        [Serializable] public class State
        {
            public string test,scene,label,centerName;
            public int sequence,frame,section,generatedCount,removedCount,savedRemovedCount,selected,savedSelected,centerInstanceId;
            public float time,rope,savedRope;
            public bool centerSelf,centerHierarchy,centerValid;
            public Vector2 position;
            public List<Platform> platforms=new(), savedPlatforms=new();
            public List<int> removedIds=new(),savedRemovedIds=new();
        }
        public static void Record(string label,GameObject player=null)
        {
            if(Output==null) return;
            player=player??Player; if(player==null) return;
            var b=player.GetComponent<RopePlatformBuilder>();
            var m=player.GetComponent<MainStageRespawnOnFall>(); var t=player.GetComponent<PrototypeRunController>();
            object run=m!=null?(object)m:t;
            var r=player.GetComponent<RopeResource>(); var c=player.GetComponent<RopeController>();
            var s=new State { test=Test,scene=SceneManager.GetActiveScene().name,label=label,sequence=sequence++,frame=Time.frameCount,time=Time.time,
                section=m!=null?m.CurrentSection:t.CurrentTutorialSection,rope=r.CurrentLength,selected=c.SelectedRopeLength,
                savedRope=(float)(Field(run,"checkpointRopeLength")??-1f),savedSelected=(int)(Field(run,"checkpointSelectedRopeLength")??-1),
                centerName=Center!=null?Center.name:"UNASSIGNED",centerValid=Center!=null,centerInstanceId=Center!=null?Center.GetInstanceID():0,
                centerSelf=Center!=null&&Center.activeSelf,centerHierarchy=Center!=null&&Center.activeInHierarchy,
                position=player.GetComponent<Rigidbody2D>().position };
            foreach(GameObject p in (IEnumerable)Field(b,"generatedPlatforms")) if(p!=null) {
                var g=p.GetComponent<GeneratedRopePlatform>(); s.platforms.Add(new Platform {start=g.Start,end=g.End,ropeLength=g.RopeLength}); }
            if(Field(run,"checkpointPlatformStates") is RopePlatformBuilder.PlatformState[] saved) foreach(var p in saved) s.savedPlatforms.Add(new Platform {start=p.Start,end=p.End,ropeLength=p.RopeLength});
            foreach(GameObject h in (IEnumerable)Field(b,"removedHooks")) s.removedIds.Add(h!=null?h.GetInstanceID():0);
            if(Field(run,"checkpointRemovedHooks") is GameObject[] hooks) foreach(var h in hooks) s.savedRemovedIds.Add(h!=null?h.GetInstanceID():0);
            s.generatedCount=s.platforms.Count;s.removedCount=s.removedIds.Count;s.savedRemovedCount=s.savedRemovedIds.Count;
            File.AppendAllText(Path.Combine(Output,"state.jsonl"),JsonUtility.ToJson(s)+"\n"); Debug.Log("FIX_STATE "+JsonUtility.ToJson(s));
        }
    }
    [DefaultExecutionOrder(10000)]
    public sealed class FixDriver:MonoBehaviour
    {
        PlayerMover mover; Rigidbody2D body; RopeController rope; RopeResource resource; RopePlatformBuilder builder;
        PrototypeRunController tutorial; MainStageRespawnOnFall main; GameObject player;
        float move,deadline; bool done; Vector2 left,center,right;
        void LateUpdate() { if(mover!=null) FixTrace.Set(mover,"moveInput",move); }
        void Update() { if(!done&&deadline>0&&Time.realtimeSinceStartup>deadline) Fail("Harness deadline exceeded"); }
        public void Begin() { deadline=Time.realtimeSinceStartup+160; StartCoroutine(Run()); }
        bool Check(bool condition,string name)
        {
            Debug.Log("ASSERT "+(condition?"PASS ":"FAIL ")+name);
            File.AppendAllText(Path.Combine(FixTrace.Output,"assertions.txt"),(condition?"PASS ":"FAIL ")+name+"\n");
            if(!condition) Fail(name); return condition;
        }
        void Fail(string message)
        {
            if(done) return; done=true; move=0; FixTrace.Record("FAIL "+message);
            File.WriteAllText(Path.Combine(FixTrace.Output,"result.txt"),"FAIL "+message+"\n");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.Exit(2);
#endif
        }
        void Pass()
        {
            done=true;move=0;FixTrace.Record("TEST_COMPLETE_PASS");
            File.WriteAllText(Path.Combine(FixTrace.Output,"result.txt"),"PASS "+FixTrace.Test+"\n");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.Exit(0);
#endif
        }
        void Position(Vector2 p)
        {
            move=0;body.position=p;body.transform.position=p;body.linearVelocity=Vector2.zero;body.angularVelocity=0;
            FixTrace.Set(mover,"previousPhysicsPosition",p);Physics2D.SyncTransforms();
            FixTrace.Record("CONTROLLED_SETUP_POSITION");
        }
        IEnumerator WalkTo(float x,float limit=10)
        {
            move=1;float until=Time.time+limit;
            while(body.position.x<x&&Time.time<until&&!done) yield return null;
            move=0;
            if(body.position.x<x-.1f) { Fail("Walk target not reached x="+x+" actual="+body.position); yield break; }
            yield return new WaitForSeconds(.15f);
        }
        Dictionary<GameObject,bool> HookStates()
        {
            var result=new Dictionary<GameObject,bool>();
            foreach(var h in Resources.FindObjectsOfTypeAll<HookPoint>()) if(h.gameObject.scene==player.scene) result[h.gameObject]=h.gameObject.activeSelf;
            return result;
        }
        bool SameHooks(Dictionary<GameObject,bool> saved)
        {
            foreach(var pair in saved) if(pair.Key==null||pair.Key.activeSelf!=pair.Value) return false;
            return HookStates().Count==saved.Count;
        }
        bool SamePlatforms(RopePlatformBuilder.PlatformState[] saved)
        {
            var actual=builder.CapturePlatformStates(); if(actual.Length!=saved.Length) return false;
            for(int i=0;i<saved.Length;i++) if(Vector2.Distance(actual[i].Start,saved[i].Start)>.0001f ||Vector2.Distance(actual[i].End,saved[i].End)>.0001f ||Mathf.Abs(actual[i].RopeLength-saved[i].RopeLength)>.0001f) return false;
            return true;
        }
        IEnumerator Run()
        {
            yield return null;
            player=FindFirstObjectByType<PlayerMover>().gameObject;FixTrace.Player=player;
            mover=player.GetComponent<PlayerMover>();body=player.GetComponent<Rigidbody2D>();rope=player.GetComponent<RopeController>();resource=player.GetComponent<RopeResource>();builder=player.GetComponent<RopePlatformBuilder>();
            tutorial=player.GetComponent<PrototypeRunController>();main=player.GetComponent<MainStageRespawnOnFall>();
            if(tutorial!=null) tutorial.BeginGameFromTitle();
            yield return new WaitForSecondsRealtime(.2f);
            var preview=Camera.main.GetComponent<MainStagePreview>();if(preview!=null&&MainStagePreview.IsActive) FixTrace.Call(preview,"Advance",0f,true);
            yield return new WaitForSeconds(.25f);
            if(FixTrace.Test=="Tutorial") { yield return TutorialRun(); if(!done) Pass(); yield break; }
            left=MainStageSectionNineSetup.LeftAnchorPosition;center=MainStageSectionNineSetup.CenterHookPosition;right=MainStageSectionNineSetup.RightAnchorPosition;
            FixTrace.Center=GameObject.Find(MainStageSectionNineSetup.CenterHookName);
            MainStageCheckpoint entry=null;
            foreach(var c in FindObjectsByType<MainStageCheckpoint>(FindObjectsSortMode.None)) if((int)FixTrace.Field(c,"sectionNumber")==9) entry=c;
            if(!Check(entry!=null,"Authored checkpoint9 exists")) yield break;
            var floor=entry.GetComponent<Collider2D>(); var respawn=(Vector2)FixTrace.Field(entry,"respawnPosition");
            Position(new Vector2(respawn.x,floor.bounds.max.y+1.2f));yield return new WaitForSeconds(.8f);
            if(!Check(main.CurrentSection==9,"Authored physical landing captured section9")) yield break;
            if(FixTrace.Test=="Normal")
            {
                var hooks=HookStates();FixTrace.Record("NORMAL_CHECKPOINT_CAPTURED");
                resource.RestoreCurrentLength(31);rope.RestoreSelectedRopeLength(3);FixTrace.Record("ROPE_PERTURBED");
                FixTrace.Call(main,"RestartFromCheckpoint");
                if(!Check(SameHooks(hooks)&&builder.CaptureRemovedHooks().Length==0,"D normal all Hook references/active states unchanged")) yield break;
                if(!Check(builder.GeneratedPlatformCount==0&&resource.CurrentLength==50&&rope.SelectedRopeLength==7,"C normal rope50/selected7 and platform0 restored")) yield break;
                yield return new WaitForSeconds(.9f);Pass();yield break;
            }
            rope.RestoreSelectedRopeLength(6);Position(left+new Vector2(-.65f,.72f));yield return new WaitForSeconds(.2f);
            if(!Check(rope.TryAttach(center),"First E attach")) yield break;
            if(!Check(builder.TryBuildCurrentPlatform(),"First Q bridge")) yield break;
            yield return WalkTo(center.x-.65f);if(done) yield break;
            if(!Check(rope.TryAttach(right),"Second E attach")) yield break;
            if(!Check(builder.TryBuildCurrentPlatform(),"Second Q bridge")) yield break;
            FixTrace.Record("TWO_UNMERGED_BRIDGES");Screenshot("01_two_bridges");
            if(FixTrace.Test=="Merged")
            {
                FixTrace.Set(rope,"keyboardAimDirection",(center-body.position).normalized);
                if(!Check(builder.TryRemoveAimedHook(),"F merge")) yield break;
                yield return new WaitForSeconds(.8f);FixTrace.Record("MERGED_BEFORE_CHECKPOINT");Screenshot("02_merged_before_checkpoint");
                move=1;float until=Time.time+12;
                while(main.CurrentSection<10&&Time.time<until&&!done) yield return null;
                move=0;
            }
            else
            {
                // Unmerged bridges do not provide the under-beam route. Use an existing floor, never a fabricated snapshot.
                var goal=GameObject.Find(MainStageSectionNineSetup.GoalFloorName).GetComponent<Collider2D>();
                Position(new Vector2(193,goal.bounds.max.y+1.2f));yield return new WaitForSeconds(.8f);
            }
            if(!Check(main.CurrentSection==10,"Authored physical landing captured checkpoint10")) yield break;
            FixTrace.Record("CHECKPOINT_CAPTURED");Screenshot("03_checkpoint");
            var saved=(RopePlatformBuilder.PlatformState[])FixTrace.Field(main,"checkpointPlatformStates");
            var savedHooks=(GameObject[])FixTrace.Field(main,"checkpointRemovedHooks");
            bool merged=FixTrace.Test=="Merged";int count=merged?1:2;int removed=merged?1:0;
            var allHooks=HookStates();int centerId=FixTrace.Center.GetInstanceID();
            if(!Check(saved.Length==count&&savedHooks.Length==removed,"Snapshot platform/removed counts")) yield break;
            if(merged&&!Check(savedHooks[0]==FixTrace.Center,"Snapshot contains exact central GameObject reference")) yield break;
            // Prove snapshot array is detached from the live list by changing/restoring that list through R.
            for(int n=1;n<=(merged?3:1);n++)
            {
                resource.RestoreCurrentLength(24-n);rope.RestoreSelectedRopeLength(4);
                FixTrace.Record("RESTART_"+n+"_BEFORE_PERTURBED");
                FixTrace.Call(main,"RestartFromCheckpoint");FixTrace.Record("RESTART_"+n+"_AFTER");
                if(!Check(FixTrace.Center!=null&&FixTrace.Center.GetInstanceID()==centerId,"Restart"+n+" Hook identity valid/unchanged")) yield break;
                if(!Check(FixTrace.Center.activeSelf==!merged&&FixTrace.Center.activeInHierarchy==!merged,"Restart"+n+" center active contract")) yield break;
                if(!Check(SamePlatforms(saved)&&builder.GeneratedPlatformCount==count,"Restart"+n+" platform count/endpoints/length exact")) yield break;
                var currentHooks=builder.CaptureRemovedHooks();
                if(!Check(currentHooks.Length==removed&&(!merged||currentHooks[0]==savedHooks[0]),"Restart"+n+" removedHooks count/identity no growth")) yield break;
                if(!Check(resource.CurrentLength==38&&rope.SelectedRopeLength==6&&main.CurrentSection==10,"Restart"+n+" rope38/selected6/section10 restored")) yield break;
                if(!Check(SameHooks(allHooks),"Restart"+n+" all authored Hook active states unchanged")) yield break;
                yield return new WaitForSeconds(.9f);
            }
            FixTrace.Record("FINAL_RESTORE");Screenshot("04_after_restore");Pass();
        }
        IEnumerator SwingToNext(Vector2 hook,float releaseX,int nextSection)
        {
            if(!Check(rope.TryAttach(hook),"Tutorial swing E section"+(nextSection-1))) yield break;
            move=1;float until=Time.time+15;bool released=false;
            while(tutorial.CurrentTutorialSection<nextSection&&Time.time<until&&!done)
            {
                if(!released&&body.position.x>=releaseX&&body.linearVelocity.x>2&&body.linearVelocity.y>0) { rope.DetachAndRefund(true);released=true;FixTrace.Record("SWING_RELEASE"); }
                if(tutorial.Outcome!=PrototypeRunController.RunOutcome.Playing) { Fail("Tutorial swing outcome "+tutorial.Outcome+" position="+body.position);yield break; }
                yield return null;
            }
            move=0;
            Check(tutorial.CurrentTutorialSection==nextSection,"Tutorial physical section transition "+nextSection);
            yield return new WaitForSeconds(.2f);
        }
        IEnumerator TutorialRun()
        {
            // Normal authored route from start: no position setters and no fabricated checkpoint capture.
            left=TutorialSectionFourSetup.LeftAnchorPosition;center=TutorialSectionFourSetup.CenterHookPosition;right=TutorialSectionFourSetup.RightAnchorPosition;
            FixTrace.Center=GameObject.Find(TutorialSectionFourSetup.CenterHookName);
            if(!Check(tutorial.CurrentTutorialSection==1&&builder.GeneratedPlatformCount==0,"Tutorial normal start")) yield break;
            yield return WalkTo(-1.3f);if(done) yield break;
            yield return SwingToNext(TutorialSectionOneSetup.HookPosition,6.0f,2);if(done) yield break;
            FixTrace.Call(tutorial,"RestartFromCheckpoint");yield return new WaitForSeconds(.9f);
            if(!Check(tutorial.CurrentTutorialSection==2&&resource.CurrentLength==99&&rope.SelectedRopeLength==6,"Tutorial section2 checkpoint restart")) yield break;
            yield return WalkTo(14.7f);if(done) yield break;
            yield return SwingToNext(TutorialSectionTwoSetup.HookPosition,22.0f,3);if(done) yield break;
            rope.RestoreSelectedRopeLength(7);
            yield return WalkTo(29.4f);if(done) yield break;
            if(!Check(rope.TryAttach(TutorialSectionThreeSetup.RightBridgeEndpoint),"Tutorial T3 E attach")) yield break;
            if(!Check(builder.TryBuildCurrentPlatform(),"Tutorial T3 Q length7")) yield break;
            move=1;float until=Time.time+10;
            while(tutorial.CurrentTutorialSection<4&&Time.time<until&&!done) yield return null;
            move=0;
            if(!Check(tutorial.CurrentTutorialSection==4,"Tutorial physical section4 transition")) yield break;
            FixTrace.Record("TUTORIAL_T4_CHECKPOINT_CAPTURED");
            FixTrace.Call(tutorial,"RestartFromCheckpoint");yield return new WaitForSeconds(.9f);
            if(!Check(builder.GeneratedPlatformCount==1&&builder.HasPlatformBetween(TutorialSectionThreeSetup.LeftBridgeEndpoint,TutorialSectionThreeSetup.RightBridgeEndpoint,7)&&resource.CurrentLength==92&&rope.SelectedRopeLength==7,"Tutorial T4 preserves T3 bridge/rope92/selection7")) yield break;
            rope.RestoreSelectedRopeLength(6);
            yield return WalkTo(41.35f);if(done) yield break;
            if(!Check(rope.TryAttach(center)&&builder.TryBuildCurrentPlatform(),"Tutorial T4 first E/Q")) yield break;
            yield return WalkTo(center.x-.65f);if(done) yield break;
            if(!Check(rope.TryAttach(right)&&builder.TryBuildCurrentPlatform(),"Tutorial T4 second E/Q")) yield break;
            FixTrace.Set(rope,"keyboardAimDirection",(center-body.position).normalized);
            if(!Check(builder.TryRemoveAimedHook(),"Tutorial T4 F merge")) yield break;
            yield return new WaitForSeconds(.8f);FixTrace.Record("TUTORIAL_MERGED");Screenshot("01_tutorial_merged");
            if(!Check(builder.GeneratedPlatformCount==2&&!FixTrace.Center.activeSelf&&builder.CaptureRemovedHooks().Length==1&&resource.CurrentLength==80,"Tutorial T3 bridge plus mergedT4, centerinactive, rope80")) yield break;
            FixTrace.Call(tutorial,"RestartFromCheckpoint");FixTrace.Record("TUTORIAL_RESTART_AFTER_MERGE");
            if(!Check(tutorial.CurrentTutorialSection==4&&builder.GeneratedPlatformCount==1&&FixTrace.Center.activeSelf&&builder.CaptureRemovedHooks().Length==0&&resource.CurrentLength==92&&rope.SelectedRopeLength==7,"Tutorial restores authored pre-merge T4 checkpoint")) yield break;
            yield return new WaitForSeconds(.9f);Screenshot("02_tutorial_after_restart");
        }
        void Screenshot(string name)
        {
            var c=Camera.main;var pos=c.transform.position;var size=c.orthographicSize;
            c.transform.position=new Vector3(center.x,center.y-1,pos.z);c.orthographicSize=6;
            var rt=RenderTexture.GetTemporary(1600,900,24);var prior=RenderTexture.active;
            c.targetTexture=rt;RenderTexture.active=rt;c.Render();
            var image=new Texture2D(1600,900,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1600,900),0,0);image.Apply();
            File.WriteAllBytes(Path.Combine(FixTrace.Output,name+".png"),image.EncodeToPNG());Destroy(image);
            RenderTexture.active=prior;c.targetTexture=null;RenderTexture.ReleaseTemporary(rt);c.transform.position=pos;c.orthographicSize=size;
        }
    }
}
