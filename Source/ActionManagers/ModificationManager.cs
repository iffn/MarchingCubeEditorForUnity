using iffnsStuff.MarchingCubeEditor.Core;
using iffnsStuff.MarchingCubeEditor.EditTools;
using System;
using UnityEngine;
using static BaseModificationTools;

public class ModificationManager
{
    // ToDo: Fix unable to add preview shape
    // ToDo: Implement max height

    MarchingCubesController linkedController;
    Transform linkedControllerTransform;

    public ModificationManager(MarchingCubesController linkedController)
    {
        this.linkedController = linkedController;
        linkedControllerTransform = linkedController.transform;
    }

    public void ModifyData(EditShape shape, IVoxelModifier modifier)
    {
        (Vector3Int minGrid, Vector3Int maxGrid) = CalculateGridBoundsClamped(shape);

        // Modify model
        ModifyModel(shape, modifier, minGrid, maxGrid, linkedController.GetDataPoint, linkedController.SetDataPointWithoutSettingItToDirty); // Warning: In this case, get has access to the new data

        // Mark affected chunks as dirty
        linkedController.MarkRegionDirty(minGrid, maxGrid);

        // Update affected chunk meshes
        linkedController.UpdateAffectedChunks(minGrid, maxGrid);
    }

    public void ShowPreviewData(EditShape shape, IVoxelModifier modifier)
    {
        (Vector3Int minGrid, Vector3Int maxGrid) = CalculateGridBoundsClamped(shape);

        linkedController.SetupPreviewZone(minGrid, maxGrid);

        //Modify the model
        ModifyModel(shape, modifier, minGrid, maxGrid, linkedController.GetDataPoint, linkedController.SetPreviewDataPoint); // Warning: In this case, get has access to the old data

        linkedController.UpdatePreviewShape();
    }

    public void ApplyPreviewChanges()
    {
        linkedController.ApplyPreviewChanges(); //Takes care of setting stuff to dirty
    }

    private (Vector3Int minGrid, Vector3Int maxGrid) CalculateGridBoundsClamped(EditShape shape)
    {
        // Precompute transformation matrices
        Matrix4x4 worldToGrid = linkedControllerTransform.worldToLocalMatrix; // Transform world space to grid space

        // Precompute shape transformation
        shape.PrepareParameters(linkedControllerTransform); // Passing the transform allows using the grid points directly

        // Get shape bounds in world space and transform to grid space
        (Vector3 worldMin, Vector3 worldMax) = shape.GetWorldBoundingBox();
        Vector3 gridMin = worldToGrid.MultiplyPoint3x4(worldMin);
        Vector3 gridMax = worldToGrid.MultiplyPoint3x4(worldMax);

        // Expand bounds by Vector3.one due to rounding and clamp to valid grid range
        Vector3Int minGrid = Vector3Int.Max(Vector3Int.zero, Vector3Int.FloorToInt(gridMin) - Vector3Int.one);
        Vector3Int maxGrid = Vector3Int.Min(Vector3Int.CeilToInt(gridMax) + Vector3Int.one, linkedController.MaxGrid);

        return (minGrid, maxGrid);
    }

    void ModifyModel(EditShape shape, IVoxelModifier modifier, Vector3Int minGrid, Vector3Int maxGrid, Func<int, int, int, VoxelData> getDataPoint, Action<int, int, int, VoxelData> setDataPoint)
    {
        float worldToGridScaleFactor = linkedControllerTransform.localScale.magnitude; //ToDo: Reimplement scaling

        // Parallel processing
        System.Threading.Tasks.Parallel.For(minGrid.x, maxGrid.x, x =>
        {
            for (int y = minGrid.y; y < maxGrid.y; y++)
            {
                for (int z = minGrid.z; z < maxGrid.z; z++)
                {
                    // Transform grid position to world space
                    Vector3 gridPoint = new Vector3(x, y, z);

                    // Calculate the distance using the shape's transformation
                    float distance = shape.OptimizedDistance(gridPoint); //Note: Since this transform was passed for the transformation matrix and each grid point has a size of 1, the grid point can be used directly.

                    // Modify the voxel value
                    VoxelData currentValue = getDataPoint(x, y, z);
                    VoxelData newValue = modifier.ModifyVoxel(x, y, z, currentValue, distance);
                    setDataPoint(x, y, z, newValue);
                }
            }
        });
    }
}
