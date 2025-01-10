using iffnsStuff.MarchingCubeEditor.EditTools;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SmoothingTool : BaseTool
{
    // Editor variables
    EditShape selectedShape;
    float threshold = 0.5f;
    int radius = 3;
    float sigma = 2f;

    bool raycastActive = false;

    // Internal variables
    Vector3 originalShapePosition;

    // Override functions
    public override string DisplayName => "Click to smooth";

    public override void OnEnable()
    {
        if (selectedShape) originalShapePosition = selectedShape.transform.position;
    }

    public override void OnDisable()
    {
        if (selectedShape) selectedShape.transform.position = originalShapePosition;
    }

    public override void DrawUI()
    {
        //Handle shape assignment
        selectedShape = EditorGUILayout.ObjectField(
            selectedShape,
            typeof(EditShape),
            true) as EditShape;

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

        if(selectedShape == null) return;

        selectedShape.HandleSceneUpdate(e);

        RayHitResult result = LinkedMarchingCubeEditor.RaycastAtMousePosition(e);

        if (result != RayHitResult.None)
        {
            selectedShape.transform.position = result.point;

            selectedShape.gameObject.SetActive(true);

            if (LeftClickDownEvent(e))
            {
                LinkedMarchingCubeController.ModificationManager.ModifyData(
                    selectedShape,
                    Modification());

                e.Use();
            }
        }
        else
        {
            selectedShape.gameObject.SetActive(false);
            LinkedMarchingCubeController.DisplayPreviewShape = false;
        }

        if (EscapeDownEvent(e))
        {
            raycastActive = false;
            e.Use();
        }
    }
}
