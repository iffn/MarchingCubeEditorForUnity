using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class DLAHeightmapGenerator
{
    public static float[,] RunGenerator(int width, int height, int scalingIterations, int upscaleFactor, int particlesPerIteration, int blurKernelSize)
    {
        int[,] intMap;
        float[,] floatMap;
        DLASimulation dla;
        HeightmapProcessor processor;
        BlurProcessor blur;

        // First pass:
        dla = new DLASimulation(width, height);
        dla.RunSimulation(particlesPerIteration, width / 2, height / 2);
        intMap = dla.GetGrid();

        processor = new HeightmapProcessor(intMap);
        floatMap = processor.NormalizeHeights();

        (floatMap, intMap) = processor.DoubleHeightmapScale(floatMap, intMap);

        blur = new BlurProcessor();
        floatMap = blur.ApplyGaussianBlur(floatMap, blurKernelSize);

        //Following iterations
        for (int i = 0; i < scalingIterations; i++)
        {
            dla = new DLASimulation(intMap);
            dla.RunSimulation(particlesPerIteration, width / 2, height / 2);
            intMap = dla.GetGrid();

            // Process the heightmap
            processor = new HeightmapProcessor(intMap);
            floatMap = processor.NormalizeHeights();

            (floatMap, intMap) = processor.DoubleHeightmapScale(floatMap, intMap);

            blur = new BlurProcessor();
            floatMap = blur.ApplyGaussianBlur(floatMap, blurKernelSize);
        }

        return floatMap;
        return processor.FloatGrid();
    }
}

