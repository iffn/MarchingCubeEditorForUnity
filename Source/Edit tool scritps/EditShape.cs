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

        public abstract (Vector3 minOffset, Vector3 maxOffset) GetLocalBoundingBox();

        public (Vector3 worldMin, Vector3 worldMax) GetWorldBoundingBox()
        {
            (Vector3 localMin, Vector3 localMax) = GetLocalBoundingBox();

            // Define the 8 corners of the local bounding box
            Vector3[] corners = new Vector3[8]
            {
                new (localMin.x, localMin.y, localMin.z),
                new (localMax.x, localMin.y, localMin.z),
                new (localMin.x, localMax.y, localMin.z),
                new (localMax.x, localMax.y, localMin.z),
                new (localMin.x, localMin.y, localMax.z),
                new (localMax.x, localMin.y, localMax.z),
                new (localMin.x, localMax.y, localMax.z),
                new (localMax.x, localMax.y, localMax.z)
            };

            // Transform the corners to world space
            Vector3 worldMin = Vector3.positiveInfinity;
            Vector3 worldMax = Vector3.negativeInfinity;

            foreach (Vector3 corner in corners)
            {
                Vector3 worldCorner = transform.TransformPoint(corner); // Apply position, rotation, and scale
                worldMin = Vector3.Min(worldMin, worldCorner);
                worldMax = Vector3.Max(worldMax, worldCorner);
            }

            return (worldMin, worldMax);
        }
    }
}