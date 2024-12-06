using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace iffnsStuff.MarchingCubeEditor.Core
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
    public class MarchingCubesView : MonoBehaviour
    {
        private MeshFilter meshFilter;
        private MeshCollider meshCollider;

        Vector3Int gridBoundsMin;
        Vector3Int gridBoundsMax;

        private bool isDirty;           // Whether this chunk's mesh needs updating
        bool invertedNormals = false;

        public Vector3Int GridBoundsMin => gridBoundsMin;
        public Vector3Int GridBoundsMax => gridBoundsMax;

        public void Initialize(Vector3Int gridBoundsMin, Vector3Int gridBoundsMax)
        {
            this.gridBoundsMin = gridBoundsMin;
            this.gridBoundsMax = gridBoundsMax;

            transform.localPosition = new Vector3(gridBoundsMin.x, gridBoundsMin.y, gridBoundsMin.z);
            
            meshFilter = GetComponent<MeshFilter>();
            meshCollider = GetComponent<MeshCollider>();

            if (meshFilter.sharedMesh == null)
            {
                meshFilter.mesh = new Mesh();
            }
            else
            {
                meshFilter.sharedMesh.Clear(); // Clear existing mesh data for reuse
            }

            meshCollider.sharedMesh = meshFilter.sharedMesh;

            isDirty = true; // Mark the chunk as dirty upon initialization
        }

        public void UpdateBounds(Vector3Int min, Vector3Int max)
        {
            gridBoundsMin = min;
            gridBoundsMax = max;

            transform.localPosition = new Vector3(min.x, min.y, min.z);
        }

        private void OnDestroy()
        {
            // Safely destroy the dynamically created mesh
            if (meshFilter != null && meshFilter.sharedMesh != null)
            {
                Destroy(meshFilter.sharedMesh);
            }

            // Optionally clear the collider's mesh reference
            if (meshCollider != null)
            {
                meshCollider.sharedMesh = null;
            }
        }

        public void MarkDirty()
        {
            isDirty = true;
        }

        public void UpdateMeshIfDirty(MarchingCubesModel model, bool enableCollider)
        {
            if (!isDirty) return;

            // Generate mesh data for this chunk
            MarchingCubesMeshData meshData = GenerateChunkMesh(model);

            // Update the view's mesh
            UpdateMesh(meshData, enableCollider);

            isDirty = false; // Mark as clean
        }

        private MarchingCubesMeshData GenerateChunkMesh(MarchingCubesModel model)
        {
            MarchingCubesMeshData meshData = new();

            for (int x = gridBoundsMin.x; x < gridBoundsMax.x; x++)
            {
                for (int y = gridBoundsMin.y; y < gridBoundsMax.y; y++)
                {
                    for (int z = gridBoundsMin.z; z < gridBoundsMax.z; z++)
                    {
                        // Directly query the model for cube weights
                        float[] cubeWeights = model.GetCubeWeights(x, y, z);
                        MarchingCubes.GenerateCubeMesh(meshData, cubeWeights, x - gridBoundsMin.x, y - gridBoundsMin.y, z - gridBoundsMin.z, invertedNormals);
                    }
                }
            }

            return meshData;
        }

        public void UpdateMesh(MarchingCubesMeshData meshData, bool enableCollider)
        {
            UpdateMesh(meshData.vertices, meshData.triangles, enableCollider);
        }

        public void UpdateMesh(List<Vector3> vertices, List<int> triangles, bool enableCollider)
        {
            Mesh mesh = meshFilter.sharedMesh;
            mesh.Clear();

            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals();

            meshCollider.enabled = enableCollider;

            ColliderEnabled = enableCollider;
        }

        public bool InvertedNormals
        {
            set
            {
                if (value != invertedNormals)
                {
                    InvertMeshTriangles();
                }

                if (meshCollider.enabled) ColliderEnabled = true;

                invertedNormals = value;
            }
        }

        void InvertMeshTriangles()
        {
            Mesh mesh = meshFilter.sharedMesh;

            // Get the current triangles from the mesh
            int[] triangles = mesh.triangles;

            // Reverse the winding order for each triangle
            for (int i = 0; i < triangles.Length; i += 3)
            {
                // Swap the second and third indices to reverse the winding
                (triangles[i + 2], triangles[i + 1]) = (triangles[i + 1], triangles[i + 2]);
            }

            // Update the mesh with the inverted triangles
            mesh.triangles = triangles;

            // Recalculate normals to reflect the inverted geometry
            mesh.RecalculateNormals();
        }

        public bool ColliderEnabled
        {
            get
            {
                return meshCollider.enabled;
            }
            set
            {
                if (!value)
                {
                    meshCollider.sharedMesh = null;
                }
                else if(!ColliderEnabled)
                {
                    meshCollider.sharedMesh = null;

                    Mesh mesh = meshFilter.sharedMesh;

                    if (mesh.vertexCount > 0 && mesh.triangles.Length > 0) // Prevent invalid mesh assignment
                    {
                        meshCollider.sharedMesh = mesh;
                    }
                }

                meshCollider.enabled = value;
            }
        }

        public bool IsWithinBounds(Vector3Int min, Vector3Int max)
        {
            // Check for overlap between the chunk and the affected region
            return !(gridBoundsMax.x <= min.x || gridBoundsMin.x >= max.x ||
                     gridBoundsMax.y <= min.y || gridBoundsMin.y >= max.y ||
                     gridBoundsMax.z <= min.z || gridBoundsMin.z >= max.z);
        }
    }
}
