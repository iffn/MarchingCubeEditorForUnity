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
        bool defaultFoldout = false;
        const int autoLoadMaxSize = 200 * 200 * 200;

        SizeAndLoaderEditorElement sizeAndLoaderEditorElement;
        ExpansionEditorElement expansionEditorElement;
        PostProcessingEditorElement postProcessingEditorElement;
        SettingsEditorElement settingsEditorElement;
        ToolEditorElement toolEditorElement;
        
        public MarchingCubesController LinkedMarchingCubeController => (MarchingCubesController)target;

        // This stores all the currently selectedTools across different Editors by using the MarchingCubesController as a Key.
        readonly static Dictionary<Object, BaseTool> selectedTool = new Dictionary<Object, BaseTool>();
		
        public BaseTool CurrentTool 
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

        private void OnEnable() 
        {
			// Initialize controller
            if (!LinkedMarchingCubeController.IsInitialized)
            {
                if (LinkedMarchingCubeController.linkedSaveData != null)
                {
                    bool largeFile = LinkedMarchingCubeController.linkedSaveData.VoxelCount > autoLoadMaxSize;

                    if (largeFile)
                    {
                        LinkedMarchingCubeController.Initialize(1, 1, 1, true, true);
                        Debug.Log($"Note: Loading skipped during initialization since it would have {LinkedMarchingCubeController.linkedSaveData.VoxelCount} voxels. Please load manually if needed.");
                    }
                    else
                    {
                        LinkedMarchingCubeController.Initialize(1, 1, 1, true, false);
                        LoadData();
                    }
                }
                else
                {
                    LinkedMarchingCubeController.Initialize(20, 20, 20, true, false);
                }
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

			if(toolEditorElement == null)
                toolEditorElement = new ToolEditorElement(this, true);
            
            SceneView.duringSceneGui += UpdateSceneInteractionForController;
        }

        private void OnDisable() 
        {
            SceneView.duringSceneGui -= UpdateSceneInteractionForController;
        }

        public override void OnInspectorGUI()
        {
            defaultFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(defaultFoldout, "Default inspector");
            if (defaultFoldout)
            {
                DrawDefaultInspector();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            sizeAndLoaderEditorElement.DrawAsFoldout();
            expansionEditorElement.DrawAsFoldout();
            settingsEditorElement.DrawAsFoldout();

            postProcessingEditorElement.DrawAsFoldout();

            toolEditorElement.DrawAsFoldout();
        }

        public void LoadData()
        {
            if (LinkedMarchingCubeController.linkedSaveData == null)
                return;

            LinkedMarchingCubeController.SaveAndLoadManager.LoadGridData(LinkedMarchingCubeController.linkedSaveData);
        }

        void UpdateSceneInteractionForController(SceneView sceneView)
        {
            CurrentTool?.HandleSceneUpdate(Event.current);
        }

        public void RefreshUI()
        {
            EditorUtility.SetDirty(target);
            Repaint();
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
