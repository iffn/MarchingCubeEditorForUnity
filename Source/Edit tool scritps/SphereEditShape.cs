using UnityEngine;
using UnityEngine.UIElements;

namespace iffnsStuff.MarchingCubeEditor.EditTools
{
    public class SphereEditShape : EditShape
    {
        public override float DistanceOutsideIsPositive(Vector3 point)
        {
            Vector3 localPoint = TransformToLocalSpace(point, transform); // Step 1: Transform to local space
            float localDistance = localPoint.magnitude - 1f;               // Step 2: Calculate distance to unit sphere

            float averageScale = (transform.localScale.x + transform.localScale.y + transform.localScale.z) / 3f;
            return localDistance * averageScale;
        }

        public override (Vector3Int min, Vector3Int max) GetBounds(Vector3Int gridResolution)
        {
            float radius = Scale.x * 0.5f;
            Vector3 min = Position - Vector3.one * radius;
            Vector3 max = Position + Vector3.one * radius;

            Vector3Int gridMin = Vector3Int.Max(Vector3Int.zero, Vector3Int.FloorToInt(min));
            Vector3Int gridMax = Vector3Int.Min(gridResolution, Vector3Int.CeilToInt(max));

            return (gridMin, gridMax);
        }
    }
}