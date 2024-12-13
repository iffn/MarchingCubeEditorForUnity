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

    // Replaced the result from RaycastHit to just a Vector3 because of all
    // the values required in RaycastHit that I wasn't able to reproduce.
    // Maybe consider returning a custom struct holding hitPoint as well as
    // hitNormal, as the normal could be useful for some tools.
    protected bool RaycastAtMousePosition(Event e, out Vector3 hitPoint)
    {
        Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hitInfo)) 
        {
            hitPoint = hitInfo.point;
            return true;
        }

        Vector3 areaPosition = linkedMarchingCubesController.transform.position;
        Vector3Int areaSize = linkedMarchingCubesController.MaxGrid;
        Bounds bounds = new Bounds(areaPosition + areaSize / 2, areaSize);
        
        var result = bounds.GetIntersectRayPoints(ray);
        if (result != null)
        {
            hitPoint = result.Value.Item2;
            return true;
        }
        
        // Both normal Raycast and Bounds intersection did not succeed 
        hitPoint = Vector3.zero;
        return false;
    }
}
