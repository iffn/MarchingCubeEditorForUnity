using iffnsStuff.MarchingCubeEditor.Core;
using iffnsStuff.MarchingCubeEditor.EditTools;
using UnityEditor;
using UnityEngine;

public class SimpleClickToPaintTool : BaseTool
{
    EditShape selectedShape;
    bool raycastActive;
    Vector3 originalShapePosition;
    Color32 brushColor;
    AnimationCurve brushCurve = AnimationCurve.Linear(0, 0, 1, 1);

    public override string displayName { get => "Click to paint tool"; }

    public SimpleClickToPaintTool(MarchingCubesController linkedController) : base(linkedController) {}

    public override void OnEnable()
    {
        if (selectedShape) originalShapePosition = selectedShape.transform.position;
    }

    public override void OnDisable()
    {
        if (selectedShape) selectedShape.transform.position = originalShapePosition;
    }

    public override void DrawUI()
    {
        // Handle shape assignment
        EditShape newSelectedShape = EditorGUILayout.ObjectField(
            "Brush Shape",
            selectedShape,
            typeof(EditShape),
            true
        ) as EditShape;

        // Handle color assignment
        brushColor = EditorGUILayout.ColorField("Brush Color", brushColor);

        // Handle curve assignment
        brushCurve = EditorGUILayout.CurveField("Brush Curve", brushCurve, Color.cyan, new Rect(0, 0, 1f, 1f));
        
        if(newSelectedShape && newSelectedShape != selectedShape)
        {
            RestoreShapePositionIfAble();
            selectedShape = newSelectedShape;
            SaveShapePositionIfAble();
        }

        if (!newSelectedShape) RestoreShapePositionIfAble();

        if (!selectedShape) return;

        //Settings
        bool newRaycastActive = EditorGUILayout.Toggle("Active", raycastActive);
        if(raycastActive != newRaycastActive)
        {
            selectedShape.gameObject.SetActive(false);
            linkedMarchingCubesController.DisplayPreviewShape = false;
            linkedMarchingCubesController.EnableAllColliders = newRaycastActive;
            raycastActive = newRaycastActive;
        }

        if (raycastActive)
        {
            EditorGUILayout.HelpBox("Controls:\n" +
                    "Click to paint\n" +
                    "Shift Scroll to scale", MessageType.Info);
        }
    }

    public override void HandleSceneUpdate(Event e)
    {
        if (!raycastActive) return;

        if(RaycastAtMousePosition(e, out Vector3 hitPoint))
        {
            selectedShape.gameObject.SetActive(true);
            selectedShape.transform.position = hitPoint;

            HandleDirectUpdate(e);
        }
        else
        {
            selectedShape.gameObject.SetActive(false);
            linkedMarchingCubesController.DisplayPreviewShape = false;
        }

        if (e.shift && e.type == EventType.ScrollWheel)
        {
            float scaleDelta = e.delta.x * -0.03f; // Scale factor; reverse direction if needed

            selectedShape.transform.localScale *= (scaleDelta + 1);

            e.Use(); // Mark event as handled
        }
    }

    public override void DrawGizmos() {}

    void HandleDirectUpdate(Event e)
    {
        selectedShape.gameObject.SetActive(true);

        // Left-click event
        if (e.type == EventType.MouseDown && e.button == 0)
        {
            linkedMarchingCubesController.ModificationManager.ModifyData(
                selectedShape, 
                new BaseModificationTools.ChangeColorModifier(brushColor, brushCurve)
            );
            e.Use();
            return;
        }
    }

    void SaveShapePositionIfAble()
    {
        if (selectedShape) originalShapePosition = selectedShape.transform.position;
    }

    void RestoreShapePositionIfAble()
    {
        if (selectedShape) selectedShape.transform.position = originalShapePosition;
    }
}
