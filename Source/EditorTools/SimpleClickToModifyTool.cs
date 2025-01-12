#if UNITY_EDITOR

using iffnsStuff.MarchingCubeEditor.EditTools;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static UnityEditor.Progress;

public class SimpleClickToModifyTool : BaseTool
{
    // Editor variables
    IPlaceableByClick selectedShape;
    bool raycastActive;
    bool RaycastActive
    {
        set
        {
            selectedShape.AsEditShape.gameObject.SetActive(false);
            LinkedMarchingCubeController.DisplayPreviewShape = false;
            LinkedMarchingCubeController.EnableAllColliders = value;
            raycastActive = value;
        }
    }

    bool displayPreviewShape;

    bool limitHeightToCursor;

    // Internal variables
    Vector3 originalShapePosition;
    double nextUpdateTime;
    double timeBetweenUpdates = 1.0 / 60.0;

    List<IPlaceableByClick> EditShapes = new List<IPlaceableByClick>();
    string[] EditShapeNames;
    int selectedIndex;

    public override string DisplayName => "Click to modify tool";

    // Override functions
    public override void OnEnable()
    {
        if(selectedShape != null) originalShapePosition = selectedShape.AsEditShape.transform.position;

        // Create shape list
        List<EditShape> shapes = LinkedMarchingCubeController.ShapeList;

        EditShapes.Clear();
        

        foreach (EditShape shape in shapes)
        {
            if (shape is IPlaceableByClick clickableShape)
            {
                EditShapes.Add(clickableShape);
            }
        }

        EditShapeNames = new string[EditShapes.Count];

        for(int i = 0; i < EditShapes.Count; i++)
        {
            EditShapeNames[i] = EditShapes[i].AsEditShape.transform.name;
        }
    }

    public override void OnDisable()
    {
        if (selectedShape != null) selectedShape.AsEditShape.transform.position = originalShapePosition;
    }

    public override void DrawUI()
    {
        //Handle shape assignment

        if(EditShapes.Count == 0) return;

        int newSelectedIndex = EditorGUILayout.Popup("Select Option", selectedIndex, EditShapeNames);

        if(newSelectedIndex != selectedIndex)
        {
            RestoreShapePositionIfAble();
            selectedIndex = newSelectedIndex;
            selectedShape = EditShapes[selectedIndex];
            SaveShapePositionIfAble();
            selectedShape.AsEditShape.Initialize();
        }

        //Settings
        bool newRaycastActive = EditorGUILayout.Toggle("Active", raycastActive);
        if(raycastActive != newRaycastActive)
        {
            RaycastActive = newRaycastActive;
        }

        bool newDisplayPreviewShape = EditorGUILayout.Toggle("Display preview shape", displayPreviewShape);
        if (displayPreviewShape != newDisplayPreviewShape)
        {
            selectedShape.AsEditShape.gameObject.SetActive(false);
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

            helpText += selectedShape.AsEditShape.HelpText;

            EditorGUILayout.HelpBox(helpText, MessageType.None);

            selectedShape.AsEditShape.DrawUI();
        }
    }

    public override void HandleSceneUpdate(Event e)
    {
        if (!raycastActive) return;

        RayHitResult result = LinkedMarchingCubeEditor.RaycastAtMousePosition(e);

        if (result != RayHitResult.None)
        {
            selectedShape.AsEditShape.transform.position = result.point;

            if (displayPreviewShape)
            {
                HandlePreviewUpdate(e);
                LinkedMarchingCubeController.DisplayPreviewShape = true;
            }
            else
            {
                HandleDirectUpdate(e);
                selectedShape.AsEditShape.gameObject.SetActive(true);
            }
        }
        else
        {
            selectedShape.AsEditShape.gameObject.SetActive(false);
            LinkedMarchingCubeController.DisplayPreviewShape = false;
        }

        if (selectedShape != null) selectedShape.AsEditShape.HandleSceneUpdate(e);

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
        bool subtract = ControlIsHeld(e);

        BaseModificationTools.IVoxelModifier modifier;

        if (limitHeightToCursor)
        {
            if (subtract)
            {
                modifier = new BaseModificationTools.ModifyShapeWithMaxHeightModifier(
                        selectedShape.AsEditShape.transform.position.y,
                        BaseModificationTools.ModifyShapeWithMaxHeightModifier.BooleanType.SubtractOnly);
            }
            else
            {
                modifier = new BaseModificationTools.ModifyShapeWithMaxHeightModifier(
                        selectedShape.AsEditShape.transform.position.y,
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
        selectedShape.AsEditShape.gameObject.SetActive(true);
        //selectedShape.Color = e.control ? subtractionColor : additionColor;

        if (LeftClickDownEvent(e))
        {
            LinkedMarchingCubeController.ModificationManager.ModifyData(selectedShape.AsEditShape, Modification(e));

            e.Use();
            return;
        }
    }

    void HandlePreviewUpdate(Event e)
    {
        if (EditorApplication.timeSinceStartup >= nextUpdateTime) //Only update once in a while
        {
            LinkedMarchingCubeController.ModificationManager.ShowPreviewData(selectedShape.AsEditShape, Modification(e)); 

            selectedShape.AsEditShape.gameObject.SetActive(false);

            nextUpdateTime = EditorApplication.timeSinceStartup + timeBetweenUpdates;
        }

        if (LeftClickDownEvent(e))
        {
            LinkedMarchingCubeController.ModificationManager.ApplyPreviewChanges();
            e.Use();
        }
    }

    void SaveShapePositionIfAble()
    {
        if (selectedShape != null) originalShapePosition = selectedShape.AsEditShape.transform.position;
    }

    void RestoreShapePositionIfAble()
    {
        if (selectedShape != null) selectedShape.AsEditShape.transform.position = originalShapePosition;
    }
}

#endif