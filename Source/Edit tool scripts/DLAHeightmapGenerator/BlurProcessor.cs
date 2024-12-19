using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlurProcessor
{
    public float[,] ApplyGaussianBlur(float[,] heightmap, int kernelSize)
    {
        int width = heightmap.GetLength(0);
        int height = heightmap.GetLength(1);
        float[,] blurredMap = new float[width, height];

        float[,] kernel = GenerateGaussianKernel(kernelSize);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                blurredMap[x, y] = ApplyKernel(heightmap, x, y, kernel);
            }
        }

        return blurredMap;
    }

    private float[,] GenerateGaussianKernel(int size, float sigma = 1.0f)
    {
        float[,] kernel = new float[size, size];
        int center = size / 2;
        float sum = 0f;

        // Generate the Gaussian kernel
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                float dx = x - center;
                float dy = y - center;
                kernel[x, y] = Mathf.Exp(-(dx * dx + dy * dy) / (2 * sigma * sigma));
                sum += kernel[x, y];
            }
        }

        // Normalize the kernel
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                kernel[x, y] /= sum;
            }
        }

        return kernel;
    }


    private float ApplyKernel(float[,] map, int x, int y, float[,] kernel)
    {
        int kernelRadius = kernel.GetLength(0) / 2;
        float sum = 0f;

        for (int i = -kernelRadius; i <= kernelRadius; i++)
        {
            for (int j = -kernelRadius; j <= kernelRadius; j++)
            {
                int xi = Mathf.Clamp(x + i, 0, map.GetLength(0) - 1);
                int yj = Mathf.Clamp(y + j, 0, map.GetLength(1) - 1);

                sum += map[xi, yj] * kernel[i + kernelRadius, j + kernelRadius];
            }
        }

        return sum; // Return the weighted sum (kernel already normalized)
    }

}
