# Phase 1 Step 1 report

Historical report of the original prototype. The primary scene and art direction were superseded by [Phase 2A](PHASE_2A_REPORT.md): strict top-down XY gameplay, with this XZ scene retained only for regression/reference.

## Implemented

- Unity 6.3 LTS / URP project with a saved, editable coastal prototype scene.
- Blue sea, sand beach, green ground, cube base, red capsule Crawlers, and a fixed orthographic isometric camera.
- Configurable logical grid, cell terrain/buildability/walkability/cost, and reusable A* navigation.
- ScriptableObject Crawler configuration: 100 HP, speed 2.5, damage 10, attack interval 1 second, attack range 1.6 world units.
- Automatic test spawner: first spawn after 1 second, then every 3 seconds, cycling three sea spawn points.
- Base health starting at 100, shared damage API, repeated attacks, centralized Game Over, and an observing UI Toolkit HUD.
- Grid, path, attack-range, and spawn Gizmos; persisted prefabs/data/materials; idempotent Editor setup menu.
- 17 EditMode and 4 PlayMode test cases.

## Architecture

### Grid

`BattlefieldDefinition` contains dimensions, cell size, coastal band sizes, and the base cell. `GridManager` creates a per-session `BattlefieldGrid` at its world origin. Defaults are 14 x 20 cells at 1.5 units each, a 21 x 30 unit arena. Water occupies the highest four Z rows, beach the next four, ground the remaining twelve. The base occupies cell (7, 2).

The grid is axis-aligned on XZ; its origin is the minimum outer corner, conversion returns cell centers, and maximum outer edges are out of bounds. Water, beach, ground, and base are walkable in this step. Only ordinary ground is buildable. `SetCell` changes runtime data, increments a revision, and emits `Changed`. Grid state never mutates the definition asset.

### Pathfinding

`PathfindingService` implements four-way A* with a Manhattan heuristic. Entering a cell pays its traversal cost, which must be finite and at least one to preserve heuristic admissibility. A blocked or unreachable endpoint returns failure and an empty result. Paths include both endpoints.

One service per grid reuses its score/parent/open-state arrays; each mover owns its result buffer. Requests are synchronous on the simulation thread, with a simple linear scan of the open set appropriate to this 280-cell prototype. Search runs on spawn and after grid changes, never routinely each frame. Multiple grid edits before a mover's next tick coalesce into one request. An unreachable enemy waits for another grid change. Replanning from mid-cell returns via the current cell center to avoid cutting a new diagonal through a blocked neighbor.

### Enemy

`EnemyController` wires explicit dependencies and coordinates one per-frame tick. `EnemyMovement` follows grid waypoints using a distance budget based on delta time, clamps the final segment to attack range, and draws diagnostics. `EnemyCombat` accumulates attack time only after arrival, waits one full configured interval for the first hit, then applies repeated damage. `EnemyHealth` owns independent mutable health and requests removal on death. Definition assets contain configuration only; the capsule is a separate presentation child.

### Base and damage

`BaseHealth` owns a configurable maximum and an instance of the reusable `Health` model. `IDamageable.TakeDamage(int)` is the shared damage boundary. Non-positive damage is ignored, lethal damage clamps to zero, and health/death events notify observers. Further damage at zero has no effect. Current/max HP and alive state are exposed without writable HP fields.

### Game state

`GameStateController` owns Initializing, Playing, and GameOver and listens to base death. Spawner and enemies consult this state; UI observes its events. It has no singleton, static mutable state, scene lookup, economy, wave ownership, or presentation responsibilities. Execution order initializes grid and base before gameplay begins. A scene reload starts a fresh session.

### Spawner

`EnemySpawner` holds serialized prefab, grid, base, state, spawn transforms, and parent references. It instantiates the configured prefab and explicitly initializes its gameplay dependencies. It stops its timer/spawning after Game Over. It is a repeatable testing aid, not a wave manager.

### UI

`PrototypeHud` observes base health and game-state events. It updates `BASE HP: current / maximum` and shows `GAME OVER`. UXML, PanelSettings, and theme assets are saved normally. UI does not apply damage, control spawning, or own game state.

## Scene

`Assets/Game/Scenes/CoastalDefense_Prototype.unity`

The scene is registered as the enabled build scene. Camera rotation is X=35.264 degrees, Y=45 degrees; it is orthographic and frames the complete arena. Player Base and Crawler use saved prefabs under `Assets/Game/Prefabs`.

## Controls

None. Enemies spawn and attack automatically. Stop Play Mode and press Play to restart.

## How to Run

1. Open this repository in Unity 6000.3.24f1 with an activated license.
2. Open `Assets/Game/Scenes/CoastalDefense_Prototype.unity`.
3. Press Play and observe the full cycle, including Game Over in approximately 18 seconds at default settings.

No Inspector assignment or setup command is needed for normal play. **Tools > Shoreline Defense > Build Phase 1 Prototype** creates a missing scene or opens the saved one. Invoking it twice was verified to retain 10 root objects without duplication. **Rebuild Prototype Scene From Definition** deliberately replaces the scene layout after map configuration changes; preserve custom scene overrides separately before using it. It retains existing prefab/data assets. Runtime never reconstructs the scene through this tool.

## Tests

Executed in Unity 6000.3.24f1 through the official Pipeline Test Runner:

| Suite | Passed | Failed | Skipped |
|---|---:|---:|---:|
| EditMode | 17 | 0 | 0 |
| PlayMode | 4 | 0 | 0 |

EditMode covers coordinate round trips with nonzero origins, boundary cases, invalid cells/costs, shortest valid paths, blocked cells, weighted detours, runtime cost changes, unreachable paths and recovery, identical endpoints, terrain policy, health damage/events/clamping, independent health, and reentrant death notifications.

PlayMode covers the persisted scene from water spawn through beach/ground traversal, repeated base attacks, UI HP changes and Game Over, frozen enemies and stopped spawning; one-time Game Over and scene reset; coalesced path recalculation and recovery after blocked routes reopen; and independent spawned enemy health/removal without changing shared definitions.

The first PlayMode run exposed a test cleanup scene-name collision and an unnecessary AudioListener referencing a disabled module. Both were fixed; all four passed on rerun. The initial test compilation mismatch in an NUnit collection assertion was also fixed before the passing run.

Recorded results: [EditMode](Docs/Validation/editmode-results.json), [PlayMode](Docs/Validation/playmode-results.json).

## Validation

- Unity script compilation passed with no compiler errors.
- Opened the saved scene through Unity and verified repeatable setup behavior.
- Ran the scene at normal simulation speed and visually inspected rendered Game view captures: [sea spawn and 100 HP](Docs/Validation/play-start.png), [travel across the zones](Docs/Validation/play-travel.png), [attacks with 60 HP displayed](Docs/Validation/play-damage.png), and [zero HP / Game Over](Docs/Validation/game-over.png).
- Verified a normal run stopped at six total spawned enemies after Game Over, with the count unchanged on a later inspection. Automated checks additionally assert all enemy positions remain unchanged after Game Over.
- The live Console reported zero errors, zero warnings, and no failed compilation following the tests and normal-speed playthrough.
- A Windows x64 standalone player build succeeded with zero errors and one advisory: Pipeline is disabled in player builds. This is intentional; the final build includes no running automation server. The output is `Build/ShorelineDefense.exe` (ignored generated output). [Build summary](Docs/Validation/windows-build.json).
- Strict project integrity checks passed with no missing/orphaned metadata, duplicate GUIDs, conflict markers, invalid manifest, or Editor-version drift. Saved scene and prefab inspection found zero missing scripts and zero broken serialized object references. [Integrity summary](Docs/Validation/project-integrity.json).
- Reviewed gameplay ownership, initialization, subscriptions, serialized references, shared data, hot loops, navigation allocation/recalculation, and runtime/editor assembly separation.

## Known Limitations

- Only Step 1: no structures, player input, economy, waves, rewards, networking, saves, final art, animation, or audio.
- Enemies overlap; there is no occupancy, crowd avoidance, or pooling yet. Navigation assumes a point agent, axis-aligned cells, and static world origin. Four-way routes turn at cell centers.
- Traversal cost influences route selection; it does not yet impose a movement-speed modifier. No destructible obstacle, attack line-of-sight, or runtime placement policy exists.
- Runtime grid edits trigger route refresh, but map dimension/origin/coastal-layout changes require scene regeneration outside Play Mode. Placement must later reject cells occupied by an enemy or intentionally define how to evacuate them.
- At zero HP enemies freeze in place. Restart uses Play Mode or scene reload; there is no restart menu.
- A* is deliberately simple and synchronous. Hundreds of simultaneous route requests would need profiling before choosing batching, a priority queue, or another navigation strategy.
- Unity can issue a startup advisory about the built-in input backend. No gameplay input is used in this step. The local incomplete URP installation was repaired; see [environment details](ENVIRONMENT_SETUP_REPORT.md).
- Windows builds emit a harmless Pipeline-disabled advisory from the official automation package. The standalone build was compiled successfully; interactive visual validation was performed in the Unity Editor, not in the standalone executable.

## Future Compatibility

- **Building placement:** cell buildability, walkability, traversal cost, and change events provide the starting boundary. Add placement validation/occupancy when implementing defenses.
- **Waves:** a future wave system can replace the test spawner's timer while using the same prefab initialization boundary.
- **Economy:** enemy death is already explicit; rewards/accounting can subscribe later, without putting currency in the health or UI layer.
- **Additional enemies:** definitions, prefabs, movement, combat, and health have separate responsibilities. Specialized movement/attacks can extend those responsibilities without duplicating grid navigation.
- **Multiplayer:** gameplay owns health, positions, damage, and game state; presentation observes it. Movement and combat accept explicit delta-time ticks. This is a feasible authority boundary, not networking implementation or a determinism guarantee.

## Next Step

The next planned step is now **Phase 2B — Defense Placement + Economy + Machine Gun Nest**, following the Phase 2A orientation conversion. Not implemented; requires an explicit new request.
