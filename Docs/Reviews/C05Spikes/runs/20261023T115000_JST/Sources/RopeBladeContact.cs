using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using HimoHito;

// ONE target. Read the actual rendered path after the native rope LateUpdate.
[DefaultExecutionOrder(30000)]
public sealed class RopeBladeContact : MonoBehaviour
{
    [Serializable] public class Sample {
        public int frame,segment,blade,edge;public float time,fixedTime,distance,radius,u;public bool bodyOverlap,centerlineIntersection;
        public Vector2 player,velocity;public Vector3 ropePoint,bladePoint;public Vector3[] path,left,right;
    }
    [Serializable] public class Evidence {
        public string mode;public bool bodyCutDisabled;public List<Sample> samples=new List<Sample>();public List<Impact> impacts=new List<Impact>();
    }
    [Serializable] public class Impact {
        public int segment,blade,edge,frame;public float u,time,frameTime,sweepFraction,radius,centerlineBladeDistance,visualSurfaceGap;
        public Vector3 point,bladePoint;public Vector3[] path,left,right;public bool sweptOnly,bodyOverlap,cutSucceeded;
        public float beforeResource,afterResource,sourceSampleError,sourceSampleErrorPixels,centerlineBladeDistancePixels;
        public string geometry;
    }
    public static Evidence Data;
    RopeController rope;Rigidbody2D body;Collider2D pc;CraftScissorsMotion motion;Sample previous;
    void Awake(){motion=GetComponent<CraftScissorsMotion>();rope=FindFirstObjectByType<RopeController>();body=rope.GetComponent<Rigidbody2D>();pc=rope.GetComponent<Collider2D>();
        Data=new Evidence{mode=GroundingReview.Arg("-contactMode","cut"),bodyCutDisabled=GroundingReview.Arg("-bodyCutBaseline")!="1"};
        var respawn=rope.GetComponent<MainStageRespawnOnFall>();if(respawn)respawn.PresentationRestartCompleted+=ResetHistory;}
    public void ResetHistory(){previous=null;}
    public static void Save(string output){if(Data!=null)File.WriteAllText(Path.Combine(output,"RopeContact.json"),JsonUtility.ToJson(Data,true));}
    void LateUpdate()
    {
        if(!motion||!rope||!rope.IsAttached||!body.simulated||Time.timeScale<=0){previous=null;return;}
        var line=rope.GetComponent<LineRenderer>();if(!line||!line.enabled||line.positionCount<2)return;
        var path=new Vector3[line.positionCount];line.GetPositions(path);if(!line.useWorldSpace)for(int i=0;i<path.Length;i++)path[i]=line.transform.TransformPoint(path[i]);
        var left=motion.WorldBlade(0);var right=motion.WorldBlade(1);
        var s=Closest(path,new[]{left,right});s.time=Time.time;s.fixedTime=Time.fixedTime;s.frame=Time.frameCount;s.player=body.position;s.velocity=body.linearVelocity;
        s.radius=line.widthMultiplier*line.widthCurve.Evaluate((s.segment+s.u)/(path.Length-1f))*.5f;
        s.bodyOverlap=Physics2D.Distance(motion.blades,pc).isOverlapped;s.path=path;s.left=left;s.right=right;
        if(GroundingReview.Arg("-routeQA")=="1")Data.samples.Add(s);
        if(Data.mode=="cut"&&Data.bodyCutDisabled){
            Sample hit=null;float fraction=1;bool sweptOnly=false;
            // Prefer contact in this ACTUAL rendered frame; the swept fallback handles an entire crossing
            // between frames. Both shapes are interpolated at one shared time, never timeless swept AABBs.
            if(s.distance<=s.radius)hit=s;
            else if(previous!=null&&previous.path.Length==path.Length&&Vector2.Distance(previous.player,s.player)<2f){
                hit=Sweep(previous,s,line,out fraction);sweptOnly=hit!=null;
            }
            if(hit!=null){
                var impact=new Impact{segment=hit.segment,u=hit.u,blade=hit.blade,edge=hit.edge,frame=Time.frameCount,
                    point=Vector3.Lerp(hit.path[hit.segment],hit.path[hit.segment+1],hit.u),bladePoint=hit.bladePoint,path=hit.path,left=hit.left,right=hit.right,
                    time=hit.time,frameTime=Time.time,sweepFraction=fraction,radius=hit.radius,centerlineBladeDistance=hit.distance,
                    visualSurfaceGap=Mathf.Max(0,hit.distance-hit.radius),sweptOnly=sweptOnly,bodyOverlap=s.bodyOverlap,
                    geometry=sweptOnly?"common-time interpolation of two observed rendered paths and both blade polygons":"actual current rendered LineRenderer and blade polygons"};
                var actual=Vector3.Lerp(path[hit.segment],path[hit.segment+1],hit.u);
                impact.sourceSampleError=Vector3.Distance(actual,impact.point);
                var camera=Camera.main;if(camera){impact.sourceSampleErrorPixels=Vector2.Distance(camera.WorldToScreenPoint(actual),camera.WorldToScreenPoint(impact.point));impact.centerlineBladeDistancePixels=Vector2.Distance(camera.WorldToScreenPoint(impact.point),camera.WorldToScreenPoint(hit.bladePoint));}
                var resource=rope.GetComponent<RopeResource>();impact.beforeResource=resource.CurrentLength;
                impact.cutSucceeded=GetComponent<RopeSpikeHazard>().TryCutAtRopeContact(rope,impact,line);
                impact.afterResource=resource.CurrentLength;Data.impacts.Add(impact);previous=null;return;
            }
        }
        previous=s;
    }
    static Vector3[] Blend(Vector3[] a,Vector3[] b,float t){var r=new Vector3[a.Length];for(int i=0;i<r.Length;i++)r[i]=Vector3.Lerp(a[i],b[i],t);return r;}
    static float Travel(Vector3[] a,Vector3[] b){float d=0;for(int i=0;i<a.Length;i++)d=Mathf.Max(d,Vector3.Distance(a[i],b[i]));return d;}
    // Conservative advancement: distance is Lipschitz-bounded by the sum of maximum endpoint travel.
    // It cannot skip a crossing of the linearly interpolated rope AND moving blade boundaries.
    public static Sample Sweep(Sample a,Sample b,LineRenderer line,out float fraction){
        fraction=0;if(a.left.Length!=b.left.Length||a.right.Length!=b.right.Length)return null;
        float travel=Travel(a.path,b.path)+Mathf.Max(Travel(a.left,b.left),Travel(a.right,b.right));
        if(travel<.000001f)return null;
        float t=0;
        for(int n=0;n<512&&t<=1;n++){
            var path=Blend(a.path,b.path,t);var left=Blend(a.left,b.left,t);var right=Blend(a.right,b.right,t);
            var h=Closest(path,new[]{left,right});h.radius=line.widthMultiplier*line.widthCurve.Evaluate((h.segment+h.u)/(path.Length-1f))*.5f;
            float gap=h.distance-h.radius;
            if(gap<=.00001f){h.path=path;h.left=left;h.right=right;h.time=Mathf.Lerp(a.time,b.time,t);fraction=t;return h;}
            t+=Mathf.Max(.000001f,gap/travel*.90f);
        }
        return null;
    }
    public static Sample Closest(Vector3[] path,Vector3[][] polygons)
    {
        var hit=new Sample{distance=float.PositiveInfinity};
        for(int i=0;i<path.Length-1;i++)for(int k=0;k<polygons.Length;k++){
            var poly=polygons[k];
            if(Inside(path[i],poly)){hit.distance=0;hit.segment=i;hit.u=0;hit.blade=k;hit.edge=-1;hit.ropePoint=hit.bladePoint=path[i];hit.centerlineIntersection=true;return hit;}
            for(int j=0;j<poly.Length;j++){
                float u,v;Vector2 a=path[i],b=path[i+1],c=poly[j],d=poly[(j+1)%poly.Length];
                ClosestSegments(a,b,c,d,out u,out v);var x=Vector2.Lerp(a,b,u);var y=Vector2.Lerp(c,d,v);float distance=Vector2.Distance(x,y);
                if(distance<hit.distance){hit.distance=distance;hit.segment=i;hit.u=u;hit.blade=k;hit.edge=j;hit.ropePoint=x;hit.bladePoint=y;hit.centerlineIntersection=distance<.000001f;}
            }
        }
        return hit;
    }
    static bool Inside(Vector2 p,Vector3[] poly){bool inside=false;for(int i=0,j=poly.Length-1;i<poly.Length;j=i++)if((poly[i].y>p.y)!=(poly[j].y>p.y)&&p.x<(poly[j].x-poly[i].x)*(p.y-poly[i].y)/(poly[j].y-poly[i].y)+poly[i].x)inside=!inside;return inside;}
    static float Cross(Vector2 a,Vector2 b)=>a.x*b.y-a.y*b.x;
    public static void ClosestSegments(Vector2 a,Vector2 b,Vector2 c,Vector2 d,out float u,out float v)
    {
        var r=b-a;var s=d-c;float den=Cross(r,s);u=v=0;
        if(Mathf.Abs(den)>.00000001f){float x=Cross(c-a,s)/den,y=Cross(c-a,r)/den;if(x>=0&&x<=1&&y>=0&&y<=1){u=x;v=y;return;}}
        float rr=r.sqrMagnitude,ss=s.sqrMagnitude;
        float[] us={0,1,rr>0?Mathf.Clamp01(Vector2.Dot(c-a,r)/rr):0,rr>0?Mathf.Clamp01(Vector2.Dot(d-a,r)/rr):0};
        float[] vs={ss>0?Mathf.Clamp01(Vector2.Dot(a-c,s)/ss):0,ss>0?Mathf.Clamp01(Vector2.Dot(b-c,s)/ss):0,0,1};
        float best=float.PositiveInfinity;for(int n=0;n<4;n++){float q=(a+r*us[n]-c-s*vs[n]).sqrMagnitude;if(q<best){best=q;u=us[n];v=vs[n];}}
    }
}
