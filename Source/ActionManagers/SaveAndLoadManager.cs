#if UNITY_EDITOR
using iffnsStuff.MarchingCubeEditor.Core;
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
        VoxelData[,,] voxelData = saveData.LoadData();

        linkedController.SetAllGridDataAndUpdateMesh(voxelData);
    }
}
#endif