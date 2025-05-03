#if UNITY_EDITOR
using UnityEngine;

namespace iffnsStuff.MarchingCubeEditor.Core
{
    public static class MarchingCubes
    {
        // Constants
        // Source: https://paulbourke.net/geometry/polygonise/
        
        public static void GenerateCubeMesh(MarchingCubesMeshData meshData, VoxelData[] cubeData, int x, int y, int z, bool invertNormals = false)
        {
            int cubeIndex = 0;

            // Determine cube configuration based on corner weights
            for (int i = 0; i < 8; i++)
            {
                if (cubeData[i].WeightInsideIsPositive > 0)
                    cubeIndex |= 1 << i;
            }

            // Skip if the cube is entirely inside or outside the surface
            if (MarchingCubesTables.edgeTable[cubeIndex] == 0)
                return;

            // Dictionary to cache edge vertices by edge index to avoid duplicates
            int[] edgeVertexIndices = new int[12];

            // Interpolate vertices on edges where there's an intersection
            Vector3[] cornerPositions = GetCornerPositions(x, y, z);

            for (int i = 0; i < 12; i++)
            {
                if ((MarchingCubesTables.edgeTable[cubeIndex] & 1 << i) != 0)
                {
                    int cornerA = CornerFromEdge(i, 0);
                    int cornerB = CornerFromEdge(i, 1);
                    (Vector3 edgeVertex, Color32 color) = InterpolateEdge(cornerPositions[cornerA], cornerPositions[cornerB], cubeData[cornerA], cubeData[cornerB]);

                    // Store the vertex index in the edge vertex index array
                    edgeVertexIndices[i] = meshData.AddVertex(edgeVertex, color);
                }
            }

            // Use triangle table to add triangles using the calculated vertices
            if (invertNormals)
            {
                for (int i = 0; MarchingCubesTables.triTable[cubeIndex, i] != -1; i += 3)
                {
                    int a0 = MarchingCubesTables.triTable[cubeIndex, i];
                    int a1 = MarchingCubesTables.triTable[cubeIndex, i + 1];
                    int a2 = MarchingCubesTables.triTable[cubeIndex, i + 2];

                    // Add triangle by referencing vertex indices in meshData
                    meshData.AddTriangle(edgeVertexIndices[a0], edgeVertexIndices[a1], edgeVertexIndices[a2]);
                }
            }
            else
            {
                for (int i = 0; MarchingCubesTables.triTable[cubeIndex, i] != -1; i += 3)
                {
                    int a1 = MarchingCubesTables.triTable[cubeIndex, i];
                    int a0 = MarchingCubesTables.triTable[cubeIndex, i + 1];
                    int a2 = MarchingCubesTables.triTable[cubeIndex, i + 2];

                    // Add triangle by referencing vertex indices in meshData
                    meshData.AddTriangle(edgeVertexIndices[a0], edgeVertexIndices[a1], edgeVertexIndices[a2]);
                }
            }
        }

        private static Vector3[] GetCornerPositions(int x, int y, int z)
        {
            return new Vector3[]
            {
                new Vector3(x, y, z),
                new Vector3(x + 1, y, z),
                new Vector3(x + 1, y + 1, z),
                new Vector3(x, y + 1, z),
                new Vector3(x, y, z + 1),
                new Vector3(x + 1, y, z + 1),
                new Vector3(x + 1, y + 1, z + 1),
                new Vector3(x, y + 1, z + 1)
            };
        }

        private static (Vector3, Color32) InterpolateEdge(Vector3 p1, Vector3 p2, VoxelData val1, VoxelData val2)
        {
            float t = (0 - val1.WeightInsideIsPositive) / (val2.WeightInsideIsPositive - val1.WeightInsideIsPositive);  // t is the interpolation factor
            return (p1 + t * (p2 - p1), Color32.Lerp(val1.Color, val2.Color, t));
        }

        private static int CornerFromEdge(int edgeIndex, int cornerPosition)
        {
            // Each edge connects two corners; map the edge index to corner indices
            int[,] edgeToCorners = new int[,]
            {
                { 0, 1 }, { 1, 2 }, { 2, 3 }, { 3, 0 },
                { 4, 5 }, { 5, 6 }, { 6, 7 }, { 7, 4 },
                { 0, 4 }, { 1, 5 }, { 2, 6 }, { 3, 7 }
            };

            return edgeToCorners[edgeIndex, cornerPosition];
        }
    }
}
#endif