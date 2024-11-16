using UnityEngine;
using UnityEngine.UIElements;

namespace iffnsStuff.MarchingCubeEditor.EditTools
{
    public class BoxEditShape : EditShape
    {
        public override float DistanceOutsideIsPositive(Vector3 point)
        {
            Vector3 localPoint = TransformToLocalSpace(point, transform);  // Transform to local space

            Vector3 d = new Vector3(
                Mathf.Abs(localPoint.x) - 1f,
                Mathf.Abs(localPoint.y) - 1f,
                Mathf.Abs(localPoint.z) - 1f
            );

            float outsideDistance = Mathf.Max(d.x, Mathf.Max(d.y, d.z));
            float insideDistance = Vector3.Max(d, Vector3.zero).magnitude;

            return outsideDistance + insideDistance;
        }

        public override (Vector3Int min, Vector3Int max) GetBounds(Vector3Int gridResolution)
        {
            float offset = Scale.x * 0.5f;
            Vector3 min = Position - Vector3.one * offset;
            Vector3 max = Position + Vector3.one * offset;

            Vector3Int gridMin = Vector3Int.Max(Vector3Int.zero, Vector3Int.FloorToInt(min));
            Vector3Int gridMax = Vector3Int.Min(gridResolution, Vector3Int.CeilToInt(max));

            return (gridMin, gridMax);
        }
    }
}