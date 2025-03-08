using iffnsStuff.MarchingCubeEditor.Core;
using iffnsStuff.MarchingCubeEditor.EditTools;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CopyPasteTool : BaseTool
{
    EditShape selectedShape;

    MarchingCubesModel previewModelWithOldData;

    Matrix4x4 initialTransform;

    public override string DisplayName => "Copy paste tool";

    bool copied = false;

    public override void DrawUI()
    {
        EditShape newSelectedShape = EditorGUILayout.ObjectField(
            selectedShape,
            typeof(EditShape),
            true) as EditShape;

        if (newSelectedShape && newSelectedShape != selectedShape)
        {
            selectedShape = newSelectedShape;
            newSelectedShape.Initialize();
        }

        if (selectedShape)
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button($"Copy")) Copy();
            if (GUILayout.Button($"Paste")) Paste();
            EditorGUILayout.EndHorizontal();
        }
    }

    public override void HandleSceneUpdate(Event currentEvent)
    {
        if (!copied) return;

        base.HandleSceneUpdate(currentEvent);

        Matrix4x4 newTransform = LinkedMarchingCubeController.transform.worldToLocalMatrix * selectedShape.transform.localToWorldMatrix;

        if (initialTransform != newTransform)
        {
            Matrix4x4 deltaTransform = newTransform * initialTransform.inverse;
            LinkedMarchingCubeController.ModificationManager.ShowPreviewData(selectedShape, new BaseModificationTools.CopyModifier(deltaTransform));
        }
    }

    void Copy()
    {
        copied = true;

        Matrix4x4 initialTransform = LinkedMarchingCubeController.transform.worldToLocalMatrix * selectedShape.transform.localToWorldMatrix;

        LinkedMarchingCubeController.ModificationManager.ShowPreviewData(selectedShape, new BaseModificationTools.CopyModifier(Matrix4x4.identity));
    }

    void Paste()
    {
        LinkedMarchingCubeController.ModificationManager.ApplyPreviewChanges();
    }
}
