#if UNITY_EDITOR

using iffnsStuff.MarchingCubeEditor.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class VoxelDebugTool : BaseTool
{
    public override string DisplayName => "Voxel debug tool";

    int coordinateX = 0;
    int coordinateY = 0;
    int coordinateZ = 0;

    public override void DrawUI()
    {
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
        float newValue = GUILayout.HorizontalSlider(currentData.WeightInsideIsPositive, minValue, maxValue, GUILayout.Width(150));

        // Float Field (allows direct input)
        newValue = EditorGUILayout.FloatField(newValue, GUILayout.Width(50));

        EditorGUILayout.EndHorizontal();

        // Ensure the value stays within limits
        newValue = Mathf.Clamp(newValue, minValue, maxValue);

        if(newValue != currentData.DistanceOutsideIsPositive)
        {
            // ToDo: Modify
        }
    }

    public override void DrawGizmos()
    {
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

    public override void HandleSceneUpdate(Event currentEvent)
    {
        
    }

    public override void OnEnable()
    {
        
    }

    public override void OnDisable()
    {
        
    }
}

#endif