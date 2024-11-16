using UnityEngine;

namespace iffnsStuff.MarchingCubeEditor.EditTools
{
    public abstract class EditShape : MonoBehaviour
    {
        public abstract float DistanceOutsideIsPositive(Vector3 point);

        public Vector3 Position => transform.position;
        public Vector3 Scale => transform.localScale;

        [SerializeField] Material linkedMaterial;

        public Color Color
        {
            set
            {
                linkedMaterial.SetColor("_Color", value);
            }
        }

        protected static Vector3 TransformToLocalSpace(Vector3 point, Transform transform)
        {
            // Step 1: Translate to local space by applying inverse position and rotation
            Vector3 localPoint = Quaternion.Inverse(transform.rotation) * (point - transform.position);

            // Step 2: Normalize by scale to treat the shape as a unit object
            localPoint.x /= transform.localScale.x * 0.5f;
            localPoint.y /= transform.localScale.y * 0.5f;
            localPoint.z /= transform.localScale.z * 0.5f;

            return localPoint;
        }

        public abstract (Vector3Int min, Vector3Int max) GetBounds(Vector3Int gridResolution);
    }
}