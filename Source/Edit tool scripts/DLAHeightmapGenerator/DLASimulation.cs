using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DLASimulation
{
    private int[,] grid; // Heightmap grid to track particle "stickiness"
    private int width, height; // Grid dimensions

    public DLASimulation(int width, int height)
    {
        this.width = width;
        this.height = height;
        grid = new int[width, height];
    }

    // Run the DLA simulation
    public void RunSimulation(int particleCount, int seedX, int seedY)
    {
        InitializeSeed(seedX, seedY);

        for (int i = 0; i < particleCount; i++)
        {
            SimulateParticle();
        }
    }

    private void InitializeSeed(int x, int y)
    {
        grid[x, y] = 1; // Mark the initial seed point
    }

    private void SimulateParticle()
    {
        // Spawn particle at a random perimeter point
        int x = Random.Range(0, width);
        int y = Random.Range(0, height);

        // Perform random walk until it sticks
        while (true)
        {
            // Random walk (up, down, left, right)
            x += Random.Range(-1, 2);
            y += Random.Range(-1, 2);

            if (IsOutOfBounds(x, y)) return;

            // Check if particle can stick to the structure
            if (CheckNeighborSticking(x, y))
            {
                grid[x, y] += 1; // Increment height
                return;
            }
        }
    }

    private bool CheckNeighborSticking(int x, int y)
    {
        // Safely check if neighboring cells are occupied
        return (IsInBounds(x + 1, y) && grid[x + 1, y] > 0) ||
               (IsInBounds(x - 1, y) && grid[x - 1, y] > 0) ||
               (IsInBounds(x, y + 1) && grid[x, y + 1] > 0) ||
               (IsInBounds(x, y - 1) && grid[x, y - 1] > 0);
    }

    private bool IsInBounds(int x, int y)
    {
        // Check if a coordinate is within the grid bounds
        return x >= 0 && x < width && y >= 0 && y < height;
    }

    private bool IsOutOfBounds(int x, int y)
    {
        return x < 0 || x >= width || y < 0 || y >= height;
    }

    public int[,] GetGrid()
    {
        return grid;
    }
}
