using iffnsStuff.MarchingCubeEditor.EditTools;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEditor;
using UnityEngine;

public class TerrainConverter : BaseTool
{
    PlaceableByClickHandler currentEditShapeHandler;
    Terrain selectedTerrain;

    public override string DisplayName => "Terrain converter";

    public override void DrawUI()
    {
        base.DrawUI();

        if (currentEditShapeHandler == null) return;
        currentEditShapeHandler.DrawEditorUI();

        selectedTerrain = EditorGUILayout.ObjectField(
               obj: selectedTerrain,
               objType: typeof(Terrain),
               true) as Terrain;

        if (GUILayout.Button($"Apply data from terrain"))
            ApplyDataFromTerrain();
    }

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

    void ApplyDataFromTerrain()
    {
        Matrix4x4 controllerTransformWTL = LinkedMarchingCubeController.transform.worldToLocalMatrix;

        BaseModificationTools.TerrainConverter converter = new BaseModificationTools.TerrainConverter(selectedTerrain, controllerTransformWTL);

        LinkedMarchingCubeController.ModificationManager.ModifyData(currentEditShapeHandler.SelectedEditShape, converter);
    }
}
