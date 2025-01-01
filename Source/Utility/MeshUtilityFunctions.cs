using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MeshUtilityFunctions
{
    // Based on:
    // https://github.com/Shirakumo/manifolds/blob/main/normalize.lisp
    // https://github.com/Shirakumo/manifolds/blob/main/manifolds.lisp

    public static void RemoveDegenerateTriangles(
        Mesh mesh, float threshold = 0.01f)
    {
        Vector3[] vertices = mesh.vertices;
        int[] indices = mesh.triangles;

        List<Vector3> newVertices = new List<Vector3>(vertices);
        List<int> newIndices = new List<int>(indices);

        List<int>[] adjacency = FaceAdjacencyList(indices);

        bool Consider(int corner, int a, int b, int face)
        {
            Vector3 cp = vertices[corner];
            Vector3 ap = vertices[a];
            Vector3 bp = vertices[b];

            if (Vector3.Angle(ap - cp, bp - cp) < threshold)
            {
                float a_d = Vector3.Distance(cp, ap);
                float b_d = Vector3.Distance(cp, bp);
                float ab_d = Vector3.Distance(ap, bp);

                if (ab_d < a_d && ab_d < b_d)
                {
                    FuseEdge(a, b, face);
                }
                else if (a_d < b_d)
                {
                    SplitEdge(corner, b, a, face);
                }
                else
                {
                    SplitEdge(corner, a, b, face);
                }

                return true;
            }

            return false;
        }

        void FuseEdge(int a, int b, int face)
        {
            var mid = (GetVertex(a) + GetVertex(b)) * 0.5f;
            SetVertex(a, mid);
            SetVertex(b, mid);
            DeleteTriangle(face);

            foreach (var adjFace in adjacency[face])
            {
                DeleteTriangle(adjFace);
            }
        }

        void SplitEdge(int a, int b, int c, int face)
        {
            var mid = (GetVertex(a) + GetVertex(b)) * 0.5f;
            int m = newVertices.Count / 3;
            AddVertex(mid);

            // Create new triangles
            AddTriangle(c, m, a);
            AddTriangle(c, b, m);

            foreach (var adjFace in adjacency[face])
            {
                int d = FaceCorner(adjFace, a, b, indices);
                AddTriangle(d, m, a);
                AddTriangle(d, b, m);
            }

            DeleteTriangle(face);
        }

        Vector3 GetVertex(int index) => newVertices[index];

        void SetVertex(int index, Vector3 value)
        {
            newVertices[index] = value; // ToDo: Add other info like UV or VertexColor
        }

        void AddVertex(Vector3 vertex)
        {
            newVertices.Add(vertex); // ToDo: Add other info like UV or VertexColor
        }

        void AddTriangle(int a, int b, int c)
        {
            newIndices.Add(a);
            newIndices.Add(b);
            newIndices.Add(c);
        }

        int deleted = 0;

        void DeleteTriangle(int face)
        {
            int i = face * 3;
            newIndices[i] = newIndices[i + 1] = newIndices[i + 2] = 0;

            deleted++;
        }

        while (true)
        {
            bool changed = false;

            for (int i = 0; i < indices.Length; i += 3)
            {
                int face = i / 3;
                int p1 = indices[i];
                int p2 = indices[i + 1];
                int p3 = indices[i + 2];

                if (p1 != p2 && p1 != p3 && p2 != p3 &&
                    (Consider(p1, p2, p3, face) || Consider(p2, p1, p3, face) || Consider(p3, p1, p2, face)))
                {
                    changed = true;
                    vertices = newVertices.ToArray();
                    indices = newIndices.ToArray();
                    adjacency = FaceAdjacencyList(indices);
                    break;
                }
            }

            if (!changed) break;
        }

        Debug.Log($"Deleted = {deleted}");

        mesh.Clear();
        mesh.vertices = newVertices.ToArray(); // ToDo: Add other info like UV or VertexColor
        mesh.triangles = newIndices.ToArray();

        RemoveUnusedVertices(mesh);
    }

    public static void RemoveUnusedVertices(Mesh mesh)
    {
        Vector3[] vertices = mesh.vertices;
        int[] indices = mesh.triangles;

        int vertexCount = vertices.Length;

        // Define helper functions
        List<Vector3> newVertices = new();
        void AddOldVertexInfoToNewLists(int oldIndex)
        {
            newVertices.Add(vertices[oldIndex]); // ToDo: Add other info like UV or VertexColor
        }

        // Step 1: Mark used vertices
        bool[] used = new bool[vertexCount];

        bool RealFace(int i0, int i1, int i2)
        {
            return i0 != i1 && i1 != i2 && i0 != i2 &&
                   i0 < vertexCount && i1 < vertexCount && i2 < vertexCount;
        }

        int faceCount = 0;
        for (int i = 0; i < indices.Length; i += 3)
        {
            int i0 = indices[i];
            int i1 = indices[i + 1];
            int i2 = indices[i + 2];

            if (RealFace(i0, i1, i2))
            {
                used[i0] = true;
                used[i1] = true;
                used[i2] = true;
                faceCount++;
            }
        }

        // Step 2: Create new vertices array and index map
        int[] indexMap = new int[vertexCount];
        int newIndex = 0;

        for (int oldIndex = 0; oldIndex < vertexCount; oldIndex++)
        {
            if (used[oldIndex])
            {
                // Add corresponding vertex components to new vertices
                AddOldVertexInfoToNewLists(oldIndex);

                // Update the index map
                indexMap[oldIndex] = newIndex;
                newIndex++;
            }
        }

        // Step 3: Rewrite indices based on the index map
        int[] newIndices = new int[faceCount * 3];
        int fi = 0;

        for (int i = 0; i < indices.Length; i += 3)
        {
            int i0 = indices[i];
            int i1 = indices[i + 1];
            int i2 = indices[i + 2];

            if (RealFace(i0, i1, i2))
            {
                newIndices[fi] = indexMap[i0];
                newIndices[fi + 1] = indexMap[i1];
                newIndices[fi + 2] = indexMap[i2];
                fi += 3;
            }
        }

        // Step 4: Rewrite data
        mesh.Clear();
        mesh.vertices = newVertices.ToArray(); // ToDo: Add other info like UV or VertexColor
        mesh.triangles = newIndices;
    }

    public static List<int>[] FaceAdjacencyList(int[] faces)
    {
        // Based on: https://github.com/Shirakumo/manifolds/blob/main/manifolds.lisp

        // Initialize hash table and adjacency list
        var table = new Dictionary<long, List<int>>();
        int faceCount = faces.Length / 3;
        List<int>[] adjacency = new List<int>[faceCount];
        for (int i = 0; i < adjacency.Length; i++)
        {
            adjacency[i] = new List<int>();
        }


        // Helper function to calculate and add edges to the table
        void AddEdge(int a, int b, int faceIndex)
        {
            if (b < a)
            {
                (a, b) = (b, a);
            }

            long edgeKey = ((long)a << 32) | (long)b; // Add both integers to a long bitwise
            if (!table.ContainsKey(edgeKey))
            {
                table[edgeKey] = new List<int>();
            }

            table[edgeKey].Add(faceIndex);
        }

        // Process each face
        int face = 0;
        for (int i = 0; i < faces.Length; i += 3)
        {
            int a = faces[i];
            int b = faces[i + 1];
            int c = faces[i + 2];

            AddEdge(a, b, face);
            AddEdge(b, c, face);
            AddEdge(c, a, face);

            face++;
        }

        // Populate adjacency list
        foreach (var adjacents in table.Values)
        {
            foreach (int currentFace in adjacents)
            {
                foreach (int otherFace in adjacents)
                {
                    if (otherFace != currentFace && !adjacency[currentFace].Contains(otherFace))
                    {
                        adjacency[currentFace].Add(otherFace);
                    }
                }
            }
        }

        return adjacency;
    }

    public static int FaceCorner(int face, int a, int b, int[] faces)
    {
        int i0 = faces[face * 3];
        int i1 = faces[face * 3 + 1];
        int i2 = faces[face * 3 + 2];

        if (i0 == a)
        {
            return (i1 == b) ? i2 : i1;
        }
        else if (i1 == a)
        {
            return (i0 == b) ? i2 : i0;
        }
        else if (i2 == a)
        {
            return (i0 == b) ? i1 : i0;
        }
        else
        {
            throw new ArgumentException($"Edge {a}, {b} is not part of face {face}!");
        }
    }

}