using iffnsStuff.MarchingCubeEditor.Core;
using UnityEditor;
using UnityEngine;

public abstract class BaseTool
{
    protected MarchingCubesController linkedMarchingCubesController;

    public abstract string displayName { get; }

    public BaseTool(MarchingCubesController linkedMarchingCubesController)
    {
        this.linkedMarchingCubesController = linkedMarchingCubesController;
    }

    public abstract void DrawUI();
    public abstract void HandleSceneUpdate(Event currentEvent);
    public abstract void DrawGizmos();
    public abstract void OnEnable();
    public abstract void OnDisable();

    protected RaycastHit RaycastAtMousePosition(Event e)
    {
        Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);

        Physics.Raycast(ray, out RaycastHit hit);

        return hit;
    }
}
