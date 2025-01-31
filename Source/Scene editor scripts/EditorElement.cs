#if UNITY_EDITOR
using iffnsStuff.MarchingCubeEditor.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public abstract class EditorElement
{
    public abstract string DisplayName { get; }


    public abstract void DrawUI(MarchingCubesController linkedController);

    public bool foldoutOpen;

    public EditorElement(bool foldoutOpenByDefault)
    {
        foldoutOpen = foldoutOpenByDefault;
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
#endif