//#define DEBUG_PERFORMANCE

using iffnsStuff.MarchingCubeEditor.EditTools;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEditor;
using UnityEngine;
using static BaseModificationTools;

namespace iffnsStuff.MarchingCubeEditor.Core
{
    public class MarchingCubesController : MonoBehaviour
    {
        private List<MarchingCubesView> chunkViews = new();
        private MarchingCubesModel model;

        [SerializeField] private Vector3Int chunkSize = new(16, 16, 16);

        public bool showGridOutline = false; // Toggle controlled by the editor tool

        public int GridResolutionX => model.VoxelData.GetLength(0);
        public int GridResolutionY => model.VoxelData.GetLength(1);
        public int GridResolutionZ => model.VoxelData.GetLength(2);

        [SerializeField] GameObject chunkPrefab; // Prefab for chunk views

        public void Initialize(int resolutionX, int resolutionY, int resolutionZ, bool setEmpty)
        {
            model = new MarchingCubesModel(resolutionX, resolutionY, resolutionZ);

            // Reuse or deactivate excess chunks
            int activeChunkCount = 0;
            Vector3Int gridResolution = new(resolutionX, resolutionY, resolutionZ);

            for (int x = 0; x < resolutionX; x += chunkSize.x)
            {
                for (int y = 0; y < resolutionY; y += chunkSize.y)
                {
                    for (int z = 0; z < resolutionZ; z += chunkSize.z)
                    {
                        // Define chunk bounds
                        Vector3Int start = new(x, y, z);
                        Vector3Int size = Vector3Int.Min(chunkSize, gridResolution - start);

                        MarchingCubesView chunkView;

                        // Reuse an existing chunk view if available
                        if (activeChunkCount < chunkViews.Count)
                        {
                            chunkView = chunkViews[activeChunkCount];
                            chunkView.gameObject.SetActive(true);
                        }
                        else
                        {
                            // Instantiate a new chunk view if no reusable chunk is available
                            GameObject chunkObj = Instantiate(chunkPrefab, transform);
                            chunkView = chunkObj.GetComponent<MarchingCubesView>();
                            chunkViews.Add(chunkView);
                        }

                        chunkView.Initialize(start, size);
                        activeChunkCount++;
                    }
                }
            }

            // Deactivate excess chunks
            for (int i = activeChunkCount; i < chunkViews.Count; i++)
            {
                chunkViews[i].gameObject.SetActive(false);
            }

            if (setEmpty) UpdateAllChunks(false);
        }

        public bool IsInitialized => model != null;

        public void ModifyShape(EditShape shape, IVoxelModifier modifier, bool updateCollider)
        {
            // Precompute transformation
            Matrix4x4 worldToGrid = transform.worldToLocalMatrix;
            shape.PrecomputeTransform(transform);

            // Determine affected grid bounds
            (Vector3 worldMin, Vector3 worldMax) = shape.GetWorldBoundingBox();
            Vector3 gridMin = worldToGrid.MultiplyPoint3x4(worldMin);
            Vector3 gridMax = worldToGrid.MultiplyPoint3x4(worldMax);

            Vector3Int minGrid = Vector3Int.Max(Vector3Int.zero, Vector3Int.FloorToInt(gridMin));
            Vector3Int maxGrid = Vector3Int.Min(
                new Vector3Int(model.ResolutionX, model.ResolutionY, model.ResolutionZ),
                Vector3Int.CeilToInt(gridMax)
            );

            // Modify the voxel data in affected grid region
            for (int x = minGrid.x; x < maxGrid.x; x++)
            {
                for (int y = minGrid.y; y < maxGrid.y; y++)
                {
                    for (int z = minGrid.z; z < maxGrid.z; z++)
                    {
                        Vector3 gridPoint = new(x, y, z);
                        float distance = shape.OptimizedDistance(gridPoint);

                        float currentValue = model.GetVoxel(x, y, z);
                        float newValue = modifier.ModifyVoxel(x, y, z, currentValue, distance);
                        model.SetVoxel(x, y, z, newValue);
                    }
                }
            }

            // Mark affected chunks as dirty
            MarkAffectedChunksDirty(minGrid, maxGrid);

            // Update affected chunk meshes
            UpdateAffectedChunks(minGrid, maxGrid, updateCollider);
        }

        private void MarkAffectedChunksDirty(Vector3Int minGrid, Vector3Int maxGrid)
        {
            foreach (var chunkView in chunkViews)
            {
                if (chunkView.IsWithinBounds(minGrid, maxGrid))
                {
                    chunkView.MarkDirty();
                }
            }
        }

        private void UpdateAffectedChunks(Vector3Int minGrid, Vector3Int maxGrid, bool enableCollider)
        {
            foreach (var chunkView in chunkViews)
            {
                if (chunkView.IsWithinBounds(minGrid, maxGrid))
                {
                    chunkView.UpdateMeshIfDirty(model, enableCollider);
                }
            }
        }

        void UpdateAllChunks(bool enableCollider)
        {
            foreach (var chunkView in chunkViews)
            {
                chunkView.MarkDirty();
                chunkView.UpdateMeshIfDirty(model, enableCollider);
            }
        }

        public bool InvertAllNormals
        {
            set
            {
                foreach (MarchingCubesView chunkView in chunkViews)
                {
                    chunkView.InvertedNormals = value;
                }
            }
        }

        public bool EnableAllColliders
        {
            set
            {
                foreach (MarchingCubesView chunkView in chunkViews)
                {
                    chunkView.ColliderEnabled = true;
                }
            }
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
            UpdateAllChunks(false);
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

                UpdateAllChunks(updateColliders); // Refresh mesh
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