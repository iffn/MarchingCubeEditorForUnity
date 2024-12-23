#if UNITY_EDITOR

using iffnsStuff.MarchingCubeEditor.Core;
using iffnsStuff.MarchingCubeEditor.EditTools;
using iffnsStuff.MarchingCubeEditor.SceneEditor;
using UnityEditor;
using UnityEngine;

public class SimpleSceneModifyTool : BaseTool
{
    EditShape selectedShape;

    public override string DisplayName => "Modify using scene object";

    public override void DrawUI()
    {
        EditShape newSelectedShape = EditorGUILayout.ObjectField(
            selectedShape,
            typeof(EditShape),
            true) as EditShape;

        if (newSelectedShape && newSelectedShape != selectedShape)
        {
            newSelectedShape.Initialize();
        }

        if (selectedShape)
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button($"Add {selectedShape.transform.name}")) LinkedMarchingCubeController.ModificationManager.ModifyData(selectedShape, new BaseModificationTools.AddShapeModifier());
            if (GUILayout.Button($"Subtract {selectedShape.transform.name}")) LinkedMarchingCubeController.ModificationManager.ModifyData(selectedShape, new BaseModificationTools.SubtractShapeModifier());
            EditorGUILayout.EndHorizontal();
        }
    }
}
#endif