#if UNITY_EDITOR

using iffnsStuff.MarchingCubeEditor.Core;
using iffnsStuff.MarchingCubeEditor.SceneEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SizeAndLoaderEditorElement : EditorElement
{
    public override string DisplayName => "Size & Loader";
    
    int gridResolutionX = 20;
    int gridResolutionY = 20;
    int gridResolutionZ = 20;

    public SizeAndLoaderEditorElement(MarchingCubeEditor linkedEditor, bool foldoutOpenByDefault) : base(linkedEditor, foldoutOpenByDefault)
    {
        // Constructor
        int gridResolutionX = linkedController.GridResolutionX;
        int gridResolutionY = linkedController.GridResolutionY;
        int gridResolutionZ = linkedController.GridResolutionZ;
    }

    public override void DrawUI()
    {
        // Normal grid size
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("X");
        GUILayout.Label("Y");
        GUILayout.Label("Z");
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label($"{linkedController.GridResolutionX}");
        GUILayout.Label($"{linkedController.GridResolutionY}");
        GUILayout.Label($"{linkedController.GridResolutionZ}");
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        gridResolutionX = EditorGUILayout.IntField(gridResolutionX);
        gridResolutionY = EditorGUILayout.IntField(gridResolutionY);
        gridResolutionZ = EditorGUILayout.IntField(gridResolutionZ);
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Apply and set empty"))
        {
            linkedController.Initialize(gridResolutionX, gridResolutionY, gridResolutionZ, true);
        }

        // Save and load
        GUILayout.Label("Save data:");
        ScriptableObjectSaveData newSaveData = EditorGUILayout.ObjectField(
        linkedController.linkedSaveData,
        typeof(ScriptableObjectSaveData),
        true) as ScriptableObjectSaveData;


        if (newSaveData != linkedController.linkedSaveData)
        {
            Undo.RecordObject(linkedController, "Set save data file");
            linkedController.linkedSaveData = newSaveData;
            EditorUtility.SetDirty(linkedController);
            if (!Application.isPlaying)
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(linkedController.gameObject.scene);
        }

        if (linkedController.linkedSaveData != null)
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button($"Save data")) linkedController.SaveAndLoadManager.SaveGridData(linkedController.linkedSaveData);
            if (GUILayout.Button($"Load data")) linkedEditor.LoadData();
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndVertical();
    }
}

#endif