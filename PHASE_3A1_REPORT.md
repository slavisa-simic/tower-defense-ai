# Phase 3A-1 report

2026-09-19. MASTER STYLE + ENVIRONMENT + PROPS + UI FOUNDATION.

## 1. STATUS

**COMPLETE.** Approved master v002 and all **52 mandatory runtime sprites** are present, exported at exact logical dimensions, visually reviewed and imported in Unity 6000.3.24f1. Five terrain placeholders in the primary scene now use the environment pack.

Master selection evidence: the user replied "continue" after v002 was displayed and the explicit selection question. The user also explicitly authorized deterministic exports with "Yes—prepare exact game-ready pixel exports." Production acceptance records do not imply separate user approval of every individual PNG.

## 2. Master references generated

- Assets/Game/Art/MasterReferences/bd_ref_danube_master_v001.png — superseded for facade/noise defects; provenance only.
- Assets/Game/Art/MasterReferences/bd_ref_danube_master_v002.png — selected style anchor, 1536x1024 RGB.
- Immutable selected SHA-256: 354421E55C341BFBCC65993BA20C644842EEC44073CD3BBB7EB3B7794F7D7E45.
- Both native sources are retained under SourceSheets/p3a1_master_001; exact prompts and review are in Docs/Art/Batches/p3a1_master_001.md.

The reference remains a style board, not a runtime atlas. Its illustrative subjects are not counted as delivered pack assets.

## 3. Environment assets generated

**10/10:** deep Danube water, shallow water, muddy shore, wet-bank transition, grass, dirt, cracked ground, bunker floor, concrete and road. All are opaque 64x64 RGBA exports at 51.2 PPU. Eight repeat XY; wet bank repeats Y with water left/mud right; road repeats X.

Native sources were generated separately from master v002. The authorized export process uses nearest-neighbor sizing, restrained material palettes and paired edge repair. Declared outer edges match exactly; 3x3 repeat previews and camera-scale appearance were reviewed.

## 4. Props generated

**17/17:** intact/destroyed rubber boat, sandbag segment, wooden/metal crates, oil/military barrels, spotlight, antenna, radar, Czech hedgehog, visual barbed barrier, scrap, wrecked supply box, floodlight, warning sign and quarantine fence.

Exact canvases range from 32x32 to 192x96, all at 51.2 PPU with real binary alpha and two-pixel minimum margin. Boats point right. Barrels show circular tops. Barriers, boats and equipment remain visual inventory and add no transport, lighting, collision or navigation behavior.

## 5. Base/fortification assets generated

**10/10:** bunker corner, wall, gate, watch post, command post, military tent, generator, ammunition crate cluster, empty sandbag nest base and checkpoint marker.

Roof/top surfaces use the approved palette; the nest opens right with a transparent center. The 192x192 command-post canvas fits the existing 3x3 visual envelope at 51.2 PPU. No base prefab was replaced.

Visual wall connection anchors are measured in Docs/Validation/Phase3A1/base-connection-qa.json. The corner end is 17 pixels thick and straight segment is 16; a three-pixel cap overlap produces the reviewed join in wall-connection-preview.png. These are visual attachment guides, not grid occupancy definitions or an autotile system.

## 6. UI foundation assets generated

**15/15:** three HUD panels; empty, selected and disabled build-button backings; start-wave button; status panel; money, base HP, wave, MG nest, wire, mine and sniper icons.

Panels have quiet opaque interiors and transparent outer margins. Functional labels, prices and counters are not baked in. The three build-button alpha masks match exactly. Selected uses amber corner brackets; disabled uses a subdued dashed inner rim.

All eight panel/button backgrounds use recorded 16-pixel nine-slice borders, verified by preview with unchanged corner pixels. Content padding is 20 pixels, or 16 for build buttons; 96x96 is a useful later sliced button size for a 64x64 icon. UI imports use 100 PPU with explicit logical dimensions. Existing UI Toolkit layouts, text, affordability/selection logic and callbacks are unchanged. Actual UI Toolkit application of the new slice settings remains part of later UI integration.

## 7. Naming conventions used

bd_<category>_<subject>_vNNN.png, with ref/env/prop/base/ui/src category tokens. All initial runtime deliveries use v001. Native spotlight revisions reach v003 and money reaches v002; these revisions were corrected before initial runtime acceptance. No previously approved production sprite was overwritten.

ASSET_LIST_PHASE_3A1.md lists every delivered filename, dimension and alpha/repeat contract. Docs/Art/ASSET_REGISTER.md records every runtime, native-source and reference PNG.

## 8. Folder structure used

- Assets/Game/Art/MasterReferences/
- Assets/Game/Art/Environment/
- Assets/Game/Art/Props/
- Assets/Game/Art/Base/
- Assets/Game/Art/UI/
- Assets/Game/Art/SourceSheets/p3a1_<pack>_001/
- Docs/Art/Batches/ — exact prompts, references, manifests, receipts and decisions.
- Docs/Validation/Phase3A1/ — export/import evidence, contact previews, scene diff, tests, screenshots and audit.

The delivery contains 52 runtime PNGs, 57 preserved native source PNGs and two reference PNGs. All 57 source files match the original generator outputs byte-for-byte. Sources/references are ordinary non-runtime textures. No raw source is linked into gameplay.

## 9. Assets regenerated for consistency

Master v001 was revised to v002 for overhead projection and quieter terrain. Spotlight v001 had a painted checkerboard that contaminated its crop; v002 drifted into side view; v003 is the accepted direct-overhead version. Money v001 read as supply tags, so v002 uses simple round coins.

Some generator outputs were RGB despite requested transparency. The first accepted isolated sources use exterior neutral-matte flood removal; later sources use a solid magenta key, including holes. Dark chroma spill is removed before quantization, and removed matte RGB is zeroed to prevent palette contamination. Sources remain untouched. Runtime resizing is nearest-neighbor with no dithering, restrained palette quantization and hard alpha. Rectangular UI backings are fitted to their exact interior canvas; props retain source aspect ratio.

Canonical reproduction and evidence: Docs/Art/Batches/p3a1_final_manifest.json, Docs/Art/export_phase3a1.py, and Docs/Validation/Phase3A1/p3a1_final_manifest-exports.json. Intermediate exports/previews are historical and are superseded by the final manifest evidence.

## 10. Light scene integration and validation

Assets/Game/Scenes/CoastalDefense_TopDown.unity retains its roots, components, materials, positions, camera, sorting, grid and gameplay references. Only five existing terrain SpriteRenderers changed to white-tinted Tiled/Continuous sprites with unit visual scale:

| Renderer | Sprite material | Final world size |
|---|---|---|
| Ocean | Deep Danube water | 5 x 22.5 |
| Beach | Muddy shore | 2.5 x 22.5 |
| Grass Combat Field | Grass | 32.5 x 22.5 |
| Shallow Water | Shallow water | 0.6 x 22.5 |
| Shoreline | Wet-bank transition | 1.25 x 22.5 |

The shoreline's visual width changed from 0.11 to one 1.25-unit transition tile. Its center and sorting order are preserved; terrain classification/buildability is unchanged. Unity's automatic scale adjustment on switching draw mode was caught and corrected. The final scale evidence supersedes the initial integration snapshot.

Validation completed:

- 52/52 exact dimensions, RGBA contracts, minimum transparent margins and declared seams pass. Light/dark and integer-scale previews reviewed.
- All 104 selected source/runtime texture imports checked; five additional master/rejected source textures configured as non-runtime. Point, no compression/mipmaps, NPOT None, centered Full Rect sprites, input alpha, 51.2 world PPU / 100 UI PPU.
- **42/42 EditMode and 22/22 PlayMode tests passed**, zero failures/skips. Results saved in editmode-tests.json and playmode-tests.json.
- Primary-scene smoke: MG placement valid; money changed 500 to 350 and HUD text matched; wave started; sample had five live enemies, four pending, nine remaining, start disabled and base HP 100. All five terrain renderers retained unit scale and Environment sorting. Play Mode was stopped afterward; smoke changes were not saved.
- Camera captures reviewed at 1920x1080 and 2560x1440; live enemy/defense appearance checked in terrain-play-1080.png. These are camera-only captures, not screenshots of the UI Toolkit overlay.
- Recompile reports up_to_date; final Console ground truth has zero errors/warnings and no failed compilation. Historical messages include the existing Input Manager deprecation, an intentionally expected observer exception in the passing EditMode test, and an earlier rejected screenshot path invocation; the capture command was corrected.
- SHA-256 audit: **283 of 284 pre-existing Assets/Packages/ProjectSettings files are unchanged**. The sole changed file is the intended primary scene. All existing metadata/GUIDs, code, prefabs, gameplay data, packages and project settings are preserved. Saved scene diff contains only the five presentation edits.
- git diff --check passes; Git reports a pre-existing EnemySpawner CRLF normalization warning without changed file bytes.

Limits: no standalone player build, full new UI implementation, boat/disembarkation system or new animation was produced. The existing dynamically fitted camera is not pixel-perfect at every resolution; no camera/package change was introduced. Foundation terrain repeats are visible at map scale, and future bounded variation tiles may reduce repetition without changing this baseline.

## 11. What should be generated next

**Phase 3A-2 — Core Enemies + First Animation Sheets** can begin when requested. Use the selected master v002, the 64-pixel-cell / 51.2-PPU contract, local +X facing and existing EnemyFacing presentation ownership. Start with one bounded core-enemy animation pilot and validate frame/pivot consistency before expanding. No Phase 3A-2 assets were generated in this phase.

Real blocker: **none**.
