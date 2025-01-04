using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MeshUtilityFunctions
{
    // Based on:
    // https://github.com/Shirakumo/manifolds/blob/main/normalize.lisp
    // https://github.com/Shirakumo/manifolds/blob/main/manifolds.lisp

    public static void RemoveDegenerateTriangles(Mesh mesh, float angleThreshold = 0.01f, float areaThreshold = 0.001f) // Based on remove-degenerate-triangles
    {
        System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();

        Vector3[] vertices = mesh.vertices;
        Color[] colors = mesh.colors;

        bool considerColors = colors.Length == vertices.Length;

        if(!considerColors)
            colors = new Color[vertices.Length]; // Create temporary array but don't use the data in the end

        int[] indices = mesh.triangles;

        List<Vector3> newPositions = new List<Vector3>(vertices);
        List<Color> newColors = new List<Color>(colors);
        List<int> newIndices = new List<int>(indices);

        List<int>[] adjacency = FaceAdjacencyList(indices);
        Dictionary<int, List<int>> vertexFaces = VertexFaces(indices);

        bool ConsiderArea(int a, int b, int c, int face)
        {
            Vector3 ap = vertices[a];
            Vector3 bp = vertices[b];
            Vector3 cp = vertices[c];

            float area = TriangleArea(ap, bp, cp);
            if (area < areaThreshold)
            {
                float a_d = Vector3.Distance(ap, bp);
                float b_d = Vector3.Distance(bp, cp);
                float c_d = Vector3.Distance(cp, ap);

                if (a_d < b_d && a_d < c_d)
                {
                    FuseEdge(a, b, face);
                }
                else if (b_d < a_d && b_d < c_d)
                {
                    FuseEdge(b, c, face);
                }
                else
                {
                    FuseEdge(c, a, face);
                }
                return true;
            }
            return false;
        }

        bool ConsiderAngle(int corner, int a, int b, int face)
        {
            Vector3 cp = vertices[corner];
            Vector3 ap = vertices[a];
            Vector3 bp = vertices[b];

            if (Vector3.Angle(ap - cp, bp - cp) < angleThreshold)
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
            // Compute the midpoint of the edge (a, b)
            Vector3 middlePosition = (GetVertex(a) + GetVertex(b)) * 0.5f;
            Color middleColor = (GetColor(a) + GetColor(b)) * 0.5f; // ToConsider: Assumption regarding picking the average color

            // Set vertex a to the midpoint and zero out vertex b
            SetVertex(a, middlePosition, middleColor);
            SetVertex(b, Vector3.zero, Color.black);

            // Update all triangles that reference vertex b to reference vertex a
            foreach (int adjacentFace in vertexFaces[b])
            {
                UpdateTriangle(adjacentFace, b, a);
            }

            // Delete the current triangle and all triangles adjacent to the edge (a, b)
            foreach (int adjFace in AdjacentFaces(face, a, b, indices, adjacency))
            {
                DeleteTriangle(adjFace);
            }
            DeleteTriangle(face);
        }

        void SplitEdge(int a, int b, int c, int face) // Based on split-edge
        {
            /*
            Split AB edge to M, create new triangles AMD, BMD
            where D is the opposing corner of any triangle over AB,
            mark the original triangles for deletion, and update any
            triangles that used to refer to C to M.
            
            This is messy because we update the adjacency map and vertex
            face map in-place to avoid recomputing them on each iteration
            */

            // Compute the midpoint of the edge (a, b)
            // (let ((mid (nv* (nv+ (v vertices a) (v vertices b)) 0.5)))
            Vector3 middlePosition = (GetVertex(a) + GetVertex(b)) * 0.5f;
            Color middleColor = (GetColor(a) + GetColor(b)) * 0.5f; // ToConsider: Assumption regarding picking the average color

            // Add the midpoint as a new vertex
            // (vector-push-extend (vx mid) vertices)
            // (vector-push-extend (vy mid) vertices)
            // (vector-push-extend (vz mid) vertices)
            int m = newPositions.Count;
            AddVertex(middlePosition, middleColor);

            // Initialize vertex-face mapping for the new vertex
            // (vector-push-extend (make-array 0 :adjustable T :fill-pointer T) vfaces)
            vertexFaces[m] = new List<int>();

            // Update all triangles that reference vertex c to reference m instead
            // (loop for face across cornering do (update-triangle face c m))
            foreach (int cornerFace in vertexFaces[c].ToArray())
            {
                UpdateTriangle(cornerFace, c, m);
            }

            /*
            (let* ((adjacents (adjacent-faces face a b indices adjacency))
                    (cornering (vfaces c)))
                (loop for face across cornering
                        do (update-triangle face c m))
                (loop for face in adjacents
                        for d = (face-corner face a b indices)
                        for al = (make-triangle d m a (adjacent-faces face d a indices adjacency))
                        for ar = (make-triangle d b m (adjacent-faces face d b indices adjacency))
                        do (push al (aref adjacency ar))
                        (push ar (aref adjacency al))
                        (loop for face across cornering
                                do (cond ((face-edge-p indices face a m)
                                        (push al (aref adjacency face)))
                                        ((face-edge-p indices face b m)
                                        (push ar (aref adjacency face))))))
            */

            IEnumerable<int> adjacents = AdjacentFaces(face, a, b, indices, adjacency); // (adjacent-faces face a b ...)
            int[] cornering = vertexFaces[c].ToArray(); // (vfaces c)

            // First loop: update triangles in cornering to replace c with m
            foreach (int cornerFace in cornering)
            {
                UpdateTriangle(cornerFace, c, m); // (update-triangle face c m)
            }

            // Second loop: process adjacents and create new triangles
            foreach (int adjFace in adjacents)
            {
                int d = FaceCorner(adjFace, a, b, indices); // (face-corner face a b ...)
                int al = AddTriangle(d, m, a); // (make-triangle d m a ...)
                int ar = AddTriangle(d, b, m); // (make-triangle d b m ...)

                UpdateAdjacency(al, ar); // (push al (aref adjacency ar))
                UpdateAdjacency(ar, al); // (push ar (aref adjacency al))

                // Inner loop: link cornering triangles with new triangles
                foreach (int cornerFace in cornering)
                {
                    if (EdgeExists(cornerFace, a, m)) // (face-edge-p indices face a m)
                    {
                        UpdateAdjacency(cornerFace, al); // (push al (aref adjacency face))
                    }
                    else if (EdgeExists(cornerFace, b, m)) // (face-edge-p indices face b m)
                    {
                        UpdateAdjacency(cornerFace, ar); // (push ar (aref adjacency face))
                    }
                }
            }


            // Delete the original face
            // (delete-triangle face)
            DeleteTriangle(face);

            // Delete all adjacent faces across the edge (a, b)
            // (mapc #'delete-triangle adjacents)
            foreach (int adjFace in AdjacentFaces(face, a, b, indices, adjacency))
            {
                DeleteTriangle(adjFace);
            }

            // Local helper functions
            void UpdateAdjacency(int face1, int face2)
            {
                if (!adjacency[face1].Contains(face2))
                {
                    adjacency[face1].Add(face2);
                }
                if (!adjacency[face2].Contains(face1))
                {
                    adjacency[face2].Add(face1);
                }
            }

            bool EdgeExists(int face, int v1, int v2)
            {
                int i0 = indices[face * 3];
                int i1 = indices[face * 3 + 1];
                int i2 = indices[face * 3 + 2];
                return (i0 == v1 && i1 == v2) || (i1 == v1 && i2 == v2) || (i2 == v1 && i0 == v2) ||
                       (i0 == v2 && i1 == v1) || (i1 == v2 && i2 == v1) || (i2 == v2 && i0 == v1);
            }
        }

        void UpdateTriangle(int face, int oldVertex, int newVertex)
        {
            int i = face * 3;
            for (int j = 0; j < 3; j++)
            {
                if (newIndices[i + j] == oldVertex)
                {
                    newIndices[i + j] = newVertex;
                }
            }
        }

        Vector3 GetVertex(int index) => newPositions[index];
        Color GetColor(int index) => newColors[index];

        void SetVertex(int index, Vector3 position, Color color)
        {
            newPositions[index] = position;
            newColors[index] = color;
        }

        void AddVertex(Vector3 position, Color color)
        {
            newPositions.Add(position);
            newColors.Add(color);
        }

        int AddTriangle(int a, int b, int c)
        {
            int newFaceIndex = newIndices.Count / 3;
            newIndices.Add(a);
            newIndices.Add(b);
            newIndices.Add(c);

            // Ensure adjacency array is large enough
            if (newFaceIndex >= adjacency.Length)
            {
                Array.Resize(ref adjacency, newFaceIndex + 1);
                adjacency[newFaceIndex] = new List<int>();
            }

            return newFaceIndex;
        }

        void DeleteTriangle(int face)
        {
            int i = face * 3;
            newIndices[i] = newIndices[i + 1] = newIndices[i + 2] = 0;
        }

        float TriangleArea(Vector3 a, Vector3 b, Vector3 c)
        {
            return Vector3.Cross(b - a, c - a).magnitude * 0.5f;
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
                    (ConsiderArea(p1, p2, p3, face) ||
                     ConsiderAngle(p1, p2, p3, face) ||
                     ConsiderAngle(p2, p1, p3, face) ||
                     ConsiderAngle(p3, p1, p2, face)))
                {
                    changed = true;
                    vertices = newPositions.ToArray();
                    indices = newIndices.ToArray();
                    vertexFaces = VertexFaces(indices);
                    break;
                }
            }

            if (!changed) break;
        }

        mesh.Clear();
        mesh.vertices = newPositions.ToArray();
        mesh.triangles = newIndices.ToArray();
        if(considerColors) mesh.colors = newColors.ToArray();

        RemoveUnusedVertices(mesh);

        Debug.Log($"Total time needed: {stopwatch.Elapsed.TotalSeconds}");
    }

    static Dictionary<int, List<int>> VertexFaces(int[] faces)
    {
        // Create a dictionary to store the list of face indices for each vertex
        Dictionary<int, List<int>> vertexFaces = new Dictionary<int, List<int>>();

        // Initialize the dictionary
        foreach (int vertex in faces)
        {
            if (!vertexFaces.ContainsKey(vertex))
            {
                vertexFaces[vertex] = new List<int>();
            }
        }

        // Populate the dictionary
        for (int i = 0; i < faces.Length; i += 3)
        {
            int faceIndex = i / 3;
            vertexFaces[faces[i]].Add(faceIndex);
            vertexFaces[faces[i + 1]].Add(faceIndex);
            vertexFaces[faces[i + 2]].Add(faceIndex);
        }

        return vertexFaces;
    }


    public static void RemoveUnusedVertices(Mesh mesh) // remove-duplicate-vertices
    {
        Vector3[] vertices = mesh.vertices;
        Color[] colors = mesh.colors;
        int[] indices = mesh.triangles;

        bool considerColors = colors.Length == vertices.Length;

        if (!considerColors)
            colors = new Color[vertices.Length]; // Create temporary array but don't use the data in the end

        int vertexCount = vertices.Length;

        // Define helper functions
        List<Vector3> newVertices = new List<Vector3>();
        List<Color> newColors = new List<Color>();

        void AddOldVertexInfoToNewLists(int oldIndex)
        {
            newVertices.Add(vertices[oldIndex]);
            newColors.Add(colors[oldIndex]);
            // ToDo: Add other info like UV
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
        mesh.vertices = newVertices.ToArray();
        mesh.triangles = newIndices;
        if(considerColors) mesh.colors = newColors.ToArray();
        // ToDo: Add other info like UV
    }

    static IEnumerable<int> AdjacentFaces(int face, int a, int b, int[] faces, List<int>[] adjacency = null) // Based on adjacent-faces
    {
        if (adjacency == null)
        {
            adjacency = FaceAdjacencyList(faces);
        }

        foreach (int adjacent in adjacency[face])
        {
            int i0 = faces[adjacent * 3];
            int i1 = faces[adjacent * 3 + 1];
            int i2 = faces[adjacent * 3 + 2];

            if ((i0 == a && (i1 == b || i2 == b)) ||
                (i1 == a && (i0 == b || i2 == b)) ||
                (i2 == a && (i0 == b || i1 == b)))
            {
                yield return adjacent;
            }
        }
    }

    public static List<int>[] FaceAdjacencyList(int[] faces) // Based on face-adjacency-list
    {
        // Initialize hash table and adjacency list
        Dictionary<long, List<int>> table = new Dictionary<long, List<int>>();
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
        foreach (List<int> adjacents in table.Values)
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

    public static int FaceCorner(int face, int a, int b, int[] faces) // Based on face-corner
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