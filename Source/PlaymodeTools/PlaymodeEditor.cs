using iffnsStuff.MarchingCubeEditor.Core;
using iffnsStuff.MarchingCubeEditor.EditTools;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public abstract class PlaymodeEditor : MonoBehaviour
{
    [SerializeField] protected MarchingCubesController linkedMarchingCubeController;
    [SerializeField] protected EditShape placeableByClick;

    protected void InitializeController()
    {
        linkedMarchingCubeController.ClearAllViews();
        linkedMarchingCubeController.Initialize(1, 1, 1, true, false);
        LoadData();
    }

    protected void LoadData()
    {
        if (linkedMarchingCubeController.linkedSaveData == null)
            return;

        linkedMarchingCubeController.SaveAndLoadManager.LoadGridData(linkedMarchingCubeController.linkedSaveData);
    }

    protected void SaveData()
    {
        ScriptableObjectSaveData saveData = linkedMarchingCubeController.linkedSaveData;

        VoxelData[,,] voxelDataReference = linkedMarchingCubeController.VoxelDataReference;

        saveData.SaveData(voxelDataReference);

#if UNITY_EDITOR
        EditorUtility.SetDirty(saveData);
        AssetDatabase.SaveAssets();
#endif
    }
}
