using iffnsStuff.MarchingCubeEditor.Core;
using iffnsStuff.MarchingCubeEditor.EditTools;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEditor;
using UnityEngine;

public class SimpleSceneModifyTool : BaseTool
{
    EditShape selectedShape;
    
    public SimpleSceneModifyTool(MarchingCubesController linkedController) : base(linkedController)
    {
        
    }

    public override string displayName
    {
        get
        {
            return "Modify using scene object";
        }
    }

    public override void DrawGizmos()
    {
        
    }

    public override void DrawUI()
    {
        selectedShape = EditorGUILayout.ObjectField(
            selectedShape,
            typeof(EditShape),
            true) as EditShape;

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button($"Add {selectedShape.transform.name}")) linkedMarchingCubesController.ModificationManager.ModifyData(selectedShape, new BaseModificationTools.AddShapeModifier());
        if (GUILayout.Button($"Subtract {selectedShape.transform.name}")) linkedMarchingCubesController.ModificationManager.ModifyData(selectedShape, new BaseModificationTools.SubtractShapeModifier());
        EditorGUILayout.EndHorizontal();
    }

    public override void HandleUIUpdate(Event e)
    {
        
    }

    public override void OnDisable()
    {
        
    }

    public override void OnEnable()
    {
        
    }
}
