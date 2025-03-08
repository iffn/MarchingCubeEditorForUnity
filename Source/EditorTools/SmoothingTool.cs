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

    float roughenWeightThreshold = 0.5f;
    int roughenRadius = 10;
    float roughenIntensity = 0.2f;
    float roughenFrequency = 0.1f;
    float roughenFalloffSharpness = 2f;

    // Internal variables
    PlaceableByClickHandler placeableByClick;

    // Override functions
    public override string DisplayName => "Click to smooth";

    public override void OnEnable()
    {
        if (placeableByClick == null) placeableByClick = new PlaceableByClickHandler(LinkedMarchingCubeController);
    }

    public override void OnDisable()
    {
        
    }

    public override void DrawUI()
    {
        placeableByClick.DrawEditorUI();

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
            roughenWeightThreshold = EditorGUILayout.FloatField("Weight threshold", roughenWeightThreshold);
            roughenRadius = EditorGUILayout.IntField("Radius", roughenRadius);
            roughenIntensity = EditorGUILayout.FloatField("Intensity", roughenIntensity);
            roughenFrequency = EditorGUILayout.FloatField("Frequency", roughenFrequency);
            roughenFalloffSharpness = EditorGUILayout.FloatField("FalloffSharpness", roughenFalloffSharpness);
        }

        if (raycastActive)
        {
            string helpText = "Controls:\n" +
                    "Note that the scene has to be active for some of these to work.\n";

            helpText += placeableByClick.SelectedEditShape.HelpText;

            EditorGUILayout.HelpBox(helpText, MessageType.None);

            placeableByClick.SelectedEditShape.DrawUI();
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
            roughenWeightThreshold,
            roughenRadius,
            roughenIntensity,
            roughenFrequency,
            roughenFalloffSharpness,
            LinkedMarchingCubeController.transform.position,
            LinkedMarchingCubeController.transform.lossyScale.x
            );
    }

    public override void HandleSceneUpdate(Event e)
    {
        if (!raycastActive) return;

        if(placeableByClick == null) return;

        placeableByClick.SelectedEditShape.HandleSceneUpdate(e);

        RayHitResult result = LinkedMarchingCubeEditor.RaycastAtMousePosition(e);

        if (result != RayHitResult.None)
        {
            placeableByClick.SelectedEditShape.transform.position = result.point;

            placeableByClick.SelectedEditShape.gameObject.SetActive(true);

            if (LeftClickDownEvent(e))
            {
                if (smooth)
                {
                    LinkedMarchingCubeController.ModificationManager.ModifyData(
                        placeableByClick.SelectedEditShape,
                        GaussianSmoothingModification());
                }
                else
                {
                    LinkedMarchingCubeController.ModificationManager.ModifyData(
                        placeableByClick.SelectedEditShape,
                        WorldSpaceRougheningModification());
                }

                e.Use();
            }
        }
        else
        {
            placeableByClick.SelectedEditShape.gameObject.SetActive(false);
            LinkedMarchingCubeController.DisplayPreviewShape = false;
        }

        if (EscapeDownEvent(e))
        {
            raycastActive = false;
            e.Use();
        }
    }
}
