//#define loadDataPerformanceOutput

#if UNITY_EDITOR
using iffnsStuff.MarchingCubeEditor.Core;
using UnityEngine;
using UnityEditor;

public class SaveAndLoadManager
{
    MarchingCubesController linkedController;

    public SaveAndLoadManager(MarchingCubesController linkedController)
    {
        this.linkedController = linkedController;
    }

    public void SaveGridData(ScriptableObjectSaveData saveData)
    {
        VoxelData[,,] voxelDataReference = linkedController.VoxelDataReference;

        saveData.SaveData(voxelDataReference);

#if UNITY_EDITOR
        EditorUtility.SetDirty(saveData);
        AssetDatabase.SaveAssets();
#endif
    }

    public void LoadGridData(ScriptableObjectSaveData saveData)
    {
#if loadDataPerformanceOutput
        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        sw.Start();
#endif

        VoxelData[,,] voxelData = saveData.LoadData();

#if loadDataPerformanceOutput
        Debug.Log($"Load voxel data: {sw.Elapsed.TotalMilliseconds}ms");
        sw.Restart();
#endif

        linkedController.SetAllGridDataAndUpdateMesh(voxelData);

#if loadDataPerformanceOutput
        Debug.Log($"Set voxel data: {sw.Elapsed.TotalMilliseconds}ms");
#endif
    }
}
#endif