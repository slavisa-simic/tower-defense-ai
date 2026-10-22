# Phase 3C integration batch

## Scope and source

- 2026-10-01; full integration and restrained polish explicitly requested by the user.
- Reuse approved Danube master v002 and technically accepted Phase 3A/3B runtime exports. No raster regeneration planned.
- Bind environment/props/base/UI inventory to the primary saved scene, retain all existing gameplay owners and definitions.
- Unity 6000.3.24f1, live CLI/pipeline, AssetDatabase/PrefabUtility/scene APIs. Baseline hashes and consumer copies in Docs/Validation/Phase3C.
- New code-native full HUD pixel font extends the original reward glyph language; new integrated gameplay reference is an actual capture.

## Art and import contract

Existing 51.2 world PPU, Point/uncompressed/no mipmaps, hard-alpha source assets, center pivots and sorting conventions stay in force. UI backgrounds use explicit 16-pixel slices. Existing terrain and sprites are reused without source modification. Props have no colliders, occupancy or navigation components.

## Review and integration

Status: **COMPLETE**, validated 2026-10-01. Visual selection is within the user's autonomous integration authorization; no new per-image approval is claimed. The recorded approved Danube master v002 remains the direction anchor; technically accepted earlier runtime exports are reused byte-identically.

Targets: CoastalDefense_TopDown, PlayerBase_TopDown, Crawler_TopDown hit frames, defense prefab obsolete visual cleanup, TopDownHud UXML/USS and presentation observers. Gameplay roots, GUIDs, footprints, waves, money, damage and navigation stay authoritative.

## Final consumers and revisions

| Existing inventory / new deliverable | Consumer / decision |
|---|---|
| Phase 3A-1 water/shore/grass/mud terrain v001 | Existing primary terrain retained; quiet silt/dirt/cracked-ground overlays, concrete apron, road and bunker floor under one presentation-only root |
| Phase 3A-1 Props v001 | River boats/wreck/debris/supplies, quarantine fence/signs/sandbags/hedgehogs/floodlights, base equipment; exact asset paths, transforms and orders in `Docs/Validation/Phase3C/dressing.json` |
| Phase 3A-1 Base v001 | Existing PlayerBase Roof replaced by command-post sprite at unit scale; wall/gate/checkpoint/tent/watchpost/generator/ammunition dressed around it |
| Basic Infected idle/walk/attack/death v001 | Existing Crawler presentation retained; hit_e_sheet v001 additionally bound as a two-frame 10-FPS, 0.2-second reaction |
| Phase 3A-3 MG/Sniper/Wire/Mine and ghosts | Existing bindings retained, obsolete dormant visual leaves removed, required fallback references preserved |
| Phase 3B VFX/catalog/pools | Existing runtime consumers retained and reviewed; no new event types |
| Phase 3A-1 UI foundation; sniper icon v002 | Explicit 16px slices, icons, functional text, selected/disabled feedback; catalog order preserved separately from display order |
| `Assets/Game/Art/UI/bd_ui_hud_pixel_v001.ttf` | New original code-native 5x7 font extending Phase 3B glyphs via `Docs/Art/create_hud_font.py`; UIDocument root and CombatFeedbackHud |
| `Assets/Game/Art/MasterReferences/ref_gameplay_integrated_01.png` | New actual wave-five firing capture, 1920x1080; non-runtime Point/uncompressed/no-mipmap texture, NPOT None, max size 2048 |

No raster prompt was issued and no accepted source/export was regenerated. Existing asset GUIDs and sub-sprite IDs are preserved. New font/reference GUIDs are recorded in normal Unity metadata. The capture is an implemented reference, not a newly approved generative master.

## Validation and delivery

Full report: `PHASE_3C_REPORT.md`, including all 19 requested sections. Final suites: 46/46 EditMode and 27/27 PlayMode. Natural five-wave walkthrough: 10/15/20/25/30 kills, base 100 throughout, exact rewards and clean transitions. Live VFX review: 21/21 checks. Natural base attack and Game Over separately checked. Actual 1920x1080/2560x1440 camera/HUD checks cover all 576 cells each. Preparation, combat, heavy combat, effects, Game Over and base attack screenshots reviewed.

Final audits: 113 production PNGs / 258 sprites; zero visible normal-gameplay primitives. 100/200 moving enemies with 33 turrets remained operational; Editor P95 frame time 3.634/5.076ms, median GC 9833 B/frame, median draw calls 390/508. Pool renderers remained 274; overflow/reset/clear passed. Warmed 10,000 muzzle dispatches allocated zero managed bytes on that call path. This is Editor evidence, not a shipping Player benchmark.

Limits: dynamic camera fit gives noninteger Point scaling; other aspect ratios and standalone GPU overdraw were not certified. Runner/Brute and eventless states remain inventory. Existing Input Manager deprecation warning remains; compilation/Console show no implementation errors. Exact pre-task TimeManager bytes were restored after validation serialization migration. Prior PNGs, metadata, gameplay data, packages and legacy scene are unchanged; `repository-integrity.json` records the 913-file comparison.
