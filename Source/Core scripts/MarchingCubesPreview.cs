#if UNITY_EDITOR

using iffnsStuff.MarchingCubeEditor.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarchingCubesPreview : MarchingCubesView
{
    [SerializeField] Material AdditionPreviewMaterial;
    [SerializeField] Material SubtractionPreviewMaterial;

    public enum PreviewDisplayStates
    {
        addition,
        subtraction
    }

    public void SetPreviewDisplayState(PreviewDisplayStates state)
    {
        switch (state)
        {
            case PreviewDisplayStates.addition:
                CurrentMainMaterial = AdditionPreviewMaterial;
                break;
            case PreviewDisplayStates.subtraction:
                CurrentMainMaterial = SubtractionPreviewMaterial;
                break;
            default:
                break;
        }
    }
}

#endif