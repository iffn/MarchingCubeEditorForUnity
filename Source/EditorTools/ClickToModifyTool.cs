#if UNITY_EDITOR

using iffnsStuff.MarchingCubeEditor.EditTools;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ClickToModifyTool : BaseTool
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

    KeyCode offsetKey = KeyCode.C;
    bool offsetKeyHeld = false;
    float currentOffset = 0;
    float offsetSpeed = 0.1f;

    PlaceableByClickHandler placeableByClick;
    bool displayPreviewShape;
    bool limitHeightToCursor;

    // Internal variables
    double nextUpdateTime;
    double timeBetweenUpdates = 1.0 / 60.0;
    Vector3 prevShapePoint = Vector3.zero;
    Vector3 prevImpactPoint = Vector3.zero;

    public override string DisplayName => "Click to modify tool";
    
    bool ShouldSubtract(Event e)
    {
        return ControlIsHeld(e);
    }

    // Base class functions
    public override void OnEnable()
    {
        base.OnEnable();

        if(placeableByClick == null) placeableByClick = new PlaceableByClickHandler(LinkedMarchingCubeController);
        currentOffset = 0;
    }

    public override void OnDisable()
    {
        base.OnDisable();

        placeableByClick.SelectedEditShape.gameObject.SetActive(false);
    }

    public override void DrawUI()
    {
        base.DrawUI();

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

        DrawTransformFields(placeableByClick.SelectedEditShape.transform);

        if (GUILayout.Button("Reset offset"))
            currentOffset = 0;

        if (raycastActive)
        {
            string helpText = base.helpText +
                    "• Click to add\n" +
                    "• Ctrl Click to subtract\n" +
                    $"• Hold {offsetKey} and scroll to change the offset. Currently: {currentOffset.ToString("F1")}"; // "F1" keeps one decimal place

            helpText += placeableByClick.SelectedEditShape.HelpText;

            EditorGUILayout.HelpBox(helpText, MessageType.None);

            placeableByClick.SelectedEditShape.DrawUI();
        }
    }

    public override void HandleSceneUpdate(Event e)
    {
        base.HandleSceneUpdate(e);

        if (!raycastActive) return;

        RayHitResult result = LinkedMarchingCubeEditor.RaycastAtMousePosition(e);

        if (result != RayHitResult.None)
        {
            Vector3 offsetDirection;

            switch (placeableByClick.SelectedEditShape.offsetType)
            {
                case EditShape.OffsetTypes.vertical:
                    offsetDirection = Vector3.up;
                    break;
                case EditShape.OffsetTypes.towardsNormal:
                    offsetDirection = result.normal;
                    break;
                default:
                    offsetDirection = result.normal;
                    break;
            }

            prevImpactPoint = result.point;

            placeableByClick.SelectedEditShape.transform.position = result.point + currentOffset * offsetDirection;

            prevShapePoint = placeableByClick.SelectedEditShape.transform.position;

            RefreshUI();

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

        if (e.keyCode == offsetKey)
        {
            if(e.type == EventType.KeyDown)
                offsetKeyHeld = true;

            if (e.type == EventType.KeyUp)
                offsetKeyHeld = false;
        }

        if (offsetKeyHeld && e.type == EventType.ScrollWheel)
        {
            currentOffset += offsetSpeed * Mathf.Sign(e.delta.y);
            RefreshUI();
            e.Use();
        }
        else
        {
            if (placeableByClick.SelectedShape != null)
                placeableByClick.SelectedEditShape.HandleSceneUpdate(e);
        }

        if (EscapeDownEvent(e))
        {
            RaycastActive = false;
            currentOffset = 0;
            RefreshUI();
            e.Use();
            return;
        }
    }

    public override void DrawGizmos()
    {
        base.DrawGizmos();

        if(!Mathf.Approximately(currentOffset, 0) && (placeableByClick.SelectedEditShape.transform.position - prevShapePoint).magnitude < 0.1f)
        {
            Color prevColor = Handles.color;

            Handles.zTest = UnityEngine.Rendering.CompareFunction.Always;
            Handles.color = Color.white;

            Handles.DrawAAPolyLine(
                2f, // Thickness
                placeableByClick.SelectedEditShape.transform.position,
                prevImpactPoint
            );

            Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;

            Handles.color = prevColor;
        }
    }

    // Internal functions
    delegate void ModifyData(EditShape shape, BaseModificationTools.IVoxelModifier modifier);

    BaseModificationTools.IVoxelModifier Modification(Event e)
    {
        bool subtract = ShouldSubtract(e);

        BaseModificationTools.IVoxelModifier modifier;

        if (limitHeightToCursor)
        {
            float localHeihgt = LinkedMarchingCubeController.transform.InverseTransformPoint(
                placeableByClick.SelectedEditShape.transform.position).y;

            if (subtract)
            {
                modifier = new BaseModificationTools.ModifyShapeWithMaxHeightModifier(
                        localHeihgt,
                        BaseModificationTools.ModifyShapeWithMaxHeightModifier.BooleanType.SubtractOnly);
            }
            else
            {
                modifier = new BaseModificationTools.ModifyShapeWithMaxHeightModifier(
                        localHeihgt,
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