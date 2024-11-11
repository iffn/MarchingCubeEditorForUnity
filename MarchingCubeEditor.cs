# if UNITY_EDITOR
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class MarchingCubeEditor : EditorWindow
{
    MarchingCubesController linkedMarchingCubesController;
    int gridResolution = 20;

    [MenuItem("Tools/iffnsStuff/MarchingCubeEditor")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(MarchingCubeEditor));
    }

    void OnGUI()
    {
        linkedMarchingCubesController = EditorGUILayout.ObjectField(
           linkedMarchingCubesController,
           typeof(MarchingCubesController),
           true) as MarchingCubesController;

        if (linkedMarchingCubesController == null)
        {
            EditorGUILayout.HelpBox("Add a Marching cube prefab to your scene and link it to this scrip", MessageType.Warning); //ToDo: Check if in scene. ToDo: Auto detect?
            return;
        }

        gridResolution = EditorGUILayout.IntField("Grid Resolution", gridResolution);

        if (GUILayout.Button("Initialize"))
        {
            linkedMarchingCubesController.Initialize(gridResolution, false);
        }

        if (GUILayout.Button("Add sphere"))
        {
            //linkedMarchingCubesController.AddSphere(gridResolution / 2 * Vector3.one, 5);
            linkedMarchingCubesController.AddSphere(5);
        }
    }

}
#endif
