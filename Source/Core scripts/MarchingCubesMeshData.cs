#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;

namespace iffnsStuff.MarchingCubeEditor.Core
{
    public class MarchingCubesMeshData
    {
        public List<Vector3> vertices = new List<Vector3>();
        public List<int> triangles = new List<int>();
        public List<Color32> colors = new List<Color32>();
        private Dictionary<Vector3, int> vertexCache = new Dictionary<Vector3, int>();

        /// <summary>
        /// Adds a vertex if it doesn't already exist and returns its index.
        /// </summary>
        /// <param name="vertex">Vertex position to add.</param>
        /// <returns>Index of the vertex in the vertices list.</returns>
        public int AddVertex(Vector3 vertex, Color32 color)
        {
            if (vertexCache.TryGetValue(vertex, out int index))
            {
                // Return the existing index if the vertex is already in the cache
                return index;
            }

            // Add the vertex and cache its index
            index = vertices.Count;
            vertices.Add(vertex);
            colors.Add(color);
            vertexCache[vertex] = index;

            return index;
        }

        /// <summary>
        /// Adds a triangle using indices of already-added vertices.
        /// </summary>
        public void AddTriangle(int index1, int index2, int index3)
        {
            triangles.Add(index1);
            triangles.Add(index2);
            triangles.Add(index3);
        }

        /// <summary>
        /// Clears all vertices, triangles, and the vertex cache.
        /// </summary>
        public void Clear()
        {
            vertices.Clear();
            triangles.Clear();
            colors.Clear();
            vertexCache.Clear();
        }
    }
}
#endif