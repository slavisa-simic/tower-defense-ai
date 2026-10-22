# Art batch record — p3a2_core_enemies_001

## Scope and source

- Phase 3A-2, 2026-09-19; user pasted brief requests Basic Infected, Runner and Brute reference, sprite and first-animation sets.
- Approved style image: `Assets/Game/Art/MasterReferences/bd_ref_danube_master_v002.png`; SHA-256 `354421E55C341BFBCC65993BA20C644842EEC44073CD3BBB7EB3B7794F7D7E45`; selection evidence retained in ASSET_REGISTER.
- User explicitly requests internal enemy-look review before animation; record agent technical selection separately from user approval.
- Existing environment/UI and camera-scale capture inspected. Unity 6000.3.24f1 connected on port 7800; Play Mode stopped.
- Built-in ImageGen is the production source. Exact prompts/inputs/returned paths/hashes will be stored alongside this record. No model/seed is asserted.
- Exact-pixel export process follows the user's recorded Phase 3A-1 authorization; source PNGs stay unmodified.

## Art and import contract

`ENEMY_ART_SPECS.md` owns the measured baseline, silhouettes, regional character cues, frame counts, export geometry and import settings. Planned deliverables: one lineup, three stills, fifteen sheets / eighty-four frames. Reject projection drift, fake alpha, inconsistent body size, clipping and grid contamination before import.

## Review and integration

Status: production. Lineup v001 technically selected by the agent under the user's requested internal review: coherent palette/materials, crown/shoulder overhead projection, east facing, recognizable bone-shirt Basic, narrow blue-gray Runner, broad armored Brute. The actual lineup is 1536x1024. It is a reference board, not a runtime atlas. Brute's final relative size is governed by the logical silhouette contract, not by illustrative board spacing. No separate user approval is asserted. No gameplay/prefab/scene replacement is required.

## Final acceptance — 2026-09-20

**Validated asset inventory complete:** four design references, fifteen transparent runtime sheets (84 frames), fifteen unbound sprite-only clips and twenty untouched native sources. The individual stills preserve the lineup's outfit/gear/palette and passed internal design review before animation. They remain opaque reference textures because their checkerboards are painted; no reference image is a runtime dependency.

Canonical source/prompt/export/import record: `p3a2_final_manifest.json`. It contains every native path, dimensions, SHA-256 and original returned file, exact generation prompts/local references, selected and rejected revisions, source rectangles, constant action scales, explicit frame pivots, logical outputs, named sprites and Unity IDs. The 48-color export palette, Point/51.2-PPU import settings and actual clip paths are recorded there. Source PNGs remain byte-identical; no model/seed is asserted.

Runner run v001 was rejected for a west-facing second row, regenerated as v002. Attack reach extraction and crown registration corrections are detailed in the phase report. Initial extra endpoint keys were removed after Unity playback timing validation; final clips have count keys and count/FPS duration. No root motion, animation events, controllers, gameplay definitions or scene/prefab/UI bindings were added.

Evidence under `Docs/Validation/Phase3A2/`: pixel-audit.json (15 sheets / 84 frames / 20 sources), unity-imports.json, clip-playback-validation.json (84 successful samples), 42 passing EditMode tests, 22 passing PlayMode tests, final compilation/Console and repository-integrity.json (511 unchanged pre-existing project files). Every frame/contact board and both offline camera-scale estimates were visually reviewed. Final primary scene is clean with Play Mode stopped, zero Console errors/warnings. No standalone build or in-game controller hookup was attempted.

Asset register updated. No blocker. Phase 3A-3 is ready to be requested; no unrelated batch began.
