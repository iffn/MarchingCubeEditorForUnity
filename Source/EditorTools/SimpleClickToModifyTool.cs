#if UNITY_EDITOR

using iffnsStuff.MarchingCubeEditor.EditTools;
using UnityEditor;
using UnityEngine;

public class SimpleClickToModifyTool : BaseTool
{
    // Editor variables
    EditShape selectedShape;
    bool raycastActive;
    bool RaycastActive
    {
        set
        {
            selectedShape.gameObject.SetActive(false);
            LinkedMarchingCubeController.DisplayPreviewShape = false;
            LinkedMarchingCubeController.EnableAllColliders = value;
            raycastActive = value;
        }
    }

    bool displayPreviewShape;

    bool limitHeightToCursor;

    // Internal variables
    Vector3 originalShapePosition;
    double nextUpdateTime;
    double timeBetweenUpdates = 1.0 / 60.0;

    public override string DisplayName => "Click to modify tool";

    // Override functions
    public override void OnEnable()
    {
        if(selectedShape) originalShapePosition = selectedShape.transform.position;
    }

    public override void OnDisable()
    {
        if (selectedShape) selectedShape.transform.position = originalShapePosition;
    }

    public override void DrawUI()
    {
        //Handle shape assignment
        EditShape newSelectedShape = EditorGUILayout.ObjectField(
            selectedShape,
            typeof(EditShape),
            true) as EditShape;

        if(newSelectedShape && newSelectedShape != selectedShape)
        {
            RestoreShapePositionIfAble();
            selectedShape = newSelectedShape;
            SaveShapePositionIfAble();
            newSelectedShape.Initialize();
        }

        if (!newSelectedShape) RestoreShapePositionIfAble();

        if (!selectedShape) return;

        //Settings
        bool newRaycastActive = EditorGUILayout.Toggle("Active", raycastActive);
        if(raycastActive != newRaycastActive)
        {
            RaycastActive = newRaycastActive;
        }

        bool newDisplayPreviewShape = EditorGUILayout.Toggle("Display preview shape", displayPreviewShape);
        if (displayPreviewShape != newDisplayPreviewShape)
        {
            selectedShape.gameObject.SetActive(false);
            LinkedMarchingCubeController.DisplayPreviewShape = false;
            displayPreviewShape = newDisplayPreviewShape;
        }

        limitHeightToCursor = EditorGUILayout.Toggle("Limit height to cursor", limitHeightToCursor);

        if (raycastActive)
        {
            string helpText = "Controls:\n" +
                    "Note that the scene has to be active for some of these to work.\n" +
                    "Click to add\n" +
                    "Ctrl Click to subtract\n";

            helpText += selectedShape.HelpText;

            EditorGUILayout.HelpBox(helpText, MessageType.None);

            selectedShape.DrawUI();
        }
    }

    public override void HandleSceneUpdate(Event e)
    {
        if (!raycastActive) return;

        RayHitResult result = LinkedMarchingCubeEditor.RaycastAtMousePosition(e);

        if (result != RayHitResult.None)
        {
            selectedShape.transform.position = result.point;

            if (displayPreviewShape)
            {
                HandlePreviewUpdate(e);
                LinkedMarchingCubeController.DisplayPreviewShape = true;
            }
            else
            {
                HandleDirectUpdate(e);
                selectedShape.gameObject.SetActive(true);
            }
        }
        else
        {
            selectedShape.gameObject.SetActive(false);
            LinkedMarchingCubeController.DisplayPreviewShape = false;
        }

        if (selectedShape) selectedShape.HandleSceneUpdate(e);

        //Press escape to cancel
        if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Escape)
        {
            RaycastActive = false;
            e.Use();
            return;
        }
    }

    public override void DrawGizmos()
    {

    }

    // Internal functions
    void HandleDirectUpdate(Event e)
    {
        selectedShape.gameObject.SetActive(true);
        //selectedShape.Color = e.control ? subtractionColor : additionColor;

        if (e.type == EventType.MouseDown && e.button == 0) // Left-click event
        {
            if (e.control) LinkedMarchingCubeController.ModificationManager.ModifyData(selectedShape, new BaseModificationTools.SubtractShapeModifier());
            else
            {
                if (limitHeightToCursor)
                    LinkedMarchingCubeController.ModificationManager.ModifyData(selectedShape, new BaseModificationTools.AddShapeWithMaxHeightModifier(selectedShape.transform.position.y)); // ToDo: Improve height calculation by implementing scale
                else 
                    LinkedMarchingCubeController.ModificationManager.ModifyData(selectedShape, new BaseModificationTools.AddShapeModifier());
            }

            e.Use();
            return;
        }
    }

    void HandlePreviewUpdate(Event e)
    {
        if (EditorApplication.timeSinceStartup >= nextUpdateTime) //Only update once in a while
        {
            if (e.control) LinkedMarchingCubeController.ModificationManager.ShowPreviewData(selectedShape, new BaseModificationTools.SubtractShapeModifier());
            else
            {
                if (limitHeightToCursor)
                    LinkedMarchingCubeController.ModificationManager.ShowPreviewData(selectedShape, new BaseModificationTools.AddShapeWithMaxHeightModifier(selectedShape.transform.position.y)); // ToDo: Improve height calculation by implementing scale
                else
                    LinkedMarchingCubeController.ModificationManager.ShowPreviewData(selectedShape, new BaseModificationTools.AddShapeModifier());
            }

            selectedShape.gameObject.SetActive(false);

            nextUpdateTime = EditorApplication.timeSinceStartup + timeBetweenUpdates;
        }

        if (e.type == EventType.MouseDown && e.button == 0) // Left-click event
        {
            LinkedMarchingCubeController.ModificationManager.ApplyPreviewChanges();
            e.Use();
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

#endif