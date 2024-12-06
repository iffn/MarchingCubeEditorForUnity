using iffnsStuff.MarchingCubeEditor.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SaveAndLoadManager
{
    // ToDo: Fix mirror when loading
    MarchingCubesController linkedController;

    public SaveAndLoadManager(MarchingCubesController linkedController)
    {
        this.linkedController = linkedController;
    }

    public void SaveGridData(ScriptableObjectSaveData saveData)
    {
        float[,,] voxelDataReference = linkedController.VoxelDataReference;

        saveData.SaveData(voxelDataReference);

#if UNITY_EDITOR
        EditorUtility.SetDirty(saveData);
        AssetDatabase.SaveAssets();
#endif
    }

    public void LoadGridData(ScriptableObjectSaveData saveData)
    {
        float[,,] voxelData = saveData.LoadData();

        linkedController.SetAllGridDataAndUpdateMesh(voxelData);
    }
}
