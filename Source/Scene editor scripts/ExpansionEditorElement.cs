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

    void UpdateGridResolutionFromController()
    {
        // ToDo
    }

    public override void DrawUI()
    {
        gridCExpandSize = EditorGUILayout.IntField("Expansion size", gridCExpandSize);

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Expand +X"))
        {
            linkedController.ExpandGrid(gridCExpandSize, MarchingCubesController.ExpansionDirections.XPos);
            UpdateGridResolutionFromController();
        }

        if (GUILayout.Button("Expand +Y"))
        {
            linkedController.ExpandGrid(gridCExpandSize, MarchingCubesController.ExpansionDirections.YPos);
            UpdateGridResolutionFromController();
        }

        if (GUILayout.Button("Expand +Z"))
        {
            linkedController.ExpandGrid(gridCExpandSize, MarchingCubesController.ExpansionDirections.ZPos);
            UpdateGridResolutionFromController();
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Expand -X"))
        {
            linkedController.ExpandGrid(gridCExpandSize, MarchingCubesController.ExpansionDirections.XNeg);

            if (moveTransformWhenExpanding)
                linkedController.transform.localPosition -= gridCExpandSize * linkedController.transform.localScale.x * Vector3.right;

            UpdateGridResolutionFromController();
        }

        if (GUILayout.Button("Expand -Y"))
        {
            linkedController.ExpandGrid(gridCExpandSize, MarchingCubesController.ExpansionDirections.YNeg);

            if (moveTransformWhenExpanding)
                linkedController.transform.localPosition -= gridCExpandSize * linkedController.transform.localScale.y * Vector3.up;

            UpdateGridResolutionFromController();
        }

        if (GUILayout.Button("Expand -Z"))
        {
            linkedController.ExpandGrid(gridCExpandSize, MarchingCubesController.ExpansionDirections.ZNeg);

            if (moveTransformWhenExpanding)
                linkedController.transform.localPosition -= gridCExpandSize * linkedController.transform.localScale.z * Vector3.forward;

            UpdateGridResolutionFromController();
        }

        EditorGUILayout.EndHorizontal();

        moveTransformWhenExpanding = EditorGUILayout.Toggle("Move transform to keep position", moveTransformWhenExpanding);
    }
}

#endif