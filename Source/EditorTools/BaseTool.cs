#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using iffnsStuff.MarchingCubeEditor.Core;
using iffnsStuff.MarchingCubeEditor.SceneEditor;
using UnityEditor;
using UnityEngine;

public abstract class BaseTool
{
    protected MarchingCubeEditor LinkedMarchingCubeEditor { get; private set; }
    protected MarchingCubesController LinkedMarchingCubeController => LinkedMarchingCubeEditor.LinkedMarchingCubeController;

    public abstract string DisplayName { get; }
    public virtual Texture DisplayIcon => null;

    public static readonly Color highlightBackgroundColor = new Color(0.7f, 0.7f, 1f); //ToDo: Improve highlight color

    public virtual void OnEnable()
    {
        LinkedMarchingCubeController.EnableAllColliders = true;
    }

    public virtual void OnDisable()
    {
        LinkedMarchingCubeController.EnableAllColliders = false; // Will still be on if force collider on is set
        LinkedMarchingCubeController.DisplayPreviewShape = false;
    }
    public virtual void DrawUI() {}
    public virtual void HandleSceneUpdate(Event currentEvent) {}
    public virtual void DrawGizmos() {}

    protected string helpText = "Controls:\n" +
                    "Note that the scene has to be active for some of these to work. Right clicking into the scene view works well for this.\n";

    // Search for all Classes inheriting from BaseTool. We do this here so
    // that we only need to search once.
    public static readonly IEnumerable<Type> Tools = AppDomain.CurrentDomain
        .GetAssemblies()
        .SelectMany(assembly => assembly.GetTypes())
        .Where(type => type.IsSubclassOf(typeof(BaseTool)));

    // Instantiates the found BaseTool Classes and sets the reference to the
    // current editor.
    public static IEnumerable<BaseTool> GetTools(MarchingCubeEditor editor) 
    {
        var tools = Tools.Select(type => Activator.CreateInstance(type) as BaseTool).ToList();
        tools.ForEach(x => x.LinkedMarchingCubeEditor = editor);

        return tools;
    }

    public static bool LeftClickDownEvent(Event e)
    {
        return e.type == EventType.MouseDown && e.button == 0;
    }

    public static bool EscapeDownEvent(Event e)
    {
        return e.type == EventType.KeyDown && e.keyCode == KeyCode.Escape;
    }

    public static bool ControlIsHeld(Event e)
    {
        return e.control;
    }

    public void RefreshUI()
    {
        LinkedMarchingCubeEditor.RefreshUI();
    }

    protected VoxelData[,,] GenerateVoxelDataCopy()
    {
        VoxelData[,,] oldData = new VoxelData[
            LinkedMarchingCubeController.VoxelDataReference.GetLength(0),
            LinkedMarchingCubeController.VoxelDataReference.GetLength(1),
            LinkedMarchingCubeController.VoxelDataReference.GetLength(2)
        ];

        Parallel.For(0, oldData.GetLength(0), x =>
        {
            for (int y = 0; y < oldData.GetLength(1); y++)
            {
                for (int z = 0; z < oldData.GetLength(2); z++)
                {
                    oldData[x, y, z] = LinkedMarchingCubeController.VoxelDataReference[x, y, z];
                }
            }
        });

        return oldData;
    }
}

#endif