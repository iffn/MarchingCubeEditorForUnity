#if UNITY_EDITOR
using UnityEngine;

namespace iffnsStuff.MarchingCubeEditor.Core
{
    public class MarchingCubesModel
    {
        public VoxelData[,,] VoxelData { get; private set; }

        public Vector3Int MaxGrid { get; private set; }
        public int ResolutionX => VoxelData.GetLength(0);
        public int ResolutionY => VoxelData.GetLength(1);
        public int ResolutionZ => VoxelData.GetLength(2);

        public MarchingCubesModel(int resolutionX, int resolutionY, int resolutionZ)
        {
            VoxelData = new VoxelData[resolutionX, resolutionY, resolutionZ];

            RecalculateMaxGrid();
        }

        void RecalculateMaxGrid()
        {
            MaxGrid = new Vector3Int(ResolutionX, ResolutionY, ResolutionZ) - Vector3Int.one;
        }

        bool IsInGrid(int x, int y, int z)
        {
            return x >= 0
                && x < ResolutionX
                && y >= 0
                && y < ResolutionY
                && z >= 0
                && z < ResolutionZ;
        }

        // Method to set a single voxel's value
        public void SetVoxel(int x, int y, int z, VoxelData value)
        {
            if(!IsInGrid(x, y, z)) return;

            VoxelData[x, y, z] = value;
        }

        /*public void AddVoxel(int x, int y, int z, float value)
        {
            if (!IsInGrid(x, y, z)) return;

            VoxelData[x, y, z] = Mathf.Max(VoxelData[x, y, z], value);
        }

        public void SubtractVoxel(int x, int y, int z, float value)
        {
            if (!IsInGrid(x, y, z)) return;

            VoxelData[x, y, z] = Mathf.Min(VoxelData[x, y, z], value);
        }*/

        public VoxelData GetVoxel(int x, int y, int z)
        {
            return VoxelData[x, y, z];
        }

        public VoxelData[,,] GetVoxelData()
        {
            return VoxelData;
        }

        public VoxelData[] GetCubeWeights(int x, int y, int z)
        {
            VoxelData[] cubeWeights = new VoxelData[8];

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

        public void SetDataAndResizeIfNeeded(VoxelData[,,] newData)
        {
            VoxelData = newData;

            RecalculateMaxGrid();
        }

        public void ChangeGridSize(int resolutionX, int resolutionY, int resolutionZ, int offsetX, int offsetY, int offsetZ)
        {
            // Create a new VoxelData array with the new size
            VoxelData[,,] newVoxelData = new VoxelData[resolutionX, resolutionY, resolutionZ];

            // Copying data over. Warning, max grid not calculated yet!
            // Determine the size of the overlapping region
            int overlapX = Mathf.Min(resolutionX, ResolutionX);
            int overlapY = Mathf.Min(resolutionY, ResolutionY);
            int overlapZ = Mathf.Min(resolutionZ, ResolutionZ);

            // Copy the overlapping region from the old VoxelData to the new one
            for (int x = offsetX; x < overlapX; x++)
            {
                for (int y = offsetY; y < overlapY; y++)
                {
                    for (int z = offsetZ; z < overlapZ; z++)
                    {
                        newVoxelData[x, y, z] = VoxelData[x, y, z];
                    }
                }
            }

            // Assign the new VoxelData array
            VoxelData = newVoxelData;

            RecalculateMaxGrid();
        }

        public void ChangeGridSizeIfNeeded(int resolutionX, int resolutionY, int resolutionZ, bool copyDataIfChanging)
        {
            // Check if the current size matches the new size
            if (resolutionX == ResolutionX && resolutionY == ResolutionY && resolutionZ == ResolutionZ)
            {
                // No changes needed
                return;
            }

            if (copyDataIfChanging)
            {
                ChangeGridSize(resolutionX, resolutionY, resolutionZ, 0, 0, 0);
            }
            else
            {
                VoxelData = new VoxelData[resolutionX, resolutionY, resolutionZ];

                RecalculateMaxGrid();
            }
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
#endif