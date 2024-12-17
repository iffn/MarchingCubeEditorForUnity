#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;
using iffnsStuff.MarchingCubeEditor.Core;
using iffnsStuff.MarchingCubeEditor.SceneEditor;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class BaseTool
{
    protected MarchingCubeEditor Editor { get; private set; }
    protected MarchingCubesController Controller => Editor.Controller;

    public virtual string DisplayName => "Unnamed Tool";
    public virtual Texture DisplayIcon => null;

    public virtual void DrawUI() {}
    public virtual void HandleSceneUpdate(Event currentEvent) {}
    public virtual void DrawGizmos() {}
    public virtual void OnEnable() {}
    public virtual void OnDisable() {}

    // Search for all Classes inheriting from BaseTool. We do this here so
    // that we only need to search once.
    private static readonly IEnumerable<Type> Tools = AppDomain.CurrentDomain
        .GetAssemblies()
        .SelectMany(assembly => assembly.GetTypes())
        .Where(type => type.IsSubclassOf(typeof(BaseTool)));

    // Instantiates the found BaseTool Classes and sets the reference to the
    // current editor.
    public static IEnumerable<BaseTool> GetTools(MarchingCubeEditor editor) => Tools.Select(type => {
        BaseTool tool = type.Instantiate(editor) as BaseTool;
        tool.Editor = editor;
        return tool;
    });
}

#endif