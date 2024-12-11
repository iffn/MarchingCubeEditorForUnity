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

        //Settings
        raycastActive = EditorGUILayout.Toggle("Display preview shape", raycastActive);
        displayPreviewShape = EditorGUILayout.Toggle("Display preview shape", displayPreviewShape);
    }

    public override void HandleUIUpdate(Event e)
    {

    }

    public override void DrawGizmos()
    {

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
