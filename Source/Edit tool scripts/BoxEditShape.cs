#if UNITY_EDITOR

using UnityEngine;
using UnityEngine.UIElements;

namespace iffnsStuff.MarchingCubeEditor.EditTools
{
    public class BoxEditShape : EditShape
    {
        protected override float DistanceOutsideIsPositive(Vector3 localPoint)
        {
            Vector3 absPoint = new Vector3(
                Mathf.Abs(localPoint.x),
                Mathf.Abs(localPoint.y),
                Mathf.Abs(localPoint.z)
            );

            Vector3 halfExtents = new Vector3(0.5f, 0.5f, 0.5f); // Unit box
            Vector3 delta = absPoint - halfExtents;

            return Mathf.Max(delta.x, delta.y, delta.z);
        }

        public override (Vector3 minOffset, Vector3 maxOffset) GetLocalBoundingBox()
        {
            return (-0.5f * Vector3.one, 0.5f * Vector3.one);
        }
    }
}

#endif