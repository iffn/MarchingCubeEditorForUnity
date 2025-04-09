// #define viewGenerationPeformanceOutput

#if UNITY_EDITOR

using iffnsStuff.MarchingCubeEditor.EditTools;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static UnityUtilityFunctions;

namespace iffnsStuff.MarchingCubeEditor.Core
{
    [SelectionBase]
    public class MarchingCubesController : MonoBehaviour
    {
        [SerializeField] MarchingCubesView chunkPrefab; // Prefab for chunk views
        [SerializeField] MarchingCubesPreview previewView;
        [SerializeField] Transform chunkHolder;
        [SerializeField] Transform shapeHolder;
        [SerializeField] VisualisationManager linkedVisualisationManager;
        [SerializeField] Material currentMainMaterial;
        [SerializeField] Material currentGrassMaterial;

        public ScriptableObjectSaveData linkedSaveData;
        public bool showGridOutline = false; // Toggle controlled by the editor tool

        MarchingCubesModel mainModel;
        MarchingCubesModel previewModelWithOldData;
        static readonly Vector3Int defaultChunkSize = new Vector3Int(16, 16, 16);
        Vector3Int chunkSize = defaultChunkSize;
        readonly List<MarchingCubesView> chunkViews = new List<MarchingCubesView>();

        public bool ViewsSetUp { get; private set; } = false;
        public List<EditShape> ShapeList { get; private set; } = new List<EditShape>();

        public Material CurrentMainMaterial
        {
            get
            {
                return currentMainMaterial;
            }
            set
            {
                currentMainMaterial = value;

                if (chunkViews == null)
                    return;

                foreach (MarchingCubesView view in chunkViews)
                {
                    view.CurrentMainMaterial = value;
                }
            }
        }

        public Material CurrentGrassMaterial
        {
            get
            {
                return currentGrassMaterial;
            }
            set
            {
                currentGrassMaterial = value;

                if (chunkViews == null)
                    return;

                foreach (MarchingCubesView view in chunkViews)
                {
                    view.CurrentGrassMaterial = value;
                }
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
                if (forceColliderOn == value)
                    return;

                forceColliderOn = value;

                UpdateColliderStates();

#if UNITY_EDITOR
                EditorUtility.SetDirty(this); // Makes it saveable
#endif
            }
        }

        bool enableAllColliders = false;
        public bool EnableAllColliders
        {
            set
            {
                if (enableAllColliders == value)
                    return;

                enableAllColliders = value;

                UpdateColliderStates();
            }
        }

        public bool CollidersShouldBeOn
        {
            get
            {
                return enableAllColliders || forceColliderOn;
            }
        }

        bool invertNormals = false;
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

        PostProcessingOptions currentPostProcessingOptions = PostProcessingOptions.Default;
        public PostProcessingOptions CurrentPostProcessingOptions
        {
            get => currentPostProcessingOptions;
            set
            {
                currentPostProcessingOptions = value;
                if (value.postProcessWhileEditing)
                    GenerateAndUpdateViewChunks(true);
            }
        }


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

        public List<MarchingCubesView> ChunkViews => chunkViews;
        public MarchingCubesPreview Preview => previewView;
        public bool IsInitialized => mainModel != null;
        public int GridResolutionX => mainModel.VoxelDataGrid.GetLength(0);
        public int GridResolutionY => mainModel.VoxelDataGrid.GetLength(1);
        public int GridResolutionZ => mainModel.VoxelDataGrid.GetLength(2);
        public Vector3Int MaxGrid => mainModel.MaxGrid;
        public VoxelData[,,] VoxelDataReference => mainModel.VoxelDataGrid;

        // Internal functions
        void UpdateColliderStates()
        {
            foreach (MarchingCubesView chunkView in chunkViews)
            {
                chunkView.ColliderEnabled = CollidersShouldBeOn;
            }
        }

        void GatherExistingChunks()
        {
            chunkViews.Clear();

            foreach (Transform child in chunkHolder)
            {
                if (child.TryGetComponent(out MarchingCubesView view))
                {
                    if (view == previewView) continue;

                    MarchingCubesView marchingCubeView = child.GetComponent<MarchingCubesView>();

                    if (marchingCubeView == null)
                        continue;

                    chunkViews.Add(marchingCubeView);
                }
            }
        }

        void GenerateAndUpdateViewChunks(bool directPostProcessCall)
        {
#if viewGenerationPeformanceOutput
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
#endif
            ViewsSetUp = true;
            int requiredChunks;

            int resolutionX = mainModel.ResolutionX;
            int resolutionY = mainModel.ResolutionY;
            int resolutionZ = mainModel.ResolutionZ;

            // Gather existing chunks
            GatherExistingChunks();

#if viewGenerationPeformanceOutput
            Debug.Log($"Gather: {sw.Elapsed.TotalMilliseconds}ms");
            sw.Restart();
#endif
            // Decide on chunk size and count
            if ((directPostProcessCall || currentPostProcessingOptions.postProcessWhileEditing) && currentPostProcessingOptions.createOneChunk)
            {
                chunkSize = new Vector3Int(mainModel.ResolutionX, mainModel.ResolutionY, mainModel.ResolutionZ);
                requiredChunks = 1;
            }
            else
            {
                chunkSize = defaultChunkSize;

                int chunksX = DivideAndRoundUp(resolutionX, chunkSize.x);
                int chunksY = DivideAndRoundUp(resolutionY, chunkSize.y);
                int chunksZ = DivideAndRoundUp(resolutionZ, chunkSize.z);
                requiredChunks = chunksX * chunksY * chunksZ;

                int DivideAndRoundUp(int value, int divisor)
                {
                    return value / divisor + (value % divisor == 0 ? 0 : 1);
                }
            }
#if viewGenerationPeformanceOutput
            Debug.Log($"Figure out: {sw.Elapsed.TotalMilliseconds}ms");
#endif

            if (requiredChunks == chunkViews.Count)
            {
                // No creation needed
#if viewGenerationPeformanceOutput
                Debug.Log($"Do nothing: {sw.Elapsed.TotalMilliseconds}ms");
#endif
            }
            else if(requiredChunks > chunkViews.Count)
            {
                // Create chunks
                int additionalChunks = requiredChunks - chunkViews.Count;

                for(int i = 0;  i < additionalChunks; i++)
                {
                    MarchingCubesView chunkView = Instantiate(chunkPrefab, chunkHolder).GetComponent<MarchingCubesView>();
                    chunkViews.Add(chunkView);
                }

#if viewGenerationPeformanceOutput
                Debug.Log($"Adding chunks: {sw.Elapsed.TotalMilliseconds}ms");
#endif
            }
            else
            {
                // Remove chunks
                List<GameObject> chunksToDestroy = new List<GameObject>();
                for(int i = requiredChunks; i < chunkViews.Count; i++)
                {
                    chunksToDestroy.Add(chunkViews[i].gameObject);
                }

                if (Application.isPlaying)
                {
                    foreach (GameObject chunk in chunksToDestroy)
                    {
                        Destroy(chunk); // Safe for runtime
                    }
                }
                else
                {
                    foreach (GameObject chunk in chunksToDestroy)
                    {
                        DestroyImmediate(chunk); // Safe for edit mode
                    }
                }

                // Set correct range
                chunkViews.RemoveRange(requiredChunks, chunkViews.Count - requiredChunks);

#if viewGenerationPeformanceOutput
                Debug.Log($"Removing chunks: {sw.Elapsed.TotalMilliseconds}ms");
#endif
                
            }

#if viewGenerationPeformanceOutput
            sw.Restart();
#endif

            if (requiredChunks == 1)
            {
                chunkViews[0].Initialize(Vector3Int.zero, mainModel.MaxGrid, CollidersShouldBeOn, CurrentMainMaterial, CurrentGrassMaterial);
            }
            else
            {
                int counter = 0;

                for (int x = 0; x < resolutionX; x += chunkSize.x)
                {
                    for (int y = 0; y < resolutionY; y += chunkSize.y)
                    {
                        for (int z = 0; z < resolutionZ; z += chunkSize.z)
                        {
                            // Define chunk bounds
                            Vector3Int gridBoundsMin = new Vector3Int(x, y, z);

                            Vector3Int gridBoundsMax = Vector3Int.Min(gridBoundsMin + chunkSize, mainModel.MaxGrid);

                            chunkViews[counter++].Initialize(gridBoundsMin, gridBoundsMax, CollidersShouldBeOn, CurrentMainMaterial, CurrentGrassMaterial);
                        }
                    }
                }
            }

#if viewGenerationPeformanceOutput
            Debug.Log($"Inits: {sw.Elapsed.TotalMilliseconds}ms");
            sw.Restart();
#endif

            UpdateAllChunks(directPostProcessCall);

#if viewGenerationPeformanceOutput
            Debug.Log($"Update mesh: {sw.Elapsed.TotalMilliseconds}ms");
            sw.Restart();
#endif

            UpdateColliderStates();

#if viewGenerationPeformanceOutput
            Debug.Log($"Update collider: {sw.Elapsed.TotalMilliseconds}ms");
            sw.Restart();
#endif
        }

        void UpdateAllChunks(bool directPostProcessCall)
        {
            foreach (MarchingCubesView view in chunkViews)
            {
                view.MarkDirty();
                view.UpdateMeshIfDirty(mainModel);
            }

            if (directPostProcessCall || currentPostProcessingOptions.postProcessWhileEditing)
            {
                MarchingCubesView.ResetPostProcessingDiagnostics();

                foreach (MarchingCubesView view in chunkViews)
                {
                    view.PostProcessMesh(currentPostProcessingOptions);
                }
            }
        }

        void GatherTools()
        {
            ShapeList.Clear();

            foreach (Transform child in shapeHolder)
            {
                if (!child.TryGetComponent(out EditShape tool)) return;

                ShapeList.Add(tool);
            }
        }

        void ClearUpOldViews()
        {
            List<MarchingCubesView> chunksToDestroy = new List<MarchingCubesView>();

            foreach (MarchingCubesView view in chunkViews)
            {
                if (!view.SetupIsCorrect(chunkPrefab))
                {
                    chunksToDestroy.Add(view);
                }
            }

            if (Application.isPlaying)
            {
                foreach (MarchingCubesView chunk in chunksToDestroy)
                {
                    Destroy(chunk.gameObject); // Safe for runtime
                    chunkViews.Remove(chunk);
                }
            }
            else
            {
                foreach (MarchingCubesView chunk in chunksToDestroy)
                {
                    DestroyImmediate(chunk.gameObject); // Safe for edit mode
                    chunkViews.Remove(chunk);
                }
            }

            List<GameObject> invalidGameObjects = new List<GameObject>();

            foreach (Transform element in chunkHolder)
            {
                element.TryGetComponent(out MarchingCubesView view);

                if (view == null)
                {
                    invalidGameObjects.Add(element.gameObject);
                }
            }

            if (Application.isPlaying)
            {
                foreach (GameObject chunk in invalidGameObjects)
                {
                    Destroy(chunk); // Safe for runtime
                }
            }
            else
            {
                foreach (GameObject chunk in invalidGameObjects)
                {
                    DestroyImmediate(chunk); // Safe for edit mode
                }
            }

            if(chunksToDestroy.Count != 0 || invalidGameObjects.Count != 0)
            {
                Debug.Log($"Marching cube cleanup: Removed {chunksToDestroy} invalid view chunks and an additional {invalidGameObjects.Count} GameObjects");
            }
        }

        // External funcitons
        /// <summary>
        /// Initializes the controller. Can be called again.
        /// </summary>
        public void Initialize(int resolutionX, int resolutionY, int resolutionZ, bool setEmpty, bool skipViewSetup)
        {
            ViewsSetUp = false;

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
                SetEmptyGrid(false); // Don't update model since chunks not yet generated
            }

            GatherExistingChunks();

            ClearUpOldViews();

            if (!skipViewSetup)
            {
                //Generate views
                GenerateAndUpdateViewChunks(false);
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

            previewView.Initialize(Vector3Int.zero, Vector3Int.one, false);
            DisplayPreviewShape = false;

            GatherTools();
        }

        /// <summary>
        /// Marks all chunks between and including the two grid points diry for editing.
        /// </summary>
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

        public void MarkRegionDirty(Vector3Int gridPoint)
        {
            foreach (var chunkView in chunkViews) // ToDo: Optimize doing a for instead of a foreach loop
            {
                if (chunkView.IsWithinBounds(gridPoint))
                {
                    chunkView.MarkDirty();
                }
            }
        }

        /// <summary>
        /// Updates all dirty chunks.
        /// </summary>
        public void UpdateAffectedChunks(Vector3Int gridPoint)
        {
            foreach (MarchingCubesView chunkView in chunkViews)
            {
                if (chunkView.IsWithinBounds(gridPoint))
                {
                    chunkView.UpdateMeshIfDirty(mainModel);
                    return;
                }
            }
        }

        public void UpdateAffectedChunks(Vector3Int minGrid, Vector3Int maxGrid)
        {
            foreach (MarchingCubesView chunkView in chunkViews)
            {
                if (chunkView.IsWithinBounds(minGrid, maxGrid))
                {
                    chunkView.UpdateMeshIfDirty(mainModel);
                }
            }
        }

        /// <summary>
        /// Sets all grid data to empty
        /// </summary>
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

            if (updateModel) UpdateAllChunks(false);
        }

        /// <summary>
        /// Sets all grid data. Resizes if needed
        /// </summary>
        public void SetAllGridDataAndUpdateMesh(VoxelData[,,] newData)
        {
            mainModel.SetDataAndResizeIfNeeded(newData);

            previewModelWithOldData.ChangeGridSizeIfNeeded(GridResolutionX, GridResolutionY, GridResolutionZ, false);

            GenerateAndUpdateViewChunks(false);
        }

        /// <summary>
        /// Applies the current post processing options
        /// </summary>
        public void PostProcessMesh()
        {
            GenerateAndUpdateViewChunks(true);
        }

        /// <summary>
        /// Gets the value of a specific voxel in the grid.
        /// </summary>
        /// <param name="x">The x-coordinate of the voxel.</param>
        /// <param name="y">The y-coordinate of the voxel.</param>
        /// <param name="z">The z-coordinate of the voxel.</param>
        /// <returns>The value of the voxel.</returns>
        public VoxelData GetVoxelWithoutClamp(int x, int y, int z)
        {
            return mainModel.GetVoxelWithoutClamp(x, y, z);
        }

        public VoxelData GetVoxelWithClamp(int x, int y, int z)
        {
            return mainModel.GetVoxelWithClamp(x, y, z);
        }

        public VoxelData GetVoxelWithClamp(float x, float y, float z)
        {
            return mainModel.GetVoxelWithClamp(x, y, z);
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
            MarkRegionDirty(new Vector3Int(x, y, z));
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

        /// <summary>
        /// Applies the current preview changes
        /// </summary>
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

            previewModelWithOldData.ChangeGridSizeIfNeeded(resolutionX, resolutionY, resolutionZ, false); // ToDo: Check when chaning grid size

            GenerateAndUpdateViewChunks(false);
        }
    }
}
#endif