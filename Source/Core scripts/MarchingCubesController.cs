using iffnsStuff.MarchingCubeEditor.EditTools;
using UnityEditor;
using UnityEngine;

namespace iffnsStuff.MarchingCubeEditor.Core
{
    [RequireComponent(typeof(MarchingCubesView))]
    public class MarchingCubesController : MonoBehaviour
    {
        MarchingCubesModel model;
        MarchingCubesMeshData meshData;
        MarchingCubesView view;

        public bool showGridOutline = false; // Toggle controlled by the editor tool

        public int GridResolutionX => model.VoxelData.GetLength(0);
        public int GridResolutionY => model.VoxelData.GetLength(1);
        public int GridResolutionZ => model.VoxelData.GetLength(2);

        public void Initialize(int resolutionX, int resolutionY, int resolutionZ, bool setEmpty)
        {
            view = GetComponent<MarchingCubesView>();
            model = new MarchingCubesModel(resolutionX, resolutionY, resolutionZ);

            view.Initialize();

            if (setEmpty) SetEmptyGrid();
        }

        public bool IsInitialized
        {
            get
            {
                if(model == null) return false;
                return true;
            }
        }

        public void GenerateAndDisplayMesh(bool updateCollider)
        {
            meshData = new MarchingCubesMeshData();

            for (int x = 0; x < model.ResolutionX - 1; x++)
            {
                for (int y = 0; y < model.ResolutionY - 1; y++)
                {
                    for (int z = 0; z < model.ResolutionZ - 1; z++)
                    {
                        float[] cubeWeights = model.GetCubeWeights(x, y, z);
                        MarchingCubes.GenerateCubeMesh(meshData, cubeWeights, x, y, z);
                    }
                }
            }

            view.UpdateMesh(meshData, updateCollider);
        }

        public void SetEmptyGrid()
        {
            for (int x = 0; x < model.ResolutionX; x++)
            {
                for (int y = 0; y < model.ResolutionY; y++)
                {
                    for (int z = 0; z < model.ResolutionZ; z++)
                    {
                        model.SetVoxel(x, y, z, -1); // Use signed distance
                    }
                }
            }
            GenerateAndDisplayMesh(false); // Update the mesh after modification
        }

        public void AddShape(EditShape shape, bool updateCollider)
        {
            for (int x = 0; x < model.ResolutionX; x++)
            {
                for (int y = 0; y < model.ResolutionY; y++)
                {
                    for (int z = 0; z < model.ResolutionZ; z++)
                    {
                        Vector3 point = new(x, y, z);
                        float distanceOutsideIsPositive = shape.DistanceOutsideIsPositive(point);

                        model.AddVoxel(x, y, z, -distanceOutsideIsPositive);
                    }
                }
            }

            GenerateAndDisplayMesh(updateCollider);
        }

        public void AddShapeWithMaxHeight(EditShape shape, float maxHeight, bool updateCollider)
        {
            int maxHeightInt = Mathf.RoundToInt(Mathf.Min(maxHeight, model.ResolutionY));

            for (int x = 0; x < model.ResolutionX; x++)
            {
                for (int y = 0; y < maxHeightInt; y++)
                {
                    for (int z = 0; z < model.ResolutionZ; z++)
                    {
                        Vector3 point = new(x, y, z);
                        float distanceOutsideIsPositive = shape.DistanceOutsideIsPositive(point);

                        model.AddVoxel(x, y, z, -distanceOutsideIsPositive);
                    }
                }
            }

            for (int x = 0; x < model.ResolutionX; x++)
            {
                for (int y = maxHeightInt + 1; y < model.ResolutionY; y++)
                {
                    for (int z = 0; z < model.ResolutionZ; z++)
                    {
                        Vector3 point = new(x, y, z);
                        float distanceOutsideIsPositive = shape.DistanceOutsideIsPositive(point);

                        model.SubtractVoxel(x, y, z, distanceOutsideIsPositive);
                    }
                }
            }

            GenerateAndDisplayMesh(updateCollider);
        }

        public void SubtractShape(EditShape shape, bool updateCollider)
        {
            for (int x = 0; x < model.ResolutionX; x++)
            {
                for (int y = 0; y < model.ResolutionY; y++)
                {
                    for (int z = 0; z < model.ResolutionZ; z++)
                    {
                        Vector3 point = new(x, y, z);
                        float distanceOutsideIsPositive = shape.DistanceOutsideIsPositive(point);

                        model.SubtractVoxel(x, y, z, distanceOutsideIsPositive);
                    }
                }
            }

            GenerateAndDisplayMesh(updateCollider);
        }

        public void SaveGridData(ScriptableObjectSaveData gridData)
        {
            if (gridData != null)
            {
                gridData.SaveData(model.GetVoxelData());
            }

#if UNITY_EDITOR
            EditorUtility.SetDirty(gridData);
            AssetDatabase.SaveAssets();
#endif
        }

        public void LoadGridData(ScriptableObjectSaveData gridData, bool updateColliders)
        {
            if (gridData != null)
            {
                float[,,] saveData = gridData.LoadData();

                model = new MarchingCubesModel(saveData.GetLength(0), saveData.GetLength(1), saveData.GetLength(2)); // Initialize model with grid data

                model.VoxelData = saveData;

                GenerateAndDisplayMesh(updateColliders); // Refresh mesh
            }
        }

        private void OnDrawGizmos()
        {
            if (!showGridOutline) return;

            if(model == null) return;

            Gizmos.color = Color.cyan; // Set outline color

            // Calculate grid bounds and draw lines for the grid outline
            DrawGridOutline();
        }

        private void DrawGridOutline()
        {
            // Define the grid size and cell size
            Vector3 cellSize = Vector3.one;  // Adjust if each voxel cell has different dimensions

            // Calculate the offset for half the grid size along each axis. Since grid is zero-indexed, subtract 1 to get the bounds
            Vector3 halfGridSize = new Vector3((model.ResolutionX - 1) * cellSize.x * 0.5f,
                                               (model.ResolutionY - 1) * cellSize.y * 0.5f,
                                               (model.ResolutionZ - 1) * cellSize.z * 0.5f);


            // Calculate the starting position of the grid (bottom-left-front corner)
            Vector3 gridOrigin = transform.position;

            // Calculate all eight corners of the grid box
            Vector3[] corners = new Vector3[8];
            corners[0] = gridOrigin;
            corners[1] = gridOrigin + new Vector3(model.ResolutionX * cellSize.x, 0, 0);
            corners[2] = gridOrigin + new Vector3(model.ResolutionX * cellSize.x, model.ResolutionY * cellSize.y, 0);
            corners[3] = gridOrigin + new Vector3(0, model.ResolutionY * cellSize.y, 0);
            corners[4] = gridOrigin + new Vector3(0, 0, model.ResolutionZ * cellSize.z);
            corners[5] = gridOrigin + new Vector3(model.ResolutionX * cellSize.x, 0, model.ResolutionZ * cellSize.z);
            corners[6] = gridOrigin + new Vector3(model.ResolutionX * cellSize.x, model.ResolutionY * cellSize.y, model.ResolutionZ * cellSize.z);
            corners[7] = gridOrigin + new Vector3(0, model.ResolutionY * cellSize.y, model.ResolutionZ * cellSize.z);

            // Draw edges of the grid box
            Gizmos.DrawLine(corners[0], corners[1]); // Bottom front edge
            Gizmos.DrawLine(corners[1], corners[2]); // Bottom right edge
            Gizmos.DrawLine(corners[2], corners[3]); // Bottom back edge
            Gizmos.DrawLine(corners[3], corners[0]); // Bottom left edge

            Gizmos.DrawLine(corners[4], corners[5]); // Top front edge
            Gizmos.DrawLine(corners[5], corners[6]); // Top right edge
            Gizmos.DrawLine(corners[6], corners[7]); // Top back edge
            Gizmos.DrawLine(corners[7], corners[4]); // Top left edge

            Gizmos.DrawLine(corners[0], corners[4]); // Front left vertical edge
            Gizmos.DrawLine(corners[1], corners[5]); // Front right vertical edge
            Gizmos.DrawLine(corners[2], corners[6]); // Back right vertical edge
            Gizmos.DrawLine(corners[3], corners[7]); // Back left vertical edge
        }
    }
}