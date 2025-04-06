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

        Material currentMaterial;
        Material newMaterial;

        // Main material
        currentMaterial = linkedController.CurrentMainMaterial;

        newMaterial = EditorGUILayout.ObjectField(
           "Grass material",
           currentMaterial,
           typeof(Material),
           true) as Material;

        if(currentMaterial != newMaterial)
            linkedController.CurrentMainMaterial = newMaterial;
            

        // Grass material
        currentMaterial = linkedController.CurrentGrassMaterial;

        newMaterial = EditorGUILayout.ObjectField(
           "Grass material", 
           currentMaterial,
           typeof(Material),
           true) as Material;

        if (currentMaterial != newMaterial)
            linkedController.CurrentGrassMaterial = newMaterial;
            
    }
}

#endif