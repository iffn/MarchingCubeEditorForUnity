# Architecture
This architectural overview describes how the program is layed out.
## Core
```mermaid
graph LR
    %% Core
    Controller["Controller<br><span style='font-size: 0.8em;'>Controls the implementation</span>"]
    Model["Model<br><span style='font-size: 0.8em;'>Holds voxel data</span>"]
    View["View<br><span style='font-size: 0.8em;'>Displays the mesh</span>"]

    Controller --> Model
    Controller --> View
```
The core is set up with a Model-View-Controller pattern.
- The model holds the voxel data.
- The view uses the model data to generate the mesh.
- The controller controls the interactions between the model and the view.

## User interaction
```mermaid
flowchart LR
    %% User interaction
    User["User"<br><span style='font-size: 0.8em;'>Interacts with the system]
    Editor["Editor"<br><span style='font-size: 0.8em;'>Allows modifications]
    User --> Editor
    Editor --> Controller
    
    %% Core
    Controller["Controller<br><span style='font-size: 0.8em;'>Controls the implementation</span>"]
    Model["Model<br><span style='font-size: 0.8em;'>Holds voxel data</span>"]
    View["View<br><span style='font-size: 0.8em;'>Displays the mesh</span>"]

    Controller --> Model
    Controller --> View
```
The goal of the system is that the user can modify the data. This is done by interacting with the editor component.

## Tool integration
```mermaid
flowchart LR
    %% <br><span style='font-size: 0.8em;'>

    %% User interaction
    User["User"<br><span style='font-size: 0.8em;'>Interacts with the system]
    Editor["Editor"<br><span style='font-size: 0.8em;'>Allows modifications]
    User --> Editor

    %% Tool connectino
    Tools["Tools<br><span style='font-size: 0.8em;'>Handle modification logic<br><br>- SimpleClickToModifyTool<br>- SimpleSceneModifyTool<br>- Bridge and tunnel tool<br>- SmoothingTool<br>- SimpleClickToPaintTool<br>- ExporterTool"]
    ActionManagers["ActionManagers<br><span style='font-size: 0.8em;'>Organize interactions<br><br>- ModificationManager<br>- SaveAndLoadManager<br>- VisualisationManager"]
    EditShape["EditShapes"<br><span style='font-size: 0.8em;'>Distance function in scene<br><br>- Box<br>- Sphere<br>- RockShape<br>- HeightmapShape<br>- BridgeOrTunnelShape<br>]

    Editor --> Tools
    EditShape --> Tools
    Tools --> ActionManagers
    ActionManagers --> Controller

    %% Core
    Controller["Controller<br><span style='font-size: 0.8em;'>Controls the implementation</span>"]
    Model["Model<br><span style='font-size: 0.8em;'>Holds voxel data</span>"]
    View["View<br><span style='font-size: 0.8em;'>Displays the mesh</span>"]

    Controller --> Model
    Controller --> View
```
In order to integrate the tools, 3 types of elements are used.
- The EditShapes contain the distance functions and are implemented as scene objects. This  allows to use the scene editor to display and modify them.
- The ActionManagers provided by the Controller to provide different functionalities to the Tools.
- The Tools are different ways how the voxel data can be modified or used. They are selected from and displayed by the editor. They use the EditShapes to control the modification location and the ActionManagers to interface with the voxel data.