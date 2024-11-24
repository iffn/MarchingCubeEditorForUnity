using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace iffnsStuff.MarchingCubeEditor.Core
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
    public class MarchingCubesView : MonoBehaviour
    {
        private MeshFilter meshFilter;
        private MeshCollider meshCollider;

        private Vector3Int chunkStart;   // Start position of this chunk in model space
        private Vector3Int chunkSize;   // Size of this chunk
        private bool isDirty;           // Whether this chunk's mesh needs updating
        bool invertedNormals = false;

        public void Initialize(Vector3Int start, Vector3Int size)
        {
            transform.localPosition = chunkStart;

            meshFilter = GetComponent<MeshFilter>();
            meshCollider = GetComponent<MeshCollider>();

            chunkStart = start;
            chunkSize = size;

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

            int lookupEndX = System.Math.Min(chunkStart.x + chunkSize.x, model.ResolutionX - 1);
            int lookupEndY = System.Math.Min(chunkStart.y + chunkSize.y, model.ResolutionY - 1);
            int lookupEndZ = System.Math.Min(chunkStart.z + chunkSize.z, model.ResolutionZ - 1);

            for (int x = chunkStart.x; x < lookupEndX; x++)
            {
                for (int y = chunkStart.y; y < lookupEndY; y++)
                {
                    for (int z = chunkStart.z; z < lookupEndZ; z++)
                    {
                        // Directly query the model for cube weights
                        float[] cubeWeights = model.GetCubeWeights(x, y, z);
                        MarchingCubes.GenerateCubeMesh(meshData, cubeWeights, x - chunkStart.x, y - chunkStart.y, z - chunkStart.z, invertedNormals);
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
                if(value != invertedNormals)
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
            set
            {
                if (value)
                {
                    meshCollider.sharedMesh = null;
                    meshCollider.sharedMesh = meshFilter.sharedMesh;
                }
                
                meshCollider.enabled = value;
            }
        }


        public bool IsWithinBounds(Vector3Int minGrid, Vector3Int maxGrid)
        {
            Vector3Int chunkEnd = chunkStart + chunkSize;

            // Check for overlap between the chunk and the affected region
            return !(chunkEnd.x <= minGrid.x || chunkStart.x >= maxGrid.x ||
                     chunkEnd.y <= minGrid.y || chunkStart.y >= maxGrid.y ||
                     chunkEnd.z <= minGrid.z || chunkStart.z >= maxGrid.z);
        }
    }
}
