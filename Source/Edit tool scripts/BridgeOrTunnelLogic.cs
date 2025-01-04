using iffnsStuff.MarchingCubeEditor.EditTools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BridgeOrTunnelLogic : EditShape
{
    [SerializeField] Transform startPoint;
    [SerializeField] Transform endPoint;

    public float radius = 1;

    public Vector3 StartPoint
    {
        set
        {
            startPoint.position = value;
        }
        get => startPoint.position;
    }

    public Vector3 EndPoint
    {
        set
        {
            endPoint.position = value;
        }
        get => endPoint.position;
    }

    Vector3 startPointOptimized;
    Vector3 endPointOptimized;

    public override void PrepareParameters(Transform gridTransform)
    {
        base.PrepareParameters(gridTransform);

        startPointOptimized = ConcertWorldToOptimizedLocalPoint(StartPoint);
        endPointOptimized = ConcertWorldToOptimizedLocalPoint(EndPoint);
    }


    public override (Vector3 minOffset, Vector3 maxOffset) GetLocalBoundingBox()
    {
        float xMin = Mathf.Min(startPointOptimized.x, endPointOptimized.x) - radius * 2;
        float yMin = Mathf.Min(startPointOptimized.y, endPointOptimized.y) - radius * 2;
        float zMin = Mathf.Min(startPointOptimized.z, endPointOptimized.z) - radius * 2;

        float xMax = Mathf.Max(startPointOptimized.x, endPointOptimized.x) + radius * 2;
        float yMax = Mathf.Max(startPointOptimized.y, endPointOptimized.y) + radius * 2;
        float zMax = Mathf.Max(startPointOptimized.z, endPointOptimized.z) + radius * 2;

        return (new Vector3(xMin, yMin, zMin), new Vector3(xMax, yMin, zMax));
    }

    protected override float DistanceOutsideIsPositive(Vector3 localPoint)
    {
        return SDFMath.ShapesDistanceOutsideIsPositive.DistanceToRoundedTube(localPoint, startPointOptimized, endPointOptimized, radius);
    }
}
