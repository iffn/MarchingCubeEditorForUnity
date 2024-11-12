using UnityEngine;

namespace iffnsStuff.MarchingCubeEditor.Core
{
    [RequireComponent(typeof(MarchingCubesView))]
    public class MarchingCubesController : MonoBehaviour
    {
        private MarchingCubesModel model;
        private MarchingCubesMeshData meshData;
        private MarchingCubesView view;

        public void Initialize(int resolution, bool setEmpty)
        {
            view = GetComponent<MarchingCubesView>();
            model = new MarchingCubesModel(resolution);

            view.Initialize();

            if (setEmpty) SetEmptyGrid();
        }

        public void GenerateAndDisplayMesh()
        {
            meshData = new MarchingCubesMeshData();
            int resolution = model.Resolution;

            for (int x = 0; x < resolution - 1; x++)
            {
                for (int y = 0; y < resolution - 1; y++)
                {
                    for (int z = 0; z < resolution - 1; z++)
                    {
                        float[] cubeWeights = model.GetCubeWeights(x, y, z);
                        MarchingCubes.GenerateCubeMesh(meshData, cubeWeights, x, y, z);
                    }
                }
            }

            view.UpdateMesh(meshData);
        }

        public void SetEmptyGrid()
        {
            int resolution = model.Resolution;
            for (int x = 0; x < resolution; x++)
            {
                for (int y = 0; y < resolution; y++)
                {
                    for (int z = 0; z < resolution; z++)
                    {
                        model.SetVoxel(x, y, z, -1); // Use signed distance
                    }
                }
            }
            GenerateAndDisplayMesh(); // Update the mesh after modification
        }

        public void AddShape(EditShape shape)
        {
            int resolution = model.Resolution;

            for (int x = 0; x < resolution; x++)
            {
                for (int y = 0; y < resolution; y++)
                {
                    for (int z = 0; z < resolution; z++)
                    {
                        Vector3 point = new(x, y, z);
                        float distance = shape.Distance(point);

                        model.AddVoxel(x, y, z, -distance);
                    }
                }
            }

            GenerateAndDisplayMesh();
        }

        public void SubtractShape(EditShape shape)
        {
            int resolution = model.Resolution;

            for (int x = 0; x < resolution; x++)
            {
                for (int y = 0; y < resolution; y++)
                {
                    for (int z = 0; z < resolution; z++)
                    {
                        Vector3 point = new(x, y, z);
                        float distance = shape.Distance(point);

                        model.SubtractVoxel(x, y, z, distance);
                    }
                }
            }

            GenerateAndDisplayMesh();
        }
    }
}