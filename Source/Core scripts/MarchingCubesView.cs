#if UNITY_EDITOR

using System.Collections.Generic;
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
        private bool invertedNormals;

        public Vector3Int GridBoundsMin => gridBoundsMin;
        public Vector3Int GridBoundsMax => gridBoundsMax;

        public void Initialize(Vector3Int gridBoundsMin, Vector3Int gridBoundsMax, bool colliderEnabled)
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

            meshCollider.enabled = colliderEnabled;

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

        public void UpdateMeshIfDirty(MarchingCubesModel model)
        {
            if (!isDirty) return;

            // Generate mesh data for this chunk
            MarchingCubesMeshData meshData = GenerateChunkMesh(model);

            // Update the view's mesh
            UpdateMesh(meshData);

            isDirty = false; // Mark as clean
        }

        private MarchingCubesMeshData GenerateChunkMesh(MarchingCubesModel model)
        {
            MarchingCubesMeshData meshData = new MarchingCubesMeshData();

            for (int x = gridBoundsMin.x; x < gridBoundsMax.x; x++)
            {
                for (int y = gridBoundsMin.y; y < gridBoundsMax.y; y++)
                {
                    for (int z = gridBoundsMin.z; z < gridBoundsMax.z; z++)
                    {
                        // Directly query the model for cube weights
                        VoxelData[] cubeData = model.GetCubeWeights(x, y, z);
                        MarchingCubes.GenerateCubeMesh(meshData, cubeData, x - gridBoundsMin.x, y - gridBoundsMin.y, z - gridBoundsMin.z, invertedNormals);
                    }
                }
            }

            return meshData;
        }

        public void UpdateMesh(MarchingCubesMeshData meshData)
        {
            UpdateMesh(meshData.vertices, meshData.triangles, meshData.colors);
        }

        public void UpdateMesh(List<Vector3> vertices, List<int> triangles, List<Color32> colors)
        {
            Mesh mesh = meshFilter.sharedMesh;
            mesh.Clear();

            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.SetColors(colors);
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            //mesh.RecalculateBounds(); // Not needed in this case since recalculated automatically when setting the triangles: https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Mesh.RecalculateBounds.html

            // Update collider if needed.
            if (ColliderEnabled) UpdateCollider();
        }

        void UpdateCollider()
        {
            meshCollider.sharedMesh = null;

            Mesh mesh = meshFilter.sharedMesh;

            if (mesh.vertexCount == 0 || mesh.triangles.Length == 0) return; // Prevent invalid mesh assignment

            meshCollider.sharedMesh = mesh;
        }

        public bool InvertedNormals
        {
            set
            {
                if (invertedNormals != value)
                    InvertMeshTriangles();
                
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

            if (ColliderEnabled) 
                UpdateCollider();
        }

        public bool ColliderEnabled
        {
            get
            {
                return meshCollider.enabled;
            }
            set
            {
                if (!ColliderEnabled && value) UpdateCollider();

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

        public void PostProcessMesh()
        {
            MergeCloseVertices(meshFilter.sharedMesh, 0.5f);

            if (ColliderEnabled) UpdateCollider();
        }

        public static void MergeCloseVertices(Mesh mesh, float threshold)
        {
            Vector3[] originalVertices = mesh.vertices;
            Color[] originalColors = mesh.colors;
            int[] originalTriangles = mesh.triangles;

            List<Vector3> newVertices = new List<Vector3>();
            List<Color> newColors = new List<Color>();
            Dictionary<int, int> vertexMapping = new Dictionary<int, int>();

            for (int i = 0; i < originalVertices.Length; i++)
            {
                bool merged = false;

                for (int j = 0; j < newVertices.Count; j++)
                {
                    if (Vector3.Distance(newVertices[j], originalVertices[i]) < threshold)
                    {
                        // Map this vertex to an existing one
                        vertexMapping[i] = j;

                        // Merge colors by averaging
                        newColors[j] = (newColors[j] + originalColors[i]) * 0.5f;

                        merged = true;
                        break;
                    }
                }

                if (!merged)
                {
                    // Add as a new unique vertex and preserve its color
                    vertexMapping[i] = newVertices.Count;
                    newVertices.Add(originalVertices[i]);
                    newColors.Add(originalColors[i]);
                }
            }

            // Rebuild the triangle array with new vertex indices
            int[] newTriangles = new int[originalTriangles.Length];
            for (int i = 0; i < originalTriangles.Length; i++)
            {
                newTriangles[i] = vertexMapping[originalTriangles[i]];
            }

            // Update the mesh
            mesh.Clear();
            mesh.vertices = newVertices.ToArray();
            mesh.triangles = newTriangles;
            mesh.colors = newColors.ToArray(); // Assign the new colors
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
        }
    }
}

#endif