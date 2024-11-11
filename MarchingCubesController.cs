using UnityEngine;

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

        if(setEmpty) SetEmptyGrid();
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

    // Add a sphere by setting voxel values based on distance to a center point
    public void AddSphere(float radius)
    {
        int resolution = model.Resolution;
        Vector3 gridCenter = new(resolution / 2f, resolution / 2f, resolution / 2f);

        for (int x = 0; x < resolution; x++)
        {
            for (int y = 0; y < resolution; y++)
            {
                for (int z = 0; z < resolution; z++)
                {
                    Vector3 point = new Vector3(x, y, z);
                    float distanceToCenter = Vector3.Distance(point, gridCenter);

                    float newValue = distanceToCenter - radius;

                    if (newValue > -0.999)
                    {
                        model.SetVoxel(x, y, z, newValue);
                    }

                }
            }
        }
        GenerateAndDisplayMesh(); // Update the mesh after modification
    }

    // Add a cube by setting voxel values within a box defined by a center point and half-size
    public void AddCube(Vector3 center, float halfSize)
    {
        int resolution = model.Resolution;
        for (int x = 0; x < resolution; x++)
        {
            for (int y = 0; y < resolution; y++)
            {
                for (int z = 0; z < resolution; z++)
                {
                    Vector3 point = new Vector3(x, y, z);

                    // Check if the point lies within the cube bounds
                    if (Mathf.Abs(point.x - center.x) <= halfSize &&
                        Mathf.Abs(point.y - center.y) <= halfSize &&
                        Mathf.Abs(point.z - center.z) <= halfSize)
                    {
                        model.SetVoxel(x, y, z, 1f); // 1f indicates solid in the cube
                    }
                }
            }
        }
        GenerateAndDisplayMesh(); // Update the mesh after modification
    }
}
