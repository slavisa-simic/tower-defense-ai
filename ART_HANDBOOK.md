# Border Defenders art handbook

## Durable Phase 3C.1 presentation conventions

- **STRICT TOP-DOWN only:** orthographic camera perpendicular to XY, identity pitch/yaw, no perspective facades, isometric diamonds or 3/4 props. In-plane Z rotation for aiming is valid; it must not introduce camera tilt. Audit camera and raster silhouettes separately. Keep correct overhead assets instead of regenerating them indiscriminately.
- **Contact shadows:** shared sprite/material, dark #1F292C, world-aligned offset (+0.04, -0.04) units, compact footprint. Default opacity .25-.30 multiplied by the shared clustered sprite alpha; mines .12 and wire .20. Avoid long casts and shadows baked into rotating weapons. Existing restrained baked contact shading may remain. Static props/vegetation have no shadow Update component; enemies carry a shadow under their unrotated gameplay root.
- **Vegetation:** deterministic saved decorative reeds at the river edge, sparse grass/shrub crowns in the combat field, cleared base apron. Strict overhead radial silhouettes, Point filtering at 51.2 PPU, hard-alpha export and only valid quarter-turn variants. No colliders, navigation changes, build occupancy or per-prop logic; render below mines/enemies. Landing lanes stay clear. Reject upright bouquet/side-view reed candidates.
- **Connected wire:** 64x64 logical canvases at 51.2 PPU, centered pivot; N=1, E=2, S=4, W=8. All 16 variants use the same central junction and canonical arm pixels. Every connected opposing edge must match RGBA pixel-for-pixel, including transparency. Arms reach the cell boundary, unlike isolated props' breathing margins. Changes refresh only the affected cell and its four neighbors. Connectivity has no effect on cost, occupancy, slowdown or traversal cost.
- **River arrival:** the Phase 3C.1 request explicitly authorizes one rubber landing craft and infected disembarkation. Boat seats are visual passengers; EnemySpawner remains the only enemy gameplay initializer. Decorative river water may extend beyond the authoritative grid to cover off-map approach/return space; it adds no navigable cells.

Phase record and validation: `PHASE_3C1_REPORT.md`, `Docs/Art/Batches/p3c1_polish_001.md`, `Docs/Validation/Phase3C1/`.

Phase 3A-1, continued 2026-09-19. Master v002 was selected by the user's "continue" response after the displayed revision and approval question. This is the production visual contract. Read with ART_PRODUCTION_RULES.md and Docs/Art/UNITY_INTEGRATION.md.

## Inspected baseline

Unity 6000.3.24f1. Primary scene: Assets/Game/Scenes/CoastalDefense_TopDown.unity. Live inspection found 16 roots, 55 sprite renderers, a clean saved scene and stopped Play Mode. The map is 32x18 cells at 1.25 world units: 40x22.5 units. Water occupies the left four columns, shore the next two, and base anchor (29,9) has a 3x3 footprint. Camera position (0,0,-10), rotation (0,0,0), orthographic size 11.85 at 16:9. Camera framing is dynamic and does not guarantee integer pixel scaling.

At initial inspection terrain consisted of stretched, tinted 16x16 Square sprites, plus Triangle placeholders elsewhere; source PPU is 16. This is not a tilemap. Do not insert a detailed tile into a field-sized stretched renderer. The camera-only baseline capture excludes overlay HUD: Docs/Validation/Phase3A1/baseline-camera.png. Scene and renderer evidence: Docs/Validation/Phase3A1/scene-inspection.json. Legacy CoastalDefense_Prototype is a separate XZ regression scene.

## Perspective and density

Strict perpendicular overhead orthographic XY. Roofs and upper surfaces, no horizon, perspective convergence, visible tall front facades, isometric diamonds or side-view characters. Gameplay water remains left and base right.

The requested production baseline is **64x64 pixels per 1.25-unit cell**, giving **51.2 PPU** at visual scale 1. This supersedes the setup's unapproved 32-PPU / 40-pixel-cell proposal for this phase. It does not change grid size, camera, collision, navigation or build footprints. All world exports share 51.2 PPU; canvas extent may cover multiple cells. Example: 192x96 boat = 3.75x1.875 units; a 192x192 base canvas = 3x3 cells. The existing Crawler body would be approximately 33x24 logical pixels; this is a future readability target, not authorization to produce enemies now.

These are logical export targets, not a promise that generated native PNGs have those dimensions. Record actual size. Do not disguise an oversized conceptual render as a 64x64 runtime sprite using PPU alone. Pixel cleanup or resampling must preserve the source and follow the project's image editing rules.

## Readability and material language

Muted blue-green Danube water, dark wet mud, desaturated olive grass and equipment, warm-gray concrete, dull steel, restrained rust and bone highlights. Keep broad low-contrast terrain clusters; reserve sharper silhouettes and higher contrast for enemies, defenses, interaction and threats. Avoid photographic noise and scattered one-pixel glitter. Shore transitions should make the water/land boundary clear without implying buildability changes.

Approved reference: Assets/Game/Art/MasterReferences/bd_ref_danube_master_v002.png. Actual sampled palette: shadow #1F292C, deep water #17444C, shallow water #2D5C5D, wet mud #5A4633, dry soil #816445, grass #4B5538, olive #636B44, concrete #939486, steel #5A615D, rust #7F4833, bone #C1B19E, amber #C18A3E, danger #934433. These are 13 sampled anchors, not a claim that the board contains only 13 colors. Exact source coordinates are in Docs/Validation/Phase3A1/palette-v002.json. Earlier prompt targets remain in the prompt file; generated shades differ from those requested.

Use one logical pixel of dark material-colored outline where it separates an interactive silhouette, two only for major structural borders. Limit each material to about 3-5 intentional shade steps. Ambient overhead lighting with slight upper-left accents on static structures; no long directional shadows baked into rotating bodies/weapons. No airbrush, bloom, glossy bevels, antialiasing or mixed pixel scales.

Infected examples use pale/dirty silhouettes, ragged shoulders and a restrained warm danger accent. They must remain visible over mud/grass without gore-driven noise. Reference depictions are scale studies only.

## Alpha, UI and naming

Terrain fill tiles are opaque and tested for repeats; isolated props, base pieces and icons require real transparent PNG alpha. No checkerboard baked into RGB. Keep at least 2 logical pixels of transparent breathing room around isolated shapes; roofs may use larger margins. Avoid opaque matte halos and clipping. Wet-bank transition defaults to an opaque water-to-mud tile; do not erase terrain behind it.

UI is restrained military instrumentation: dark charcoal/olive panels, bone functional text and amber interaction accents. Keep texture quiet behind text. Selected uses a border and corner markers; disabled uses reduced value/contrast and a distinct border treatment. Leave all labels, prices, money, HP and wave counters to live UI Toolkit text. No baked text in production panel/button/icon PNGs. Delivered panel/button borders are 16 logical pixels on each side, verified by static nine-slice preview; content padding is 20 pixels, or 16 for build buttons. Apply UI Toolkit slicing explicitly during later binding.

Names: bd_<category>_<subject>_<part-or-action>[_<direction>]_vNNN[__fNNN].png. Tokens ref/env/prop/base/ui/src; world directions use local +X for east. Preserve all existing prefab/data/code filenames and GUIDs. Full phase manifest: ASSET_LIST_PHASE_3A1.md.

## Import and acceptance

Runtime sprites: Sprite Single (Multiple only for explicitly sliced sheets), 51.2 world PPU, Point, compression None / Crunch off, mipmaps off, Clamp except proven repeating tiles, NPOT None, sRGB, Full Rect, centered pivots unless documented, no generated colliders, read/write off. Retain alpha, max size at least native dimensions, and inspect platform overrides. UI uses recorded logical sizing and verified slice borders. References/raw sources remain ordinary non-runtime textures, Point/uncompressed with no mipmaps, no runtime consumers.

Before promotion: inspect at native and gameplay size; test repeats in a 3x3 preview; check alpha over light/dark backgrounds; verify shared scale, orientation and silhouette. Imports must retain source size. Before optional terrain integration, capture a rollback baseline, preserve sorting (Environment/GroundObjects/Enemies/Defenses/Effects), use presentation-only children and validate in Unity. No full UI integration, animation, VFX, audio or gameplay changes in this phase.

## Phase 3A-1 export decision

User authorization: "Yes—prepare exact game-ready pixel exports." Native ImageGen PNGs remain byte-identical under SourceSheets. Runtime files use authorized nearest-neighbor fitting, no-dither palette reduction, paired terrain seam repair and binary alpha. The generator's RGB matte outputs are cleaned by exterior neutral flood removal or magenta keying including dark chroma spill; removed matte RGB is zeroed before palette quantization. UI backings are fitted to exact canvases. The final manifest and final export/import evidence supersede earlier candidates.

The primary scene now uses five tiled terrain sprites with white tint and unit visual scale; map/camera/grid/sorting remain unchanged. Final 52-asset dimensions/import settings and the unchanged-source hashes are recorded in Docs/Art/ASSET_REGISTER.md. See PHASE_3A1_REPORT.md for validated scene sizes and presentation limits.

## Phase 3C integrated presentation decisions

The implemented reference is `Assets/Game/Art/MasterReferences/ref_gameplay_integrated_01.png`, an actual 1920x1080 wave-five gameplay capture. Use it with the approved Danube master v002 for future content. Accepted existing sprites provide muted terrain, quiet worn tracks/silt transitions, abandoned river boats and a restrained quarantine/base equipment perimeter around an open combat field. Decorative props are presentation-only and cannot imply occupancy or navigation rules.

Existing sorting layers remain sufficient. Terrain/props, Wire/Mine, readable enemies, fixed/aiming defense layers and bounded effects use stable explicit orders. The command post belongs on GroundObjects order 8 so live base attackers remain visible. No additional Y-sort system is needed for this map.

The camera reserves UI strips with viewport `(0, .17, 1, .69)` and 0.6-unit framing padding. Actual 1920x1080 and 2560x1440 preserve access to all grid cells. Retain 51.2 world PPU and Point filtering; dynamic fit is noninteger and is not claimed to be pixel-perfect integer scaling.

The HUD uses approved panel/button/icon language, explicit 16px slices and original 5x7 glyphs extended from the reward font. Live text/bindings remain functional. Money left, waves/enemies center, HP right; bottom display order Wire/Mine/MG/Sniper is independent of the catalog. Brief money/HP feedback and accepted hit poses are observers only. Consumers and validation: `PHASE_3C_REPORT.md` and `Docs/Art/Batches/p3c_integration_001.md`.
