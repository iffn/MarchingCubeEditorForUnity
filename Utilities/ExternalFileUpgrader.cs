# if UNITY_EDITOR
using System.Text.RegularExpressions;
using System;
using System.IO;
using System.Text.RegularExpressions;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using System.Text;

public class ExternalFileUpgrader : EditorWindow
{
    [MenuItem("Tools/MarchingCubeEditor/Utilities/External File     Upgrader")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(ExternalFileUpgrader));
    }

    string globalFilePath = "";

    void OnGUI()
    {
        globalFilePath = EditorGUILayout.TextField("File Path", globalFilePath);

        if (GUILayout.Button("Upgrade to V2"))
        {
            UpgradeFileToV2(globalFilePath);
        }
    }

    void UpgradeFileToV2(string globalFilePath)
    {
        // Read the file content
        string fileContent = File.ReadAllText(globalFilePath);

        // Extract resolution and version using Regex
        int resolutionX = ExtractIntValue(fileContent, @"resolutionX:\s*(\d+)", 1);
        int resolutionY = ExtractIntValue(fileContent, @"resolutionY:\s*(\d+)", 1);
        int resolutionZ = ExtractIntValue(fileContent, @"resolutionZ:\s*(\d+)", 1);
        int version = ExtractIntValue(fileContent, @"version:\s*(\d+)", 0);

        Regex versionPattern = new Regex(@"(version:\s*)(\S+)");

        // Regex to match 'packedData: ' followed by any text
        Regex packedDataPattern = new Regex(@"(packedData:\s*)(\S+)");
        Match match = packedDataPattern.Match(fileContent);

        if (!match.Success)
        {
            Debug.LogWarning("Error: 'packedData:' not found in the file.");
            return;
        }

        string originalPackedData = match.Groups[2].Value;

        string convertedPackedData;

        // Convert packedData (Modify this function as needed)
        if (version == 0)
        {
            convertedPackedData = ConvertV0ToV2(originalPackedData, resolutionX * resolutionY * resolutionZ);
        }
        else
        {
            Debug.LogWarning($"Error: version {version} not handled");
            return;
        }

        // Replace packedData in file content
        StringBuilder updatedContent = new StringBuilder(fileContent);
        updatedContent = new StringBuilder(packedDataPattern.Replace(updatedContent.ToString(), $"$1{convertedPackedData}"));
        updatedContent = new StringBuilder(versionPattern.Replace(updatedContent.ToString(), $"$1{2}")); // Update version to 2

        // Write the modified content back to the file
        File.WriteAllText(globalFilePath, updatedContent.ToString());

        Debug.Log($"Upgrade from version {version} to version 2 complete");
        Debug.Log($"Note: version 2 needs to be added manually");
    }

    static int ExtractIntValue(string content, string pattern, int defaultValue)
    {
        Match match = Regex.Match(content, pattern);
        return match.Success ? int.Parse(match.Groups[1].Value) : defaultValue;
    }

    static string ConvertV0ToV2(string packedData, int voxelCount)
    {
        byte[] byteData = Convert.FromBase64String(packedData);

        byteData = ScriptableObjectSaveData.ConvertV0ToV2(byteData, voxelCount);

        return Convert.ToBase64String(byteData);
    }
}
#endif
