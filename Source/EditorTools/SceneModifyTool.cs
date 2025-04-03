#if UNITY_EDITOR

using iffnsStuff.MarchingCubeEditor.Core;
using iffnsStuff.MarchingCubeEditor.EditTools;
using iffnsStuff.MarchingCubeEditor.SceneEditor;
using UnityEditor;
using UnityEngine;

public class SceneModifyTool : BaseTool
{
    public override string DisplayName => "Modify using scene object";

    PlaceableByClickHandler placeableByClick;

    bool movingActive = false;

    // Base class functions
    public override void OnEnable()
    {
        base.OnEnable();

        if (placeableByClick == null) placeableByClick = new PlaceableByClickHandler(LinkedMarchingCubeController);
    }

    public override void DrawUI()
    {
        base.DrawUI();

        if (placeableByClick == null) return;

        placeableByClick.DrawEditorUI();

        if (movingActive)
        {
            // Store original colors
            Color originalBackground = GUI.backgroundColor;
            Color originalContentColor = GUI.contentColor;

            // Set custom colors for the selected tool
            GUI.backgroundColor = highlightBackgroundColor;
            GUI.contentColor = Color.white; // Text color

            ShowClickToMoveToOriginButton();

            // Restore original colors
            GUI.backgroundColor = originalBackground;
            GUI.contentColor = originalContentColor;
        }
        else
        {
            ShowClickToMoveToOriginButton();
        }

        // Show add and remove options
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button($"Add {placeableByClick.SelectedEditShape.transform.name}"))
            LinkedMarchingCubeController.ModificationManager.ModifyData(placeableByClick.SelectedEditShape, new BaseModificationTools.AddShapeModifier());
        if (GUILayout.Button($"Subtract {placeableByClick.SelectedEditShape.transform.name}"))
            LinkedMarchingCubeController.ModificationManager.ModifyData(placeableByClick.SelectedEditShape, new BaseModificationTools.SubtractShapeModifier());
        EditorGUILayout.EndHorizontal();

        DrawTransformFields(placeableByClick.SelectedEditShape.transform);

        void ShowClickToMoveToOriginButton()
        {
            if (GUILayout.Button($"Move to origin of click on object"))
                movingActive = !movingActive;
        }

        EditorGUILayout.HelpBox("Note: The idea is that you lock the inspector using the lock symbol on top and them move the shape around.", MessageType.Info);
    }

    public override void HandleSceneUpdate(Event currentEvent)
    {
        base.HandleSceneUpdate(currentEvent);

        if(movingActive)
        {
            if (LeftClickDownEvent(currentEvent))
            {
                Ray ray = HandleUtility.GUIPointToWorldRay(currentEvent.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    Transform hitObject = hit.transform;
                    placeableByClick.SelectedEditShape.transform.SetPositionAndRotation(hitObject.position, hitObject.rotation);
                    currentEvent.Use();
                }
            }
        }
    }
}
#endif