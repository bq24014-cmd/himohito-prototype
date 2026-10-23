using System;
using System.Collections.Generic;
using UnityEngine;
using HimoHito;

// Isolated presentation; authoritative contact is supplied by the target rope/blade detector.
public sealed class ScissorsCutVfx : MonoBehaviour
{
    [Serializable] public class CutRecord {
        public int frame, pointCount, pieces, endFrays, fibres=4;
        public float time, fraction, distanceFromContact, sourceWidth, elapsed, linePointError, fixedTime, endTime, scissorsClosure;
        public Vector3 contact, cut, first, last; public string mode, cleanup;
        public bool attachedBefore, attachedAfter, replacedOldFluff;
        public Vector3[] renderedRope, bladeLeft, bladeRight;public Vector3 playerMin,playerMax,currentHead;
        public Vector2 playerPosition,playerVelocity;public int successSnips,finishedFrame;public bool physicalFailureObserved;
        public string presentation;public float configuredLifetime;
        public float contactTime,renderedCutStartError,renderedCutStartErrorPixels;public bool authoritativeContact;
    }
    public readonly List<CutRecord> records=new List<CutRecord>();
    public int captureFailures; public CutRopeCopy Active {get;private set;}
    RopeSpikeHazard hazard; RopeSnapshot pending;
    void Awake(){if(name!=CraftScissorsMotion.TargetName){enabled=false;return;}hazard=GetComponent<RopeSpikeHazard>();}
    void OnEnable(){if(hazard)hazard.RopeCutSucceeded+=OnSuccess;}
    void OnDisable(){if(hazard)hazard.RopeCutSucceeded-=OnSuccess;Clear();}
    void Clear(){if(pending!=null){Destroy(pending.material);pending=null;}if(Active)Active.Finish("source disabled / scene exit");}
    public void DiscardPendingCapture(){if(pending!=null){if(pending.material)Destroy(pending.material);pending=null;}}
    public bool CaptureBeforeCut(RopeController rope,Collider2D blade,Vector2 contact)
    {
        if(!enabled||!rope.IsAttached)return false;
        if(pending!=null){Destroy(pending.material);pending=null;}
        var line=rope.GetComponent<LineRenderer>();
        if(!line||!line.enabled||line.positionCount<2||!line.sharedMaterial){captureFailures++;return false;}
        pending=RopeSnapshot.Read(line,blade,contact);
        var motion=GetComponent<CraftScissorsMotion>();var body=rope.GetComponent<Rigidbody2D>();var playerCollider=rope.GetComponent<Collider2D>();
        pending.record.fixedTime=Time.fixedTime;pending.record.renderedRope=(Vector3[])pending.points.Clone();
        pending.record.bladeLeft=motion.WorldBlade(0);pending.record.bladeRight=motion.WorldBlade(1);pending.record.scissorsClosure=motion.closure;
        pending.record.playerMin=playerCollider.bounds.min;pending.record.playerMax=playerCollider.bounds.max;
        pending.record.playerPosition=body.position;pending.record.playerVelocity=body.linearVelocity;pending.record.currentHead=rope.GetComponent<RopeBodyVisual>().RopeOrigin;
        return true;
    }
    public bool CaptureConfirmed(RopeController rope,RopeBladeContact.Impact impact,LineRenderer line)
    {
        if(!enabled||!rope.IsAttached||!line||!line.sharedMaterial)return false;
        if(GroundingReview.Arg("-forceCaptureFailure")=="1"){captureFailures++;return false;}
        if(pending!=null)Destroy(pending.material);
        pending=RopeSnapshot.Confirmed(line,impact);
        var motion=GetComponent<CraftScissorsMotion>();var body=rope.GetComponent<Rigidbody2D>();var collider=rope.GetComponent<Collider2D>();
        pending.record.fixedTime=Time.fixedTime;pending.record.renderedRope=(Vector3[])pending.points.Clone();
        pending.record.bladeLeft=impact.left;pending.record.bladeRight=impact.right;pending.record.scissorsClosure=motion.closure;
        pending.record.playerMin=collider.bounds.min;pending.record.playerMax=collider.bounds.max;
        pending.record.playerPosition=body.position;pending.record.playerVelocity=body.linearVelocity;pending.record.currentHead=rope.GetComponent<RopeBodyVisual>().RopeOrigin;
        return true;
    }
    void OnSuccess(RopeController rope)
    {
        if(pending==null)return;
        if(Active)Active.Finish("new cut replaces old copy");
        var record=pending.record;record.frame=Time.frameCount;record.time=Time.time;
        record.attachedBefore=true;record.attachedAfter=rope.IsAttached;record.replacedOldFluff=true;
        record.successSnips=GetComponent<CraftScissorsMotion>().snips;
        records.Add(record);
        Active=new GameObject("C05 cut rope display copy").AddComponent<CutRopeCopy>();
        Active.Begin(pending,rope);pending=null;
    }
}

public sealed class RopeSnapshot
{
    public Vector3[] points; public Material material; public AnimationCurve width;
    public Gradient color;public float multiplier;public Vector2 textureScale;public LineTextureMode textureMode;
    public LineAlignment alignment;public int layer,order,caps,corners,segment;public float u;
    public ScissorsCutVfx.CutRecord record;
    public static RopeSnapshot Confirmed(LineRenderer line,RopeBladeContact.Impact impact)
    {
        var s=new RopeSnapshot{points=(Vector3[])impact.path.Clone(),material=new Material(line.sharedMaterial),
            width=line.widthCurve,multiplier=line.widthMultiplier,color=line.colorGradient,textureScale=line.textureScale,
            textureMode=line.textureMode,alignment=line.alignment,layer=line.sortingLayerID,order=line.sortingOrder,
            caps=line.numCapVertices,corners=line.numCornerVertices,segment=impact.segment,u=impact.u};
        float f=(s.segment+s.u)/(s.points.Length-1f);
        s.record=new ScissorsCutVfx.CutRecord{pointCount=s.points.Length,contact=impact.bladePoint,cut=impact.point,
            first=s.points[0],last=s.points[s.points.Length-1],fraction=f,sourceWidth=s.width.Evaluate(f)*s.multiplier,
            distanceFromContact=impact.centerlineBladeDistance,mode=impact.geometry,contactTime=impact.time,authoritativeContact=true};
        return s;
    }
    public static RopeSnapshot Read(LineRenderer line,Collider2D blade,Vector2 contact)
    {
        var s=new RopeSnapshot {points=new Vector3[line.positionCount],material=new Material(line.sharedMaterial),
            width=line.widthCurve,multiplier=line.widthMultiplier,color=line.colorGradient,textureScale=line.textureScale,
            textureMode=line.textureMode,alignment=line.alignment,layer=line.sortingLayerID,order=line.sortingOrder,
            caps=line.numCapVertices,corners=line.numCornerVertices};
        line.GetPositions(s.points);
        if(!line.useWorldSpace)for(int i=0;i<s.points.Length;i++)s.points[i]=line.transform.TransformPoint(s.points[i]);
        float best=float.PositiveInfinity; bool intersection=false;
        var poly=blade as PolygonCollider2D;
        if(poly){for(int p=0;p<poly.pathCount;p++){
            var path=poly.GetPath(p);
            for(int j=0;j<path.Length;j++){
                Vector2 c=poly.transform.TransformPoint(path[j]+poly.offset),d=poly.transform.TransformPoint(path[(j+1)%path.Length]+poly.offset);
                for(int i=0;i<s.points.Length-1;i++){
                    Vector2 a=s.points[i],b=s.points[i+1],r=b-a,q=d-c;float den=Cross(r,q);
                    if(Mathf.Abs(den)<.000001f)continue;
                    float t=Cross(c-a,q)/den,v=Cross(c-a,r)/den;
                    if(t<0||t>1||v<0||v>1)continue;
                    float distance=((a+t*r)-contact).sqrMagnitude;
                    if(distance<best){best=distance;s.segment=i;s.u=t;intersection=true;}
                }
            }
        }}
        if(!intersection){for(int i=0;i<s.points.Length-1;i++){
            Vector2 a=s.points[i],r=(Vector2)s.points[i+1]-a;
            float t=r.sqrMagnitude>0?Mathf.Clamp01(Vector2.Dot(contact-a,r)/r.sqrMagnitude):0;
            float distance=(a+t*r-contact).sqrMagnitude;
            if(distance<best){best=distance;s.segment=i;s.u=t;}
        }}
        Vector3 cut=Vector3.Lerp(s.points[s.segment],s.points[s.segment+1],s.u);
        float fraction=(s.segment+s.u)/(s.points.Length-1f);
        bool endpoint=fraction<.0001f||fraction>.9999f;
        s.record=new ScissorsCutVfx.CutRecord {pointCount=s.points.Length,contact=contact,cut=cut,first=s.points[0],last=s.points[s.points.Length-1],
            fraction=fraction,sourceWidth=s.width.Evaluate(fraction)*s.multiplier,distanceFromContact=Mathf.Sqrt(best),
            mode=intersection?"observed rope / blade intersection":endpoint?"nearest rope endpoint (no invented extension)":"closest point on rendered rope (not an intersection)",linePointError=0};
        return s;
    }
    static float Cross(Vector2 a,Vector2 b)=>a.x*b.y-a.y*b.x;
}

// Contains only LineRenderers and private material; never controls the live rope or physics.
public sealed class CutRopeCopy : MonoBehaviour
{
    class Piece {public LineRenderer line;public Vector3[] rest;public Vector3 recoil;public float[] weight;public float from,to;}
    class Fibre {public LineRenderer line;public Vector3 origin,axis,drift;public float life;public bool attachedEnd;}
    RopeSnapshot saved; RopeController owner;Rigidbody2D body;Vector2 lastBody;int born;
    MainStageRespawnOnFall respawn;bool legacy;ProgressiveUnravelDrawing progressive;PreviousProgressiveUnravelDrawing previousProgressive;
    readonly List<Piece> pieces=new List<Piece>();readonly List<Fibre> fibres=new List<Fibre>();
    public float Elapsed {get;private set;}public bool Finished {get;private set;}
    public float CurrentAlpha {get;private set;}public float CurrentRecoil {get;private set;}
    public float FrontProgress=>previousProgressive!=null?previousProgressive.Progress:0;
    public float RetainedLength=>progressive!=null?progressive.RetainedLength:previousProgressive!=null?previousProgressive.RetainedLength:0;
    public float NearAlpha=>progressive!=null?progressive.NearAlpha:previousProgressive!=null?previousProgressive.NearAlpha:CurrentAlpha;
    public float MiddleAlpha=>progressive!=null?progressive.MiddleAlpha:previousProgressive!=null?previousProgressive.MiddleAlpha:CurrentAlpha;
    public float FarAlpha=>progressive!=null?progressive.FarAlpha:previousProgressive!=null?previousProgressive.FarAlpha:CurrentAlpha;
    public float MaxSlack=>progressive!=null?progressive.MaxSlack:previousProgressive!=null?previousProgressive.MaxSlack:0;
    public int VisibleFibres=>progressive!=null?progressive.VisibleFibres:previousProgressive!=null?previousProgressive.VisibleFibres:0;
    public Vector3 CurrentFront=>progressive!=null?progressive.Front:previousProgressive!=null?previousProgressive.Front:Vector3.zero;
    public Vector3 FixedEnd=>progressive!=null?progressive.FixedEnd:previousProgressive!=null?previousProgressive.FixedEnd:Vector3.zero;
    public ProgressiveUnravelDrawing.Metrics SoftMetrics=>progressive!=null?progressive.Actual:null;
    public void Begin(RopeSnapshot snapshot,RopeController player)
    {
        saved=snapshot;owner=player;body=player.GetComponent<Rigidbody2D>();lastBody=body.position;born=Time.frameCount;
        legacy=GroundingReview.Arg("-vfxLegacy")=="1";
        respawn=player.GetComponent<MainStageRespawnOnFall>();if(respawn)respawn.PresentationRestartCompleted+=OnRestart;
        if(!legacy&&GroundingReview.Arg("-vfxContinuity")!="1"){
            if(GroundingReview.Arg("-vfxProgressive")=="1"){
                previousProgressive=new PreviousProgressiveUnravelDrawing(saved,transform);
                saved.record.presentation="3caca29 progressive trim baseline";saved.record.configuredLifetime=PreviousProgressiveUnravelDrawing.Lifetime;
                Draw();return;
            }
            progressive=new ProgressiveUnravelDrawing(saved,transform,owner.GetComponent<RopeBodyVisual>());
            saved.record.presentation="soft free-end collapse, connected fibre separation, stationary regional fade";saved.record.configuredLifetime=ProgressiveUnravelDrawing.Lifetime;
            Draw();return;
        }
        saved.record.presentation="1ccfbd0 continuity baseline";saved.record.configuredLifetime=.27f;
        var left=new List<Vector3>();for(int i=0;i<=saved.segment;i++)left.Add(saved.points[i]);
        if(Vector3.Distance(left[left.Count-1],saved.record.cut)>.00001f)left.Add(saved.record.cut);
        var right=new List<Vector3>{saved.record.cut};for(int i=saved.segment+1;i<saved.points.Length;i++)if(Vector3.Distance(right[right.Count-1],saved.points[i])>.00001f)right.Add(saved.points[i]);
        if(left.Count>1)AddPiece(left.ToArray(),false,0,saved.record.fraction);
        if(right.Count>1)AddPiece(right.ToArray(),true,saved.record.fraction,1);
        saved.record.pieces=pieces.Count;
        for(int p=0;p<pieces.Count;p++){
            var piece=pieces[p];for(int j=0;j<2;j++)AddFibre("end fray",piece.recoil*.02f,Quaternion.Euler(0,0,(j==0?-24:25))*piece.recoil*(.07f+j*.025f),piece.recoil*.14f,.25f);
        }
        saved.record.endFrays=pieces.Count*2;
        // Fixed index directions, not Random: presentation does not perturb gameplay randomness.
        for(int j=0;j<4;j++){
            Vector3 d=Quaternion.Euler(0,0,37+j*91)*Vector3.right;
            AddFibre("scattered yarn fibre",Vector3.zero,Quaternion.Euler(0,0,43)*d*(.07f+.02f*j),d*(.15f+.02f*j),.21f+.015f*j);
        }
        Draw();
    }
    LineRenderer Make(string name,float from,float to,float scale=1)
    {
        var go=new GameObject(name);go.transform.SetParent(transform,false);var l=go.AddComponent<LineRenderer>();
        l.useWorldSpace=true;l.sharedMaterial=saved.material;l.alignment=saved.alignment;l.sortingLayerID=saved.layer;l.sortingOrder=saved.order+1;
        l.textureMode=saved.textureMode;l.textureScale=saved.textureScale;l.numCapVertices=saved.caps;l.numCornerVertices=saved.corners;
        var keys=new Keyframe[17];for(int i=0;i<keys.Length;i++){float t=i/16f;keys[i]=new Keyframe(t,saved.width.Evaluate(Mathf.Lerp(from,to,t)));}
        l.widthCurve=new AnimationCurve(keys);l.widthMultiplier=saved.multiplier*scale;
        l.colorGradient=Colors(from,to,1);return l;
    }
    Gradient Colors(float from,float to,float alpha)
    {
        var c=new GradientColorKey[8];var a=new GradientAlphaKey[8];
        for(int i=0;i<8;i++){float t=i/7f;Color v=saved.color.Evaluate(Mathf.Lerp(from,to,t));c[i]=new GradientColorKey(v,t);a[i]=new GradientAlphaKey(v.a*alpha,t);}
        var g=new Gradient();g.SetKeys(c,a);return g;
    }
    void AddPiece(Vector3[] points,bool cutAtStart,float from,float to)
    {
        Vector3 recoil=cutAtStart?(points[1]-points[0]).normalized:(points[points.Length-2]-points[points.Length-1]).normalized;
        var p=new Piece{line=Make("severed rope copy",from,to),rest=points,recoil=recoil,weight=new float[points.Length],from=from,to=to};
        float distance=0;for(int n=0;n<points.Length;n++){int i=cutAtStart?n:points.Length-1-n;if(n>0)distance+=Vector3.Distance(points[i],points[cutAtStart?i-1:i+1]);p.weight[i]=Mathf.Clamp01(1-distance/.7f);}
        p.line.positionCount=points.Length;pieces.Add(p);
    }
    void AddFibre(string name,Vector3 offset,Vector3 axis,Vector3 drift,float life)
    {
        var line=Make(name,saved.record.fraction,saved.record.fraction,.25f);line.positionCount=2;
        fibres.Add(new Fibre{line=line,origin=saved.record.cut+offset,axis=axis,drift=drift,life=life,attachedEnd=name=="end fray"});
    }
    void Update()
    {
        if(Finished||saved==null)return;
        if(!owner||!body||!owner.gameObject.activeInHierarchy){Finish("owner removed");return;}
        if(!body.simulated||!owner.enabled){
            saved.record.physicalFailureObserved=true;
            if(legacy){Finish("failure / owner disabled");return;}
            // A normal fall failure disables the physical player, not this short world-space drawing.
        }
        if(owner.IsAttached){Finish("reattach; live rope untouched");return;}
        float distance=Vector2.Distance(body.position,lastBody);
        if(distance>Mathf.Max(2.5f,body.linearVelocity.magnitude*Time.deltaTime*3+.2f)){Finish("R / discontinuous relocation");return;}
        lastBody=body.position;
        if(Time.frameCount==born||Time.deltaTime<=0)return;
        float life=progressive!=null?ProgressiveUnravelDrawing.Lifetime:previousProgressive!=null?PreviousProgressiveUnravelDrawing.Lifetime:.27f;
        Elapsed=Mathf.Min(life,Elapsed+Time.deltaTime);saved.record.elapsed=Elapsed;
        if(Elapsed>=life){Draw();Finish(progressive!=null?"soft collapse and fray completed":previousProgressive!=null?"progressive unravel completed":"fade completed");return;}
        Draw();
    }
    void Draw()
    {
        if(previousProgressive!=null){previousProgressive.Draw(Elapsed);CurrentAlpha=previousProgressive.FarAlpha;CurrentRecoil=.14f*Mathf.SmoothStep(0,1,(Elapsed-.02f)/.10f);return;}
        if(progressive!=null){progressive.Draw(Elapsed);CurrentAlpha=progressive.Actual.maximumAlpha;CurrentRecoil=.045f*Mathf.Sin(Mathf.PI*Mathf.Clamp01(Elapsed/.18f));if(Elapsed==0){saved.record.linePointError=progressive.Actual.initialShapeError;saved.record.renderedCutStartError=progressive.Actual.cutStartError;saved.record.renderedCutStartErrorPixels=progressive.Actual.cutStartErrorPixels;}return;}
        // Native closing advances at the next fixed step. A short lead-in lets closure and recoil read together;
        // it does not delay the physical cut, sound or create a fictional rope at the blade.
        float recoil=.14f*Mathf.SmoothStep(0,1,(Elapsed-(legacy?0:.02f))/.10f);
        float alpha=legacy?1-Mathf.Clamp01((Elapsed-.12f)/.15f):1-Mathf.SmoothStep(0,1,(Elapsed-.08f)/.19f);
        CurrentAlpha=alpha;CurrentRecoil=recoil;
        foreach(var p in pieces){for(int i=0;i<p.rest.Length;i++)p.line.SetPosition(i,p.rest[i]+p.recoil*(recoil*p.weight[i]));p.line.colorGradient=Colors(p.from,p.to,alpha);}
        foreach(var f in fibres){float t=Mathf.Clamp01(Elapsed/f.life);Vector3 offset=f.drift*Mathf.Sin(t*Mathf.PI*.5f);
            if(!legacy&&f.attachedEnd)offset=f.drift.normalized*recoil;
            Vector3 o=f.origin+offset;f.line.SetPosition(0,o);f.line.SetPosition(1,o+f.axis);f.line.colorGradient=Colors(saved.record.fraction,saved.record.fraction,(1-t*t)*(legacy?1:alpha));}
    }
    void OnRestart()=>Finish("native R restart notification");
    public void Finish(string reason){if(Finished)return;Finished=true;if(saved!=null){saved.record.cleanup=reason;saved.record.endTime=Time.time;saved.record.finishedFrame=Time.frameCount;}gameObject.SetActive(false);Destroy(gameObject);}
    void OnDestroy(){if(respawn)respawn.PresentationRestartCompleted-=OnRestart;if(saved!=null){if(string.IsNullOrEmpty(saved.record.cleanup))saved.record.cleanup="scene unload";Destroy(saved.material);}}
}
