#if UNITY_EDITOR
using System;
using UnityEngine;
using iffnsStuff.MarchingCubeEditor.Core;

[CreateAssetMenu(fileName = "SO SaveData", menuName = "Marching Cubes/ScriptableObjectSaveData")]
public class ScriptableObjectSaveData : ScriptableObject
{
    public int resolutionX;
    public int resolutionY;
    public int resolutionZ;
    [HideInInspector] public string packedData; // Base64-encoded grid data

    //ToDo: Most data points are likely 1 or -1. So optimizing for those cases might help a lot.

    /// <summary>
    /// Saves the entire 3D voxel grid as a Base64-encoded string.
    /// </summary>
    /// <param name="voxelValues">3D array of voxel values to save.</param>
    public void SaveData(VoxelData[,,] voxelValues)
    {
        resolutionX = voxelValues.GetLength(0);
        resolutionY = voxelValues.GetLength(1);
        resolutionZ = voxelValues.GetLength(2);

        int totalValues = resolutionX * resolutionY * resolutionZ;
        byte[] byteArray = new byte[totalValues * VoxelData.Size];
        int byteIndex = 0;

        foreach (VoxelData value in voxelValues)
        {
            value.Serialize(byteArray, byteIndex);
            byteIndex += VoxelData.Size;
        }

        packedData = Convert.ToBase64String(byteArray); // Encode as Base64
    }

    /// <summary>
    /// Loads the voxel grid from the Base64-encoded string.
    /// </summary>
    /// <returns>3D array of voxel values.</returns>
    public VoxelData[,,] LoadData()
    {
        if (string.IsNullOrEmpty(packedData))
        {
            Debug.LogWarning("Packed data is empty. Returning default grid.");
            return new VoxelData[resolutionX, resolutionY, resolutionZ];
        }

        byte[] byteArray = Convert.FromBase64String(packedData);
        VoxelData[,,] voxelValues = new VoxelData[resolutionX, resolutionY, resolutionZ];

        int byteIndex = 0;

        for (int x = 0; x < resolutionX; x++)
        {
            for (int y = 0; y < resolutionY; y++)
            {
                for (int z = 0; z < resolutionZ; z++)
                {
                    voxelValues[x, y, z].Deserialize(byteArray, byteIndex);
                    byteIndex += VoxelData.Size;
                }
            }
        }

        return voxelValues;
    }
}
#endif