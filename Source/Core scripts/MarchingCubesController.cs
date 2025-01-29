#if UNITY_EDITOR
//#define DEBUG_PERFORMANCE

using iffnsStuff.MarchingCubeEditor.EditTools;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static UnityUtilityFunctions;

namespace iffnsStuff.MarchingCubeEditor.Core
{
    [SelectionBase]
    public class MarchingCubesController : MonoBehaviour
    {
        private MarchingCubesModel mainModel;
        private MarchingCubesModel previewModelWithOldData;
        private static readonly Vector3Int defaultChunkSize = new Vector3Int(16, 16, 16);
        private Vector3Int chunkSize = defaultChunkSize;

        public ScriptableObjectSaveData linkedSaveData;

        private readonly List<MarchingCubesView> chunkViews = new List<MarchingCubesView>();
        public List<MarchingCubesView> ChunkViews => chunkViews;

        public bool DisplayPreviewShape
        {
            set
            {
                previewView.gameObject.SetActive(value);
            }
            get
            {
                return previewView.gameObject.activeSelf;
            }
        }

        [SerializeField] GameObject chunkPrefab; // Prefab for chunk views
        [SerializeField] private MarchingCubesPreview previewView;
        [SerializeField] private Transform chunkHolder;
        [SerializeField] Transform shapeHolder;
        [SerializeField] private VisualisationManager linkedVisualisationManager;

        public MarchingCubesPreview Preview => previewView;

        public List<EditShape> ShapeList { get; private set; } = new List<EditShape>();
        
        [SerializeField, HideInInspector]
        private bool invertNormals = false;

        public bool showGridOutline = false; // Toggle controlled by the editor tool

        public int GridResolutionX => mainModel.VoxelDataGrid.GetLength(0);
        public int GridResolutionY => mainModel.VoxelDataGrid.GetLength(1);
        public int GridResolutionZ => mainModel.VoxelDataGrid.GetLength(2);

        public VoxelData[,,] VoxelDataReference => mainModel.VoxelDataGrid;

        public Vector3Int MaxGrid => mainModel.MaxGrid;

        PostProcessingOptions currentPostProcessingOptions = PostProcessingOptions.Default;
        public PostProcessingOptions CurrentPostProcessingOptions
        {
            get => currentPostProcessingOptions;
            set
            {
                currentPostProcessingOptions = value;
                GenerateViewChunks(false);
            }
        }

        //Managers
        public ModificationManager ModificationManager { get; private set; }
        public SaveAndLoadManager SaveAndLoadManager { get; private set; }
        public VisualisationManager VisualisationManager
        {
            get
            {
                return linkedVisualisationManager;
            }
        }

        [HideInInspector, SerializeField] bool forceColliderOn = false;
        public bool ForceColliderOn
        {
            get
            {
                return forceColliderOn;
            }
            set
            {
                if (value)
                {
                    foreach (MarchingCubesView chunkView in chunkViews)
                    {
                        chunkView.ColliderEnabled = true;
                    }
                }
                else
                {
                    foreach (MarchingCubesView chunkView in chunkViews)
                    {
                        chunkView.ColliderEnabled = enableAllColliders;
                    }
                }

                forceColliderOn = value;

#if UNITY_EDITOR
                EditorUtility.SetDirty(this);
#endif
            }
        }

        bool enableAllColliders = false; // EnableAllColliders
        public bool EnableAllColliders
        {
            get
            {
                if (forceColliderOn) return true;
                return enableAllColliders;
            }
            set
            {
                enableAllColliders = value;

                UpdateColliderStates();
            }
        }

        void UpdateColliderStates()
        {
            foreach (MarchingCubesView chunkView in chunkViews)
            {
                chunkView.ColliderEnabled = EnableAllColliders;
            }
        }

        void GenerateViewChunks(bool postProcessCall)
        {
            // Decide on chunk size
            if ((postProcessCall || currentPostProcessingOptions.postProcessWhileEditing) && currentPostProcessingOptions.createOneChunk)
            {
                chunkSize = new Vector3Int(mainModel.ResolutionX, mainModel.ResolutionY, mainModel.ResolutionZ);
            }
            else
            {
                chunkSize = defaultChunkSize;
            }

            // Destroy all chunks, save with foreach since Unity doesn't immediately destroy them
            List<GameObject> chunksToDestroy = new List<GameObject>();

            foreach (Transform child in chunkHolder)
            {
                if (child.TryGetComponent(out MarchingCubesView view))
                {
                    if (view == previewView) continue;

                    chunksToDestroy.Add(child.gameObject);
                }
            }

            foreach (GameObject chunk in chunksToDestroy)
            {
                if (Application.isPlaying)
                {
                    Destroy(chunk); // Safe for runtime
                }
                else
                {
                    DestroyImmediate(chunk); // Safe for edit mode
                }
            }

            chunkViews.Clear();

            // Create and setup chunks
            int resolutionX = mainModel.ResolutionX;
            int resolutionY = mainModel.ResolutionY;
            int resolutionZ = mainModel.ResolutionZ;

            for (int x = 0; x < resolutionX; x += chunkSize.x)
            {
                for (int y = 0; y < resolutionY; y += chunkSize.y)
                {
                    for (int z = 0; z < resolutionZ; z += chunkSize.z)
                    {
                        // Define chunk bounds
                        Vector3Int gridBoundsMin = new Vector3Int(x, y, z);

                        Vector3Int gridBoundsMax = Vector3Int.Min(gridBoundsMin + chunkSize, mainModel.MaxGrid);

                        MarchingCubesView chunkView = Instantiate(chunkPrefab, chunkHolder).GetComponent<MarchingCubesView>();
                        chunkViews.Add(chunkView);
                        chunkView.Initialize(gridBoundsMin, gridBoundsMax, enableAllColliders);
                    }
                }
            }

            UpdateAllChunks(postProcessCall);

            UpdateColliderStates();
        }

        public void Initialize(int resolutionX, int resolutionY, int resolutionZ, bool setEmpty)
        {
            // We don't want to initialize if we are inside a prefab
            if (gameObject.scene.name == null)
                return;

            //Setup managers
            if (ModificationManager == null)
                ModificationManager = new ModificationManager(this);
            if (SaveAndLoadManager == null)
                SaveAndLoadManager = new SaveAndLoadManager(this);
            VisualisationManager.Initialize(this);

            // Create and setup model
            if (mainModel == null)
            {
                mainModel = new MarchingCubesModel(resolutionX, resolutionY, resolutionZ);
            }
            else
            {
                mainModel.ChangeGridSizeIfNeeded(resolutionX, resolutionY, resolutionZ, !setEmpty);
            }

            if (setEmpty)
            {
                SetEmptyGrid(true);
            }

            //Generate views
            GenerateViewChunks(false);

            // Setup preview model
            if (previewModelWithOldData == null)
            {
                previewModelWithOldData = new MarchingCubesModel(resolutionX, resolutionY, resolutionZ);
            }
            else
            {
                previewModelWithOldData.ChangeGridSizeIfNeeded(resolutionX, resolutionY, resolutionZ, false);
            }

            previewView.Initialize(Vector3Int.zero, Vector3Int.one, false);
            DisplayPreviewShape = false;

            GatherTools();
        }

        public bool IsInitialized => mainModel != null;

        public void GatherTools()
        {
            ShapeList.Clear();

            foreach (Transform child in shapeHolder)
            {
                if (!child.TryGetComponent(out EditShape tool)) return;

                ShapeList.Add(tool);
            }
        }

        public void ApplyPreviewChanges()
        {
            // Get grid size from preview
            Vector3Int gridBoundsMin = previewView.GridBoundsMin;
            Vector3Int gridBoundsMax = previewView.GridBoundsMax;

            // Copy data from preview
            mainModel.CopyRegion(previewModelWithOldData, gridBoundsMin, gridBoundsMax);

            // Mark affected chunks as dirty
            MarkRegionDirty(gridBoundsMin, gridBoundsMax);

            // Update affected chunk meshes
            UpdateAffectedChunks(gridBoundsMin, gridBoundsMax);
        }

        public void MarkRegionDirty(Vector3Int minGrid, Vector3Int maxGrid)
        {
            foreach (var chunkView in chunkViews) // ToDo: Optimize doing a for instead of a foreach loop
            {
                if (chunkView.IsWithinBounds(minGrid, maxGrid))
                {
                    chunkView.MarkDirty();
                }
            }
        }

        public void UpdateAffectedChunks(Vector3Int minGrid, Vector3Int maxGrid)
        {
            foreach (var chunkView in chunkViews)
            {
                if (chunkView.IsWithinBounds(minGrid, maxGrid))
                {
                    chunkView.UpdateMeshIfDirty(mainModel);
                }
            }
        }

        void UpdateAllChunks(bool postProcessingCall)
        {
            foreach (MarchingCubesView view in chunkViews)
            {
                view.MarkDirty();
                view.UpdateMeshIfDirty(mainModel);
            }

            if (currentPostProcessingOptions.postProcessWhileEditing || postProcessingCall)
            {
                PostProcessMesh();
            }
        }

        public bool InvertAllNormals
        {
            set 
            {
                if (InvertAllNormals != value) 
                    chunkViews.ForEach(chunk => chunk.InvertedNormals = value);

                invertNormals = value;
            }
            get => invertNormals;
        }

        public void SetEmptyGrid(bool updateModel)
        {
            for (int x = 0; x < mainModel.ResolutionX; x++)
            {
                for (int y = 0; y < mainModel.ResolutionY; y++)
                {
                    for (int z = 0; z < mainModel.ResolutionZ; z++)
                    {
                        mainModel.SetVoxel(x, y, z, VoxelData.Empty); // Use signed distance
                    }
                }
            }

            if(updateModel) UpdateAllChunks(false);
        }

        public void PostProcessMesh()
        {
            MarchingCubesView.ResetPostProcessingDiagnostics();

            foreach (MarchingCubesView view in chunkViews)
            {
                view.PostProcessMesh(currentPostProcessingOptions);
            }
        }

        public void SetAllGridDataAndUpdateMesh(VoxelData[,,] newData)
        {
            mainModel.SetDataAndResizeIfNeeded(newData);
            GenerateViewChunks();

            UpdateAllChunks();
        }

        /// <summary>
        /// Gets the value of a specific voxel in the grid.
        /// </summary>
        /// <param name="x">The x-coordinate of the voxel.</param>
        /// <param name="y">The y-coordinate of the voxel.</param>
        /// <param name="z">The z-coordinate of the voxel.</param>
        /// <returns>The value of the voxel.</returns>
        public VoxelData GetDataPoint(int x, int y, int z)
        {
            return mainModel.GetVoxel(x, y, z);
        }

        /// <summary>
        /// Sets the value of a specific voxel in the grid.
        /// </summary>
        /// <param name="x">The x-coordinate of the voxel.</param>
        /// <param name="y">The y-coordinate of the voxel.</param>
        /// <param name="z">The z-coordinate of the voxel.</param>
        /// <param name="value">The value to set.</param>
        public void SetDataPointWithSettingItToDirty(int x, int y, int z, VoxelData value)
        {
            SetDataPointWithoutSettingItToDirty(x, y, z, value);
            MarkRegionDirty(new Vector3Int(x, y, z), new Vector3Int(x + 1, y + 1, z + 1)); // ToDo: Optimize my only passing one element
        }

        public void SetDataPointWithoutSettingItToDirty(int x, int y, int z, VoxelData value)
        {
            mainModel.SetVoxel(x, y, z, value);
        }

        /// <summary>
        /// Sets up the preview zone by resizing the preview model and configuring bounds.
        /// </summary>
        /// <param name="minGrid">Minimum bounds of the preview zone.</param>
        /// <param name="maxGrid">Maximum bounds of the preview zone.</param>
        public void SetupPreviewZone(Vector3Int minGrid, Vector3Int maxGrid)
        {
            previewModelWithOldData.CopyRegion(mainModel, minGrid, maxGrid);
            previewView.UpdateBounds(minGrid, maxGrid);
            DisplayPreviewShape = true;
        }

        /// <summary>
        /// Sets a voxel value in the preview model.
        /// </summary>
        /// <param name="x">The x-coordinate of the voxel.</param>
        /// <param name="y">The y-coordinate of the voxel.</param>
        /// <param name="z">The z-coordinate of the voxel.</param>
        /// <param name="value">The value to set in the preview model.</param>
        public void SetPreviewDataPoint(int x, int y, int z, VoxelData value)
        {
            previewModelWithOldData.SetVoxel(x, y, z, value);
        }

        /// <summary>
        /// Updates the preview shape with the new data.
        /// </summary>
        public void UpdatePreviewShape()
        {
            previewView.MarkDirty();
            previewView.UpdateMeshIfDirty(previewModelWithOldData);
        }

        public enum ExpansionDirections
        {
            XPos, YPos, ZPos,
            XNeg, YNeg, ZNeg
        }

        public void ExpandGrid(int offset, ExpansionDirections expansionDirection)
        {
            int offsetX = 0;
            int offsetY = 0;
            int offsetZ = 0;

            int resolutionX = mainModel.ResolutionX;
            int resolutionY = mainModel.ResolutionY;
            int resolutionZ = mainModel.ResolutionZ;

            switch (expansionDirection)
            {
                case ExpansionDirections.XPos:
                    resolutionX += offset;
                    break;
                case ExpansionDirections.YPos:
                    resolutionY += offset;
                    break;
                case ExpansionDirections.ZPos:
                    resolutionZ += offset;
                    break;
                case ExpansionDirections.XNeg:
                    resolutionX += offset;
                    offsetX = offset;
                    break;
                case ExpansionDirections.YNeg:
                    resolutionY += offset;
                    offsetY = offset;
                    break;
                case ExpansionDirections.ZNeg:
                    resolutionZ += offset;
                    offsetZ = offset;
                    break;
                default:
                    break;
            }

            mainModel.ChangeGridSize(resolutionX, resolutionY, resolutionZ, offsetX, offsetY, offsetZ);

            GenerateViewChunks();
        }
    }
}
#endif