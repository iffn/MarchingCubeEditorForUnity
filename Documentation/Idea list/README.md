# Marching cube editor
## Architecture
### Facade for implementing functions
```mermaid
graph LR
    %% User to Use Cases
    User["User"] --> Initialization["Initialization"]
    User --> Modifying["Modifying"]
    User --> HistoryManagement["History Management"]
    User --> Exporting["Exporting"]

    %% Use Cases to Editor
    Initialization --> Editor["Marching Cube Editor"]
    Modifying --> Editor
    HistoryManagement --> Editor
    Exporting --> Editor

    %% Editor to actions
    Editor --> InitializationAction["initialization"]
    Editor --> GridModification["Grid Modification"]
    Editor --> SaveLoad["Save and Load"]
    Editor --> UndoRedo["Undo and Redo"]
    Editor --> CopyPaste["Copy and Paste"]
    Editor --> ExportAction["Exporting"]

    %% Actions to GridController
    InitializationAction --> GridController["initialization"]
    GridModification <--> GridController["Grid Controller"]
    SaveLoad <--> GridController
    UndoRedo <--> GridController
    CopyPaste <--> GridController
    ExportAction <--> GridController

    %% GridController to Model and View
    GridController <--> Model["Marching Cubes Model"]
    GridController --> View["Marching Cubes View"]

    %% Styles
    style Initialization rx:15
    style Modifying rx:15
    style HistoryManagement rx:15
    style Exporting rx:15
    style InitializationAction rx:15
    style GridModification rx:15
    style SaveLoad rx:15
    style UndoRedo rx:15
    style CopyPaste rx:15
    style ExportAction rx:15
```

### Improved user experience
```mermaid
flowchart TD
    
    Start[Start] --> TryToFindController[Try to find controller]

    %% Initial controller assignment
    TryToFindController --> ControllerFoundCheck{Controller found?}
    ControllerFoundCheck -->|No| PromptUserToAddController(Prompt user to find controller)
    PromptUserToAddController --> UserAddsController([User adds controller])
    UserAddsController --> ControllerFoundCheck
    
    ControllerFoundCheck -->|Yes| InitializeController[Initialize controller]

    %% Main user action
    InitializeController --> UserMainActions{{User main actions in parallel}}
    
    %% Redefine controller
    UserMainActions -->|Redefine controller| AllowUserToRedefineController(Allow user to redefine controller)
    AllowUserToRedefineController --> UserAddsController
    
    %% Save and load
    UserMainActions -->|Save and load| AllowUserToDefineSaveFile(Allow user to define save file)
    AllowUserToDefineSaveFile --> UserDefinesSaveFile([User defines save file])
    UserDefinesSaveFile --> DisplaySaveAndLoadOptions(Display save and load options)
    DisplaySaveAndLoadOptions --> UserSaveAndLoadAction((User save and load action))
    UserSaveAndLoadAction --> |User clicks save| SaveData[Save data]
    UserSaveAndLoadAction --> |User clicks load| LoadData[Load data]
    
    %% Modify shape
    UserMainActions -->|Modify| SelectEditShape(Allow the user to select a modification shape)
    SelectEditShape --> UserSelectsEditShape([User selects edit shape])
    UserSelectsEditShape --> AllowUserToSelectModificationOption(Allow user to select modification option)
    AllowUserToSelectModificationOption --> UserModificationAction((User modification action))
    UserModificationAction --> |Use shape directly| UserEditsShape([User edits shape])
    UserEditsShape --> SelectAddOrSubrtactShape([User selects add or subtract shape])
    SelectAddOrSubrtactShape --> AddOrSubtractShape[Add or subtract shape]
    UserModificationAction --> |Add shape with raycast|RaycastToFindIntersection[Raycast to find intersection]
    RaycastToFindIntersection --> UserSelectsSimpleOrComplexPreview([User selects simple or complex preview])
    UserSelectsSimpleOrComplexPreview --> UserSelectsAddOrRemove([User indicates add or remove])
    UserSelectsAddOrRemove -->|If hit| DisplayPreview[Display preview]
    DisplayPreview --> UserSelectsApplyModification([User selects apply modification])
    UserSelectsApplyModification --> ModifyShape[Modify shape]
    ModifyShape --> RaycastToFindIntersection
```

### Better tool integration
```mermaid
classDiagram
    class MarchingCubeEditor {
        MarchingCubeEditor
        + DrawUI() : void
        + HanldeUIUpdate() : void
    }

    %% Base class for tools
    class BaseTool {
        + *DrawUI()* : void
        + *HanldeUIUpdate()* : void
        + *DrawGizmos()* : void
        + *OnEnable()* : void
        + *OnDisable()* : void
        # RaycastAtMousePosition() : Hit
    }
    <<abstract>> BaseTool

    %% Basic Tools
    class SimpleSceneModifyTool {
        - currentShape : EditShape
        - AddShape() : void
        - RemoveShape () : void
    }
    
    class SimpleClickToModifyTool {
        - currentShape : EditShape
        - showPreview : bool
        - limitHeight : bool
        - AddShape() : void
        - RemoveShape () : void
    }

    class ModifySurfaceTool {
        - size : float
        - type : enum -> makeSmooth, makeRough...
    }

    class CopyPasteTool {
        - currentShape : EditShape
    }

    %% Path Tools
    class PathElement {
        + CreateLine(start: Vector3, end: Vector3) void
        + SnapToGrid(position: Vector3) Vector3
        + VisualizeLine() void
    }

    class PathShape {

    }

    class TunnelAlongLineTool {
        
    }

    class PathAlongLineTool {
        
    }

    %% Relationships
    MarchingCubeEditor --> BaseTool : implements
    
    BaseTool <|-- SimpleSceneModifyTool
    BaseTool <|-- SimpleClickToModifyTool
    BaseTool <|-- ModifySurfaceTool
    BaseTool <|-- CopyPasteTool
    BaseTool <|-- TunnelAlongLineTool
    BaseTool <|-- PathAlongLineTool

    SimpleSceneModifyTool --> EditShape : uses
    SimpleClickToModifyTool --> EditShape : uses
    ModifySurfaceTool --> EditShape : uses
    CopyPasteTool --> EditShape : uses

    PathAlongLineTool --> PathElement : uses
    PathElTunnelAlongLineToolement --> PathElement : uses
    TunnelAlongLineTool --> PathShape : uses
    PathAlongLineTool --> PathShape : uses
```

## Implementation elements
### Save and load


### Editing tools
#### Distance funciton shape
Shapes can be defiend using distance functions. These can be used within the voxel grid to calculate the influence over different data points. Various different tools can be defined by using different mathematical functions.
Simple shapes like:
- Sphere
- Cube

Smart shapes like:
- Tunnel
- Terrain heightmap
- Limits (like height limit)

#### Add and remove shape
Shapes can be added or removed by setting the new value based on the distance function.

#### Copy paste shape

#### Move shape

#### Scale shape

#### Edit by click and hold to and and remove


### Optimizations
#### Chunks with same resolution for faster editing

#### Chunks with different resolution for reduced visual load
-> Border region apparently difficult to handle


### Materials
#### Triplanar shader
Creating good UV maps for Marching cubes is likely not very feasible. Therefore, triplanar shaders should work best.

#### Auto textures
For most natural elements, automatically drawing the texture based on the normal direction works quite well. So, steep parts would have rocks while flat parts would have grass. Close to the sea level, sand can be used instead.
However, this means that the top of even steep rocks would be covered by sand or grass.

#### Material type grid
To give more control over the visual effects, the material type could be drawn into the grid. This could then be applied to the vertex color, which the shader can look up to set the correct material.


### Visual tools
#### Cuts