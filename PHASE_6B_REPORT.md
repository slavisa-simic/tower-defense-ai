# Phase 6B — aquatic enemy overhaul and organic assault carriers

Validation date: 2026-11-02. Unity 6000.3.24f1. Strict overhead XY, Danube left / inland line right. No Phase 6C, new map, multiplayer, campaign, audio system or anti-carrier combat added.

## 1. Status

COMPLETE. Implementation, generated first-pass art/animations, both-map integration, regression, live validation and legacy Continue migration pass. Read the final validation summary below for exact terminal results and limits. No staging, commit, reset or unrelated package change.

## 2. Old → new enemy mapping

| Previous active asset | Current asset / role |
|---|---|
| Basic / Crawler | Crawler, foundational amphibious mass enemy |
| Runner | Skitter, fast breach pressure |
| Brute | Bulwark, armored heavy |
| Breacher | Spitter, new ranged defense attack |
| Heavy faction variant | Broodling, new tiny swarm role |
| Pirate runner variant | Burrower, limited visible infiltration |
| Raider variant | Retired from active catalogs; archived |
| First human boss | The Dredger |
| Second human boss | The Broodmother |

Data/prefab moves use AssetDatabase and preserve GUIDs. Explicit enum slots retain serialized references; slot six is reserved and absent from the active roster. Save migration maps old detailed kills by equivalent role, independently of which source asset was reused: old basic/breacher/raider → Crawler; old runners → Skitter; old heavy variants → Bulwark; boss slots → corresponding aquatic bosses. New special counters begin at zero.

## 3. New lore summary

LORE_BIBLE.md establishes a mysterious catastrophic change in the Danube ecosystem. Radiation, industrial contamination, disturbed deep-water habitats and abandoned research leave clues without a confirmed origin. Amphibious organisms threaten inland Serbia. HOMICIDES remains projected civilian casualties from creatures escaping the quarantine line. Current handbooks, production rules, map descriptions and README follow this premise; historical phase reports remain provenance.

## 4. Aquatic enemy roster

Crawler, Skitter, Broodling and Broodmother share Swarm-family biology; Bulwark and Dredger form the Armored family; Spitter and Burrower are Aberrant. Generation uses these original creature kinds and rotating biological emphasis, with no human-faction weighting. Shared gills, sensory pits, membranes, keratin plates, algae and mud unify the ecosystem.

## 5–6. Final prototype stats and unlocks

Values before existing endless HP/speed scaling. Capacity is ordinary units; half-unit arithmetic is used internally.

| Creature | HP | Speed | Armor | Reward RSD | Homicides | Wave cost | Unlock | Capacity |
|---|---:|---:|---:|---:|---:|---:|---:|---:|
| Crawler | 100 | 2.5 | 0 | 25 | 25 | 1 | 1 | 1 |
| Skitter | 60 | 5.0 | 0 | 35 | 50 | 2 | 3 | 1 |
| Bulwark | 600 | 1.35 | 25% | 75 | 100 | 5 | 7 | 4 |
| Spitter | 180 | 2.2 | 0 | 55 | 80 | 4 | 10 | 2 |
| Broodling | 18 | 3.8 | 0 | 2 | 5 | .5 | 5 | .5 |
| Burrower | 140 | 3.2 | 0 | 50 | 90 | 4 | 15 | 2 |
| The Dredger | 8000 | 1.15 | 35% | 2500 | 1000 | encounter 20 | 25 | 4 |
| The Broodmother | 6000 | 1.65 | 15% | 2200 | 900 | encounter 18 | 50 schedule | 4 |

Broodling budget selections expand to two larvae for one reference budget unit, respecting the configured planned cap. Existing fivefold formations, quantity events, active cap 400, pressure tuning, starting money and defense prices remain. Seeds reproduce the new roster, rather than reproducing obsolete compositions.

## 7. Spitter implementation

SpitterAttack is attached only to Spitters and clocked by EnemyController. A local grid search every .5 seconds selects the nearest living weapon defense within five cells (6.25 world units), excluding wire/mines. No per-frame scene scan. A five-second cooldown launches a visible generated corrosive glob; the attack pose briefly stops movement. Flight lasts .65 seconds to a locked impact position. Damage 24 resolves once through DefenseBase health, with small deduplicated footprint splash and pooled impact VFX. Dismantled/missing targets do not redirect the shot. Game Over cancels unresolved shots. Repair uses the ordinary BuildManager transaction.

## 8. Burrower implementation

One second of ordinary surface approach precedes a single configurable five-cell burrow. It follows existing valid navigation, never teleports or passes static/blocking terrain. Only wire movement slowdown is bypassed. Distance is limited further to leave at least eight cells of final approach. Burrowed units remain in registry/wave accounting but are excluded from targeting. Generated digging transition, moving mud trail and emergence frames remain visible; the contact shadow is disabled underground. Normal targeting, slow and breach behavior resume after emergence.

## 9. Broodling performance design

No Spitter/Burrower/Broodmother ability components on larvae. Minimal move/attack/death sprites, no per-hit burst or burn overlay, sampled detached death effects, ordinary pooled enemy lifecycle and bounded global VFX/popups. Existing movement, health, burn damage and registry targeting stay shared. No formal LOD or new package. Registry removal immediately updates Broodmother child ownership before a pooled object can be reused.

## 10–11. Arrival architecture and boat refactor

AssaultCarrierSystem contains the proven scheduling executor, weighted cargo, sector selection, independent unloading, entry/berth traffic checks, visual pools and cancellation. WaveManager remains the scheduler and pending/alive owner; AssaultPressureDirector remains its policy object. Existing serialized BoatLandingSystem is a thin inherited compatibility adapter, preserving scene GUID bindings. CarrierDefinition owns method, prefab, half-unit capacity, water extents, rupture/release/remains timings and compatibility. Arrival selection is deterministic by wave/dispatch sequence, configurable ratios and event overrides.

Actual carrier extents are swept through authored water masks. Larger Leviathans stop farther offshore; invalid lanes are skipped. Map startup validates a usable approach for every configured carrier. Collision spacing accounts for both carrier widths. No carrier enters EnemyRegistry or takes player damage. Future audio hooks expose creature attack/hit/death and carrier approach/rupture/boss emergence without an audio implementation.

## 12. Direct Emergence

Early direct ratio .6, later .3 per eligible dispatch. Crawler/Skitter/Burrower and Dredger are compatible. Capacity six ordinary units. The actual requested creature's submerged/emergence art moves through water with wake, then releases through existing landing starts. No targetable land entity exists before arrival; no ground shadow in water. Dredger gets its own large emergence presentation.

## 13. Drift Pod

Capacity twelve ordinary units. Floating/move/arrival/rupture/empty-remains frames. Short rupture, independent .07-second release cadence, about 1.1 seconds of organic remains, then cleanup. It ruptures rather than reversing as an empty boat. Tiny Broodlings occupy half units.

## 14. Brood Carrier

Capacity twenty-four ordinary units; multiple living organisms can unload concurrently. Lobed ray-like anatomy, brood chambers, arrival/release/retreat animations, .04-second releases. Default later-wave chance .25 outside direct selection, increased by Brood Tide/Deep Water events.

## 15. Leviathan Carrier

Capacity thirty-two ordinary units. Distinct armored head, gill/mouth brood release and submerging retreat. Carries heavy cargo and Broodmother, staying in validated water. No recursive carrier spawning or anti-carrier weapons.

## 16. Dredger implementation

Alternating boss cadence begins at 25, then 75, etc. Slow armored tank with direct deep-water emergence. Additive escorts: eight Bulwarks, twenty Crawlers and six Spitters before the existing formation multiplier. Ordinary arrivals continue. Shared armor, HUD, reward, once-only breach and wave completion remain.

## 17. Broodmother implementation

Boss waves 50,100,..., arriving with a Leviathan. Less armor, faster motion and reproductive pressure. Six larvae per eight-second interval, maximum sixty per lifetime and eighteen concurrent owned larvae. Spawns require valid land/route/space and the shared active cap. EnemySpawner raises reinforcement registration to WaveManager so every child gates completion. Killing/breaching/removing a child immediately frees ownership. Parent removal cancels further spawning; remaining children resolve normally. Escorts: twenty-four Broodlings, twelve Skitters and twelve Crawlers before formation multiplication.

## 18. Event migration

Thirteen events retained. Deterministic mappings: Pirate Raid → Swarm Surge; Militant Assault → Armored Migration; Heavy Assault → Deep Water Disturbance; Runner Swarm → Brood Tide; Mass Landing → Mass Emergence; Concentrated Landing → Concentrated Emergence. Biological weights and arrival ratios replace faction biases. Fog, flooding, supply, repair/upgrade pricing, warnings and sector lifetimes retain their shared owners. Development ForceEvent uses new IDs; no obsolete dropdown is exposed.

## 19–20. Map integration

Both maps ran before changes (baseline.json), resolving all eighty initial units through real arrivals/breaches. Both now support all four carrier definitions with water-safe approaches and all existing breach destinations. Original grid masks, containers, docks, terrain, defense footprints and checkpoint layouts remain. Both-map five-wave, encounter, arrival, ability and persistence receipts are under Docs/Validation/Phase6B.

## 21. Art generated

Built-in ImageGen directly generated three selected master lineups and twelve raw production sheets. Runtime: seventy-one four-frame sheets, 284 sliced sprites and seventy-one sprite-only clips covering six creatures, two bosses, three carrier designs and six biological/water VFX actions. Direct emergence reuses creature-specific frames. 64px standard, 32px larvae, 128px boss frames; carriers 192x96. Point/uncompressed/51.2 PPU/no mips/FullRect/center pivots. All runtime textures have only 0/255 alpha. Sources and references remain non-runtime textures.

Source dimensions are recorded rather than assumed; generated canvases vary. Export uses common per-sheet scale, centered opaque bounds, nearest sampling, 220 alpha cutoff and five-bit channels without dithering. Four-frame hit feedback retains the original .2-second lifetime. Runtime state animates presentation children only, with pooled detached deaths and restrained biological fluids. Selection is by the executing agent under the autonomous brief, not independent human art approval. Exact prompts, hashes, paths and consumer settings: batch p6b_aquatic_001, art-manifest.json and import-receipt.json.

## 22. Save version and migration

Preparation snapshots are version 3. Versions 1/2 validate then migrate in memory before catalog preflight; v1 resolves only to Danube Border, v2 retains its map. Nine counter slots retain exact/saturating totals with equivalent-role migration; boss indices/cadence are preserved. Committed event IDs, recent exclusions, cooldowns, history, counts and supply amounts map without reroll or repeat credit. Read-only loading does not rewrite the source; the next successful checkpoint writes v3 through the existing atomic store. Unknown versions/maps and incompatible structures still reject with backup preservation. History keeps generic totals and optional map identity without exposing retired factions.

## 23. UI and terminology audit

Boss HUD displays THE DREDGER / THE BROODMOTHER with MASSIVE BIOLOGICAL SIGNATURE warning. End-run detailed counters use all six creatures and both bosses. Wire tooltip, event descriptions, map text, current lore, README and pressure debug labels use aquatic language. UI Toolkit bindings, RSD, Start Wave, structure management and game-speed controls remain. Old identifiers survive only in explicit migration, the serialized adapter and historical/archived provenance.

## 24. Automated tests

Final EditMode 127/127 passed, including equivalent-role/event migration and half-cost escort rules. Full PlayMode 80/80 passed in 255.62 seconds, including both maps, 100 waves, all events, shared defenses/management, product flow and aquatic abilities. Initial full PlayMode 74/80 identified the longer hit feedback and superseded armor/capacity assumptions, all corrected. Focused aquatic tests also passed 5/5. The final pure encounter-cost reporting correction was followed by all 127 EditMode tests and a successful Player build; it does not change scheduling/composition. No zero-test or merely-started run is counted as validation.

## 25. Script-assisted playtests

Prepared inspection: Waves 1–5 complete on both maps with real movement/combat/arrivals and unlimited breach threshold; 15,000 RSD inspection grant disclosed. A separate earned-income walkthrough uses the real 500 RSD start, threshold 1000, scripted purchases/upgrades and .1-second ticks. Both maps clear 1–5: Danube 717 kills / 450 Homicides; port 662 kills / 700 Homicides. Final pre-reinvestment balances 11,472 / 11,447 RSD. This is not human mouse-only certification.

Both bosses complete on both maps under explicit test damage, with zero pending/alive and exactly one boss kill each. Broodmother produces six counted children in each encounter fixture. All four arrival methods occur in the encounter receipts. These are scheduling/damage/reward/completion tests, not earned-income boss victories.

Actual 1920x1080 normal-Update Mass Emergence captures show 47 enemies/10 carriers on Danube and 43/11 on port. Both biological boss HUDs were captured and visually inspected. Fogged paused-tick camera captures retain diagnostic value but are not the showcase images: paused VFX clocks accumulated tracers. Actual Game View captures use normal effect lifetimes. No isometric enemy or old delivery hull appeared.

## 26. Performance and stress

Three-second Editor main-thread samples after warmup; movement held at .05 multiplier to retain each exact requested population while normal Update/path/animation/VFX work continues. No defenses in this load profile. Results are not standalone shipping or sustained combat benchmarks.

| Broodlings | Samples | Median ms | p95 ms | Maximum ms | Mean GC bytes/frame |
|---|---:|---:|---:|---:|---:|
| 100 | 301 | 3.291 | 4.280 | 6.122 | 10,310 |
| 200 | 302 | 4.361 | 5.510 | 9.392 | 10,459 |
| 350 | 254 | 5.845 | 7.130 | 14.713 | 10,354 |

Population stayed exact throughout each sample; registry cleared and retained enemy pool stayed <=400 after removal. Automated 350-unit stress also verifies rewards and reuse. Existing 100-wave lifecycle test covers procedural bounds, cargo accounting, pools, completion and transient cleanup. No speculative spatial partition/package/LOD was introduced.

## 27. Legacy asset cleanup

Humanoid sprite packs moved with metadata to Art/Legacy/PreAquatic. Old delivery hull prefabs and unused Raider data/prefab are under Game/Legacy/Phase4C; former boat prop textures under Art/Legacy/Props. Active definitions/prefabs have aquatic names, with original GUIDs retained. Saved menu and both maps have zero missing scripts and zero legacy enemy/transport prefab dependencies. Old masters/source sheets, historical reports and earlier validation captures remain labeled provenance, not current gameplay content.

## 28. Known issues and validation limits

Long-session standalone performance, human mouse-only balance and earned-income midgame/boss victories remain unperformed. Early opening remains demanding, especially port Wave 1. Generated sprite art is a coherent first pass; frame polish can improve through focused later art review. Audio is deferred by request. No next-phase implementation is authorized by these results.

## 29. Recommended next phase

Phase 6C planning can follow successful terminal regression and owner review, with human balance/sustained performance review alongside it. Phase 6C has not been started.

## Final validation summary

STATUS: COMPLETE. EditMode 127/127 passed (see editmode-final.json); full PlayMode 80/80 passed, zero skipped/inconclusive (playmode-final.json). The runtime suite includes the final hit-pose, burn-persistence, ownership and teardown fixes. A subsequent pure encounter-budget reporting correction passed the full EditMode suite and Player compilation.

Actual Main Menu Continue migrated four committed-event cases: v1 Danube Border and v2 Industrial Port, each with Pirate Raid or Militant Assault. All restore as Swarm Surge / Armored Migration with the same run/map, 2,049 RSD, 450 total kills, Level III MG and 420 HP. Read-only migration leaves the old file bytes untouched; successful restored Preparation checkpoints write v3. Receipt: legacy-continue.json. Both-map eight-defense/upgrade/repair/dismantle/Continue regression remains passing in the full suite.

Windows release: Builds/Phase6B/BorderDefenders.exe, succeeded, 127,202,163 total bytes, zero errors. One intentional Pipeline warning confirms enableInBuilds=false and no developer endpoint in release. The final incremental build took 10.28 seconds. The first cold build completed in 37.95 seconds but the tool's default 30-second request timed out and logged an error; that attempt is retained separately and superseded by the clean build using an explicit longer timeout. Build.cs explicitly packages only Main Menu and the two production maps, retaining the separate regression scene in the project.

Compilation succeeds. Final Console is archived/checked after build and saved-scene validation. Historical buffers include corrected intermediate compile/test failures, expected broken-observer test logs and a canceled async retry submitted across Editor stop/domain reload; none are presented as final passes. No package/Time settings change. All fourteen moved active definition/prefab GUIDs are verified preserved against the exact before-change checkout. Player-facing text audit found zero obsolete display/description fields. Asset audit found zero missing scripts or legacy enemy/transport prefab dependencies in all three production scenes.

Visual review includes actual 1920x1080 Mass Emergence on both maps, both biological boss HUDs, all runtime hard-alpha exports and 100/200/350-unit stress captures. Earned early-wave and prepared encounter evidence is explicitly separated. No human mouse-only, earned-income boss victory, native standalone launch or sustained standalone performance claim is made. Ready for owner review and Phase 6C planning; Phase 6C was not started.

Final map-card review found one obsolete boat in the Danube preview. It was replaced by a native saved-scene capture with its existing texture GUID/import settings preserved; prior image retained as provenance. The port preview was inspected and retained. Final production-scene dependency audit includes all Legacy assets, not just prefabs: zero found. Temporary QA runInBackground setting was restored to the exact baseline before the final build.

Post-review presentation check: Broodmother release now plays all four poses at 8 FPS within its .5-second window; aquatic PlayMode fixture reran 5/5 successfully afterward (aquatic-post-review.json). Runtime logic and composition are unchanged. Final release build includes this presentation correction.
