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
    bool showPreview = true;
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

    public override void OnEnable()
    {
        if (placeableByClick == null) placeableByClick = new PlaceableByClickHandler(LinkedMarchingCubeController);
    }

    public override void OnDisable()
    {
        
    }

    enum SurfaceOptions
    {
        smooth,
        roughen
    }


    public override void DrawUI()
    {
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

    BaseModificationTools.IVoxelModifier GaussianSmoothingModification()
    {
        VoxelData[,,] currentDataCopy = GenerateVoxelDataCopy();

        return new BaseModificationTools.GaussianSmoothingModifier(
            currentDataCopy,
            smoothThreshold,
            smoothRadius,
            smoothSigma
            );
    }

    BaseModificationTools.IVoxelModifier WorldSpaceRougheningModification()
    {
        VoxelData[,,] currentDataCopy = GenerateVoxelDataCopy();

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
            e.Use();
        }
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