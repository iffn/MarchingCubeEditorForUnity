using UnityEngine;

[CreateAssetMenu(fileName = "SO SaveData", menuName = "Marching Cubes/ScriptableObjectSaveData")]
public class ScriptableObjectSaveData : ScriptableObject
{
    public int resolution;
    public float[] voxelValues; // Flattened 1D array to store voxel values

    // Initialize the grid data based on the resolution
    public void Initialize(int resolution)
    {
        this.resolution = resolution;
        voxelValues = new float[resolution * resolution * resolution];
    }

    // Get and set values with bounds checking
    public float GetValue(int x, int y, int z)
    {
        return voxelValues[GetIndex(x, y, z)];
    }

    public void SetValue(int x, int y, int z, float value)
    {
        voxelValues[GetIndex(x, y, z)] = value;
    }

    private int GetIndex(int x, int y, int z)
    {
        return x + resolution * (y + resolution * z);
    }
}
