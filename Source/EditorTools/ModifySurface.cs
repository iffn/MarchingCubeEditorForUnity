#if UNITY_EDITOR

using iffnsStuff.MarchingCubeEditor.Core;
using iffnsStuff.MarchingCubeEditor.EditTools;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ModifySurface : BaseTool
{
    // Editor variables
    bool raycastActive = true;
    bool showPreview = false;
    double nextUpdateTime;
    double timeBetweenUpdates = 1.0 / 60.0;
    int surfaceModification;

    float smoothThreshold = 0.5f;
    int smoothRadius = 3;
    float smoothSigma = 2f;

    int roughenRadius = 10;
    float roughenIntensity = 0.2f;
    float roughenFrequency = 0.1f;
    float roughenFalloffSharpness = 2f;

    // Internal variables
    PlaceableByClickHandler placeableByClick;

    // Override functions
    public override string DisplayName => "Modify surface";

    enum SurfaceOptions
    {
        smooth,
        roughen
    }

    // Base class functions
    public override void OnEnable()
    {
        base.OnEnable();

        if (placeableByClick == null) placeableByClick = new PlaceableByClickHandler(LinkedMarchingCubeController);
    }

    public override void OnDisable()
    {
        base.OnDisable();

        placeableByClick.SelectedEditShape.gameObject.SetActive(false);
    }

    public override void DrawUI()
    {
        base.DrawUI();

        placeableByClick.DrawEditorUI();

        raycastActive = EditorGUILayout.Toggle("Active", raycastActive);
        showPreview = EditorGUILayout.Toggle("Show preview", showPreview);

        surfaceModification = EditorGUILayout.Popup("Select Option", surfaceModification, new string[] {"Smooth", "Roughen"});

        switch (surfaceModification)
        {
            case 0:
                smoothThreshold = EditorGUILayout.FloatField("Threshold", smoothThreshold);
                smoothRadius = EditorGUILayout.IntField("Radius", smoothRadius);
                smoothSigma = EditorGUILayout.FloatField("Sigma", smoothSigma);
                break;
            case 1:
                roughenRadius = EditorGUILayout.IntField("Radius", roughenRadius);
                roughenIntensity = EditorGUILayout.FloatField("Intensity", roughenIntensity);
                roughenFrequency = EditorGUILayout.FloatField("Frequency", roughenFrequency);
                roughenFalloffSharpness = EditorGUILayout.FloatField("FalloffSharpness", roughenFalloffSharpness);
                break;
            default:
                break;
        }

        if (raycastActive)
        {
            string helpText = base.helpText;

            helpText += placeableByClick.SelectedEditShape.HelpText;

            EditorGUILayout.HelpBox(helpText, MessageType.None);

            placeableByClick.SelectedEditShape.DrawUI();
        }
    }

    public override void HandleSceneUpdate(Event e)
    {
        base.HandleSceneUpdate(e);

        if (!raycastActive) return;

        if(placeableByClick == null) return;

        placeableByClick.SelectedEditShape.HandleSceneUpdate(e);

        RayHitResult result = LinkedMarchingCubeEditor.RaycastAtMousePosition(e);

        if (result != RayHitResult.None)
        {
            placeableByClick.SelectedEditShape.transform.position = result.point;

            placeableByClick.SelectedEditShape.gameObject.SetActive(true);

            if (showPreview)
            {
                HandlePreviewUpdate(e);
            }
            else
            {
                if (LeftClickDownEvent(e))
                {
                    if (surfaceModification == 0)
                    {
                        LinkedMarchingCubeController.ModificationManager.ModifyData(
                            placeableByClick.SelectedEditShape,
                            GaussianSmoothingModification());
                    }
                    else if (surfaceModification == 1)
                    {
                        LinkedMarchingCubeController.ModificationManager.ModifyData(
                            placeableByClick.SelectedEditShape,
                            WorldSpaceRougheningModification());
                    }

                    e.Use();
                }
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
            placeableByClick.SelectedEditShape.gameObject.SetActive(false);
            e.Use();
            RefreshUI();
        }
    }

    VoxelData[,,] CalculateDataCopyBasedOnSelectedShape(int maxOffset)
    {
        (Vector3Int minGrid, Vector3Int maxGrid) = LinkedMarchingCubeController.ModificationManager.CalculateGridBoundsClamped(placeableByClick.SelectedEditShape);

        return GenerateVoxelDataCopy(LinkedMarchingCubeController, minGrid, maxGrid, maxOffset);
    }

    // Internal functions
    BaseModificationTools.IVoxelModifier GaussianSmoothingModification()
    {
        VoxelData[,,] currentDataCopy = CalculateDataCopyBasedOnSelectedShape(smoothRadius);

        return new BaseModificationTools.GaussianSmoothingModifier(
            currentDataCopy,
            smoothThreshold,
            smoothRadius,
            smoothSigma
            );
    }

    BaseModificationTools.IVoxelModifier WorldSpaceRougheningModification()
    {
        VoxelData[,,] currentDataCopy = CalculateDataCopyBasedOnSelectedShape(roughenRadius);

        return new BaseModificationTools.WorldSpaceRougheningModifier(
            currentDataCopy,
            roughenRadius,
            roughenIntensity,
            roughenFrequency,
            roughenFalloffSharpness,
            LinkedMarchingCubeController.transform.position,
            LinkedMarchingCubeController.transform.lossyScale.x
            );
    }

    void HandlePreviewUpdate(Event e)
    {
        if (EditorApplication.timeSinceStartup >= nextUpdateTime) //Only update once in a while
        {
            if (surfaceModification == 0)
            {
                LinkedMarchingCubeController.ModificationManager.ShowPreviewData(
                    placeableByClick.SelectedEditShape,
                    GaussianSmoothingModification());
            }
            else if (surfaceModification == 1)
            {
                LinkedMarchingCubeController.ModificationManager.ShowPreviewData(
                    placeableByClick.SelectedEditShape,
                    WorldSpaceRougheningModification());
            }

            placeableByClick.SelectedEditShape.gameObject.SetActive(false);

            nextUpdateTime = EditorApplication.timeSinceStartup + timeBetweenUpdates;
        }

        if (LeftClickDownEvent(e))
        {
            LinkedMarchingCubeController.ModificationManager.ApplyPreviewChanges();
            e.Use();
        }
    }
}

#endif