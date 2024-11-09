using UnityEngine;

public class MarchingCubesModel
{
    private float[] voxelData;
    private int resolution;

    public MarchingCubesModel(int resolution)
    {
        this.resolution = resolution;
        voxelData = new float[resolution * resolution * resolution];
    }

    // Initialize voxel data with a shape, e.g., a sphere at the center
    public void InitializeData(float sphereRadius)
    {
        Vector3 gridCenter = new Vector3(resolution / 2f, resolution / 2f, resolution / 2f);

        for (int x = 0; x < resolution; x++)
        {
            for (int y = 0; y < resolution; y++)
            {
                for (int z = 0; z < resolution; z++)
                {
                    int index = x + resolution * (y + resolution * z);
                    Vector3 point = new Vector3(x, y, z);
                    float distanceToCenter = Vector3.Distance(point, gridCenter);
                    voxelData[index] = distanceToCenter < sphereRadius ? 1f : -1f;
                }
            }
        }
    }

    // Get the density values for a 2x2x2 cube at the specified location
    public float[] GetCubeWeights(int x, int y, int z)
    {
        float[] cube = new float[8];
        int index = 0;
        for (int dx = 0; dx <= 1; dx++)
        {
            for (int dy = 0; dy <= 1; dy++)
            {
                for (int dz = 0; dz <= 1; dz++)
                {
                    int ix = (x + dx) + resolution * ((y + dy) + resolution * (z + dz));
                    cube[index++] = voxelData[ix];
                }
            }
        }
        return cube;
    }
}
