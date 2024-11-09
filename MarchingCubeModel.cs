using UnityEngine;

public class MarchingCubesModel
{
    private float[,,] voxelData;
    private int resolution;

    public MarchingCubesModel(int resolution)
    {
        this.resolution = resolution;
        voxelData = new float[resolution, resolution, resolution];
    }

    public int Resolution => resolution; // Expose resolution as a read-only property

    public void InitializeSphere(float sphereRadius)
    {
        Vector3 gridCenter = new(resolution / 2f, resolution / 2f, resolution / 2f);

        for (int x = 0; x < resolution; x++)
        {
            for (int y = 0; y < resolution; y++)
            {
                for (int z = 0; z < resolution; z++)
                {
                    Vector3 point = new Vector3(x, y, z);
                    float distanceToCenter = Vector3.Distance(point, gridCenter);
                    voxelData[x, y, z] = distanceToCenter - sphereRadius;

                }
            }
        }
    }

    public void InitializeCube()
    {
        for (int x = 0; x < resolution; x++)
        {
            for (int y = 0; y < resolution; y++)
            {
                for (int z = 0; z < resolution; z++)
                {
                    voxelData[x, y, z] = 1;
                    if (x < 2 || y < 2 || z < 2) voxelData[x, y, z] = -1;
                    if (x > resolution - 2 || y > resolution - 2 || z > resolution - 2) voxelData[x, y, z] = -1;

                }
            }
        }
    }

    public float[] GetCubeWeights(int x, int y, int z)
    {
        float[] cubeWeights = new float[8];

        // Order thanks to: https://github.com/Scrawk/Marching-Cubes/blob/4afa148b5d9ba74b31fe321a29c462ea28a3a248/Assets/MarchingCubes/Marching/Marching.cs#L158
        cubeWeights[0] = voxelData[x,     y,     z    ]; // {0, 0, 0}
        cubeWeights[1] = voxelData[x + 1, y,     z    ]; // {1, 0, 0}
        cubeWeights[2] = voxelData[x + 1, y + 1, z    ]; // {1, 1, 0}
        cubeWeights[3] = voxelData[x,     y + 1, z    ]; // {0, 1, 0}
        cubeWeights[4] = voxelData[x,     y,     z + 1]; // {0, 0, 1}
        cubeWeights[5] = voxelData[x + 1, y,     z + 1]; // {1, 0, 1}
        cubeWeights[6] = voxelData[x + 1, y + 1, z + 1]; // {1, 1, 1}
        cubeWeights[7] = voxelData[x,     y + 1, z + 1]; // {0, 1, 1}

        return cubeWeights;
    }
}
