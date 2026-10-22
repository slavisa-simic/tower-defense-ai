# Art batch p3a3_defenses_001

Phase 3A-3, produced 2026-09-20–21. Status: **integrated and validated**, except connected-wire/UI inventory and explicitly superseded sources below. The user's Phase 3A-3 brief authorizes generation, inspection, regeneration, internal selection, normalization, import, and safe replacement of the four existing defense visuals. No separate per-candidate user approval is claimed.

## Scope, references, and generation receipts

Approved style anchor: `Assets/Game/Art/MasterReferences/bd_ref_danube_master_v002.png`, selected in Phase 3A-1. Existing bibles, production rules, environment/enemy art, and actual DefenseDefinitions/prefabs/grid informed this batch. The approved master and previous phase exports are unchanged.

Used the built-in ImageGen capability directly, sequentially for each defense and its edits. The tool did not expose a model revision or seed; neither is invented. All 29 native PNG outputs were copied byte-for-byte into `Assets/Game/Art/SourceSheets/p3a3_defenses_001/`. The [final manifest](p3a3_final_manifest.json) records each original generator path, repository path, exact dimensions, mode, SHA-256, prompt path, and byte-identity result. It also catalogs all 24 runtime exports with source, dimensions, pivot, FPS, PPU, consumer, and status.

Exact prompts are in [p3a3_lineup_prompt.txt](p3a3_lineup_prompt.txt) and [p3a3_prompts](p3a3_prompts). Subject-edit prompts describe image 1 as the approved style master, image 2 as the selected defense lineup, and subsequent images as the subject/design or previous state to preserve. Production began from internally coherent lineup v001; later lineup v002 records the sniper overhead correction. Early calls did not use the later v002 board.

| Source sequence | Reference role and internal disposition |
|---|---|
| `defense_lineup_v001` | Initial scale/material/silhouette board, generated before individual production. Superseded as final board by v002. |
| `mg_reference_v001` | Selected MG emplacement design, checked against master/lineup; anchor for fixed base and weapon production. |
| `mg_base_idle_v001` → `v002` | Same sandbag/mount design; v002 removes unwanted painted checkerboard. v001 rejected as export input. |
| `mg_base_damaged_v001`, `mg_base_destroyed_v001`, `mg_weapon_fire_sheet_v001` | Registered structure-state edits and isolated +X recoil weapon, retaining MG design continuity. |
| `sniper_reference_v001` → `v002` | v001 supports suggested oblique projection. Selected v002 hides the legs and uses an internal deck ladder hatch. |
| `sniper_base_idle_v001`, `damaged_v001`, `destroyed_v001`, `sniper_aim_fire_sheet_v001` | Corrected deck/marksman design, fixed base states, separate +X aiming/recoil. |
| `wire_reference_v001` | Selected concertina language; design anchor for primary and connected variants. |
| `wire_single_v001`, `horizontal_v001`, `vertical_v001`, four `corner_*_v001`, `damaged_v001`, `destroyed_v001` | Primary tile, useful future connectors, same-structure damage states. |
| `mine_reference_v001` | Selected compact worn olive mechanical mine. |
| `mine_idle_v001` → `v002`, `mine_armed_v001` | v002 corrects painted checkerboard; registered armed version preserves the silhouette. |
| `defense_lineup_v002` | Final board updates the sniper to the corrected reference; copied to requested `ref_defense_lineup_core_01.png`. |
| `sniper_icon_v002` | Only inconsistent prior icon revised; corrected deck/rifle. Prior icon and GUID preserved. |

Selected individual stills are copied into MasterReferences with `bd_ref_` filenames. Old sniper reference v001 remains superseded. References are not runtime sprites. Runtime v001 filenames follow repository conventions; raw iteration revisions are independent of runtime release numbers.

## Authorized deterministic export and art contract

The request explicitly requires canvas normalization, background removal, consistent frame alignment, and programmatic sheet packing. [export_phase3a3.py](../export_phase3a3.py) uses bundled Python with Pillow/NumPy for these operations, not substitute artwork generation. Native outputs remain untouched.

The exporter keys magenta, zeros transparent RGB, applies nearest-neighbor affine sampling around measured source pivots, clips source frame cells before sampling, and reduces the pack to a shared 48-color palette without dithering (40 sampled colors plus eight direction anchors). Registration and scale are shared within each state family/action; frames are not independently stretched to their bounds. MG/sniper previews composite their real idle layers. [p3a3_export_manifest.json](p3a3_export_manifest.json) stores exact source-space pivots/scales and hashes.

MG frames are 64 pixels; sniper 96; wire 64; mine 32; UI icon 64. World PPU 51.2; UI 100. All existing footprints remain 1 x 1. See [asset table](p3a3_asset_table.md) and [DEFENSE_ART_SPECS.md](../../../DEFENSE_ART_SPECS.md) for every path, size, state, pivot, muzzle, and consumer.

Upper-left light, short baked contact shading, restrained neutral rotating-layer shading. Binary alpha, at least two transparent pixels at all frame edges, no magenta residue in any export. Seven firing frames inspected individually and on light/dark backgrounds. One-row sheets, no gutters: four 64 x 64 MG / three 96 x 96 sniper frames, left-to-right. Non-looping 20/12 FPS, existing Fired event. No Animator, animation events, clips, or controllers. Mine armed state is static. No final VFX/audio.

## Imports and integration

Unity 6000.3.24f1, live Editor APIs through CLI/Pipeline. [ImportAndIntegrate.cs](../../Validation/Phase3A3/ImportAndIntegrate.cs) uses AssetDatabase, TextureImporter, sprite data providers, and prefab APIs. Scenes/prefabs were not manually rewritten.

Runtime: Sprite, Point, Uncompressed, no Crunch/mips, Clamp, NPOT None, sRGB, alpha transparency, Full Rect, center pivot, Read/Write off, no generated physics shapes, max size 512. Raw/reference textures are non-runtime, max size 4096. No unrelated import rules or platform overrides were changed.

The four existing defense prefabs and existing primary-scene placement object are bound. Visual children have unit scale/white tint; replaced body renderers are disabled/cleared. DefenseSpritePresentation observes health/arming, TurretPresentation plays recoil, DefensePreviewPresentation observes selection. Gameplay roots/GUIDs/components/definitions/economy/targeting/damage/navigation/wire/mine/waves remain unchanged.

Connected wire and the revised sniper UI icon are inventory. Destroyed assets are bound to death but do not persist after existing immediate destruction. MG/wire/mine icons are unchanged. No UI bindings/layout, gameplay manager, prefab clone, package, camera, or project setting was added.

## Verification and final disposition

- Compile passed; 42/42 EditMode and 22/22 PlayMode passed, none skipped.
- Live primary-scene walkthrough passed 45/45 checks: ghosts/tints, placements/costs/footprints, fixed bases/aim/recoil/idle/muzzles, damage, wire slowdown, mine removal, waves, Game Over.
- Visual review at 1080p/1440p, light/dark alpha backgrounds, seven firing frames, grass/mud/wet-bank/bunker contexts beside Phase 3A-2 enemy art. This was agent-operated live Editor validation, not a human-input playtest.
- Final Editor: primary scene, Edit Mode, scene clean, no compile failure, zero current Console errors/warnings. Historical warning/expected test exception diagnosed in report.
- All 24 runtime exports pass pixel QA. All 29 native outputs match original generator hashes.
- Baseline audit: 627 pre-existing engine/content files, 621 unchanged, exactly six intended presentation files modified, none missing. Task-only diffs reviewed; user work preserved.
- No standalone build/hardware performance profile. Dynamic camera is not guaranteed pixel-perfect. Existing VFX await Phase 3B. Connected wire and UI binding are future presentation work.

Evidence: [Docs/Validation/Phase3A3](../../Validation/Phase3A3), including import/compile/test results, final editor inspection, live checks, screenshots, palette/contact sheets, GIFs, six diffs, baseline, and integrity audit. Initial live-check result is retained with the corrected unfocused-Player timing explanation in [report](../../../PHASE_3A3_REPORT.md); production settings are unchanged.

[ASSET_REGISTER.md](../ASSET_REGISTER.md) contains complete native/runtime inventory and status. Phase 3B is ready but has not started.
