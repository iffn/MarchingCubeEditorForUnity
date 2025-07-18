#if UNITY_EDITOR

using iffnsStuff.MarchingCubeEditor.SceneEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;
using static UnityEngine.GraphicsBuffer;

public class ToolEditorElement : EditorElement
{
    public override string DisplayName => "Tools";
    readonly List<BaseTool> tools;

    public ToolEditorElement(MarchingCubeEditor linkedEditor, bool foldoutOpenByDefault) : base(linkedEditor, foldoutOpenByDefault)
    {
        tools = BaseTool.GetTools(linkedEditor).ToList();
    }

    BaseTool currentTool
    {
        get
        {
            return linkedEditor.CurrentTool;
        }
        set
        {
            linkedEditor.CurrentTool = value;
        }
    }

    public override void DrawUI()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        if (!linkedController.ViewsSetUp)
        {
            EditorGUILayout.HelpBox("Views are not set up. Please load data first or set it to empty.", MessageType.Warning);
            EditorGUILayout.EndVertical();
            return;
        }

        int columns = 2; // Number of buttons per row

        Color highlightColor = new Color(0.7f, 0.7f, 1f); //ToDo: Improve highlight color

        for (int i = 0; i < tools.Count; i += columns)
        {
            EditorGUILayout.BeginHorizontal();
            for (int j = 0; j < columns; j++)
            {
                int index = i + j;
                if (index < tools.Count) // Ensure index is within bounds
                {
                    // Store original colors
                    Color originalBackground = GUI.backgroundColor;
                    Color originalContentColor = GUI.contentColor;

                    if (tools[index] == currentTool)
                    {
                        // Set custom colors for the selected tool
                        GUI.backgroundColor = highlightColor;
                        GUI.contentColor = Color.white; // Text color
                    }

                    if (GUILayout.Button(tools[index].DisplayName))
                    {
                        if (currentTool == tools[index])
                        {
                            currentTool = null;
                        }
                        else
                        {
                            currentTool = tools[index];
                        }
                    }

                    // Restore original colors
                    GUI.backgroundColor = originalBackground;
                    GUI.contentColor = originalContentColor;
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        // Draw current tool UI
        if (currentTool != null)
        {
            GUILayout.Label($"{currentTool.DisplayName}:");
            currentTool.DrawUI();
        }

        EditorGUILayout.EndVertical();
    }
}

#endif