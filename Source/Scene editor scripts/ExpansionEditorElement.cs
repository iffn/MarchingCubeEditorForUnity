#if UNITY_EDITOR

using iffnsStuff.MarchingCubeEditor.Core;
using iffnsStuff.MarchingCubeEditor.SceneEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ExpansionEditorElement : EditorElement
{
    public override string DisplayName => "Expansion";

    public ExpansionEditorElement(MarchingCubeEditor linkedEditor, bool foldoutOpenByDefault) : base(linkedEditor, foldoutOpenByDefault)
    {
        // Constructor
    }

    int gridCExpandSize = 0;
    bool moveTransformWhenExpanding = true;
    float xScale = 1;
    float yScale = 1;
    float zScale = 1;

    public override void OnEnable()
    {
        base.OnEnable();

        if(linkedController != null)
        {
            xScale = linkedController.transform.localScale.x;
            yScale = linkedController.transform.localScale.y;
            zScale = linkedController.transform.localScale.z;
        }
    }

    public override void DrawUI()
    {
        gridCExpandSize = EditorGUILayout.IntField("Expansion size", gridCExpandSize);

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Expand +X"))
        {
            linkedController.ExpandGrid(gridCExpandSize, MarchingCubesController.ExpansionDirections.XPos);
        }

        if (GUILayout.Button("Expand +Y"))
        {
            linkedController.ExpandGrid(gridCExpandSize, MarchingCubesController.ExpansionDirections.YPos);
        }

        if (GUILayout.Button("Expand +Z"))
        {
            linkedController.ExpandGrid(gridCExpandSize, MarchingCubesController.ExpansionDirections.ZPos);
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Expand -X"))
        {
            linkedController.ExpandGrid(gridCExpandSize, MarchingCubesController.ExpansionDirections.XNeg);

            if (moveTransformWhenExpanding)
                linkedController.transform.localPosition -= gridCExpandSize * linkedController.transform.localScale.x * Vector3.right;
        }

        if (GUILayout.Button("Expand -Y"))
        {
            linkedController.ExpandGrid(gridCExpandSize, MarchingCubesController.ExpansionDirections.YNeg);

            if (moveTransformWhenExpanding)
                linkedController.transform.localPosition -= gridCExpandSize * linkedController.transform.localScale.y * Vector3.up;
        }

        if (GUILayout.Button("Expand -Z"))
        {
            linkedController.ExpandGrid(gridCExpandSize, MarchingCubesController.ExpansionDirections.ZNeg);

            if (moveTransformWhenExpanding)
                linkedController.transform.localPosition -= gridCExpandSize * linkedController.transform.localScale.z * Vector3.forward;
        }

        EditorGUILayout.EndHorizontal();

        moveTransformWhenExpanding = EditorGUILayout.Toggle("Move transform to keep position", moveTransformWhenExpanding);

        // Scale up layout
        EditorGUILayout.LabelField("Resolution change");

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("X");
        GUILayout.Label("Y");
        GUILayout.Label("Z");
        EditorGUILayout.EndHorizontal();

        Vector3 currentLocalScale = linkedController.transform.localScale;

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label($"{currentLocalScale.x}");
        GUILayout.Label($"{currentLocalScale.y}");
        GUILayout.Label($"{currentLocalScale.z}");
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        xScale = EditorGUILayout.FloatField(xScale);
        yScale = EditorGUILayout.FloatField(yScale);
        zScale = EditorGUILayout.FloatField(zScale);
        EditorGUILayout.EndHorizontal();

        float averageScale = (xScale + yScale + zScale) * 0.3333333333f;
        float newAverageScale = EditorGUILayout.FloatField("Average", averageScale);

        if(!Mathf.Approximately(averageScale, newAverageScale))
        {
            float multiplier = newAverageScale / averageScale;
            xScale *= multiplier;
            yScale *= multiplier;
            zScale *= multiplier;
        }

        if (GUILayout.Button("Apply scale"))
        {
            // ToDo: Apply scale
        }
    }
}

#endif