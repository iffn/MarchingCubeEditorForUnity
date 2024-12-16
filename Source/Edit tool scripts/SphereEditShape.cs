#if UNITY_EDITOR

using UnityEngine;
using UnityEngine.UIElements;

namespace iffnsStuff.MarchingCubeEditor.EditTools
{
    public class SphereEditShape : EditShape
    {
        protected override float DistanceOutsideIsPositive(Vector3 localPoint)
        {
            // Transform the point into the shape's local space
            return localPoint.magnitude - 0.5f;
        }

        public override (Vector3 minOffset, Vector3 maxOffset) GetLocalBoundingBox()
        {
            return (-0.5f * Vector3.one, 0.5f * Vector3.one);
        }
    }
}

#endif