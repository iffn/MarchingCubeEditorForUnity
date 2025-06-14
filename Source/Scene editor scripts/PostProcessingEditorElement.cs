#if UNITY_EDITOR
using iffnsStuff.MarchingCubeEditor.Core;
using iffnsStuff.MarchingCubeEditor.SceneEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PostProcessingEditorElement : EditorElement
{
    public override string DisplayName => "Post processing";

    public PostProcessingEditorElement(MarchingCubeEditor linkedEditor, bool foldoutOpenByDefault) : base(linkedEditor, foldoutOpenByDefault)
    {
        // Constructor
    }

    public override void DrawUI()
    {
        PostProcessingOptions options = linkedController.CurrentPostProcessingOptions;

        bool changed = false;

        foreach (UnityUtilityFunctions.FieldMetadata field in PostProcessingOptions.FieldDefinitions)
        {
            // Check if the field is visible
            if (field.IsVisible(options))
            {
                // Draw the field based on its type
                if (field.FieldType == typeof(bool))
                {
                    bool currentValue = (bool)field.GetValue(options);
                    bool newValue = EditorGUILayout.Toggle(field.Name, currentValue);
                    if (newValue != currentValue)
                    {
                        options = field.SetValue(options, newValue);
                        changed = true;
                    }
                }
                else if (field.FieldType == typeof(float))
                {
                    float currentValue = (float)field.GetValue(options);
                    float newValue = EditorGUILayout.FloatField(field.Name, currentValue);
                    if (!Mathf.Approximately(newValue, currentValue))
                    {
                        options = field.SetValue(options, newValue);
                        changed = true;
                    }
                }
                else if (field.FieldType == typeof(double))
                {
                    float currentValue = (float)(double)field.GetValue(options);
                    float newValue = EditorGUILayout.FloatField(field.Name, currentValue);
                    if (!Mathf.Approximately(newValue, currentValue))
                    {
                        double newValueDouble = newValue;
                        options = field.SetValue(options, newValueDouble);
                        changed = true;
                    }
                }
            }
        }

        if (changed)
        {
            linkedController.CurrentPostProcessingOptions = options;
        }

        GUILayout.Label($"Last post processing time: {MarchingCubesView.ElapsedPostProcessingTimeSeconds}");
        GUILayout.Label($"Modified elements: {MarchingCubesView.ModifiedElements}");
        GUILayout.Label($"Removed vertices: {MarchingCubesView.RemovedVertices}");

        if (GUILayout.Button($"Post process mesh")) linkedController.PostProcessMesh();
    }
}
#endif