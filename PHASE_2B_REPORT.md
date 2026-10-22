# Phase 2B — Defense placement, economy, and Machine Gun Nest

## Status

**COMPLETE** — verified in Unity 6000.3.24f1, including mouse-driven Play Mode validation on 2026-09-17 (Europe/Sarajevo).

Primary scene: `Assets/Game/Scenes/CoastalDefense_TopDown.unity`.

## Implemented

- Configurable economy, event-driven money display and build-button affordability feedback.
- Defense ScriptableObject, reusable placement command, selection controller, mouse input adapter, reusable grid preview, and footprint occupancy.
- Persisted top-down MG prefab with independent health, nearest-enemy targeting, rotating weapon, hitscan damage, reusable muzzle flash and tracer.
- Scene-owned enemy registry, one-time kill reward flow, immediate target invalidation and deferred object cleanup.
- Game Over integration, focused tests, saved scene wiring, and updated rebuild tooling.

## Economy

`EconomyManager` owns runtime currency and exposes `CanAfford`, `TrySpend`, `AddMoney`, and `MoneyChanged`. Starting money is serialized on the scene component: **500**. Defense cost comes from its definition: **150**. Crawler kill reward comes from `Crawler.asset`: **25**.

`EnemyHealth` records whether the lethal hit came from a player defense. `EnemyController` removes the dead enemy from `EnemyRegistry`, which publishes one kill notification. `EnemyRewardService` credits the economy only during Playing and only for defense kills. Unattributed damage, disabling, despawning, and post-game deaths earn nothing. No turret or UI script owns currency. Money observers cannot turn a committed payment into a failed build by throwing.

## Placement

Click the MG button, then left-click the battlefield. Right-click or Escape cancels without spending. Selection remains active after building while another MG is affordable; it exits when funds fall below the selected cost or Game Over occurs. The button dims and reports the money shortfall, then re-enables after sufficient income.

`PlacementInput` raycasts the screen position onto the existing XY gameplay plane and uses the existing world-to-cell conversion. It blocks world placement over HUD elements. `PlacementController` owns selection and the persistent preview, colored green for valid and red for invalid cells. The preview is reused, including while moving outside the map.

`BuildManager.TryPlaceDefense(definition, anchor, out defense)` checks game state, valid definition, every footprint cell's bounds, Ground terrain, buildability, occupancy, and affordability. The lower-left anchor and arbitrary positive rectangular footprint support 1×2, 2×2, and 2×3 structures. Gameplay roots are centered on the footprint. The MG uses 1×1.

Creation and initialization happen under an inactive staging parent before payment. The command revalidates, occupies the entire footprint, charges once, and activates the prepared defense. Failure before commitment removes the candidate and releases any reservation without spending. Reentrant placement attempts are rejected. Occupancy is owner-checked and cleared both on defense death and destruction.

Water, beach, blocked terrain, occupied cells, out-of-bounds cells, and the base are rejected. The primary map now reserves a walkable, non-buildable 3×3 base area centered on (29,9), covering its larger visual marker. The historical scene retains its original 1×1 base reservation.

**MG nests do not block enemy navigation in Phase 2B.** Enemies may pass through their cells, and placement over a moving enemy is permitted. Occupancy emits `OccupancyChanged` without changing navigation revisions or requesting A*. Terrain walkability/cost and the existing navigation `Changed` event remain available for future obstacles. No placeholder path-blocking validator was introduced.

## Machine Gun Nest

`Assets/Game/Data/Defenses/MachineGunNest.asset` references `Assets/Game/Prefabs/Defenses/MachineGunNest.prefab`.

| Setting | Value |
|---|---:|
| Cost | $150 |
| Health | 300 |
| Range | 5 cells / 6.25 world units on the primary map |
| Damage | 20 |
| Fire rate | 4 shots/second |
| Footprint | 1×1 |

The enemy registry finds the closest live enabled enemy in XY distance; equal-distance ties retain registration order. Each MG reacquires at 0.15-second intervals and validates its current target before firing. Disabled, destroyed, dead, out-of-range and post-game targets are invalid. An elapsed-time cooldown carries fractional time between frames and does not bank idle shots. An isolated 100-HP Crawler takes five successful hits.

`MachineGunCombat` applies damage through `EnemyHealth` and the shared `Health.TakeDamage` API. It emits firing feedback separately. `MachineGunPresentation` rotates only the weapon around Z, aims the muzzle, and reuses one tracer sprite and one flash sprite per turret. No projectile objects, physics queries, animation system, audio, or final artwork were added.

## Enemy Death

Health clamps at zero and emits death once. The controller's health guard stops movement and attacks immediately, the registry removes targetability synchronously, and Unity destroys the GameObject at the end of the frame. A second hit before destruction cannot repeat death or reward. The spawner prunes destroyed references during its next spawn attempt.

## Architecture

```text
BuildHud → PlacementController ← PlacementInput
                   ↓
           BuildManager placement command
          ↙           ↓              ↘
  BattlefieldGrid  EconomyManager  DefenseBase
                                   └─ MachineGunCombat → EnemyRegistry
                                         ↓                   ↓
                               MachineGunPresentation    EnemyHealth
                                                             ↓ death
                                               EnemyRewardService → EconomyManager
```

`GameStateController` remains the session-state owner. Dependencies are serialized in the scene or passed explicitly at spawn/placement. No singleton, duplicate state manager, command bus, multiplayer, or new package was added. `Phase2BSceneSetup` authors ordinary saved assets and references; normal Play does not run setup. The existing top-down scene rebuild also includes Phase 2B.

## Tests

Final Unity Pipeline Test Runner results:

| Suite | Passed | Failed | Skipped |
|---|---:|---:|---:|
| EditMode | 37 | 0 | 0 |
| PlayMode | 14 | 0 | 0 |

[EditMode results](Docs/Validation/Phase2B/editmode-results.json), [PlayMode results](Docs/Validation/Phase2B/playmode-results.json).

New coverage includes starting balance, exact spending, insufficient funds, invalid amounts, income/events/overflow, observer failure isolation, terrain/buildability rejection, bounds, partial-footprint rejection, owner-checked occupancy cleanup, navigation remaining open, real 2×3 placement, creation failure without charge, the actual UI button selection event, five-hit kills, repeated death damage, uncredited/post-game deaths, money UI updates, affordability recovery, disabled/out-of-range targets, weapon aiming, long-frame cooldown, defense destruction, and Game Over. All 26 prior EditMode and nine prior PlayMode regression cases remain included.

Unity compilation passed. Following normal gameplay and Game Over the Console reported zero errors, zero warnings, and no failed compilation: [Console status](Docs/Validation/Phase2B/console-status.json). One test intentionally throws an observer exception and verifies that it cannot corrupt payment; the exception is explicitly expected by that test. A temporary automation request during Play Mode domain reload was retried after the Editor became ready; this was not a gameplay error. A prior interrupted session was restarted after recompilation, since runtime state is not serialized for hot reload.

Scene/prefab checks found zero missing scripts or broken object references, one of each manager, and one preview. Running the scene setup twice retained 15 roots. Asset checks found no missing/orphaned metadata or duplicate GUIDs. [Scene integrity](Docs/Validation/Phase2B/scene-integrity.json), [file integrity](Docs/Validation/Phase2B/file-integrity.json). Reviewed the added runtime/editor code, modified enemy/grid call sites, saved configuration, scene/prefab references, and tests; `git diff --check` passed. No standalone player build was rerun for Phase 2B; validation used the Unity Editor.

## Manual Validation

Ran the saved primary scene at its actual 1920×1080 Game view resolution and used Windows mouse/keyboard input on the real HUD and battlefield. Simulation time was held at zero for placement inspection and paused at selected shot/reward boundaries for captures; enemy movement, aiming, firing, damage, and kills ran at time scale 1 between those checkpoints. No currency or enemy HP was injected into this interactive economy run.

- Confirmed $500 and the $150 button on a fresh scene. Button click entered placement mode; mouse movement snapped the reusable preview to cells. Green ground and red invalid previews were visible. Mouse clicks on ocean, beach, the base, outside the map, and an occupied MG cell were rejected without payment. HUD clicks over underlying grass did not place a defense. Escape and right-click both canceled without spending.
- Placed the first MG at (8,3): $500 → $350. Its weapon turned left around Z, with the gameplay root unrotated. Visually inspected the muzzle flash and tracer; the first shot left the Crawler at 80 HP. Five shots killed it; its object disappeared, registered enemies became zero, its target cleared, and money became $375 exactly once. [First shot](Docs/Validation/Phase2B/first-shot.png), [first kill](Docs/Validation/Phase2B/first-kill.png), [runtime snapshot](Docs/Validation/Phase2B/first-kill.json).
- Built MGs at (8,8) and (8,13), leaving $75. Selection exited, the button dimmed, and the HUD stated the $75 shortfall. Three further kills restored $150 and enabled the button. Used that money to place a fourth MG at (12,9), leaving $0. Four kills had produced 20 total shots; the base remained at 100 HP. [Insufficient funds](Docs/Validation/Phase2B/insufficient-funds.png), [income restoring affordability](Docs/Validation/Phase2B/earned-fourth-build.json), [fourth purchase](Docs/Validation/Phase2B/fourth-build.png).
- Reloaded the scene and deliberately covered only the bottom edge with a nest at (23,0). Let enemies travel and attack at normal speed until natural Game Over: base 0 HP, eight total spawns, one kill, and $375. Placement exited and the build button stayed disabled even after a click. Later snapshots confirmed unchanged money, turret shots, enemy positions, attacks, spawn count, and A* requests. [Game Over](Docs/Validation/Phase2B/game-over.png), [stopped-state comparison](Docs/Validation/Phase2B/post-game-over.json).
- Strict perpendicular camera, ocean left, vertical beach, ground field, and base right remained intact. Stopped Play Mode after validation and left the saved primary scene open.

Additional captures: [initial state](Docs/Validation/Phase2B/play-start.png), [valid preview](Docs/Validation/Phase2B/valid-preview.png), [invalid beach](Docs/Validation/Phase2B/invalid-beach.png), [first placement](Docs/Validation/Phase2B/first-build.png).

## Known Limitations

- MGs occupy build space only; enemy crowd avoidance, structure attacks, blocking route validation, and line of sight remain future work.
- The existing repeating prototype spawner remains; there is no WaveManager or victory state.
- Restart uses Stop/Play or scene reload. Runtime state is not preserved across script recompilation during Play; restart the session after changing scripts.
- No floating money numbers, pooling of enemies, final art, audio, upgrades, saves, multiplayer, or additional defense types.

## Performance Notes

Registry queries are allocation-free linear scans at an interval, suitable as a straightforward starting point for hundreds of enemies. No scene-wide discovery occurs in runtime targeting. Tracers and the placement preview are reused. Enemy spawning/destruction still allocates; profile before adding pools or a spatial index. Existing synchronous A* runs on spawn and navigation changes, not on ordinary build occupancy changes. Range/target Gizmos are selection-only; occupied-cell diagnostics use the existing GridManager Gizmos.

## Future Extension

New structures add a definition, prefab, and specialized behavior component, sharing placement, footprint reservation, economy, and health. Barbed Wire can later alter traversal cost through the navigation grid. A Mine can add proximity/detonation behavior; a Sniper Tower can add distinct combat/presentation tuning. Blocking variants must first validate remaining routes and update navigation with explicit change notifications. None of these structures are implemented.

## Next Recommended Step

**Ready for Phase 2C — Additional Defensive Structures + Wave System.** Not started.
