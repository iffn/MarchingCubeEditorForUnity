using System;
using UnityEngine;

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
    public void SaveData(float[,,] voxelValues)
    {
        resolutionX = voxelValues.GetLength(0);
        resolutionY = voxelValues.GetLength(1);
        resolutionZ = voxelValues.GetLength(2);

        int totalValues = resolutionX * resolutionY * resolutionZ;
        byte[] byteArray = new byte[totalValues * 4]; // Each float is 4 bytes
        int byteIndex = 0;

        foreach (float value in voxelValues)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            Buffer.BlockCopy(bytes, 0, byteArray, byteIndex, 4);
            byteIndex += 4;
        }

        packedData = Convert.ToBase64String(byteArray); // Encode as Base64
    }

    /// <summary>
    /// Loads the voxel grid from the Base64-encoded string.
    /// </summary>
    /// <returns>3D array of voxel values.</returns>
    public float[,,] LoadData()
    {
        if (string.IsNullOrEmpty(packedData))
        {
            Debug.LogWarning("Packed data is empty. Returning default grid.");
            return new float[resolutionX, resolutionY, resolutionZ];
        }

        int totalValues = resolutionX * resolutionY * resolutionZ;
        byte[] byteArray = Convert.FromBase64String(packedData);
        float[,,] voxelValues = new float[resolutionX, resolutionY, resolutionZ];

        for (int i = 0, byteIndex = 0; i < totalValues; i++)
        {
            int x = i % resolutionX;
            int y = (i / resolutionX) % resolutionY;
            int z = i / (resolutionX * resolutionY);

            voxelValues[x, y, z] = BitConverter.ToSingle(byteArray, byteIndex);
            byteIndex += 4;
        }

        return voxelValues;
    }
}
