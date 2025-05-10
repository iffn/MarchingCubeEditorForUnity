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

    protected override void GeneratePersistentUI()
    {
        // Row 1: Column headers (X, Y, Z)
        GenericUIElements.Add
        (
            new GenericPersistentUI.HorizontalArrangement
            (
                new List<GenericPersistentUI.UIElement>
                {
                    new GenericPersistentUI.RefLabel("", () => "X"),
                    new GenericPersistentUI.RefLabel("", () => "Y"),
                    new GenericPersistentUI.RefLabel("", () => "Z")
                }
            )
        );

        // Row 2: Current resolution values
        GenericUIElements.Add
        (
            new GenericPersistentUI.HorizontalArrangement
            (
                new List<GenericPersistentUI.UIElement>
                {
                    new GenericPersistentUI.RefLabel("", () => linkedController.GridResolutionX.ToString()),
                    new GenericPersistentUI.RefLabel("", () => linkedController.GridResolutionY.ToString()),
                    new GenericPersistentUI.RefLabel("", () => linkedController.GridResolutionZ.ToString())
                }
            )
        );

        // Row 3: Editable input fields
        GenericUIElements.Add
        (
            new GenericPersistentUI.HorizontalArrangement
            (
                new List<GenericPersistentUI.UIElement>
                {
                    new GenericPersistentUI.IntField("", () => gridResolutionX, v => gridResolutionX = v),
                    new GenericPersistentUI.IntField("", () => gridResolutionY, v => gridResolutionY = v),
                    new GenericPersistentUI.IntField("", () => gridResolutionZ, v => gridResolutionZ = v)
                }
            )
        );

        // Final row: Apply button
        GenericUIElements.Add
        (
            new GenericPersistentUI.Button("Apply and set empty", () =>
            {
                linkedController.Initialize(gridResolutionX, gridResolutionY, gridResolutionZ, true, false);
            })
        );

        GenericUIElements.Add
        (
            new GenericPersistentUI.ScriptableObjectField<ScriptableObjectSaveData>
            (
                "Save Data",
                () => linkedController.linkedSaveData,
                val => linkedController.linkedSaveData = val
            )
        );

        // Outer conditional block: only display if save data exists
        GenericUIElements.Add
        (
            new GenericPersistentUI.DisplayIfTrue
            (
                () => linkedController.linkedSaveData != null,
                new List<GenericPersistentUI.UIElement>
                {
                    // First conditional warning: Views not set up
                    new GenericPersistentUI.DisplayIfTrue
                    (
                        () => !linkedController.ViewsSetUp,
                        new List<GenericPersistentUI.UIElement>
                        {
                            new GenericPersistentUI.Heading("Note: Views are not set up. Please load data first or set it to empty.")
                        }
                    ),

                    // Second conditional warning: Grid resolution mismatch
                    new GenericPersistentUI.DisplayIfTrue
                    (
                        () =>
                            linkedController.linkedSaveData.resolutionX != linkedController.GridResolutionX ||
                            linkedController.linkedSaveData.resolutionY != linkedController.GridResolutionY ||
                            linkedController.linkedSaveData.resolutionZ != linkedController.GridResolutionZ,
                        new List<GenericPersistentUI.UIElement>
                        {
                            new GenericPersistentUI.Heading("Note: The grid resolution is different. Make sure everything is correct before saving.")
                        }
                    ),

                    // Buttons row
                    new GenericPersistentUI.HorizontalArrangement
                    (
                        new List<GenericPersistentUI.UIElement>
                        {
                            // Save button (only if ViewsSetUp)
                            new GenericPersistentUI.DisplayIfTrue
                            (
                                () => linkedController.ViewsSetUp,
                                new List<GenericPersistentUI.UIElement>
                                {
                                    new GenericPersistentUI.Button("Save data", () =>
                                        linkedController.SaveAndLoadManager.SaveGridData(linkedController.linkedSaveData))
                                }
                            ),

                            // Always show Load button
                            new GenericPersistentUI.Button("Load data", () =>
                                linkedEditor.LoadData())
                        }
                    )
                }
            )
        );
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
            linkedController.Initialize(gridResolutionX, gridResolutionY, gridResolutionZ, true, false);
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
            if(!linkedController.ViewsSetUp)
                EditorGUILayout.HelpBox("Note: Views are not set up. Please load data first or set it to empty.", MessageType.Warning);
            else if (linkedController.linkedSaveData.resolutionX != linkedController.GridResolutionX
                || linkedController.linkedSaveData.resolutionY != linkedController.GridResolutionY
                || linkedController.linkedSaveData.resolutionZ != linkedController.GridResolutionZ)
                EditorGUILayout.HelpBox("Note: The grid resolution is different. Make sure everything is correct before saving.", MessageType.Warning);

            EditorGUILayout.BeginHorizontal();

            if (linkedController.ViewsSetUp)
            {
                if (GUILayout.Button($"Save data"))
                    linkedController.SaveAndLoadManager.SaveGridData(linkedController.linkedSaveData);
            }

            if (GUILayout.Button($"Load data"))
                linkedEditor.LoadData();

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndVertical();
    }
}

#endif