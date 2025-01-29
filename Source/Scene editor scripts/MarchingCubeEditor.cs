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

        bool defaultFoldout = false;
        bool generalFoldout = true;
        bool expansionFoldout = false;
        bool settingsFoldout = true;
        bool toolsFoldout = true;
        bool moveTransformWhenExpanding = true;

		PostProcessingEditorElement postProcessingEditorElement;

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

        void LoadData()
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
            postProcessingEditorElement.DrawAsFoldout(Controller);
            DrawEditUI();
        }

        private void OnDisable() 
        {
            SceneView.duringSceneGui -= UpdateSceneInteractionForController;
        }

        private void OnEnable() 
        {
            tools = BaseTool.GetTools(this).ToList();
			
			if(postProcessingEditorElement == null)
                postProcessingEditorElement = new PostProcessingEditorElement(true);
			
            if (!LinkedMarchingCubeController.IsInitialized)
            {
                LinkedMarchingCubeController.Initialize(gridResolutionX, gridResolutionY, gridResolutionZ, true);

                if (LinkedMarchingCubeController.linkedSaveData != null)
                    LoadData();
            }
            else
            {
                UpdateGridResolutionFromController();
            }

            SceneView.duringSceneGui += UpdateSceneInteractionForController;
        }

        void UpdateGridResolutionFromController()
        {
            gridResolutionX = LinkedMarchingCubeController.GridResolutionX;
            gridResolutionY = LinkedMarchingCubeController.GridResolutionY;
            gridResolutionZ = LinkedMarchingCubeController.GridResolutionZ;
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
                GUILayout.Label($"{LinkedMarchingCubeController.GridResolutionX}");
                GUILayout.Label($"{LinkedMarchingCubeController.GridResolutionY}");
                GUILayout.Label($"{LinkedMarchingCubeController.GridResolutionZ}");
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                gridResolutionX = EditorGUILayout.IntField(gridResolutionX);
                gridResolutionY = EditorGUILayout.IntField(gridResolutionY);
                gridResolutionZ = EditorGUILayout.IntField(gridResolutionZ);
                EditorGUILayout.EndHorizontal();

                if (GUILayout.Button("Apply and set empty"))
                {
                    LinkedMarchingCubeController.Initialize(gridResolutionX, gridResolutionY, gridResolutionZ, true);
                }

                // Save and load
                GUILayout.Label("Save data:");
                ScriptableObjectSaveData newSaveData = EditorGUILayout.ObjectField(
                LinkedMarchingCubeController.linkedSaveData,
                typeof(ScriptableObjectSaveData),
                true) as ScriptableObjectSaveData;


                if (newSaveData != LinkedMarchingCubeController.linkedSaveData)
                {
                    Undo.RecordObject(LinkedMarchingCubeController, "Set save data file");
                    LinkedMarchingCubeController.linkedSaveData = newSaveData;
                    EditorUtility.SetDirty(LinkedMarchingCubeController);
                    if (!Application.isPlaying)
                        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(LinkedMarchingCubeController.gameObject.scene);
                }

                if (LinkedMarchingCubeController.linkedSaveData != null)
                {
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button($"Save data")) LinkedMarchingCubeController.SaveAndLoadManager.SaveGridData(LinkedMarchingCubeController.linkedSaveData);
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
                {
                    LinkedMarchingCubeController.ExpandGrid(gridCExpandSize, MarchingCubesController.ExpansionDirections.XPos);
                    UpdateGridResolutionFromController();
                }

                if (GUILayout.Button("Expand +Y"))
                {
                    LinkedMarchingCubeController.ExpandGrid(gridCExpandSize, MarchingCubesController.ExpansionDirections.YPos);
                    UpdateGridResolutionFromController();
                }

                if (GUILayout.Button("Expand +Z"))
                {
                    LinkedMarchingCubeController.ExpandGrid(gridCExpandSize, MarchingCubesController.ExpansionDirections.ZPos);
                    UpdateGridResolutionFromController();
                }

                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();

                if (GUILayout.Button("Expand -X"))
                {
                    LinkedMarchingCubeController.ExpandGrid(gridCExpandSize, MarchingCubesController.ExpansionDirections.XNeg);
                    
                    if (moveTransformWhenExpanding)
                        LinkedMarchingCubeController.transform.localPosition -= gridCExpandSize * LinkedMarchingCubeController.transform.localScale.x * Vector3.right;

                    UpdateGridResolutionFromController();
                }

                if (GUILayout.Button("Expand -Y"))
                {
                    LinkedMarchingCubeController.ExpandGrid(gridCExpandSize, MarchingCubesController.ExpansionDirections.YNeg);

                    if (moveTransformWhenExpanding)
                        LinkedMarchingCubeController.transform.localPosition -= gridCExpandSize * LinkedMarchingCubeController.transform.localScale.y * Vector3.up;

                    UpdateGridResolutionFromController();
                }

                if (GUILayout.Button("Expand -Z"))
                {
                    LinkedMarchingCubeController.ExpandGrid(gridCExpandSize, MarchingCubesController.ExpansionDirections.ZNeg);

                    if (moveTransformWhenExpanding)
                        LinkedMarchingCubeController.transform.localPosition -= gridCExpandSize * LinkedMarchingCubeController.transform.localScale.z * Vector3.forward;

                    UpdateGridResolutionFromController();
                }

                EditorGUILayout.EndHorizontal();

                moveTransformWhenExpanding = EditorGUILayout.Toggle("Move transform to keep position", moveTransformWhenExpanding);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            settingsFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(settingsFoldout, "Settings");
            if (settingsFoldout)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                LinkedMarchingCubeController.ForceColliderOn = EditorGUILayout.Toggle("Force colliders on", LinkedMarchingCubeController.ForceColliderOn);

                LinkedMarchingCubeController.VisualisationManager.ShowGridOutline = EditorGUILayout.Toggle("Show Grid Outline", LinkedMarchingCubeController.VisualisationManager.ShowGridOutline);
                LinkedMarchingCubeController.InvertAllNormals = EditorGUILayout.Toggle("Inverted normals", LinkedMarchingCubeController.InvertAllNormals);

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
