# Collision, navigation and defense combat overhaul

Date: 2026-11-02. **Complete.** Unity 6000.3.24f1 compiles successfully; final suites pass **140/140 EditMode and 90/90 PlayMode**. Final Console has zero errors/warnings. No additional production phase, new art, audio, multiplayer or map was started. Existing Phase 6B working changes were preserved.

## 1. Original cause of ghosting

`NAVIGATION_COLLISION_AUDIT.md` was written before the final implementation. Build occupancy did not affect walkability/revision, A* ignored defenses, and Transform movement trusted cached waypoints. Normal creatures had no defense obstruction combat. Danube Border had zero static blocked cells; Port already had 163 correct container/building/compound cells but omitted three dockside crane bases. All audited major props lacked Collider2Ds. The exact pre-change inspection is retained in `Docs/Validation/NavigationCollision/original-map-audit.txt`.

## 2. Static obstacle architecture

BattlefieldDefinition now owns explicit named/category-tagged rectangular `navigationObstacles`, alongside the retained original masks. These create permanent blocked terrain once when a grid is created. Saved scenes contain matching `StaticNavigationObstacle` IDs and simple BoxCollider2Ds under `Navigation Obstacles`. No per-frame sprite-bounds sampling, physics crowd pushing, static-object health, duplicate navigation manager or sprite replacement was added.

## 3. Dynamic defense occupancy

DefenseDefinition explicitly configures `BlocksMovement`. MG, Sniper, Flamethrower, Mortar, Auto Cannon and Rocket register their existing build footprint in a separate owner-scoped navigation overlay. BuildManager installs it before committing a placement; failed commands release it. Continue/restoration installs the same overlay. Placement rejects a solid footprint containing an active creature, including a Burrower; the preview and hint follow that contact rule. Construction occupancy remains independently queryable for selection and Wire connectivity.

Player walls are allowed. The former code did not actually implement a keep-one-route-open check; it checked construction occupancy/terrain/money only. This request replaces the older anticipated mandatory-route-preservation rule with real attack-through walls. Static layouts still require landing-to-breach connectivity. AGENTS.md records the new rule.

## 4. Navigation revision behavior

Each solid footprint addition/removal emits a navigation revision; Wire retains its separate traversal-cost revision. Ordinary A* and its bounded shared cache remain. Movement checks all configured breach exits, preserving the preference for any open border route. Dirty paths repath with a deterministic 0–0.30-second offset and a shared maximum of eight enemy repaths per rendered frame (each can check up to three exits). At every section, movement verifies the waypoint and cell being entered; a stale path stops before a solid cell. No teleport or pass-through recovery is used.

## 5. Structure obstruction resolution

`BlockingStructureResolver` is owned by the existing PathfindingService. A revision-cached reverse field for each queried exit considers permanent terrain impassable and player walls expensive but breachable. The first wall on its route identifies a useful blocking defense, and the returned approach contains walkable cells only. Crowds share these fields; there is no every-enemy/every-defense A* loop or per-enemy obstruction field. Actual searches/field counts are recorded by the density fixture.

Border destination selection and EnemySpawner accept destructible-wall routes, so walling cannot freeze incoming deployments. Permanent disconnections still fail. Creatures abandon a defense when an open objective route becomes available. When a revision invalidates an approach but the target survives, the approach is explicitly rebuilt.

## 6. Enemy attack-state changes

`EnemyStructureAttack` owns Moving → PathBlocked → Approaching → Attacking, separate from EnemyMovement. It uses the existing EnemyController clock and real DefenseBase damage. Attackers stop outside the footprint, vary explicit contact slots within a reachable cell, face the target and notify the existing creature attack presentation. A twelve-creature same-frontier test verifies twelve distinct safe contact positions. RemainingPathDistance includes the planned route beyond the obstruction, so reaching a contact slot does not incorrectly report border arrival to ClosestToBase targeting. Navigation preparation runs once per controller tick; ability movement consumes the same clock. Damage clocks are gameplay timing, independent of animation frames. Pooled enemies reset targets/timers and unsubscribe on disable.

Authored initial tuning (damage/interval, not final balance): Crawler 12/1s, Skitter 6/1s, Bulwark 45/1s, Spitter 24/5s, Broodling 3/1s, Burrower 18/1s, Dredger 120/1.2s, Broodmother 45/1s. Melee contact range is 0.6 cells; Spitter retains 5 cells and its visible flight/impact behavior. Bulwark breaks a 300-HP MG in about seven seconds of uninterrupted attacks; Dredger in about four seconds (three hits). Groups combine damage; cheap structures are not invincible walls.

## 7. Defense destruction flow and management

Existing Health remains the HP authority. Lethal damage is latched once, releases traversal/navigation/build ownership immediately, clears target/selection observers, emits death, deactivates the root immediately and defers object destruction. Turret target references clear on disable; weapons, smoke and child VFX stop with the root. Existing detached destruction presentation can finish independently. No destruction path calls an economy refund. Dismantle refunds once through the existing command and invalidates attacking creatures synchronously.

Existing Changed events keep selected HP, Repair cost and action validity live. Repair during an active wave retains the more expensive combat rate and does not pause attacks. Upgrade health preserves missing HP through the existing Health owner. Existing hit/impact VFX, damage smoke and damaged sprites provide feedback. In-flight projectiles already emitted before destruction retain their independent existing lifetime.

## 8. Wire and Mine exceptions

Both remain walkable. Wire retains path cost, sampled slowdown, owned cleanup, upgrades and connected masks. Mines retain arming, proximity detection and explosion, without a navigation wall. Existing build occupancy still prevents overlapping constructions. The solid-placement enemy-contact restriction does not prevent laying Wire/Mines on traversable cells occupied by creatures.

## 9. Burrower and boss exceptions

Burrower retains its single limited Wire-slow bypass along valid cell routes. It never ignores permanent geometry or solid defenses, and surfaces for obstruction combat. After surfacing normal rules apply. Dredger and Broodmother use the same wall/contact safety and real structure damage. Broodmother's existing reinforcement owner remains unchanged.

## 10. Industrial Port blocker audit

Final authored inventory: **177 permanent blocked cells**, **18 footprint records/colliders**: six container groups (24 container sprites), four crane footprints (landmark plus three dockside chassis), six building/compound footprints (warehouse, maintenance, shed and three checkpoints), forklift and logistics truck. Existing 163 blocked-mask cells remain. The new chassis footprints preserve x=6 quay access; treating the overhead boom as a solid building originally sealed two northern landings and was corrected during validation. Small decorative cargo/oil, ladders, markings and lighting poles remain nonblocking.

## 11. Danube Border blocker audit

Final inventory: **50 permanent blocked cells**, **12 footprint records/colliders**: Command Post, two permanent bunker-wall sections, two gate supports, two fence strips, three checkpoint gate supports and two sandbag fortifications. Gate/breach rows 4, 9 and 14 remain passable. Flat bunker floors, apron/road markings, weeds, debris and other small dressing remain decorative.

## 12. Automated test results

Final EditMode: **140/140 passed**, 0.95 seconds (`editmode-full.json`). Final PlayMode: **90/90 passed**, 290.91 seconds (`playmode-full.json`). Both have zero failures, skipped or inconclusive tests. Receipts under `Docs/Validation/NavigationCollision/` distinguish intermediate failed/cancelled attempts from final results. New fixtures test static detours, owner-scoped overlays, wrong-owner removal, stale long-frame movement, relevant frontier targets/shared fields, permanent-disconnection rejection, all six solid types on both maps, full walls/deployment, target-surviving revisions, live HP/repair/upgrade, dismantle/refund/selection, all eight creature types including bosses, Wire/Mine behavior, Burrower permanent geometry, distinct shared-frontier contacts/objective progress and dense crowds.

Regression fixture updates are intentional: old tests attempted to spawn inside the now-solid bunker or Mortar, swept management placements into Command Post, expected solid MG construction not to change navigation, or required immediate repathing. These fixtures now use valid ground/contact distances and explicitly allow the scheduled path refresh; original economy, HP, targeting, pooling, breach and lifecycle assertions remain.

## 13. 100-enemy test

Both maps pass with fully sealed mixed MG/Sniper/Flamethrower walls and another route change during movement. Each sample verifies every active enemy's occupied cell is traversable, walls receive real damage, and all enemies eventually breach. Final-run samples: Danube Border clears in 21.1 simulated seconds; Industrial Port in 20.2. No solid-cell entry or endless stalled crowd. See timing table below.

## 14. 200-enemy test

The same scenario passes with 200 active Crawler instances on each map, using the production spawner, movement, obstruction, health and breach owners. Danube Border clears in 21.5 simulated seconds; Port in 25.5. No solid-cell entry or endless stalled crowd. Test budget is explicitly increased to 50,000 RSD for fixture construction; this is not an earned-income balance certification. No weapon damage is applied by fixture turrets, allowing routing/attack-through completion to be observed.

## 15. Live Play Mode result

Passed on both maps with real Update-driven simulation and animation at 3x time, then paused for inspection. The reproducible Editor-controlled scenario is `Docs/Validation/NavigationCollision/Live.cs`; `live-results.txt` records completion. Inspected the six native camera captures (`*-open-routes.png`, `*-wall-attacks.png`, `*-wall-opened.png`): creatures follow corridors outside container/building footprints, stop at the wall, show attack/hit feedback, destroy a real defense and resume toward a breach. These camera renders omit UI; live selected HP/Repair text has separate UI assertions and a final composited capture. Human mouse-only play is not claimed. The CLI wrapper timed out at 30 seconds during this longer asynchronous script; the script continued and wrote its successful terminal receipt/captures. The session was then stopped normally.

Final 1920×1080 composited HUD capture `selected-damaged-defense-hud.png` was inspected: MG HP **276/300**, **Repair 5 RSD**, two real Crawler attacks, selected panel still bound. `hud-final.txt` records the Update-driven check. `saved-assets-final.txt` verifies zero missing scripts on both saved maps, exact collider centers/sizes for all static footprints, all six solid-defense colliders, Wire/Mine exceptions and the collision matrix. `port-navigation-debug.png` shows authoritative blocked cells and a real 28-cell A* crane/container detour, with every path cell traversable; the transient export creates no saved scene objects. Editor ends stopped in the clean saved Industrial Port scene.

## 16. Performance observations and development tools

Density v2 measures only the enemy gameplay ticks; occupancy assertions, scene lookups, screenshots, Editor rendering and NUnit instrumentation are outside the stopwatch. Final full-run samples (`density-results-v2.txt`, all remaining counts zero):

| Map | Creatures | Mean batch ms | Peak batch ms | New A* searches | New obstruction fields | Peak repaths/frame |
|---|---:|---:|---:|---:|---:|---:|
| Danube Border | 100 | 0.403 | 2.556 | 90 | 4 | 8 |
| Danube Border | 200 | 0.768 | 4.770 | 138 | 4 | 8 |
| Industrial Port | 100 | 0.343 | 0.704 | 50 | 3 | 8 |
| Industrial Port | 200 | 0.548 | 1.401 | 82 | 3 | 8 |

Earlier `density-results.txt` includes expensive test-side scene lookups inside its timing window and must not be used as gameplay performance evidence. These are Editor simulation samples, not standalone FPS or a long-duration profiler capture.

Select GridManager in the Editor to inspect cells: red permanent/terrain blockers, orange solid structure overlays, cyan nonblocking build occupancy, white open cells and yellow base. Select an enemy to inspect its cyan/red remaining path and destination. Gizmos are Editor-only. `MapRuntimeContext.ValidateStaticObstacles` uses a fresh terrain-only grid for every landing-to-exit check and reports category counts, invalid/zero/duplicate footprints and missing collider bindings; development startup warns for invalid setup. Named physics layers are EnvironmentObstacle=8, PlayerDefense=9, Enemy=10. Enemy–Enemy contacts are disabled; Environment/Defense query boundaries remain enabled. There are no dynamic crowd Rigidbodies. Unity's previously disabled built-in Physics 2D module was enabled; no external navigation package was added.

Final compile: completed, failed=false, compilationFailed=false. Final Console ground truth after archiving intermediate diagnostics and a fresh live damage/HUD check: **zero errors, zero warnings**, both during Play Mode (`console-final-playing.json`) and stopped (`console-final.json`). The TimeManager asset remains byte-identical to the captured starting version. Complete source/asset diff reviewed; `git diff --check` passes. No commit/staging/reset was performed.

## 17. Remaining known issues and scope

Spitter had no obstacle LOS before this work; its existing ballistic/splash behavior remains and can shoot across permanent buildings. A large LOS system was deliberately not introduced, as requested. Navigation footprints represent ground occupancy; roofs, crane booms, weapon barrels, shadows and large sprite silhouettes may overhang their cells. Old preparation saves that placed a structure inside newly solid permanent geometry fail the existing compatibility preflight rather than silently moving structures or modifying the save. Existing compatible saves retain levels/HP/currency/occupancy and rebuild blockers on Continue. Final balance, human-only usability and sustained standalone profiling remain outside this fix. No future phase was begun.
