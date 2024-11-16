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
        ScriptableObjectSaveData linkedScriptableObjectSaveData;
        EditShape selectedShape;
        int gridResolutionX = 20;
        int gridResolutionY = 20;
        int gridResolutionZ = 20;
        bool addingShape = false;
        bool limitMaxHeight;
        bool invertNormals;
        Vector3 originalShapePosition;

        Color additionColor = new Color(1f, 0.5f, 0f, 0.5f);
        Color subtractionColor = new Color(1f, 0f, 0f, 0.5f);

        [MenuItem("Tools/iffnsStuff/MarchingCubeEditor")]
        public static void ShowWindow()
        {
            GetWindow(typeof(MarchingCubeEditor));
        }

        void OnGUI()
        {
            GUILayout.Label("Scene component:");

            linkedMarchingCubesController = EditorGUILayout.ObjectField(
               linkedMarchingCubesController,
               typeof(MarchingCubesController),
               true) as MarchingCubesController;

            if (linkedMarchingCubesController == null)
            {
                EditorGUILayout.HelpBox("Add a Marching cube prefab to your scene and link it to this scrip", MessageType.Warning); //ToDo: Check if in scene. ToDo: Auto detect?
                return;
            }

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("X");
            GUILayout.Label("Y");
            GUILayout.Label("Z");
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            gridResolutionX = EditorGUILayout.IntField(gridResolutionX);
            gridResolutionY = EditorGUILayout.IntField(gridResolutionY);
            gridResolutionZ = EditorGUILayout.IntField(gridResolutionZ);
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Initialize")) linkedMarchingCubesController.Initialize(gridResolutionX, gridResolutionY, gridResolutionZ, true);

            bool newInvertedNormals = EditorGUILayout.Toggle("Inverted normals", invertNormals);

            if(newInvertedNormals != invertNormals)
            {
                InvertNormals(newInvertedNormals);
            }

            GUILayout.Label("Save data:");
            linkedScriptableObjectSaveData = EditorGUILayout.ObjectField(
               linkedScriptableObjectSaveData,
               typeof(ScriptableObjectSaveData),
               true) as ScriptableObjectSaveData;

            if(linkedScriptableObjectSaveData != null)
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button($"Save data")) linkedMarchingCubesController.SaveGridData(linkedScriptableObjectSaveData);
                if (GUILayout.Button($"Load data")) LoadData();
                EditorGUILayout.EndHorizontal();
            }

            if (!linkedMarchingCubesController.IsInitialized) return;

            GUILayout.Label("Editing:");
            selectedShape = EditorGUILayout.ObjectField(
               selectedShape,
               typeof(EditShape),
               true) as EditShape;

            if (selectedShape)
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button($"Add {selectedShape.transform.name}")) linkedMarchingCubesController.AddShape(selectedShape, false);
                if (GUILayout.Button($"Subtract {selectedShape.transform.name}")) linkedMarchingCubesController.SubtractShape(selectedShape, false);
                EditorGUILayout.EndHorizontal();

                bool newAddingShape = EditorGUILayout.Toggle("Add Shape Mode", addingShape);
                limitMaxHeight = EditorGUILayout.Toggle("Limit max height", limitMaxHeight);

                if (newAddingShape && !addingShape) //Toggle on
                {
                    linkedMarchingCubesController.GenerateAndDisplayMesh(true);
                    originalShapePosition = selectedShape.transform.position;
                }
                else if(!newAddingShape && addingShape) //Toggle off
                {
                    selectedShape.transform.position = originalShapePosition;
                    selectedShape.gameObject.SetActive(true);
                }

                addingShape = newAddingShape;
            }

            if (addingShape)
            {
                SceneView.duringSceneGui += OnSceneGUI; // Subscribe to SceneView GUI event

                EditorGUILayout.HelpBox("Controls:\n" +
                    "Click to add\n" +
                    "Ctrl Click to subtract\n" +
                    "Shift Scroll to scale", MessageType.None);
            }
            else
            {
                SceneView.duringSceneGui -= OnSceneGUI; // Unsubscribe when not in use
            }
        }

        void InvertNormals(bool value)
        {
            linkedMarchingCubesController.invertedNormals = value;
            linkedMarchingCubesController.GenerateAndDisplayMesh(addingShape);

            invertNormals = value;
        }

        void LoadData()
        {
            linkedMarchingCubesController.LoadGridData(linkedScriptableObjectSaveData, addingShape);
            gridResolutionX = linkedMarchingCubesController.GridResolutionX;
            gridResolutionY = linkedMarchingCubesController.GridResolutionY;
            gridResolutionZ = linkedMarchingCubesController.GridResolutionZ;
        }

        void OnSceneGUI(SceneView sceneView)
        {
            Event e = Event.current;

            if (e.type == EventType.KeyUp && e.keyCode == KeyCode.Tab)
            {
                InvertNormals(!invertNormals);

                Debug.Log("Invert");

                e.Use();
            }

            if (addingShape && selectedShape)
            {
                Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);

                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    // Convert world hit point to grid or object coordinates, then add shape
                    Vector3 placementPosition = hit.point;
                    selectedShape.transform.position = hit.point;
                    selectedShape.gameObject.SetActive(true);
                    selectedShape.Color = e.control ? subtractionColor : additionColor;

                    if (e.shift && e.type == EventType.ScrollWheel)
                    {
                        float scaleDelta = e.delta.x * -0.03f; // Scale factor; reverse direction if needed

                        selectedShape.transform.localScale *= (scaleDelta + 1);

                        e.Use(); // Mark event as handled
                    }

                    if (e.type == EventType.MouseDown && e.button == 0) // Left-click event
                    {
                        if(e.control) linkedMarchingCubesController.SubtractShape(selectedShape, true);
                        else
                        {
                            if (limitMaxHeight) linkedMarchingCubesController.AddShapeWithMaxHeight(selectedShape, hit.point.y, true);
                            else linkedMarchingCubesController.AddShape(selectedShape, true);
                        }

                        e.Use();
                    }

                }
                else
                {
                    selectedShape.gameObject.SetActive(false);
                }
            }
        }
    }
}
#endif
