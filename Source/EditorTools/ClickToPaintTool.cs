#if UNITY_EDITOR

using iffnsStuff.MarchingCubeEditor.Core;
using iffnsStuff.MarchingCubeEditor.EditTools;
using UnityEditor;
using UnityEngine;

public class ClickToPaintTool : BaseTool
{
    // Editor variabels
    bool raycastActive = true;
    Color32 brushColor = Color.white;
    AnimationCurve brushCurve = AnimationCurve.Linear(0, 0, 1, 1);
    PlaceableByClickHandler placeableByClick;
    bool getColorActive = false;

    bool modifyRed = true;
    bool modifyGreen = true;
    bool modifyBlue = true;
    bool modifyAlpha = true;

    // Internal values
    public override string DisplayName => "Click to paint tool";

    // Base class functions
    public override void OnEnable()
    {
        base.OnEnable();

        if (placeableByClick == null) placeableByClick = new PlaceableByClickHandler(LinkedMarchingCubeController);
    }

    public override void OnDisable()
    {
        base.OnDisable();

        placeableByClick.SelectedEditShape.gameObject.SetActive(false);
    }

    public override void DrawUI()
    {
        base.DrawUI();

        // Handle shape assignment
        placeableByClick.DrawEditorUI();

        // Handle color assignment
        brushColor = EditorGUILayout.ColorField("Brush Color", brushColor);

        if (getColorActive)
        {
            // Store original colors
            Color originalBackground = GUI.backgroundColor;
            Color originalContentColor = GUI.contentColor;

            // Set custom colors for the selected tool
            GUI.backgroundColor = highlightBackgroundColor;
            GUI.contentColor = Color.white; // Text color

            ShowClickGotGetColorButton();

            // Restore original colors
            GUI.backgroundColor = originalBackground;
            GUI.contentColor = originalContentColor;
        }
        else
        {
            ShowClickGotGetColorButton();
        }

        GUILayout.Label("Modify individual channels and enable modification");

        byte newRed = DisplayModifyColor("R", brushColor.r, ref modifyRed);
        if (newRed != brushColor.r)
            brushColor.r = newRed;

        byte newGreen = DisplayModifyColor("G", brushColor.g, ref modifyGreen);
        if (newGreen != brushColor.g)
            brushColor.g = newGreen;

        byte newBlue = DisplayModifyColor("B", brushColor.b, ref modifyBlue);
        if (newBlue != brushColor.b)
            brushColor.b = newBlue;

        byte newAlpha = DisplayModifyColor("A", brushColor.a, ref modifyAlpha);
        if (newAlpha != brushColor.a)
            brushColor.a = newAlpha;

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
            string helpText = base.helpText +
                    "• Click to paint" + System.Environment.NewLine +
                    "• Ctrl click to get color from voxels";

            helpText += placeableByClick.SelectedEditShape.HelpText;

            EditorGUILayout.HelpBox(helpText, MessageType.None);

            placeableByClick.SelectedEditShape.DrawUI();
        }

        void ShowClickGotGetColorButton()
        {
            if (GUILayout.Button($"Get color from voxels"))
                getColorActive = !getColorActive;
        }

        byte DisplayModifyColor(string title, int currentValue, ref bool modify)
        {
            EditorGUILayout.BeginHorizontal(); // Align slider and input field in one row
            GUILayout.Label(title, GUILayout.Width(20)); // Optional label
            int newColor = EditorGUILayout.IntSlider(currentValue, 0, 255);
            modify = EditorGUILayout.Toggle(modify, GUILayout.Width(20));
            EditorGUILayout.EndHorizontal();
            return (byte)newColor;
        }
    }

    public override void HandleSceneUpdate(Event e)
    {
        base.HandleSceneUpdate(e);

        if (!raycastActive && !getColorActive) return;
        
        RayHitResult result = LinkedMarchingCubeEditor.RaycastAtMousePosition(e);

        if (getColorActive)
        {
            if (LeftClickDownEvent(e))
            {
                SetColorFromRaycast(result);

                e.Use();
            }

            if (EscapeDownEvent(e))
            {
                getColorActive = false;
                RefreshUI();
                e.Use();
                return;
            }
        }
        else
        {
            if (result != RayHitResult.None)
            {
                placeableByClick.SelectedEditShape.gameObject.SetActive(true);
                placeableByClick.SelectedEditShape.transform.position = result.point;

                // Left-click event
                if (LeftClickDownEvent(e))
                {
                    if (ControlIsHeld(e))
                    {
                        SetColorFromRaycast(result);
                    }
                    else
                    {
                        LinkedMarchingCubeController.ModificationManager.ModifyData(
                            placeableByClick.SelectedEditShape,
                            new BaseModificationTools.ChangeColorModifier(brushColor, brushCurve, modifyRed, modifyGreen, modifyBlue, modifyAlpha)
                        );
                    }
                    
                    e.Use();
                    return;
                }
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
                placeableByClick.SelectedEditShape.gameObject.SetActive(false);
                RefreshUI();
                e.Use();
                return;
            }
        }

        void SetColorFromRaycast(RayHitResult result2)
        {
            Vector3 localPoint = LinkedMarchingCubeController.transform.InverseTransformPoint(result2.point);

            VoxelData data = LinkedMarchingCubeController.GetVoxelWithClamp(localPoint.x, localPoint.y, localPoint.z);

            brushColor = data.Color;

            RefreshUI();
        }
    }
}

#endif