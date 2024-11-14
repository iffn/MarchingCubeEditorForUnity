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
        bool addingShape = false;

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

            if (!linkedMarchingCubesController.IsInitialized) return;

            selectedShape = EditorGUILayout.ObjectField(
               selectedShape,
               typeof(EditShape),
               true) as EditShape;

            if (selectedShape)
            {
                if (GUILayout.Button($"Add {selectedShape.transform.name}"))
                {
                    linkedMarchingCubesController.AddShape(selectedShape, false);
                }

                if (GUILayout.Button($"Subtract {selectedShape.transform.name}"))
                {
                    linkedMarchingCubesController.SubtractShape(selectedShape, false);
                }

                bool newAddingShape = EditorGUILayout.Toggle("Add Shape Mode", addingShape);

                if (newAddingShape && !addingShape)
                {
                    linkedMarchingCubesController.GenerateAndDisplayMesh(true);
                }

                addingShape = newAddingShape;
            }

            if (addingShape)
            {
                SceneView.duringSceneGui += OnSceneGUI; // Subscribe to SceneView GUI event
            }
            else
            {
                SceneView.duringSceneGui -= OnSceneGUI; // Unsubscribe when not in use
            }
        }

        private void OnSceneGUI(SceneView sceneView)
        {
            Event e = Event.current;
            if (e.type == EventType.MouseDown && e.button == 0 && addingShape && selectedShape) // Left-click event
            {
                Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
                
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    // Convert world hit point to grid or object coordinates, then add shape
                    Vector3 placementPosition = hit.point;
                    Vector3 prevPosition = selectedShape.transform.position;
                    selectedShape.transform.position = hit.point;
                    linkedMarchingCubesController.AddShape(selectedShape, true);
                    selectedShape.transform.position = prevPosition;

                    e.Use(); // Mark event as used
                }
            }
        }
    }
}
#endif
