using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace HimoHito
{
    // Diagnostic copy only. Records fields without calling snapshot methods.
    public static class ReproTrace
    {
        public static string Output;
        public static GameObject Center;
        public static GameObject Player;
        public static readonly List<State> States = new();
        public static object Field(object o, string name) => o == null ? null : o.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(o);
        public static void Set(object o, string name, object value) => o.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance).SetValue(o, value);
        public static object Call(object o, string name, params object[] args) => o.GetType().GetMethod(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance).Invoke(o, args);
        [Serializable] public class Platform { public Vector2 start, end; public float ropeLength; public int releaseCount, wrapsCount; }
        [Serializable] public class State
        {
            public int sequence, frame, section, generatedCount, removedHooksCount, checkpointSelected;
            public string scene, label, centerName;
            public float time, ropeCurrent, checkpointRope;
            public int selected;
            public bool centerSelf, centerHierarchy, attached;
            public Vector2 playerPosition;
            public List<Platform> platforms = new();
            public List<Platform> checkpointPlatforms = new();
            public List<string> removedHooks = new();
        }
        public static void Record(string label, GameObject player = null)
        {
            if (Output == null) return;
            player = player ?? Player;
            if (player == null) return;
            var builder = player.GetComponent<RopePlatformBuilder>();
            var main = player.GetComponent<MainStageRespawnOnFall>();
            var tutorial = player.GetComponent<PrototypeRunController>();
            object run = main != null ? (object)main : tutorial;
            var resource = player.GetComponent<RopeResource>();
            var rope = player.GetComponent<RopeController>();
            var s = new State { sequence=States.Count, frame=Time.frameCount, time=Time.time,
                scene=SceneManager.GetActiveScene().name, label=label,
                section=main != null ? main.CurrentSection : tutorial != null ? tutorial.CurrentTutorialSection : -1,
                centerName=Center != null ? Center.name : "UNASSIGNED", centerSelf=Center != null && Center.activeSelf,
                centerHierarchy=Center != null && Center.activeInHierarchy,
                ropeCurrent=resource.CurrentLength, selected=rope.SelectedRopeLength, attached=rope.IsAttached,
                playerPosition=player.GetComponent<Rigidbody2D>().position,
                checkpointRope=(float)(Field(run,"checkpointRopeLength") ?? -1f),
                checkpointSelected=(int)(Field(run,"checkpointSelectedRopeLength") ?? -1) };
            foreach (GameObject p in (IEnumerable)Field(builder,"generatedPlatforms"))
            {
                if (p == null) continue;
                var g=p.GetComponent<GeneratedRopePlatform>();
                var binding=p.GetComponent<RopeBridgeBindings>();
                s.platforms.Add(new Platform { start=g.Start,end=g.End,ropeLength=g.RopeLength,
                    releaseCount=(Field(binding,"releases") as Array)?.Length ?? 0,
                    wrapsCount=(Field(binding,"wraps") as Array)?.Length ?? 0 });
            }
            foreach (GameObject h in (IEnumerable)Field(builder,"removedHooks")) s.removedHooks.Add(h != null ? h.name : "NULL");
            s.generatedCount=s.platforms.Count; s.removedHooksCount=s.removedHooks.Count;
            if (Field(run,"checkpointPlatformStates") is RopePlatformBuilder.PlatformState[] saved)
                foreach (var p in saved) s.checkpointPlatforms.Add(new Platform { start=p.Start,end=p.End,ropeLength=p.RopeLength });
            States.Add(s);
            File.AppendAllText(Path.Combine(Output,"state.jsonl"),JsonUtility.ToJson(s)+"\n");
            Debug.Log("REPRO_STATE "+JsonUtility.ToJson(s));
        }
    }

    [DefaultExecutionOrder(10000)]
    public sealed class ReproDriver : MonoBehaviour
    {
        public float Move;
        PlayerMover mover;
        Rigidbody2D body;
        RopeController rope;
        RopePlatformBuilder builder;
        PrototypeRunController tutorial;
        MainStageRespawnOnFall main;
        string scene;
        Vector2 left, center, right;
        bool done;
        float deadline;
        void LateUpdate() { if (mover != null) ReproTrace.Set(mover,"moveInput",Move); }
        void Update()
        {
            if (!done && Time.realtimeSinceStartup > deadline && deadline > 0) Fail("Runtime deadline exceeded");
        }
        void Fail(string reason)
        {
            if (done) return;
            done=true; Move=0;
            ReproTrace.Record("FAILED: "+reason);
            File.AppendAllText(Path.Combine(ReproTrace.Output,"result.txt"),scene+" FAILED "+reason+"\n");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.Exit(2);
#endif
        }
        public void Begin()
        {
            scene=SceneManager.GetActiveScene().name;
            deadline=Time.realtimeSinceStartup+120;
            StartCoroutine(Run());
        }
        void Position(Vector2 pos)
        {
            // Diagnostic setup only; never used to create/capture the merged checkpoint.
            Move=0; body.position=pos; body.transform.position=pos;
            body.linearVelocity=Vector2.zero; body.angularVelocity=0;
            ReproTrace.Set(mover,"previousPhysicsPosition",pos);
            Physics2D.SyncTransforms();
        }
        IEnumerator Run()
        {
            yield return null;
            ReproTrace.Player=GameObject.Find(scene=="Tutorial" ? "Player" : "Main Player");
            if (ReproTrace.Player == null) ReproTrace.Player=FindFirstObjectByType<PlayerMover>().gameObject;
            var p=ReproTrace.Player;
            mover=p.GetComponent<PlayerMover>(); body=p.GetComponent<Rigidbody2D>(); rope=p.GetComponent<RopeController>(); builder=p.GetComponent<RopePlatformBuilder>();
            tutorial=p.GetComponent<PrototypeRunController>(); main=p.GetComponent<MainStageRespawnOnFall>();
            if (tutorial != null) tutorial.BeginGameFromTitle();
            yield return new WaitForSecondsRealtime(.2f);
            // Same existing skip action as Enter during the intro, no authored data edited.
            var preview=Camera.main.GetComponent<MainStagePreview>();
            if (preview != null && MainStagePreview.IsActive) ReproTrace.Call(preview,"Advance",0f,true);
            yield return new WaitForSeconds(.2f);
            if (scene=="Tutorial")
            {
                left=TutorialSectionFourSetup.LeftAnchorPosition; center=TutorialSectionFourSetup.CenterHookPosition; right=TutorialSectionFourSetup.RightAnchorPosition;
                ReproTrace.Center=GameObject.Find(TutorialSectionFourSetup.CenterHookName);
                foreach (var cp in FindObjectsByType<TutorialCheckpoint>(FindObjectsSortMode.None))
                    Debug.Log("AUTHORED_TUTORIAL_CHECKPOINT "+cp.name+" section="+cp.SectionNumber+" respawn="+cp.RespawnPosition);
                // Controlled setup: land on existing section floors in order; do not claim manual traversal.
                for(int n=2;n<=4;n++)
                {
                    TutorialCheckpoint target=null;
                    foreach (var cp in FindObjectsByType<TutorialCheckpoint>(FindObjectsSortMode.None)) if(cp.SectionNumber==n) target=cp;
                    if(target==null) { Fail("Missing authored tutorial checkpoint "+n); yield break; }
                    var floor=target.GetComponent<Collider2D>();
                    Position(new Vector2(target.RespawnPosition.x,floor.bounds.max.y+1.2f));
                    yield return new WaitForSeconds(.8f);
                    if(tutorial.CurrentTutorialSection!=n) { Fail("Authored tutorial landing did not capture "+n); yield break; }
                }
            }
            else
            {
                left=MainStageSectionNineSetup.LeftAnchorPosition; center=MainStageSectionNineSetup.CenterHookPosition; right=MainStageSectionNineSetup.RightAnchorPosition;
                ReproTrace.Center=GameObject.Find(MainStageSectionNineSetup.CenterHookName);
                MainStageCheckpoint entry=null;
                foreach(var cp in FindObjectsByType<MainStageCheckpoint>(FindObjectsSortMode.None))
                    if ((int)ReproTrace.Field(cp,"sectionNumber")==9) entry=cp;
                if(entry==null) { Fail("Missing authored main checkpoint 9"); yield break; }
                var floor=entry.GetComponent<Collider2D>();
                var respawn=(Vector2)ReproTrace.Field(entry,"respawnPosition");
                Position(new Vector2(respawn.x,floor.bounds.max.y+1.2f));
                yield return new WaitForSeconds(.8f);
                if(main.CurrentSection!=9) { Fail("Main section9 authored landing failed"); yield break; }
            }
            ReproTrace.Record("SETUP_COMPLETE: controlled relocation to authored entry checkpoint; earlier puzzles not traversed");
            rope.RestoreSelectedRopeLength(6);
            Position(left+new Vector2(-.65f,.72f));
            yield return new WaitForSeconds(.2f);
            ReproTrace.Record("BEFORE_MERGE_0_BRIDGES"); Screenshot("01_before_build");
            if(!rope.TryAttach(center)) { Fail("First E-equivalent attach failed"); yield break; }
            if(!builder.TryBuildCurrentPlatform()) { Fail("First Q-equivalent build failed"); yield break; }
            Move=1;
            float until=Time.time+8;
            while(body.position.x<center.x-.65f && Time.time<until) yield return null;
            Move=0;
            yield return new WaitForSeconds(.15f);
            ReproTrace.Record("BEFORE_SECOND_BUILD");
            if(body.position.x<center.x-1f) { Fail("Walk to center failed"); yield break; }
            if(!rope.TryAttach(right)) { Fail("Second E-equivalent attach failed"); yield break; }
            if(!builder.TryBuildCurrentPlatform()) { Fail("Second Q-equivalent build failed"); yield break; }
            ReproTrace.Set(rope,"keyboardAimDirection",(center-body.position).normalized);
            ReproTrace.Record("BEFORE_MERGE_2_BRIDGES"); Screenshot("02_before_merge");
            if(!builder.TryRemoveAimedHook()) { Fail("F-equivalent merge failed"); yield break; }
            ReproTrace.Record("MERGE_IMMEDIATE_BINDING");
            yield return new WaitForSeconds(.8f);
            ReproTrace.Record("REPRO_BEFORE_CHECKPOINT"); Screenshot("03_merged_before_checkpoint");
            if(scene=="Tutorial")
            {
                Debug.Log("NOT REPRODUCIBLE WITH AUTHORED TUTORIAL FLOW: checkpoints only 2,3,4; T4 captured before merge; goal has no later checkpoint.");
                ReproTrace.Record("RESTART_BEFORE");
                ReproTrace.Call(tutorial,"RestartFromCheckpoint");
                ReproTrace.Record("RESTART_COMPLETE");
                yield return new WaitForSeconds(1f);
                Screenshot("04_after_restart");
                File.AppendAllText(Path.Combine(ReproTrace.Output,"result.txt"),"Tutorial R3: no post-merge authored checkpoint; pre-merge restore expected.\n");
                done=true;
#if UNITY_EDITOR
                UnityEditor.EditorApplication.Exit(0);
#endif
            }
            else
            {
                // From merge onwards: walk through unmodified physics to the authored goal floor.
                Move=1; until=Time.time+12;
                while(main.CurrentSection<10 && Time.time<until) yield return null;
                Move=0;
                if(main.CurrentSection!=10) { Fail("Walk to authored checkpoint10 failed"); yield break; }
                ReproTrace.Record("CHECKPOINT_CAPTURE_OBSERVED"); Screenshot("05_checkpoint_captured");
                yield return new WaitForSeconds(.15f);
                ReproTrace.Record("RESTART_BEFORE");
                ReproTrace.Call(main,"RestartFromCheckpoint");
                ReproTrace.Record("RESTART_COMPLETE");
                yield return new WaitForSeconds(1f);
                Screenshot("06_after_restart");
                bool reproduced=builder.GeneratedPlatformCount==1 && ReproTrace.Center.activeSelf &&
                    builder.HasPlatformBetween(left,right,12f);
                File.AppendAllText(Path.Combine(ReproTrace.Output,"result.txt"),"MainStage "+(reproduced?"R1: merged bridge + center revived":"NOT R1")+"\n");
                done=true;
#if UNITY_EDITOR
                UnityEditor.EditorApplication.Exit(reproduced?0:3);
#endif
            }
        }
        void Screenshot(string label)
        {
            var camera=Camera.main;
            var follow=camera.GetComponent<HorizontalCameraFollow>();
            Vector3 old=camera.transform.position; float oldSize=camera.orthographicSize;
            camera.transform.position=new Vector3(center.x,center.y-1f,old.z); camera.orthographicSize=6f;
            var rt=RenderTexture.GetTemporary(1600,900,24); camera.targetTexture=rt;
            var previous=RenderTexture.active; RenderTexture.active=rt; camera.Render();
            var image=new Texture2D(1600,900,TextureFormat.RGB24,false);
            image.ReadPixels(new Rect(0,0,1600,900),0,0); image.Apply();
            File.WriteAllBytes(Path.Combine(ReproTrace.Output,scene+"_"+label+".png"),image.EncodeToPNG());
            Destroy(image); RenderTexture.active=previous; camera.targetTexture=null; RenderTexture.ReleaseTemporary(rt);
            camera.transform.position=old; camera.orthographicSize=oldSize;
        }
    }
}
