#if UNITY_EDITOR

using iffnsStuff.MarchingCubeEditor.Core;
using iffnsStuff.MarchingCubeEditor.SceneEditor;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using static BaseModificationTools;

public class ExpansionEditorElement : EditorElement
{
    public override string DisplayName => "Expansion and resolution";

    public ExpansionEditorElement(MarchingCubeEditor linkedEditor, bool foldoutOpenByDefault) : base(linkedEditor, foldoutOpenByDefault)
    {
        // Constructor
    }

    int gridCExpandSize = 0;
    bool moveTransformWhenExpanding = true;
    float xScale = 1;
    float yScale = 1;
    float zScale = 1;

    public override void OnEnable()
    {
        base.OnEnable();

        if(linkedController != null)
        {
            xScale = linkedController.transform.localScale.x;
            yScale = linkedController.transform.localScale.y;
            zScale = linkedController.transform.localScale.z;
        }
    }

    protected override void GeneratePersistentUI()
    {
        GenericUIElements.Clear();

        GenericUIElements.Add(new GenericPersistentUI.IntField(
            "Expansion size",
            () => gridCExpandSize,
            val => gridCExpandSize = val
        ));

        GenericUIElements.Add
        (
            new GenericPersistentUI.HorizontalArrangement
            (
                new List<GenericPersistentUI.UIElement>
                {
                    new GenericPersistentUI.Button("Expand +X", () =>
                        linkedController.ExpandGrid(gridCExpandSize, MarchingCubesController.ExpansionDirections.XPos)),

                    new GenericPersistentUI.Button("Expand +Y", () =>
                        linkedController.ExpandGrid(gridCExpandSize, MarchingCubesController.ExpansionDirections.YPos)),

                    new GenericPersistentUI.Button("Expand +Z", () =>
                        linkedController.ExpandGrid(gridCExpandSize, MarchingCubesController.ExpansionDirections.ZPos))
                }
            )
        );

        GenericUIElements.Add
        (
            new GenericPersistentUI.HorizontalArrangement
            (
                new List<GenericPersistentUI.UIElement>
                {
                    new GenericPersistentUI.Button("Expand -X", () =>
                    {
                        linkedController.ExpandGrid(gridCExpandSize, MarchingCubesController.ExpansionDirections.XNeg);

                        if (moveTransformWhenExpanding)
                            linkedController.transform.localPosition -=
                                gridCExpandSize * linkedController.transform.localScale.x * Vector3.right;
                    }),

                    new GenericPersistentUI.Button("Expand -Y", () =>
                    {
                        linkedController.ExpandGrid(gridCExpandSize, MarchingCubesController.ExpansionDirections.YNeg);

                        if (moveTransformWhenExpanding)
                            linkedController.transform.localPosition -=
                                gridCExpandSize * linkedController.transform.localScale.y * Vector3.up;
                    }),

                    new GenericPersistentUI.Button("Expand -Z", () =>
                    {
                        linkedController.ExpandGrid(gridCExpandSize, MarchingCubesController.ExpansionDirections.ZNeg);

                        if (moveTransformWhenExpanding)
                            linkedController.transform.localPosition -=
                                gridCExpandSize * linkedController.transform.localScale.z * Vector3.forward;
                    })
                }
            )
        );

        GenericUIElements.Add
        (
            new GenericPersistentUI.Toggle
            (
                "Move transform to keep position",
                () => moveTransformWhenExpanding,
                val => moveTransformWhenExpanding = val
            )
        );

        GenericUIElements.Add(new GenericPersistentUI.Heading("Resolution change"));

        GenericUIElements.Add
        (
            new GenericPersistentUI.HorizontalArrangement
            (
                new List<GenericPersistentUI.UIElement>
                {
                    new GenericPersistentUI.Heading("X"),
                    new GenericPersistentUI.Heading("Y"),
                    new GenericPersistentUI.Heading("Z"),
                }
            )
        );

        GenericUIElements.Add
        (
            new GenericPersistentUI.HorizontalArrangement
            (
                new List<GenericPersistentUI.UIElement>
                {
                    new GenericPersistentUI.RefLabel("", () => linkedController.transform.localScale.x.ToString("0.###")),
                    new GenericPersistentUI.RefLabel("", () => linkedController.transform.localScale.y.ToString("0.###")),
                    new GenericPersistentUI.RefLabel("", () => linkedController.transform.localScale.z.ToString("0.###"))
                }
            )
        );

        GenericUIElements.Add
        (
            new GenericPersistentUI.HorizontalArrangement
            (
                new List<GenericPersistentUI.UIElement>
                {
                    new GenericPersistentUI.FloatField("", () => xScale, val => xScale = val),
                    new GenericPersistentUI.FloatField("", () => yScale, val => yScale = val),
                    new GenericPersistentUI.FloatField("", () => zScale, val => zScale = val)
                }
            )
        );

        GenericUIElements.Add
        (
            new GenericPersistentUI.FloatField
            (
                "Average",
                () => (xScale + yScale + zScale) * 0.3333333333f,
                newAverage =>
                {
                    float average = (xScale + yScale + zScale) * 0.3333333333f;

                    if (newAverage != 0f && !Mathf.Approximately(average, newAverage))
                    {
                        float multiplier = newAverage / average;
                        xScale *= multiplier;
                        yScale *= multiplier;
                        zScale *= multiplier;
                    }
                }
            )
        );

        GenericUIElements.Add
        (
            new GenericPersistentUI.Button("Apply resolution", ApplyResolution)
        );
    }

    public override void DrawUI()
    {
        gridCExpandSize = EditorGUILayout.IntField("Expansion size", gridCExpandSize);

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Expand +X"))
        {
            linkedController.ExpandGrid(gridCExpandSize, MarchingCubesController.ExpansionDirections.XPos);
        }

        if (GUILayout.Button("Expand +Y"))
        {
            linkedController.ExpandGrid(gridCExpandSize, MarchingCubesController.ExpansionDirections.YPos);
        }

        if (GUILayout.Button("Expand +Z"))
        {
            linkedController.ExpandGrid(gridCExpandSize, MarchingCubesController.ExpansionDirections.ZPos);
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Expand -X"))
        {
            linkedController.ExpandGrid(gridCExpandSize, MarchingCubesController.ExpansionDirections.XNeg);

            if (moveTransformWhenExpanding)
                linkedController.transform.localPosition -= gridCExpandSize * linkedController.transform.localScale.x * Vector3.right;
        }

        if (GUILayout.Button("Expand -Y"))
        {
            linkedController.ExpandGrid(gridCExpandSize, MarchingCubesController.ExpansionDirections.YNeg);

            if (moveTransformWhenExpanding)
                linkedController.transform.localPosition -= gridCExpandSize * linkedController.transform.localScale.y * Vector3.up;
        }

        if (GUILayout.Button("Expand -Z"))
        {
            linkedController.ExpandGrid(gridCExpandSize, MarchingCubesController.ExpansionDirections.ZNeg);

            if (moveTransformWhenExpanding)
                linkedController.transform.localPosition -= gridCExpandSize * linkedController.transform.localScale.z * Vector3.forward;
        }

        EditorGUILayout.EndHorizontal();

        moveTransformWhenExpanding = EditorGUILayout.Toggle("Move transform to keep position", moveTransformWhenExpanding);

        // Scale up layout
        EditorGUILayout.LabelField("Resolution change");

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("X");
        GUILayout.Label("Y");
        GUILayout.Label("Z");
        EditorGUILayout.EndHorizontal();

        Vector3 currentLocalScale = linkedController.transform.localScale;

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label($"{currentLocalScale.x}");
        GUILayout.Label($"{currentLocalScale.y}");
        GUILayout.Label($"{currentLocalScale.z}");
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        xScale = EditorGUILayout.FloatField(xScale);
        yScale = EditorGUILayout.FloatField(yScale);
        zScale = EditorGUILayout.FloatField(zScale);
        EditorGUILayout.EndHorizontal();

        float averageScale = (xScale + yScale + zScale) * 0.3333333333f;
        float newAverageScale = EditorGUILayout.FloatField("Average", averageScale);

        if(newAverageScale != 0f && !Mathf.Approximately(averageScale, newAverageScale))
        {
            float multiplier = newAverageScale / averageScale;
            xScale *= multiplier;
            yScale *= multiplier;
            zScale *= multiplier;
        }

        if (GUILayout.Button("Apply resolution"))
            ApplyResolution();
    }

    void ApplyResolution()
    {
        // Copy data
        Matrix4x4 initialTransformWTL = linkedController.transform.worldToLocalMatrix;
        VoxelData[,,] currentDataCopy = BaseTool.GenerateVoxelDataCopy(linkedController);

        // Change size
        Vector3 oldScale = linkedController.transform.localScale;

        // Change size
        int xSize = Mathf.RoundToInt(linkedController.GridResolutionX * (oldScale.x / xScale));
        int ySize = Mathf.RoundToInt(linkedController.GridResolutionY * (oldScale.y / yScale));
        int zSize = Mathf.RoundToInt(linkedController.GridResolutionZ * (oldScale.z / zScale));

        linkedController.transform.localScale = new Vector3(xScale, yScale, zScale);

        // Paste
        Matrix4x4 newTransformWTL = linkedController.transform.worldToLocalMatrix;
        CopyModifier copyModifier = new CopyModifier(currentDataCopy, newTransformWTL, initialTransformWTL, newTransformWTL); // ToDo: Fix when resolution is not (1,1,1)

        VoxelData[,,] newData = new VoxelData[xSize, ySize, zSize];

        Parallel.For(0, xSize, x =>
        {
            for (int y = 0; y < ySize; y++)
            {
                for (int z = 0; z < zSize; z++)
                {
                    // Transform grid position to world space
                    Vector3 gridPoint = new Vector3(x, y, z);

                    // Modify the voxel value
                    VoxelData newValue = copyModifier.ModifyVoxel(x, y, z, newData[x, y, z], -1f);

                    newData[x, y, z] = newValue;
                }
            }
        });

        linkedController.SetAllGridDataAndUpdateMesh(newData);
    }
}

#endif