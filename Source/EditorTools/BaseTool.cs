using iffnsStuff.MarchingCubeEditor.Core;
using System.Collections;
using System.Collections.Generic;
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
    public abstract void HandleUIUpdate(Event e);
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
