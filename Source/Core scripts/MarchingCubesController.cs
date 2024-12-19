#if UNITY_EDITOR
//#define DEBUG_PERFORMANCE

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace iffnsStuff.MarchingCubeEditor.Core
{
    [SelectionBase]
    public class MarchingCubesController : MonoBehaviour
    {
        private readonly List<MarchingCubesView> chunkViews = new List<MarchingCubesView>();
        private MarchingCubesModel mainModel;
        private MarchingCubesView previewView;
        private MarchingCubesModel previewModelWithOldData;
        private static readonly Vector3Int chunkSize = new Vector3Int(16, 16, 16);

        public ScriptableObjectSaveData linkedSaveData;

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
        [SerializeField] private GameObject previewPrefab;
        [SerializeField] private VisualisationManager linkedVisualisationManager;

        
        [SerializeField, HideInInspector]
        private bool invertNormals = false;

        public bool showGridOutline = false; // Toggle controlled by the editor tool

        public int GridResolutionX => mainModel.VoxelData.GetLength(0);
        public int GridResolutionY => mainModel.VoxelData.GetLength(1);
        public int GridResolutionZ => mainModel.VoxelData.GetLength(2);

        public VoxelData[,,] VoxelDataReference => mainModel.VoxelData;

        public Vector3Int MaxGrid => mainModel.MaxGrid;

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
                bool newState = value || forceColliderOn;

                foreach (MarchingCubesView chunkView in chunkViews)
                {
                    chunkView.ColliderEnabled = newState;
                }

                enableAllColliders = value;
            }
        }

        void InitializeColliderState()
        {
            EnableAllColliders = enableAllColliders; // Ensure correct fall through logic when changing stuff
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

            // Create model
            if (mainModel == null)
            {
                mainModel = new MarchingCubesModel(resolutionX, resolutionY, resolutionZ);
            }
            else
            {
                mainModel.ChangeGridSizeIfNeeded(resolutionX, resolutionY, resolutionZ, !setEmpty);
            }

            Vector3Int gridResolution = new Vector3Int(resolutionX, resolutionY, resolutionZ);

            // Destroy all chunks, save with foreach since Unity doesn't immediately destroy them
            List<GameObject> chunksToDestroy = new List<GameObject>();

            foreach (Transform child in transform)
            {
                if (child.TryGetComponent(out MarchingCubesView view))
                {
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

            // Create chunks
            for (int x = 0; x < resolutionX; x += chunkSize.x)
            {
                for (int y = 0; y < resolutionY; y += chunkSize.y)
                {
                    for (int z = 0; z < resolutionZ; z += chunkSize.z)
                    {
                        // Define chunk bounds
                        Vector3Int gridBoundsMin = new Vector3Int(x, y, z);

                        Vector3Int gridBoundsMax = Vector3Int.Min(gridBoundsMin + chunkSize, mainModel.MaxGrid);

                        MarchingCubesView chunkView = Instantiate(chunkPrefab, transform).GetComponent<MarchingCubesView>();
                        chunkViews.Add(chunkView);
                        chunkView.Initialize(gridBoundsMin, gridBoundsMax, enableAllColliders);
                    }
                }
            }

            // Set grid to empty
            if (setEmpty)
            {
                SetEmptyGrid(true);
            }

            // Setup preview model
            if (previewModelWithOldData == null)
            {
                previewModelWithOldData = new MarchingCubesModel(resolutionX, resolutionY, resolutionZ);
            }
            else
            {
                previewModelWithOldData.ChangeGridSizeIfNeeded(resolutionX, resolutionY, resolutionZ, false);
            }

            if (!previewView)
            {
                previewView = Instantiate(previewPrefab, transform).GetComponent<MarchingCubesView>();
                previewView.Initialize(Vector3Int.zero, Vector3Int.one, false);
                DisplayPreviewShape = false;
            }

            // Initialize the correct collider state
            InitializeColliderState();
        }

        public bool IsInitialized => mainModel != null;

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

        void UpdateAllChunks()
        {
            foreach (var chunkView in chunkViews)
            {
                chunkView.MarkDirty();
                chunkView.UpdateMeshIfDirty(mainModel);
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

            if(updateModel) UpdateAllChunks();
        }

        public void SetAllGridDataAndUpdateMesh(VoxelData[,,] newData)
        {
            mainModel.SetDataAndResizeIfNeeded(newData);
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
    }
}
#endif