#if UNITY_EDITOR
using UnityEngine;
using System;

namespace iffnsStuff.MarchingCubeEditor.Core
{
    public class MarchingCubesModel
    {
        public VoxelData[,,] VoxelDataGrid { get; private set; }

        public Vector3Int MaxGrid { get; private set; }
        public int ResolutionX => VoxelDataGrid.GetLength(0);
        public int ResolutionY => VoxelDataGrid.GetLength(1);
        public int ResolutionZ => VoxelDataGrid.GetLength(2);

        public MarchingCubesModel(int resolutionX, int resolutionY, int resolutionZ)
        {
            VoxelDataGrid = new VoxelData[resolutionX, resolutionY, resolutionZ];

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

            VoxelDataGrid[x, y, z] = value;
        }

        public VoxelData GetVoxelWithoutClamp(int x, int y, int z)
        {
            return VoxelDataGrid[x, y, z];
        }

        public VoxelData GetVoxelWithClamp(int x, int y, int z)
        {
            x = Math.Clamp(x, 0, MaxGrid.x);
            y = Math.Clamp(y, 0, MaxGrid.y);
            z = Math.Clamp(z, 0, MaxGrid.z);

            return GetVoxelWithoutClamp(x, y, z);
        }

        public VoxelData GetVoxelWithClamp(float x, float y, float z)
        {
            int xi = Mathf.RoundToInt(x);
            int yi = Mathf.RoundToInt(y);
            int zi = Mathf.RoundToInt(z);

            return GetVoxelWithClamp(xi, yi, zi); // A bit dangerous, since the a loop is called if int is paresed to a float
        }

        public VoxelData[,,] GetVoxelData()
        {
            return VoxelDataGrid;
        }

        public void GetCubeWeights(int x, int y, int z, Span<VoxelData> outWeights)
        {
            outWeights[0] = VoxelDataGrid[x,     y,     z    ]; // {0, 0, 0}
            outWeights[1] = VoxelDataGrid[x + 1, y,     z    ]; // {1, 0, 0}
            outWeights[2] = VoxelDataGrid[x + 1, y + 1, z    ]; // {1, 1, 0}
            outWeights[3] = VoxelDataGrid[x,     y + 1, z    ]; // {0, 1, 0}
            outWeights[4] = VoxelDataGrid[x,     y,     z + 1]; // {0, 0, 1}
            outWeights[5] = VoxelDataGrid[x + 1, y,     z + 1]; // {1, 0, 1}
            outWeights[6] = VoxelDataGrid[x + 1, y + 1, z + 1]; // {1, 1, 1}
            outWeights[7] = VoxelDataGrid[x,     y + 1, z + 1]; // {0, 1, 1}
        }

        public void SetDataAndResizeIfNeeded(VoxelData[,,] newData)
        {
            VoxelDataGrid = newData;

            RecalculateMaxGrid();
        }

        public void ChangeGridSize(int resolutionX, int resolutionY, int resolutionZ, int offsetX, int offsetY, int offsetZ)
        {
            // Create a new VoxelData array with the new size
            VoxelData[,,] newVoxelData = new VoxelData[resolutionX, resolutionY, resolutionZ];

            for(int x = 0; x < resolutionX; x++)
            {
                for (int y = 0; y < resolutionY; y++)
                {
                    for (int z = 0; z < resolutionZ; z++)
                    {
                        newVoxelData[x, y, z] = VoxelData.Empty;
                    }
                }
            }

            // Copying data over. Warning, max grid not calculated yet!
            // Determine the size of the overlapping region
            int minX = Mathf.Max(0, -offsetX);
            int minY = Mathf.Max(0, -offsetY);
            int minZ = Mathf.Max(0, -offsetZ);

            int maxX = Mathf.Min(ResolutionX, resolutionX - offsetX);
            int maxY = Mathf.Min(ResolutionY, resolutionY - offsetY);
            int maxZ = Mathf.Min(ResolutionZ, resolutionZ - offsetZ);

            // Copy the overlapping region from the old VoxelData to the new one
            for (int x = minX; x < maxX; x++)
            {
                for (int y = minY; y < maxY; y++)
                {
                    for (int z = minZ; z < maxZ; z++)
                    {
                        VoxelData data = VoxelDataGrid[x, y, z];

                        newVoxelData[x + offsetX, y + offsetY, z + offsetZ] = data;
                    }
                }
            }

            // Assign the new VoxelData array
            VoxelDataGrid = newVoxelData;

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
                VoxelDataGrid = new VoxelData[resolutionX, resolutionY, resolutionZ];

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
                        VoxelDataGrid[x, y, z] = source.GetVoxelWithoutClamp(x, y, z);
                    }
                }
            }
        }
    }
}
#endif