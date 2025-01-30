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
        List<BaseTool> tools;

        bool defaultFoldout = false;
        bool toolsFoldout = true;

        SizeAndLoaderEditorElement sizeAndLoaderEditorElement;
        ExpansionEditorElement expansionEditorElement;
        PostProcessingEditorElement postProcessingEditorElement;
        SettingsEditorElement settingsEditorElement;

        // This stores all the currently selectedTools across different Editors by using the MarchingCubesController as a Key.
        readonly static Dictionary<Object, BaseTool> selectedTool = new Dictionary<Object, BaseTool>();
		
        BaseTool CurrentTool 
        {
            get => selectedTool.TryGetValue(target, out BaseTool tool) ? tool : null;
            set
            {
                if (selectedTool.TryGetValue(target, out BaseTool currentTool))
                {
                    currentTool.OnDisable();
                }

                if (value == null)
                {
                    selectedTool.Remove(target);
                    LinkedMarchingCubeController.VisualisationManager.drawGizmosTool = null;
                }
                else
                {
                    selectedTool[target] = value;
                    value.OnEnable();
                    LinkedMarchingCubeController.VisualisationManager.drawGizmosTool = value;
                }
            }
        }

        public MarchingCubesController LinkedMarchingCubeController => (MarchingCubesController)target;

        public void LoadData()
        {
            if (LinkedMarchingCubeController.linkedSaveData == null)
                return;

            LinkedMarchingCubeController.SaveAndLoadManager.LoadGridData(LinkedMarchingCubeController.linkedSaveData);
        }

        public override void OnInspectorGUI()
        {
            defaultFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(defaultFoldout, "Default inspector");
            if (defaultFoldout)
            {
                DrawDefaultInspector();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            DrawSetupUI();
            postProcessingEditorElement.DrawAsFoldout();
            DrawEditUI();
        }

        private void OnDisable() 
        {
            SceneView.duringSceneGui -= UpdateSceneInteractionForController;
        }

        private void OnEnable() 
        {
			// Initialize controller
            if (!LinkedMarchingCubeController.IsInitialized)
            {
                LinkedMarchingCubeController.Initialize(20, 20, 20, true);

                if (LinkedMarchingCubeController.linkedSaveData != null)
                    LoadData();
            }
            else
            {
                // ToDo: Update resolution
            }

            // Setup foldouts
            if (sizeAndLoaderEditorElement == null)
                sizeAndLoaderEditorElement = new SizeAndLoaderEditorElement(this, true);

			if(expansionEditorElement == null)
                expansionEditorElement = new ExpansionEditorElement(this, false);

			if(postProcessingEditorElement == null)
                postProcessingEditorElement = new PostProcessingEditorElement(this, false);

			if(settingsEditorElement == null)
                settingsEditorElement = new SettingsEditorElement(this, false);
            
            // Seutp tools
            tools = BaseTool.GetTools(this).ToList();

            SceneView.duringSceneGui += UpdateSceneInteractionForController;
        }

        //Components
        void DrawSetupUI()
        {
            sizeAndLoaderEditorElement.DrawAsFoldout();

            expansionEditorElement.DrawAsFoldout();

            settingsEditorElement.DrawAsFoldout();
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
                                if(CurrentTool == tools[index])
                                {
                                    CurrentTool = null;
                                }
                                else
                                {
                                    CurrentTool = tools[index];
                                }
                                
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

            Vector3 areaPosition = LinkedMarchingCubeController.transform.position;
            Vector3Int areaSize = LinkedMarchingCubeController.MaxGrid;
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
