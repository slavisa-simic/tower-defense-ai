# Phase 2C — Additional defenses and wave system

## Status

**COMPLETE** — implemented and verified in Unity 6000.3.24f1 on 2026-09-17. Final suites: **42 EditMode and 22 PlayMode tests passed**, with no failures or skipped cases. Interactive Game view checks and controlled runtime inspections passed. The gameplay Console recorded zero errors, zero warnings, and no compilation failure.

Primary scene: `Assets/Game/Scenes/CoastalDefense_TopDown.unity`. Play Mode was stopped after validation, leaving the saved primary scene open. Phase 3A was not started.

## Defenses implemented / build costs

| Defense | Cost | HP | Behavior |
|---|---:|---:|---|
| MG Nest | $150 | 300 | 20 damage, 4 shots/sec, 5-cell range, nearest target |
| Barbed Wire | $50 | 200 | 50% movement speed while crossing; 2× traversal cost; no damage |
| Land Mine | $75 | 1 | One 150-damage explosion, 1.75-cell radius |
| Sniper Tower | $300 | 250 | 100 damage, 0.5 shots/sec, 11-cell range, shortest remaining route |

All four use 1×1 footprints. Values come from saved `DefenseDefinition` assets under `Assets/Game/Data/Defenses/`, with ordinary saved prefabs under `Assets/Game/Prefabs/Defenses/`. A cell is 1.25 world units: MG range is 6.25, sniper range 13.75, and mine explosion radius 2.1875 world units.

## Defense architecture

The Phase 2B `BuildManager`, placement command, economy transaction, occupancy ownership, `PlacementController`, mouse adapter, preview, and `DefenseBase` remain shared. No defense-name/type branches were added to placement. New behavior consists of `BarbedWire`, `LandMine`, and the shared hitscan turret components.

`TurretCombat` and `TurretPresentation` contain the existing MG firing, cooldown, aiming, flash, and reusable-tracer behavior. Thin `MachineGunCombat` / `MachineGunPresentation` subclasses preserve the existing MG prefab's serialized component identities. The MG prefab and definition were not recreated. The sniper uses the shared components directly with different data and cyan tracer tuning. There is one combat/presentation component per turret, verified in the asset audit.

The build HUD observes an ordered defense catalog and generates its buttons from data. Each button shows name/cost and independently updates affordability. Selection switches through the existing controller; one preview shows the selected footprint, green validity, or red rejection. Escape and right-click cancel. UI owns neither money nor wave progression.

## Barbed Wire behavior

Wire registers an owner-checked runtime grid overlay while active. The overlay changes traversal cost and local movement speed without changing `EnemyDefinition` or overwriting terrain. Destruction/disable removes only its own overlay, including after an unrelated terrain edit.

Enemies sample the current cell's slowdown directly. Movement splits its time budget at cell boundaries, so even a long tick enters and exits slow cells correctly. Adjacent wires cannot leave a stale slow behind. `MovementModifiers` supports additional source-owned runtime slows: modifiers multiply, a repeated source replaces its own value, removal is independent, and the combined status multiplier has a 0.05 floor. Wire's multiplier combines with these status effects. Actual overlapping wire footprints are prevented by build occupancy.

No wire performs a global enemy query, and wire deals no damage.

## Land Mine behavior

The mine arms after 0.35 seconds and checks the existing live-enemy registry every 0.05 seconds. Its trigger radius is 0.55 cells. Dead and disabled targets are ineligible. It uses proximity checks rather than turret targeting or added physics components.

The explosion latch is set before damage/callbacks, preventing reentrant or repeated detonation. A caller-owned victim buffer permits registry removals during AOE damage without skipping victims. Damage goes through `EnemyHealth.TakeDefenseDamage`; the mine never adds currency. It releases its placement cell, disables combat, and is destroyed. A detached orange flash remains briefly to show the explosion; it does not retain build occupancy.

## Sniper behavior

The sniper selects the in-range enemy with the shortest remaining calculated route to attack range. `EnemyMovement.RemainingPathDistance` combines distance to the next waypoint with the four-way route suffix. The query is O(1) and requires no additional A*. Unreachable/invalidated routes report infinity until movement replans; arrived enemies report zero. Tests include an obstacle detour that reverses straight-line ordering.

It fires 100-damage hitscan shots approximately every two seconds, normally killing a basic 100-HP Crawler in one shot. Only the weapon visual rotates around Z. Its cyan, wider, longer-lived tracer differs from the MG's yellow tracer. Existing cooldown behavior, target release, and reacquisition are shared.

## Wave architecture

`WaveManager` owns preparation, scheduling, wave membership, remaining counts, completion, and progression. `EnemySpawner.TrySpawn` only executes requests, cycles coastline points, and checks walkability and separation. Failed attempts are retried without decrementing pending counts. A long frame never forces a stacked burst of all overdue spawns.

States are `Preparation`, `Spawning`, `Combat`, `WaveComplete`, `AllWavesComplete`, and `GameOver`. The session-level `GameStateController` remains authoritative for whether gameplay may run. `WaveHud` observes wave changes and issues the explicit start command.

The wave tracks its own spawned enemies in a set. Completion requires zero pending enemies and zero surviving wave members; unrelated registry enemies cannot hold a wave open. Registry removal notifications cover death, disable, and destruction. Disable is treated as removal from this prototype wave; reactivation/pooling of old wave enemies is not implemented. The existing enemy kill notification remains the sole source of kill income.

Completion changes state before issuing the bonus, preventing repeated death/removal callbacks from paying twice. A 1.5-second completion message precedes the next preparation phase. There is no automatic next-wave start. Finishing the configured fifth wave displays ALL WAVES COMPLETE and disables Start Wave.

## Wave configuration

| Wave asset | Crawlers | Spawn interval | Completion reward |
|---|---:|---:|---:|
| Wave1 | 10 | 0.75 sec | $100 |
| Wave2 | 15 | 0.75 sec | $100 |
| Wave3 | 20 | 0.75 sec | $100 |
| Wave4 | 25 | 0.75 sec | $100 |
| Wave5 | 30 | 0.75 sec | $100 |

Each `WaveDefinition` contains a wave number, configurable reward, and ordered enemy groups with prefab, count, interval, and optional group delay. Additional enemy types can later use the same groups; only Crawler is implemented. The UI displays the real configured maximum of five waves. During preparation it previews the next scheduled count; during spawning/combat it shows pending plus living wave members.

The primary scene has **zero** `PrototypeSpawnLoop` components. That explicit repeating test scheduler exists only in the historical Phase 1 scene and in the old regression test fixtures. Those fixtures deliberately opt into their original cadence so their previous assertions remain meaningful. Both scene builders preserve their respective behavior; normal gameplay never invokes scene setup.

## Economy flow

Starting money remains $500. `BuildManager` spends each definition's cost once. Defense-attributed enemy deaths pass through `EnemyRegistry` → `EnemyRewardService` → `EconomyManager`, awarding $25 per Crawler. WaveManager awards the configured $100 bonus once after completion. Cleanup and post-Game-Over deaths do not generate income. Building and all combat/spawn behavior stop on Game Over; no completion bonus is paid afterward.

## Pathfinding impact

All four defenses remain enemy-walkable, preserving the established MG navigation behavior. Their build occupancy still prevents building on each other. No blocking-defense option or incomplete route validator was introduced.

Wire overlays raise A* traversal costs, so a cheaper detour can be preferred. Placement/removal produces a navigation revision once per footprint. Existing enemy invalidation coalesces grid changes and replans on the next movement tick, not every frame. Ordinary MG, sniper, and mine placement changes build occupancy only. Future blocking defenses must add real route validation first.

## Test results

| Suite | Passed | Failed | Skipped |
|---|---:|---:|---:|
| EditMode | 42 | 0 | 0 |
| PlayMode | 22 | 0 | 0 |

[EditMode results](Docs/Validation/Phase2C/editmode-results.json), [PlayMode results](Docs/Validation/Phase2C/playmode-results.json).

All 37 previous EditMode and 14 previous PlayMode cases remain included. Added coverage checks independent movement modifiers; cell-boundary slowdown at different tick sizes; walkable, occupied wire; weighted routing and owner cleanup; obstacle-sensitive progress; persisted costs/stats/waves; real HUD selection and affordability; wire entry/exit; mine arming/dead-target rejection/AOE/outside-radius behavior/single detonation/removal/rewards; sniper range/priority/damage/cooldown/reacquisition; all 100 scheduled enemies across five waves; pending/live count accuracy; final-survivor completion; exactly-once bonuses; blocked-spawn retry; Game Over; and a real first-wave smoke fight using only the normal $500 budget.

Unity compilation passed after the final changes. A per-movement-tick params-array allocation found during review was removed, and new definition fields reject invalid values before placement payment. Both suites passed again afterward. The final gameplay [Console status](Docs/Validation/Phase2C/console-status.json) records zero errors/warnings and no compilation failure. The old economy test intentionally logs and expects an observer exception. Unity's existing Input Manager deprecation advisory can appear at Editor startup; it is unrelated to Phase 2C.

Scene/prefab inspection found no missing scripts or broken references, exactly one of each manager, one preview, valid definitions/waves, and no primary-scene test loop. Running the setup again retained 16 scene roots. [Scene integrity](Docs/Validation/Phase2C/scene-integrity.json). Source/metadata and final diff checks are recorded in [file integrity](Docs/Validation/Phase2C/file-integrity.json). No standalone player build or 300-enemy performance benchmark was run for this phase.

## Manual Play Mode validation results

Used the real Unity Game view at 1920×1080, with Windows mouse/keyboard input on the build bar and battlefield. The camera remained perpendicular to XY, ocean left, beach vertical, and base right. Simulation ran at time scale 1; selected event boundaries were held at time scale 0 for inspection/capture.

- Verified $500, all four names/costs, selection switching, invalid beach rejection without spending, mine rejection over an existing sniper, and Escape/right-click cancellation. Placed sniper (19,9), wire (25,9), and MG (23,9): $500 → $200 → $150 → $0, with affordability dimming. [Initial HUD](Docs/Validation/Phase2C/play-start.png), [invalid terrain](Docs/Validation/Phase2C/invalid-beach.png), [prepared defenses](Docs/Validation/Phase2C/prepared-defenses.png).
- Clicked Start Wave. Enemies appeared progressively across the coast. The first sniper shot killed one Crawler at long range, rotated the weapon, showed a cyan tracer, and added $25. Snapshot: six spawned, four pending, five alive, nine remaining. [Shot image](Docs/Validation/Phase2C/first-sniper-shot.png), [snapshot](Docs/Validation/Phase2C/first-sniper-shot.json).
- Observed the wave remain in Combat with ten spawned, one survivor, and $225 earned; the MG had fired ten shots and the sniper seven. [Final survivor](Docs/Validation/Phase2C/last-enemy.json).
- The Editor was closed during a user pause, losing runtime state. Recreated the same paid opening through the shared build command in a fresh session, with no money/HP/damage injection, and let it fight normally. Completion yielded $350, zero enemies, one completed wave, base HP 100, and only ten A* requests. [Completion](Docs/Validation/Phase2C/wave-complete.png), [snapshot](Docs/Validation/Phase2C/wave-complete.json). After the delay, Wave 2 stayed in Preparation with 15 scheduled enemies and $350. [Next-wave state](Docs/Validation/Phase2C/next-wave-ready.json).
- Selected and mouse-placed a mine at (6,9), observing a green preview and the exact $75 payment ($350 → $275). [Valid mine preview](Docs/Validation/Phase2C/valid-mine-preview.png).
- For isolated interaction inspection, temporarily disabled the turrets and spawned/repositioned diagnostic Crawlers through the shared spawner. One walked through the built wire: speed 2.5 → 1.25 → 2.5, definition speed unchanged, wire occupied/walkable with cost 2. [Entry](Docs/Validation/Phase2C/wire-enter.json), [exit](Docs/Validation/Phase2C/wire-exit.json), [image](Docs/Validation/Phase2C/wire-enter.png).
- In the same controlled inspection, two Crawlers inside the mine blast died, the outside target stayed at 100 HP, the mine disappeared, one orange flash appeared, and two ordinary kill rewards changed $275 → $325. [AOE snapshot](Docs/Validation/Phase2C/mine-aoe.json), [explosion](Docs/Validation/Phase2C/mine-explosion.png). Restored the turrets, then mouse-started Wave 2: three alive plus twelve pending equaled fifteen remaining. [Wave 2 started](Docs/Validation/Phase2C/wave2-started.json). Diagnostic enemies were separate from wave membership.
- Reloaded and deliberately defended only the bottom edge with an MG at (23,0). Started Wave 1 and let normal enemy attacks cause Game Over: base 0 HP, ten spawns, two kills, $400, eight survivors, no completed wave. Start Wave and building stayed disabled. After another 25.9 seconds and a click on the disabled Start Wave button, money, spawns, enemy positions/health, turret shots, and A* counts were unchanged. [Game Over](Docs/Validation/Phase2C/game-over.png), [stopped-state comparison](Docs/Validation/Phase2C/stopped-state-comparison.json).

Five-wave scheduling/rewards were verified automatically; the interactive normal combat run covered Wave 1 completion and Wave 2 starting. Controlled wire/mine inspections are distinguished above from the normal-budget fight.

## Known limitations

- Prototype visuals/UI only; no final sprites, animation, audio, upgrades, saves, or multiplayer. HUD panels overlay portions of the map.
- All defenses remain walkable; enemy attacks on defenses, blocking route validation, line of sight, and crowd avoidance are not implemented.
- Five finite waves only. Restart uses scene reload or Stop/Play. Script recompilation or closing the Editor does not preserve runtime progress.
- Mine proximity is sampled at 20 Hz and uses point-target distances. Very fast future enemy types may require swept detection; the current 2.5-speed Crawler is covered.
- No known Phase 2C gameplay blocker. Larger-scale balance, performance profiling, and pooling remain later work.

## Performance notes

Turrets use allocation-free registry scans at 0.15-second acquisition intervals and O(1) route-progress scores. Mines scan the same registry at 0.05-second intervals while armed and reuse their AOE buffers. Wire has no Update or enemy scan. Movement uses grid-local lookups and does not allocate a temporary params array. A* buffers remain shared/reused and replanning is driven by navigation changes. Runtime hot paths have no scene-wide searches or LINQ.

The expected 100–300-enemy scale has been considered structurally, not benchmarked. Synchronous A* may spike after a large navigation edit, and registry scans scale with defenses × enemies. Enemy instantiation/destruction and one flash per mine still allocate. Profile actual workloads before adding a spatial index, path budget, or pools.

## Next recommended phase

**Ready for Phase 3A — Final Art Pipeline + First Complete Art Set.** The defense/wave prototype loop is verified. Phase 3A has not been implemented.
