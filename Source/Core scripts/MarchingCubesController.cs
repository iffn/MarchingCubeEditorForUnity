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
        public bool invertedNormals = false;

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
                        MarchingCubes.GenerateCubeMesh(meshData, cubeWeights, x, y, z, invertedNormals);
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
            System.Diagnostics.Stopwatch sw = new();

            sw.Start();

            Vector3Int gridResolution = new Vector3Int(model.ResolutionX, model.ResolutionY, model.ResolutionZ);

            (Vector3Int minGrid, Vector3Int maxGrid) = shape.GetBounds(gridResolution);

            for (int x = minGrid.x; x < maxGrid.x; x++)
            {
                for (int y = minGrid.y; y < maxGrid.y; y++)
                {
                    for (int z = minGrid.z; z < maxGrid.z; z++)
                    {
                        Vector3 point = new Vector3(x, y, z);
                        float distanceOutsideIsPositive = shape.DistanceOutsideIsPositive(point);

                        model.AddVoxel(x, y, z, -distanceOutsideIsPositive);
                    }
                }
            }

            Debug.Log($"Add voxel time = {sw.Elapsed.TotalSeconds * 1000}ms");
            sw.Restart();

            GenerateAndDisplayMesh(updateCollider);

            Debug.Log($"Generate mesh time = {sw.Elapsed.TotalSeconds * 1000}ms");
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
            float cellSize = 1;

            // Calculate the starting position of the grid (bottom-left-front corner)
            Vector3 gridOrigin = transform.position;

            Vector3 outlineSize = cellSize * new Vector3(model.ResolutionX - 1, model.ResolutionY - 1, model.ResolutionZ - 1);

            // Calculate all eight corners of the grid box
            Vector3[] corners = new Vector3[8];
            corners[0] = gridOrigin;
            corners[1] = gridOrigin + new Vector3(outlineSize.x, 0, 0);
            corners[2] = gridOrigin + new Vector3(outlineSize.x, outlineSize.y, 0);
            corners[3] = gridOrigin + new Vector3(0, outlineSize.y, 0);
            corners[4] = gridOrigin + new Vector3(0, 0, outlineSize.z);
            corners[5] = gridOrigin + new Vector3(outlineSize.x, 0, outlineSize.z);
            corners[6] = gridOrigin + new Vector3(outlineSize.x, outlineSize.y, outlineSize.z);
            corners[7] = gridOrigin + new Vector3(0, outlineSize.y, outlineSize.z);

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