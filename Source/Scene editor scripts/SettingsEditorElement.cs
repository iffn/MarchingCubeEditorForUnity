#if UNITY_EDITOR

using iffnsStuff.MarchingCubeEditor.Core;
using iffnsStuff.MarchingCubeEditor.SceneEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SettingsEditorElement : EditorElement
{
    public override string DisplayName => "Settings";

    public SettingsEditorElement(MarchingCubeEditor linkedEditor, bool foldoutOpenByDefault) : base(linkedEditor, foldoutOpenByDefault)
    {
        // Constructor
    }

    public override void DrawUI()
    {
        linkedController.ForceColliderOn = EditorGUILayout.Toggle("Force colliders on", linkedController.ForceColliderOn);

        linkedController.VisualisationManager.ShowGridOutline = EditorGUILayout.Toggle("Show Grid Outline", linkedController.VisualisationManager.ShowGridOutline);
        linkedController.InvertAllNormals = EditorGUILayout.Toggle("Inverted normals", linkedController.InvertAllNormals);

        Material currentMaterial = linkedController.CurrentMaterial;

        Material newMaterial = EditorGUILayout.ObjectField(
           currentMaterial,
           typeof(Material),
           true) as Material;

        if(currentMaterial != newMaterial)
            linkedController.CurrentMaterial = newMaterial;
    }
}

#endif