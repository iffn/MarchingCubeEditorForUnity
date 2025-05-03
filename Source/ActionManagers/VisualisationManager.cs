#if UNITY_EDITOR
using iffnsStuff.MarchingCubeEditor.Core;
using UnityEngine;

public class VisualisationManager : MonoBehaviour
{
    MarchingCubesController linkedController;
    Transform linkedControllerTransform;

    public BaseTool drawGizmosTool;

    bool showGridOutline => linkedController.ShowGridOutline;

    public bool InvertNormals
    {
        set
        {
            linkedController.InvertAllNormals = value;
        }
    }

    public void Initialize(MarchingCubesController linkedController)
    {
        this.linkedController = linkedController;
        linkedControllerTransform = linkedController.transform;
    }

    private void OnDrawGizmos() //ToDo: Implement different controller scales
    {
        if (linkedController == null || !linkedController.IsInitialized)
            return;

        // Draw tool gizmos
        if(drawGizmosTool != null)
            drawGizmosTool.DrawGizmos();

        if (showGridOutline)
        {
            Vector3 outlineSize = new Vector3(linkedController.GridResolutionX - 1, linkedController.GridResolutionY - 1, linkedController.GridResolutionZ - 1);
            outlineSize = new Vector3(outlineSize.x * linkedControllerTransform.localScale.x, outlineSize.y * linkedControllerTransform.localScale.y, outlineSize.z * linkedControllerTransform.localScale.z);
            Gizmos.color = Color.cyan;
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, outlineSize);
            Gizmos.DrawWireCube(Vector3.one / 2f, Vector3.one);
        }
    }

    public  void DrawCircle(Vector3 center, float radius, int segments, Vector3 direction)
    {
        // Normalize the direction vector to ensure consistent results
        direction = direction.normalized;

        // Calculate a basis for the plane
        Vector3 up = Vector3.up; // Default up vector
        if (Vector3.Dot(direction, up) > 0.99f) // Handle edge case where direction is parallel to up
            up = Vector3.right;

        Vector3 right = Vector3.Cross(direction, up).normalized;
        up = Vector3.Cross(right, direction).normalized;

        float angleStep = 360f / segments;
        Vector3 prevPoint = center + right * radius; // Start at the "rightmost" point relative to the plane

        for (int i = 1; i <= segments; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;

            // Calculate the new point in the plane defined by `right` and `up`
            Vector3 newPoint = center + (Mathf.Cos(angle) * right + Mathf.Sin(angle) * up) * radius;

            // Draw the line between the previous point and the new point
            Gizmos.DrawLine(prevPoint, newPoint);
            prevPoint = newPoint;
        }
    }

}
#endif