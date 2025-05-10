#if UNITY_EDITOR

using iffnsStuff.MarchingCubeEditor.Core;
using iffnsStuff.MarchingCubeEditor.EditTools;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CopyPasteTool : BaseTool
{
    PlaceableByClickHandler currentEditShapeHandler;

    Matrix4x4 initialTransformWTL;
    Matrix4x4 previousTransformWTL;
    Matrix4x4 gizmosMatrix;

    Vector3 originalPositionLocal;
    Quaternion originalRotationLocal;
    Vector3 originalScaleLocal;

    VoxelData[,,] currentDataCopy;

    public override string DisplayName => "Copy paste tool";

    bool displayPreview = false;

    bool copied = false;

    // Base class functions
    public override void OnEnable()
    {
        base.OnEnable();

        if (currentEditShapeHandler == null)
            currentEditShapeHandler = new PlaceableByClickHandler(LinkedMarchingCubeController);
    }

    public override void OnDisable()
    {
        base.OnDisable();

        if (currentEditShapeHandler != null)
            currentEditShapeHandler.SelectedEditShape.gameObject.SetActive(false);
    }

    protected override void GeneratePersistentUI()
    {

    }

    public override void DrawUI()
    {
        base.DrawUI();

        if (currentEditShapeHandler == null) return;
        currentEditShapeHandler.DrawEditorUI();

        EditorGUILayout.HelpBox("Note: The idea is that you lock the inspector using the lock symbol on top and them move the shape around.", MessageType.Info);

        Transform selectedTransform = currentEditShapeHandler.SelectedEditShape.transform;
        if (selectedTransform == null) return;

        DrawTransformFields(selectedTransform);

        bool newDisplayPreview = EditorGUILayout.Toggle("Display preview", displayPreview);

        if(newDisplayPreview != displayPreview)
        {
            displayPreview = newDisplayPreview;

            LinkedMarchingCubeController.DisplayPreviewShape = newDisplayPreview;

            DisplayPreivewIfEnabled();
        }

        if (copied && GUILayout.Button($"Reset location"))
            ResetLocation();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button($"Copy"))
            MarkCopyLocation();
        if (GUILayout.Button($"Paste"))
            Paste();
        EditorGUILayout.EndHorizontal();
    }

    public override void HandleSceneUpdate(Event currentEvent)
    {
        base.HandleSceneUpdate(currentEvent);

        if (!copied) return;

        DisplayPreivewIfEnabled();
    }

    public override void DrawGizmos()
    {
        base.DrawGizmos();

        Gizmos.matrix = gizmosMatrix;

        // Draw Wire Cube
        Gizmos.color = new Color(1f, 0.5f, 0f);
        Gizmos.DrawWireCube(Vector3.zero, Vector3.one); // Cube centered at origin
    }

    // Internal functions
    void MarkCopyLocation()
    {
        copied = true;

        currentDataCopy = GenerateVoxelDataCopy(LinkedMarchingCubeController);

        Transform shapeTransform = currentEditShapeHandler.SelectedEditShape.transform;

        initialTransformWTL = shapeTransform.worldToLocalMatrix;

        if (displayPreview)
            LinkedMarchingCubeController.ModificationManager.ShowPreviewData(currentEditShapeHandler.SelectedEditShape, new BaseModificationTools.CopyModifier(currentDataCopy, Matrix4x4.identity, Matrix4x4.identity, Matrix4x4.identity));

        // Extract position, rotation, and scale
        originalPositionLocal = shapeTransform.localPosition;
        originalRotationLocal = shapeTransform.localRotation;
        originalScaleLocal = shapeTransform.lossyScale;

        // Apply transformation
        gizmosMatrix = Matrix4x4.TRS(shapeTransform.position, shapeTransform.rotation, shapeTransform.lossyScale);
    }

    BaseModificationTools.IVoxelModifier PasteModifier()
    {
        Matrix4x4 newTransformWTL = currentEditShapeHandler.SelectedEditShape.transform.worldToLocalMatrix;

        Matrix4x4 controllerTransformWTL = LinkedMarchingCubeController.transform.worldToLocalMatrix;

        return new BaseModificationTools.CopyModifier(currentDataCopy, initialTransformWTL, newTransformWTL, controllerTransformWTL);
    }

    void DisplayPreivewIfEnabled()
    {
        if (!displayPreview || !copied)
            return;

        Matrix4x4 newTransformWTL = currentEditShapeHandler.SelectedEditShape.transform.worldToLocalMatrix;

        if (!MatricesAreEqual(previousTransformWTL, newTransformWTL, 0.0001f))
        {
            BaseModificationTools.IVoxelModifier modifier = PasteModifier();

            LinkedMarchingCubeController.ModificationManager.ShowPreviewData(currentEditShapeHandler.SelectedEditShape, modifier);

            previousTransformWTL = currentEditShapeHandler.SelectedEditShape.transform.worldToLocalMatrix;
        }
    }

    void Paste()
    {
        if (!copied) return;

        if (displayPreview)
        {
            LinkedMarchingCubeController.ModificationManager.ApplyPreviewChanges();
        }
        else
        {
            BaseModificationTools.IVoxelModifier modifier = PasteModifier();
            LinkedMarchingCubeController.ModificationManager.ModifyData(currentEditShapeHandler.SelectedEditShape, modifier);
        }
            
    }

    void ResetLocation()
    {
        Transform shapeTransform = currentEditShapeHandler.SelectedEditShape.transform;

        shapeTransform.SetLocalPositionAndRotation(originalPositionLocal, originalRotationLocal);

        shapeTransform.localScale = originalScaleLocal;
    }

    bool MatricesAreEqual(Matrix4x4 m1, Matrix4x4 m2, float tolerance)
    {
        for (int i = 0; i < 16; i++)
        {
            if (Mathf.Abs(m1[i] - m2[i]) > tolerance) return false;
        }

        return true;
    }
}

#endif