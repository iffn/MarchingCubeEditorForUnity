#if UNITY_EDITOR

using iffnsStuff.MarchingCubeEditor.EditTools;
using iffnsStuff.MarchingCubeEditor.SceneEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TerrainConverter : BaseTool
{
    PlaceableByClickHandler currentEditShapeHandler;
    Terrain selectedTerrain;

    public override string DisplayName => "Terrain converter";

    public TerrainConverter(MarchingCubeEditor editor) : base(editor)
    {
        // Constructor
    }

    // Base class functions
    public override void OnEnable()
    {
        base.OnEnable();

        if (currentEditShapeHandler == null)
            currentEditShapeHandler = new PlaceableByClickHandler(LinkedMarchingCubeController);
    }

    public override void OnDisable()
    {
        base.OnDisable();

        if (currentEditShapeHandler != null)
            currentEditShapeHandler.SelectedEditShape.gameObject.SetActive(false);
    }

    protected override void GeneratePersistentUI()
    {

    }

    public override void DrawUI()
    {
        base.DrawUI();

        if (currentEditShapeHandler == null) return;
        currentEditShapeHandler.DrawEditorUI();

        selectedTerrain = EditorGUILayout.ObjectField(
               obj: selectedTerrain,
               objType: typeof(Terrain),
               true) as Terrain;

        if (GUILayout.Button($"Make shape fill the entire controller"))
            FillEntireGridWithShape();

        if (GUILayout.Button($"Apply data from terrain in the shape region"))
            ApplyDataFromTerrain();
    }

    // Internal functions
    void FillEntireGridWithShape()
    {
        Transform controllerTransform = LinkedMarchingCubeController.transform;
        Transform selectedShapeTransform = currentEditShapeHandler.SelectedEditShape.transform;
        Vector3 gridSize = new Vector3(LinkedMarchingCubeController.GridResolutionX, LinkedMarchingCubeController.GridResolutionY, LinkedMarchingCubeController.GridResolutionZ) - Vector3.one;

        selectedShapeTransform.SetLocalPositionAndRotation(0.5f * gridSize, Quaternion.identity);

        selectedShapeTransform.localScale = gridSize;
    }

    void ApplyDataFromTerrain()
    {
        Matrix4x4 controllerTransformWTL = LinkedMarchingCubeController.transform.worldToLocalMatrix;

        Matrix4x4 controllerTransformLTW = controllerTransformWTL.inverse;

        float[,] heights = new float[LinkedMarchingCubeController.VoxelDataReference.GetLength(0), LinkedMarchingCubeController.VoxelDataReference.GetLength(2)];

        (Vector3Int minGrid, Vector3Int maxGrid) = LinkedMarchingCubeController.ModificationManager.CalculateGridBoundsClamped(currentEditShapeHandler.SelectedEditShape);

        for(int x = minGrid.x; x < maxGrid.x; x++)
        {
            for(int z = minGrid.z; z < maxGrid.z; z++)
            {
                Vector3 samplePositionLocal = new Vector3(x, 0, z);
                Vector3 samplePositionWorld = controllerTransformLTW.MultiplyPoint3x4(samplePositionLocal);

                float heightWorld = selectedTerrain.SampleHeight(samplePositionWorld) + selectedTerrain.transform.position.y; // Can only be called on the main thread

                Vector3 heightPositionWorld = new Vector3(samplePositionWorld.x, heightWorld, samplePositionWorld.z);

                Vector3 heightPositionLocal = controllerTransformWTL.MultiplyPoint3x4(heightPositionWorld);

                heights[x, z] = heightPositionLocal.y;
            }
        }

        BaseModificationTools.TerrainConverter converter = new BaseModificationTools.TerrainConverter(heights);

        LinkedMarchingCubeController.ModificationManager.ModifyData(currentEditShapeHandler.SelectedEditShape, converter);
    }
}

#endif