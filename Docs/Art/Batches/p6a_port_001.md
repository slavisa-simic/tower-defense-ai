# Phase 6A industrial port batch

Authorization: the Phase 6A request explicitly requires autonomous generation, reference selection, exports, integration and validation. Selection here is agent selection under that instruction, not independent human approval.

Reference: `Assets/Game/Art/MasterReferences/ref_industrial_port_map_01.png` (1536x1024), generated from approved Danube master v002. Original master/GUIDs preserved. Complete exact prompts: `p6a_port_001_prompts.json`.

Built-in ImageGen produced the reference, bounded 16-prop sheet, corrected overhead revision, four-material terrain sheet and industrial preview. Raw PNGs are retained byte-for-byte in `Assets/Game/Art/SourceSheets/p6a_port_001/`. Prop v001 was rejected for visible front/side faces; v002 removes warehouse fronts, side wheels and barrel sides. Only v002 is exported.

Exports use the existing authorized exact pixel-export workflow: `Docs/Validation/Phase6A/ArtAuthor.cs` slices row-major cells, finds alpha bounds, samples nearest-neighbor, uses binary alpha at .5, zeroes transparent RGB, and writes 16 128x128 prop canvases and four 64x64 opaque terrain canvases. Terrain cells are opaque and do not require alpha trimming. No gameplay dimensions are changed. Actual raw dimensions and source hashes are recorded in the validation manifest. Larger blocker visuals are fitted to authored grid rectangles; small equipment is decoration.

Import: Sprite Single, 51.2 PPU, centered pivot, Point, uncompressed, no mipmaps, NPOT None, retained binary alpha. Runtime paths: `Art/Props/bd_prop_port_00_v001.png` through `15`; `Art/Environment/bd_env_port_00_v001.png` through `03`. Source and reference textures have no runtime consumers.

Consumers: saved DanubeIndustrialPort scene, static Port Environment root. Terrain Environment order 0; roads order 1; decals/loose cargo GroundObjects order 2; blockers/equipment/checkpoint order 3–5. Static contact shadows reuse each object's silhouette at (+.04,-.04), dark .27 opacity with lower sort order, without shadow scripts. Existing gameplay roots/managers, prefabs, GUIDs, eight defense definitions, UI bindings, camera and sorting layers are retained.

Layout intentionally has three dock sectors, eight berths, three route families with cross connections, multiple cargo blocks, warehouse/service roofs, cranes, mooring ladders, sparse cargo/oil, reinforced checkpoint compounds and two inland roads. Visual footprints are distinct from decorative equipment. No LOS, interiors, crane gameplay or new balance table.

Validation and visual captures are in `Docs/Validation/Phase6A`; final results and limitations are recorded in `PHASE_6A_REPORT.md`. Generated previews are card illustrations; gameplay screenshots remain separate evidence.
