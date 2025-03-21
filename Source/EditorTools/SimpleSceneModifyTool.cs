#if UNITY_EDITOR

using iffnsStuff.MarchingCubeEditor.Core;
using iffnsStuff.MarchingCubeEditor.EditTools;
using iffnsStuff.MarchingCubeEditor.SceneEditor;
using UnityEditor;
using UnityEngine;

public class SimpleSceneModifyTool : BaseTool
{
    public override string DisplayName => "Modify using scene object";

    PlaceableByClickHandler placeableByClick;

    public override void OnEnable()
    {
        if (placeableByClick == null) placeableByClick = new PlaceableByClickHandler(LinkedMarchingCubeController);
    }

    public override void DrawUI()
    {
        if (placeableByClick == null) return;

        placeableByClick.DrawEditorUI();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button($"Add {placeableByClick.SelectedEditShape.transform.name}"))
            LinkedMarchingCubeController.ModificationManager.ModifyData(placeableByClick.SelectedEditShape, new BaseModificationTools.AddShapeModifier());
        if (GUILayout.Button($"Subtract {placeableByClick.SelectedEditShape.transform.name}"))
            LinkedMarchingCubeController.ModificationManager.ModifyData(placeableByClick.SelectedEditShape, new BaseModificationTools.SubtractShapeModifier());
        EditorGUILayout.EndHorizontal();
    }
}
#endif