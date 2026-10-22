# Phase 2A — Top-down gameplay conversion

## Status

**COMPLETE** — verified in Unity 6000.3.24f1. Final validation completed 2026-09-17 (Europe/Sarajevo).

## What Changed

- Added a saved, editable strict top-down battlefield with ocean on the left, a vertical beach, a broad grassy combat field, and the player base on the right.
- The map is 32 × 18 cells at 1.25 units per cell (40 × 22.5 world units). Water occupies four columns (12.5% of width), beach two columns (6.25%), and ground the remaining 26 columns. There are 467 ordinary buildable ground cells; the base is cell (29, 9).
- Added an orthographic camera perpendicular to XY. It fits the whole field using the current viewport aspect ratio and a small border, supporting both 1920×1080 and 2560×1440 without resolution-specific positions.
- Added simple unlit SpriteRenderer placeholders: blue ocean, sand beach, green field with faint cell guides, red directional crawler shapes, and a pale base marker. No final artwork, reference-image backdrop, animations, physics, or gameplay packages were added.
- Five serialized coastline transforms at cells (1, 3), (1, 6), (1, 8), (1, 11), and (1, 14) provide vertically distributed approaches. The existing test spawner cycles these points at the original 1-second initial delay / 3-second interval. It skips blocked or occupied spawn positions and tries another configured point.
- The minimal HP HUD sits at the upper right; Game Over remains centered.
- Registered Environment, GroundObjects, Enemies, Defenses, and Effects sorting layers. UI Toolkit renders above the world. The primary camera selects a URP 2D renderer added to both existing quality pipelines; their original 3D renderer stays available for the historical scene.
- Updated README and AGENTS.md to record the permanent strict top-down pixel-art direction. The Phase 1 report is clearly marked historical.

No supplied gameplay-reference image or REFERENCES folder was found in the repository. Available Phase 1 screenshots were inspected; the new composition follows the explicit written brief. The generated primitive shape textures are individual placeholder assets, not baked map art.

## Coordinate System

**XY is the primary gameplay plane:** X is horizontal (sea → inland), Y is vertical, and Z is presentation depth. Gameplay roots remain at the grid origin's Z; visual children may carry presentation offsets. The camera sits at negative Z with identity rotation, looking along +Z, so there is no tilt or perspective distortion.

Grid origin remains the minimum outer corner. Grid-to-world returns cell centers. World-to-grid uses X/Y, ignores Z, includes the minimum edges, and excludes the maximum edges. Movement projects initial positions onto the gameplay plane, then uses the existing delta-time distance budget and attack-range clamp.

The original scene explicitly uses `GameplayPlane.LegacyXZ`. Its data asset was migrated to persist that setting and the renamed coastal-band fields without changing map values or its GUID. New definitions and grid instances default to XY. Compatibility is restricted to coordinate conversion, terrain layout, and debug/presentation axes; it does not create a second grid or navigation implementation.

## Scene

Primary: `Assets/Game/Scenes/CoastalDefense_TopDown.unity`.

It is the first enabled build scene. `Assets/Game/Scenes/CoastalDefense_Prototype.unity` remains a separate playable regression/reference scene; its scene file is byte-for-byte unchanged. Both are tested separately and never loaded additively by gameplay.

**Tools → Shoreline Defense → Open or Create Top-Down Scene** opens the saved scene or creates it if missing. **Rebuild Top-Down Scene From Definition** regenerates the scene layout while retaining existing data/prefabs; it replaces scene-specific layout overrides. Runtime does not invoke either builder.

## Preserved Systems

- Four-way A* algorithm and reusable search buffers are unchanged, including blocked cells, weighted paths, grid revision notifications, and coalesced replanning.
- Shared Health / IDamageable, EnemyHealth, EnemyCombat, BaseHealth, GameStateController, and PrototypeHud gameplay logic are unchanged.
- Crawler configuration remains 100 HP, speed 2.5, attack damage 10, interval 1 second, and range 1.6. Base maximum health remains 100.
- Explicit initialization, one gameplay update per enemy, event-driven health/UI, Game Over, and stopped spawning/movement are preserved.
- Phase 1 data, prefabs, materials, scene, and regression coverage remain available.

## Refactored Systems

- BattlefieldGrid: XY conversion by default, explicit historical XZ support, and projection to the gameplay plane. Cell data and pathfinding algorithm remain shared.
- BattlefieldDefinition: coastal bands run along X for XY maps. `FormerlySerializedAs` preserves old `waterRows`/`beachRows` values under orientation-neutral field names. New assets default to the new map convention.
- GridManager: only its grid Gizmos change axes; ownership and initialization are unchanged.
- EnemyMovement exposes facing direction; EnemyController requests facing toward the base on arrival. New EnemyFacing rotates only the Visual child, leaving health/movement/combat independent of renderers and gameplay roots unrotated.
- EnemySpawner adds configurable separation at spawn time, cleans up references to removed enemies, and checks alternate points when a lane is unavailable. It performs no physics queries or per-frame A*.
- TopDownCamera and TopDownSceneBuilder contain camera framing and authoring responsibilities. The old builder retains its historical data defaults and preserves the primary build-scene ordering.

## Tests

Unity 6000.3.24f1, official Pipeline Test Runner:

| Suite | Passed | Failed | Skipped |
|---|---:|---:|---:|
| EditMode | 26 | 0 | 0 |
| PlayMode | 9 | 0 | 0 |

EditMode includes XY conversion/depth independence, bounds and nonzero origins, legacy serialization, all coastline rows reaching the right-hand base through every terrain band, blocked/weighted/unreachable paths, change notifications, health regressions, movement through turns at different tick sizes, attack-range clamping, and camera framing at 1280×720, 1920×1080, 2560×1440, 1024×768, and 2560×1080.

The four original PlayMode regressions also run against the new persisted scene. They cover sea → beach → grass → base travel, repeated attacks, HP UI, Game Over, frozen enemies, stopped spawning, scene reset, health independence/removal, and replanning/recovery. A new regression checks occupied/blocked spawn points and a reopened lane.

Both suites were rerun successfully after the final definition-default/legacy-serialization change. Unity script compilation passed. The final Play Mode Console reported zero errors, zero warnings, and no failed compilation. [EditMode results](Docs/Validation/Phase2A/editmode-results.json), [PlayMode results](Docs/Validation/Phase2A/playmode-results.json), [Console status](Docs/Validation/Phase2A/console-status.json).

A Windows x64 player build succeeded with zero errors and one existing advisory: the official Pipeline automation package is intentionally disabled in player builds. Output: `Build/Phase2A/ShorelineDefense.exe` (ignored generated output). [Build summary](Docs/Validation/Phase2A/windows-build.json). Visual validation was performed in the Unity Editor; the standalone executable was compiled but not separately play-tested.

## Manual Validation

Ran the saved primary scene in Unity Play Mode at time scale 1 and inspected composited Game view captures. Verified strict top-down orientation, full-field framing, left ocean, vertical beach, large grass region, right-hand base, distinct enemies with directional tips, multiple coastline approaches, and actual travel across terrain.

Inspected [initial approach](Docs/Validation/Phase2A/play-start.png), [travel](Docs/Validation/Phase2A/play-travel.png), [base damage / HP UI](Docs/Validation/Phase2A/play-damage.png), and [Game Over](Docs/Validation/Phase2A/game-over.png). Runtime snapshots recorded HP falling from 100 to 70 to 30 to 0; screenshot capture occurs slightly after each snapshot, so the damage image shows 60 HP. Eight enemies had spawned at Game Over. Their count, positions, and A* request count remained unchanged at the later stopped-state inspection. [Recorded snapshots](Docs/Validation/Phase2A/normal-playthrough.json).

Also ran at an actual 2560×1440 Game view size, inspecting full-field framing, readable HP UI, sea emergence and base attacks. [QHD capture](Docs/Validation/Phase2A/play-2560x1440.png).

Scene/prefab inspection found no missing scripts or broken serialized object references. Each scene contains exactly one GridManager, GameStateController, and EnemySpawner. Opening the top-down scene twice retains nine roots without duplication. Asset metadata checks found no missing/orphaned metadata or duplicate GUIDs. [Scene inspection](Docs/Validation/Phase2A/scene-integrity.json), [file integrity](Docs/Validation/Phase2A/file-integrity.json), [post-Game-Over checks](Docs/Validation/Phase2A/post-game-over-check.json).

Reviewed the conversion diff against a pre-change snapshot, including field migration, prefabs, scene references, initialization, subscriptions, coordinate conversions, A* request frequency, spawn state and camera settings. Corrected the new-asset defaults during that review. No gameplay-manager duplication, new scene searches in runtime code, or Phase 2B features were introduced.

## Remaining Issues

- No known Phase 2A gameplay blocker. The supplied visual reference could not be compared because it was absent.
- Spawn separation prevents stacking at the coastline, but moving enemies may overlap or converge near the base. Crowd avoidance and occupancy are not implemented.
- Four-way navigation still makes right-angle turns and treats enemies as point agents. Traversal cost affects route choice, not movement speed. A* remains synchronous; profile before scaling to large crowds.
- Map dimension/origin changes require scene rebuilding outside Play Mode. Future placement must define structure footprints, occupancy, and route validation; the base's visible marker is a placeholder, not a placement footprint.
- Pixel-perfect final art, animation, and Y-based character depth sorting are not part of this graybox step. Restart still uses scene reload or Stop/Play.

## Phase 2B Readiness

**Ready for Phase 2B — Defense Placement + Economy + Machine Gun Nest.** The verified foundation includes broad buildable ground, explicit cell state and navigation updates, separate presentation, and preserved health/game-state boundaries. No Phase 2B features were implemented. Begin only on a new user request.
