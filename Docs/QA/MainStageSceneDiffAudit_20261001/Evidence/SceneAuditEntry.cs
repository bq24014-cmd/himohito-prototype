// QA-only Editor harness. Copied to an isolated project; never installed in production Assets.
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
[InitializeOnLoad]
public static class SceneAuditEntry
{
    const string Key="MainStageSceneDiffAudit20261001";
    static string output;
    static int targetFrame;
    [Serializable] public class Packet { public string scene; public List<Node> nodes=new List<Node>(); }
    [Serializable] public class Node {
        public string path; public bool activeSelf,activeInHierarchy;public int layer;public string tag;
        public Vector3 position,localPosition,localScale,lossyScale;public Quaternion localRotation;
        public string[] components;public List<Field> fields=new List<Field>();
        public List<Box> boxes=new List<Box>();public List<Render> renderers=new List<Render>();
    }
    [Serializable] public class Field {public string component,property,value;}
    [Serializable] public class Box {public bool enabled,isTrigger,usedByEffector;public Vector2 size,offset;}
    [Serializable] public class Render {public bool enabled;public int sortingLayerID,sortingOrder;public string drawMode,tileMode,sprite,texture;public Vector2 size,spriteSize;public Rect rect;public Color color;public Bounds bounds;}
    static SceneAuditEntry(){EditorApplication.playModeStateChanged+=Changed;}
    public static void Run(){
        var args=Environment.GetCommandLineArgs();output=null;
        for(int i=0;i<args.Length-1;i++)if(args[i]=="-sceneAuditEvidence")output=args[i+1];
        if(output==null)throw new Exception("Missing evidence output");Directory.CreateDirectory(output);
        SessionState.SetString(Key,output);
        // Never open MainStage in Edit mode: production SceneSync would save it.
        EditorSceneManager.playModeStartScene=null;
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        EditorApplication.EnterPlaymode();
    }
    static void Changed(PlayModeStateChange state){
        if(state!=PlayModeStateChange.EnteredPlayMode)return;
        output=SessionState.GetString(Key,"");if(output=="")return;
        EditorSceneManager.LoadSceneInPlayMode("Assets/Scenes/MainStage.unity",new LoadSceneParameters(LoadSceneMode.Single));
        targetFrame=Time.frameCount+30;EditorApplication.update+=Tick;
    }
    static string PathOf(Transform t)=>t.parent==null?t.name:PathOf(t.parent)+"/"+t.name;
    static void Tick(){
        if(!EditorApplication.isPlaying||Time.frameCount<targetFrame)return;
        EditorApplication.update-=Tick;
        try{
            string[] names={"Main S05 Left Shelf","Main S05 Right Shelf","Main Section 2 Front Spike","Main Section 2 Far Spike"};
            var roots=names.Select(n=>GameObject.Find(n)).ToArray();
            if(roots.Any(x=>x==null))throw new Exception("Missing affected object");
            var packet=new Packet{scene=SceneManager.GetActiveScene().name};
            foreach(var root in roots)foreach(Transform t in root.GetComponentsInChildren<Transform>(true)){
                var go=t.gameObject;var cs=go.GetComponents<Component>();
                var node=new Node{path=PathOf(t),activeSelf=go.activeSelf,activeInHierarchy=go.activeInHierarchy,layer=go.layer,tag=go.tag,position=t.position,localPosition=t.localPosition,localScale=t.localScale,lossyScale=t.lossyScale,localRotation=t.localRotation,components=cs.Select(c=>c==null?"MISSING":c.GetType().FullName).OrderBy(x=>x,StringComparer.Ordinal).ToArray()};
                foreach(var c in cs){
                    if(c is BoxCollider2D b)node.boxes.Add(new Box{enabled=b.enabled,isTrigger=b.isTrigger,usedByEffector=b.usedByEffector,size=b.size,offset=b.offset});
                    if(c is SpriteRenderer r)node.renderers.Add(new Render{enabled=r.enabled,sortingLayerID=r.sortingLayerID,sortingOrder=r.sortingOrder,drawMode=r.drawMode.ToString(),tileMode=r.tileMode.ToString(),size=r.size,color=r.color,bounds=r.bounds,sprite=r.sprite==null?"NULL":r.sprite.name,texture=r.sprite==null?"NULL":r.sprite.texture.name,spriteSize=r.sprite==null?Vector2.zero:(Vector2)r.sprite.bounds.size,rect=r.sprite==null?new Rect():r.sprite.rect});
                    if(c is MonoBehaviour){var so=new SerializedObject(c);var p=so.GetIterator();
                        while(p.NextVisible(true)){
                            string val=null;
                            switch(p.propertyType){
                                case SerializedPropertyType.Boolean:val=p.boolValue.ToString();break;
                                case SerializedPropertyType.Integer:val=p.intValue.ToString();break;
                                case SerializedPropertyType.Float:val=p.floatValue.ToString("R",System.Globalization.CultureInfo.InvariantCulture);break;
                                case SerializedPropertyType.String:val=p.stringValue;break;
                                case SerializedPropertyType.Enum:val=p.enumValueIndex.ToString();break;
                                case SerializedPropertyType.Vector2:val=p.vector2Value.ToString("R");break;
                                case SerializedPropertyType.Vector3:val=p.vector3Value.ToString("R");break;
                                case SerializedPropertyType.ObjectReference:
                                    var reference=p.objectReferenceValue;
                                    val=reference==null?"NULL":reference is Component cc?PathOf(cc.transform)+":"+cc.GetType().Name:reference is GameObject gg?PathOf(gg.transform):reference.GetType().Name+":"+reference.name;break;
                            }
                            if(val!=null)node.fields.Add(new Field{component=c.GetType().FullName,property=p.propertyPath,value=val});
                        }
                    }
                }
                node.fields=node.fields.OrderBy(f=>f.component,StringComparer.Ordinal).ThenBy(f=>f.property,StringComparer.Ordinal).ToList();packet.nodes.Add(node);
            }
            packet.nodes=packet.nodes.OrderBy(n=>n.path,StringComparer.Ordinal).ToList();
            File.WriteAllText(System.IO.Path.Combine(output,"runtime.json"),JsonUtility.ToJson(packet,true));
            // Local visual comparison only: isolate changed objects on layer 30 after snapshot.
            foreach(var root in roots)foreach(Transform t in root.GetComponentsInChildren<Transform>(true))t.gameObject.layer=30;
            foreach(var cam in UnityEngine.Object.FindObjectsByType<Camera>(FindObjectsSortMode.None))cam.enabled=false;
            Capture("S05_Rails",new Vector2(97.3f,-.95f),4f);
            Capture("S02_FrontSpike",roots[2].transform.position,1.2f);
            Capture("S02_FarSpike",roots[3].transform.position,1.2f);
            File.WriteAllText(System.IO.Path.Combine(output,"success.txt"),"Setup complete; runtime snapshot and three local renders captured. No Human Art PASS assigned.");
            Debug.Log("SCENE_AUDIT_CAPTURE_PASS "+output);EditorApplication.Exit(0);
        }catch(Exception e){Debug.LogException(e);File.WriteAllText(System.IO.Path.Combine(output,"failure.txt"),e.ToString());EditorApplication.Exit(1);}
    }
    static void Capture(string label,Vector2 center,float height){
        var go=new GameObject("SceneAuditCaptureCamera");var cam=go.AddComponent<Camera>();
        go.transform.position=new Vector3(center.x,center.y,-10f);cam.orthographic=true;cam.orthographicSize=height;
        cam.cullingMask=1<<30;cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=new Color(.07f,.06f,.11f,1f);cam.enabled=false;
        var rt=new RenderTexture(1280,720,24,RenderTextureFormat.ARGB32);cam.targetTexture=rt;cam.Render();
        var old=RenderTexture.active;RenderTexture.active=rt;var tex=new Texture2D(1280,720,TextureFormat.RGBA32,false);
        tex.ReadPixels(new Rect(0,0,1280,720),0,0);tex.Apply();File.WriteAllBytes(System.IO.Path.Combine(output,label+".png"),tex.EncodeToPNG());
        RenderTexture.active=old;cam.targetTexture=null;UnityEngine.Object.DestroyImmediate(tex);UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(go);
    }
}
