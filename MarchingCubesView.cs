using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class MarchingCubesView : MonoBehaviour
{
    private MeshFilter meshFilter;

    private void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshFilter.mesh = new Mesh();
    }

    public void UpdateMesh(MarchingCubesMeshData meshData)
    {
        Mesh mesh = meshFilter.mesh;
        mesh.Clear();

        mesh.SetVertices(meshData.vertices);
        mesh.SetTriangles(meshData.triangles, 0);
        mesh.RecalculateNormals();
    }

    public void UpdateMesh(List<Vector3> vertices, List<int> triangles)
    {
        Mesh mesh = meshFilter.mesh;
        mesh.Clear();

        mesh.SetVertices(vertices.ToArray());
        mesh.SetTriangles(triangles.ToArray(), 0);
        mesh.RecalculateNormals();
    }
}
