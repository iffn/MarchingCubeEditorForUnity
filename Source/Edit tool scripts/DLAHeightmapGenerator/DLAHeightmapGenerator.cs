using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DLAHeightmapGenerator : MonoBehaviour
{
    public int width = 128, height = 128;
    public int particleCount = 10000;
    public int upscaleFactor = 4;

    void Start()
    {
        DLASimulation dla = new DLASimulation(width, height);
        dla.RunSimulation(particleCount, width / 2, height / 2);

        HeightmapProcessor processor = new HeightmapProcessor(dla.GetGrid());
        float[,] normalizedMap = processor.NormalizeHeights();
        float[,] upscaledMap = processor.UpscaleHeightmap(normalizedMap, upscaleFactor);

        BlurProcessor blur = new BlurProcessor();
        float[,] blurredMap = blur.ApplyGaussianBlur(upscaledMap, 5);

        // Finalize: Use blurredMap to generate Unity terrain or texture
    }
}
