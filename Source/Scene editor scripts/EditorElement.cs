using iffnsStuff.MarchingCubeEditor.Core;
using iffnsStuff.MarchingCubeEditor.SceneEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public abstract class EditorElement
{
    public abstract string DisplayName { get; }

    protected MarchingCubeEditor linkedEditor;
    protected MarchingCubesController linkedController => linkedEditor.LinkedMarchingCubeController;

    public abstract void DrawUI();

    public bool foldoutOpen;

    public EditorElement(MarchingCubeEditor linkedEditor, bool foldoutOpenByDefault)
    {
        this.linkedEditor = linkedEditor;
        foldoutOpen = foldoutOpenByDefault;
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
