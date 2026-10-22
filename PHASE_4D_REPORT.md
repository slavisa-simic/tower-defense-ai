# PHASE 4D — Extreme assault rebalance and pressure director

## 1. Status
PARTIAL. The implementation, Unity compilation, automated regression checks, live combat fixtures and 100–400 enemy stress measurements are implemented and tested. Human mouse-only difficulty testing remains outstanding; the earned-money scripted run survived Waves 1–2 and failed on Wave 3. Separate prepared-layout checks completed Waves 4–5 and collapsed at 10, 20 and 25. Do not interpret these prepared fixtures as an earned-money progression. No Phase 5A work, packages, new enemies, defenses or art were added.

Primary scene: Assets/Game/Scenes/CoastalDefense_TopDown.unity, Unity 6000.3.24f1. Extensive pre-existing workspace changes were preserved; no commit or staging. Before-change runtime scripts and focused changes are retained in Docs/Validation/Phase4D/baseline-scripts and runtime-changes.diff.

## 2. Before/after enemy counts
Seed 1977, measured against the actual Phase 4C build before changes:

| Wave | Previous planned | New planned | Ratio |
|---|---:|---:|---:|
| 1 | 16 | 160 | 10.00 |
| 5 | 38 | 380 | 10.00 |
| 10 | 52 | 520 | 10.00 |
| 20 | 87 | 870 | 10.00 |
| 25 | 139 | 1,381 | 9.94 |
| 50 | 289 | 2,881 | 9.97 |

EnemyQuantityMultiplier is 10 in the existing EndlessSurvival settings asset. The original seeded budget/composition is evaluated in reference units; each selected group and each escort formation is expanded by 10. Exactly one boss remains per encounter. Budget accounting scales accordingly. Maximum standard planned units is 2,400, with additive boss and scaled escorts; extreme-index generation remains bounded. Counts intentionally exceed several rough suggested ranges to honor the explicit order-of-magnitude increase. This never instantiates the whole plan at once.

## 3. Global health multiplier
GlobalEnemyHealthMultiplier = 3.0. EnemyDefinition assets retain their base HP. All normal and special infected receive the runtime multiplier through the existing spawner and EnemyController initializer. Defense damage, movement archetypes and breach penalties remain unchanged.

## 4. Boss health multiplier
BossHealthMultiplier = 3.0 independently of normal enemy tuning. Exact formula:

`EffectiveHP = ceil(BaseHP * WaveHealthMultiplier * (isBoss ? BossHealthMultiplier : GlobalEnemyHealthMultiplier))`

The boss factor replaces the ordinary global factor, so these defaults produce 3x, not an accidental 9x. Wave scaling is applied once; armor remains a separate damage rule. Without wave growth the bosses are 24,000 / 18,000 HP. At Wave 25 Osama is 25,500 HP (previous 8,500); at Wave 50 Warlord is 24,750 HP (previous 8,250). Large values retain integer-safe damage and float-ratio HUD fill.

## 5. Pressure director architecture
AssaultPressureDirector is a lightweight pure policy object owned by the existing WaveManager, which remains the scheduler and pending/alive count owner. No duplicate manager or UI-owned gameplay state. Five evaluations per simulated second consider wave number, live enemies, undeployed enemies, incoming passengers, active boats and whether the wave boss is awaiting deployment or alive.

The low/high hysteresis band is 20%. Committed pressure includes live enemies plus incoming cargo. Low pressure accelerates dispatch to .15 seconds; high live pressure lengthens cadence. Dispatch pauses when committed pressure fills the upper band and resumes as casualties/breaches free room. This avoids filling the river queue beyond the intended field pressure. A cargo batch can overshoot the band by its capacity; this is deliberate bounded streaming, not an exact cap. At most one new boat is scheduled per manager tick; each boat independently unloads at most one unit per tick. No giant one-frame enemy spawn operation.

An optional development-only UI Toolkit debug observer shows wave, alive/pending enemies, active/estimated pending boats, target, mode, surge, peaks, average frame time and current-wave earned RSD. It is disabled by default and suppressed in release builds. Pending boat count is explicitly an estimate because cargo weights vary. No preset menu was added; all difficulty values live in the existing settings asset.

## 6. Active enemy targets
Target = min(320, 50 + 8 * (wave - 1)). Waves 1/5/10/20/25/50 target 50/82/122/202/242/320. Hard spawner safety limit is 400; the previous 300 code clamp was removed. The director controls ordinary pressure without redefining the hard stress limit. A proposed first-wave ramp was tested and removed because spreading attackers reduced useful area-weapon clusters and performed worse.

Baseline no-defense Wave 1: peak 16, average 7.62 live enemies over 30 real seconds, cap 200, speed 5, capacity 7, concurrency 2, interval 2.2. Baseline frame-time snapshot was 1.59 ms; this single snapshot is not a frame distribution or a matched defended benchmark. Final mixed-load distributions are recorded below.

## 7. Boat concurrency
Configured maximums at Waves 1/5/10/20/25/50: 6/8/10/15/18/28; surges can add 3, bounded by 32. Final live prepared fixtures observed 7 boats at Wave 4, 8 at 5, 10 at 10, 14 at 20 and 13 at 25. This is actual overlap, not a claim every maximum is always filled.

Old single-inbound-boat-per-sector admission was an additional bottleneck. Incoming boats now queue behind occupied berths/approaches with hull clearance. Landing selection weighs current lane occupancy and recent use with deterministic sequence jitter. Five original landing sectors, river-only geometry, scene roots and hull assets are preserved. Returning hulls follow the existing return lanes; inbound/outbound presentation can overlap during departure and should be reviewed during human play.

## 8. Boat speed changes
Runtime speed 5 -> 9 world units/second (1.8x), with existing boss craft speed factors retained. The original 6.625-unit approach takes about .74 seconds before craft-specific factors. First dispatch is .25 seconds after Start Wave; with the original entry position and hull extent, the leading hull enters the visible river roughly .4–.7 seconds after start at normal time. This visibility timing is a geometry estimate, not a high-speed video measurement.

## 9. Boat capacity
Capacity 7 -> 12 weighted transport units; heavy and boss weights remain unchanged. Existing passenger seat artwork is reused and is not expanded into new art. Unload interval .15 -> .07 seconds; departure delay .2 -> .15. Independent boats unload concurrently. A .4-unit disembark separation replaces the old 1-unit spacing during director-driven endless waves, enabling dense formations and useful area damage without changing enemy movement or navigation. Legacy finite fixtures retain their own motion/separation until explicitly configured.

## 10. Arrival intervals
Static baseline is max(.3, 1 / (1 + .08*(wave-1))): Waves 1/5/10/20/25/50 = 1/.758/.581/.397/.342/.3 seconds. Director acceleration reaches .15 while room remains; actual boss state can shorten normal cadence. Departure, available entries, population limits and safe landing routes still backpressure dispatch.

## 11. Assault surge logic
Optional seven-second surges every 24 simulated seconds on Wave 5+, while undeployed enemies remain. They add three boat slots and accelerate cadence, but still respect committed population, safety limits and river entry clearance. No repeated giant banners. Optional settings allow independent later tuning.

## 12. Economy impact
Starting money remains 500 RSD. Kill rewards, completion rewards, defense prices and 50% dismantle refunds are unchanged. RunStatistics now records current/last-wave earned income and average pressure/frame time. Live earned-money mine run: Wave 1 earned 4,100 RSD including completion; Wave 2 earned 5,955. Expendable construction consumed that income and the run entered a fatal Wave 3 with only 55 RSD. This is not evidence of free-map Rocket affordability.

Prepared fixtures injected 18,000 RSD before construction, disclosed in their receipts and excluded from reported combat income: Wave 10 earned 2,720 before collapse; Wave 20 1,925; Wave 25 1,190. Prepared early waves earned 8,360 and 10,540 at 4/5. No reward rebalance was justified from these short samples; successful later farming still needs monitoring.

## 13. Pathfinding performance
Existing movement already repaths on navigation changes, not every frame. The dense fixture made 2,080 route requests, including repeated landing validation, destination selection and initialization. A bounded 2,048-entry start/goal cache shares identical paths until the grid navigation revision changes; caller buffers remain separate and cached failures are revision-scoped too. Final 400 fixture: 255 actual A* searches, 1,825 cache hits, versus 2,080 searches before caching. Navigation/dismantle tests verify invalidation.

Pooling initially revealed a real regression: OnEnable invalidated a freshly initialized inactive enemy path, temporarily making progress scores infinite and preventing progress-based targeting. Activation now preserves a freshly valid path while still invalidating stale revisions/changed starting cells. The targeting regression and full suites pass. Individual repaths after large edits are still synchronous; standalone profiling should examine rare edit hitches.

## 14. Targeting performance
MG/Sniper/Cannon retain .15-second acquisition, registry queries and squared-distance range checks. Mortar/Rocket retain bounded cluster candidates and reusable snapshots; Flame retains its .2-second cone/radius tick. No combat-loop full scene scans were added. Stress fixtures place enemies where all weapon types can actually fire; per-weapon shot counters are in the JSON receipts. No spatial-partition redesign or speculative package was needed for the measured load.

## 15. Pooling / VFX optimization
Enemies are pooled per prefab by the spawner, with at most 400 retained idle enemies. Registry removal and inactive state are immediate; reuse is deferred to LateUpdate so reward/death observers finish. Reinitialization clears HP/armor attribution, burn, movement modifiers, combat clocks and presentation; target registration is restored exactly once. Excess idle entries are destroyed. Boats reuse prefab-specific visual pools after departure/cancellation, resetting hull and seat state.

Existing VFX pools remain bounded at 256 total effect renderers, including bursts, tracers, smoke, decals and corpses. Death animation/decals do not retain gameplay enemies. Burn remains at most one overlay per enemy, bounded by active/pool counts. The 24 existing popup slots aggregate nearby kill rewards over a .2-second window; currency calculations remain separate and exact. Gameplay damage never depends on a displayed effect.

Projectiles were audited and retained as short-lived objects: actual simultaneous shots are bounded by structure cooldowns/travel time. They remain an allocation source, as do cargo arrays and UI formatting. The measured GC behavior did not justify rewriting stable projectile/presentation ownership during this pass. Pooling removes repeated enemy/boat instantiation but does not eliminate first-use creation or all allocation.

## 16. Wave simulation results
Full EditMode: 97/97 passed. Full PlayMode: 51/51 passed (206.88 seconds), recorded in playmode-final-envelope.json. Final compilation succeeds; the Console was archived after tests, then checked with zero errors/warnings. The saved primary scene is clean and stopped with zero missing scripts. The real-boat 100-wave test checks exact counts/rewards, nonnegative/empty pending queues, eventual completion, Preparation through Wave 101, bounded pools/boats, and at most one transient generated WaveDefinition. It exercises requested waves 1/5/10/20/25/50 and both alternating bosses. No win/difficulty claim is inferred from its accelerated no-defense breach simulation.

The initial 42/51 runtime result and its fixes are retained in playmode-envelope.json. Destroyed-object assertions were changed to verify removal/inactivity under pooling; immediate-dispatch and old boat-cap expectations were updated. The finite-wave damage smoke now admits lethal single Sniper hits, which need not leave a damaged survivor for polling. The actual pooled-path targeting bug was fixed rather than weakening its assertion.

## 17–20. 100 / 200 / 300 / 400 enemy stress tests
Five-second natural Update samples at TimeScale 1, 1920x1080 Editor Game View, 21 mixed defenses, Wire, Flame/burn, Mortar, Rocket, actual boats and bounded effects. High initial enemy HP and unlimited breach threshold are isolated stress overrides to keep a measurable population; incoming cargo and breaches change counts. All weapon shot counters are included.

| Initial | Actual range | Median ms | p95 ms | Maximum ms |
|---|---:|---:|---:|---:|
| 100 | 100–135 | 3.24 | 4.00 | 58.40 |
| 200 | 183–235 | 4.23 | 4.93 | 100.68 |
| 300 | 252–335 | 5.51 | 6.99 | 187.98 |
| 400 | 327–400 | 6.20 | 7.67 | 282.53 |

No generation 0/1/2 collections occurred during these short samples. Allocations and managed-memory growth remain; exact averages/deltas are in stress-100/200/300/400.json. Maximums include the first frame following synchronous fixture construction, so these are not hitch-free claims. At 400 the setup itself is about 267 ms; production streams individual passengers rather than using this deliberate one-frame stress setup. First-use creation/Editor setup remains the dominant observed spike; steady targeting/movement stayed below 8 ms p95 on this host. Do not infer a standalone Player guarantee or a long-term memory certification.

Wire, MG and Rocket dismantle during an active Spawning state with 327 -> 311 live enemies returned exactly 25/75/450 RSD, rejected repeat refunds and cleared footprint ownership. Wire neighbor mask changed to zero and navigation revision changed. Nonblocking MG/Rocket correctly do not revise navigation. Receipt: dismantle-under-load.json.

## 21. Manual difficulty test
These are script-driven live Play Mode actions and visual screenshot inspection, not a human mouse-only walkthrough. Start Wave uses the real UI submit event; purchases use BuildManager, with normal Update-driven combat at 3x game time. Multiple fixed openings (Sniper/MG/Wire, Flame, Mortar, mine combinations and reinvestment) collapsed on Wave 1. A rapidly adaptive mine strategy completed Waves 1–2 from the actual 500 RSD without injected income, then failed Wave 3. It made 394 kills across the first two waves; third-wave Runner pressure exposed depleted construction reserves. A slower mine/mixed-weapon attempt killed 116 on Wave 1 but lost at 1,000 Homicides. Wave 1 is therefore not mechanically impossible, but the successful opening relies on rapid scripted decisions and does not establish human fairness.

Separately, an injected-budget 21-defense layout completed Wave 4 with 287 kills/150 Homicides and Wave 5 with 374 kills/225 Homicides. That same held layout collapsed at Wave 10 (80 kills, 1,000 Homicides, peak154/10 boats) and Wave 20 (55 kills, 1,000 Homicides, peak232/14). This is strong evidence against the former casual-idling behavior, but no earned-money progression through 5/10/20 was completed. Screenshots and early-0 through early-8 receipts disclose the attempts. Early combat took roughly 13–35 simulated seconds in these automated decisions, below the suggested 1–2 minute duration; pacing still merits human review.

## 22. Boss wave test
Final prepared Wave 25: 1,381 planned hostiles, peak279 active enemies, peak13 boats, Osama health bar 25,500/25,500, visibly overlapping multi-sector assault and escorts. The layout collapsed at 1,040 Homicides after 36 kills; it did not defeat or breach the boss before collapse. This is a loss, not an omitted result. Boss-under-load mechanics use a separate disclosed 150-standard-enemy fixture; its receipt is boss-under-load.json. Existing automated transport/HP/armor/HUD/kill reward/breach checks and 100-wave simulation cover both bosses and completion.

## 23. Known issues
- No human mouse-only playthrough; ordinary starting layouts fail quickly, and the successful scripted mine opening is demanding. Human survivability and reasonable durations are unverified.
- No earned-money progression through Wave 5, 10, 20 or 25; prepared fixtures are deliberately not represented as earned runs.
- First-use/setup hitches, allocations, long-session GC/memory and standalone Player performance remain unverified. Synchronous repaths after edits may still hitch despite cached repeated routes.
- Existing generated Osama art limitations from Phase 4C remain outside this gameplay scope. Existing sprite sorting, HUD and strict XY camera are preserved.
- Sector queue presentation can show returning/incoming hull overlap. Current maximums are limits, not proof of sustained 28-boat populations.
- Static pressure targets and exact 10x group expansion need additional player-layout sampling. No claim every rough suggested duration/count range was met.

## 24. Recommended next phase
Finish Phase 4D validation with human difficulty play and sustained standalone profiling before declaring it complete or proceeding to Phase 5A. Keep the active 10x/3x assault settings for that review; do not revert the game to easy defaults. No next phase was started.

