# Phase 5B — random events, endless variety and assault modifiers

Validation date: 2026-10-03. Unity 6000.3.24f1. Primary scene: `Assets/Game/Scenes/CoastalDefense_TopDown.unity`.

## 1. Status

COMPLETE. All thirteen events are implemented, forced live fixtures pass, thirty natural event waves and the existing hundred-wave regression pass. Final suites: 113/113 EditMode and 65/65 PlayMode. Saved scene is clean with zero missing scripts and final Console has zero errors/warnings. No Phase 5C, packages, additional defenses/bosses, audio, campaign, multiplayer or progression were added. Existing uncommitted work is preserved; no commits/staging/reset.

## 2. Event system architecture

Thirteen `EndlessEventDefinition` ScriptableObjects and one `EndlessEventSettings` catalog live under `Assets/Game/Data/Events/`. Definitions carry identity, text, category/classification, minimum/maximum wave, weight, duration type/value, repeat cooldown, incompatibility IDs, boss exception, icon, preparation applicability and typed effect parameters. Weight supports rough rarity without a redundant player-facing rarity system.

`EndlessEventManager` is a lightweight run-owned policy object held by the existing WaveManager, not another scene scheduler or singleton. `ActiveEventContext` stores stage, selected sector, remaining waves/time, warning and actual credited supply. WaveManager retains generation, Start Wave, pending/alive ownership, transport dispatch and completion. EconomyManager remains the money owner; BuildManager retains all construction/upgrade/repair/refund transactions. UI and overlays observe state.

Five duration types are supported: NextWave and EntireWave activate at Start Wave and finish at completion; Timed counts scaled active-wave seconds and also clears at completion; Preparation clears at Start Wave; MultiWave suspends modifiers between waves and reactivates on the next start. Initial catalog uses EntireWave and Preparation. Already committed invasion cargo is not retroactively removed if a timed event expires during combat. Canceling an unstarted event restores its seeded baseline plan, removing hidden quantity/composition changes as well as live modifiers.

## 3. Event selection rules

Selection uses a separate per-wave System.Random stream derived from RunSeed. Composition keeps the existing generator's own seeded stream. Wave ranges, cooldowns, recent exclusions, weight and boss rules filter eligibility before category/event weighted selection. One major event at a time; symmetric incompatibility checks exist for future use without enabling stacking. Definitions remain unchanged during play.

## 4. Cooldown rules

All initial definitions have eight-wave repeat cooldown. Last two selected event IDs are excluded independently. Wave history retains the latest 128 entries; lifetime totals/per-event counts remain available through RunStatistics. No permanent analytics or history screen.

## 5. Event frequency

No events on waves 1–4. First eligible selection is wave 5. Subsequent selections use randomized gaps of 2–4 waves; from wave 30, 2–3 waves. Boss or empty eligible-pool suppression can extend gaps. Category targets are Challenge 70%, Relief 20%, Pattern 10%, normalized across categories that currently have eligible definitions. Values are editable in the catalog.

## 6. Event list

| Event | First wave | Classification | Lifetime |
|---|---:|---|---|
| Mass Landing | 7 | Challenge | Entire wave |
| Pirate Raid | 20 | Challenge | Entire wave |
| Militant Assault | 18 | Challenge | Entire wave |
| Heavy Assault | 18 | Challenge | Entire wave |
| Runner Swarm | 8 | Challenge | Entire wave |
| Fog | 5 | Challenge | Entire wave |
| Flooded Shore | 8 | Challenge | Preparation + entire wave |
| Ammunition Shortage | 8 | Challenge | Preparation + entire wave |
| Emergency Supply | 5 | Relief | Preparation |
| Field Engineers | 5 | Relief | Preparation |
| Calm Before the Storm | 10 | Challenge | Preparation warning + entire wave |
| Multi-Sector Attack | 8 | Pattern | Entire wave |
| Concentrated Landing | 8 | Pattern | Entire wave |

The generator still filters every enemy by its own unlock wave. Pirate Raid waits for Raider wave 20; militant/heavy events wait for Heavy wave 18. No new enemy behavior or faction propaganda art.

## 7. Mass Landing settings

Quantity ×1.5, director target ×1.5, boat concurrency ×1.5, arrival interval ×.7, distributed sectors. Fractional formation expansion uses cumulative rounding, preserving the exact total rounding of +50%. HP unchanged. Existing hard population and 32-boat limits still apply. Wave-24 seed-1977 fixture: normal 485 planned, Mass Landing 728, observed peak 15 simultaneous boats in a six-game-second sample, versus 13 in the ordinary fixture. These peaks are observations, not concurrency guarantees.

## 8. Pirate Raid settings

Composition weight factors: Pirate Runner ×5, Pirate Raider ×4, Runner ×3, Basic ×.4; other unlocked types retain baseline weights. Boat speed ×1.2, interval ×.75, distributed sectors. Existing PirateBossTransport reused for ordinary raid cargo; an actual boss retains its own transport priority. Wave-24 fixture: 430 planned, peak 13 boats. HP unchanged.

## 9. Militant Assault settings

Breacher ×5, ISIS Heavy ×5, Brute ×2, Runner ×.5 composition weights; Basic support remains. Existing enemy definitions, armor and fictionalized presentation reused. Wave-24 fixture: 350 planned. Heavy compositions spend the normal budget on more expensive types; count reduction is intentional and not an HP multiplier.

## 10. Heavy Assault settings

Brute ×7, ISIS Heavy ×10, Basic ×.5, Runner/Pirate Runner ×.4. Retains supporting light targets; not an all-Brute wave. Wave-24 fixture: 230 planned. Baseline HP/armor remain authoritative.

## 11. Runner Swarm settings

Runner/Pirate Runner ×6, Basic/Brute ×.3, distributed sectors. Existing unlock filtering applies to Pirate Runner. Wave-24 fixture: 455 planned. No added global movement/HP penalty.

## 12. Fog settings

Effective weapon range ×.8 during the wave. Mines and Wire retain physical blast/traversal areas. Existing level stats/assets are never changed; Level III Sniper restores its exact upgraded range on cleanup. Selected panel and targeting read the same effective range. Live Level I Sniper: 13.75 -> 11.0 world units -> 13.75 after cancellation. HUD explicitly says DEFENSE RANGE REDUCED BY 20%.

One static transparent quad covers the authoritative XY battlefield, Effects order 18, alpha .24; no particles or per-frame visual animation. Existing Square sprite is rendered Simple, avoiding importer changes and tight-mesh tiling warnings. Readability checked in the actual 1920x1080 capture.

## 13. Flooded Shore settings

First inland Ground column, immediately after existing Water/Beach bands, is unavailable for new placement during Preparation and the event wave. Footprints crossing that column also reject. Existing structures remain alive, owned and repairable/upgradable/dismantlable. This BuildManager-only placement mask changes no GridCell, occupancy, traversal, pathfinding cache or permanent terrain. Cleanup restores placement immediately. Existing shallow-water tile provides a dark teal wet strip on GroundObjects order 1, alpha .64.

## 14. Ammunition Shortage settings

Upgrade price ×1.2 in Preparation and combat; combat repair multiplier gets an additional ×1.2. Preparation repair unchanged. Effective prices use ceiling, with decimal-normalized upgrade factors preventing float noise from changing exact integer boundaries (225 -> 270, not 271). Live damaged Sniper: Preparation repair 45, combat repair 90 rather than baseline 75.

## 15. Emergency Supply settings

Preparation grant `min(500, 100 + wave*8)` RSD. Wave 20 gives 260; live wave 24 credits 292. EconomyManager executes the credit once when queued, actual credited amount appears in the event text, and existing paper-bundle imagery is reused. No coins. Canceling a delivered supply does not revoke money already received. Queue handles an observer ending the run during the credit.

## 16. Field Engineers settings

Additional Preparation repair factor ×.7; normal repair basis and phase factor remain. Live damaged Sniper: 45 -> 32 RSD, with a positive minimum cost. Modifier ends at Start Wave; normal combat repair returns to 75. Refund is unchanged.

## 17. Calm Before Storm settings

Eight scaled gameplay seconds of Preparation warning before Start Wave becomes available. Existing Preparation is already player-controlled and can remain open indefinitely, so no automatic-start system was added. Warning freezes at Pause and advances at x2. Then quantity/pressure ×1.3 and boat slots ×1.25 for the assault. Live wave 24: 630 planned. No audio requirement.

## 18. Landing-pattern event settings

Logical South/Center/North sectors use berth Y position relative to grid thirds. Multi-Sector strongly favors alternating separated sectors, interval ×.7 and slots ×1.25; actual runtime test confirms arrivals in all three. Concentrated Landing chooses and announces one seeded sector during Preparation. Four of five dispatches wait for an available lane in that sector; the fifth permits support elsewhere. This prevents free-lane fallback from silently defeating the advertised focus. Route, hull clearance, capacity and active-population backpressure remain intact.

## 19. Pressure Director integration

Optional per-call event modifiers multiply target/concurrency/interval around the original settings. Static and accelerated director intervals both receive the event factor. Pressure target respects the existing 400 hard enemy cap; boat limits stay <=32. ConfigureMotion reads permanent settings; event speed is an independent transport factor. No base configuration overwrite/restore gamble. Completion, cancellation, Game Over, disable/restart and scene unload remove live modifiers; next generation uses only its own selected context.

Baseline settings remain formation quantity 5, ordinary HP 1, boss HP 1, maximum planned standard count 1200 and maximum active enemies 400. Previous historical 10x/3x tuning was not reinstated. Quantity events increase only temporary planned formations.

## 20. Upgrade/repair integration

BuildManager supplies the same effective prices to UI availability and payment. StructureHud refreshes immediately on event changes and phase transitions; it ignores passenger-only wave notifications. Actual transaction tests check charged money, upgraded stats, repair, restored range and stable refund fraction. Paid event-priced upgrades remain part of actual investment under the existing refund formula; no event directly multiplies dismantle refunds. Repair spend still does not increase investment.

## 21. UI / art changes

Existing UIDocument/UXML/USS and controls retained. Small event row in the existing wave HUD shows icon, event name, description, received RSD and selected sector. It replaces the ordinary status line while present; northern terrain remains visible. Start button says EVENT WAVE during a queued event. Existing military pixel font/panels, RSD paper-money language, defense icons/tools and wave chevrons reused. Exactly two runtime overlay renderers; no new raster generation, asset replacements or gameplay prefab edits. Reuse record: `Docs/Art/Batches/p5b_events_001.md` and asset register.

Development-only override: during an eligible Preparation, invoke `WaveManager.ForceEvent("fog")` or another catalog ID through the Unity development command/fixture. Method is excluded from normal production builds and no debug dropdown is exposed to players. Wave minimum/boss eligibility still applies. `Docs/Validation/Phase5B/Live.cs` demonstrates all thirteen overrides at non-boss wave 24.

## 22. Automated test results

Final EditMode: 113/113, zero failed/skipped, .99 seconds (`editmode-final.json`). Includes fourteen new rule cases for eligibility, seed reproducibility, cooldown/recent exclusion, weighting, compatibility, all five lifetimes, quantity/director restoration and four composition biases.

Final full PlayMode: 65/65, zero failed/skipped, 167.56 seconds (`playmode-envelope.json`). Six new runtime tests exercise all thirteen events through real boat dispatch/completion, immediate UI/payment, upgraded fog restoration, canceled Preparation quantity restoration, flood placement/navigation invariants, actual sector arrivals, Pause/x2, Game Over and thirty natural event waves. Existing 100-wave test now honors Calm warning and the event-aware bounded concurrency maximum; counts/rewards/bosses/pools/transient checks remain. Initial failures and fixes are retained in `focused-initial.json`, `playmode-initial.json`, `playmode-second.json`.

## 23. Forced event playtest results

All thirteen scripted live Play Mode fixtures passed. Both Preparation and active PNGs retained; `live-batch-0/4/8.json` contains actual values, boats, sector totals and cleanup checks. Real UI Toolkit submit events start waves and select Pause/x2; normal Unity Update drives enemies, defenses, landings and effects. Six game seconds sampled per assault, warning staged separately. Every forced event is also drained to completion in automated tests.

Fixtures explicitly inject 50,000 RSD and use unlimited breach threshold to inspect behavior at wave 24 with a prepared mixed Level III defense line. They do not establish an earned-income balance run or human mouse-only usability. Actual screenshot resolution 1920x1080; contact sheet and individual fog/flood/price/landing captures visually inspected. Capture-driven layout revision moved event content into the existing top HUD. Overlay, cost and range cleanup checks pass.

## 24. Natural 20–30 wave event test

Thirty waves with procedural selection, seed 1977, real boat packing/unloading and wave completion passed in the focused suite. Accelerated fixture uses explicit ticks and lethal test damage to drain attackers; it verifies scheduling/lifetime rather than normal-money difficulty. Boss wave 25 has no ordinary event, waves 1–4 have none, at least five distinct event IDs appear, cooldowns are respected and no old modifiers remain at completion. The natural run completed eight event waves with six distinct IDs: Fog 5, Mass Landing 7, Ammunition Shortage 10, Fog 14, Emergency Supply 17, Concentrated Landing 20, Multi-Sector 24 and Ammunition Shortage 27. On reaching Preparation 31 it previewed Heavy Assault. A 150-wave policy test separately checks repeat spacing and weighted variety over many seeds. Natural runtime history is logged in Console/test evidence; `seed-1977-selection-history.json` supplies the pure-policy reproduction.

## 25. Performance observations

Three-second, x1, 1920x1080 Editor main-thread ProfilerRecorder samples, after one-second warmup, with a prepared mixed line and real wave-24 combat:

| Sample | Frames | Median ms | p95 ms | Max ms | Mean GC bytes/frame |
|---|---:|---:|---:|---:|---:|
| Normal | 1181 | 2.477 | 3.121 | 4.033 | 10307 |
| Fog | 1170 | 2.472 | 3.351 | 4.476 | 10344 |
| Mass Landing | 1169 | 2.527 | 3.209 | 9.865 | 10302 |

Final populations differ (76/80/70 enemies, 12/11/13 boats), so these are bounded samples, not a precise isolated event overhead benchmark. Fog adds no particle stream; two overlay renderers are created once. Selection allocates bounded per-wave buffers, gameplay modifiers are read from current context, and presentation/stat UI uses notifications. Existing cargo/UI/VFX allocations remain. No standalone or sustained memory/performance certification is claimed.

## 26. Known issues

No known Phase 5B gameplay/compile regression after the final full suites, live validation and saved integration audit. Human mouse-only balance, earned-income survival under events, standalone profiling and sustained long-session GC remain unverified. Reused generic wave icons are functional initial presentation rather than a unique illustrated set for every event. Static fog deliberately prioritizes readability/performance. Earlier boss-art and transport visual-overlap limitations remain outside this phase.

## 27. Recommended next phase

Ready for Phase 5C planning. Review human event balance and usability alongside that planning. No Phase 5C work started automatically. Reports/evidence live under `Docs/Validation/Phase5B/`; authoring uses Unity APIs, and focused diffs compare the preserved before-change runtime/UI snapshots.

