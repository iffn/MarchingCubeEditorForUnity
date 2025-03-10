using iffnsStuff.MarchingCubeEditor.Core;
using iffnsStuff.MarchingCubeEditor.EditTools;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CopyPasteTool : BaseTool
{
    PlaceableByClickHandler currentEditShapeHandler;

    Matrix4x4 initialTransform;
    Matrix4x4 previousTransform;

    public override string DisplayName => "Copy paste tool";

    bool copied = false;

    public override void DrawUI()
    {
        if (currentEditShapeHandler == null) return;
        currentEditShapeHandler.DrawEditorUI();

        EditorGUILayout.HelpBox("Note: The idea is that you lock the inspector using the lock symbol on top and them move the shape around.", MessageType.Info);

        Transform selectedTransform = currentEditShapeHandler.SelectedEditShape.transform;
        if (selectedTransform == null) return;

        // Draw Transform Fields
        EditorGUI.BeginChangeCheck();

        Vector3 newPosition = EditorGUILayout.Vector3Field("Position", selectedTransform.position);
        Vector3 newRotation = EditorGUILayout.Vector3Field("Rotation", selectedTransform.eulerAngles);
        Vector3 newScale = EditorGUILayout.Vector3Field("Scale", selectedTransform.localScale);

        // Compute Average Scale
        float avgScale = (newScale.x + newScale.y + newScale.z) / 3f;
        float newAvgScale = EditorGUILayout.FloatField("Uniform Scale", avgScale);

        if (EditorGUI.EndChangeCheck()) // If a value was changed
        {
            Undo.RecordObject(selectedTransform, "Transform Change"); // Register for undo

            selectedTransform.position = newPosition;
            selectedTransform.eulerAngles = newRotation;

            if (!Mathf.Approximately(newAvgScale, avgScale)) // If uniform scale is changed
            {
                float scaleFactor = newAvgScale / avgScale;
                selectedTransform.localScale *= scaleFactor; // Scale uniformly
            }
            else
            {
                selectedTransform.localScale = newScale; // Apply normal scaling
            }

            EditorUtility.SetDirty(selectedTransform); // Ensure the change is applied
        }

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button($"Mark copy location")) MarkCopyLocation();
        if (GUILayout.Button($"Paste")) Paste();
        EditorGUILayout.EndHorizontal();
    }

    public override void HandleSceneUpdate(Event currentEvent)
    {
        if (!copied) return;

        base.HandleSceneUpdate(currentEvent);

        //Matrix4x4 newTransform = LinkedMarchingCubeController.transform.worldToLocalMatrix * selectedShape.transform.localToWorldMatrix;
        Matrix4x4 newTransform = currentEditShapeHandler.SelectedEditShape.transform.worldToLocalMatrix;

        if (!MatricesAreEqual(previousTransform, newTransform, 0.0001f))
        {
            LinkedMarchingCubeController.ModificationManager.ShowPreviewData(currentEditShapeHandler.SelectedEditShape, new BaseModificationTools.CopyModifier(initialTransform, newTransform));

            previousTransform = newTransform;
        }
    }

    public override void OnEnable()
    {
        base.OnEnable();
        if (currentEditShapeHandler == null)
            currentEditShapeHandler = new PlaceableByClickHandler(LinkedMarchingCubeController);
    }

    public override void OnDisable()
    {
        base.OnDisable();

        if(currentEditShapeHandler != null)
            currentEditShapeHandler.SelectedEditShape.gameObject.SetActive(false);
    }

    void MarkCopyLocation()
    {
        copied = true;

        initialTransform = currentEditShapeHandler.SelectedEditShape.transform.worldToLocalMatrix;

        LinkedMarchingCubeController.ModificationManager.ShowPreviewData(currentEditShapeHandler.SelectedEditShape, new BaseModificationTools.CopyModifier(Matrix4x4.identity, Matrix4x4.identity));
    }

    void Paste()
    {
        if (!copied) return;

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
