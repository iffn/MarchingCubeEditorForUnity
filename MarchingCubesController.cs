using UnityEngine;

public class MarchingCubesController : MonoBehaviour
{
    private int gridResolution;
    private float sphereRadius;
    private MarchingCubesModel model;
    private MarchingCubesMeshData meshData;
    private MarchingCubesView view;

    public void Initialize(int resolution, float radius, MarchingCubesView viewComponent)
    {
        gridResolution = resolution;
        sphereRadius = radius;
        view = viewComponent;

        // Step 1: Initialize the model
        model = new MarchingCubesModel(gridResolution);
        model.InitializeData(sphereRadius);
    }

    public void GenerateAndDisplayMesh()
    {
        meshData = new MarchingCubesMeshData();

        // Step 2: Generate the mesh
        for (int x = 0; x < gridResolution - 1; x++)
        {
            for (int y = 0; y < gridResolution - 1; y++)
            {
                for (int z = 0; z < gridResolution - 1; z++)
                {
                    float[] cubeWeights = model.GetCubeWeights(x, y, z);
                    MarchingCubes.GenerateCubeMesh(meshData, cubeWeights, x, y, z);
                }
            }
        }

        // Step 3: Update the view with generated mesh data
        view.UpdateMesh(meshData);
    }
}
