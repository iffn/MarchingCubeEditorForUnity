using iffnsStuff.MarchingCubeEditor.EditTools;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SmoothingTool : BaseTool
{
    // Editor variables
    float threshold = 0.5f;
    int radius = 3;
    float sigma = 2f;

    bool raycastActive = false;

    // Internal variables
    PlaceableByClickHandler PlaceableByClick;

    // Override functions
    public override string DisplayName => "Click to smooth";

    public override void OnEnable()
    {
        if (PlaceableByClick == null) PlaceableByClick = new PlaceableByClickHandler(LinkedMarchingCubeController);
    }

    public override void OnDisable()
    {
        
    }

    public override void DrawUI()
    {
        PlaceableByClick.DrawEditorUI();

        raycastActive = EditorGUILayout.Toggle("Active", raycastActive);

        threshold = EditorGUILayout.FloatField("Threshold", threshold);
        radius = EditorGUILayout.IntField("Radius", radius);
        sigma = EditorGUILayout.FloatField("Sigma", sigma);
    }

    BaseModificationTools.IVoxelModifier Modification()
    {
        return new BaseModificationTools.GaussianSmoothingModifier(
            LinkedMarchingCubeController.VoxelDataReference,
            threshold,
            radius,
            sigma);
    }

    public override void HandleSceneUpdate(Event e)
    {
        if (!raycastActive) return;

        if(PlaceableByClick == null) return;

        PlaceableByClick.SelectedEditShape.HandleSceneUpdate(e);

        RayHitResult result = LinkedMarchingCubeEditor.RaycastAtMousePosition(e);

        if (result != RayHitResult.None)
        {
            PlaceableByClick.SelectedEditShape.transform.position = result.point;

            PlaceableByClick.SelectedEditShape.gameObject.SetActive(true);

            if (LeftClickDownEvent(e))
            {
                LinkedMarchingCubeController.ModificationManager.ModifyData(
                    PlaceableByClick.SelectedEditShape,
                    Modification());

                e.Use();
            }
        }
        else
        {
            PlaceableByClick.SelectedEditShape.gameObject.SetActive(false);
            LinkedMarchingCubeController.DisplayPreviewShape = false;
        }

        if (EscapeDownEvent(e))
        {
            raycastActive = false;
            e.Use();
        }
    }
}
