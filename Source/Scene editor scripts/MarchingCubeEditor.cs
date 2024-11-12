# if UNITY_EDITOR
using iffnsStuff.MarchingCubeEditor.Core;
using iffnsStuff.MarchingCubeEditor.EditTools;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace iffnsStuff.MarchingCubeEditor.SceneEditor
{
    public class MarchingCubeEditor : EditorWindow
    {
        MarchingCubesController linkedMarchingCubesController;
        EditShape selectedShape;
        int gridResolution = 20;

        [MenuItem("Tools/iffnsStuff/MarchingCubeEditor")]
        public static void ShowWindow()
        {
            GetWindow(typeof(MarchingCubeEditor));
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
                linkedMarchingCubesController.Initialize(gridResolution, true);
            }

            selectedShape = EditorGUILayout.ObjectField(
               selectedShape,
               typeof(EditShape),
               true) as EditShape;

            if (selectedShape)
            {
                if (GUILayout.Button($"Add {selectedShape.transform.name}"))
                {
                    linkedMarchingCubesController.AddShape(selectedShape);
                }

                if (GUILayout.Button($"Subtract {selectedShape.transform.name}"))
                {
                    linkedMarchingCubesController.SubtractShape(selectedShape);
                }
            }
        }

    }
}
#endif
