#if UNITY_EDITOR

using iffnsStuff.MarchingCubeEditor.EditTools;
using UnityEditor;
using UnityEngine;

public class SimpleClickToModifyTool : BaseTool
{
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
    Vector3 originalShapePosition;

    public override string DisplayName => "Click to modify tool";

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

        if (raycastActive)
        {
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

    double nextUpdateTime;
    double timeBetweenUpdates = 1.0 / 60.0;

    void HandleDirectUpdate(Event e)
    {
        selectedShape.gameObject.SetActive(true);
        //selectedShape.Color = e.control ? subtractionColor : additionColor;

        if (e.type == EventType.MouseDown && e.button == 0) // Left-click event
        {
            if (e.control) LinkedMarchingCubeController.ModificationManager.ModifyData(selectedShape, new BaseModificationTools.SubtractShapeModifier());
            else
            {
                LinkedMarchingCubeController.ModificationManager.ModifyData(selectedShape, new BaseModificationTools.AddShapeModifier());
                /*
                if (limitMaxHeight) linkedMarchingCubesController.AddShapeWithMaxHeight(selectedShape, hit.point.y, true);
                else linkedMarchingCubesController.AddShape(selectedShape, true);
                */
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
                LinkedMarchingCubeController.ModificationManager.ShowPreviewData(selectedShape, new BaseModificationTools.AddShapeModifier());
                /*
                if (limitMaxHeight) linkedMarchingCubesController.PreviewAddShapeWithMaxHeight(selectedShape, hit.point.y);
                else linkedMarchingCubesController.PreviewAddShape(selectedShape);
                */
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