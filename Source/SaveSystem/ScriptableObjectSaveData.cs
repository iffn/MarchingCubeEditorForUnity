#pragma warning disable IDE0090 // Use 'new(...)'
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

    public static int currentVersion = 2;

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

        int totalVoxels = resolutionX * resolutionY * resolutionZ;

        float[] weightInsideIsPositive = new float[totalVoxels];
        Color32[] colors = new Color32[totalVoxels];

        int counter = 0;

        for (int x = 0; x < resolutionX; x++)
        {
            for(int y = 0; y < resolutionY; y++)
            {
                for(int z = 0; z < resolutionZ; z++)
                {
                    weightInsideIsPositive[counter] = voxelValues[x, y, z].WeightInsideIsPositive;
                    colors[counter] = voxelValues[x, y, z].Color;
                    counter++;
                }
            }
        }

        byte[] serializedData = SerializeDataV2(weightInsideIsPositive, colors);


        // Convert to Base64 string
        packedData = Convert.ToBase64String(serializedData);
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
        int totalVoxels = resolutionX * resolutionY * resolutionZ;
        VoxelData[,,] voxelValues = new VoxelData[resolutionX, resolutionY, resolutionZ];

        // Get the data
        byte[] byteData = Convert.FromBase64String(packedData);

        int currentDataVersion = version;

        // Check version and upgrade if necessary.
        // Note: Implementing the upgrade to the next version means that they will still work with newer versions. Otherwise, V5 would need to care about V0 instead of doing V0 -> V2 -> V3...
        if (currentDataVersion == 0)
        {
            // Convert from V0 to V2
            byteData = ConvertV0ToV2(byteData, totalVoxels);
            currentDataVersion = 2;
        }

        if (currentDataVersion == 1)
        {
            Debug.LogWarning("V1 was an intermediate version and cannot be converted at this time. Loading failed.");
            return voxelValues;
        }

        // Current version = actual implementation
        if (currentDataVersion == currentVersion)
        {
            // Assign data
            (float[] weightInsideIsPositive, Color32[] colors) = DeserializeDataV2(byteData, totalVoxels);

            int counter = 0;
            for (int x = 0; x < resolutionX; x++)
            {
                for (int y = 0; y < resolutionY; y++)
                {
                    for (int z = 0; z < resolutionZ; z++)
                    {
                        voxelValues[x, y, z] = new VoxelData(weightInsideIsPositive[counter], colors[counter]);
                        counter++;
                    }
                }
            }
        }

        return voxelValues;
    }

    static byte[] SerializeDataV2(float[] weightInsideIsPositive, Color32[] colors)
    {
        List<byte> returnValue = new List<byte>();

        // Run-Length Encoding for weights
        short prevValue = ConvertCenterFloatToShort(weightInsideIsPositive[0]);

        returnValue.Add(0); // Start run-length at 0. It can never have a length of 0, so 0 means 1

        byte[] bytes = ConvertShortToByteArray(prevValue);
        foreach (byte b in bytes)
        {
            returnValue.Add(b);
        }

        for(int i = 1; i<weightInsideIsPositive.Length; i++)
        {
            float weight = weightInsideIsPositive[i];

            short scaledValue = ConvertCenterFloatToShort(weight); // Scale float to short

            if (scaledValue == prevValue && returnValue[returnValue.Count - 3] < 254)
            {
                // returnValue[^3]++; // ToDo: Test if this compiles with the old Unity version
                returnValue[returnValue.Count - 3]++;
                
            }
            else
            {
                returnValue.Add(0); // Start run-length at 0. It can never have a length of 0, so 0 means 1
                bytes = ConvertShortToByteArray(scaledValue);
                foreach (byte b in bytes)
                {
                    returnValue.Add(b);
                }

                prevValue = scaledValue;
            }
        }

        // Run-Length Encoding for Colors
        Color32 prevColor = colors[0];
        returnValue.Add(0); // Start run-length at 0. It can never have a length of 0, so 0 means 1

        returnValue.Add(prevColor.r);
        returnValue.Add(prevColor.g);
        returnValue.Add(prevColor.b);
        returnValue.Add(prevColor.a);

        for (int i = 1; i < colors.Length; i++)
        {
            Color32 currentColor = colors[i];

            if (currentColor.r == prevColor.r &&
                currentColor.g == prevColor.g &&
                currentColor.b == prevColor.b &&
                currentColor.a == prevColor.a &&
                returnValue[returnValue.Count - 5] < 254)
            {
                returnValue[returnValue.Count - 5]++; // Increment run-length count for colors
            }
            else
            {
                returnValue.Add(0); // Start run-length at 0. It can never have a length of 0, so 0 means 1

                returnValue.Add(currentColor.r);
                returnValue.Add(currentColor.g);
                returnValue.Add(currentColor.b);
                returnValue.Add(currentColor.a);

                prevColor = currentColor; // Update previous color
            }
        }

        return returnValue.ToArray();

        short ConvertCenterFloatToShort(float value)
        {
            return (short)(value * 32767);
        }

        byte[] ConvertShortToByteArray(short value)
        {
            return BitConverter.GetBytes(value);
        }
    }

    (float[] weightInsideIsPositive, Color32[] colors) DeserializeDataV2(byte[] data, int totalVoxels)
    {
        List<float> weightList = new List<float>();
        List<Color32> colorList = new List<Color32>();

        int index = 0;

        // ---- Decode Weights ----
        while (weightList.Count < totalVoxels) // Use provided length
        {
            int runLength = data[index++] + 1; // Add one to the run length. It can never have a length of 0, so 0 means 1.

            short weightShort = BitConverter.ToInt16(data, index);

            index += 2;

            float weight = ConvertShortToFloat(weightShort);

            for (int i = 0; i < runLength; i++) // Expand run-length
            {
                weightList.Add(weight);
            }
        }

        if (weightList.Count != totalVoxels)
        {
            Debug.LogWarning($"Mismatch! Weights: {weightList.Count}, Expected: {totalVoxels}");
        }

        // ---- Decode Colors ----
        while (colorList.Count < totalVoxels)
        {
            int runLength = data[index++] + 1; // Add one to the run length. It can never have a length of 0, so 0 means 1.

            Color32 color = new Color32(data[index], data[index + 1], data[index + 2], data[index + 3]);

            index += 4;

            for (int i = 0; i < runLength; i++) // Expand run-length
            {
                colorList.Add(color);
            }
        }

        return (weightList.ToArray(), colorList.ToArray());

        static float ConvertShortToFloat(short value)
        {
            return value / 32767f;
        }
    }

    private static byte[] ConvertV0ToV2(byte[] v0Data, int totalVoxels)
    {
        (float[] weightInsideIsPositive, Color32[] colors) = DeserializeDataV0(v0Data, totalVoxels);

        return SerializeDataV2(weightInsideIsPositive, colors);

        static (float[] weightInsideIsPositive, Color32[] colors) DeserializeDataV0(byte[] data, int totalVoxels)
        {
            float[] weightInsideIsPositive = new float[totalVoxels];
            Color32[] colors = new Color32[totalVoxels];

            int byteIndex = 0;

            for (int i = 0; i < totalVoxels; i++)
            {
                weightInsideIsPositive[i] = BitConverter.ToSingle(data, byteIndex);
                colors[i] = new Color32(data[byteIndex + 4], data[byteIndex + 5], data[byteIndex + 6], data[byteIndex + 7]);

                byteIndex += 36; // Skip 4+32 bytes per voxel
            }

            return(weightInsideIsPositive, colors);
        }
    }
}
#endif