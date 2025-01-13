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

    public PlaceableByClickHandler PlaceableByClick;

    public override string DisplayName => "Click to paint tool";

    public override void OnEnable()
    {
        if (PlaceableByClick == null) PlaceableByClick = new PlaceableByClickHandler(LinkedMarchingCubeController);
    }

    public override void OnDisable()
    {
        
    }

    public override void DrawUI()
    {
        // Handle shape assignment
        PlaceableByClick.EditorUI();

        // Handle color assignment
        brushColor = EditorGUILayout.ColorField("Brush Color", brushColor);

        // Handle curve assignment
        brushCurve = EditorGUILayout.CurveField("Brush Curve", brushCurve, Color.cyan, new Rect(0, 0, 1f, 1f));

        if (PlaceableByClick == null) return;

        //Settings
        bool newRaycastActive = EditorGUILayout.Toggle("Active", raycastActive);
        if(raycastActive != newRaycastActive)
        {
            PlaceableByClick.SelectedEditShape.gameObject.SetActive(false);
            LinkedMarchingCubeController.DisplayPreviewShape = false;
            LinkedMarchingCubeController.EnableAllColliders = newRaycastActive;
            raycastActive = newRaycastActive;
        }

        if (raycastActive)
        {
            string helpText = "Controls:\n" +
                    "Note that the scene has to be active for some of these to work.\n" +
                    "Click to paint\n";

            helpText += PlaceableByClick.SelectedEditShape.HelpText;

            EditorGUILayout.HelpBox(helpText, MessageType.None);

            PlaceableByClick.SelectedEditShape.DrawUI();
        }
    }

    public override void HandleSceneUpdate(Event e)
    {
        if (!raycastActive) return;

        RayHitResult result = LinkedMarchingCubeEditor.RaycastAtMousePosition(e);

        if(result != RayHitResult.None)
        {
            PlaceableByClick.SelectedEditShape.gameObject.SetActive(true);
            PlaceableByClick.SelectedEditShape.transform.position = result.point;

            HandleDirectUpdate(e);
        }
        else
        {
            PlaceableByClick.SelectedEditShape.gameObject.SetActive(false);
            LinkedMarchingCubeController.DisplayPreviewShape = false;
        }

        if (PlaceableByClick.SelectedShape != null) PlaceableByClick.SelectedEditShape.HandleSceneUpdate(e);

        if (EscapeDownEvent(e))
        {
            raycastActive = false;
            e.Use();
            return;
        }
    }

    void HandleDirectUpdate(Event e)
    {
        PlaceableByClick.SelectedEditShape.gameObject.SetActive(true);

        // Left-click event
        if (LeftClickDownEvent(e))
        {
            LinkedMarchingCubeController.ModificationManager.ModifyData(
                PlaceableByClick.SelectedEditShape, 
                new BaseModificationTools.ChangeColorModifier(brushColor, brushCurve)
            );
            e.Use();
            return;
        }
    }
}

#endif