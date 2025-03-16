#if UNITY_EDITOR

using iffnsStuff.MarchingCubeEditor.Core;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class ExporterTool : BaseTool
{
    public override string DisplayName => "Exporter";

    public override void DrawUI()
    {
        if (GUILayout.Button($"Export as Obj"))
            ExportAsObj();
    }

    void ExportAsObj()
    {
        // Setup
        List<MarchingCubesView> views = LinkedMarchingCubeController.ChunkViews;

        StringBuilder objStringBuilder = new StringBuilder();

        // Combiue obj stsring
        int vertexOffset = 0;
        for(int i = 0; i < views.Count; i++)
        {
            MarchingCubesView view = views[i];

            if(EmptyMesh(view.SharedMesh)) continue;

            objStringBuilder.AppendLine($"o Chunk{i}");

            objStringBuilder.Append(GetObjString(view.SharedMesh, view.transform.localPosition, vertexOffset));
            objStringBuilder.AppendLine($"");

            vertexOffset += view.SharedMesh.vertexCount;
        }

        // Save to asset path
        string unixTimestampSeconds = ((int)System.DateTimeOffset.UtcNow.ToUnixTimeSeconds()).ToString();

        SaveToAssetPath($"{LinkedMarchingCubeController.transform.name}-{unixTimestampSeconds}.obj", objStringBuilder.ToString());
    }

    public static void SaveToAssetPath(string nameWithFileEnding, string content)
    {
        string path = Path.Combine(Application.dataPath, nameWithFileEnding);

        try
        {
            File.WriteAllText(path, content);
            Debug.Log($"File successfully saved to: {path}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to save file to: {path}. Error: {e.Message}");
        }
    }

    public static bool EmptyMesh(Mesh mesh)
    {
        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;

        return vertices.Length == 0 || triangles.Length == 0;
    }

    public static string GetObjString(Mesh mesh, Vector3 positionOffset , int indicesOffset)
    {
        StringBuilder objStringBuilder = new StringBuilder();

        // Add vertices
        foreach (Vector3 vertex in mesh.vertices)
        {
            Vector3 outputPosition = vertex + positionOffset;

            objStringBuilder.AppendLine($"v {outputPosition.x} {outputPosition.y} {outputPosition.z}");
        }

        /*
        // Add normals
        foreach (Vector3 normal in mesh.normals)
        {
            objStringBuilder.AppendLine($"vn {normal.x} {normal.y} {normal.z}");
        }

        // Add UVs
        foreach (Vector2 uv in mesh.uv)
        {
            objStringBuilder.AppendLine($"vt {uv.x} {uv.y}");
        }
        */

        // Add faces
        int[] triangles = mesh.triangles;

        for (int i = 0; i < mesh.triangles.Length; i += 3)
        {
            int v1 = triangles[i] + 1 + indicesOffset;
            int v2 = triangles[i + 1] + 1 + indicesOffset;
            int v3 = triangles[i + 2] + 1 + indicesOffset;

            objStringBuilder.AppendLine($"f {v1}/{v1}/{v1} {v2}/{v2}/{v2} {v3}/{v3}/{v3}");
        }

        return objStringBuilder.ToString();
    }
}

#endif