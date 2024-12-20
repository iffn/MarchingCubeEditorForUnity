using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEditor;
using UnityEngine;
using System.IO;

public class DLAHeightmapTool : EditorWindow
{
    // Parameters for the tool
    private int initialWidth = 8;
    private int initialHeight = 8;
    private int scalingIterations = 6;
    private int upscaleFactor = 2;
    private int particlesPerIteration = 50;
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
        initialWidth = EditorGUILayout.IntField("Initial width", initialWidth);
        initialHeight = EditorGUILayout.IntField("Initial height", initialHeight);
        scalingIterations = EditorGUILayout.IntField("Scaling iterations", scalingIterations);
        upscaleFactor = EditorGUILayout.IntField("Upscale Factor", upscaleFactor);
        particlesPerIteration = EditorGUILayout.IntField("Particles per iteration", particlesPerIteration);
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
        float[,] heightmap = DLAHeightmapGenerator.RunGenerator(
            initialWidth, initialHeight, 
            scalingIterations, upscaleFactor, 
            particlesPerIteration, 
            blurKernelSize);

        // Convert the heightmap to a texture
        generatedTexture = ConvertToTexture(heightmap);
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

