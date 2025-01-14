#if UNITY_EDITOR

using iffnsStuff.MarchingCubeEditor.EditTools;
using UnityEditor;
using UnityEngine;

public class SimpleClickToPaintTool : BaseTool
{
    bool raycastActive;
    Vector3 originalShapePosition;
    Color32 brushColor;
    AnimationCurve brushCurve = AnimationCurve.Linear(0, 0, 1, 1);

    PlaceableByClickHandler placeableByClick;

    public override string DisplayName => "Click to paint tool";

    public override void OnEnable()
    {
        if (placeableByClick == null) placeableByClick = new PlaceableByClickHandler(LinkedMarchingCubeController);
    }

    public override void OnDisable()
    {
        
    }

    public override void DrawUI()
    {
        // Handle shape assignment
        placeableByClick.DrawEditorUI();

        // Handle color assignment
        brushColor = EditorGUILayout.ColorField("Brush Color", brushColor);

        // Handle curve assignment
        brushCurve = EditorGUILayout.CurveField("Brush Curve", brushCurve, Color.cyan, new Rect(0, 0, 1f, 1f));

        if (placeableByClick == null) return;

        //Settings
        bool newRaycastActive = EditorGUILayout.Toggle("Active", raycastActive);
        if(raycastActive != newRaycastActive)
        {
            placeableByClick.SelectedEditShape.gameObject.SetActive(false);
            LinkedMarchingCubeController.DisplayPreviewShape = false;
            LinkedMarchingCubeController.EnableAllColliders = newRaycastActive;
            raycastActive = newRaycastActive;
        }

        if (raycastActive)
        {
            string helpText = "Controls:\n" +
                    "Note that the scene has to be active for some of these to work.\n" +
                    "Click to paint\n";

            helpText += placeableByClick.SelectedEditShape.HelpText;

            EditorGUILayout.HelpBox(helpText, MessageType.None);

            placeableByClick.SelectedEditShape.DrawUI();
        }
    }

    public override void HandleSceneUpdate(Event e)
    {
        if (!raycastActive) return;

        RayHitResult result = LinkedMarchingCubeEditor.RaycastAtMousePosition(e);

        if(result != RayHitResult.None)
        {
            placeableByClick.SelectedEditShape.gameObject.SetActive(true);
            placeableByClick.SelectedEditShape.transform.position = result.point;

            HandleDirectUpdate(e);
        }
        else
        {
            placeableByClick.SelectedEditShape.gameObject.SetActive(false);
            LinkedMarchingCubeController.DisplayPreviewShape = false;
        }

        if (placeableByClick.SelectedShape != null) placeableByClick.SelectedEditShape.HandleSceneUpdate(e);

        if (EscapeDownEvent(e))
        {
            raycastActive = false;
            e.Use();
            return;
        }
    }

    void HandleDirectUpdate(Event e)
    {
        placeableByClick.SelectedEditShape.gameObject.SetActive(true);

        // Left-click event
        if (LeftClickDownEvent(e))
        {
            LinkedMarchingCubeController.ModificationManager.ModifyData(
                placeableByClick.SelectedEditShape, 
                new BaseModificationTools.ChangeColorModifier(brushColor, brushCurve)
            );
            e.Use();
            return;
        }
    }
}

#endif