# Phase 3B core VFX batch — p3b_core_001

Status: COMPLETE, validated 2026-10-01. Production began 2026-09-21 and resumed on the user's continue instruction. The user explicitly authorized autonomous generation, correction, import and integration. All selected outputs received technical/visual review; selection does not claim separate user approval.

## Direction and provenance

Built-in ImageGen produced the reference and native artwork directly. The master board used previously approved `Assets/Game/Art/MasterReferences/bd_ref_danube_master_v002.png`. Runtime generations used that approved anchor and the selected `Assets/Game/Art/MasterReferences/ref_vfx_core_01.png` as style references; the MG v002 edit used the rejected v001 sheet as its edit input. Exact prompts are linked per source below. Native generation files are preserved byte-for-byte under `Assets/Game/Art/Effects/SourceSheets/p3b_core_001/`; all 23 hashes match original tool outputs in `Docs/Validation/Phase3B/source-receipts.json`.

The master board is an opaque 1536x1024 style document. Runtime generations use a uniform magenta export key after the initial transparency request yielded a painted checkerboard. The export script keys this backing to hard alpha, uses measured common registration/crop extents, nearest-neighbor reduction and a shared 32-color action palette without dithering. It never edits native source files. Mine row anchors are individually registered without normalizing frame expansion. Two tiny tracer streaks and the original reward digit font are code-native assets, not substitute generated illustrations.

Rejected sources are retained for provenance only: MG v001 has a painted checkerboard; medium water splash v001 has an upright fountain perspective. MG v002 and medium splash v002 replace those sources in all runtime exports. No rejected source is bound to runtime. Master and every exported frame were visually inspected; runtime audit confirms equal cells, nonempty frames, transparent zero-RGB borders and no magenta residue. Point/alpha behavior and scene intensity were reviewed at 1080p/1440p, with HUD captured at the current 1280x720 Game View.

## Native sources

| Source | Native PNG px | Exact prompt | Selection |
|---|---|---|---|
| `Assets/Game/Art/Effects/SourceSheets/p3b_core_001/bd_src_blood_decal_medium_v001.png` | 1254 x 1254 | [prompt](p3b_prompts/blood_decal_medium_v001.txt) | Selected; prepared as runtime art |
| `Assets/Game/Art/Effects/SourceSheets/p3b_core_001/bd_src_blood_decal_v001.png` | 1536 x 1024 | [prompt](p3b_prompts/blood_decal_v001.txt) | Selected; prepared as runtime art |
| `Assets/Game/Art/Effects/SourceSheets/p3b_core_001/bd_src_blood_hit_v001.png` | 2172 x 724 | [prompt](p3b_prompts/blood_hit_v001.txt) | Selected; prepared as runtime art |
| `Assets/Game/Art/Effects/SourceSheets/p3b_core_001/bd_src_explosion_small_v001.png` | 2172 x 724 | [prompt](p3b_prompts/explosion_small_v001.txt) | Selected; prepared as runtime art |
| `Assets/Game/Art/Effects/SourceSheets/p3b_core_001/bd_src_impact_concrete_v001.png` | 1536 x 1024 | [prompt](p3b_prompts/impact_concrete_v001.txt) | Selected; prepared as runtime art |
| `Assets/Game/Art/Effects/SourceSheets/p3b_core_001/bd_src_impact_dirt_v001.png` | 2172 x 724 | [prompt](p3b_prompts/impact_dirt_v001.txt) | Selected; prepared as runtime art |
| `Assets/Game/Art/Effects/SourceSheets/p3b_core_001/bd_src_impact_flesh_v001.png` | 1536 x 1024 | [prompt](p3b_prompts/impact_flesh_v001.txt) | Selected; prepared as runtime art |
| `Assets/Game/Art/Effects/SourceSheets/p3b_core_001/bd_src_impact_metal_v001.png` | 1536 x 1024 | [prompt](p3b_prompts/impact_metal_v001.txt) | Selected; prepared as runtime art |
| `Assets/Game/Art/Effects/SourceSheets/p3b_core_001/bd_src_impact_mud_v001.png` | 2172 x 724 | [prompt](p3b_prompts/impact_mud_v001.txt) | Selected; prepared as runtime art |
| `Assets/Game/Art/Effects/SourceSheets/p3b_core_001/bd_src_mg_muzzle_v001.png` | 2172 x 724 | [prompt](p3b_prompts/mg_muzzle_v001.txt) | Rejected: painted checkerboard; v002 replaces it |
| `Assets/Game/Art/Effects/SourceSheets/p3b_core_001/bd_src_mg_muzzle_v002.png` | 2172 x 724 | [prompt](p3b_prompts/mg_muzzle_v002.txt) | Selected; prepared as runtime art |
| `Assets/Game/Art/Effects/SourceSheets/p3b_core_001/bd_src_mine_explosion_v001.png` | 1536 x 1024 | [prompt](p3b_prompts/mine_explosion_v001.txt) | Selected; prepared as runtime art |
| `Assets/Game/Art/Effects/SourceSheets/p3b_core_001/bd_src_shore_foam_v001.png` | 1983 x 793 | [prompt](p3b_prompts/shore_foam_v001.txt) | Selected; prepared as runtime art |
| `Assets/Game/Art/Effects/SourceSheets/p3b_core_001/bd_src_smoke_medium_v001.png` | 2172 x 724 | [prompt](p3b_prompts/smoke_medium_v001.txt) | Selected; prepared as runtime art |
| `Assets/Game/Art/Effects/SourceSheets/p3b_core_001/bd_src_smoke_small_v001.png` | 2172 x 724 | [prompt](p3b_prompts/smoke_small_v001.txt) | Selected; prepared as runtime art |
| `Assets/Game/Art/Effects/SourceSheets/p3b_core_001/bd_src_sniper_muzzle_v001.png` | 2172 x 724 | [prompt](p3b_prompts/sniper_muzzle_v001.txt) | Selected; prepared as runtime art |
| `Assets/Game/Art/Effects/SourceSheets/p3b_core_001/bd_src_vfx_master_v001.png` | 1536 x 1024 | [prompt](p3b_prompts/master_v001.txt) | Selected reference; copied to ref_vfx_core_01.png |
| `Assets/Game/Art/Effects/SourceSheets/p3b_core_001/bd_src_water_ripple_v001.png` | 2172 x 724 | [prompt](p3b_prompts/water_ripple_v001.txt) | Selected; prepared as runtime art |
| `Assets/Game/Art/Effects/SourceSheets/p3b_core_001/bd_src_water_splash_large_v001.png` | 2172 x 724 | [prompt](p3b_prompts/water_splash_large_v001.txt) | Selected; prepared as runtime art |
| `Assets/Game/Art/Effects/SourceSheets/p3b_core_001/bd_src_water_splash_medium_v001.png` | 2172 x 724 | [prompt](p3b_prompts/water_splash_medium_v001.txt) | Rejected: upright fountain; v002 replaces it |
| `Assets/Game/Art/Effects/SourceSheets/p3b_core_001/bd_src_water_splash_medium_v002.png` | 2172 x 724 | [prompt](p3b_prompts/water_splash_medium_v002.txt) | Selected; prepared as runtime art |
| `Assets/Game/Art/Effects/SourceSheets/p3b_core_001/bd_src_water_splash_small_v001.png` | 2172 x 724 | [prompt](p3b_prompts/water_splash_small_v001.txt) | Selected; prepared as runtime art |
| `Assets/Game/Art/Effects/SourceSheets/p3b_core_001/bd_src_water_wake_v001.png` | 2172 x 724 | [prompt](p3b_prompts/water_wake_v001.txt) | Selected; prepared as runtime art |

## Prepared assets and consumers

All runtime exports are revision v001, prepared from the selected native revision shown. Runtime paths, cells, pivot, FPS, duration, caps, sorting and every consumer are specified in `VFX_ART_SPECS.md` and the machine-readable `p3b_manifest.json`. These records are authoritative together; filenames follow the repository's bd_fx naming rather than the request's illustrative filenames.

| Runtime asset | Native source / construction | Integration status |
|---|---|---|
| `Assets/Game/Art/Effects/Combat/MachineGun/bd_fx_mg_muzzle_sheet_v001.png` | `Assets/Game/Art/Effects/SourceSheets/p3b_core_001/bd_src_mg_muzzle_v002.png` | MG Fired; existing muzzle transform |
| `Assets/Game/Art/Effects/Combat/Sniper/bd_fx_sniper_muzzle_sheet_v001.png` | `Assets/Game/Art/Effects/SourceSheets/p3b_core_001/bd_src_sniper_muzzle_v001.png` | Sniper Fired; existing muzzle transform |
| `Assets/Game/Art/Effects/Combat/Impacts/bd_fx_impact_dirt_sheet_v001.png` | `Assets/Game/Art/Effects/SourceSheets/p3b_core_001/bd_src_impact_dirt_v001.png` | Validated inventory; no surface-impact event exists |
| `Assets/Game/Art/Effects/Combat/Impacts/bd_fx_impact_mud_sheet_v001.png` | `Assets/Game/Art/Effects/SourceSheets/p3b_core_001/bd_src_impact_mud_v001.png` | Validated inventory; no surface-impact event exists |
| `Assets/Game/Art/Effects/Combat/Impacts/bd_fx_impact_concrete_sheet_v001.png` | `Assets/Game/Art/Effects/SourceSheets/p3b_core_001/bd_src_impact_concrete_v001.png` | Validated inventory; no surface-impact event exists |
| `Assets/Game/Art/Effects/Combat/Impacts/bd_fx_impact_metal_sheet_v001.png` | `Assets/Game/Art/Effects/SourceSheets/p3b_core_001/bd_src_impact_metal_v001.png` | Validated inventory; no surface-impact event exists |
| `Assets/Game/Art/Effects/Combat/Impacts/bd_fx_impact_flesh_sheet_v001.png` | `Assets/Game/Art/Effects/SourceSheets/p3b_core_001/bd_src_impact_flesh_v001.png` | MG/sniper Fired at authoritative enemy impact |
| `Assets/Game/Art/Effects/Blood/Hits/bd_fx_blood_hit_sheet_v001.png` | `Assets/Game/Art/Effects/SourceSheets/p3b_core_001/bd_src_blood_hit_v001.png` | EnemyHealth.Damaged (alive) / Died |
| `Assets/Game/Art/Effects/Blood/Death/bd_fx_blood_decal_v001.png` | `Assets/Game/Art/Effects/SourceSheets/p3b_core_001/bd_src_blood_decal_v001.png` | Existing Crawler death |
| `Assets/Game/Art/Effects/Blood/Death/bd_fx_blood_decal_medium_v001.png` | `Assets/Game/Art/Effects/SourceSheets/p3b_core_001/bd_src_blood_decal_medium_v001.png` | Validated inventory; Death API at effectScale > 1 |
| `Assets/Game/Art/Effects/Explosions/Mine/bd_fx_mine_explosion_sheet_v001.png` | `Assets/Game/Art/Effects/SourceSheets/p3b_core_001/bd_src_mine_explosion_v001.png` | LandMine.Detonated; existing AOE represented visually |
| `Assets/Game/Art/Effects/Explosions/Generic/bd_fx_explosion_small_sheet_v001.png` | `Assets/Game/Art/Effects/SourceSheets/p3b_core_001/bd_src_explosion_small_v001.png` | Validated reusable inventory; no invented gameplay use |
| `Assets/Game/Art/Effects/Smoke/bd_fx_smoke_small_sheet_v001.png` | `Assets/Game/Art/Effects/SourceSheets/p3b_core_001/bd_src_smoke_small_v001.png` | MG/sniper health <= 35%; once per 1.2 seconds |
| `Assets/Game/Art/Effects/Smoke/bd_fx_smoke_medium_sheet_v001.png` | `Assets/Game/Art/Effects/SourceSheets/p3b_core_001/bd_src_smoke_medium_v001.png` | Mine aftermath |
| `Assets/Game/Art/Effects/Water/Splash/bd_fx_water_splash_small_sheet_v001.png` | `Assets/Game/Art/Effects/SourceSheets/p3b_core_001/bd_src_water_splash_small_v001.png` | Existing Crawler Water -> non-Water transition |
| `Assets/Game/Art/Effects/Water/Splash/bd_fx_water_splash_medium_sheet_v001.png` | `Assets/Game/Art/Effects/SourceSheets/p3b_core_001/bd_src_water_splash_medium_v002.png` | Validated inventory; no playable Runner exists |
| `Assets/Game/Art/Effects/Water/Splash/bd_fx_water_splash_large_sheet_v001.png` | `Assets/Game/Art/Effects/SourceSheets/p3b_core_001/bd_src_water_splash_large_v001.png` | Validated inventory; no playable Brute exists |
| `Assets/Game/Art/Effects/Water/Wake/bd_fx_water_wake_sheet_v001.png` | `Assets/Game/Art/Effects/SourceSheets/p3b_core_001/bd_src_water_wake_v001.png` | Attached renderer while existing enemy occupies Water |
| `Assets/Game/Art/Effects/World/bd_fx_water_ripple_sheet_v001.png` | `Assets/Game/Art/Effects/SourceSheets/p3b_core_001/bd_src_water_ripple_v001.png` | Existing Water cells; one request per .65 seconds |
| `Assets/Game/Art/Effects/World/bd_fx_shore_foam_sheet_v001.png` | `Assets/Game/Art/Effects/SourceSheets/p3b_core_001/bd_src_shore_foam_v001.png` | Existing Water/Beach boundary; looping strip per row |
| `Assets/Game/Art/Effects/Combat/MachineGun/bd_fx_mg_tracer_v001.png` | `code-native pixel streak: Docs/Art/export_phase3b.py` | MG Fired; visual hitscan segment |
| `Assets/Game/Art/Effects/Combat/Sniper/bd_fx_sniper_tracer_v001.png` | `code-native pixel streak: Docs/Art/export_phase3b.py` | Sniper Fired; visual hitscan segment |

Additional deliverables: `Assets/Game/Art/MasterReferences/ref_vfx_core_01.png`; `Assets/Game/Art/Effects/CombatVfxCatalog.asset`; `Assets/Game/Art/Effects/UIFeedback/bd_reward_digits_v001.ttf` (original digit/plus glyphs from `Docs/Art/create_reward_font.py`, fonttools used only in the external tool runtime). All 18 AnimationClips are listed below; each uses the corresponding ordered catalog frames/FPS and sprite-only binding.

- `Assets/Game/Animations/Effects/bd_fx_mg_muzzle_v001.anim`
- `Assets/Game/Animations/Effects/bd_fx_sniper_muzzle_v001.anim`
- `Assets/Game/Animations/Effects/bd_fx_impact_dirt_v001.anim`
- `Assets/Game/Animations/Effects/bd_fx_impact_mud_v001.anim`
- `Assets/Game/Animations/Effects/bd_fx_impact_concrete_v001.anim`
- `Assets/Game/Animations/Effects/bd_fx_impact_metal_v001.anim`
- `Assets/Game/Animations/Effects/bd_fx_impact_flesh_v001.anim`
- `Assets/Game/Animations/Effects/bd_fx_blood_hit_v001.anim`
- `Assets/Game/Animations/Effects/bd_fx_mine_explosion_v001.anim`
- `Assets/Game/Animations/Effects/bd_fx_explosion_small_v001.anim`
- `Assets/Game/Animations/Effects/bd_fx_smoke_small_v001.anim`
- `Assets/Game/Animations/Effects/bd_fx_smoke_medium_v001.anim`
- `Assets/Game/Animations/Effects/bd_fx_water_splash_small_v001.anim`
- `Assets/Game/Animations/Effects/bd_fx_water_splash_medium_v001.anim`
- `Assets/Game/Animations/Effects/bd_fx_water_splash_large_v001.anim`
- `Assets/Game/Animations/Effects/bd_fx_water_wake_v001.anim`
- `Assets/Game/Animations/Effects/bd_fx_water_ripple_v001.anim`
- `Assets/Game/Animations/Effects/bd_fx_shore_foam_v001.anim`

## Integration and validation

Only the primary scene, two turret prefabs, Crawler presentation, EnemyHealth's new committed-damage notification, TurretPresentation and MineExplosionPresentation change preexisting project files. Their GUIDs are preserved. Crawler is the existing gameplay type; binding Basic Infected art does not create an enemy type. Surface-specific non-flesh impacts, generic explosion, medium decal and medium/large splashes remain validated inventory because no current event/type consumes them. No boat props are instantiated, so boat ambience is not bound. Existing mine debris is visual sprite content only.

46/46 EditMode and 25/25 PlayMode tests passed. All live primary-scene checks passed, including MG/sniper damage, immediate death/reward, mine AOE edges and occupancy release, water exit, smoke, wave rewards and Game Over. 22 runtime texture audits and 18 complete clip sampling checks passed; 23/23 sources match originals. 100/300 moving-enemy and 32-MG stress fixtures passed; all pool overflow/clear checks passed. See `PHASE_3B_REPORT.md` for measurements and limitations. No live validation fixture was saved into the scene. Temporary imported screenshot staging was verified and removed via AssetDatabase; evidence remains under Docs.
