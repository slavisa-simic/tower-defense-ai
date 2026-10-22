# Art batch p3a1_master_001

Phase 3A-1, generated 2026-09-18 and selected 2026-09-19. Status: **master v002 approved for the requested packs**. Approval evidence: user replied "continue" after the image and explicit approval question. Earlier candidate/review observations below are retained as history; v001 remains superseded. No further master approval is needed.

## Scope, references and exact prompts

The current user brief authorizes master, environment (10), props (17), base (10) and UI foundation (15), in that sequence. This batch creates a master board in two revisions. Dependent packs use v002 following the recorded 2026-09-19 selection. The current request excludes the setup's generic Crawler/defense pilot.

v001 input: Docs/Validation/Phase3A1/baseline-camera.png, 1920x1080, inspected with view_image, live scene composition only. v002 input: saved local bd_ref_danube_master_v001.png, inspected with view_image, edit target. Both calls use built-in image_gen with local referenced_image_paths; no CLI/API fallback. No seed/model identity asserted.

Exact complete prompts:
- Docs/Art/Batches/p3a1_master_001.prompt.txt
- Docs/Art/Batches/p3a1_master_001.v002.prompt.txt

Native returned paths:
- C:/Users/Slavisa/.codex/generated_images/01a0b1b5-5187-73b1-ab47-d87b085ef564/exec-318df4e2-6888-4846-bab0-8ad97a96510d.png
- C:/Users/Slavisa/.codex/generated_images/01a0b1b5-5187-73b1-ab47-d87b085ef564/exec-f28ba272-cce6-4fbc-adfe-23befc23b4d7.png

Project paths:
- Assets/Game/Art/MasterReferences/bd_ref_danube_master_v001.png
- Assets/Game/Art/MasterReferences/bd_ref_danube_master_v002.png
- Assets/Game/Art/SourceSheets/p3a1_master_001/bd_src_danube_master_v001.png
- Assets/Game/Art/SourceSheets/p3a1_master_001/bd_src_danube_master_v002.png

Both actual native sizes: 1536x1024, PNG color type 2 / 24-bit RGB, intentionally opaque, no alpha channel. No crop, resize, palette conversion, extraction or runtime export performed. Raw/reference pairs match byte-for-byte.

| Revision | Bytes | SHA-256 for both copies |
|---|---:|---|
| v001 | 2237857 | 6D846F1EB59D21A1B1C0CC216F351C13C69349EDE99061AEE9E663E584AAB393 |
| v002 | 1971434 | 354421E55C341BFBCC65993BA20C644842EEC44073CD3BBB7EB3B7794F7D7E45 |

## Contract and review

Opaque landscape board, overhead battlefield and lower materials/silhouettes/UI strip. Future logical tile target 64x64 per 1.25-unit cell (51.2 PPU), pending pilot.

v001 established palette and composition but retained a bunker front facade, side-view footer, front-facing UI icon and noisy terrain. It is superseded and must not guide production. Retained only for provenance/comparison.

v002 corrected the major roof projection, removed the footer landscape, replaced the UI icon with an overhead roof, and quieted terrain. It preserves river-left/base-right geography, pale infected silhouettes, MG/wire/mine/sniper examples, quarantine gate and restrained military UI. Its illustrated subjects are reference studies, not separate sprite assets or animation frames.

Remaining limitations: this is not a measured tile atlas or exact gameplay map. Minor props, notably the reddish barrel, still have side-face shading that must be excluded from production exports. Board text/insignia are illustrative. Exact logical pixel alignment, tile seams, alpha and gameplay-size production readability remain unvalidated. Do not extract runtime assets from the board or count subjects as completed pack entries.

Actual v002 swatch samples, top-left coordinates at y=768:

| Material | x | RGB hex |
|---|---:|---|
| Shadow | 424 | #1F292C |
| Deep water | 487 | #17444C |
| Shallow water | 551 | #2D5C5D |
| Wet mud | 614 | #5A4633 |
| Dry soil | 676 | #816445 |
| Grass | 740 | #4B5538 |
| Olive | 804 | #636B44 |
| Concrete | 868 | #939486 |
| Steel | 930 | #5A615D |
| Rust | 993 | #7F4833 |
| Bone | 1055 | #C1B19E |
| Amber | 1120 | #C18A3E |
| Danger | 1182 | #934433 |

The 13 sampled anchors differ from requested prompt targets; board contains additional shades. Sampling was read-only.

## Import and validation

All four imported through live Unity 6000.3.24f1 / AssetDatabase: Default texture, Point, Uncompressed, Crunch off, mipmaps off, Clamp, NPOT None, sRGB, read/write off, max 4096, input-alpha policy, alphaIsTransparency false for RGB. Loaded dimensions verified 1536x1024. No PPU/pivot/slicing/nine-slice/animation/runtime consumers apply to reference textures.

Docs/Validation/Phase3A1/ImportMaster.cs and InspectScene.cs ran through unity command run_script, compiled in memory without diagnostics. Helpers live outside Assets and add no runtime code.

Evidence under Docs/Validation/Phase3A1/: image-metadata.json, palette-v002.json, master-imports.json, scene-inspection.json, baseline-camera.png, editor-status.json, compile-before.json, compile-after.json, console-before.json, console-after.json.

Unity reports no active compilation failure, zero current Console errors/warnings. Historical Input Manager deprecation warning and expected observer-failure test exception predate this batch; retained in console-history-before.json. No new gameplay recompile/test run for reference-only files. No PlayMode, sprite alpha/seam, moving-pixel or target-resolution runtime art review claimed.

The master-only batch changed no scene/prefab/UXML/USS/gameplay/data/footprint/sorting and added no runtime consumer. Pre-batch Assets/Packages/ProjectSettings files were audited by SHA-256. The user subsequently selected v002 on 2026-09-19; dependent production and its separate terrain integration are recorded in the pack batches and PHASE_3A1_REPORT.md.
