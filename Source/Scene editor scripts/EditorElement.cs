using iffnsStuff.MarchingCubeEditor.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public abstract class EditorElement
{
    public abstract string DisplayName { get; }


    public abstract void DrawUI(MarchingCubesController linkedController);

    protected abstract bool foldoutOutOpenByDefault { get; }
    public bool foldoutOpen;

    public EditorElement()
    {
        foldoutOpen = foldoutOutOpenByDefault;
    }

    public void DrawAsFoldout(MarchingCubesController linkedController)
    {
        foldoutOpen = EditorGUILayout.BeginFoldoutHeaderGroup(foldoutOpen, DisplayName);
        if (foldoutOpen)
        {
            DrawUI(linkedController);
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
    }
}
