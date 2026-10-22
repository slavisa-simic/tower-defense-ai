# Phase 4A — Endless survival, border breach, enemy expansion and RSD money UI

## 1. Status
COMPLETE. Primary scene: Assets/Game/Scenes/CoastalDefense_TopDown.unity; Unity 6000.3.24f1. Implementation compiled, automated regressions passed, live gameplay and screenshots reviewed, and bounded stress/lifecycle checks completed. Scope excludes later enemies, defenses, audio, multiplayer, saves and backend. Unity is stopped with the saved primary scene clean; temporary validation fixtures were not saved. Pre-existing workspace changes were preserved. No commit or staging was performed.

## 2. Endless wave architecture
The existing WaveManager remains the only scheduler/state owner. Primary-scene EndlessSurvival settings select EndlessWaveGenerator; historical Wave1–Wave5 definitions and finite scheduling remain available to explicit regression fixtures. Preparation previews one generated transient WaveDefinition. Completion releases that definition and generates the next; no cumulative wave-asset list exists. Player Start Wave remains required. AllWavesComplete is unreachable in endless mode. Integer wave numbering saturates at int.MaxValue without ending normal waves or overflowing.

## 3. Difficulty formula
For age = wave - 1: budget = floor(10 + 3*age + .015*age²), clamped to [1,100,000,000]. Parameters live in EndlessSurvival.asset. Generated count caps at 180; spawner enforces 150 live enemies, with defensive hard bounds of 300. After wave 20, HP = min(20, 1 + .0125*(wave-20)); speed = min(1.6, 1 + .002*(wave-20)). Wave rewards = 100 + 5*(wave-1), saturating at int.MaxValue. Arithmetic uses double/long before clamps; NaN/infinite generation tuning is sanitized.

## 4. Enemy costs
Basic 1; Runner 2; Brute 5. EnemyDefinition owns difficulty cost, unlock, transport units, kind, breach penalty and existing gameplay stats. The generator spends affordable budget via weighted random selections. Basic starts with weight 60; Runner gradually increases from 15 toward 50 and Brute from 5 toward 25. Budget prevents early heavy compositions; later counts remain capped while modifiers/composition/transport pressure grow.

## 5. Unlocks and seed
Basic wave 1, Runner wave 3, Brute wave 6. Each is configurable on its definition. Normal runs select a new Guid-derived integer RunSeed once; WaveManager exposes it for debugging. Disable randomizeSeed to use configured seed 1977. Per-wave System.Random uses unchecked(runSeed*397 XOR wave), so the same settings, run seed and wave reproduce composition without retaining RNG/wave history. No seed UI.

## 6. Runner
60 HP, speed 4.5 versus Basic 2.5, reward 35 RSD, breach 50, cost 2, capacity 1. New saved Runner definition and prefab use existing Runner idle/run/attack/hit/death inventory and shared EnemyController/Health/Movement/Presentation. Run presentation 12 FPS. No special ability.

## 7. Brute
600 HP, speed 1.35, reward 75 RSD, breach 100, cost 5, capacity 3. Saved Brute definition/prefab use existing Brute inventory and shared behaviors; walk 6 FPS. MG sustained fire and sniper high damage apply through existing generic defense damage. No class-specific bonuses.

## 8. Boat grouping
WaveManager packs shuffled single-enemy composition entries into mixed cargo until capacity 5 is reached. BoatLandingSystem validates total weighted units, keeps a bounded cargo array, shows the appropriate body sprite and reserves three physical seat positions for a Brute. Unloading remains staggered and EnemySpawner alone initializes/registers enemies. Failed landings/active-cap attempts retain cargo and pending counts. Five original river points use round-robin distribution, skipping occupied/invalid lanes. Arrival interval = max(.65, 3/(1+.035*(wave-1))); concurrency grows from 1 to at most 3, one additional boat per 10 waves. Both are settings. Returning empty boats can finish during preparation; Game Over cancels all arrivals/cargo immediately.

## 9. Border breach architecture
BorderBreachService owns route destination selection and the breach command. It compares reachable routes to cells (31,4), (31,9), (31,14), chooses the lowest traversal cost and requires arrival within .15 world units. EnemyController passes arrival to the service rather than attacking BaseHealth when configured for border mode. Movement remains reusable navigation. A resolved latch commits exactly once before observers. Homicides/loss commit before registry removal, preventing a loss-causing final breach from paying a completion bonus. Breach deactivates/destroys the enemy and unregisters it through ordinary EnemyRemoved; EnemyKilled is never raised. BaseHealth/EnemyCombat remain usable for explicit historical tests, with no primary-scene enemy attacks on the static base.

## 10. Homicides
Initial 0. Configurable Basic 25, Runner 50, Brute 100. RunStatistics uses nonnegative saturating long totals and OnHomicidesChanged. UI subscribes/unsubscribes; no statistic polling. Breach uses removal without kill reward, death animation or death attribution.

## 11. Game Over
BorderBreachService.maxHomicides defaults to 1000. Values <=0 mean unlimited homicide sandbox. Threshold invokes existing GameStateController.EndRun, which closes build/wave controls, freezes gameplay and cancels transports. Homicides HUD omits the denominator for unlimited mode. Game Over displays BORDER DEFENSE COLLAPSED, highest started wave, Homicides, total and per-kind kills and actual credited money earned.

## 12. Border visuals
Existing strict-overhead quarantine fence segments, gate pieces, floodlights and warning markers form the right-edge checkpoint. Three existing road-tile exits continue to the boundary; no added collision/occupancy systems. The command-post visual is moved aside while preserving its root/components; an existing upper tent is moved clear of the upper exit road. Camera remains orthographic/perpendicular XY. Breaches produce brief red pooled UI labels near their world position, clamped inside the right screen edge and staggered across three rows per lane; no screen-wide flash. Homicides uses danger text and threshold progress. Final scene audit found no missing scripts or border colliders.

## 13. Statistics
RunStatistics records current wave, highest wave actually started, total/per-kind kills, breaches and Homicides. EnemyRegistry's single removal/kill notification is the event source. EconomyManager owns saturating long TotalEarned/TotalSpent, updated by committed actual credits/debits, excluding initial money and rejected payments. Homicide/breach observer exceptions are isolated and resolved enemies are removed in finally, so presentation cannot strand gameplay state. No permanent profile or leaderboard.

## 14. RSD UI
HUD is RSD n,nnn. All build prices and kill/wave reward labels use RSD. Existing UI Toolkit binding, selection, affordability, named controls and callbacks remain. Build strip widened and label font set to 10px to fit currency labels; Game Over panel supports multiple statistic lines. Money icon/panel are newly generated fictionalized notes in muted beige/olive/rust; no exact real banknote replica or baked text. Old HP icon is removed from primary HUD. Actual 1920x1080 captures were visually reviewed, including WAVE 100 / ENEMIES 180, every build-price suffix, breach labels and the statistics panel. The wave-100 screenshot is a controlled HUD fixture, not a natural wave-100 combat walkthrough.

## 15. Generated money assets
Built-in imagegen, references approved bd_ref_danube_master_v002.png and actual ref_gameplay_integrated_01.png. Raw sources are under Assets/Game/Art/SourceSheets/p4a_money_001; exact prompts, hashes and selection records are under Docs/Art/Batches. Runtime UI: ui_icon_money_rsd.png (32x32), ui_icon_money_bundle_rsd.png (32x32, inventory), ui_money_panel_rsd.png (160x64). Unity authoring script records nearest sampling, hard alpha threshold 240, no-dither 5-bit channels, 2px margin, icon aspect preserved, panel fit. Prior game-ready pixel-export authorization and current small HUD integration scope support this conversion. Point/uncompressed/no mipmaps/Clamp/NPOT None, UI 100 PPU. Panel uses existing 16px nine-slice. Raw native source bytes retained.

## 16. Coin replacement
Primary USS now references banknotes. Old bd_ui_money_icon_v001.png moved with metadata through AssetDatabase to Art/UI/Archive, preserving GUID/source as historical rollback, with no runtime consumer. Historical source sheets/master screenshots remain provenance. Existing economy values are unchanged by the art replacement.

## 17. Automated results
Final full suites: **71/71 EditMode and 39/39 PlayMode passed** (PlayMode 156.49 seconds). After a final small HUD OnEnable correction, the focused breach/Game Over/re-enable test passed **1/1**; the full suite was not repeated for that correction. Final compilation succeeded; Console inspection reported **0 errors and 0 warnings**. Evidence: [EditMode](Docs/Validation/Phase4A/editmode-tests.json), [PlayMode](Docs/Validation/Phase4A/playmode-tests.json), [focused HUD check](Docs/Validation/Phase4A/hud-reenable-test.json), [Console](Docs/Validation/Phase4A/console-final.json), [asset audit](Docs/Validation/Phase4A/asset-audit.json).

Initial PlayMode was 36/37: its sole failure was a historical boat route assertion expecting base (29,9), corrected to check configured border destinations. No damage, capacity, reward or lifecycle assertion was weakened. Historical base/finite fixtures explicitly select their original paths; presentation assertions now expect RSD. Nine new generator test cases and six survival PlayMode tests cover unlocks/costs/seeds, cap/scaling, mixed cargo, every enemy/weapon role, wire/mine interactions, once-only breaches, unlimited mode, threshold ordering, statistics and 100-wave cleanup. Initial failed evidence is retained in playmode-first.json.

## 18. Endless simulation
Initial real transport/movement simulation: 100 completed waves, 10,562 breaches, 460,350 Homicides, 34,750 RSD completion income, peak 3 active enemies and 3 boats. The 100-wave transport test also passed in subsequent full-suite runs, including the final run. Each simulation uses real boats and enemy route movement with accelerated deterministic ticks and Unity cleanup yields. Every wave returned to Preparation, zero enemies/transports after cleanup, at most one transient generated wave retained; final progression explicitly verifies WAVE 101. This no-defense sandbox removes enemies on routed arrival; it is lifecycle evidence, not a claim of 150 simultaneous enemies or natural combat balance. Seed variation can change composition totals between runs. High-wave generation covers 1000 and int.MaxValue without arithmetic overflow.

## 19. Manual Play Mode
Seven consecutive natural-update waves completed in live Play Mode at 4x time scale, seed -1886260500. This was **script-driven live interaction plus visual inspection**, not human mouse-only testing: the real Start Wave UI callback and BuildManager purchases were used, then normal movement, combat, boats and wave completion ran. Starting purchases were Sniper, Machine Gun and Wire, followed by affordable reinforcements.

| Wave | Enemies / kills | Homicides | Completion bonus RSD | Ending balance RSD |
|---|---:|---:|---:|---:|
| 1 | 10 | 0 | 100 | 350 |
| 2 | 13 | 0 | 105 | 505 |
| 3 | 14 | 0 | 110 | 685 |
| 4 | 14 | 0 | 115 | 975 |
| 5 | 19 | 0 | 120 | 1375 |
| 6 | 16 | 0 | 125 | 1625 |
| 7 | 19 | 0 | 130 | 2105 |

Runner first appeared at wave 3 and Brute at wave 6. Final totals: 105 kills (83 Basic, 20 Runner, 2 Brute), 3730 RSD earned, 2125 spent, no breaches. [Natural results](Docs/Validation/Phase4A/natural-waves.json). Separate controlled boundary fixtures visually verified each body sprite, +25/+50/+100 breach feedback, and threshold Game Over. natural-game-over.png preserves the seven-wave statistics; the later game-over.png is a fresh fixture with zero started waves/kills and 200 Homicides. Screenshots reside in Docs/Validation/Phase4A. An initial stalled attempt after the test window retained Time.time at zero; focusing GameView and enabling background execution corrected it. That attempt is retained separately and excluded from successful results. Scene settings/fixtures were restored by stopping Play Mode.

## 20. Performance
Live Editor stress spawned 150 mixed enemies and rejected enemy 151 at the configured cap. Spawn duration was 49.827 ms. A roughly three-second natural-update sample recorded 887 frames, median 3.261 ms, p95 4.071 ms and maximum 128.305 ms; 141 enemies remained at the end of the sample. The maximum is a real observed spike; Editor/script-call overhead may contribute, so this is not a standalone shipping benchmark. After mass removal and cleanup, zero enemies and UI popups remained and SpriteRenderer count returned to its original 672. [Stress evidence](Docs/Validation/Phase4A/stress.json).

Existing VFX/corpse/decal pools (capacity 256) and 24 UI popup slots remain bounded. Spawner prunes destroyed/deactivated references; boats destroy returned visuals and clear cargo on cancellation. Event observers unsubscribe on disable. Generator releases replaced definitions. The 100-wave lifecycle test adds evidence against retained wave/enemy/boat accumulation. No standalone Player FPS or memory certification is claimed.

## 21. Known issues
No outstanding implementation blocker was found. Dynamic camera fit retains noninteger pixel scaling. Long-term balance remains prototype tuning; seven combat waves and accelerated 100-wave lifecycle tests do not establish late-game balance. Standalone Player profiling and extended real-time sessions remain necessary, particularly to investigate the observed 128.305 ms stress spike. The latest Console inspection was clean; pre-existing Input Manager deprecation is outside this change. Human mouse-only walkthrough was not performed.

## 22. Next phase
Ready for an explicitly authorized next phase, with the prototype balance/performance limits above. No next phase was started. Recommend dedicated balance and standalone performance review before shipping; audio and other later systems remain separately scoped.

## Evidence and review
Focused runtime diff review covered 13 existing scripts (baseline copies and changed-existing-scripts.txt are retained in Docs/Validation/Phase4A) and four new runtime classes. No packages were added. Author.cs records scene/prefab/import operations through Unity APIs; saved legacy regression assets remain available. [Art batch and prompts](Docs/Art/Batches/p4a_money_001.md), [asset register](Docs/Art/ASSET_REGISTER.md), [raw source integrity](Docs/Validation/Phase4A/raw-source-integrity.json). The three original generated source files match their preserved copies byte-for-byte. Report and evidence describe the final state; early screenshots/test failures remain historical diagnostics.
