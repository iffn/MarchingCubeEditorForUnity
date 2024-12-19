using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEditor;
using UnityEngine;
using System.IO;

public class DLAHeightmapTool : EditorWindow
{
    // Parameters for the tool
    private int width = 128;
    private int height = 128;
    private int particleCount = 10000;
    private int upscaleFactor = 4;
    private int blurKernelSize = 5;

    private Texture2D generatedTexture;

    [MenuItem("Tools/DLA Heightmap Generator")]
    public static void ShowWindow()
    {
        GetWindow<DLAHeightmapTool>("DLA Heightmap Generator");
    }

    void OnGUI()
    {
        GUILayout.Label("DLA Heightmap Generator", EditorStyles.boldLabel);

        // Input fields
        width = EditorGUILayout.IntField("Width", width);
        height = EditorGUILayout.IntField("Height", height);
        particleCount = EditorGUILayout.IntField("Particle Count", particleCount);
        upscaleFactor = EditorGUILayout.IntField("Upscale Factor", upscaleFactor);
        blurKernelSize = EditorGUILayout.IntField("Blur Kernel Size", blurKernelSize);

        // Generate button
        if (GUILayout.Button("Generate Heightmap"))
        {
            GenerateHeightmap();
        }

        // Display the generated texture
        if (generatedTexture != null)
        {
            GUILayout.Label("Generated Texture Preview:");
            GUILayout.Label(generatedTexture, GUILayout.Width(256), GUILayout.Height(256));

            // Save button
            if (GUILayout.Button("Save as PNG"))
            {
                SaveTextureAsPNG(generatedTexture, "Assets/GeneratedHeightmap.png");
                Debug.Log("Texture saved to Assets/GeneratedHeightmap.png");
            }
        }
    }

    void GenerateHeightmap()
    {
        // Run the DLA simulation
        DLASimulation dla = new DLASimulation(width, height);
        dla.RunSimulation(particleCount, width / 2, height / 2);

        // Process the heightmap
        HeightmapProcessor processor = new HeightmapProcessor(dla.GetGrid());
        float[,] normalizedMap = processor.NormalizeHeights();

        float[,] upscaledMap = processor.UpscaleHeightmap(normalizedMap, upscaleFactor);

        BlurProcessor blur = new BlurProcessor();
        float[,] blurredMap = blur.ApplyGaussianBlur(upscaledMap, blurKernelSize);

        float max = 0;
        for(int x = 0; x < width; x++)
        {
            for(int y = 0; y < height; y++)
            {
                if(max < blurredMap[x,y]) max = blurredMap[x,y];
            }
        }

        Debug.Log(max);

        // Convert the heightmap to a texture
        generatedTexture = ConvertToTexture(blurredMap);
    }

    Texture2D ConvertToTexture(float[,] heightmap)
    {
        int width = heightmap.GetLength(0);
        int height = heightmap.GetLength(1);

        Texture2D texture = new Texture2D(width, height);
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float value = heightmap[x, y];
                Color color = new Color(value, value, value, 1f); // Grayscale
                texture.SetPixel(x, y, color);
            }
        }
        texture.Apply();
        return texture;
    }

    void SaveTextureAsPNG(Texture2D texture, string path)
    {
        byte[] bytes = texture.EncodeToPNG();
        File.WriteAllBytes(path, bytes);
        AssetDatabase.Refresh(); // Refresh Unity Asset Database
    }
}

