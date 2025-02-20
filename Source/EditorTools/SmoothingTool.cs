using iffnsStuff.MarchingCubeEditor.EditTools;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SmoothingTool : BaseTool
{
    // Editor variables
    bool raycastActive = true;
    bool smooth = false;

    float smoothThreshold = 0.5f;
    int smoothRadius = 3;
    float smoothSigma = 2f;

    float weightThreshold = 0.5f;
    int radius = 10;
    float intensity = 0.2f;
    float frequency = 0.1f;
    float falloffSharpness = 2f;

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

        smooth = EditorGUILayout.Toggle("Smooth", smooth);

        if (smooth)
        {
            smoothThreshold = EditorGUILayout.FloatField("Threshold", smoothThreshold);
            smoothRadius = EditorGUILayout.IntField("Radius", smoothRadius);
            smoothSigma = EditorGUILayout.FloatField("Sigma", smoothSigma);
        }
        else
        {
            weightThreshold = EditorGUILayout.FloatField("Weight threshold", weightThreshold);
            radius = EditorGUILayout.IntField("Radius", smoothRadius);
            intensity = EditorGUILayout.FloatField("Intensity", intensity);
            frequency = EditorGUILayout.FloatField("Frequency", frequency);
            falloffSharpness = EditorGUILayout.FloatField("FalloffSharpness", falloffSharpness);
        }
    }

    BaseModificationTools.IVoxelModifier GaussianSmoothingModification()
    {
        return new BaseModificationTools.GaussianSmoothingModifier(
            LinkedMarchingCubeController.VoxelDataReference,
            smoothThreshold,
            smoothRadius,
            smoothSigma
            );
    }

    BaseModificationTools.IVoxelModifier WorldSpaceRougheningModification()
    {
        return new BaseModificationTools.WorldSpaceRougheningModifier(
            LinkedMarchingCubeController.VoxelDataReference,
            weightThreshold,
            radius,
            intensity,
            frequency,
            falloffSharpness,
            LinkedMarchingCubeController.transform.position,
            LinkedMarchingCubeController.transform.lossyScale.x
            );
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
                if (smooth)
                {
                    LinkedMarchingCubeController.ModificationManager.ModifyData(
                        PlaceableByClick.SelectedEditShape,
                        GaussianSmoothingModification());
                }
                else
                {
                    LinkedMarchingCubeController.ModificationManager.ModifyData(
                        PlaceableByClick.SelectedEditShape,
                        WorldSpaceRougheningModification());
                }

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
