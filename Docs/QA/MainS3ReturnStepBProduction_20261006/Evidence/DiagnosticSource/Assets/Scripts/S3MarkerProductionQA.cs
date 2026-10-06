#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using HimoHito;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-1000)]
public sealed class S3MarkerProductionQA : MonoBehaviour
{
    public static string Output;
    public static Func<Texture2D,byte[]> EncodePng;
    [Serializable] sealed class CheckRow { public string name, status, detail; }
    [Serializable] sealed class Results { public string unity, method; public CheckRow[] checks; public string[] errors, editorSearchErrors; public int failures; }
    [Serializable] sealed class CameraRecord { public Vector3 position; public float ortho, aspect; public string follow; }
    readonly List<CheckRow> checks = new List<CheckRow>();
    readonly List<string> errors = new List<string>();
    readonly List<string> editorSearchErrors = new List<string>();
    string Dir => Output;
    PlayerMover mover;
    Rigidbody2D body;
    float input;
    bool jump;
    S3ReturnStepBMarkerProofVisual proof;
    MainS3MarkerVisual production;
    static readonly string[] Floors = {
        "Main Start Ground", "Main Landing 1", "Main Landing 2", "Main Landing 3",
        "Main Section 3 Low Dead End", "Main Section 3 Return Step A", "Main Section 3 Return Step B",
        "Main Section 3 Return Step C", "Main Section 3 Return Step D", "Main Section 4 Intermediate Column",
        "Main Section 4 Landing", "Main S05 Landing", "Main S06 Upper Left", "Main S06 Upper Right",
        "Main S06 Upper Final Step", "Main S06 Merge", "Main S07 Middle Shelf", "Main S07 Upper Step",
        "Main S07 Landing", "Main S09 Goal Floor", "Main S10 Goal Floor" };
    void OnEnable() { Application.logMessageReceived += Logged; }
    void OnDisable() { Application.logMessageReceived -= Logged; }
    void Logged(string message, string stack, LogType type)
    {
        if (type != LogType.Error && type != LogType.Exception && type != LogType.Assert) return;
        if (stack.Contains("UnityEditor.Search")) editorSearchErrors.Add(message+"\n"+stack);
        else errors.Add(message+"\n"+stack);
    }
    void FixedUpdate()
    {
        if (!mover) return;
        Set(mover, "moveInput", input);
        if (jump) { Set(mover, "jumpBufferTimer", .15f); jump = false; }
    }
    static void Set(object o,string field,object value) => o.GetType().GetField(field,BindingFlags.Instance|BindingFlags.NonPublic).SetValue(o,value);
    static object Invoke(object o,string method,params object[] args) => o.GetType().GetMethod(method,BindingFlags.Instance|BindingFlags.NonPublic).Invoke(o,args);
    static GameObject Find(string name) => Resources.FindObjectsOfTypeAll<GameObject>().FirstOrDefault(g=>g.scene.IsValid()&&g.scene.name=="MainStage"&&g.name==name);
    void Check(bool ok,string name,string detail="")
    {
        checks.Add(new CheckRow{name=name,status=ok?"PASS":"FAIL",detail=detail});
        File.WriteAllText(Path.Combine(Dir,"Progress.json"),JsonUtility.ToJson(new Results{checks=checks.ToArray(),errors=errors.ToArray(),editorSearchErrors=editorSearchErrors.ToArray()},true));
    }
    string StaticPhysics()
    {
        var rows = new List<string>();
        foreach(var b in Resources.FindObjectsOfTypeAll<Collider2D>().Where(b=>b.gameObject.scene.IsValid()).OrderBy(b=>b.gameObject.scene.name+"/"+PathOf(b.transform)))
        {
            if(b.attachedRigidbody && b.attachedRigidbody.bodyType!=RigidbodyType2D.Static) continue;
            var t=b.transform; var rb=b.attachedRigidbody;
            rows.Add(t.gameObject.scene.name+"/"+PathOf(t)+"|"+t.position.ToString("F6")+"|"+t.lossyScale.ToString("F6")+"|"+t.rotation.ToString("F6")+"|"+EditorJsonUtility.ToJson(b)+"|"+(rb?EditorJsonUtility.ToJson(rb):"noRB"));
        }
        return string.Join("\n",rows);
    }
    static string PathOf(Transform t)=>t.parent?PathOf(t.parent)+"/"+t.name:t.name;
    string CameraState(Camera camera) => JsonUtility.ToJson(new CameraRecord{position=camera.transform.position,ortho=camera.orthographicSize,aspect=camera.aspect,follow=EditorJsonUtility.ToJson(camera.GetComponent<HorizontalCameraFollow>())});
    void Capture(Camera source,string name,int width=1920,int height=1080,Vector3? position=null,float? ortho=null)
    {
        // A render-only camera has no follow/preview component. Main camera
        // transform, framing, targetTexture and gameplay behavior are untouched.
        var go=new GameObject("Temporary offscreen S3 proof capture");
        var cam=go.AddComponent<Camera>();cam.CopyFrom(source);cam.enabled=false;
        cam.transform.SetPositionAndRotation(position??source.transform.position,source.transform.rotation);
        if(ortho.HasValue)cam.orthographicSize=ortho.Value;
        cam.aspect=width/(float)height;
        var rt=new RenderTexture(width,height,24);var tex=new Texture2D(width,height,TextureFormat.RGB24,false);
        var previous=RenderTexture.active;
        try {
            cam.targetTexture=rt;cam.Render();RenderTexture.active=rt;
            tex.ReadPixels(new Rect(0,0,width,height),0,0);tex.Apply();
            File.WriteAllBytes(Path.Combine(Dir,name+".png"),EncodePng(tex));
            File.WriteAllText(Path.Combine(Dir,name+"_Framing.json"),JsonUtility.ToJson(new CameraRecord{
                position=cam.transform.position,ortho=cam.orthographicSize,aspect=cam.aspect,follow="Render-only clone; no follow behaviour"},true));
        } finally {RenderTexture.active=previous;cam.targetTexture=null;DestroyImmediate(tex);DestroyImmediate(rt);DestroyImmediate(go);}
    }
    void ComparePair(Camera camera,string prefix,Vector3? position=null,float? ortho=null,int width=1920,int height=1080)
    {
        string physics=StaticPhysics(), cameraState=CameraState(camera), otherVisuals=OtherVisualState();
        proof.RestoreLegacyForComparison();
        production.Surface.enabled=true;
        Capture(camera,prefix+"_Production",width,height,position,ortho);
        production.Surface.enabled=false;
        proof.SetCandidate(true);
        Capture(camera,prefix+"_ApprovedProof",width,height,position,ortho);
        proof.RestoreLegacyForComparison();
        production.Surface.enabled=true;
        Check(physics==StaticPhysics(),prefix+" physics invariant across diagnostic render switch");
        Check(cameraState==CameraState(camera),prefix+" actual camera invariant across captures");
        Check(otherVisuals==OtherVisualState(),prefix+" other 20 floor visual state invariant");
    }
    IEnumerator Start()
    {
        for(int i=0;i<30;i++)yield return null;
        var preview=UnityEngine.Object.FindFirstObjectByType<MainStagePreview>();
        if(preview&&preview.IsPreviewing)Invoke(preview,"Advance",0f,true);
        for(int i=0;i<180 && (MainStagePreview.IsActive||StageStartTransition.IsActive);i++)yield return null;
        yield return new WaitForSeconds(.3f);
        Check(!MainStagePreview.IsActive&&!StageStartTransition.IsActive,"Gameplay resumed through existing preview skip path");
        Check(Floors.Length==21&&Floors.Distinct().Count()==21,"Floor audit contains 21 distinct targets");
        Check(Floors.Count(n=>n!=MainS3MarkerVisual.TargetName)==20,"Exactly 20 non-target floors are audited");
        mover=UnityEngine.Object.FindFirstObjectByType<PlayerMover>();body=mover.GetComponent<Rigidbody2D>();
        var target=Find(S3ReturnStepBMarkerProofVisual.TargetName);
        var box=target.GetComponent<BoxCollider2D>();var camera=Camera.main;
        var stairs=Find(MainStageSectionThreeSetup.LowDeadEndName).GetComponent<MainStageSectionThreeRecoveryStairs>();
        production=UnityEngine.Object.FindFirstObjectByType<MainS3MarkerVisual>();
        Check(production&&production.Ready,"Production auto-bootstrap prepares inactive Step B");
        if(!production||!production.Ready) {throw new Exception("Production visual not ready");}
        Check(UnityEngine.Object.FindObjectsByType<MainS3MarkerVisual>(FindObjectsSortMode.None).Length==1,"One Production presentation owner");
        var owner=new GameObject("QA-only approved proof reference");proof=owner.AddComponent<S3ReturnStepBMarkerProofVisual>();
        proof.SetCandidate(false);
        for(int i=0;i<15;i++)yield return null;
        Check(proof.Ready,"Candidate prepared while step is inactive");
        Check(!target.activeInHierarchy&&!stairs.IsRevealed,"Step B hidden before switch");
        Check(Find(MainStageSectionThreeSetup.ReturnStepAName)&&Find(MainStageSectionThreeSetup.ReturnStepCName)&&Find(MainStageSectionThreeSetup.ReturnStepDName),"Other return steps remain present");
        File.WriteAllText(Path.Combine(Dir,"TargetAudit.txt"),"Scene="+target.scene.name+"\nName="+target.name+"\nPosition="+target.transform.position.ToString("F6")+"\nScale="+target.transform.lossyScale.ToString("F6")+"\nBounds="+box.bounds+"\nCollider="+EditorJsonUtility.ToJson(box)+"\nRigidbody="+EditorJsonUtility.ToJson(target.GetComponent<Rigidbody2D>())+"\nComponents="+string.Join(",",target.GetComponents<Component>().Select(c=>c.GetType().Name)));
        string initialPhysics=StaticPhysics();
        string[] otherContracts=Floors.Where(n=>n!=S3ReturnStepBMarkerProofVisual.TargetName).Select(n=>n+"|"+EditorJsonUtility.ToJson(Find(n).transform)+"|"+EditorJsonUtility.ToJson(Find(n).GetComponent<BoxCollider2D>())).ToArray();

        // Diagnostic placement, not a stage/source change. Exercise the actual
        // proximity guard + switch activation method with the original components.
        body.position=new Vector2(45.6f,-5.42f);body.linearVelocity=Vector2.zero;Physics2D.SyncTransforms();
        yield return new WaitForSeconds(.5f);
        var sw=Find(MainStageSectionThreeSetup.RecoverySwitchName).GetComponent<MainStageSectionThreeRecoverySwitch>();
        Invoke(sw,"TryActivate");
        Check(stairs.IsRevealed&&target.activeInHierarchy,"Existing switch activation reveals step");
        var revealTrace=new List<string>();
        for(int i=0;i<14;i++) {
            yield return new WaitForSeconds(.08f);
            var block=new MaterialPropertyBlock();production.Surface.GetPropertyBlock(block);
            revealTrace.Add(Time.time.ToString("F4")+"|"+production.Surface.transform.localPosition.ToString("F6")+"|"+production.Surface.forceRenderingOff+"|"+block.GetColor("_Color"));
        }
        File.WriteAllLines(Path.Combine(Dir,"RevealTimeline.txt"),revealTrace);
        Check(production.Surface.transform.localPosition==Vector3.zero&&!production.Surface.forceRenderingOff,"Existing reveal completes with final marker pose");
        File.WriteAllText(Path.Combine(Dir,"RevealedStaticPhysics.txt"),StaticPhysics());
        Check(Floors.Where(n=>n!=S3ReturnStepBMarkerProofVisual.TargetName).Select(n=>n+"|"+EditorJsonUtility.ToJson(Find(n).transform)+"|"+EditorJsonUtility.ToJson(Find(n).GetComponent<BoxCollider2D>())).SequenceEqual(otherContracts),"All other 20 floor transform/collider contracts unchanged");
        Check(Vector3.Distance(target.transform.position,new Vector3(41f,-10.775f,0))<.0001f && Vector3.Distance(target.transform.lossyScale,new Vector3(1.8f,10.55f,1))<.0001f,"Target Transform unchanged");
        Check(box.size==Vector2.one&&box.offset==Vector2.zero&&!box.isTrigger,"Target collider unchanged");

        body.position=new Vector2(41f,-4.87f);body.linearVelocity=Vector2.zero;Physics2D.SyncTransforms();
        yield return new WaitForSeconds(.85f);
        Check(mover.IsGrounded,"Step B standing",body.position.ToString("F5"));
        var contacts=new ContactPoint2D[32];int count=body.GetContacts(contacts);
        Check(contacts.Take(count).Any(c=>c.collider==box||c.otherCollider==box),"Step B actual collider contact");
        File.WriteAllText(Path.Combine(Dir,"ActualGameplayCamera.json"),CameraState(camera));
        int gameplayWidth=Mathf.RoundToInt(camera.aspect*1080);
        Check(Mathf.Abs(gameplayWidth/1080f-camera.aspect)<.000001f,"Gameplay capture preserves measured Game-view aspect");
        ComparePair(camera,"01_Gameplay",null,null,gameplayWidth,1080);
        ComparePair(camera,"02_Near",new Vector3(41f,-5.45f,-10),2.1f);
        ComparePair(camera,"03_Surroundings",new Vector3(41f,-3.0f,-10),8.7f);
        ComparePair(camera,"04_FullObject",new Vector3(41f,-10.775f,-10),5.9f,600,1800);
        body.position=new Vector2(45.6f,-5.42f);body.linearVelocity=Vector2.zero;Physics2D.SyncTransforms();
        yield return new WaitForSeconds(.4f);
        ComparePair(camera,"04b_BareEnd",new Vector3(41f,-5.55f,-10),1.6f);
        body.position=new Vector2(41f,-4.87f);body.linearVelocity=Vector2.zero;Physics2D.SyncTransforms();
        yield return new WaitForSeconds(.5f);

        float startX=body.position.x;input=.6f;
        for(int i=0;i<4;i++)yield return new WaitForFixedUpdate();input=0;
        for(int i=0;i<5;i++)yield return new WaitForFixedUpdate();
        Check(mover.IsGrounded&&body.position.x>startX+.015f,"Step B walking",startX+" -> "+body.position.x);
        jump=true;for(int i=0;i<3;i++)yield return new WaitForFixedUpdate();
        Check(body.linearVelocity.y>1,"Step B jump",body.linearVelocity.ToString("F5"));
        yield return new WaitForSeconds(.18f);Check(!mover.IsGrounded,"Step B airborne");
        yield return new WaitForSeconds(1.25f);Check(mover.IsGrounded,"Step B landing");
        var rope=mover.GetComponent<RopeController>();var resource=mover.GetComponent<RopeResource>();
        var hook=Find(MainStageSectionThreeSetup.LowerHookName).GetComponent<HookPoint>();
        Vector2 aim=hook.transform.position;
        Set(rope,"keyboardAimDirection",(aim-body.position).normalized);
        rope.RestoreSelectedRopeLength(7);yield return null;
        float length=resource.CurrentLength;
        Check(rope.TryResolveCurrentAimHook(out HookPoint found,out Vector2 hit)&&found==hook,"S3 lower Hook aiming");
        Check(rope.TryAttach(aim)&&rope.IsAttached,"S3 Hook attachment");
        Check(Mathf.Approximately(length,resource.CurrentLength),"Rope amount unchanged by attachment");
        Capture(camera,"05_Attached_Production",Mathf.RoundToInt(camera.aspect*1080),1080);
        Vector2 velocity=body.linearVelocity;float angular=body.angularVelocity;
        rope.DetachAndRefund();
        Check(!rope.IsAttached&&body.linearVelocity==velocity&&body.angularVelocity==angular,"Detach preserves measured velocity",velocity.ToString("F5"));
        Check(Mathf.Approximately(length,resource.CurrentLength),"Rope amount unchanged by detach");

        // Check clean hide/re-reveal and restoration without altering the stairs.
        stairs.Hide();yield return null;
        Check(!target.activeInHierarchy&&!production.Surface.gameObject.activeInHierarchy,"Hidden state also hides Production marker");
        stairs.Reveal();yield return new WaitForSeconds(1.1f);
        Check(target.activeInHierarchy&&production.Surface.transform.localPosition==Vector3.zero&&!production.Surface.forceRenderingOff,"Second reveal returns same Production pose, not cumulative motion");
        Check(target.GetComponentsInChildren<S3ReturnStepBMarkerProofVisual>(true).Length==0&&target.transform.Cast<Transform>().Count(t=>t.name==S3ReturnStepBMarkerProofVisual.SurfaceName)==1,"Exactly one child visual; no duplicate");
        Check(production.Surface.GetComponents<Component>().All(c=>c is Transform||c is MeshFilter||c is MeshRenderer),"Production child contains render components only");
        Check(target.GetComponents<BoxCollider2D>().Length==1&&target.GetComponents<Rigidbody2D>().Length==1&&target.GetComponent<Rigidbody2D>().bodyType==RigidbodyType2D.Static,"Target static Physics components unchanged");
        Check(Mathf.Abs(box.bounds.max.y-(-5.5f))<.0001f,"Walking surface remains Y -5.50");
        Check(Find(MainStageSectionThreeSetup.ReturnStepAName).transform.Find(MainS3PencilVisual.SurfaceName),"Adopted Step A pencil remains present");
        var mesh=production.Surface.GetComponent<MeshFilter>().sharedMesh;
        var approvedMesh=proof.Surface.GetComponent<MeshFilter>().sharedMesh;
        Check(mesh.vertices.SequenceEqual(approvedMesh.vertices)&&mesh.normals.SequenceEqual(approvedMesh.normals)&&mesh.triangles.SequenceEqual(approvedMesh.triangles),"Production mesh exactly matches approved mesh");
        Check(mesh.uv.SequenceEqual(approvedMesh.uv)&&mesh.uv2.SequenceEqual(approvedMesh.uv2),"Production UVs exactly match approved mesh");
        Check(production.Surface.sharedMaterial.mainTexture==proof.Surface.sharedMaterial.mainTexture,"Production uses identical approved surface texture");
        Check(production.Surface.sharedMaterial.shader==Resources.Load<Shader>("MainS3Marker")&&proof.Surface.sharedMaterial.shader==Resources.Load<Shader>("S3ReturnStepBMarkerProof"),"Production and reference use their intended approved Shader assets");
        Check(target.transform.Cast<Transform>().Count(t=>t.name==MainS3MarkerVisual.SurfaceName)==1,"Exactly one Production marker child");
        Check(mesh.vertices.All(v=>v.x>=-.500001f&&v.x<=.500001f&&v.y>=-.500001f&&v.y<=.500001f),"All visual XY vertices remain inside fixed collider");
        Check(mesh.vertices.Count(v=>Mathf.Abs(v.y-.5f)<.00001f)>=2&&mesh.bounds.min.x<=-.49999f&&mesh.bounds.max.x>=.49999f,"Flat full-width walking rim");
        var start=Find("Main Start Ground");var ruler=Find(MainStageSectionFourSetup.IntermediateColumnName);
        Check(start.transform.Find("Main Start Ground Craft Stack Visual"),"Adopted Start Ground presentation remains present");
        Check(ruler.transform.Find("Main S4 Wooden Ruler Visual"),"Adopted S4 ruler remains present");
        Check(Find(MainStageSectionThreeSetup.ReturnStepAName).transform.Find(MainS3MarkerVisual.SurfaceName)==null&&Find(MainStageSectionThreeSetup.ReturnStepCName).transform.Find(MainS3MarkerVisual.SurfaceName)==null&&Find(MainStageSectionThreeSetup.ReturnStepDName).transform.Find(MainS3MarkerVisual.SurfaceName)==null,"No A/C/D expansion");
        proof.RestoreLegacyForComparison();
        Check(!proof.Surface.enabled&&production.Surface.enabled,"QA reference hidden; Production presentation remains active");
        SnapshotOtherFloors(camera);
        mover=null;body=null;
        EditorSceneManager.LoadSceneInPlayMode("Assets/Scenes/Tutorial.unity",new LoadSceneParameters(LoadSceneMode.Single));
        for(int i=0;i<40;i++)yield return null;
        Check(!UnityEngine.Object.FindFirstObjectByType<S3ReturnStepBMarkerProofVisual>(),"No QA reference in Tutorial");
        Check(!UnityEngine.Object.FindFirstObjectByType<MainS3MarkerVisual>(),"No Production S3 owner in Tutorial");
        Check(UnityEngine.Object.FindObjectsByType<TutorialTerrainHybridVisual>(FindObjectsSortMode.None).Length==1,"Tutorial approved terrain remains");
        Check(UnityEngine.Object.FindFirstObjectByType<TutorialCraftRoomLayers>(),"Tutorial approved 2.5D background remains");
        File.WriteAllText(Path.Combine(Dir,"TutorialStaticPhysics.txt"),StaticPhysics());
        EditorSceneManager.LoadSceneInPlayMode("Assets/Scenes/MainStage.unity",new LoadSceneParameters(LoadSceneMode.Single));
        for(int i=0;i<40;i++)yield return null;
        var reloaded=UnityEngine.Object.FindFirstObjectByType<MainS3MarkerVisual>();
        Check(reloaded&&reloaded.Ready,"MainStage reload automatically restores Production presentation");
        Check(UnityEngine.Object.FindObjectsByType<MainS3MarkerVisual>(FindObjectsSortMode.None).Length==1,"MainStage reload does not duplicate owner");
        Check(!UnityEngine.Object.FindFirstObjectByType<S3ReturnStepBMarkerProofVisual>(),"Reload has no QA reference automatically");
        Check(errors.Count==0,"Gameplay console errors",string.Join("\n",errors));
        File.WriteAllText(Path.Combine(Dir,"Result.json"),JsonUtility.ToJson(new Results{unity=Application.unityVersion,
            method="Production Runtime in existing isolated Editor Play. QA reference is constructed only by this Editor harness for synchronous approved-vs-Production captures. Diagnostic player placement, private input-field injection and existing switch/rope API calls; NOT human real-key play or a full route difficulty test. Render-only camera for PNG capture; real camera settings/behavior unmodified. UnityEditor.Search exceptions recorded separately, not concealed as Console zero.",checks=checks.ToArray(),errors=errors.ToArray(),editorSearchErrors=editorSearchErrors.ToArray(),failures=checks.Count(c=>c.status=="FAIL")},true));
        EditorApplication.isPlaying=false;
    }
    void SnapshotOtherFloors(Camera camera)
    {
        var records=new List<string>();
        foreach(string name in Floors.Where(n=>n!=S3ReturnStepBMarkerProofVisual.TargetName)) {
            var floor=Find(name);records.Add(name+"|"+EditorJsonUtility.ToJson(floor.transform)+"|"+EditorJsonUtility.ToJson(floor.GetComponent<BoxCollider2D>())+"|"+string.Join(",",floor.GetComponentsInChildren<Renderer>(true).Select(r=>r.gameObject.name+":"+(r.sharedMaterial?r.sharedMaterial.shader.name:"none"))));
        }
        File.WriteAllLines(Path.Combine(Dir,"Other20FloorPresentations.txt"),records);
    }
    string OtherVisualState()
    {
        var rows=new List<string>();
        foreach(string name in Floors.Where(n=>n!=S3ReturnStepBMarkerProofVisual.TargetName))
            foreach(var r in Find(name).GetComponentsInChildren<Renderer>(true).OrderBy(r=>PathOf(r.transform)))
            {
                var filter=r.GetComponent<MeshFilter>();
                var sprite=r as SpriteRenderer;
                rows.Add(name+"/"+PathOf(r.transform)+"|"+r.transform.localPosition.ToString("R")+"|"+r.transform.localScale.ToString("R")+"|"+r.enabled+"|"+r.forceRenderingOff+"|"+r.sortingLayerID+"|"+r.sortingOrder+"|"+(r.sharedMaterial?r.sharedMaterial.GetInstanceID():0)+"|"+(filter&&filter.sharedMesh?filter.sharedMesh.GetInstanceID():0)+"|"+(sprite&&sprite.sprite?sprite.sprite.GetInstanceID():0));
            }
        return string.Join("\n",rows);
    }
}
#endif
