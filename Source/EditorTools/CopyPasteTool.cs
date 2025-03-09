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
            if(selectedShape) selectedShape.gameObject.SetActive(false);
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

        if (!MatricesAreEqual(initialTransform, newTransform, 0.0001f))
        {
            Matrix4x4 deltaTransform = newTransform * initialTransform.inverse;
            LinkedMarchingCubeController.ModificationManager.ShowPreviewData(selectedShape, new BaseModificationTools.CopyModifier(deltaTransform));

            initialTransform = newTransform;
        }
    }

    public override void OnEnable()
    {
        base.OnEnable();

        if (selectedShape) selectedShape.gameObject.SetActive(true);
    }

    public override void OnDisable()
    {
        base.OnDisable();

        if (selectedShape) selectedShape.gameObject.SetActive(false);
    }

    void Copy()
    {
        copied = true;

        initialTransform = LinkedMarchingCubeController.transform.worldToLocalMatrix * selectedShape.transform.localToWorldMatrix;

        LinkedMarchingCubeController.ModificationManager.ShowPreviewData(selectedShape, new BaseModificationTools.CopyModifier(Matrix4x4.identity));
    }

    void Paste()
    {
        LinkedMarchingCubeController.ModificationManager.ApplyPreviewChanges();
    }

    private bool MatricesAreEqual(Matrix4x4 m1, Matrix4x4 m2, float tolerance)
    {
        for (int i = 0; i < 16; i++)
        {
            if (Mathf.Abs(m1[i] - m2[i]) > tolerance) return false;
        }
        return true;
    }
}
