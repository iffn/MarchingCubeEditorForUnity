#if UNITY_EDITOR

using iffnsStuff.MarchingCubeEditor.Core;
using iffnsStuff.MarchingCubeEditor.EditTools;
using UnityEditor;
using UnityEngine;

public class SimpleSceneModifyTool : BaseTool
{
    EditShape selectedShape;

    public override string displayName
    {
        get
        {
            return "Modify using scene object";
        }
    }

    public SimpleSceneModifyTool(MarchingCubesController linkedController) : base(linkedController)
    {
        
    }

    public override void OnEnable()
    {

    }

    public override void OnDisable()
    {

    }

    public override void DrawUI()
    {
        selectedShape = EditorGUILayout.ObjectField(
            selectedShape,
            typeof(EditShape),
            true) as EditShape;

        if (selectedShape)
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button($"Add {selectedShape.transform.name}")) linkedMarchingCubesController.ModificationManager.ModifyData(selectedShape, new BaseModificationTools.AddShapeModifier());
            if (GUILayout.Button($"Subtract {selectedShape.transform.name}")) linkedMarchingCubesController.ModificationManager.ModifyData(selectedShape, new BaseModificationTools.SubtractShapeModifier());
            EditorGUILayout.EndHorizontal();
        }
    }

    public override void HandleSceneUpdate(Event e)
    {
        
    }

    public override void DrawGizmos()
    {
        
    }
}
#endif