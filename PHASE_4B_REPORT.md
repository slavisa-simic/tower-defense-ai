# PHASE 4B — HEAVY DEFENSES + TACTICAL EXPANSION

## 1. STATUS

COMPLETE. Four defenses, generated art, VFX, data assets, prefabs and build UI are integrated in `CoastalDefense_TopDown`. Unity compilation succeeds, EditMode passes 77/77, PlayMode passes 45/45 and the final Console contains zero errors or warnings. Existing work in the checkout is preserved; no commit or next phase was started.

## 2. DEFENSES ADDED

MortarPosition, FlamethrowerPosition, AutoCannon and RocketLauncherPosition use the existing DefenseDefinition / BuildManager / DefenseBase architecture. EconomyManager remains the currency owner. New weapons read EnemyRegistry and apply ordinary defense damage through EnemyHealth, preserving rewards and kill accounting. No new manager, enemy ability, boat system or multiplayer system was added.

All structures use XY placement and shared occupancy. These defenses are walkable, matching the existing MG/Sniper convention; their footprints occupy build cells without changing navigation. Existing blocking placement route validation is retained. Saved asset references and original catalog indices are preserved; visual button order is separate.

## 3. FINAL PROTOTYPE STATS

Ranges and splash radii below are grid cells, converted using the existing 1.25-unit cell size. HP is structure health.

| Defense | Cost RSD | HP | Footprint | Range | Damage | Rate | Extras |
|---|---:|---:|---|---:|---:|---|---|
| Mortar | 400 | 250 | 2×2 | 12 | 120 AOE | 1 / 3 seconds | Minimum 3; splash 2.25; travel .8 s; visual arc 2.5 units |
| Flamethrower | 300 | 350 | 1×1 | 3.5 | 12 per .2 s | 5 ticks/s | Cone 55°; burn 2 s at 15 DPS |
| Auto Cannon | 600 | 450 | 2×2 | 7.5 | 55 single target | 2/s | Highest current health |
| Rocket Launcher | 900 | 300 | 2×2 | 10 | 250 AOE | 1 / 5 seconds | Splash 2.75; travel .65 s; straight flight |

Additional weapon configuration lives in HeavyWeaponData ScriptableObjects referenced by DefenseDefinition. Existing defenses do not require this optional data.

## 4. TARGETING STRATEGIES

Existing closest and furthest-progressed enum values keep their serialized indices. HighestHealth and BestCluster are appended. Auto Cannon chooses highest current HP, with stable registry order for ties. Flamethrower aims at a nearby enemy and damages all enemies within its geometric cone.

Mortar/Rocket score at most 16 candidates against one snapshot of the active registry. Score prefers nearby predicted enemies, then their combined current health, then path progress, with stable order as final tie breaker. Positions are led using current facing, speed and remaining path distance. This is a bounded approximation rather than an exhaustive global optimum. Mortar candidates and impact centers must lie inside its range annulus; a prediction outside it falls back to the current candidate position. Splash can reach enemies inside the minimum targeting radius.

## 5. MORTAR IMPLEMENTATION

AreaProjectileWeapon queries only when its cooldown is ready; an empty search retries after .25 seconds. TimedAreaProjectile retains an impact point, independently of target lifetime, and displays an XY parabola over .8 seconds. It resolves radius damage once through shared AreaDamage, then destroys itself. Visual arc offset does not change damage position. Game over cancels unresolved shots. Structures and friendly objects are not in the enemy registry and receive no splash damage.

The mortar has a separate base, rotating barrel, recoil frames, shell and impact explosion. The mathematical arc avoids physics and does not introduce XZ gameplay.

## 6. FLAMETHROWER / BURN IMPLEMENTATION

FlameWeapon collects enemies in range into a reused list and tests cone membership using distance and a dot product every .2 seconds. Each hit applies direct damage and refreshes EnemyDamageOverTime on surviving enemies.

Each enemy has one burn slot: strongest DPS and longest remaining duration win, without stacking damage from multiple sources. Fractional damage accumulates into integer defense damage; elapsed damage is capped at the remaining duration. Burn continues after the enemy leaves the cone. Death, disable and loss of targetability clear it. Rewards use the existing defense kill path once. This provides a small reusable status foundation without adding an unrequested status framework.

One cone renderer belongs to each flamethrower. BurnPresentation creates its small overlay only on first burn and disables it after expiration. Firing frames animate while the cone is active. Wire synergy comes from existing slow exposure, without special damage multipliers.

## 7. AUTO CANNON IMPLEMENTATION

Auto Cannon reuses TurretCombat with HighestHealth targeting and normal cooldown, reacquisition and defense damage. Its presentation opts into heavy muzzle, tracer and impact VFX while existing MG/Sniper behavior remains unchanged. It has an independent rotating weapon, recoil, damaged and destroyed states.

## 8. ROCKET IMPLEMENTATION

Rocket shares the point-retaining projectile and radius damage implementation with mortar, using its own damage, cooldown, radius and zero arc height. A killed target cannot cancel or redirect the stored impact point. Rocket projectile art, intermittent smoke and a larger explosion use the existing bounded effects service. No rigidbody, per-victim coroutine or target-reference homing was added.

## 9. ART GENERATED

Image generation produced a new four-defense lineup reference, four defense sheets and four effects sheets, plus corrected variants. References reuse the approved Danube master v002 and integrated gameplay image. The lineage, exact prompts and raw output paths are in `Docs/Art/Batches/p4b_heavy_prompts.json`. The selected lineup revision is recorded as agent-selected under explicit autonomous authorization; no human approval is claimed.

Each defense has base, rotating weapon, four-frame firing sheet, damaged and destroyed sprites: 20 new runtime defense PNGs. Heavy structures use 128-pixel logical canvases; flamethrower uses 64. Four 32-pixel build icons were exported. Raw sources and rejected revisions are retained under `Assets/Game/Art/SourceSheets/p4b_heavy_001`. Rejected flame assets had inconsistent facing or baked hardware; rejected rocket smoke included projectile pixels. Corrected variants separate these responsibilities.

Exports use nearest sampling, hard alpha, a no-dither limited palette, Point filtering, uncompressed textures, Clamp, no mipmaps and 51.2 PPU. Fixed cells and normalized weapon pivots reduce generated-frame drift. Existing compact world-aligned contact shadows are reused. Strict overhead silhouettes, Danube-left/base-right composition and gameplay-scale readability were visually inspected. New assets have their own GUIDs; old gameplay roots remain intact.

`Docs/Art/ASSET_REGISTER.md`, the batch record and image inventory record filenames, revisions, settings, targets and acceptance.

## 10. VFX GENERATED

Twelve four-frame sequences were generated/exported: mortar muzzle/shell/explosion; flame cone/burn flame/burn smoke; cannon muzzle/tracer/impact; rocket projectile/smoke/explosion. New VfxId entries are appended to preserve existing serialized identifiers. Damage and visual effects remain separate.

The existing VFX pool remains capped at 256 renderers. Flame uses one persistent cone renderer and burned enemies use one small lazy overlay. Rocket smoke is emitted at .12-second intervals through that pool. Low-frequency gameplay projectile objects have short fixed lifetimes; they are not pooled. Their population is bounded by placement/cooldowns and travel times, rather than an unbounded effect spawn loop.

## 11. BUILD UI CHANGES

UI Toolkit now wraps eight build options into two rows of four, retaining icons, full functional names, RSD prices, affordability/selection states and existing callbacks. Button logical size remains approximately 210×68. Start Wave and HUD bindings remain intact. Camera viewport reserves the bottom 30% for the taller panel and keeps its upper boundary at 86%; grid, PPU and XY orientation do not change.

Actual 1920×1080 and 1440×810 Game View checks confirmed eight buttons inside their panel and every one of the 576 cell centers reachable without HUD interception. See `cells-1920.json`, `cells-1440.json`, `hud-tests-final.json` and native HUD screenshots in `Docs/Validation/Phase4B`.

## 12. ECONOMY/BALANCE CHANGES

Starting money, rewards, boats and endless settings are unchanged. New costs gate heavier weapons: 900-RSD Rocket exceeds the 500-RSD starting balance. The earned-money fixture first bought Mortar at wave 4, Flame at 5, Cannon at 6 and Rocket at 7, using only existing income. Waves 1–14 earned 10,540 RSD, spent 6,975 and entered wave 15 with 4,065. Its 31-defense mixed layout retained MG, Sniper, Wire and Mine.

Waves 15–25 then used natural Unity Updates at 6× time, started through the live Start Wave UI callback. All completed with zero breaches; wave 25 ended at 21,690 RSD. This is a viable smoke-test layout, not a claim of final difficulty calibration. The generator was seeded to 1977 in the fixture after initial wave creation; saved settings were not changed.

Controlled role evidence (`balance-roles.json`): Flame killed six clustered Basic enemies within 2 s; MG killed one in the same window. Mortar and Rocket killed all six within a separate 4-s window. These different windows should not be read as a direct DPS comparison. Against one Brute over 2 s, Cannon dealt 275, MG 180 and Sniper 200. A moving Brute took 402 flame damage on free ground versus 447 starting on Wire over 8 s. MG retains cheap sustained coverage, Sniper longer precision coverage, Wire exposure control and Mine cheap expendable AOE. No blanket economy or existing-defense rebalance was necessary.

## 13. PERFORMANCE DECISIONS

Registry access replaces repeated scene queries in gameplay targeting. Area radius buffers and a per-weapon cluster workspace are reused. Cluster work runs when ready to fire, with candidate count capped at 16. An initial 300-enemy benchmark exposed repeated native property reads: 2.712 ms/query. Snapshotting position, prediction, progress and health once per enemy reduced this to .459 ms/query across 1,000 queries, with zero measured managed bytes after workspace creation (about 5.9× faster).

Final Editor stress: 300 enemies initially, 19 defenses, 194 natural frame samples over 3 s, median 14.17 ms, p95 24.00 ms, maximum 67.18 ms; 194 enemies remained and four projectiles were active. Cleanup returned enemies/projectiles to zero and sprite renderers to the same 722 baseline. See `stress.json` and `stress-cleanup.json`. This is Editor sampling, not a standalone Player guarantee. The older pre-optimization capture includes a 3,053-ms sample caused by its synchronous microbenchmark; final sampling explicitly excludes that benchmark interval.

## 14. AUTOMATED TEST RESULTS

EditMode: 77/77 passed after the cluster optimization (.95 s). Final complete PlayMode: 45/45 passed (204.4 s), including all heavy-defense tests and the existing 100-wave real boat lifecycle regression. Results are recorded in `Docs/Validation/Phase4B/editmode-tests.json` and `playmode-tests.json`. Six heavy-defense tests cover placement/payment/footprints/UI, mortar minimum range/cluster prediction/arc/once-only splash, dead-target rocket impact, flame cone/burn refresh/expiry/reward, cannon HP targeting/cooldown/reacquisition, and game-over cancellation. Five cone cases and data validation extend EditMode coverage. Final Console inspection reports zero errors, zero warnings and zero logs (`console.json`); compilation completed without errors.

Initial full regression results (41/45) are preserved. Corrections expanded old four-option UI assertions, separated the mortar targeting dead zone from its valid splash area, and spread idle-art fixtures so new footprints do not overlap. The existing 100-wave accelerated lifecycle test retains its checks and has a 360-second timeout for Editor scheduling. No assertion was removed to manufacture success.

The final EditMode suite deliberately logs `InvalidOperationException: Expected observer failure` in the existing economy observer-isolation test; it passes using its expected-log assertion. Console history was archived in `console-history.json` before clearing that expected test exception, then the natural heavy-firing fixture was rerun to inspect a clean implementation Console. This diagnostic is not a runtime implementation error.

## 15. MANUAL PLAY TEST

Live Editor validation used authored C# fixtures and the real game components; screenshots were visually inspected. This was script-driven Play Mode testing, not a human mouse-only playthrough. Earned-income progression manually advances actual gameplay ticks through waves 1–14; waves 15–25 use ordinary Update, boat landings and the UI callback. `economy-simulation.json`, `natural-waves.json` and per-wave combat/completion screenshots preserve evidence.

Natural-update visual fixtures show the mortar/rocket firing and impact effects, heavy tracer, flame cone and enemy burn overlays. `heavy-firing.png`, `heavy-impact-burn.png` and `burn-after-flame.png` are native Game View captures including UI. Asset audit reports eight valid definitions, zero missing scripts, correct import filters and clean saved scene. Stress-fixture currency/breach-threshold overrides are isolated from the earned-money run and were discarded on leaving Play Mode.

## 16. KNOWN ISSUES

Prototype economy becomes comfortable with the tested mixed layout by wave 25; more player-layout sampling is needed for final balance. Cluster sampling approximates a best cluster and lead assumes current direction, so turns can cause misses. Frame-time results are from one short Editor stress sample, with a measured 67-ms maximum; standalone profiling remains future work. Generated sprites retain some stylistic variation and non-integer display scaling at these resolutions, consistent with the existing presentation. There is no known blocked gameplay feature or failing regression. Historical fixture/test failures are retained as diagnostic evidence and resolved in the final results.

## 17. RECOMMENDED NEXT PHASE

The project is ready to proceed to the user's selected next phase. A bounded difficulty/economy tuning pass using several mixed layouts and a standalone Player profile would be useful. No bosses, special abilities, upgrades, campaign, multiplayer, audio pass or platform integration was introduced. No next-phase work has been started.


