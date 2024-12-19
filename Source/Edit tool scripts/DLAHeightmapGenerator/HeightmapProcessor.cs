using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeightmapProcessor
{
    private int[,] originalGrid;

    public HeightmapProcessor(int[,] grid)
    {
        originalGrid = grid;
    }

    public float[,] NormalizeHeights()
    {
        int width = originalGrid.GetLength(0);
        int height = originalGrid.GetLength(1);
        float[,] normalized = new float[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float h = originalGrid[x, y];
                normalized[x, y] = 1 - (1 / (1 + h)); // Apply height formula
            }
        }

        return normalized;
    }

    public float[,] UpscaleHeightmap(float[,] lowResMap, int upscaleFactor)
    {
        int newWidth = lowResMap.GetLength(0) * upscaleFactor;
        int newHeight = lowResMap.GetLength(1) * upscaleFactor;
        float[,] upscaledMap = new float[newWidth, newHeight];

        for (int x = 0; x < newWidth; x++)
        {
            for (int y = 0; y < newHeight; y++)
            {
                // Bilinear interpolation
                float srcX = x / (float)upscaleFactor;
                float srcY = y / (float)upscaleFactor;

                int x0 = Mathf.FloorToInt(srcX);
                int y0 = Mathf.FloorToInt(srcY);
                int x1 = Mathf.Min(x0 + 1, lowResMap.GetLength(0) - 1);
                int y1 = Mathf.Min(y0 + 1, lowResMap.GetLength(1) - 1);

                float dx = srcX - x0;
                float dy = srcY - y0;

                upscaledMap[x, y] = Mathf.Lerp(
                    Mathf.Lerp(lowResMap[x0, y0], lowResMap[x1, y0], dx),
                    Mathf.Lerp(lowResMap[x0, y1], lowResMap[x1, y1], dx),
                    dy
                );
            }
        }

        return upscaledMap;
    }
}

