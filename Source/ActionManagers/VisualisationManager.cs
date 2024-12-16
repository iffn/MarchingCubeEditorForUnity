using iffnsStuff.MarchingCubeEditor.Core;
using UnityEngine;

public class VisualisationManager : MonoBehaviour
{
    MarchingCubesController linkedController;
    Transform linkedControllerTransform;

    public bool ShowGridOutline = true;

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

    private void OnDrawGizmos()
    {
        if (!ShowGridOutline) return;

        if (linkedController == null || !linkedController.IsInitialized) return;

        Gizmos.color = Color.cyan; // Set outline color

        // Calculate grid bounds and draw lines for the grid outline
        DrawGridOutline();
    }

    private void DrawGridOutline()
    {
        // Define the grid size and cell size
        Vector3 cellSize = linkedControllerTransform.localScale;

        // Calculate the starting position of the grid (bottom-left-front corner)
        Vector3 gridOrigin = linkedControllerTransform.position;
        Vector3 outlineSize = new Vector3(linkedController.GridResolutionX - 1, linkedController.GridResolutionY - 1, linkedController.GridResolutionZ - 1);
        outlineSize = UnityUtilityFunctions.ComponentwiseMultiply(outlineSize, cellSize);

        // Calculate all eight corners of the grid box
        Vector3[] corners = new Vector3[8];
        corners[0] = gridOrigin;
        corners[1] = gridOrigin + new Vector3(outlineSize.x, 0, 0);
        corners[2] = gridOrigin + new Vector3(outlineSize.x, outlineSize.y, 0);
        corners[3] = gridOrigin + new Vector3(0, outlineSize.y, 0);
        corners[4] = gridOrigin + new Vector3(0, 0, outlineSize.z);
        corners[5] = gridOrigin + new Vector3(outlineSize.x, 0, outlineSize.z);
        corners[6] = gridOrigin + new Vector3(outlineSize.x, outlineSize.y, outlineSize.z);
        corners[7] = gridOrigin + new Vector3(0, outlineSize.y, outlineSize.z);

        // Draw edges of the grid box
        Gizmos.DrawLine(corners[0], corners[1]); // Bottom front edge
        Gizmos.DrawLine(corners[1], corners[2]); // Bottom right edge
        Gizmos.DrawLine(corners[2], corners[3]); // Bottom back edge
        Gizmos.DrawLine(corners[3], corners[0]); // Bottom left edge

        Gizmos.DrawLine(corners[4], corners[5]); // Top front edge
        Gizmos.DrawLine(corners[5], corners[6]); // Top right edge
        Gizmos.DrawLine(corners[6], corners[7]); // Top back edge
        Gizmos.DrawLine(corners[7], corners[4]); // Top left edge

        Gizmos.DrawLine(corners[0], corners[4]); // Front left vertical edge
        Gizmos.DrawLine(corners[1], corners[5]); // Front right vertical edge
        Gizmos.DrawLine(corners[2], corners[6]); // Back right vertical edge
        Gizmos.DrawLine(corners[3], corners[7]); // Back left vertical edge
    }
}
