using UnityEngine;

public class MarchingCubesInitializer : MonoBehaviour
{
    [SerializeField] private GameObject marchingCubesPrefab;
    [SerializeField] private int gridResolution = 10;
    [SerializeField] private float sphereRadius = 5f;

    private void Start()
    {
        // Step 1: Create or find instances of model, controller, and view
        var marchingCubesObj = Instantiate(marchingCubesPrefab);

        // Ensure marchingCubesPrefab has a MarchingCubesController and MarchingCubesView attached
        MarchingCubesController controller = marchingCubesObj.GetComponent<MarchingCubesController>();
        MarchingCubesView view = marchingCubesObj.GetComponent<MarchingCubesView>();

        if (controller == null || view == null)
        {
            Debug.LogError("MarchingCubesPrefab must have a MarchingCubesController and MarchingCubesView attached.");
            return;
        }

        // Step 2: Initialize the controller with parameters
        controller.Initialize(gridResolution, sphereRadius, view);

        // Step 3: Trigger mesh generation
        controller.GenerateAndDisplayMesh();
    }
}
