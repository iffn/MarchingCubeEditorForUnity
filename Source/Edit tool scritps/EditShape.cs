using UnityEngine;

namespace iffnsStuff.MarchingCubeEditor.EditTools
{
    public abstract class EditShape : MonoBehaviour
    {
        public abstract float DistanceOutsideIsPositive(Vector3 point);

        public Vector3 Position => transform.position;
        public Vector3 Scale => transform.localScale;
    }
}