// Diagnostic only. Installed AFTER Build into isolation, never Production.
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SyncProbe
{
    static string output;
    public static void BuildWithTrace()
    {
        var args=Environment.GetCommandLineArgs();
        for(int i=0;i<args.Length-1;i++)if(args[i]=="-detEvidence")output=args[i+1];
        if(output==null)throw new Exception("Missing trace output");Directory.CreateDirectory(output);
        EditorSceneManager.sceneSaving+=Saving;
        HimoHitoEditor.WindowsPrototypeBuilder.BuildFromCommandLine();
    }
    [Serializable] public class Packet { public string unity,scene; public int objects,components; public List<Node> nodes=new(); }
    [Serializable] public class Node { public string path; public List<Part> components=new(); }
    [Serializable] public class Part { public string type; public List<Field> fields=new(); }
    [Serializable] public class Field { public string property,type,value; }
    static string PathOf(Transform t)=>t.parent==null?t.name:PathOf(t.parent)+"/"+t.name;
    static string Reference(UnityEngine.Object obj)
    {
        if(obj==null)return "NULL";
        if(obj is Component c)return "SCENE:"+PathOf(c.transform)+":"+c.GetType().FullName;
        if(obj is GameObject g)return "SCENE:"+PathOf(g.transform);
        if(AssetDatabase.TryGetGUIDAndLocalFileIdentifier(obj,out string guid,out long id))return "ASSET:"+guid+":"+id;
        return "NAMED:"+obj.GetType().FullName+":"+obj.name;
    }
    public static void Run()
    {
        var args=Environment.GetCommandLineArgs(); string mode="Snapshot";
        for(int i=0;i<args.Length-1;i++) { if(args[i]=="-detEvidence")output=args[i+1]; if(args[i]=="-detMode")mode=args[i+1]; }
        if(output==null)throw new Exception("Missing output"); Directory.CreateDirectory(output);
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        if(mode=="Cause")EditorSceneManager.sceneSaving+=Saving;
        var scene=EditorSceneManager.OpenScene("Assets/Scenes/MainStage.unity",OpenSceneMode.Single);
        var packet=new Packet{unity=Application.unityVersion,scene=scene.name};
        foreach(var go in scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Transform>(true)).Select(t=>t.gameObject).OrderBy(g=>PathOf(g.transform),StringComparer.Ordinal))
        {
            var node=new Node{path=PathOf(go.transform)};
            foreach(var c in go.GetComponents<Component>())
            {
                var part=new Part{type=c==null?"MISSING":c.GetType().FullName};
                if(c!=null)
                {
                    var so=new SerializedObject(c);var p=so.GetIterator();
                    while(p.Next(true))
                    {
                        string v=null;
                        switch(p.propertyType)
                        {
                            case SerializedPropertyType.Boolean:v=p.boolValue.ToString();break;
                            case SerializedPropertyType.Integer:v=p.longValue.ToString();break;
                            case SerializedPropertyType.Float:v=p.doubleValue.ToString("R",System.Globalization.CultureInfo.InvariantCulture);break;
                            case SerializedPropertyType.String:v=p.stringValue;break;
                            case SerializedPropertyType.Enum:v=p.enumValueIndex.ToString();break;
                            case SerializedPropertyType.Vector2:v=p.vector2Value.ToString("R");break;
                            case SerializedPropertyType.Vector3:v=p.vector3Value.ToString("R");break;
                            case SerializedPropertyType.Vector4:v=p.vector4Value.ToString("R");break;
                            case SerializedPropertyType.Quaternion:v=p.quaternionValue.ToString("R");break;
                            case SerializedPropertyType.Color:v=p.colorValue.ToString("R");break;
                            case SerializedPropertyType.Rect:v=p.rectValue.ToString("R");break;
                            case SerializedPropertyType.Bounds:v=p.boundsValue.ToString("R");break;
                            case SerializedPropertyType.ObjectReference:v=Reference(p.objectReferenceValue);break;
                            case SerializedPropertyType.ArraySize:v=p.intValue.ToString();break;
                        }
                        if(v!=null)part.fields.Add(new Field{property=p.propertyPath,type=p.propertyType.ToString(),value=v});
                    }
                    part.fields=part.fields.OrderBy(f=>f.property,StringComparer.Ordinal).ToList();
                }
                node.components.Add(part);packet.components++;
            }
            packet.nodes.Add(node);packet.objects++;
        }
        File.WriteAllText(System.IO.Path.Combine(output,"all_scene_serialized.json"),JsonUtility.ToJson(packet,true));
        File.WriteAllText(System.IO.Path.Combine(output,"success.txt"),"Full scene snapshot completed, mode="+mode);
        Debug.Log("DETERMINISTIC_SYNC_SNAPSHOT_PASS objects="+packet.objects+" components="+packet.components);
        EditorApplication.Exit(0);
    }
    static void Saving(Scene scene,string path)
    {
        File.AppendAllText(System.IO.Path.Combine(output,"scene_saving_stack.txt"),path+"\n"+Environment.StackTrace+"\n");
    }
}
