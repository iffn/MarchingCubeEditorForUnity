#if UNITY_EDITOR
using iffnsStuff.MarchingCubeEditor.Core;
using System.Collections.Generic;
using System.Linq;
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


        bool generalFoldout = true;
        bool settingsFoldout = true;
        bool toolsFoldout = true;

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

        public MarchingCubesController Controller => (MarchingCubesController)target;

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
            tools = BaseTool.GetTools(this).ToList();

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
            generalFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(generalFoldout, "General");
            if (generalFoldout)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
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

                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();


            settingsFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(settingsFoldout, "Settings");
            if (settingsFoldout)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                Controller.ForceColliderOn = EditorGUILayout.Toggle("Force colliders on", Controller.ForceColliderOn);
                Controller.VisualisationManager.ShowGridOutline = EditorGUILayout.Toggle("Show Grid Outline", Controller.VisualisationManager.ShowGridOutline);
                Controller.InvertAllNormals = EditorGUILayout.Toggle("Inverted normals", Controller.InvertAllNormals);

                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        void DrawEditUI()
        {
            toolsFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(toolsFoldout, "Tools");
            if (toolsFoldout)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                string[] tabs = tools.Select(tool => tool.DisplayName).ToArray();
                int index = tools.FindIndex(tab => tab == CurrentTool);

                // Draw Toolbar
                int newIndex = GUILayout.Toolbar(index, tabs);
                if (newIndex != index)
                    CurrentTool = tools[newIndex];
                    
                // Draw current tool
                if (CurrentTool != null)
                    CurrentTool.DrawUI();

                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        void UpdateSceneInteractionForController(SceneView sceneView)
        {
            CurrentTool?.HandleSceneUpdate(Event.current);
        }

        
        public RayHitResult RaycastAtMousePosition(Event e, bool detectBoundingBox = true)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hitInfo))
                return new RayHitResult(hitInfo.point, hitInfo.normal);
                
            if (!detectBoundingBox)
                return RayHitResult.None;

            Vector3 areaPosition = Controller.transform.position;
            Vector3Int areaSize = Controller.MaxGrid;
            Bounds bounds = new Bounds(areaPosition + areaSize / 2, areaSize);
            
            var result = bounds.GetIntersectRayPoints(ray);
            if (result != null)
                return new RayHitResult(result.Value.Item2, bounds.GetNormalToSurface(result.Value.Item2));
            
            // Both normal Raycast and Bounds intersection did not succeed 
            return RayHitResult.None;
        }
    }
}
#endif
