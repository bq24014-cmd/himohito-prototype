using UnityEngine;

// Contact uses the baked view-facing LineRenderer triangles, including its actual cap/corner settings.
public static class RopeDrawMesh
{
    public static Vector3[] Bake(LineRenderer line,Camera camera,out int[] triangles){
        var mesh=new Mesh();line.BakeMesh(mesh,camera,true);
        var v=mesh.vertices;triangles=mesh.triangles;Object.Destroy(mesh);return v;
    }
    public struct Hit {public float distance;public Vector3 point,bladePoint;public int triangle,blade;}
    static float Cross(Vector2 a,Vector2 b)=>a.x*b.y-a.y*b.x;
    static bool Triangle(Vector2 p,Vector2 a,Vector2 b,Vector2 c){
        if(Mathf.Abs(Cross(b-a,c-a))<.000000001f)return false;
        float x=Cross(b-a,p-a),y=Cross(c-b,p-b),z=Cross(a-c,p-c);
        return (x>=0&&y>=0&&z>=0)||(x<=0&&y<=0&&z<=0);
    }
    static bool Inside(Vector2 p,Vector3[] v){bool yes=false;for(int i=0,j=v.Length-1;i<v.Length;j=i++)if((v[i].y>p.y)!=(v[j].y>p.y)&&p.x<(v[j].x-v[i].x)*(p.y-v[i].y)/(v[j].y-v[i].y)+v[i].x)yes=!yes;return yes;}
    public static Hit Measure(Vector3[] vertices,int[] indices,Vector3[][] blades){
        var hit=new Hit{distance=float.PositiveInfinity,triangle=-1};
        for(int n=0;n<indices.Length;n+=3){
            var tri=new[]{vertices[indices[n]],vertices[indices[n+1]],vertices[indices[n+2]]};
            for(int b=0;b<blades.Length;b++){
                var poly=blades[b];
                foreach(var v in tri)if(Inside(v,poly))return new Hit{distance=0,point=v,bladePoint=v,triangle=n/3,blade=b};
                foreach(var v in poly)if(Triangle(v,tri[0],tri[1],tri[2]))return new Hit{distance=0,point=v,bladePoint=v,triangle=n/3,blade=b};
                for(int i=0;i<3;i++)for(int j=0;j<poly.Length;j++){
                    float u,w;RopeBladeContact.ClosestSegments(tri[i],tri[(i+1)%3],poly[j],poly[(j+1)%poly.Length],out u,out w);
                    Vector3 x=Vector3.Lerp(tri[i],tri[(i+1)%3],u),y=Vector3.Lerp(poly[j],poly[(j+1)%poly.Length],w);float d=Vector2.Distance(x,y);
                    if(d<hit.distance)hit=new Hit{distance=d,point=x,bladePoint=y,triangle=n/3,blade=b};
                    if(d<.000001f)return hit;
                }
            }
        }
        return hit;
    }
    public static Vector3[] Blend(Vector3[] a,Vector3[] b,float t){var v=new Vector3[a.Length];for(int i=0;i<v.Length;i++)v[i]=Vector3.Lerp(a[i],b[i],t);return v;}
    static float Travel(Vector3[] a,Vector3[] b){float max=0;for(int i=0;i<a.Length;i++)max=Mathf.Max(max,Vector3.Distance(a[i],b[i]));return max;}
    public static bool Sweep(Vector3[] a,Vector3[] b,int[] triangles,Vector3[][] oldBlades,Vector3[][] newBlades,out float time,out Hit hit){
        time=0;hit=default;
        if(a.Length!=b.Length||oldBlades[0].Length!=newBlades[0].Length||oldBlades[1].Length!=newBlades[1].Length)return false;
        float bound=Travel(a,b)+Mathf.Max(Travel(oldBlades[0],newBlades[0]),Travel(oldBlades[1],newBlades[1]));
        for(int i=0;i<512&&time<=1;i++){
            hit=Measure(Blend(a,b,time),triangles,new[]{Blend(oldBlades[0],newBlades[0],time),Blend(oldBlades[1],newBlades[1],time)});
            if(hit.distance<.000001f)return true;if(bound<.000001f)return false;
            time+=Mathf.Max(.0000001f,hit.distance/bound*.90f);
        }
        return false;
    }
}
