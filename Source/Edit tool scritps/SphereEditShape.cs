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

        public override (Vector3 minOffset, Vector3 maxOffset) GetLocalBoundingBox()
        {
            return (-0.5f * Scale, 0.5f * Scale);
        }
    }
}