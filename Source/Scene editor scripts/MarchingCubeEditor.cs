# if UNITY_EDITOR
using iffnsStuff.MarchingCubeEditor.Core;
using iffnsStuff.MarchingCubeEditor.EditTools;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static UnityEngine.GridBrushBase;

namespace iffnsStuff.MarchingCubeEditor.SceneEditor
{
    public class MarchingCubeEditor : EditorWindow
    {
        int gridResolutionX = 20;
        int gridResolutionY = 20;
        int gridResolutionZ = 20;
        bool invertNormals;

        readonly List<BaseTool> tools = new();
        BaseTool currentTool;

        [MenuItem("Tools/iffnsStuff/MarchingCubeEditor")]
        public static void ShowWindow()
        {
            GetWindow(typeof(MarchingCubeEditor));
        }

        readonly static List<MarchingCubeEditor> editors = new();
        static MarchingCubesController linkedMarchingCubesController;
        static EditShape selectedShape;

        public void UpdateLinkedCubesController(MarchingCubesController controller) 
        {
            linkedMarchingCubesController = controller;
        }

        static void FindSceneObjectsIfNeeded()
        {
            if(linkedMarchingCubesController == null)
            {
                linkedMarchingCubesController = Object.FindObjectOfType<MarchingCubesController>(false);

                foreach(MarchingCubeEditor editor in editors)
                {
                    editor.Repaint();
                }
            }

            if (selectedShape == null)
            {
                selectedShape = Object.FindObjectOfType<EditShape>(false);

                foreach (MarchingCubeEditor editor in editors)
                {
                    editor.Repaint();
                }
            }
        }

        static void RepaintWindow()
        {
            foreach (MarchingCubeEditor editor in editors)
            {
                editor.Repaint();
            }
        }

        private void OnEnable()
        {
            FindSceneObjectsIfNeeded();

            editors.Add(this);

            if(editors.Count == 1)
            {
                EditorApplication.hierarchyChanged += FindSceneObjectsIfNeeded;
                Undo.undoRedoPerformed += RepaintWindow;
            }
        }

        private void OnDisable()
        {
            editors.Remove(this);

            if(editors.Count == 0)
            {
                EditorApplication.hierarchyChanged -= FindSceneObjectsIfNeeded;
                Undo.undoRedoPerformed -= RepaintWindow;
            }
        }

        void OnGUI()
        {
            DrawSetupUI();
            DrawEditUI();
        }

        //Components
        void DrawSetupUI()
        {
            GUILayout.Label("Scene component:");

            linkedMarchingCubesController = EditorGUILayout.ObjectField(
               linkedMarchingCubesController,
               typeof(MarchingCubesController),
               true) as MarchingCubesController;

            if (linkedMarchingCubesController == null)
            {
                EditorGUILayout.HelpBox("Add a Marching cube prefab to your scene and link it to this scrip", MessageType.Warning);
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

            if (GUILayout.Button("Apply and set empty"))
            {
                linkedMarchingCubesController.Initialize(gridResolutionX, gridResolutionY, gridResolutionZ, true);
            }

            GUILayout.Label("Save data:");
            ScriptableObjectSaveData newSaveData = EditorGUILayout.ObjectField(
               linkedMarchingCubesController.linkedSaveData,
               typeof(ScriptableObjectSaveData),
               true) as ScriptableObjectSaveData;


            if (newSaveData != linkedMarchingCubesController.linkedSaveData)
            {
                Undo.RecordObject(linkedMarchingCubesController, "Set save data file");
                linkedMarchingCubesController.linkedSaveData = newSaveData;
                EditorUtility.SetDirty(linkedMarchingCubesController);
                if (!Application.isPlaying)
                    UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(linkedMarchingCubesController.gameObject.scene);
            }

            if (linkedMarchingCubesController.linkedSaveData != null)
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button($"Save data")) linkedMarchingCubesController.SaveAndLoadManager.SaveGridData(linkedMarchingCubesController.linkedSaveData);
                if (GUILayout.Button($"Load data")) LoadData();
                EditorGUILayout.EndHorizontal();
            }

            // Auto initialize and load
            // ToDo: Check if it makes sense to move this into the MarchingCubeController using [ExecuteInEditMode] and OnEnable
            if (!linkedMarchingCubesController.IsInitialized)
            {
                bool loadData = linkedMarchingCubesController.linkedSaveData != null;

                linkedMarchingCubesController.Initialize(gridResolutionX, gridResolutionY, gridResolutionZ, !loadData);

                if (loadData) LoadData();
            }

            GUILayout.Label("Visualization:");
            // Invert normals
            bool newInvertedNormals = EditorGUILayout.Toggle("Inverted normals", invertNormals);
            if (newInvertedNormals != invertNormals)
            {
                linkedMarchingCubesController.InvertAllNormals = newInvertedNormals;
                invertNormals = newInvertedNormals;
            }
        }

        void DrawEditUI()
        {
            // Create elements if needed
            //ToDo: Implement setup differently, since somewhat slow and running every update
            if (!tools.Exists(tool => tool is SimpleSceneModifyTool))
                tools.Add(new SimpleSceneModifyTool(linkedMarchingCubesController));

            if (!tools.Exists(tool => tool is SimpleClickToModifyTool))
                tools.Add(new SimpleClickToModifyTool(linkedMarchingCubesController));

            if (!tools.Exists(tool => tool is SimpleClickToPaintTool))
                tools.Add(new SimpleClickToPaintTool(linkedMarchingCubesController));

            // Show element buttons
            GUILayout.Label("Edit tools:");
            foreach (BaseTool tool in tools)
            {
                if (GUILayout.Button(tool.displayName))
                {
                    currentTool?.OnDisable();
                    currentTool = tool;
                    currentTool.OnEnable();
                }
            }

            //Draw current tool
            if(currentTool != null)
            {
                GUILayout.Label($"{currentTool.displayName}:");
                currentTool.DrawUI();
            }

            //Update scene interactions
            if (currentTool != null)
            {
                SceneView.duringSceneGui += UpdateSceneInteractionForController;
            }
            else
            {
                SceneView.duringSceneGui -= UpdateSceneInteractionForController;
            }
        }

        void UpdateSceneInteractionForController(SceneView sceneView)
        {
            Event currentEvent = Event.current;

            currentTool.HandleSceneUpdate(currentEvent);
        }

        //Helper functions
        void LoadData() //Note: Only load data using this function to ensure that the grid resolution values are correctly set.
        {
            linkedMarchingCubesController.SaveAndLoadManager.LoadGridData(linkedMarchingCubesController.linkedSaveData);
            gridResolutionX = linkedMarchingCubesController.GridResolutionX;
            gridResolutionY = linkedMarchingCubesController.GridResolutionY;
            gridResolutionZ = linkedMarchingCubesController.GridResolutionZ;
        }
    }
}
#endif
