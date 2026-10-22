# Phase 3A-1 production pack acceptance

2026-09-19: all four packs meet this foundation delivery's acceptance checks. Master v002 was selected by the user; the user authorized deterministic pixel exports. This record is technical QA, not an assertion of separate user approval for every sprite.

## Environment — 10/10

Opaque 64x64 at 51.2 PPU. Palette and camera-scale contrast match the selected reference. Declared outer seams are equal; 3x3 repetition reviewed. Wet bank keeps water left and repeats vertically; road repeats horizontally. Five existing terrain renderers were updated and validated without gameplay changes. Repetition is visible on a large field; optional variation tiles remain future work.

## Props — 17/17

Real binary alpha and minimum two-pixel margin, centered pivot, common 51.2 PPU. Light/dark previews reviewed. Boats point right. Crates/barrels show tops; spotlight v003 repairs previous checkerboard/projection failures. Visual barrier/boat/lighting props add no gameplay systems. Rejected source revisions are retained only under SourceSheets.

## Base — 10/10

Roofs and upper surfaces, no front-facade structures. The command-post canvas is 192x192 / three cells. Sandbag nest has an empty transparent center. Corner/wall visual anchors and the three-pixel overlap join are measured in base-connection-qa.json and reviewed in wall-connection-preview.png. A one-pixel section variation is absorbed by the join. These are modular visual assets, not an autotile or occupancy system.

## UI — 15/15

No baked functional labels. Opaque quiet panel interiors with alpha outside. All three build-button alpha masks are identical. Selected brackets and disabled dashed rim differ in shape as well as color/value. Coin, HP, wave and four defense silhouettes reviewed at logical and integer-magnified sizes. Eight panel/button backgrounds have verified 16-pixel slice corners; static nine-slice previews retain those corners. Later UI Toolkit integration must apply the documented slice settings and preserve all live text/state/callbacks.

## Evidence and handoff

The final manifest is Docs/Art/Batches/p3a1_final_manifest.json. Exact source/native/export hashes, dimensions, alpha and importer settings are in Docs/Validation/Phase3A1/p3a1_final_manifest-exports.json and p3a1_final_manifest-imports.json. The 52-export audit passes. Docs/Art/ASSET_REGISTER.md lists every retained runtime, source and reference PNG. Full phase scope, 64 passing Unity tests, smoke check, diff review and real limits are in PHASE_3A1_REPORT.md.
