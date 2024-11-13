using iffnsStuff.MarchingCubeEditor.EditTools;
using UnityEngine;

namespace iffnsStuff.MarchingCubeEditor.Core
{
    [RequireComponent(typeof(MarchingCubesView))]
    public class MarchingCubesController : MonoBehaviour
    {
        private MarchingCubesModel model;
        private MarchingCubesMeshData meshData;
        private MarchingCubesView view;

        public bool showGridOutline = false; // Toggle controlled by the editor tool

        public void Initialize(int resolution, bool setEmpty)
        {
            view = GetComponent<MarchingCubesView>();
            model = new MarchingCubesModel(resolution);

            view.Initialize();

            if (setEmpty) SetEmptyGrid();
        }

        public void GenerateAndDisplayMesh()
        {
            meshData = new MarchingCubesMeshData();
            int resolution = model.Resolution;

            for (int x = 0; x < resolution - 1; x++)
            {
                for (int y = 0; y < resolution - 1; y++)
                {
                    for (int z = 0; z < resolution - 1; z++)
                    {
                        float[] cubeWeights = model.GetCubeWeights(x, y, z);
                        MarchingCubes.GenerateCubeMesh(meshData, cubeWeights, x, y, z);
                    }
                }
            }

            view.UpdateMesh(meshData);
        }

        public void SetEmptyGrid()
        {
            int resolution = model.Resolution;
            for (int x = 0; x < resolution; x++)
            {
                for (int y = 0; y < resolution; y++)
                {
                    for (int z = 0; z < resolution; z++)
                    {
                        model.SetVoxel(x, y, z, -1); // Use signed distance
                    }
                }
            }
            GenerateAndDisplayMesh(); // Update the mesh after modification
        }

        public void AddShape(EditShape shape)
        {
            int resolution = model.Resolution;

            for (int x = 0; x < resolution; x++)
            {
                for (int y = 0; y < resolution; y++)
                {
                    for (int z = 0; z < resolution; z++)
                    {
                        Vector3 point = new(x, y, z);
                        float distanceOutsideIsPositive = shape.DistanceOutsideIsPositive(point);

                        model.AddVoxel(x, y, z, -distanceOutsideIsPositive);
                    }
                }
            }

            GenerateAndDisplayMesh();
        }

        public void SubtractShape(EditShape shape)
        {
            int resolution = model.Resolution;

            for (int x = 0; x < resolution; x++)
            {
                for (int y = 0; y < resolution; y++)
                {
                    for (int z = 0; z < resolution; z++)
                    {
                        Vector3 point = new(x, y, z);
                        float distanceOutsideIsPositive = shape.DistanceOutsideIsPositive(point);

                        model.SubtractVoxel(x, y, z, distanceOutsideIsPositive);
                    }
                }
            }

            GenerateAndDisplayMesh();
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
            float gridSize = model.Resolution - 1; // Since grid is zero-indexed, subtract 1 to get the bounds
            Vector3 cellSize = Vector3.one;  // Adjust if each voxel cell has different dimensions

            // Calculate the offset for half the grid size along each axis
            Vector3 halfGridSize = new Vector3(gridSize * cellSize.x * 0.5f,
                                               gridSize * cellSize.y * 0.5f,
                                               gridSize * cellSize.z * 0.5f);


            // Calculate the starting position of the grid (bottom-left-front corner)
            Vector3 gridOrigin = transform.position;

            // Calculate all eight corners of the grid box
            Vector3[] corners = new Vector3[8];
            corners[0] = gridOrigin;
            corners[1] = gridOrigin + new Vector3(gridSize * cellSize.x, 0, 0);
            corners[2] = gridOrigin + new Vector3(gridSize * cellSize.x, gridSize * cellSize.y, 0);
            corners[3] = gridOrigin + new Vector3(0, gridSize * cellSize.y, 0);
            corners[4] = gridOrigin + new Vector3(0, 0, gridSize * cellSize.z);
            corners[5] = gridOrigin + new Vector3(gridSize * cellSize.x, 0, gridSize * cellSize.z);
            corners[6] = gridOrigin + new Vector3(gridSize * cellSize.x, gridSize * cellSize.y, gridSize * cellSize.z);
            corners[7] = gridOrigin + new Vector3(0, gridSize * cellSize.y, gridSize * cellSize.z);

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