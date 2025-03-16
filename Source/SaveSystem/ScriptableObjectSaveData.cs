#if UNITY_EDITOR
using System;
using UnityEngine;
using iffnsStuff.MarchingCubeEditor.Core;
using System.Collections.Generic;
using System.Collections;

[CreateAssetMenu(fileName = "SO SaveData", menuName = "Marching Cubes/ScriptableObjectSaveData")]
public class ScriptableObjectSaveData : ScriptableObject
{
    public int resolutionX;
    public int resolutionY;
    public int resolutionZ;
    public int version = 0;
    [HideInInspector] public string packedData; // Base64-encoded grid data

    public static int currentVersion = 1;

    //ToDo: Most data points are likely 1 or -1. So optimizing for those cases might help a lot.

    /// <summary>
    /// Saves the entire 3D voxel grid as a Base64-encoded string.
    /// </summary>
    /// <param name="voxelValues">3D array of voxel values to save.</param>
    public void SaveData(VoxelData[,,] voxelValues)
    {
        version = currentVersion;

        resolutionX = voxelValues.GetLength(0);
        resolutionY = voxelValues.GetLength(1);
        resolutionZ = voxelValues.GetLength(2);

        List<byte> compressedData = new List<byte>();

        int totalValues = resolutionX * resolutionY * resolutionZ;

        VoxelData prevVoxel = voxelValues[0, 0, 0];
        int runLength = 1;

        for (int i = 1; i < totalValues; i++)
        {
            int x = i % resolutionX;
            int y = (i / resolutionX) % resolutionY;
            int z = i / (resolutionX * resolutionY);
            VoxelData currentVoxel = voxelValues[x, y, z];

            if (currentVoxel.Equals(prevVoxel) && runLength < 255)
            {
                runLength++;
            }
            else
            {
                // Store (value + run-length)
                compressedData.AddRange(prevVoxel.SerializeCompressed());
                compressedData.Add((byte)runLength);

                prevVoxel = currentVoxel;
                runLength = 1;
            }
        }

        // Store last run
        compressedData.AddRange(prevVoxel.SerializeCompressed());
        compressedData.Add((byte)runLength);

        // Convert to Base64 string
        packedData = Convert.ToBase64String(compressedData.ToArray());
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

        // Declare data:
        int index = 0;
        int totalVoxels = resolutionX * resolutionY * resolutionZ;
        int voxelIndex = 0;
        VoxelData[,,] voxelValues = new VoxelData[resolutionX, resolutionY, resolutionZ];
        int expectedSize;

        // Get the data
        byte[] compressedData = Convert.FromBase64String(packedData);

        // Upgrade the data if needed
        switch (version)
        {
            case 0:
                // Sanity check
                expectedSize = resolutionX * resolutionY * resolutionZ * (4 + 32);
                if (compressedData.Length != expectedSize)
                {
                    Debug.LogWarning($"V0 Voxel data size mismatch! Expected {expectedSize} bytes, but got {compressedData.Length} bytes.");
                    return voxelValues;
                }

                compressedData = ConvertV0ToV1(compressedData);
                break;
            case 1:
                //Current version
                break;
            default:
                break;
        }

        // Decode the data:
        while (voxelIndex < totalVoxels)
        {
            // Decode weight (1 byte)
            float decodedWeight = (compressedData[index] / 127.5f) - 1f;
            index++;

            // Decode color (4 bytes)
            Color32 decodedColor = new Color32(compressedData[index], compressedData[index + 1], compressedData[index + 2], compressedData[index + 3]);
            index += 4;

            // Decode run-length (1 byte)
            int runLength = compressedData[index];
            index++;

            // Apply the decoded voxel to multiple locations
            for (int i = 0; i < runLength; i++)
            {
                int x = voxelIndex % resolutionX;
                int y = (voxelIndex / resolutionX) % resolutionY;
                int z = voxelIndex / (resolutionX * resolutionY);

                voxelValues[x, y, z] = new VoxelData(decodedWeight, decodedColor);
                voxelIndex++;
            }
        }

        return voxelValues;
    }

    void LoadDataV0()
    {
        (float WeightInsideIsPositive, Color color) DeserializeVoxel(byte[] src, int srcOffset)
        {
            float WeightInsideIsPositive = BitConverter.ToSingle(src, srcOffset);
            Color color = new Color32(src[srcOffset + 4], src[srcOffset + 5], src[srcOffset + 6], src[srcOffset + 7]);

            return (WeightInsideIsPositive, color);
        }
    }

    private static byte[] ConvertV0ToV1(byte[] v0Data)
    {
        int voxelCount = v0Data.Length / (4 + 32); // Old voxel size = 36 bytes (4 weight + 32 color)
        List<byte> v1Data = new List<byte>();

        int readIndex = 0;

        for (int i = 0; i < voxelCount; i++)
        {
            VoxelData voxel = ReadV0Voxel(v0Data, ref readIndex);
            v1Data.AddRange(voxel.SerializeCompressed()); // Store in V1 format
        }

        return v1Data.ToArray();

        VoxelData ReadV0Voxel(byte[] v0Data, ref int index)
        {
            // Read weight (4 bytes)
            float weight = BitConverter.ToSingle(v0Data, index);
            index += 4;

            // Read color (RGBA, 4 bytes)
            Color32 color = new Color32(v0Data[index], v0Data[index + 1], v0Data[index + 2], v0Data[index + 3]);
            index += 4;

            // Skip the unused 28 bytes from V0
            index += 28;

            return new VoxelData(weight, color);
        }
    }
}
#endif