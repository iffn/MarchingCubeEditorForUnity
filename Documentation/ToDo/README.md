# Marching cube editor ToDo

## Tools
- Heightmap addition
  - [x] Implement
  - [ ] Fix wrong addition when world origin of heightmap not (0,0,0)
- Terrain conversion
  - [ ] Basic implementation
- Limit height to cursor
  - [x] Reimplement
  - [ ] Fix when prefab has offset
- Tunnel
  - [x] Basic implementation
  - [x] Preview with options
  - [ ] Better gizmos shape
  - [ ] Different tunnel cross sections
- [x] Path
- Surface modifications
  - [x] Basic implementation
  - [ ] Stop mostly removing material with GaussianSmoothingModifier
  - [ ] Implement other smoothing modifiers if needed
  - [x] Add basic roughen tool option
  - [ ] Add 3D voronoi roughen option
- [ ] Normal expansion
- Copy paste tool
  - [x] Basic implementation
  - [ ] Different boolean options
- [ ] Let tool GameObjects have their own UI

## Editor options
- [x] Select tool from UI
- [x] Link save file to prefab
- [x] Integrate into inspector
- [x] Expand voxel field
- [ ] Add material selection

## Result visualization
- Post process mesh (Chunk border normals, close by vertex merge, null area triangle merge)
  - [x] Basic implementation
  - [ ] Give more options (One chunk, smooth normals, remove narrow triangles)
  - [ ] Give time limits for processing
  - [ ] Set on build option
- [ ] Triplanar shader
- [ ] Create LOD level for Chunks (Low prio)

## Tool visualization
- [x] Distance function shader
- [x] Preview modification

## Editor visualization
- [ ] Visual cuts
- [ ] Normal direction shader

## Performance improvements
- [ ] Measure current mesh processing time
- [ ] Implement elemements (Like the preview) as a geometry shader (Low prio)

## Additional functionalities
- [x] Chunks
- [x] Coloring with weight and vertex colors
- [x] Improve the GameObject sorting inside the MarchingCube prefab
- [ ] Create testing concept (Low prio)

## Branch merge checklist
- [ ] Use preprocessor directives to allow building the project