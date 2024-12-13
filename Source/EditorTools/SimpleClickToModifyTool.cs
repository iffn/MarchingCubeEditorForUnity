using iffnsStuff.MarchingCubeEditor.Core;
using iffnsStuff.MarchingCubeEditor.EditTools;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SimpleClickToModifyTool : BaseTool
{
    EditShape selectedShape;
    bool raycastActive;
    bool displayPreviewShape;
    Vector3 originalShapePosition;

    public override string displayName
    {
        get
        {
            return "Click to modify tool";
        }
    }

    public SimpleClickToModifyTool(MarchingCubesController linkedController) : base(linkedController)
    {

    }

    public override void OnEnable()
    {
        if(selectedShape) originalShapePosition = selectedShape.transform.position;
    }

    public override void OnDisable()
    {
        if (selectedShape) selectedShape.transform.position = originalShapePosition;
    }

    public override void DrawUI()
    {
        //Handle shape assignment
        EditShape newSelectedShape = EditorGUILayout.ObjectField(
            selectedShape,
            typeof(EditShape),
            true) as EditShape;

        if(newSelectedShape && newSelectedShape != selectedShape)
        {
            RestoreShapePositionIfAble();
            selectedShape = newSelectedShape;
            SaveShapePositionIfAble();
        }

        if (!newSelectedShape) RestoreShapePositionIfAble();

        if (!selectedShape) return;

        //Settings
        bool newRaycastActive = EditorGUILayout.Toggle("Active", raycastActive);
        if(raycastActive != newRaycastActive)
        {
            selectedShape.gameObject.SetActive(false);
            linkedMarchingCubesController.DisplayPreviewShape = false;
            linkedMarchingCubesController.EnableAllColliders = newRaycastActive;
            raycastActive = newRaycastActive;
        }

        bool newDisplayPreviewShape = EditorGUILayout.Toggle("Display preview shape", displayPreviewShape);
        if (displayPreviewShape != newDisplayPreviewShape)
        {
            selectedShape.gameObject.SetActive(false);
            linkedMarchingCubesController.DisplayPreviewShape = false;
            displayPreviewShape = newDisplayPreviewShape;
        }

        if (raycastActive)
        {
            EditorGUILayout.HelpBox("Controls:\n" +
                    "Click to add\n" +
                    "Ctrl Click to subtract\n" +
                    "Shift Scroll to scale", MessageType.None);
        }
    }

    public override void HandleSceneUpdate(Event e)
    {
        if (!raycastActive) return;

        RayHitResult result = RaycastAtMousePosition(e);

        if(result != RayHitResult.None)
        {
            selectedShape.transform.position = result.point;

            if (displayPreviewShape)
            {
                HandlePreviewUpdate(e);
                linkedMarchingCubesController.DisplayPreviewShape = true;
            }
            else
            {
                HandleDirectUpdate(e);
                selectedShape.gameObject.SetActive(true);
            }
        }
        else
        {
            selectedShape.gameObject.SetActive(false);
            linkedMarchingCubesController.DisplayPreviewShape = false;
        }

        if (e.shift && e.type == EventType.ScrollWheel)
        {
            float scaleDelta = e.delta.x * -0.03f; // Scale factor; reverse direction if needed

            selectedShape.transform.localScale *= (scaleDelta + 1);

            e.Use(); // Mark event as handled
        }
    }

    public override void DrawGizmos()
    {

    }

    double nextUpdateTime;
    double timeBetweenUpdates = 1.0 / 60.0;

    void HandleDirectUpdate(Event e)
    {
        selectedShape.gameObject.SetActive(true);
        //selectedShape.Color = e.control ? subtractionColor : additionColor;

        if (e.type == EventType.MouseDown && e.button == 0) // Left-click event
        {
            if (e.control) linkedMarchingCubesController.ModificationManager.ModifyData(selectedShape, new BaseModificationTools.SubtractShapeModifier());
            else
            {
                linkedMarchingCubesController.ModificationManager.ModifyData(selectedShape, new BaseModificationTools.AddShapeModifier());
                /*
                if (limitMaxHeight) linkedMarchingCubesController.AddShapeWithMaxHeight(selectedShape, hit.point.y, true);
                else linkedMarchingCubesController.AddShape(selectedShape, true);
                */
            }

            e.Use();
            return;
        }
    }

    void HandlePreviewUpdate(Event e)
    {
        if (EditorApplication.timeSinceStartup >= nextUpdateTime) //Only update once in a while
        {
            if (e.control) linkedMarchingCubesController.ModificationManager.ShowPreviewData(selectedShape, new BaseModificationTools.SubtractShapeModifier());
            else
            {
                linkedMarchingCubesController.ModificationManager.ShowPreviewData(selectedShape, new BaseModificationTools.AddShapeModifier());
                /*
                if (limitMaxHeight) linkedMarchingCubesController.PreviewAddShapeWithMaxHeight(selectedShape, hit.point.y);
                else linkedMarchingCubesController.PreviewAddShape(selectedShape);
                */
            }

            selectedShape.gameObject.SetActive(false);

            nextUpdateTime = EditorApplication.timeSinceStartup + timeBetweenUpdates;
        }

        if (e.type == EventType.MouseDown && e.button == 0) // Left-click event
        {
            linkedMarchingCubesController.ModificationManager.ApplyPreviewChanges();
            e.Use();
        }
    }

    void SaveShapePositionIfAble()
    {
        if (selectedShape) originalShapePosition = selectedShape.transform.position;
    }

    void RestoreShapePositionIfAble()
    {
        if (selectedShape) selectedShape.transform.position = originalShapePosition;
    }
}
