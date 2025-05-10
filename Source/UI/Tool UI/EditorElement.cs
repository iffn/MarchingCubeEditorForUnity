#if UNITY_EDITOR
using iffnsStuff.MarchingCubeEditor.Core;
using iffnsStuff.MarchingCubeEditor.SceneEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public abstract class EditorElement
{
    public List<GenericPersistentUI.UIElement> GenericUIElements { get; } = new List<GenericPersistentUI.UIElement>();

    protected abstract void GeneratePersistentUI();

    public abstract string DisplayName { get; }

    protected MarchingCubeEditor linkedEditor;
    protected MarchingCubesController linkedController => linkedEditor.LinkedMarchingCubeController;

    public GenericPersistentUI.Foldout Foldout { get; private set; }

    public abstract void DrawUI();

    bool foldoutOpen;

    public EditorElement(MarchingCubeEditor linkedEditor, bool foldoutOpenByDefault)
    {
        this.linkedEditor = linkedEditor;
        foldoutOpen = foldoutOpenByDefault;
        Foldout = new GenericPersistentUI.Foldout(DisplayName, GenericUIElements, foldoutOpenByDefault);

        GeneratePersistentUI();
    }

    public virtual void OnEnable()
    {

    }

    public void DrawAsFoldout()
    {
        if (foldoutOpen)
        {
            foldoutOpen = EditorGUILayout.BeginFoldoutHeaderGroup(foldoutOpen, $"{DisplayName}:");
            DrawUI();
        }
        else
        {
            foldoutOpen = EditorGUILayout.BeginFoldoutHeaderGroup(foldoutOpen, DisplayName); // Only show colon when foldout is open
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
    }
}
#endif