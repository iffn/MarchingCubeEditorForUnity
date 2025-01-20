using iffnsStuff.MarchingCubeEditor.Core;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http.Headers;
using UnityEngine;

public class MarchingCubesPreview : MarchingCubesView
{
    [SerializeField] MeshRenderer linkedMeshRenderer;
    [SerializeField] Material AdditionPreviewMaterial;
    [SerializeField] Material SubtractionPreviewMaterial;

    public enum PreviewDisplayStates
    {
        addition,
        subtraction
    }

    public void ShowPreviewState(PreviewDisplayStates state)
    {
        Material previewMaterial = AdditionPreviewMaterial;

        switch (state)
        {
            case PreviewDisplayStates.addition:
                linkedMeshRenderer.sharedMaterial = AdditionPreviewMaterial;
                break;
            case PreviewDisplayStates.subtraction:
                linkedMeshRenderer.sharedMaterial = SubtractionPreviewMaterial;
                break;
            default:
                break;
        }
    }
}
