# Unity art integration contract

## Verified architecture

Primary scene: `Assets/Game/Scenes/CoastalDefense_TopDown.unity`. Keep Unity pinned to `ProjectSettings/ProjectVersion.txt` (6000.3.24f1 at setup). The separate `CoastalDefense_Prototype.unity` is a regression scene; never load both gameplay scenes additively.

Gameplay is XY, camera at negative Z looking along +Z, orthographic and unrotated. The map is 32 x 18 cells, each 1.25 world units. Sorting layers are Environment, GroundObjects, Enemies, Defenses, Effects in that order. The camera uses the existing URP 2D renderer. Preserve these and the current unlit material behavior.

`EnemyFacing` rotates the `Visual` child, with sprites authored facing local +X. `TurretPresentation` rotates `Rotating Weapon`, uses its `Muzzle` child, and reuses flash/tracer renderers. Keep gameplay roots unrotated and state in existing owners. The shared `DefenseDefinition`, `BuildManager`, and `DefenseBase` retain placement and footprint ownership. The four current defense footprints are 1 x 1.

## Pixel and importer specification

The current 16 x 16 placeholders import at 16 pixels per unit (PPU) and use deliberately nonuniform child scales. They are a fallback, not an approved final-art resolution.

**Phase 3A-1 validated production baseline (2026-09-19):** the user's current brief recommends 64 x 64 terrain tiles. At the unchanged 1.25-unit cell size, use **51.2 PPU** for world exports at visual scale 1, so one 64 x 64 tile covers exactly one cell. This replaces the setup's unapproved 32-PPU / 40-pixel-cell proposal; master v002 was selected and the terrain pilot validated. A future Crawler silhouette at its existing approximately 0.65 x 0.46 world-unit size would occupy about 33 x 24 source pixels. No enemy production is authorized in this phase. A transparent canvas is not a collision footprint. Larger props use larger frames at the same PPU. Never rescale gameplay or change cell size to fit artwork. See `ART_HANDBOOK.md` and `ASSET_LIST_PHASE_3A1.md` for the current logical export targets, which must be distinguished from generated native PNG dimensions.

The existing camera fits the viewport dynamically; it is not proven to render at an integer pixel scale. Test at 1920 x 1080 and 2560 x 1440 and inspect moving/rotating pixels. If later pixel-perfect presentation is required, evaluate it as a focused presentation change; do not silently add a camera/package or alter movement now.

| Setting | Runtime pixel sprite default |
|---|---|
| Texture Type | Sprite (2D and UI) |
| Sprite Mode | Single for individual PNG, Multiple for runtime sheets |
| Filter Mode | Point (no filter) |
| Compression / Crunch | None / off; verify platform overrides |
| Mipmaps | Off |
| Wrap | Clamp; Repeat only for an intentionally repeatable environment texture |
| NPOT resizing | None; no silent power-of-two scaling |
| Max Size | At least the actual source sheet's longest edge; verify no importer downscale |
| Alpha | Input alpha retained; Alpha Is Transparency enabled where appropriate |
| Color | sRGB for color art |
| PPU | Recorded world baseline, consistent across each pack |
| Mesh | Full Rect initially, stable rectangular frames |
| Pivot | Center for overhead unit body; documented weapon rotation axis / tile anchor exceptions |
| Read/Write | Off unless a concrete runtime consumer needs pixels |
| Physics shape | Do not auto-add colliders or use generated outlines to change gameplay |

Master references and raw source sheets stay ordinary non-runtime textures until an export is selected. Do not apply a global sprite-import rule to references, old placeholders, or unrelated textures. Save import settings in `.meta`; establish reusable Unity Presets only after the pilot confirms final values. No automatic reimport/postprocessor is installed by this preparation step.

Official references: [Sprite import settings](https://docs.unity3d.com/6000.3/Documentation/Manual/texture-type-sprite.html), [2D Sprite package setup](https://docs.unity3d.com/6000.3/Documentation/Manual/sprite/renderer/install-2d-sprite-package.html). The table above defines project choices; verify available controls in the pinned Editor.

## Sprite sheets and animation

Use Unity's 2D Sprite editor for exact grid/rectangle slicing. Keep raw generations in SourceSheets and selected runtime exports under the subject category. A generated montage is not automatically a valid animation sheet.

Before slicing, record frame width/height, rows/columns, count, padding, gutters, frame order, pivot, action, direction, FPS, and looping. Require equal canvases, consistent subject scale, stable pivot and light, no clipping, and no unintended frame jitter. Specify row-major left-to-right/top-to-bottom frame order in the batch record; Unity rectangle coordinates originate at bottom-left, so top-row frame positions need conversion: `y = imageHeight - topOffset - frameHeight`. Gaps and offsets must be included. Inspect actual named sub-sprites before binding clips.

Use constant/discrete sprite keys in `.anim` clips; document the chosen FPS per action. Put clips/controllers in `Assets/Game/Animations/<category>/`. Animate the renderer below the presentation child, never the gameplay root. Keep root motion disabled; animations cannot drive movement, damage, targeting, rewards, mine detonation, or wave progression.

The initial integration can reuse one local-+X design rotated by existing facing code. Four/eight-direction sprites require an explicit presentation adapter; do not combine directional frame selection with existing continuous rotation accidentally. `EnemyFacing` and turret aiming must remain sole owners of their current transform rotations unless intentionally replaced at the presentation layer.

Existing enemies are destroyed on death, and mines remove themselves after detonation. A lingering death/explosion animation needs a separate presentation effect with independent lifetime; do not delay gameplay death, target removal, or rewards for a clip. Turret flash duration/tracer scale are currently code-controlled; preserve those semantics or adapt the presentation explicitly. In particular, the tracer is scaled as a one-world-unit-long source: a differently sized texture needs a presentation sizing adjustment.

No bone rigging, 2D Animation package, Aseprite importer, or external editor is necessary for the first frame-based PNG workflow. Animation authoring uses Unity's Animation window and Animator. Add specialist dependencies only when the requested format needs them.

## Prefabs and environment

- Inspect the saved prefab hierarchy and renderer references before editing. Use Unity Prefab APIs/Prefab Mode through the installed CLI/Pipeline. Preserve asset GUIDs, components, serialized data, root transforms, muzzle references, and gameplay footprints.
- Normalize or compensate only visual child scales when replacing stretched shape placeholders; do not put new detailed art under an old nonuniform scale without checking it. Check renderer tint too: colored placeholder tints can distort the final palette.
- Preserve sorting layer/order and local Z. Keep enough transparent margin for rotation. Verify muzzle position and aim along +X after replacement.
- Art may visually overhang a tile, but placement/navigation still uses the definition footprint. Keep buildability, blocked-cell preview, wire slowdown, and mine radius legible.
- Do not use the scene-rebuild commands as a routine art import step. `TopDownSceneBuilder` and phase setup helpers can restore placeholder scene layout or presentation overrides. Treat a rebuild as explicit destructive authoring work and review the concrete scope first. Ordinary art integration edits saved consumers.
- River craft belong to art/reference scope initially. Existing spawns and movement do not implement boat transport/disembarkation. Depicting a boat does not authorize adding that feature.

## UI integration

Runtime UI is UI Toolkit: `TopDownHud.uxml`, `TopDownHud.uss`, `PrototypeTheme.tss`, and `PrototypePanelSettings.asset` under `Assets/Game/UI/`. Raster art belongs under `Assets/Game/Art/UI/`. Keep prices, labels, wave counts and instructions as actual UI text, not baked into generated images.

`BuildHud` creates later defense buttons from the ordered `DefenseDefinition` catalog. It does not currently have an icon field. Attach art with a small presentation mapping keyed by existing defense IDs or USS classes when requested; do not hardcode defense-specific branches into placement or make UI own money. Preserve named UXML elements used by `BuildHud`, `WaveHud`, and `PrototypeHud`, their callbacks, affordability opacity, selection/disabled states, input cancellation, and Start Wave state rules.

Record logical UI dimensions, panel slice borders, and padding. Use UI Toolkit's supported background/nine-slice settings and inspect them in the pinned Editor. Preserve click targets and text contrast. Check selected/hover/pressed/disabled states with more than hue alone. Art scale and nine-slice borders should not stretch the pixel corners or obscure the playable map.

## Repeatable verification

1. Read `AGENTS.md`, repository status, the approved reference, register and affected consumers. Capture baseline screenshots/state if the batch changes presentation. Do not stage/reset other work.
2. Check `unity status --json`; target this project explicitly. Use `unity command` to discover supported operations. Enable `set_autotick` for an unfocused Editor. Use the installed `unity-cli` skill and `.agents/skills/unity-pipeline/SKILL.md`; package upgrades can change commands.
3. Import only the batch, verify importer and `.meta` settings, sub-sprite names and serialized references. Let Unity compile and inspect `recompile_status`, `editor_status`, and Console warnings/errors. Do not clear evidence before diagnosing a failure.
4. For prefab, UI, animation, or code changes, run relevant tests in the existing EditMode/PlayMode assemblies. Run the full current suites when a shared integration change warrants it. The CLI's live `run_tests` accepts `--mode editor` / `--mode playmode`; use `--async_tests true` plus `test_status` for long runs. Save actual terminal results.
5. Inspect the primary scene in Play Mode: water-left/base-right, readable spawns, movement/facing, build preview/placement, affordability, firing/muzzle, wire/mine effects, Start Wave, rewards, HP, Game Over, and clipping/sorting. Include animation loops and target resolutions when applicable. Root movement and state must agree with the baseline.
6. Review the full batch diff including metadata, package changes, prefabs and scene references. Save evidence under `Docs/Validation/<batch-id>/`; update register/batch records. Report unperformed checks and failures honestly. Restore only this batch's changes if an integration regression cannot be resolved.

With the Editor closed, an alternative is `unity test . --editor-version 6000.3.24f1 --mode EditMode --output TestResults/editmode.xml`, followed by PlayMode. Do not open a second Editor/test runner on a project already open in Unity.

## Phase 3A-1 delivered foundation

All 52 runtime sprites use the settings above. Five terrain renderers in CoastalDefense_TopDown use Tiled/Continuous, white tint and unit visual scale. Detailed sizes, the one-tile shoreline transition and rollback diff are in PHASE_3A1_REPORT.md and Docs/Validation/Phase3A1/terrain-scene.diff. Existing Square/Triangle assets remain available.

UI backgrounds have 16px sprite borders, with 20px content padding (16px for build buttons), verified in static nine-slice previews. UI Toolkit's background slice properties still need explicit application during the later UI integration; sprite import metadata alone does not bind the UI. UI remains live-text driven. Props/base remain imported inventory; no new prefab, collider or gameplay-root binding was introduced.

## Phase 3A-3 delivered defense presentation

All four existing defense prefabs now bind generated defense sprites at unit visual scale and 51.2 PPU. Their gameplay roots, definitions, 1 x 1 footprints, components, and GUIDs are unchanged. Full filenames, pivots, muzzle coordinates, state rules, slicing, and evidence are in `DEFENSE_ART_SPECS.md` and `PHASE_3A3_REPORT.md` at repository root.

MG/sniper reuse fixed Sandbag Base and rotating Receiver renderers; existing aiming owns Rotating Weapon. Muzzles are local +X at 0.5078125 / 0.6640625 units. `TurretPresentation` plays four MG frames at 20 FPS / three sniper frames at 12 FPS on the existing Fired event, then returns to idle. This bounded code-driven sequence needs no Animator, clips, animation events, or new state owner. Existing flash/tracer renderers remain Phase 3B placeholders.

`DefenseSpritePresentation` observes existing health (damaged at half maximum), death, and mine arming. It cannot prolong a dead defense's lifetime. Wire uses GroundObjects order 1 and mine order 2; MG/sniper bases/aiming use Defenses orders 0/2. Replaced body shapes are disabled and cleared. Existing mine explosion presentation remains unchanged.

The primary scene's existing placement object has `DefensePreviewPresentation`, which changes the existing ghost sprite after placement updates. MG/sniper use composites of their real idle layers; wire/mine use primary sprites. PlacementController retains tint, visibility, grid position, and validity. UI Toolkit controls and affordability/selection state remain unchanged; revised sniper icon and wire connectors are imported inventory.

Runtime import max size is 512; native references/sources are non-runtime textures. No gameplay, package, global import, camera, or project-setting changes accompany this integration. The live walkthrough and both test suites pass. Older scene/prefab setup helpers can still restore placeholders; continue editing saved consumers through Unity APIs.

## Phase 3B delivered combat and world presentation

The saved primary scene now owns one `VfxService` and `WorldWaterPresentation`, with `CombatFeedbackHud` on the existing UI Toolkit object. MG/sniper and mine presentation forward their existing events. The Crawler prefab binds the already-imported Basic Infected animation frames and a reusable wake; no new playable enemy was added. `EnemyHealth.Damaged` reports committed damage for visual observers. Damage/reward/movement/placement ownership remains unchanged.

World playback uses 256 prewarmed SpriteRenderer slots (96 burst, 64 tracer, 16 smoke, 48 decal, 32 detached death), plus at most 18 fixed shore strips and one wake per living enemy. UI rewards reuse 24 labels. Death art has a separate bounded visual lifetime after immediate gameplay removal. `CombatVfxCatalog.asset` contains the ordered frames and presentation settings; all 18 animated exports also have sprite-only clips, while runtime playback avoids per-effect Animators.

See repository-root `VFX_ART_SPECS.md` and `PHASE_3B_REPORT.md` for exact paths, pivot/FPS/sorting/lifetime settings, validated consumers, inventory-only effects, performance limits and test evidence. The reproducible Unity import/binding script is `Docs/Validation/Phase3B/ImportAndIntegrate.cs`. Use it only in Edit Mode with the primary scene active; it preserves existing asset GUIDs and sub-sprite IDs. Runtime imports use Point, 51.2 PPU, uncompressed hard-alpha RGBA and no mipmaps. Existing camera/sorting layers/material and gameplay definitions are unchanged.

## Phase 3C saved production consumers

The saved primary scene binds accepted environment/prop/base/UI inventory. `Docs/Validation/Phase3C/Integrate.cs` is a guarded, idempotent authoring receipt: run only with the clean saved primary scene in Edit Mode. It replaces only its named decorative root and uses PrefabUtility/SerializedObject/scene APIs. Exact decoration assets/transforms/orders are in `dressing.json`; do not run legacy scene builders over these production consumers.

The 155-renderer dressing root has no colliders, occupancy or navigation ownership. PlayerBase retains its original root, footprint and health; Command Post is a unit-scale 192x192 sprite on GroundObjects order 8. Crawler binds accepted hit frames in EnemySpritePresentation; its 0.2-second override cannot stop movement/attacks. Obsolete leaves are removed while required fallback flash/tracer/mine renderers remain disabled in the normal VfxService scene.

UI Toolkit retains existing control names and catalog order. BuildHud's separate displayOrder controls Wire/Mine/MG/Sniper layout; reenable removes old generated icons/buttons before subscribing again. HudArtPresentation observes EconomyManager/BaseHealth, assigns the original font and brief feedback, and unsubscribes on disable. CombatFeedbackHud uses the same font for wave/reward text. Panel/button slices are explicitly 16px in USS. Existing PanelSettings uses 1280x720 reference; actual 1080/1440 Game Views and every-cell picking were checked.

Camera rect `(0,.17,1,.69)` reserves HUD bands; padding is 0.6. No Pixel Perfect package or global sort change. Dynamic Point scaling is crisp but noninteger. Capture the actual composited Game View at the target resolution, including UI; requesting a resized capture alone does not test resolution. `ref_gameplay_integrated_01.png` is a non-runtime reference texture.

Audits/tests are under `Docs/Validation/Phase3C/`. Load/attack/Game Over fixtures run only in Play Mode; never save fixture placements/currency. Validation may migrate Time settings to Unity's rational format; compare and restore exact baseline ProjectSettings bytes afterward. Keep unsupported enemy/state/VFX inventory ready without adding events to show it. `PHASE_3C_REPORT.md` records performance and limits.
