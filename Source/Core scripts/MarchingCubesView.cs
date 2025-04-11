//#define singleViewPerformanceOutput

#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEngine;
using static iffnsStuff.MarchingCubeEditor.Core.MarchingCubesController;

namespace iffnsStuff.MarchingCubeEditor.Core
{
    public class MarchingCubesView : MonoBehaviour
    {
        static readonly System.Diagnostics.Stopwatch PostProcessingStopwatch = new System.Diagnostics.Stopwatch();
        public static int ModifiedElements { get; private set; }
        public static int RemovedVertices { get; private set; }

        Vector3Int gridBoundsMin;
        Vector3Int gridBoundsMax;

        private bool isDirty; // Whether this chunk's mesh needs updating
        private bool invertedNormals;

        Mesh mesh;

        public static void ResetPostProcessingDiagnostics()
        {
            PostProcessingStopwatch.Reset();
            ModifiedElements = 0;
            RemovedVertices = 0;
        }

        public static double ElapsedPostProcessingTimeSeconds => PostProcessingStopwatch.Elapsed.TotalSeconds;

        [SerializeField] MeshFilter linkedMeshFilter;
        [SerializeField] MeshCollider linkedMeshCollider;
        [SerializeField] MeshRenderer mainMeshRenderer;
        [SerializeField] MeshFilter grassMeshFilter;
        [SerializeField] MeshRenderer grassMeshRenderer;

        public bool SetupIsCorrect(MarchingCubesView prefabReference)
        {
            // Assignments
            if(linkedMeshFilter == null) return false;
            if(linkedMeshCollider == null) return false;
            if(mainMeshRenderer == null) return false;
            if(grassMeshFilter == null) return false;
            if(grassMeshRenderer == null) return false;

            // GetComponents
            // All done via SerializeField so far

            // Comparison with prefeabReference
            if(transform.childCount != prefabReference.transform.childCount) return false;

            // Return true if no problem found
            return true;
        }

        public Material CurrentMainMaterial
        {
            get
            {
                if (mainMeshRenderer == null)
                    return null;
                else
                    return mainMeshRenderer.sharedMaterial;
            }
            set
            {
                if (mainMeshRenderer == null)
                    return;
                else
                    mainMeshRenderer.sharedMaterial = value;
            }
        }

        public Material CurrentGrassMaterial
        {
            get
            {
                if (grassMeshRenderer == null)
                    return null;
                else
                    return grassMeshRenderer.sharedMaterial;
            }
            set
            {
                if (grassMeshRenderer == null)
                    return;
                else
                    grassMeshRenderer.sharedMaterial = value;
            }
        }

        public Vector3Int GridBoundsMin => gridBoundsMin;
        public Vector3Int GridBoundsMax => gridBoundsMax;

        public Mesh SharedMesh => linkedMeshFilter.sharedMesh;

        public bool ColliderEnabled
        {
            get
            {
                return linkedMeshCollider.enabled;
            }
            set
            {
                if (value && (!ColliderEnabled || linkedMeshCollider.sharedMesh != null))
                    UpdateCollider();

                linkedMeshCollider.enabled = value;
            }
        }

        // Public functions
        public void Initialize(Vector3Int gridBoundsMin, Vector3Int gridBoundsMax, bool colliderEnabled, Material mainMaterial, Material grassMaterial)
        {
            Initialize(gridBoundsMin, gridBoundsMax, colliderEnabled);

            if (mainMaterial != null)
                mainMeshRenderer.sharedMaterial = mainMaterial;

            if (grassMaterial != null)
                grassMeshRenderer.sharedMaterial = grassMaterial;
        }

        public void Initialize(Vector3Int gridBoundsMin, Vector3Int gridBoundsMax, bool colliderEnabled)
        {
            this.gridBoundsMin = gridBoundsMin;
            this.gridBoundsMax = gridBoundsMax;


            transform.localPosition = new Vector3(gridBoundsMin.x, gridBoundsMin.y, gridBoundsMin.z);

            if (linkedMeshFilter.sharedMesh == null)
            {
                linkedMeshFilter.mesh = new Mesh();
            }
            else
            {
                linkedMeshFilter.sharedMesh.Clear(); // Clear existing mesh data for reuse
            }

            mesh = linkedMeshFilter.sharedMesh;

            grassMeshFilter.sharedMesh = mesh;

            linkedMeshCollider.enabled = colliderEnabled;

            if(mesh.vertexCount > 0)
                linkedMeshCollider.sharedMesh = mesh;

            isDirty = true; // Mark the chunk as dirty upon initialization
        }

        public bool IsWithinBounds(Vector3Int min, Vector3Int max)
        {
            // Check for overlap between the chunk and the affected region
            return !(gridBoundsMax.x <= min.x || gridBoundsMin.x >= max.x ||
                     gridBoundsMax.y <= min.y || gridBoundsMin.y >= max.y ||
                     gridBoundsMax.z <= min.z || gridBoundsMin.z >= max.z);
        }

        public bool IsWithinBounds(Vector3Int point)
        {
            // Check for overlap between the chunk and the affected region
            return !(gridBoundsMax.x <= point.x || gridBoundsMin.x >= point.x ||
                     gridBoundsMax.y <= point.y || gridBoundsMin.y >= point.y ||
                     gridBoundsMax.z <= point.z || gridBoundsMin.z >= point.z);
        }

        public void PostProcessMesh(PostProcessingOptions currentPostProcessingOptions)
        {
            PostProcessingStopwatch.Start();

            if (currentPostProcessingOptions.mergeTriangles)
            {
                MeshUtilityFunctions.RemoveDegenerateTriangles(
                    mesh,
                    PostProcessingStopwatch, currentPostProcessingOptions.maxProcessingTimeSeconds,
                    out int removedVertices, out int modifiedElements,
                    currentPostProcessingOptions.angleThresholdDeg, currentPostProcessingOptions.areaThreshold);

                ModifiedElements += modifiedElements;
                RemovedVertices += removedVertices;
            }

            FinishMesh();

            if (currentPostProcessingOptions.smoothNormals)
            {
                mesh.RecalculateNormals();
                SmoothNormalsWithDistanceBias(mesh, currentPostProcessingOptions.smoothNormalsDistanceFactorBias, currentPostProcessingOptions);

                mesh.RecalculateTangents();
                //meshFilter.sharedMesh.RecalculateBounds(); // Not needed in this case since recalculated automatically when setting the triangles: https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Mesh.RecalculateBounds.html
                if (ColliderEnabled)
                    UpdateCollider();
            }

            PostProcessingStopwatch.Stop();
        }

        public void UpdateBounds(Vector3Int min, Vector3Int max)
        {
            gridBoundsMin = min;
            gridBoundsMax = max;

            transform.localPosition = new Vector3(min.x, min.y, min.z);
        }

        public void MarkDirty()
        {
            isDirty = true;
        }

        public void UpdateMeshIfDirty(MarchingCubesModel model, bool parallelCall)
        {
            if (!isDirty) return;

#if singleViewPerformanceOutput
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
#endif

            // Generate mesh data for this chunk
            cachedMeshData = GenerateChunkMesh(model);

#if singleViewPerformanceOutput
            Debug.Log($"GenerateChunkMesh: {sw.Elapsed.TotalMilliseconds}ms");
            sw.Restart();
#endif

            // Update the view's mesh
            if (!parallelCall)
                ApplyNonParallelMeshDataIfDirty();

#if singleViewPerformanceOutput
            Debug.Log($"UpdateMesh: {sw.Elapsed.TotalMilliseconds}ms");
            sw.Restart();
#endif
        }

        MarchingCubesMeshData cachedMeshData;

        public void ApplyNonParallelMeshDataIfDirty()
        {
            if (!isDirty) return;

            mesh.Clear();

            mesh.indexFormat = (cachedMeshData.vertices.Count > 65535) ? UnityEngine.Rendering.IndexFormat.UInt32 : UnityEngine.Rendering.IndexFormat.UInt16;

            mesh.SetVertices(cachedMeshData.vertices);
            mesh.SetTriangles(cachedMeshData.triangles, 0);
            mesh.SetColors(cachedMeshData.colors);

            FinishMesh();

            isDirty = false; // Mark as clean
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

            // Rebuild the triangle array and filter degenerate triangles
            List<int> filteredTriangles = new List<int>();
            for (int i = 0; i < originalTriangles.Length; i += 3)
            {
                int v1 = vertexMapping[originalTriangles[i]];
                int v2 = vertexMapping[originalTriangles[i + 1]];
                int v3 = vertexMapping[originalTriangles[i + 2]];

                // Add the triangle only if it is non-degenerate
                if (v1 != v2 && v2 != v3 && v3 != v1)
                {
                    filteredTriangles.Add(v1);
                    filteredTriangles.Add(v2);
                    filteredTriangles.Add(v3);
                }
            }

            // Update the mesh
            mesh.Clear();
            mesh.vertices = newVertices.ToArray();
            mesh.triangles = filteredTriangles.ToArray(); // Only valid triangles remain
            mesh.colors = newColors.ToArray();
        }

        // Unity functions
        void OnDestroy()
        {
            // Safely destroy the dynamically created mesh
            if (linkedMeshFilter != null && mesh != null)
            {
                Destroy(mesh);
            }

            // Optionally clear the collider's mesh reference
            if (linkedMeshCollider != null)
            {
                linkedMeshCollider.sharedMesh = null;
            }
        }

        // Internal functions
        MarchingCubesMeshData GenerateChunkMesh(MarchingCubesModel model)
        {
            MarchingCubesMeshData meshData = new MarchingCubesMeshData();

            int sizeX = gridBoundsMax.x - gridBoundsMin.x;
            int sizeY = gridBoundsMax.y - gridBoundsMin.y;
            int sizeZ = gridBoundsMax.z - gridBoundsMin.z;
            
            VoxelData[] tempWeights = new VoxelData[8];

            for (int x = 0; x < sizeX; x++)
            {
                for (int y = 0; y < sizeY; y++)
                {
                    for (int z = 0; z < sizeZ; z++)
                    {
                        model.GetCubeWeights(x + gridBoundsMin.x, y + gridBoundsMin.y, z + gridBoundsMin.z, tempWeights);

                        MarchingCubes.GenerateCubeMesh(meshData, tempWeights, x, y, z, invertedNormals);
                    }
                }
            }

            return meshData;
        }

        void UpdateCollider()
        {
            linkedMeshCollider.sharedMesh = null;

            if (mesh.vertexCount < 3 || mesh.triangles.Length < 3) // Prevent invalid mesh assignment
                return;

            linkedMeshCollider.sharedMesh = mesh;
        }

        void InvertMeshTriangles()
        {
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

            FinishMesh();
        }

        void FinishMesh()
        {
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            //meshFilter.sharedMesh.RecalculateBounds(); // Not needed in this case since recalculated automatically when setting the triangles: https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Mesh.RecalculateBounds.html
            if (ColliderEnabled)
                UpdateCollider();
        }

        void SmoothNormalsWithDistanceBias(Mesh mesh, float distanceBiasFactor, PostProcessingOptions currentPostProcessingOptions)
        {
            if (PostProcessingStopwatch.Elapsed.TotalSeconds > currentPostProcessingOptions.maxProcessingTimeSeconds)
            {
                Debug.LogWarning("Did not start normal smoothing because time already ran out.");
                return;
            }

            // Step 1: Recalculate initial normals
            mesh.RecalculateNormals();
            Vector3[] vertices = mesh.vertices;
            Vector3[] normals = mesh.normals;
            int[] triangles = mesh.triangles;

            // Step 2: Build adjacency information
            List<int>[] vertexToNeighbors = new List<int>[vertices.Length];
            for (int i = 0; i < vertices.Length; i++)
            {
                vertexToNeighbors[i] = new List<int>();
            }

            for (int i = 0; i < triangles.Length; i += 3)
            {
                int v0 = triangles[i];
                int v1 = triangles[i + 1];
                int v2 = triangles[i + 2];

                // Add neighbors for each vertex of the triangle
                AddNeighbor(vertexToNeighbors, v0, v1);
                AddNeighbor(vertexToNeighbors, v1, v2);
                AddNeighbor(vertexToNeighbors, v2, v0);
            }

            // Step 3: Smooth normals using distance bias
            Vector3[] smoothedNormals = new Vector3[vertices.Length];
            for (int i = 0; i < vertices.Length; i++)
            {
                Vector3 smoothedNormal = Vector3.zero;
                float totalWeight = 0;

                foreach (int neighborIndex in vertexToNeighbors[i])
                {
                    float distance = Vector3.Distance(vertices[i], vertices[neighborIndex]);
                    float weight = 1.0f / Mathf.Pow(distance + 0.0001f, distanceBiasFactor); // Avoid division by zero
                    smoothedNormal += normals[neighborIndex] * weight;
                    totalWeight += weight;
                }

                smoothedNormal /= totalWeight; // Normalize by total weight
                smoothedNormals[i] = smoothedNormal.normalized;

                if (PostProcessingStopwatch.Elapsed.TotalSeconds > currentPostProcessingOptions.maxProcessingTimeSeconds)
                {
                    Debug.LogWarning("Interrupted normal smoothing because time ran out. Continuation not yet implemented for this.");
                    break;
                }
            }

            // Step 4: Update mesh normals
            mesh.normals = smoothedNormals;
        }

        // Helper function to add neighbors
        void AddNeighbor(List<int>[] adjacencyList, int v1, int v2)
        {
            if (!adjacencyList[v1].Contains(v2))
            {
                adjacencyList[v1].Add(v2);
            }
            if (!adjacencyList[v2].Contains(v1))
            {
                adjacencyList[v2].Add(v1);
            }
        }
    }
}

#endif