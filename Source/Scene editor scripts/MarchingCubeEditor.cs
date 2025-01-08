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

        int gridCExpandSize = 0;
        

        List<BaseTool> tools;

        // This stores all the currently selectedTools across different Editors by using the
        // MarchingCubesController as a Key.
        readonly static Dictionary<Object, BaseTool> selectedTool = new Dictionary<Object, BaseTool>();


        bool generalFoldout = true;
        bool expansionFoldout = true;
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

                Controller.VisualisationManager.drawGizmosTool = value;
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
                // Normal grid size
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

                // Save and load
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

            // Expansion
            expansionFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(expansionFoldout, "Expansion");
            if (expansionFoldout)
            {
                gridCExpandSize = EditorGUILayout.IntField("Expansion size", gridCExpandSize);

                EditorGUILayout.BeginHorizontal();

                if (GUILayout.Button("Expand +X"))
                    Controller.ExpandGrid(gridCExpandSize, MarchingCubesController.ExpansionDirections.XPos);

                if (GUILayout.Button("Expand +Y"))
                    Controller.ExpandGrid(gridCExpandSize, MarchingCubesController.ExpansionDirections.YPos);

                if (GUILayout.Button("Expand +Z"))
                    Controller.ExpandGrid(gridCExpandSize, MarchingCubesController.ExpansionDirections.ZPos);

                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();

                if (GUILayout.Button("Expand -X"))
                    Controller.ExpandGrid(gridCExpandSize, MarchingCubesController.ExpansionDirections.XNeg);

                if (GUILayout.Button("Expand -Y"))
                    Controller.ExpandGrid(gridCExpandSize, MarchingCubesController.ExpansionDirections.YNeg);

                if (GUILayout.Button("Expand -Z"))
                    Controller.ExpandGrid(gridCExpandSize, MarchingCubesController.ExpansionDirections.ZNeg);

                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            settingsFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(settingsFoldout, "Settings");
            if (settingsFoldout)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                Controller.ForceColliderOn = EditorGUILayout.Toggle("Force colliders on", Controller.ForceColliderOn);
                Controller.PostProcessMesh = EditorGUILayout.Toggle("Post process mesh (slow)", Controller.PostProcessMesh);

                if(Controller.PostProcessMesh)
                {
                    Controller.AngleThresholdDeg = EditorGUILayout.FloatField("Angle threshold [°]", Controller.AngleThresholdDeg);
                    Controller.AreaThreshold = EditorGUILayout.FloatField("Area threshold", Controller.AreaThreshold);
                }

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

                int columns = 2; // Number of buttons per row

                Color highlightColor = new Color(0.7f, 0.7f, 1f); //ToDo: Improve highlight color

                for (int i = 0; i < tools.Count; i += columns)
                {
                    EditorGUILayout.BeginHorizontal();
                    for (int j = 0; j < columns; j++)
                    {
                        int index = i + j;
                        if (index < tools.Count) // Ensure index is within bounds
                        {
                            // Store original colors
                            Color originalBackground = GUI.backgroundColor;
                            Color originalContentColor = GUI.contentColor;

                            if (tools[index] == CurrentTool)
                            {
                                // Set custom colors for the selected tool
                                GUI.backgroundColor = highlightColor;
                                GUI.contentColor = Color.white; // Text color
                            }

                            if (GUILayout.Button(tools[index].DisplayName))
                            {
                                CurrentTool = tools[index];
                            }

                            // Restore original colors
                            GUI.backgroundColor = originalBackground;
                            GUI.contentColor = originalContentColor;
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }

                // Draw current tool UI
                if (CurrentTool != null)
                {
                    GUILayout.Label($"{CurrentTool.DisplayName}:");
                    CurrentTool.DrawUI();
                }
                    

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
