//#define DEBUG_PERFORMANCE

using iffnsStuff.MarchingCubeEditor.EditTools;
using System;
using UnityEditor;
using UnityEngine;
using static BaseModificationTools;

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
                if (model == null) return false;
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

        public void ModifyShape(
            EditShape shape,
            IVoxelModifier modifier, // Handles specific voxel modification logic
            bool updateCollider)
        {
            System.Diagnostics.Stopwatch sw = new();
#if DEBUG_PERFORMANCE
    sw.Start();
#endif

            // Get grid properties
            Vector3 gridScale = transform.localScale;
            Vector3 inverseGridScale = new Vector3(1f / gridScale.x, 1f / gridScale.y, 1f / gridScale.z);
            Vector3Int gridResolution = new Vector3Int(model.ResolutionX, model.ResolutionY, model.ResolutionZ);

            // Get shape bounds in world space and transform to grid space
            (Vector3 worldMin, Vector3 worldMax) = shape.GetWorldBoundingBox();
            Vector3 gridMin = Vector3.Scale(worldMin - transform.position, inverseGridScale);
            Vector3 gridMax = Vector3.Scale(worldMax - transform.position, inverseGridScale);

            Vector3Int minGrid = Vector3Int.Max(Vector3Int.zero, Vector3Int.FloorToInt(gridMin));
            Vector3Int maxGrid = Vector3Int.Min(gridResolution, Vector3Int.CeilToInt(gridMax));

            // Loop through grid-space bounds
            for (int x = minGrid.x; x < maxGrid.x; x++)
            {
                for (int y = minGrid.y; y < maxGrid.y; y++)
                {
                    for (int z = minGrid.z; z < maxGrid.z; z++)
                    {
                        // Transform grid point to world space for distance calculation
                        Vector3 worldPoint = transform.TransformPoint(x, y, z);
                        float distance = shape.DistanceOutsideIsPositive(worldPoint);

                        // Retrieve the current voxel value
                        float currentValue = model.GetVoxel(x, y, z);

                        // Modify the voxel value using the modifier
                        float newValue = modifier.ModifyVoxel(x, y, z, currentValue, distance);

                        // Update the voxel in the model
                        model.SetVoxel(x, y, z, newValue);
                    }
                }
            }

#if DEBUG_PERFORMANCE
    Debug.Log($"Modify voxel time = {sw.Elapsed.TotalMilliseconds}ms");
    sw.Restart();
#endif

            // Update the mesh and collider if necessary
            GenerateAndDisplayMesh(updateCollider);

#if DEBUG_PERFORMANCE
    Debug.Log($"Generate mesh time = {sw.Elapsed.TotalMilliseconds}ms");
#endif
        }

        public void AddShape(EditShape shape, bool updateCollider)
        {
            ModifyShape(shape, new AddShapeModifier(), updateCollider);
        }

        public void AddShapeWithMaxHeight(EditShape shape, float maxHeight, bool updateCollider)
        {
            int maxHeightInt = Mathf.RoundToInt(Mathf.Min(maxHeight, model.ResolutionY));
            ModifyShape(shape, new AddShapeWithMaxHeightModifier(maxHeightInt), updateCollider);
        }

        public void SubtractShape(EditShape shape, bool updateCollider)
        {
            ModifyShape(shape, new SubtractShapeModifier(), updateCollider);
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

                model = new(saveData.GetLength(0), saveData.GetLength(1), saveData.GetLength(2))
                {
                    VoxelData = saveData
                }; // Initialize model with grid data

                GenerateAndDisplayMesh(updateColliders); // Refresh mesh
            }
        }

        private void OnDrawGizmos()
        {
            if (!showGridOutline) return;

            if (model == null) return;

            Gizmos.color = Color.cyan; // Set outline color

            // Calculate grid bounds and draw lines for the grid outline
            DrawGridOutline();
        }

        private void DrawGridOutline()
        {
            // Define the grid size and cell size
            Vector3 cellSize = transform.localScale;

            // Calculate the starting position of the grid (bottom-left-front corner)
            Vector3 gridOrigin = transform.position;

            Vector3 outlineSize = new(model.ResolutionX - 1, model.ResolutionY - 1, model.ResolutionZ - 1);
            outlineSize = UnityUtilityFunctions.ComponentwiseMultiply(outlineSize, cellSize);

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