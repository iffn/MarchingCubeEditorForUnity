using iffnsStuff.MarchingCubeEditor.Core;
using iffnsStuff.MarchingCubeEditor.EditTools;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static UnityEngine.ParticleSystem;

public class CopyPasteTool : BaseTool
{
    EditShape selectedShape;

    MarchingCubesModel previewModelWithOldData;

    Vector3 originalPosition;
    Vector3 newPosition;

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

        if(newPosition != selectedShape.transform.position)
        {
            newPosition = selectedShape.transform.position;
            LinkedMarchingCubeController.ModificationManager.ShowPreviewData(selectedShape, new BaseModificationTools.CopyModifier(-selectedShape.transform.position + originalPosition));
        }
    }

    void Copy()
    {
        copied = true;
        
        LinkedMarchingCubeController.ModificationManager.ShowPreviewData(selectedShape, new BaseModificationTools.CopyModifier(Vector3Int.zero));
        originalPosition = selectedShape.transform.position;
    }

    void Paste()
    {
        LinkedMarchingCubeController.ModificationManager.ApplyPreviewChanges();
    }
}
