# Phase 3C.1 bounded polish batch — 2026-10-01

## Scope and source

The attached Phase 3C.1 request authorizes autonomous overhead corrections, vegetation, shadows, one rubber landing craft, wire variants and runtime integration. Existing master v002 approval and exact-pixel export authorization are recorded in ASSET_REGISTER. No new reference approval is claimed. Technical selection/integration follows the current request.

Approved style reference: `Assets/Game/Art/MasterReferences/bd_ref_danube_master_v002.png` (approval/hash unchanged). Supporting wire reference: existing `bd_defense_wire_horizontal_v001.png`. Both were visually inspected before generation. Reused overhead boat: `Assets/Game/Art/Props/bd_prop_rubber_boat_v001.png`.

Built-in ImageGen, five calls, no API fallback or claimed seed/model. Exact prompts: `p3c1_reedPrompt.txt`, `p3c1_reedRevisionPrompt.txt`, `p3c1_bushPrompt.txt`, `p3c1_grassPrompt.txt`, `p3c1_wirePrompt.txt` in this folder. Returned images were copied intact from `C:/Users/Slavisa/.codex/generated_images/01a0f8ee-f094-7f91-a855-96b52454f3ab/`:

| Raw project source | Returned filename | Selection |
|---|---|---|
| `Assets/Game/Art/SourceSheets/p3c1_polish_001/bd_src_reeds_v001.png` | `exec-b16777c5-e2f5-4ce4-a9e5-a59762cda26b.png` | Rejected: upright stalks read from side |
| `Assets/Game/Art/SourceSheets/p3c1_polish_001/bd_src_reeds_v002.png` | `exec-83da2e48-91cb-4ba3-9356-41078a94e705.png` | Selected overhead rosette |
| `Assets/Game/Art/SourceSheets/p3c1_polish_001/bd_src_shrub_v001.png` | `exec-61e3fc51-2d8e-48f0-a984-333c81aedc36.png` | Selected overhead crown |
| `Assets/Game/Art/SourceSheets/p3c1_polish_001/bd_src_grass_v001.png` | `exec-b814199a-3aa6-474f-92c0-2880a8de33bb.png` | Selected radial tuft |
| `Assets/Game/Art/SourceSheets/p3c1_polish_001/bd_src_wire_cross_v001.png` | `exec-60e1e370-94e4-46aa-90d3-db2e10fcecf3.png` | Selected cross/arm source for exact connected exports |

Actual native dimensions and SHA-256 hashes: `Docs/Validation/Phase3C1/asset-audit.json`. Export inventory: `p3c1_exports.json`. No source PNG was overwritten. Existing runtime PNGs and GUIDs were retained; new wire v002 siblings replace prefab bindings within the requested connected-wire scope.

## Export and import contract

Reproducible Unity C# exporter: `Docs/Validation/Phase3C1/ExportAndIntegrate.cs`, `Phase3C1Author.Export`. Nearest pixel sampling of solid-alpha bounds (alpha >=240); 5-bit channel reduction without dithering, hard alpha, zero transparent RGB. Reeds/shrub 48x48 with 3px margins; grass 32x32 with 3px margins. Diffuse generator alpha/glow is excluded. Wire 64x64, canonical east half-arm mirrored/transposed to identical cardinal connections, center retained, outermost connected row/column matched to adjoining boundary. All 16 opposing horizontal and vertical masks were exhaustively compared (128 edge-pairs).

Shared contact ellipse is a small code-native SpriteRenderer support asset, 64x64, #1F292C with two pixel-clustered alpha steps (100/180) inside the compact disk. Renderer scales produce footprint ellipses; contact opacity remains low. It is not a generated raster edit or a real-time shadow material.

Every runtime sprite: 51.2 PPU, centered pivot, Sprite Single / Full Rect, Point, uncompressed/Crunch off, mipmaps off, Clamp, NPOT None, sRGB, input alpha, no read/write or generated physics shapes; platform overrides cleared. Raw sources: Default non-runtime textures. No new animation clip/controller or UI art required.

## Integration and review

Six existing primary gameplay prefabs retain their GUIDs/roots/definitions: Crawler_TopDown, PlayerBase_TopDown and all four defenses. Presentation shadows and wire bindings change; MG/sniper rest rotation is left. Existing accepted overhead art remains. `RubberLandingBoat.prefab` owns renderers only (hull, five visual seats, contact shadow), with no enemy gameplay. The scene uses one BoatLandingSystem alongside WaveManager.

Scene: 119 saved deterministic vegetation props, zero decorative behaviors/colliders; river lanes clear, central sprites below gameplay, base apron cleared. Decorative water extension covers approach/return outside the original grid. Existing static intact boats are removed from primary dressing to avoid overlap with working arrivals; the existing wreck is moved entirely into water. Existing PNGs are kept for rollback.

Status: technically selected, integrated and visually validated; no invented separate human approval. Reviewed native/logical assets and real preparation/arrival/unloading/combat/topology screenshots at 1920x1080. Report and test/Console evidence live in `PHASE_3C1_REPORT.md` and `Docs/Validation/Phase3C1/`. Automatic checks cover alpha/imports, identical edge pixels, cardinal topology, placement/removal/ghost, route/registration, pending waves and Game Over. A rejected source, early stale captures and fixed authoring/capture issues are documented in the report rather than counted as final evidence.
