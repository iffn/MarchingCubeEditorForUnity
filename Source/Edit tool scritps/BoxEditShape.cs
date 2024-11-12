using UnityEngine;
using UnityEngine.UIElements;

namespace iffnsStuff.MarchingCubeEditor.EditTools
{
    public class BoxEditShape : EditShape
    {
        public override float DistanceOutsideIsPositive(Vector3 point)
        {
            // Step 1: Translate point into box's local space by applying the inverse of position and rotation
            Vector3 localPoint = Quaternion.Inverse(transform.rotation) * (point - Position);

            // Step 2: Scale normalization to treat the box as an axis-aligned unit box
            Vector3 halfSize = Scale * 0.5f;
            Vector3 normalizedPoint = new Vector3(
                localPoint.x / halfSize.x,
                localPoint.y / halfSize.y,
                localPoint.z / halfSize.z
            );

            // Step 3: Compute the signed distance
            Vector3 d = new Vector3(
                Mathf.Abs(normalizedPoint.x) - 1f,
                Mathf.Abs(normalizedPoint.y) - 1f,
                Mathf.Abs(normalizedPoint.z) - 1f
            );

            // Inside distance (negative if point is inside), otherwise positive distance
            float outsideDistance = Mathf.Max(d.x, Mathf.Max(d.y, d.z));
            float insideDistance = Vector3.Max(d, Vector3.zero).magnitude;

            return outsideDistance + insideDistance;
        }
    }
}