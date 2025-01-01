using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimplifyMeshTest : MonoBehaviour
{
    [SerializeField] MeshFilter baseMeshFilter;
    [SerializeField] MeshFilter outputMeshFilter;
    [SerializeField] float threshold = 1;

    private void Start()
    {
        Mesh mesh = new Mesh();

        mesh.vertices = baseMeshFilter.mesh.vertices;
        mesh.triangles = baseMeshFilter.mesh.triangles;

        MeshUtilityFunctions.RemoveDegenerateTriangles(mesh, threshold);

        outputMeshFilter.sharedMesh = mesh;
    }
}
