using UnityEngine;
using UnityEngine.UIElements;

namespace iffnsStuff.MarchingCubeEditor.EditTools
{
    public class SphereEditShape : EditShape
    {
        public override float Distance(Vector3 point)
        {
            float radius = Scale.x * 0.5f; // Assume uniform scale for radius
            return Vector3.Distance(point, Position) - radius;
        }
    }
}