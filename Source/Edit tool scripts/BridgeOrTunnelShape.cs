using iffnsStuff.MarchingCubeEditor.EditTools;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public class BridgeOrTunnelShape : EditShape
{
    [SerializeField] Transform startPoint;
    [SerializeField] Transform endPoint;

    public float radius = 1;

    Vector3 StartPoint
    {
        set
        {
            startPoint.position = value;
        }
        get => startPoint.position;
    }

    Vector3 EndPoint
    {
        set
        {
            endPoint.position = value;
        }
        get => endPoint.position;
    }


    Vector3 startPointOptimized;
    Vector3 endPointOptimized;
    shapeTypes shapeType = shapeTypes.circle; 

    public enum shapeTypes
    {
        circle,
        flatTop,
        flatBottom
    }


    public void SetParameters(Vector3 startPoint, Vector3 endPoint, shapeTypes shapeType)
    {
        StartPoint = startPoint;
        EndPoint = endPoint;
        this.shapeType = shapeType;
    }

    public override void PrepareParameters(Transform gridTransform)
    {
        base.PrepareParameters(gridTransform);

        startPointOptimized = ConcertWorldToOptimizedLocalPoint(StartPoint);
        endPointOptimized = ConcertWorldToOptimizedLocalPoint(EndPoint);
    }

    public override (Vector3 minOffset, Vector3 maxOffset) GetLocalBoundingBox()
    {
        float xMin = Mathf.Min(startPointOptimized.x, endPointOptimized.x) - radius;
        float yMin = Mathf.Min(startPointOptimized.y, endPointOptimized.y) - radius;
        float zMin = Mathf.Min(startPointOptimized.z, endPointOptimized.z) - radius;

        float xMax = Mathf.Max(startPointOptimized.x, endPointOptimized.x) + radius;
        float yMax = Mathf.Max(startPointOptimized.y, endPointOptimized.y) + radius;
        float zMax = Mathf.Max(startPointOptimized.z, endPointOptimized.z) + radius;

        return (new Vector3(xMin, yMin, zMin), new Vector3(xMax, yMax, zMax));
    }

    protected override float DistanceOutsideIsPositive(Vector3 localPoint)
    {
        float roundTubeDistance = SDFMath.ShapesDistanceOutsideIsPositive.DistanceToRoundedTube(localPoint, startPointOptimized, endPointOptimized, radius);

        float planeDistance;

        switch (shapeType)
        {
            case shapeTypes.circle:
                return roundTubeDistance;
            case shapeTypes.flatTop:
                planeDistance = SDFMath.ShapesDistanceOutsideIsPositive.DistanceToLevelPlaneFilledBelow(localPoint, startPointOptimized, endPointOptimized);
                return SDFMath.CombinationFunctionsOutsideIsPositive.Intersect(roundTubeDistance, planeDistance);
            case shapeTypes.flatBottom:
                planeDistance = -SDFMath.ShapesDistanceOutsideIsPositive.DistanceToLevelPlaneFilledBelow(localPoint, startPointOptimized, endPointOptimized);
                return SDFMath.CombinationFunctionsOutsideIsPositive.Intersect(roundTubeDistance, planeDistance);
            default:
                return roundTubeDistance;
        }
    }
}
