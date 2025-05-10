#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;


public static class UnityEditorUIRenderer
{
    public static void RenderUI(GenericPersistentUI.UIElement element)
    {
        DrawElement(element);
    }

    public static void RenderUI(List<GenericPersistentUI.UIElement> elements)
    {
        foreach (GenericPersistentUI.UIElement element in elements)
        {
            DrawElement(element);
        }
    }

    static void DrawElement(GenericPersistentUI.UIElement element)
    {
        if (element is GenericPersistentUI.Toggle toggle)
        {
            toggle.Value = EditorGUILayout.Toggle(toggle.Title, toggle.Value);
        }
        else if (element is GenericPersistentUI.Slider slider)
        {
            slider.Value = EditorGUILayout.Slider(slider.Title, slider.min, slider.max, slider.Value);
        }
        else if (element is GenericPersistentUI.Button button)
        {
            if (GUILayout.Button(button.Title))
                button.Invoke();
        }
        else if (element is GenericPersistentUI.Heading heading)
        {
            GUILayout.Label(heading.Title);
        }
        else if (element is GenericPersistentUI.RefLabel refLabel)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(refLabel.Title);
            GUILayout.Label(refLabel.Value);
            EditorGUILayout.EndHorizontal();
        }
        else if (element is GenericPersistentUI.Dropdown dropdown)
        {
            int newValue = EditorGUILayout.Popup(dropdown.Title, dropdown.Value, dropdown.Options);
            dropdown.Value = newValue;
        }
        // Unity specific stuff
        else if (element is GenericPersistentUI.MaterialField matField)
        {
            Material newMat = EditorGUILayout.ObjectField(
                matField.Title,
                matField.Value,
                typeof(Material),
                true) as Material;

            matField.Value = newMat;
        }
        else if (element is GenericPersistentUI.HorizontalArrangement row)
        {
            EditorGUILayout.BeginHorizontal();
            foreach (var child in row.Elements)
                DrawElement(child);
            EditorGUILayout.EndHorizontal();
        }
        // Organization stuff
        else if (element is GenericPersistentUI.Foldout foldout)
        {
            foldout.Open = EditorGUILayout.BeginFoldoutHeaderGroup(foldout.Open, foldout.Title); // Wokrs fine with nested layouts. BeginFoldoutHeaderGroup could have problems.
            EditorGUILayout.EndFoldoutHeaderGroup();

            if (foldout.Open)
            {
                EditorGUI.indentLevel++;
                foreach (var child in foldout.Elements)
                    DrawElement(child);
                EditorGUI.indentLevel--;
            }
        }

    }
}

#endif