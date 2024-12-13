# Class diagram
```mermaid
classDiagram
    class MarchingCubesController {
        - List~MarchingCubesView~ chunkViews
        - MarchingCubesModel previewModel
        - MarchingCubesView previewView
        - Vector3Int chunkSize
        - GameObject chunkPrefab
        + int GridResolutionX
        + int GridResolutionY
        + int GridResolutionZ
        + bool IsInitialized as Get
        + bool InvertAllNormals
        + bool EnableAllColliders
        + bool DisplayPreviewShape
        - MarchingCubesModel mainModel
        + float[,,] VoxelDataReference as Get
        + Vector3Int MaxGrid as Get
        + ModificationManager as Get, private Set
        + SaveAndLoadManager as Get, private Set
        + VisualisationManager as Get
        + Initialize(resolutionX: int, resolutionY: int, resolutionZ: int, setEmpty: bool) void
        + SetEmptyGrid(updateModel: bool) void
        + MarkRegionDirty(minGrid: Vector3Int, maxGrid: Vector3Int) void
        + UpdateAffectedChunks(minGrid: Vector3Int, maxGrid: Vector3Int) void
        + UpdateAllChunks() void
        + ApplyPreviewChanges() void
        + GetDataPoint(x: int, y: int, z: int) float
        + SetDataPoint(x: int, y: int, z: int, value: float)
        + SetupPreviewZone(minGrid: Vector3Int, maxGrid: Vector3Int)
        + SetPreviewPoint(x: int, y: int, z: int, value: float)
        + UpdatePreviewShape()
        + SetAllGridDataAndUpdateMesh(newData: float[,,])
    }
    <<MonoBehaviour>> MarchingCubesController

    class MarchingCubesView {
        - MeshFilter meshFilter
        - MeshCollider meshCollider
        - Vector3Int gridBoundsMin
        - Vector3Int gridBoundsMax
        - bool isDirty
        - bool invertedNormals
        + Initialize(start: Vector3Int, size: Vector3Int, collidersEnabled: bool) void
        + UpdateBounds(min: Vector3Int, max: Vector3Int) void
        + MarkDirty() void
        + UpdateMeshIfDirty(model: MarchingCubesModel) void
        + UpdateMesh(meshData: MarchingCubesMeshData) void
        + UpdateMesh(vertices: List~Vector3~, triangles: List~int~) void
        + bool InvertedNormals
        + bool ColliderEnabled
        + bool IsWithinBounds(minGrid: Vector3Int, maxGrid: Vector3Int) bool
        - GenerateChunkMesh(model: MarchingCubesModel) MarchingCubesMeshData
        - InvertMeshTriangles() void
        - OnDestroy() void
    }
    <<MonoBehaviour>> MarchingCubesView

    class MarchingCubesModel {
        + float[,,] VoxelData with get and private set
        + MarchingCubesModel(xResolution: int, yResolution: int, zResolution: int)
        + int ResolutionX
        + int ResolutionY
        + int ResolutionZ
        + int MaxGrid -> get, private set
        + SetVoxel(x: int, y: int, z: int, value: float) void
        + AddVoxel(x: int, y: int, z: int, value: float) void
        + SubtractVoxel(x: int, y: int, z: int, value: float) void
        + GetVoxel(x: int, y: int, z: int) float
        + GetVoxelData() float[,,]
        + GetCubeWeights(x: int, y: int, z: int) float[]
        + SetDataAndResizeIfNeeded(newData: float[,,]) void
        + ChangeGridSizeIfNeeded(resolutionX: int, resolutionY: int, resolutionZ: int, copyDataIfChanging: bool) void
        + CopyRegion(source: MarchingCubesModel, minGrid: Vector3Int, maxGrid: Vector3Int) void
        - IsInGrid(x: int, y: int, z: int) bool
    }

    class MarchingCubesMeshData {
        + List~Vector3~ vertices
        + List~int~ triangles
        - Dictionary~Vector3, int~ vertexCache
        + AddVertex(vertex: Vector3) int
        + AddTriangle(index1: int, index2: int, index3: int) void
        + Clear() void
    }

    class EditShape {
        - Matrix4x4 worldToLocalMatrix
        + PrecomputeTransform(gridTransform: Transform) void
        + OptimizedDistance(worldPoint: Vector3) float
        # DistanceOutsideIsPositive(localPoint: Vector3) float
        + Vector3 Position
        + Vector3 Scale
        - Material linkedMaterial
        + Color Color
        + GetLocalBoundingBox() Vector3~minOffset~, Vector3~maxOffset~
        + GetWorldBoundingBox() Vector3~worldMin~, Vector3~worldMax~
    }
    EditShape <|-- SphereShape
    EditShape <|-- CubeShape


    class ModificationManager {
        + ModifyData(shape: EditShape, modifier: IVoxelModifier)
        + ShowPreview(shape: EditShape, modifier: IVoxelModifier)
        + ApplyPreviewData()
        + HidePreview()
    }

    class SaveAndLoadManager {
        + SaveGridData(gridData: ScriptableObjectSaveData) void
        + LoadGridData(gridData: ScriptableObjectSaveData, updateColliders: bool) void
    }

    class VisualisationManager {
        + bool ShowOutline
        + bool InvertAllNormals as Set
    }
    <<MonoBehaviour>> VisualisationManager

    class MarchingCubeEditor {
        - MarchingCubesController linkedMarchingCubesController
        - ScriptableObjectSaveData linkedScriptableObjectSaveData
        - EditShape selectedShape
        - int gridResolutionX
        - int gridResolutionY
        - int gridResolutionZ
        - bool addingShape
        - bool limitMaxHeight
        - bool invertNormals
        - Vector3 originalShapePosition
        - Color additionColor
        - Color subtractionColor
        + static ShowWindow() void
        + OnGUI() void
        + InvertNormals(value: bool) void
        + LoadData() void
        + OnSceneGUI(sceneView: SceneView) void
    }
    <<EditorWindow>> MarchingCubeEditor

    class ScriptableObjectSaveData {
        - int resolutionX
        - int resolutionY
        - int resolutionZ
        - string packedData
        + SaveData(voxelValues: float[,,]) void
        + LoadData() float[,,]
    }
    <<ScriptableObject>> ScriptableObjectSaveData

    class IVoxelModifier {
        <<interface>>
        ModifyVoxel(x: int, y: int, z: int, currentValue: float, distance: float) float
    }

    class AddShapeModifier
    class SubtractShapeModifier
    class AddShapeWithMaxHeightModifier {
        - int maxHeight
        + AddShapeWithMaxHeightModifier(maxHeight: int)
    }
    IVoxelModifier <|.. AddShapeModifier
    IVoxelModifier <|.. SubtractShapeModifier
    IVoxelModifier <|.. AddShapeWithMaxHeightModifier

    ModificationManager --> IVoxelModifier : uses
    ModificationManager --> EditShape : uses
    MarchingCubeEditor ..> ModificationManager : controls
    MarchingCubeEditor ..> EditShape : uses
    MarchingCubeEditor ..> SaveAndLoadManager : controls
    MarchingCubeEditor ..> VisualisationManager : controls
    SaveAndLoadManager ..> ScriptableObjectSaveData : writes to and reads from
    MarchingCubesController --> MarchingCubesModel : modifies
    MarchingCubesController --> MarchingCubesView : controls 1...*
    MarchingCubesController --> MarchingCubesView : controls preview
    MarchingCubesController --> MarchingCubesModel : modifies preview
    MarchingCubesView --> MarchingCubesModel : reads from
    MarchingCubesView --> MarchingCubesMeshData : writes to
    MarchingCubesMeshData --> MarchingCubesView : provides
    MarchingCubesMeshData --> MarchingCubesModel : visualizes
    %% Relationships
    ModificationManager -- MarchingCubesController
    SaveAndLoadManager -- MarchingCubesController
    VisualisationManager -- MarchingCubesController

    %% Styling
    style ModificationManager fill:#005CFF
    style SaveAndLoadManager fill:#005CFF
    style VisualisationManager fill:#005CFF
```
