using iffnsStuff.MarchingCubeEditor.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PostProcessingEditorElement : EditorElement
{
    public override string DisplayName => "Post processing";
    protected override bool foldoutOutOpenByDefault => false;

    // Post processing options
    bool createOneChunk;
    bool smoothNormals;
    bool mergeTriangles;
    bool updateWhileEditing;
    float maxProcessingTimeSeconds = 10;

    // Output
    float lastProcessingTimeSeconds = 0;
    int lastMergedElemements = 0;


    public override void DrawUI(MarchingCubesController linkedController)
    {
        linkedController.PostProcessMesh = EditorGUILayout.Toggle("Post process mesh (slow)", linkedController.PostProcessMesh);

        if (linkedController.PostProcessMesh)
        {
            linkedController.AngleThresholdDeg = EditorGUILayout.FloatField("Angle threshold [°]", linkedController.AngleThresholdDeg);
            linkedController.AreaThreshold = EditorGUILayout.FloatField("Area threshold", linkedController.AreaThreshold);
        }
    }
}
