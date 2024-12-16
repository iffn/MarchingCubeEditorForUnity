using System;
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

    protected RayHitResult RaycastAtMousePosition(Event e, bool detectBoundingBox = true)
    {
        Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hitInfo))
            return new RayHitResult(hitInfo.point, hitInfo.normal);
            
        if (!detectBoundingBox)
            return RayHitResult.None;

        Vector3 areaPosition = linkedMarchingCubesController.transform.position;
        Vector3Int areaSize = linkedMarchingCubesController.MaxGrid;
        Bounds bounds = new Bounds(areaPosition + areaSize / 2, areaSize);
        
        var result = bounds.GetIntersectRayPoints(ray);
        if (result != null)
            return new RayHitResult(result.Value.Item2, bounds.GetNormalToSurface(result.Value.Item2));
        
        // Both normal Raycast and Bounds intersection did not succeed 
        return RayHitResult.None;
    }
}
