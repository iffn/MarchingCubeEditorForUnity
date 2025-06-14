#if UNITY_EDITOR
using iffnsStuff.MarchingCubeEditor.Core;
using iffnsStuff.MarchingCubeEditor.EditTools;
using System;
using System.Threading.Tasks;
using UnityEngine;
using static BaseModificationTools;
using static MarchingCubesPreview;

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
        ModifyModel(shape, modifier, minGrid, maxGrid, linkedController.GetVoxelWithoutClamp, linkedController.SetDataPointWithoutSettingItToDirty); // Warning: In this case, get has access to the new data

        // Mark affected chunks as dirty
        linkedController.MarkRegionDirty(minGrid, maxGrid);

        // Update affected chunk meshes
        linkedController.UpdateAffectedChunks(minGrid, maxGrid);
    }

    public void SetPreviewDisplayState(MarchingCubesPreview.PreviewDisplayStates newState)
    {
       linkedController.Preview.SetPreviewDisplayState(newState);
    }

    public void ShowPreviewData(EditShape shape, IVoxelModifier modifier)
    {
        (Vector3Int minGrid, Vector3Int maxGrid) = CalculateGridBoundsClamped(shape);

        linkedController.SetupPreviewZone(minGrid, maxGrid);

        //Modify the model
        ModifyModel(shape, modifier, minGrid, maxGrid, linkedController.GetVoxelWithoutClamp, linkedController.SetPreviewDataPoint); // Warning: In this case, get has access to the old data

        linkedController.UpdatePreviewShape();
    }

    public void ApplyPreviewChanges()
    {
        linkedController.ApplyPreviewChanges(); //Takes care of setting stuff to dirty
    }

    public (Vector3Int minGrid, Vector3Int maxGrid) CalculateGridBoundsClamped(EditShape shape)
    {
        // Precompute transformation matrices
        Matrix4x4 worldToGrid = linkedControllerTransform.worldToLocalMatrix; // Transform world space to grid space

        // Precompute shape transformation
        shape.PrepareParameters(linkedControllerTransform); // Passing the transform allows using the grid points directly

        // Get shape bounds in world space and transform to grid space
        (Vector3 worldMin, Vector3 worldMax) = shape.GetWorldBoundingBox();
        Vector3 gridMin = worldToGrid.MultiplyPoint3x4(worldMin);
        Vector3 gridMax = worldToGrid.MultiplyPoint3x4(worldMax);

        // Ensure correct min/max per component in case of inverted bounds
        Vector3 gridLower = Vector3.Min(gridMin, gridMax);
        Vector3 gridUpper = Vector3.Max(gridMin, gridMax);

        // Expand bounds by Vector3.one due to rounding and clamp to valid grid range
        Vector3Int unclampedMin = Vector3Int.FloorToInt(gridLower) - Vector3Int.one;
        Vector3Int unclampedMax = Vector3Int.CeilToInt(gridUpper) + Vector3Int.one;

        Vector3Int minGrid = new Vector3Int(
            Mathf.Max(0, unclampedMin.x),
            Mathf.Max(0, unclampedMin.y),
            Mathf.Max(0, unclampedMin.z)
        );

        Vector3Int maxGrid = new Vector3Int(
            Mathf.Min(linkedController.MaxGrid.x, unclampedMax.x),
            Mathf.Min(linkedController.MaxGrid.y, unclampedMax.y),
            Mathf.Min(linkedController.MaxGrid.z, unclampedMax.z)
        );

        return (minGrid, maxGrid);
    }

    public void ModifySingleVoxel(int x, int y, int z, VoxelData newValue)
    {
        linkedController.VoxelDataReference[x,y, z] = newValue;

        Vector3Int point = new Vector3Int(x, y, z);

        linkedController.MarkRegionDirty(point);

        linkedController.UpdateAffectedChunks(point);
    }

    void ModifyModel(EditShape shape, IVoxelModifier modifier, Vector3Int minGrid, Vector3Int maxGrid, Func<int, int, int, VoxelData> getDataPoint, Action<int, int, int, VoxelData> setDataPoint)
    {
        float worldToGridScaleFactor = linkedControllerTransform.localScale.magnitude; //ToDo: Reimplement scaling

        // Parallel processing
        Parallel.For(minGrid.x, maxGrid.x + 1, x =>
        {
            for (int y = minGrid.y; y < maxGrid.y + 1; y++)
            {
                for (int z = minGrid.z; z < maxGrid.z + 1; z++)
                {
                    // Transform grid position to world space
                    Vector3 gridPoint = new Vector3(x, y, z);

                    // Calculate the distance using the shape's transformation
                    float distanceOutsideIsPositive = shape.OptimizedDistanceOutsideIsPositive(gridPoint); //Note: Since this transform was passed for the transformation matrix and each grid point has a size of 1, the grid point can be used directly.

                    // Modify the voxel value
                    VoxelData newValue = modifier.ModifyVoxel(x, y, z, linkedController.VoxelDataReference[x, y, z], distanceOutsideIsPositive);
                    setDataPoint(x, y, z, newValue);
                }
            }
        });
    }
}
#endif