using iffnsStuff.MarchingCubeEditor.Core;
using iffnsStuff.MarchingCubeEditor.EditTools;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public abstract class PlaymodeEditor : MonoBehaviour
{
    [SerializeField] protected EditShape placeableByClick;

    protected abstract MarchingCubesController LinkedMarchingCubeController { get; }

    protected virtual void InitializeController()
    {
        LinkedMarchingCubeController.ClearAllViews();
        LinkedMarchingCubeController.Initialize(1, 1, 1, true, false);
        LoadData();
    }

    protected void LoadData()
    {
        if (LinkedMarchingCubeController.linkedSaveData == null)
            return;

        LinkedMarchingCubeController.SaveAndLoadManager.LoadGridData(LinkedMarchingCubeController.linkedSaveData);
    }

    protected void SaveData()
    {
        ScriptableObjectSaveData saveData = LinkedMarchingCubeController.linkedSaveData;

        VoxelData[,,] voxelDataReference = LinkedMarchingCubeController.VoxelDataReference;

        saveData.SaveData(voxelDataReference);

#if UNITY_EDITOR
        EditorUtility.SetDirty(saveData);
        AssetDatabase.SaveAssets();
#endif
    }
}
