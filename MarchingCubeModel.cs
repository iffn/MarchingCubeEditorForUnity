using UnityEngine;

namespace iffnsStuff.MarchingCubeEditor.Core
{
    public class MarchingCubesModel
    {
        private float[,,] voxelData;
        private int resolution;

        public MarchingCubesModel(int resolution)
        {
            this.resolution = resolution;
            voxelData = new float[resolution, resolution, resolution];
        }

        public int Resolution => resolution;

        // Method to set a single voxel’s value
        public void SetVoxel(int x, int y, int z, float value)
        {
            if (x >= 0 && x < resolution && y >= 0 && y < resolution && z >= 0 && z < resolution)
            {
                voxelData[x, y, z] = value;
            }
        }

        public void AddVoxel(int x, int y, int z, float value)
        {
            if (x >= 0 && x < resolution && y >= 0 && y < resolution && z >= 0 && z < resolution)
            {
                voxelData[x, y, z] = Mathf.Max(voxelData[x, y, z], value);
            }
        }

        public void SubtractVoxel(int x, int y, int z, float value)
        {
            if (x >= 0 && x < resolution && y >= 0 && y < resolution && z >= 0 && z < resolution)
            {
                voxelData[x, y, z] = Mathf.Min(voxelData[x, y, z], value);
            }
        }

        public float[] GetCubeWeights(int x, int y, int z)
        {
            float[] cubeWeights = new float[8];

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
}