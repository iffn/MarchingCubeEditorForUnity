#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using iffnsStuff.MarchingCubeEditor.Core;
using iffnsStuff.MarchingCubeEditor.SceneEditor;
using UnityEditor;
using UnityEngine;

public abstract class BaseTool
{
    public BaseTool(MarchingCubeEditor editor)
    {
        this.LinkedMarchingCubeEditor = editor;
        GeneratePersistentUI();
    }

    public List<GenericPersistentUI.UIElement> GenericUIElements { get; } = new List<GenericPersistentUI.UIElement>();

    protected abstract void GeneratePersistentUI();

    protected MarchingCubeEditor LinkedMarchingCubeEditor { get; private set; }
    protected MarchingCubesController LinkedMarchingCubeController => LinkedMarchingCubeEditor.LinkedMarchingCubeController;

    public abstract string DisplayName { get; }
    public virtual Texture DisplayIcon => null;

    public static readonly Color highlightBackgroundColor = new Color(0.7f, 0.7f, 1f); //ToDo: Improve highlight color

    public virtual void OnEnable()
    {
        LinkedMarchingCubeController.EnableAllColliders = true;
    }

    public virtual void OnDisable()
    {
        LinkedMarchingCubeController.EnableAllColliders = false; // Will still be on if force collider on is set
        LinkedMarchingCubeController.DisplayPreviewShape = false;
    }
    public virtual void DrawUI() {}
    public virtual void HandleSceneUpdate(Event currentEvent) {}
    public virtual void DrawGizmos() {}

    protected string helpText = "Controls:\n" +
                    "Note that the scene has to be active for some of these to work. Right clicking into the scene view works well for this.\n";

    // Search for all Classes inheriting from BaseTool. We do this here so
    // that we only need to search once.
    public static readonly List<Type> Tools = AppDomain.CurrentDomain
        .GetAssemblies()
        .SelectMany(assembly => assembly.GetTypes())
        .Where(type => type.IsSubclassOf(typeof(BaseTool)) && !type.IsAbstract)
        .ToList();

    // Instantiates the found BaseTool Classes and sets the reference to the
    // current editor.
    public static IEnumerable<BaseTool> GetTools(MarchingCubeEditor editor)
    {
        List<BaseTool> result = new List<BaseTool>();

        foreach (Type type in Tools)
        {
            try
            {
                ConstructorInfo constructor = type.GetConstructor(new[] { typeof(MarchingCubeEditor) });

                if (constructor == null)
                {
                    Debug.LogError($"{type.Name} must have a public constructor with (MarchingCubeEditor editor).");
                    continue;
                }

                BaseTool tool = constructor.Invoke(new object[] { editor }) as BaseTool;

                if (tool != null)
                {
                    result.Add(tool);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to instantiate {type.Name}: {ex.Message}");
            }
        }

        return result;
    }

    public static bool LeftClickDownEvent(Event e)
    {
        return e.type == EventType.MouseDown && e.button == 0;
    }

    public static bool EscapeDownEvent(Event e)
    {
        return e.type == EventType.KeyDown && e.keyCode == KeyCode.Escape;
    }

    public static bool ControlIsHeld(Event e)
    {
        return e.control;
    }

    public void RefreshUI()
    {
        LinkedMarchingCubeEditor.RequestRefresh();
    }

    protected void DrawTransformFields(Transform selectedTransform)
    {
        // Draw Transform Fields
        EditorGUI.BeginChangeCheck();

        Vector3 newPosition = EditorGUILayout.Vector3Field("Position", selectedTransform.position);
        Vector3 newRotation = EditorGUILayout.Vector3Field("Rotation", selectedTransform.eulerAngles);
        Vector3 newScale = EditorGUILayout.Vector3Field("Scale", selectedTransform.localScale);

        // Compute Average Scale
        float avgScale = (newScale.x + newScale.y + newScale.z) / 3f;
        float newAvgScale = EditorGUILayout.FloatField("Uniform Scale", avgScale);

        if (EditorGUI.EndChangeCheck()) // If a value was changed
        {
            Undo.RecordObject(selectedTransform, "Transform Change"); // Register for undo

            selectedTransform.position = newPosition;
            selectedTransform.eulerAngles = newRotation;

            if (!Mathf.Approximately(newAvgScale, avgScale)) // If uniform scale is changed
            {
                float scaleFactor = newAvgScale / avgScale;
                selectedTransform.localScale *= scaleFactor; // Scale uniformly
            }
            else
            {
                selectedTransform.localScale = newScale; // Apply normal scaling
            }

            EditorUtility.SetDirty(selectedTransform); // Ensure the change is applied
        }
    }


    public static VoxelData[,,] GenerateVoxelDataCopy(MarchingCubesController linkedController, Vector3Int minGrid, Vector3Int maxGrid, int maxOffset)
    {
        VoxelData[,,] oldData = new VoxelData[
            linkedController.VoxelDataReference.GetLength(0),
            linkedController.VoxelDataReference.GetLength(1),
            linkedController.VoxelDataReference.GetLength(2)
        ];

        minGrid -= maxOffset * Vector3Int.one;
        maxGrid += maxOffset * Vector3Int.one;

        minGrid.x = Math.Max(minGrid.x, 0);
        minGrid.y = Math.Max(minGrid.y, 0);
        minGrid.z = Math.Max(minGrid.z, 0);

        maxGrid.x = Math.Min(maxGrid.x, oldData.GetLength(0));
        maxGrid.y = Math.Min(maxGrid.y, oldData.GetLength(1));
        maxGrid.z = Math.Min(maxGrid.z, oldData.GetLength(2));

        Parallel.For(minGrid.x, maxGrid.x, x =>
        {
            for (int y = minGrid.y; y < maxGrid.y; y++)
            {
                for (int z = minGrid.z; z < maxGrid.z; z++)
                {
                    oldData[x, y, z] = linkedController.VoxelDataReference[x, y, z];
                }
            }
        });

        return oldData;
    }

    public static VoxelData[,,] GenerateVoxelDataCopy(MarchingCubesController linkedController)
    {
        VoxelData[,,] oldData = new VoxelData[
            linkedController.VoxelDataReference.GetLength(0),
            linkedController.VoxelDataReference.GetLength(1),
            linkedController.VoxelDataReference.GetLength(2)
        ];

        Parallel.For(0, oldData.GetLength(0), x =>
        {
            for (int y = 0; y < oldData.GetLength(1); y++)
            {
                for (int z = 0; z < oldData.GetLength(2); z++)
                {
                    oldData[x, y, z] = linkedController.VoxelDataReference[x, y, z];
                }
            }
        });

        return oldData;
    }
}

#endif