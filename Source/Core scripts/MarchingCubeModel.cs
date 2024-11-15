using UnityEngine;

namespace iffnsStuff.MarchingCubeEditor.Core
{
    public class MarchingCubesModel
    {
        public float[,,] VoxelData;

        public MarchingCubesModel(int xResolution, int yResolution, int zResolution)
        {
            VoxelData = new float[xResolution, yResolution, zResolution];
        }

        public int ResolutionX => VoxelData.GetLength(0);
        public int ResolutionY => VoxelData.GetLength(1);
        public int ResolutionZ => VoxelData.GetLength(2);

        // Method to set a single voxel’s value
        public void SetVoxel(int x, int y, int z, float value)
        {
            if(!IsInGrid(x, y, z)) return;

            VoxelData[x, y, z] = value;
        }

        public void AddVoxel(int x, int y, int z, float value)
        {
            if (!IsInGrid(x, y, z)) return;

            VoxelData[x, y, z] = Mathf.Max(VoxelData[x, y, z], value);
        }

        public void SubtractVoxel(int x, int y, int z, float value)
        {
            if (!IsInGrid(x, y, z)) return;

            VoxelData[x, y, z] = Mathf.Min(VoxelData[x, y, z], value);
        }

        bool IsInGrid(int x, int y, int z)
        {
            return x >= 0
                && x < VoxelData.GetLength(0) 
                && y >= 0 
                && y < VoxelData.GetLength(1) 
                && z >= 0 
                && z < VoxelData.GetLength(2);
        }

        public float GetVoxel(int x, int y, int z)
        {
            return VoxelData[x, y, z];
        }

        public float[,,] GetVoxelData()
        {
            return VoxelData;
        }

        public float[] GetCubeWeights(int x, int y, int z)
        {
            float[] cubeWeights = new float[8];

            cubeWeights[0] = VoxelData[x,     y,     z    ]; // {0, 0, 0}
            cubeWeights[1] = VoxelData[x + 1, y,     z    ]; // {1, 0, 0}
            cubeWeights[2] = VoxelData[x + 1, y + 1, z    ]; // {1, 1, 0}
            cubeWeights[3] = VoxelData[x,     y + 1, z    ]; // {0, 1, 0}
            cubeWeights[4] = VoxelData[x,     y,     z + 1]; // {0, 0, 1}
            cubeWeights[5] = VoxelData[x + 1, y,     z + 1]; // {1, 0, 1}
            cubeWeights[6] = VoxelData[x + 1, y + 1, z + 1]; // {1, 1, 1}
            cubeWeights[7] = VoxelData[x,     y + 1, z + 1]; // {0, 1, 1}

            return cubeWeights;
        }
    }
}