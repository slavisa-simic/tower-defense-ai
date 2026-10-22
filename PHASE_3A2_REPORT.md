# Phase 3A-2 report

Produced 2026-09-19 to 2026-09-20. CORE ENEMIES + FIRST ANIMATION SHEETS.

## 1. STATUS

**COMPLETE for the requested asset-production phase.** Three enemy sets, **15 transparent sheets / 84 frames / 15 unbound animation clips**, one shared lineup, three individual master stills and 20 preserved native PNGs are delivered. Gameplay animation/controller integration remains deferred as requested.

## 2. Master enemy lineup/reference generated

`Assets/Game/Art/MasterReferences/bd_ref_enemy_lineup_core_v001.png` is the selected 1536x1024 overhead lineup. The bone-shirt/scarf Basic, narrow blue-gray Runner and larger sand/olive Brute all face local +X and share the approved material/contamination language. Danube master v002 was supplied as a local style reference to every generation and remains unchanged.

Three individual 1254x1254 stills use `bd_ref_enemy_<enemy>_v001.png` in MasterReferences. They are opaque RGB design references: the generator painted checkerboards into their backgrounds. They are never used as runtime sprites. Actual runtime sheets have verified real transparency.

Enemy looks were technically selected under the brief's requested internal review; this is not a claim of separate per-file user approval.

## 3. Basic Infected assets generated

Idle, walk, attack, hit and death sheets, plus five clips. Torn beige long shirt, checked scarf, olive water satchel, wrapped forearms and restrained burns. Initial idle silhouette: **35x27 pixels**. Art lives in `Assets/Game/Art/Enemies/BasicInfected/`. This is a future visual candidate for existing Crawler; its definition/prefab was not renamed or replaced.

## 4. Runner assets generated

Idle, run, attack, hit and death sheets, plus five clips. Lean former urban delivery/work runner with blue-gray clothing, ochre shoulder textile, small rust sling and darker natural skin. Initial idle silhouette: **34x26 pixels**, with less filled area than Basic. Art lives in `Assets/Game/Art/Enemies/Runner/`. Run source v002 replaces the rejected v001. No Runner gameplay implementation was added.

## 5. Brute assets generated

Idle, walk, attack, hit and death sheets, plus five clips. Broad shoulders, torn sand-colored outer garment, olive improvised plate, rope/canister gear and enlarged wrapped right forearm. Initial idle silhouette: **54x49 pixels**. Art lives in `Assets/Game/Art/Enemies/Brute/`. No Brute gameplay implementation was added.

Regional clothing, equipment and skin variation describe distinct former humans within the fictional contamination premise. Designs remain grounded, serious and readable, without fantasy anatomy or caricature.

## 6. Chosen sprite sizes

| Enemy | Frame | PPU | Canvas in world units |
|---|---|---:|---|
| Basic Infected | 64x64 | 51.2 | 1.25x1.25 |
| Runner | 64x64 | 51.2 | 1.25x1.25 |
| Brute | 96x96 | 51.2 | 1.875x1.875 |

Transparent canvases do not change collision/navigation footprints. Camera, map and 1.25-unit grid remain unchanged. Native scale differences are corrected using a constant scale per action, calibrated to 10/9/14-pixel crown widths, and recorded per-frame registration. No frame is independently stretched to its bounds. Exports use nearest-neighbor resampling, a shared 48-color no-dither palette and binary alpha under the recorded exact-pixel workflow.

## 7. Chosen frame counts

| Action | Frames | FPS | Loop |
|---|---:|---|---|
| idle | 4 | 5 | yes |
| walk/run | 8 | Basic 8, Runner 12, Brute 6 | yes |
| attack | 6 | 10 | no |
| hit | 2 | 10 | no |
| death | 8 | 9 | no |

28 frames per enemy. Each sheet is one horizontal row, no gutters, equal frames and center pivots. Frame i rectangle: `(i*frameSize, 0, frameSize, frameSize)`. Exact crop rectangles, source pivots, scale factors, names and Unity IDs are in the final manifest. Each clip has exactly one discrete sprite key per frame.

## 8. Folder structure used

- `Assets/Game/Art/MasterReferences/` — four design references.
- `Assets/Game/Art/SourceSheets/p3a2_core_enemies_001/` — 20 untouched native PNGs, including one rejected source.
- `Assets/Game/Art/Enemies/BasicInfected/`, `Runner/`, `Brute/` — five runtime sheets each.
- `Assets/Game/Animations/Enemies/BasicInfected/`, `Runner/`, `Brute/` — five unbound clips each.
- `Docs/Art/Batches/` — exact prompts, receipts, batch record and canonical `p3a2_final_manifest.json`.
- `Docs/Validation/Phase3A2/` — frame/contact/GIF previews, measurements, pixel/import audits, playback checks, tests and file-integrity evidence.

Actual inventory is also recorded in `ASSET_LIST_PHASE_3A2.md`, `ENEMY_ART_SPECS.md` and `Docs/Art/ASSET_REGISTER.md`. Export/catalog helpers are under Docs/Art; live Editor import/validation scripts remain outside Assets and install no gameplay framework.

## 9. Naming conventions used

Runtime: `bd_enemy_<basic_infected|runner|brute>_<action>_e_sheet_v001.png`.

Sub-sprite: `bd_enemy_<enemy>_<action>_e__fNNN`, starting at 000. Clip: `bd_enemy_<enemy>_<action>_e_v001.anim`. References use `bd_ref_enemy_...`; native sources use `bd_src_enemy_...`. Runner's first accepted runtime export is v001, sourced from native run v002.

## 10. Regeneration and corrections

- Runner run v001 reverses west in frames 4-7. Regenerated v002 keeps all eight poses east and has stronger alternating limb movement. The rejected source remains registered.
- Reference stills returned painted checkerboards. They remain opaque references. Animation sources use a flat keyed backing; selected exports remove it, zero transparent RGB and preserve the native originals.
- Runner/Brute maximum attack poses extend beyond nominal native cells without touching neighboring artwork. Custom extraction rectangles preserve the hands and pack them into exact runtime frames.
- Crown measurement, constant action scaling and explicit registration correct montage size/placement differences. Brute's strike registration was adjusted to retain the required border.
- Initial clip authoring included an extra endpoint hold. Playback validation caught it; final clips have the correct key count and duration. The isolated sampling rig also needed an Animator component; the corrected rig passes all checks.

## 11. Unity import preparation and validation

Unity 6000.3.24f1. All 15 sheets are Sprite Multiple, 51.2 PPU, Point, uncompressed, mipmaps/Crunch off, Clamp, NPOT None, sRGB, Full Rect/center, input alpha, read/write off, max 4096, with no generated physics shapes. Platform overrides are cleared only for this batch. All 24 raw/reference textures are non-runtime Default textures. The importer reuses existing sprite IDs by semantic name.

Clips target only `Visual/Body:SpriteRenderer.m_Sprite`; no events, float/transform curves, controllers or prefab bindings. Later binding must normalize the placeholder Body's nonuniform scale/tint. Existing EnemyFacing owns rotation. Current immediate death behavior remains unchanged; lingering death playback needs an independent presentation lifetime.

Validation completed:

- **15/15 sheets and 84/84 unique action frames:** exact dimensions, RGBA/binary alpha, zero transparent RGB, no magenta residue and at least two clear border pixels pass.
- All **20 native PNGs** match original generator files byte-for-byte.
- All 84 Unity sprites have expected names, rectangles, pivots and PPU. **84 keys across 15 clips** sampled correctly on Animator-backed rigs in an isolated preview scene; transforms and active scene remained unchanged.
- Every logical frame reviewed on light/dark backgrounds. Offline camera-scale estimates at 1080p/1440p use existing grass, mud and concrete. Basic's light shirt, Runner's narrow blue-gray body and Brute's broad silhouette remain distinguishable. These are scale estimates, not gameplay screenshots. Per-action GIFs and `locomotion-preview.gif` are available.
- **42/42 EditMode and 22/22 PlayMode tests passed**, zero failures/skips. Results: `editmode-tests.json`, `playmode-tests.json`.
- Final compilation is up_to_date, compilationFailed false; final Console has **zero errors and zero warnings**. Earlier startup showed the existing Input Manager deprecation warning; no input/package change was introduced.
- SHA-256 audit: **511/511 pre-existing Assets/Packages/ProjectSettings files unchanged**, including scenes, metadata, foundation art, code, prefabs and data. Active scene is CoastalDefense_TopDown, clean, Play Mode stopped. `git diff --check` passes; the existing EnemySpawner CRLF notice does not reflect changed bytes.

Not performed: full controller/gameplay hookup, in-game playback of new skins, standalone player build or directional-sheet selection. These are outside this asset-first phase.

## 12. What should be generated next

**Phase 3A-3 — Defense Assets + Structure States can begin when requested**, using Danube master v002, the 64-pixel tile / 51.2-PPU baseline and existing defense architecture. No defense art, VFX, audio, UI polish, rebalance or new gameplay types were added here.

Real blocker: **none**.
