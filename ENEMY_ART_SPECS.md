# Phase 3A-2 enemy art specifications

2026-09-19. Batch `p3a2_core_enemies_001`. Production uses the user-selected Danube master v002, the existing art/lore handbooks, and the requested internal enemy-look review before animation. Technical selection is not a claim of separate user approval.

## Scale and presentation

Strict perpendicular overhead XY; every living frame faces east/local +X. One 1.25-world-unit terrain cell is 64 pixels; all enemy exports use **51.2 PPU**, centered pivots and unit presentation scale. The current Crawler body is 0.65 x 0.46 units (about 33 x 24 pixels); its outline and facing tip extend further. Camera orthographic size is 11.85 at 16:9: logical sprites display at about 0.89 screen pixels per source pixel at 1080p and 1.19 at 1440p. The camera remains unchanged.

| Enemy | Role | Frame | Standing silhouette target | Identity cues |
|---|---|---|---|---|
| Basic Infected | Baseline; later visual candidate for existing Crawler | 64x64 | about 34x28 px | Former Levantine civilian: torn bone/beige long shirt, desaturated checked scarf, olive water bag, improvised forearm wrap; warm medium skin with limited burn accents |
| Runner | Planned fast fragile attacker; art only | 64x64 | about 32x22 px | Former West African urban delivery/work runner: faded blue-gray short work shirt, narrow striped shoulder textile, small rust-red travel sling, dark brown skin; lean forward reach |
| Brute | Planned slow heavy attacker; art only | 96x96 | about 54x46 px | Former North African industrial worker: torn sand-colored hooded outer garment, charcoal work trousers, salvaged olive shoulder protection, rope/water canister; enlarged shoulders and one swollen forearm |

Regional cues describe individual former humans, not an ethnic cause of infection. Clothing, travel gear and skin tones vary; anatomy and disease effects follow the shared fictional radiation event. No flags, religious caricature, fantasy anatomy, real-world hate symbols, or invented readable writing. No new enemy definitions or gameplay types are implemented.

Shared anchors: shadow #1F292C, mud #5A4633, soil #816445, grass #4B5538, olive #636B44, concrete #939486, steel #5A615D, rust #7F4833, bone #C1B19E, danger #934433. Skin and faded blue-gray cloth use restrained compatible shades. Crisp clusters, one logical pixel outline, 3-5 shades per material, overhead ambient lighting and no long rotating shadows.

## Animation contract

| Action | Frames | FPS | Loop | Motion |
|---|---:|---:|---|---|
| idle | 4 | 5 | yes | Restrained breathing/shoulder sway |
| walk (Basic/Brute) | 8 | 8 / 6 | yes | Alternating feet/arms, no root translation |
| run (Runner) | 8 | 12 | yes | Long alternating strides and arm drive |
| attack | 6 | 10 | no | Anticipate, reach/swing, contact pose, recover; visual only |
| hit | 2 | 10 | no | Recoil and recovery |
| death | 8 | 9 | no | Stagger, knees buckle, fall and settle; retained corpse endpoint |

28 frames per enemy / 84 total. Each runtime action sheet is a single horizontal row, no gutters, uniform frames. Frame `i` has Unity rectangle `(i*frameSize, 0, frameSize, frameSize)`, pivot `(0.5,0.5)`, stable semantic name `bd_enemy_<enemy>_<action>_e__fNNN`. At least two transparent border pixels in every frame. Uniform scaling across a character's actions; no individual bounding-box normalization that pumps the body size. Exact measured export transforms belong in the manifest.

Names: `bd_enemy_<basic_infected|runner|brute>_<action>_e_sheet_v001.png`. Native outputs remain byte-identical under `Assets/Game/Art/SourceSheets/p3a2_core_enemies_001/`. Selected references go under MasterReferences; runtime sheets under Enemies/BasicInfected, Enemies/Runner, Enemies/Brute. Review evidence and previews live under `Docs/Validation/Phase3A2/`.

Use the recorded exact-pixel export workflow from Phase 3A-1 (user authorization recorded in ASSET_REGISTER): nearest-neighbor sizing, restrained no-dither palette, hard real alpha, retained native sources. Any matte defect must be reported and corrected before acceptance. Source artwork is generated with built-in ImageGen; deterministic processing prepares its requested game-ready canvases.

## Unity preparation and boundaries

Sprite Multiple, Point, uncompressed, no mipmaps/Crunch, Clamp, NPOT None, sRGB, input alpha, Full Rect, 51.2 PPU, read/write off, no generated physics shapes; max size at least source size. Sources/references remain Default textures. Optional sprite-only `.anim` clips may target `Visual/Body` on a future presentation prefab, with discrete keys and no events/root motion. No controllers or prefab bindings in this batch. Existing EnemyFacing owns rotation. Existing immediate enemy death remains unchanged; later death playback needs a separate presentation lifetime.

Acceptance: inspect native sources, every logical frame, light/dark edges, loop continuity, fixed body scale/pivot, east facing, silhouette/value separation on existing terrain at 1080p/1440p. Unity imports must retain dimensions and all named frames. Record compilation/Console state and exact checks performed; scene/prefab/gameplay files must remain byte-identical.

## Delivered calibration and clips — 2026-09-20

All fifteen sheets and fifteen unbound clips are delivered. Initial idle silhouette bounds are 35x27 pixels (Basic), 34x26 (Runner) and 54x49 (Brute). The original target dimensions above are approximate design targets. A transparent frame remains 64/64/96 pixels at the common 51.2 PPU.

Final registration is explicit in `Docs/Art/Batches/p3a2_animation_manifest.json`: measured crown rectangles, 10/9/14-pixel target crown widths, one scale per action, source pivots, and recorded one-pixel breathing/gait bob, recoil and eastward death-settle offsets. Brute braces backward slightly in its maximum strike pose to keep the reaching hand within the 96-pixel canvas. No per-frame resizing or synthesized artwork is used. Runner/Brute attack source extraction rectangles depart from the nominal native grid where extended hands require more room; runtime grids remain exact.

Clips are under `Assets/Game/Animations/Enemies/<enemy-folder>/`, use exactly one sprite key per frame at i/FPS, and match the duration count/FPS. Unity includes a final sprite-frame hold automatically; do not append a duplicate endpoint key. Future integration must normalize the existing Body's nonuniform placeholder scale to unit scale and its tint to white, and review placeholder outline/tip presentation separately. This phase does not change those consumers.

Playback and pixel checks passed; see `PHASE_3A2_REPORT.md`. The three opaque individual master stills contain painted checkerboards and are reference-only. Use the actual RGBA runtime sheets for rendering.
