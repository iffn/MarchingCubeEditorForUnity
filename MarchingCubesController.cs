using UnityEngine;

public class MarchingCubesController : MonoBehaviour
{
    private MarchingCubesModel model;
    private MarchingCubesMeshData meshData;
    private MarchingCubesView view;

    public float sphereRadius = 5;

    // Initialize the controller with the resolution and view reference
    public void Initialize(int resolution, float sphereRadius, MarchingCubesView viewComponent)
    {
        view = viewComponent;
        model = new MarchingCubesModel(resolution); // Initialize model with resolution
        model.InitializeSphere(sphereRadius);
        //model.InitializeCube();
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
}
