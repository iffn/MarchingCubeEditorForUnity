#if UNITY_EDITOR

using iffnsStuff.MarchingCubeEditor.EditTools;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static UnityEditor.Progress;

public class SimpleClickToModifyTool : BaseTool
{
    // Editor variables
    bool raycastActive = true;
    bool RaycastActive
    {
        set
        {
            placeableByClick.SelectedEditShape.gameObject.SetActive(false);
            LinkedMarchingCubeController.DisplayPreviewShape = false;
            LinkedMarchingCubeController.EnableAllColliders = value;
            raycastActive = value;
        }
    }

    PlaceableByClickHandler placeableByClick;
    bool displayPreviewShape;
    bool limitHeightToCursor;

    // Internal variables
    double nextUpdateTime;
    double timeBetweenUpdates = 1.0 / 60.0;

    public override string DisplayName => "Click to modify tool";
    
    bool ShouldSubtract(Event e)
    {
        return ControlIsHeld(e);
    }

    // Override functions
    public override void OnEnable()
    {
        if(placeableByClick == null) placeableByClick = new PlaceableByClickHandler(LinkedMarchingCubeController);
    }

    public override void OnDisable()
    {
        
    }

    public override void DrawUI()
    {
        //Handle shape assignment

        if(placeableByClick == null) return;

        placeableByClick.DrawEditorUI();

        //Settings
        bool newRaycastActive = EditorGUILayout.Toggle("Active", raycastActive);
        if(raycastActive != newRaycastActive)
        {
            RaycastActive = newRaycastActive;
        }

        bool newDisplayPreviewShape = EditorGUILayout.Toggle("Display preview shape", displayPreviewShape);
        if (displayPreviewShape != newDisplayPreviewShape)
        {
            placeableByClick.SelectedEditShape.gameObject.SetActive(false);
            LinkedMarchingCubeController.DisplayPreviewShape = false;
            displayPreviewShape = newDisplayPreviewShape;
        }

        limitHeightToCursor = EditorGUILayout.Toggle("Limit height to cursor", limitHeightToCursor);

        if (raycastActive)
        {
            string helpText = "Controls:\n" +
                    "Note that the scene has to be active for some of these to work.\n" +
                    "Click to add\n" +
                    "Ctrl Click to subtract\n";

            helpText += placeableByClick.SelectedEditShape.HelpText;

            EditorGUILayout.HelpBox(helpText, MessageType.None);

            placeableByClick.SelectedEditShape.DrawUI();
        }
    }

    public override void HandleSceneUpdate(Event e)
    {
        if (!raycastActive) return;

        RayHitResult result = LinkedMarchingCubeEditor.RaycastAtMousePosition(e);

        if (result != RayHitResult.None)
        {
            placeableByClick.SelectedEditShape.transform.position = result.point;

            if (displayPreviewShape)
            {
                HandlePreviewUpdate(e);
                LinkedMarchingCubeController.DisplayPreviewShape = true;
            }
            else
            {
                HandleDirectUpdate(e);
                placeableByClick.SelectedEditShape.gameObject.SetActive(true);
            }
        }
        else
        {
            placeableByClick.SelectedEditShape.gameObject.SetActive(false);
            LinkedMarchingCubeController.DisplayPreviewShape = false;
        }

        if (placeableByClick.SelectedShape != null) placeableByClick.SelectedEditShape.HandleSceneUpdate(e);

        if (EscapeDownEvent(e))
        {
            RaycastActive = false;
            e.Use();
            return;
        }
    }

    public override void DrawGizmos()
    {

    }

    // Internal functions
    delegate void ModifyData(EditShape shape, BaseModificationTools.IVoxelModifier modifier);

    BaseModificationTools.IVoxelModifier Modification(Event e)
    {
        bool subtract = ShouldSubtract(e);

        BaseModificationTools.IVoxelModifier modifier;

        if (limitHeightToCursor)
        {
            if (subtract)
            {
                modifier = new BaseModificationTools.ModifyShapeWithMaxHeightModifier(
                        placeableByClick.SelectedEditShape.transform.position.y,
                        BaseModificationTools.ModifyShapeWithMaxHeightModifier.BooleanType.SubtractOnly);
            }
            else
            {
                modifier = new BaseModificationTools.ModifyShapeWithMaxHeightModifier(
                        placeableByClick.SelectedEditShape.transform.position.y,
                        BaseModificationTools.ModifyShapeWithMaxHeightModifier.BooleanType.AddOnly);
            }
        }
        else
        {
            if (subtract)
            {
                modifier = new BaseModificationTools.SubtractShapeModifier();
            }
            else
            {
                modifier = new BaseModificationTools.AddShapeModifier();
            }
        }

        return modifier;
    }

    void HandleDirectUpdate(Event e)
    {
        placeableByClick.SelectedEditShape.gameObject.SetActive(true);
        //selectedShape.Color = e.control ? subtractionColor : additionColor;

        if (LeftClickDownEvent(e))
        {
            LinkedMarchingCubeController.ModificationManager.ModifyData(placeableByClick.SelectedEditShape, Modification(e));

            e.Use();
            return;
        }
    }

    void HandlePreviewUpdate(Event e)
    {
        if (EditorApplication.timeSinceStartup >= nextUpdateTime) //Only update once in a while
        {
            if (ShouldSubtract(e))
                LinkedMarchingCubeController.ModificationManager.SetPreviewDisplayState(MarchingCubesPreview.PreviewDisplayStates.subtraction);
            else
                LinkedMarchingCubeController.ModificationManager.SetPreviewDisplayState(MarchingCubesPreview.PreviewDisplayStates.addition);

            LinkedMarchingCubeController.ModificationManager.ShowPreviewData(placeableByClick.SelectedEditShape, Modification(e));

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