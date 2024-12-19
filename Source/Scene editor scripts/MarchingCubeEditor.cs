#if UNITY_EDITOR
using iffnsStuff.MarchingCubeEditor.Core;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace iffnsStuff.MarchingCubeEditor.SceneEditor
{
    [CustomEditor(typeof(MarchingCubesController))]
    public class MarchingCubeEditor : Editor
    {
        int gridResolutionX = 20;
        int gridResolutionY = 20;
        int gridResolutionZ = 20;
        bool invertNormals;

        List<BaseTool> tools;

        // This stores all the currently selectedTools across different Editors by using the
        // MarchingCubesController as a Key.
        readonly static Dictionary<Object, BaseTool> selectedTool = new Dictionary<Object, BaseTool>();

        BaseTool CurrentTool 
        {
            get => selectedTool.TryGetValue(target, out BaseTool tool) ? tool : null;
            set 
            {
                if (selectedTool.TryGetValue(target, out BaseTool tool)) 
                    tool.OnDisable();
                selectedTool[target] = value;
                value.OnEnable();
            }
        }

        MarchingCubesController Controller => (MarchingCubesController)target;

        public void LoadData()
        {
            if (Controller.linkedSaveData == null)
                return;

            gridResolutionX = Controller.linkedSaveData.resolutionX;
            gridResolutionY = Controller.linkedSaveData.resolutionY;
            gridResolutionZ = Controller.linkedSaveData.resolutionZ;
            Controller.Initialize(gridResolutionX, gridResolutionY, gridResolutionZ, true);
            Controller.SaveAndLoadManager.LoadGridData(Controller.linkedSaveData);
        }

        public override void OnInspectorGUI()
        {
            DrawSetupUI();
            DrawEditUI();
        }

        private void OnDisable() 
        {
            SceneView.duringSceneGui -= UpdateSceneInteractionForController;
        }

        private void OnEnable() 
        {
            tools = new List<BaseTool>() 
            {
                new SimpleSceneModifyTool(Controller),
                new SimpleClickToModifyTool(Controller),
                new SimpleClickToPaintTool(Controller),
            };

            if (!Controller.IsInitialized)
            {
                if (Controller.linkedSaveData == null)
                    Controller.Initialize(gridResolutionX, gridResolutionY, gridResolutionZ, true);
                else
                    LoadData();
            }
            else
            {
                gridResolutionX = Controller.GridResolutionX;
                gridResolutionY = Controller.GridResolutionY;
                gridResolutionZ = Controller.GridResolutionZ;
            }

            SceneView.duringSceneGui += UpdateSceneInteractionForController;
        }

        //Components
        void DrawSetupUI()
        {
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
                Controller.Initialize(gridResolutionX, gridResolutionY, gridResolutionZ, true);
            }

            GUILayout.Label("Save data:");
            ScriptableObjectSaveData newSaveData = EditorGUILayout.ObjectField(
               Controller.linkedSaveData,
               typeof(ScriptableObjectSaveData),
               true) as ScriptableObjectSaveData;


            if (newSaveData != Controller.linkedSaveData)
            {
                Undo.RecordObject(Controller, "Set save data file");
                Controller.linkedSaveData = newSaveData;
                EditorUtility.SetDirty(Controller);
                if (!Application.isPlaying)
                    UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(Controller.gameObject.scene);
            }

            if (Controller.linkedSaveData != null)
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button($"Save data")) Controller.SaveAndLoadManager.SaveGridData(Controller.linkedSaveData);
                if (GUILayout.Button($"Load data")) LoadData();
                EditorGUILayout.EndHorizontal();
            }

            GUILayout.Label("Visualization:");
            // Invert normals
            bool newInvertedNormals = EditorGUILayout.Toggle("Inverted normals", invertNormals);
            if (newInvertedNormals != invertNormals)
            {
                Controller.InvertAllNormals = newInvertedNormals;
                invertNormals = newInvertedNormals;
            }

            GUILayout.Label("Additional settings:");
            bool newForceCollidersOn = EditorGUILayout.Toggle("Force colliders on", Controller.ForceColliderOn);
            if (newForceCollidersOn != Controller.ForceColliderOn)
            {
                Controller.ForceColliderOn = newForceCollidersOn;
            }
        }

        void DrawEditUI()
        {
            // Show element buttons
            GUILayout.Label("Edit tools:");

            foreach (BaseTool tool in tools)
                if (GUILayout.Button(tool.displayName))
                    CurrentTool = tool;

            //Draw current tool
            if(CurrentTool != null)
            {
                GUILayout.Label($"{CurrentTool.displayName}:");
                CurrentTool.DrawUI();
            }
        }

        void UpdateSceneInteractionForController(SceneView sceneView)
        {
            CurrentTool?.HandleSceneUpdate(Event.current);
        }
    }
}
#endif
