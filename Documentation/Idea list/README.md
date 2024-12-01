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