# Phase 4B heavy defenses and VFX

Scope: four new defenses and their bounded effects. Existing approved art remains untouched.

Reference: approved bd_ref_danube_master_v002.png, ref_gameplay_integrated_01.png and new ref_heavy_defense_lineup_01.png. The lineup is an agent-selected first-pass candidate under the user's explicit autonomous generation/integration authorization; no human approval is claimed.

Outputs: each defense has base, independently rotating local-+X weapon, four-frame firing sheet, damaged and destroyed sprites. Three heavy bases use 128x128 logical canvases and 2x2 footprints; Flamethrower uses 64x64 and 1x1. Weapon canvases match their bases. VFX source sheets contain separate equal-cell sequences for muzzle/projectile/explosion or flame/burn overlays. Raw PNG bytes retained; game-ready exports use nearest sampling, hard alpha and no-dither palette reduction at 51.2 PPU. Point/uncompressed/Clamp/no mipmaps, centered pivots. Contact shadows use the existing shared asset and world-aligned offset (+.04,-.04).

Acceptance: strict overhead silhouettes, matching palette, genuine alpha, aligned fixed cells, no long shadows; inspect at gameplay scale before acceptance. Functional text and RSD bindings remain in UI Toolkit. Record exact prompts and returned source paths in prompts.json; authoring/export and slicing in Author.cs.

Final integration acceptance: agent-accepted under the explicit autonomous Phase 4B request after native gameplay-scale inspection. Exact manifest filenames are `p4b_heavy_prompts.json` and `p4b_heavy_inventory.json`; the latter records 48 reference/source/runtime/icon images with dimensions, alpha extrema and SHA-256. Rejected v001 source sheets remain archival and are not referenced by runtime assets. Corrected flame defense and cone sheets remove facing/nozzle problems; corrected rocket smoke removes baked projectile pixels.

Targets: four prefabs under `Assets/Game/Prefabs/Defenses`, four definitions plus three HeavyWeaponData assets, TopDownVfx catalog, existing BuildHud catalog and top-down enemy status presentation. Catalog indices, original VfxId indices, enemy roots, footprint ownership and existing asset GUIDs are retained. Effects remain presentation-only.

Validation: eight valid definitions, zero missing scripts, Point/uncompressed/no-mipmap import audit; actual 1920×1080 and 1440×810 build panel checks; natural firing/impact/burn captures; earned-money waves 15–25 all complete without breaches; 300-enemy/19-defense stress cleanup returns to its renderer baseline. Evidence and limitations are recorded in `PHASE_4B_REPORT.md` and `Docs/Validation/Phase4B`. Final regression totals are maintained in the report.
