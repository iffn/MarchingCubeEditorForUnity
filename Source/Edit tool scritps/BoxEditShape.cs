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

        public override (Vector3 minOffset, Vector3 maxOffset) GetLocalBoundingBox()
        {
            return (-0.5f * Scale, 0.5f * Scale);
        }
    }
}