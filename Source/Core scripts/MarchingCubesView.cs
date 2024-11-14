using System.Collections.Generic;
using UnityEngine;

namespace iffnsStuff.MarchingCubeEditor.Core
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
    public class MarchingCubesView : MonoBehaviour
    {
        private MeshFilter meshFilter;
        private MeshCollider meshCollider;

        public void Initialize()
        {
            meshFilter = GetComponent<MeshFilter>();
            meshCollider = GetComponent<MeshCollider>();
            meshFilter.mesh = new Mesh();
        }

        public void UpdateMesh(MarchingCubesMeshData meshData, bool updateCollider)
        {
            UpdateMesh(meshData.vertices, meshData.triangles, updateCollider);
        }

        public void UpdateMesh(List<Vector3> vertices, List<int> triangles, bool updateCollider)
        {
            Mesh mesh = meshFilter.sharedMesh;
            mesh.Clear();

            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals();

            // Update the mesh collider if requested
            if (updateCollider)
            {
                meshCollider.sharedMesh = null; // Clear previous collider mesh
                meshCollider.sharedMesh = mesh; // Assign the updated mesh to the collider
            }
        }
    }
}
