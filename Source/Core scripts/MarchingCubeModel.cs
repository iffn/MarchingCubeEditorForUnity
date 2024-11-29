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

        // Method to set a single voxel�s value
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

        public void ChangeGridSizeIfNeeded(int resolutionX, int resolutionY, int resolutionZ, bool copyDataIfChanging)
        {
            // Check if the current size matches the new size
            if (resolutionX == ResolutionX && resolutionY == ResolutionY && resolutionZ == ResolutionZ)
            {
                // No changes needed
                return;
            }

            // Create a new VoxelData array with the new size
            float[,,] newVoxelData = new float[resolutionX, resolutionY, resolutionZ];

            if (copyDataIfChanging)
            {
                // Determine the size of the overlapping region
                int overlapX = Mathf.Min(resolutionX, ResolutionX);
                int overlapY = Mathf.Min(resolutionY, ResolutionY);
                int overlapZ = Mathf.Min(resolutionZ, ResolutionZ);

                // Copy the overlapping region from the old VoxelData to the new one
                for (int x = 0; x < overlapX; x++)
                {
                    for (int y = 0; y < overlapY; y++)
                    {
                        for (int z = 0; z < overlapZ; z++)
                        {
                            newVoxelData[x, y, z] = VoxelData[x, y, z];
                        }
                    }
                }
            }

            // Assign the new VoxelData array
            VoxelData = newVoxelData;
        }

        public void CopyRegion(MarchingCubesModel source, Vector3Int minGrid, Vector3Int maxGrid)
        {
            // Copy the voxel data from source to this model
            for (int x = minGrid.x; x <= maxGrid.x; x++)
            {
                for (int y = minGrid.y; y <= maxGrid.y; y++)
                {
                    for (int z = minGrid.z; z <= maxGrid.z; z++)
                    {
                        VoxelData[x, y, z] = source.GetVoxel(x, y, z);
                    }
                }
            }
        }
    }
}