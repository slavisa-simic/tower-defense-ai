# Phase 3C — full art integration and visual polish

Validated 2026-10-01 in Unity 6000.3.24f1. Evidence is in `Docs/Validation/Phase3C/`; integration receipt is `Docs/Art/Batches/p3c_integration_001.md`.

## 1. STATUS

**COMPLETE.** The saved primary scene presents the existing game as a coherent overhead Danube quarantine defense slice. Existing accepted art was integrated without raster regeneration. All five natural waves completed, both complete test suites passed, and normal gameplay has zero visible programmer-art placeholders. No subsequent phase was started.

## 2. Primary playable scene

`Assets/Game/Scenes/CoastalDefense_TopDown.unity`. Strict XY, unrotated orthographic camera, water left and base right. The authoritative 32 x 18 grid, 1.25-unit cells, base anchor (29,9), 3 x 3 base footprint, saved gameplay roots and existing asset GUIDs remain intact. `CoastalDefense_Prototype` remains the unchanged XZ regression scene.

## 3. Environment integration

The existing tiled water, transition, mud and grass remain the foundation. Approved concrete, road and bunker-floor tiles dress the base apron. Quiet silt fringes, worn dirt tracks and weathered patches soften the large field using existing tiles and restrained renderer alpha.

One `Danube Quarantine Dressing` root contains 155 decorative renderers, organized into terrain detail, river arrival props and quarantine position. Intact/wrecked inflatable boats, damaged supplies and debris frame the river; walls, a gate, fence, sandbags, signs and hedgehogs frame the quarantine line; the command post, tent, watch post, generator, radio/radar equipment, antenna, floodlights and ammunition frame the base. The center approach remains visually open. These props have no colliders, navigation blockers, build occupancy or new gameplay components. They do not imply new boat/disembarkation systems.

The original PlayerBase root and health owner are retained. Its former roof now binds the approved 192 x 192 command-post sprite at unit scale. Removed primitive visual children no longer overlap it.

## 4. Enemy integration

The existing playable `Crawler_TopDown` uses the accepted Basic Infected sprites. Pale overhead silhouettes read over water, mud and olive terrain. Root transforms, independent health, movement, targeting and immediate removal remain authoritative. Runner and Brute have accepted imported art but no playable prefab/wave definition in the inspected game; their inventory remains ready without inventing enemy types.

## 5. Enemy animation integration

Existing code-driven idle (5 FPS), walk (8 FPS), attack (10 FPS) and detached death (9 FPS) remain connected. Added the accepted two-frame hit sequence at 10 FPS, a 0.2-second presentation override, plus the short existing damage tint. Rapid hits do not continually restart the pose. Movement, attacks, damage, targetability and rewards continue independently. No per-enemy Animator or animation event owns gameplay. A natural attack fixture confirmed 100 -> 90 base HP, the expected attack sprite and identity root rotation. The final command-post sorting correction keeps this attacker visible.

## 6. Defense integration

MG, Sniper, Wire and Mine retain their accepted Phase 3A-3 sprites and original definitions, GUIDs, footprints and placement architecture. MG/sniper fixed bases and rotating weapons retain the existing aiming and firing observers; local muzzle positions are preserved. Wire/mine remain low ground objects. Existing healthy/damaged/arming presentation remains connected; destroyed inventory cannot prolong a removed gameplay owner. Enemies still target the base; no new structure-targeting system was introduced.

All four real-art placement ghosts were checked in valid and invalid cells, including green/red tint, cost, occupancy and cancellation. Defense prefab cleanup removes obsolete disabled/null visual leaves while retaining required fallback flash/tracer/explosion references.

## 7. VFX integration

Retained the single Phase 3B `VfxService`, catalog and bounded pools. Muzzle flashes, thin MG tracers, stronger sniper tracers, flesh impacts, blood, detached death art, compact mine burst/aftermath smoke, health-based structure smoke, water wakes, shore exit splashes, ripples and foam all remain integrated. Reward labels and wave banners use existing observers. Non-flesh surface impacts, generic explosions and larger arrival splashes remain validated inventory where the game has no corresponding event/type.

The live VFX review passed **21/21 checks**, including 120 combined MG/sniper damage, repeated fire, immediate death/reward, wake termination, one shore splash, mine AOE inside/outside distinction, immediate occupancy release, smoke fade and pool bounds. Screenshots were rendered after effect sampling so the actual frames, rather than stale backbuffers, were reviewed.

## 8. UI integration

UI Toolkit remains authoritative for controls and functional text. Approved 16-pixel sliced panel/button backings, money/HP/wave icons and four defense icons now bind the existing HUD. Money is top-left, wave/enemy counts top-center, HP and a thin health bar top-right. The bottom bar displays Wire, Mine, MG, Sniper and their prices, plus Start Wave and placement guidance. Normal/hover/focus/selected/unaffordable states remain readable. Existing `build-mg`, money, health, wave, start and Game Over bindings are preserved.

Visual order is serialized separately from the original MG/Wire/Mine/Sniper catalog. This fixed an early catalog-order regression without changing existing tests. `HudArtPresentation` only observes money/HP and adds brief purchase/reward/damage feedback; it cannot mutate state. Reenable coverage verifies exactly four icons and the original MG binding. The new original 5 x 7 pixel font extends the Phase 3B reward glyph language, and the wave/reward overlays use it. Game Over uses the approved panel language, existing overlay and disabled controls.

## 9. Camera / pixel-art settings

World sprites retain **51.2 PPU**, unit visual scale, Point filtering, no mipmaps, no compression/Crunch and preserved pivots. UI imports retain their recorded 100 PPU. The final import audit checked **113 production PNGs / 258 sub-sprites**; all prior source/runtime PNG bytes and metadata are preserved.

The camera reserves top/bottom HUD strips with viewport `(0, .17, 1, .69)` and 0.6 world-unit framing padding, keeping orthographic size 11.85. The actual full Game View was tested at **1920 x 1080 and 2560 x 1440**, with a 1280 x 720 UI reference panel. At each size, all **576 cell centers** were projected, mapped back to the correct cell, and checked against HUD picking; all required panels stayed in bounds. Original Game View selection was restored.

The camera retains dynamic fit rather than forced integer pixel scaling. Point-filtered art remains crisp, but some logical pixels occupy alternating screen widths and moving sprites can step unevenly. No Pixel Perfect package, camera snapping or broad package change was introduced. This is a documented presentation limit, not a gameplay failure.

## 10. Sorting strategy

Retained existing `Environment`, `GroundObjects`, `Enemies`, `Defenses`, `Effects` layers and UI Toolkit overlay. Terrain detail sits above terrain; decorative props generally use GroundObjects -5; Wire/Mine use 1/2. The command post uses GroundObjects 8 so base attackers remain visible above its edge. MG/sniper bases/rotating weapons retain Defenses 0/2. Existing catalog orders keep flashes, smoke and tracers visible, with restrained ground blood/death art and water effects. No Y-sort manager or unstable per-frame sorting was needed. Gameplay root rotation and presentation depth remain separate.

## 11. Removed placeholders

The normal scene hides the former cell-guide overlay. PlayerBase Footprint/Base/Roof Stripe primitives were removed and its Roof became Command Post. Enemy Body Outline/Facing Tip and obsolete defense visual leaves were removed. Temporary capture import folders/review PNG were removed through AssetDatabase. Normal terrain, base, enemies, placed defenses, placement ghosts, VFX and HUD all use production equivalents.

## 12. Remaining placeholders

**Zero player-visible placeholders in normal gameplay.** Retained intentionally: 52 inactive debug guide lines, the inactive initial preview sprite (replaced by real art on selection), disabled MG/sniper flash/tracer and mine-explosion fallback references for contexts without VfxService, original placeholder library/material names, old scene builders and the legacy regression scene. The material's historical name does not make a production sprite a placeholder. These retained assets neither overlap final art nor appear in the normal player flow. See `placeholder-audit.json` for exact paths and enabled/active flags.

## 13. Performance results

Measured in the actual 1920 x 1080 Unity Editor Game View on Windows 11, Ryzen 5 5500 and Radeon RX 6600. Three-second samples observed unique rendered-frame delta values and ProfilerRecorder GC/draw counters. They include Editor and sampling overhead and are not a standalone Player benchmark or GPU-overdraw certification.

| Load | Samples | Median frame | P95 frame | Max frame | Median GC/frame | Median draw calls |
|---|---:|---:|---:|---:|---:|---:|
| 100 enemies, 33 turrets | 1,055 | 2.719 ms | 3.634 ms | 13.002 ms | 9,833 B | 390 |
| 200 enemies, 33 turrets | 787 | 3.674 ms | 5.076 ms | 8.071 ms | 9,833 B | 508 |

GC P95 was 9,885/9,889 bytes. This does not prove zero allocations for the entire game. A separate warmed same-thread dispatch check issued 10,000 muzzle requests in 6.847 ms with **0 managed bytes**; that result applies only to that bounded call path. No obvious disabling targeting/pathfinding spike or uncontrolled VFX instantiation appeared. Many sprite draw calls and dynamic camera scaling are reasonable future measurement targets, without speculative optimization in this phase.

## 14. Stress-test results

Both 100 and 200 moving high-health enemies remained operational against 32 MG nests, one Sniper, Wire and Mine. The final run reached 628 cumulative real shots. Natural burst peak was 77/96 and tracer peak 19/64, with no natural overflow. Synthetic overflow capped/recycled every channel as designed. Renderer count stayed **274 before/after**; clearing emptied pools. Synthetic money/health/placements were confined to Play Mode and never saved or used to rebalance waves. Final measurements are `stress-results.json`; `stress-first-results.json` is an earlier sample, not the reported final result.

## 15. Automated-test results

**46/46 EditMode and 27/27 PlayMode passed**, with zero failures, skips or inconclusive tests. Existing tests were not modified. Two added PlayMode tests verify HUD reenable/named binding/camera picking and nonblocking bounded hit poses. Existing coverage includes grid, A*, economy, placement, targeting, Wire, Mine, damage/rewards, waves, Game Over and legacy regression behavior.

Unity compilation was clean. Final reopened-Editor inspection found **0 Console errors** and one existing Input Manager deprecation warning; no implementation-related warning/error was observed. `compile-final.json`, `console-final.json`, `editmode-tests.json` and `playmode-tests.json` record results. Earlier failed catalog-order checks were diagnosed and fixed; historical failed evidence is retained and superseded by the final passing suite.

## 16. Manual gameplay validation

Live Play Mode walkthrough used the real HUD Start Wave event, normal spawning/movement/combat and purchases constrained by available money. It was driven through Editor scripts at 3x simulation speed and visually inspected through rendered screenshots; it is not a claim of a human-operated mouse-only session.

| Wave | Real kills | Base HP at completion | Balance after exact kill/bonus payment |
|---|---:|---:|---:|
| 1 | 10 | 100 | $425 |
| 2 | 15 | 100 | $600 |
| 3 | 20 | 100 | $900 |
| 4 | 25 | 100 | $1,400 |
| 5 | 30 | 100 | $2,050 |

The initial two MG/Wire/Mine purchases cost $425 from $500; reinforcements used earned money. Each wave paid exactly 25 per kill plus its 100 bonus, returned cleanly to Preparation, and the fifth entered AllWavesComplete. Separate fixtures checked a natural base attack and lethal damage/Game Over, since the successful defense walkthrough took no base damage. Selection, ghosts, money, wave counts, actual weapon aim/fire, river exit, hit/death, reward feedback and VFX cleanup were checked. `live-checks.json` and `Vfx/live-checks.json` have no failed checks.

## 17. Screenshot-review findings

Reviewed preparation, built preparation, all five waves, active firing, heavy stress combat, Game Over, natural base attack and individual water/mine/smoke/death effects as images. Two actual resolutions confirmed layout and full-map visibility. Fixed the obvious base-roof occlusion of attackers; the recaptured `base-attack-1080.png` shows the attacker above the left roof edge. River props, quiet tracks, restrained effects and separated HUD strips leave the combat field readable.

The new production reference is **`Assets/Game/Art/MasterReferences/ref_gameplay_integrated_01.png`**, a 1920 x 1080 capture of real wave-five firing, copied from `Docs/Validation/Phase3C/wave-5-firing.png`. It is an ordinary non-runtime Point/uncompressed/no-mipmap texture. Future art should match this implemented scene alongside the approved master direction. No labels, generation borders, white/checkerboard mattes or source-sheet canvases are bound to gameplay. Source imperfections that were already rejected in earlier batches remain archived as rejected sources, not active art.

## 18. Known visual issues

No blocking visual or gameplay regression remains. Dynamic fit does not guarantee integer pixel scaling, and pointer-only desktop 16:9 sizes were validated; other aspect ratios are outside this phase's tested claim. Stress evidence includes Editor overhead and needs a standalone target-hardware profile before a shipping performance claim. The startup Input Manager deprecation warning predates this integration. Runner/Brute, connected wire variants, unused surface impacts and removed-owner destruction states remain inventory until supported gameplay events exist. Audio production remains deferred as requested.

The repository integrity audit uses 913 pre-task file hashes rather than the already-dirty Git index. Only the 12 intended existing consumer/code files changed. All preexisting gameplay definitions, packages, project settings, prior art PNGs, existing metadata and legacy scene stayed byte-identical. Validation briefly triggered Unity's TimeManager serialization migration; its file was restored to the exact recorded pre-task SHA-256 before delivery. No fixture state or temporary capture asset is shipped in the scene.

## 19. Recommended next phase

The visual slice is ready for the next authorized phase. A dedicated audio pass and a standalone Player profile on target hardware are appropriate next work; retain this actual gameplay reference for future art. No campaign, multiplayer, new roster, defenses, progression, backend, audio assets or publishing work was started.
