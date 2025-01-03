using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimplifyMeshTest : MonoBehaviour
{
    [SerializeField] MeshFilter baseMeshFilter;
    [SerializeField] MeshFilter outputMeshFilter;
    [SerializeField] float angleThresholdDeg = 1f;
    [SerializeField] float areaThreshold = 0.001f;

    private void Start()
    {
        Mesh mesh = new Mesh();

        mesh.vertices = baseMeshFilter.mesh.vertices;
        mesh.triangles = baseMeshFilter.mesh.triangles;

        Debug.Log($"Running for: {transform.name}");
        MeshUtilityFunctions.RemoveDegenerateTriangles(mesh, angleThresholdDeg, areaThreshold);

        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        mesh.RecalculateBounds();

        outputMeshFilter.sharedMesh = mesh;
    }
}
