using iffnsStuff.MarchingCubeEditor.EditTools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeightmapShape : EditShape
{
    private float[,] heightmapData; // 2D array to store heightmap values
    private int heightmapWidth;
    private int heightmapHeight;
    float heightScaler = 1;
    [SerializeField] private Texture2D heightmapTexture; // ToDo: Get from display material

    public override void PrepareParameters(Transform gridTransform)
    {
        base.PrepareParameters(gridTransform);

        heightScaler = transform.localScale.y;

        if (heightmapTexture != null)
            LoadHeightmapData();
    }

    /// <summary>
    /// Load heightmap data from the provided texture.
    /// </summary>
    private void LoadHeightmapData()
    {
        heightmapWidth = heightmapTexture.width;
        heightmapHeight = heightmapTexture.height;

        heightmapData = new float[heightmapWidth, heightmapHeight];

        float max = -1000;
        float min = 1000;

        for (int x = 0; x < heightmapWidth; x++)
        {
            for (int y = 0; y < heightmapHeight; y++)
            {
                // Read pixel grayscale value and store as height
                heightmapData[x, y] = heightmapTexture.GetPixel(x, y).r;

                if(max < heightmapData[x, y]) max = heightmapData[x, y];
                if(min > heightmapData[x, y]) min = heightmapData[x, y];
            }
        }

        Debug.Log($"Height in the middle = {heightmapData[heightmapWidth / 2, heightmapHeight / 2]}");
        Debug.Log($"Height at [10, 10] = {heightmapData[10, 10]}");
        Debug.Log($"Range = {min}...{max}");
    }

    /// <summary>
    /// Override the distance function for the terrain heightmap.
    /// </summary>
    protected override float DistanceOutsideIsPositive(Vector3 localPoint)
    {
        if (heightmapData == null)
            return -1; // Return large value if heightmap is not loaded

        // Convert localPoint.x and localPoint.z to heightmap coordinates
        int xIndex = Mathf.Clamp(Mathf.FloorToInt((localPoint.x + 0.5f) * heightmapWidth), 0, heightmapWidth - 1);
        int zIndex = Mathf.Clamp(Mathf.FloorToInt((localPoint.z + 0.5f) * heightmapHeight), 0, heightmapHeight - 1);

        // Get the height at this (x, z) position
        float heightAtPoint = heightmapData[xIndex, zIndex];

        // Compare the heightmap's height to the localPoint's y position
        float distance = localPoint.y - heightAtPoint;

        return distance * heightScaler; // Positive if above the terrain, negative if below
    }

    /// <summary>
    /// Return the bounding box based on heightmap dimensions.
    /// </summary>
    public override (Vector3 minOffset, Vector3 maxOffset) GetLocalBoundingBox()
    {
        // Assuming the heightmap covers a unit box (scale: -0.5 to +0.5 in x and z)
        return (-0.5f * Vector3.one, new Vector3(0.5f, 1.0f, 0.5f));
    }
}
