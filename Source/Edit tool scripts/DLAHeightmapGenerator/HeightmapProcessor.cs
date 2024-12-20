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

    public float[,] FloatGrid()
    {
        int width = originalGrid.GetLength(0);
        int height = originalGrid.GetLength(1);
        float[,] floatGrid = new float[width, height];

        int max = 0;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (max < originalGrid[x, y]) max = originalGrid[x, y];
            }
        }

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                floatGrid[x, y] = (float)originalGrid[x, y] / max; // Apply height formula
            }
        }

        return floatGrid;
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

    public (float[,], int[,]) DoubleHeightmapScale(float[,] lowResFloatMap, int[,] lowResIntMap)
    {
        int oldWidth = lowResFloatMap.GetLength(0);
        int oldHeight = lowResFloatMap.GetLength(1);
        int newWidth = oldWidth * 2;
        int newHeight = oldHeight * 2;

        float[,] upscaledFloatMap = new float[newWidth, newHeight];
        int[,] upscaledIntMap = new int[newWidth, newHeight];

        // Step 1: Process the main grid area (excluding boundaries)
        int forBoundX = oldWidth - 1;
        int forBoundY = oldHeight - 1;
        for (int oldX = 0; oldX < forBoundX; oldX++)
        {
            for (int oldY = 0; oldY < oldHeight - 1; oldY++)
            {
                int newX = oldX * 2;
                int newY = oldY * 2;

                // Copy original value
                upscaledFloatMap[newX, newY] = lowResFloatMap[oldX, oldY];
                upscaledIntMap[newX, newY] = lowResIntMap[oldX, oldY];

                // Horizontal interpolation
                upscaledFloatMap[newX + 1, newY] = (lowResFloatMap[oldX, oldY] + lowResFloatMap[oldX + 1, oldY]) * 0.5f;
                upscaledIntMap[newX + 1, newY] = Mathf.RoundToInt((lowResIntMap[oldX, oldY] + lowResIntMap[oldX + 1, oldY]) * 0.5f);

                // Vertical interpolation
                upscaledFloatMap[newX, newY + 1] = (lowResFloatMap[oldX, oldY] + lowResFloatMap[oldX, oldY + 1]) * 0.5f;
                upscaledIntMap[newX, newY + 1] = Mathf.RoundToInt((lowResIntMap[oldX, oldY] + lowResIntMap[oldX, oldY + 1]) * 0.5f);
            }
        }

        // Step 2: Process the right-most column
        for (int oldY = 0; oldY < forBoundX; oldY++)
        {
            int newX = (oldWidth - 1) * 2;
            int newY = oldY * 2;

            upscaledFloatMap[newX, newY] = lowResFloatMap[oldWidth - 1, oldY];
            upscaledIntMap[newX, newY] = lowResIntMap[oldWidth - 1, oldY];

            upscaledFloatMap[newX, newY + 1] = (lowResFloatMap[oldWidth - 1, oldY] + lowResFloatMap[oldWidth - 1, oldY + 1]) * 0.5f;
            upscaledIntMap[newX, newY + 1] = Mathf.RoundToInt((lowResIntMap[oldWidth - 1, oldY] + lowResIntMap[oldWidth - 1, oldY + 1]) * 0.5f);
        }

        // Step 3: Process the bottom-most row
        for (int oldX = 0; oldX < forBoundY; oldX++)
        {
            int newX = oldX * 2;
            int newY = (oldHeight - 1) * 2;

            upscaledFloatMap[newX, newY] = lowResFloatMap[oldX, oldHeight - 1];
            upscaledIntMap[newX, newY] = lowResIntMap[oldX, oldHeight - 1];

            upscaledFloatMap[newX + 1, newY] = (lowResFloatMap[oldX, oldHeight - 1] + lowResFloatMap[oldX + 1, oldHeight - 1]) * 0.5f;
            upscaledIntMap[newX + 1, newY] = Mathf.RoundToInt((lowResIntMap[oldX, oldHeight - 1] + lowResIntMap[oldX + 1, oldHeight - 1]) * 0.5f);
        }

        return (upscaledFloatMap, upscaledIntMap);
    }

    public (float[,], int[,]) UpscaleHeightmap(float[,] lowResFloatMap, int[,] lowResIntMap, int upscaleFactor)
    {
        int oldWidth = lowResFloatMap.GetLength(0);
        int oldHeight = lowResFloatMap.GetLength(1);
        int newWidth = oldWidth * upscaleFactor;
        int newHeight = oldHeight * upscaleFactor;

        float[,] upscaledFloatMap = new float[newWidth, newHeight];
        int[,] upscaledIntMap = new int[newWidth, newHeight];

        // Step 1: Nearest-Neighbor-Like Upscaling for Both Maps
        for (int x = 0; x < oldWidth; x++)
        {
            for (int y = 0; y < oldHeight; y++)
            {
                float floatValue = lowResFloatMap[x, y];
                int intValue = lowResIntMap[x, y];

                // Fill the upscaleFactor x upscaleFactor block for both maps
                for (int i = 0; i < upscaleFactor; i++)
                {
                    for (int j = 0; j < upscaleFactor; j++)
                    {
                        int newX = x * upscaleFactor + i;
                        int newY = y * upscaleFactor + j;

                        upscaledFloatMap[newX, newY] = floatValue;
                        upscaledIntMap[newX, newY] = intValue;
                    }
                }
            }
        }

        // Step 2: Thin the Integer Map to Preserve 1-Pixel-Wide Lines
        int[,] thinnedIntMap = ThinLines(upscaledIntMap);

        // Step 3: Sync the Float Map with the Thinned Integer Map
        float[,] syncedFloatMap = SyncFloatMap(upscaledFloatMap, thinnedIntMap);

        return (syncedFloatMap, thinnedIntMap);
    }

    private int[,] ThinLines(int[,] grid)
    {
        int width = grid.GetLength(0);
        int height = grid.GetLength(1);
        int[,] thinnedMap = (int[,])grid.Clone();

        for (int x = 1; x < width - 1; x++)
        {
            for (int y = 1; y < height - 1; y++)
            {
                if (grid[x, y] > 0) // Non-zero pixel
                {
                    // Check if the pixel has too many neighbors (redundant)
                    int neighborCount = CountNeighbors(grid, x, y);
                    if (neighborCount > 2)
                    {
                        thinnedMap[x, y] = 0; // Remove redundant pixel
                    }
                }
            }
        }

        return thinnedMap;
    }

    private int CountNeighbors(int[,] grid, int x, int y)
    {
        int count = 0;
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if (i == 0 && j == 0) continue; // Skip self
                if (grid[x + i, y + j] > 0) count++;
            }
        }
        return count;
    }

    private float[,] SyncFloatMap(float[,] floatMap, int[,] intMap)
    {
        int width = floatMap.GetLength(0);
        int height = floatMap.GetLength(1);
        float[,] syncedFloatMap = new float[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // Set float value to zero where int map was thinned
                syncedFloatMap[x, y] = (intMap[x, y] > 0) ? floatMap[x, y] : 0f;
            }
        }

        return syncedFloatMap;
    }



}

