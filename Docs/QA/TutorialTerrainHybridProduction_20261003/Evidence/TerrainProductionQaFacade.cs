#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using HimoHito;
using UnityEngine;

// Isolated Editor QA ONLY. This type and every comparison operation are
// preprocessor-excluded from standalone builds. No input or runtime bootstrap.
// Production has no public test API: reflection is limited to read-only state;
// the temporary same-frame comparison modifies renderer visibility only.
public sealed class TerrainProductionQaFacade
{
    const BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
    public TutorialTerrainHybridVisual Instance { get; }
    public bool Ready => Instance != null && (bool)Field(Instance, "ready");
    public bool HybridEnabled { get; private set; } = true;
    public bool enabled { get => Instance.enabled; set => Instance.enabled = value; }
    public int TargetCount
    {
        get { int count = 0; if (Ready) foreach (object floor in Floors) if (Field(floor, "target") is GameObject target && target != null) count++; return count; }
    }
    IEnumerable Floors => (IEnumerable)Field(Instance, "floors");
    struct Visibility
    {
        public bool enabled, forcedOff;
        public Visibility(Renderer renderer) { enabled = renderer.enabled; forcedOff = renderer.forceRenderingOff; }
        public void Restore(Renderer renderer) { renderer.enabled = enabled; renderer.forceRenderingOff = forcedOff; }
    }
    Dictionary<Renderer, Visibility> temporaryOriginal;
    static object Field(object obj, string name)
    {
        FieldInfo field = obj.GetType().GetField(name, PrivateInstance);
        if (field == null) throw new MissingFieldException(obj.GetType().FullName, name);
        return field.GetValue(obj);
    }
    public TerrainProductionQaFacade(TutorialTerrainHybridVisual instance)
    { Instance = instance != null ? instance : throw new ArgumentNullException(nameof(instance)); }
    public GameObject GetTarget(string name)
    {
        foreach (object floor in Floors) if ((string)Field(floor, "name") == name) return Field(floor, "target") as GameObject;
        return null;
    }
    public Bounds GetVisualBounds(string name)
    {
        foreach (object floor in Floors) if ((string)Field(floor, "name") == name && Field(floor, "display") is Renderer renderer && renderer != null)
            return renderer.bounds;
        return default;
    }
    public string TextureCropInfo
    {
        get
        {
            var result = new StringBuilder();
            foreach (object floor in Floors) result.Append(Field(floor, "name")).Append(": ").Append(Field(floor, "resource"))
                .Append(" ").Append(Field(floor, "textureSize")).Append(" UV ").Append(Field(floor, "crop")).AppendLine();
            return result.ToString();
        }
    }
    public void SetHybrid(bool value)
    {
        // This is a synchronous diagnostic scope, never a persistent mode.
        // Each false must be followed by true before yielding the Unity frame.
        if (value)
        {
            if (temporaryOriginal != null)
            {
                foreach (var item in temporaryOriginal) if (item.Key != null) item.Value.Restore(item.Key);
                temporaryOriginal = null;
            }
            HybridEnabled = true; return;
        }
        if (!Ready || temporaryOriginal != null) throw new InvalidOperationException("Comparison requires ready presentation and no nested temporary override");
        temporaryOriginal = new Dictionary<Renderer, Visibility>();
        foreach (object floor in Floors)
        {
            var display = Field(floor, "display") as Renderer;
            if (display != null) { temporaryOriginal.Add(display, new Visibility(display)); display.enabled = false; }
            var originals = (IDictionary)Field(floor, "originalVisibility");
            foreach (DictionaryEntry entry in originals)
            {
                var renderer = entry.Key as Renderer; if (renderer == null) continue;
                temporaryOriginal.Add(renderer, new Visibility(renderer));
                renderer.forceRenderingOff = (bool)entry.Value;
            }
        }
        HybridEnabled = false;
    }
}
#endif
