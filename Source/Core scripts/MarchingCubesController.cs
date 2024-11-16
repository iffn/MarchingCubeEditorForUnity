//#define DEBUG_PERFORMANCE

using iffnsStuff.MarchingCubeEditor.EditTools;
using System;
using System.Threading;
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

        public void ModifyShape(EditShape shape, IVoxelModifier modifier, bool updateCollider)
        {
            System.Diagnostics.Stopwatch sw = new();
#if DEBUG_PERFORMANCE
            sw.Start();
#endif

            // Precompute transformation matrices
            Matrix4x4 gridToWorld = transform.localToWorldMatrix; // Transform grid space to world space
            Matrix4x4 worldToGrid = transform.worldToLocalMatrix; // Transform world space to grid space

            // Precompute shape transformation
            shape.PrecomputeTransform(transform); //Passing the transform allows using the grid points directly, since they have a size of one.

            // Get shape bounds in world space and transform to grid space
            (Vector3 worldMin, Vector3 worldMax) = shape.GetWorldBoundingBox();
            Vector3 gridMin = worldToGrid.MultiplyPoint3x4(worldMin);
            Vector3 gridMax = worldToGrid.MultiplyPoint3x4(worldMax);

            //Expand by Vector3.one due to rounding.
            Vector3Int minGrid = Vector3Int.Max(Vector3Int.zero, Vector3Int.FloorToInt(gridMin) - Vector3Int.one);
            Vector3Int maxGrid = Vector3Int.Min(
                new Vector3Int(model.ResolutionX, model.ResolutionY, model.ResolutionZ),
                Vector3Int.CeilToInt(gridMax) + Vector3Int.one
            );

            float worldToGridScaleFactor = transform.localScale.magnitude;

            // Parallel processing
            System.Threading.Tasks.Parallel.For(minGrid.x, maxGrid.x, x =>
            {
                for (int y = minGrid.y; y < maxGrid.y; y++)
                {
                    for (int z = minGrid.z; z < maxGrid.z; z++)
                    {
                        // Transform grid position to world space
                        Vector3 gridPoint = new(x, y, z);

                        // Calculate the distance using the shape's transformation
                        float distance = shape.OptimizedDistance(gridPoint); //Note: Since this transform was passed for the transformation matrix and each grid point has a size of 1, the grid point can be used directly.

                        // Modify the voxel value
                        float currentValue = model.GetVoxel(x, y, z);
                        float newValue = modifier.ModifyVoxel(x, y, z, currentValue, distance);
                        model.SetVoxel(x, y, z, newValue);
                    }
                }
            });

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