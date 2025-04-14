#if UNITY_EDITOR

using iffnsStuff.MarchingCubeEditor.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System;

public class VoxelDebugTool : BaseTool
{
    public override string DisplayName => "Voxel debug tool";

    int coordinateX = 0;
    int coordinateY = 0;
    int coordinateZ = 0;
    bool clickToGetActive = true;

    // Base class functions
    public override void OnEnable()
    {
        base.OnEnable();
    }

    public override void OnDisable()
    {
        base.OnDisable();
    }

    public override void DrawUI()
    {
        base.DrawUI();

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("X");
        GUILayout.Label("Y");
        GUILayout.Label("Z");
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        coordinateX = System.Math.Clamp(EditorGUILayout.IntField(coordinateX), 0, LinkedMarchingCubeController.GridResolutionX - 1);
        coordinateY = System.Math.Clamp(EditorGUILayout.IntField(coordinateY), 0, LinkedMarchingCubeController.GridResolutionY - 1);
        coordinateZ = System.Math.Clamp(EditorGUILayout.IntField(coordinateZ), 0, LinkedMarchingCubeController.GridResolutionZ - 1);
        EditorGUILayout.EndHorizontal();

        //Value slider
        VoxelData currentData = LinkedMarchingCubeController.VoxelDataReference[coordinateX, coordinateY, coordinateZ];

        float minValue = -1f;  // Minimum slider value
        float maxValue = 1f; // Maximum slider value

        EditorGUILayout.BeginHorizontal(); // Align slider and input field in one row
        GUILayout.Label("Weight", GUILayout.Width(100)); // Optional label

        // Slider (adjusts value within range)
        float currentWeight = currentData.WeightInsideIsPositive;

        float newWeight = GUILayout.HorizontalSlider(currentWeight, minValue, maxValue, GUILayout.Width(150));

        // Float Field (allows direct input)
        newWeight = EditorGUILayout.FloatField(newWeight);

        EditorGUILayout.EndHorizontal();

        newWeight = Mathf.Clamp(newWeight, minValue, maxValue);

        if(!Mathf.Approximately(newWeight, currentWeight))
        {
            LinkedMarchingCubeController.ModificationManager.ModifySingleVoxel(coordinateX, coordinateY, coordinateZ, currentData.WithWeightInsideIsPositive(newWeight));
        }

        // Color
        Color newColor = EditorGUILayout.ColorField("Color", currentData.Color);

        if(newColor != currentData.Color)
        {
            Debug.Log("Update");
            LinkedMarchingCubeController.ModificationManager.ModifySingleVoxel(coordinateX, coordinateY, coordinateZ, currentData.WithColor(newColor));
        }

        // Raycast toggle
        clickToGetActive = GUILayout.Toggle(clickToGetActive, "Click to get active");
    }

    public override void HandleSceneUpdate(Event currentEvent)
    {
        base.HandleSceneUpdate(currentEvent);

        if (!clickToGetActive)
            return;

        if (LeftClickDownEvent(currentEvent))
        {
            RayHitResult result = LinkedMarchingCubeEditor.RaycastAtMousePosition(currentEvent);

            if (result != RayHitResult.None)
            {
                Vector3 localHit = LinkedMarchingCubeController.transform.InverseTransformPoint(result.point);

                int xi = Mathf.RoundToInt(localHit.x);
                int yi = Mathf.RoundToInt(localHit.y);
                int zi = Mathf.RoundToInt(localHit.z);

                coordinateX = Math.Clamp(xi, 0, LinkedMarchingCubeController.MaxGrid.x);
                coordinateY = Math.Clamp(yi, 0, LinkedMarchingCubeController.MaxGrid.y);
                coordinateZ = Math.Clamp(zi, 0, LinkedMarchingCubeController.MaxGrid.z);

                RefreshUI();
            }

            currentEvent.Use();
        }
    }

    public override void DrawGizmos()
    {
         base.DrawGizmos();

        Vector3 coordinate = new Vector3(coordinateX, coordinateY, coordinateZ);

        Transform controllerTransform = LinkedMarchingCubeController.transform;

        coordinate = controllerTransform.TransformPoint(coordinate); 

        float lineLength = LinkedMarchingCubeController.transform.localScale.x;

        Gizmos.DrawLine(
            coordinate + lineLength * controllerTransform.right,
            coordinate + lineLength * -controllerTransform.right);

        Gizmos.DrawLine(
            coordinate + lineLength * controllerTransform.up,
            coordinate + lineLength * -controllerTransform.up);

        Gizmos.DrawLine(
            coordinate + lineLength * controllerTransform.forward,
            coordinate + lineLength * -controllerTransform.forward);
    }
}

#endif