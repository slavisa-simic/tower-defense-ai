# Phase 6E — final balance and gameplay polish

Validation dates: 2026-11-02–03 (local). Unity 6000.3.24f1, Windows Editor, strict overhead XY. Audio remains deferred. The existing Phase 6D uncommitted work was preserved; no staging, commit, new map, major subsystem, multiplayer or Phase 6F implementation.

## 1. STATUS

**PARTIAL.** The evidence below separates actual Update-driven play, explicit-clock combat simulations, controlled load fixtures, lifecycle tests and synthetic RP inputs. A test pass does not establish optimal human play. The native mouse walkthrough was stopped when concurrent user desktop input was detected; no further desktop input was sent. UI Toolkit actions and captured Game Views supplied the subsequent product walkthrough.

## 2. BASELINE BALANCE SNAPSHOT

`Docs/Balance/PHASE_6E_BASELINE.md` was captured before balance edits from 71 loaded ScriptableObjects. `baseline-assets.json` preserves resolved values, including defaults absent from YAML. `baseline-serialized-assets.json` preserves original asset YAML/GUID references from the initially clean data revision. `baseline-source.json` preserves the actual already-modified Phase 6D runtime baseline.

The baseline already had fivefold formations, global ordinary/boss HP factors 1, 500 starting RSD and 400 hard active cap. Seed 1977 planned 80/115/240/395/540/690/1061/985/1200/1441 organisms at Waves 1/3/5/10/15/20/25/30/40/50, including additive encounters. No historical 10x/3x tuning was restored.

## 3. DEFENSE BALANCE CHANGES

|Defense|Before|Change|Reason/result|
|---|---|---|---|
|Wire|50%/60%/70% slow; upgrades 38/63 RSD|III slow 65%; upgrades 30/50|Strong support with less III lockdown; walkable, no damage, no overlapping-cell slow stacking|
|Mine|150/188/240 damage; upgrades 57/94|150/210/300 damage; upgrades 20/35|A worthwhile paid upgrade for one charge; disposable burst, no profitable ordinary kill cycling|
|MG|20 base damage; III 32 at 4.8/s|Base unchanged; III 35 at 5/s; upgrades 75/150|Dependable light fire; armor still applies fully|
|Sniper|100 damage, progressed-target selection|180 damage, 100% armor penetration; stable max-HP priority|Focuses high-value organisms and keeps wounded heavy priority; slow reload still wastes potential against swarms|
|Mortar|II no damage gain, III reload unchanged|II 150 at 0.4167/s; III 210 at 0.5/s|Better upgrades; range 12, minimum 3, travel .8s and clustered targeting preserved|
|Flame|225/375 upgrade costs|150/225; damage, cone, range and burn role retained|Upgrade competes with a new emplacement; vulnerable to armored/ranged pressure|
|Auto Cannon|55 damage at 2/s|100 at 2/s; ignores 75% of armor|Actual sustained anti-heavy role; no splash; base cost 600 and 2x2 footprint retained|
|Rocket|250 base damage; weak II/III efficiency|350 base; II 438, III 612; upgrades 450/675|Heavy area burst; reload/travel and committed impact point remain meaningful|

Burn damage now applies armor before fractional accumulation: the reproduced 15-DPS/two-second/50%-armor case dealt 15 versus 30 damage with different frame sizes before the fix, and 15 in both afterward. Flame preserves cadence remainder so +5% research remains effective under fixed ticks. Armor penetration is a defense-authored direct-hit parameter in the existing damage path. Zero penetration preserves prior mitigation; it never increases damage against unarmored organisms. AOE and burns retain ordinary armor processing. No new combat manager or damage-type framework.

## 4. ENEMY BALANCE CHANGES

All six base HP/speed/unlock/structure-damage values remain. Bulwark armor rises from 25% to 50%, making heavy counters relevant without adding HP. Spitter range rises from 5 to 6 cells and it holds a valid firing position during reload rather than walking into close fire. Losing/removing the target resumes routing. Its 24 damage, five-second reload, initial 1.5s reload and .65s committed glob remain.

Crawler remains mass; Skitter remains fast/light; Broodling remains cheap dense threat; Burrower retains one visible five-cell route-following bypass after one surface second and at least eight final surface cells. Creature identity, GUIDs, presentation ownership and navigation rules are unchanged.

## 5. BOSS BALANCE CHANGES

Dredger HP/armor/speed/120-per-1.2s siege attack/escorts and 2500 RSD reward remain. Broodmother keeps HP/armor/speed/escorts/2200 RSD reward; first brood moves from eight to three seconds so a short specialized fight includes her identity. Later releases remain six per eight seconds, lifetime 60/concurrent 18; no exponential spawning. Removal and wave accounting remain in existing owners.

## 6. STRUCTURE-DAMAGE BALANCE

No blanket attack multiplier. Crawler 12/s; Skitter 6/s; Bulwark 45/s; Spitter 24/5s; Broodling 3/s; Burrower 18/s; Dredger 120/1.2s; Broodmother 45/s. A single uninterrupted Bulwark needs seven hits against a 300-HP MG and ten against 450 HP. Dredger needs three/four hits (3.6/4.8s). One Spitter needs 19 hits against Level III MG: about 92.15s from acquisition including first reload/flight. Groups combine pressure and can destroy unsupported positions. Research adds bounded HP, not invulnerability.

## 7. ENDLESS CURVE CHANGES

Quantity remains fivefold. HP/speed growth stays at the original .0125/.002 after Wave 20, without a global HP boost. After Wave 20 composition gradually shifts toward Bulwark/Spitter/Burrower support, saturating at 60 while retaining Crawlers and swarm types. Full seeded projections through int.MaxValue are in the final balance table. Fixed-budget Wave 40 failures are retained; they are not earned-run failures.

## 8. PRESSURE DIRECTOR CHANGES

Early target 25 → 8; growth remains four per wave. Late ceiling 160 → 240 of hard cap 400. Cargo packing now uses current active-plus-incoming admission room, even between .2s policy evaluations; a large carrier cannot overshoot a small early target by its whole cargo. Existing 20% hysteresis, accelerated/normal/throttled modes and seven-second/24-second surges provide streaming and recovery. No second scheduler.

Before: three approach MGs lost Wave 1 on both maps after 34–36 simulated seconds with unused income. After admission correction: first wave survives without mid-combat purchasing. Natural Update evidence and the full product loop are recorded separately below.

## 9. CARRIER BALANCE

Direct/Pod/Brood/Leviathan capacities remain 6/12/24/32 ordinary units, with biological compatibility, half-unit larvae, validated water approaches, release/rupture/remains timing and sector scheduling. Early loads may be partial because admission is authoritative. Tests assert exact passenger accounting and nonempty bounded dispatches rather than falsely requiring every pod to be full. No geometry, arrival art or transport ownership replacement.

## 10. ECONOMY CHANGES

Starting funds remain 500 (full tree 750). Kill RSD becomes Crawler 1, Skitter 3, Bulwark 8, Spitter 4, Broodling 1, Burrower 4; boss rewards stay 2500/2200. Wave grant becomes 300 + 2×(wave−1), replacing 100 + 5×age. This moves early purchasing power into a predictable survival payment and limits mass-kill farming.

Measured iteration ledger: an early pacing-only ten-wave trial banked approximately 37,500 RSD; a 4-RSD Crawler trial still banked approximately 60,000 by Wave 30; a 2-RSD/200-grant trial reduced that further. A too-harsh 2-RSD/100-grant opening failed at Waves 4–5 and was rejected. The final lower kill payouts/larger grant preserve a viable light-first opening. These are actual individual asset values, not a hidden runtime economy multiplier.

Late reserves still deserve human adversarial review; the scripts intentionally stop expanding at specified waves and can bank money instead of spending it on every available site. This is disclosed rather than claiming all economic decisions are proven optimal.

## 11. REPAIR / UPGRADE / DISMANTLE CHANGES

Repair formulas remain ceil(paid investment × .5 × missing/max HP × .75 Preparation or 1.25 combat × research × event), minimum one. A neutral III MG now costs 375 total; half-health repair is 71 Preparation/118 combat. Full tree has 353 paid investment, 540 HP and half-health repair 60/100. Field Engineers and Ammunition Shortage retain their bounded phase effects. Upgrade preserves missing HP; repair is excluded from refundable investment.

Refund remains floor(actual paid construction/upgrades × .50), full tree .55, hard safety ceiling .75. No sale can return its purchase cost; repeated/destroyed/consumed/foreign sales reject. Final efficiency tables include all levels, nominal DPS, HP, coverage and area-output ratios. Coverage, minimum range, overkill, armor and multiple positions explain why these are guidance rather than rankings.

## 12. TECH TREE BALANCE

No compensating enemy HP or research branch multiplier. Twenty nodes/610 RP and frozen per-run snapshots remain. Full research gives +250 starting RSD, +20% relevant HP, +5% MG/Cannon damage, +5% weapon cadence, +8% Sniper range, +10% splash/burn duration, .9 upgrade/repair factors, .55 refund, +10% wave grant and information. These effects remain bounded and data displays use effective queries. The Hardened Wire HP bonus has little ordinary combat value: walkable Wire does not obstruct routes and Spitters deliberately exclude it from acquisition. Manual damage and the narrow committed splash can damage it, but those are not evidence of a useful routine durability purchase. This remains an unresolved research-value issue; no unrequested wear subsystem was added. Continuing an old run retains its research snapshot and exact saved money.

## 13. RP ECONOMY

Actual reward/milestone/purchase/atomic-profile services simulated repeated weak/average/boss/good/late failed runs. Input performance is synthetic, at 1000 Homicides, not claimed player skill. Zero means not reached in 40 runs. Breadth-first purchases make first tier III slower than focused branch progression. Wave 5 still awards enough for the first useful 10-RP node in one run.

|Input|Wave|Bosses|First node run|First tier III|First capstone|Full tree|
|---|---:|---:|---:|---:|---:|---:|
|weak|3|0|0|0|0|0|
|average|15|0|1|35|0|0|
|boss|25|1|1|10|18|26|
|good|40|1|1|5|10|14|
|late|60|2|1|3|6|9|

Good Wave-40 runs complete the tree in 14; strong Wave-60 runs in nine. Repeated first-boss-only runs are slower. No new farming exploit or kill-to-RP conversion was introduced.

## 14. EVENT BALANCE

One major event, existing eligibility/cooldowns and all thirteen effects remain. Fog ×.8 range; Mass ×1.5 quantity/pressure/concurrency; Calm ×1.3 quantity/pressure and ×1.25 concurrency; shortage ×1.2 upgrade/combat-repair; engineers ×.7 Preparation repair; supply min(500,100+8×wave). Flood limits only new construction. Full exact modifiers/composition/carrier biases are in the final asset table.

All thirteen fought on both maps with representative none/partial/full profiles and disclosed 9000-RSD prepared budgets. Deaths are results, not omitted cases. Not every event at every progression state was tested.

|Event|Map|Profile|Result|Homicides in fixture|
|---|---|---|---|---:|
|ammunition-shortage|CoastalDefense_TopDown|none|WaveComplete|480|
|ammunition-shortage|DanubeIndustrialPort|none|WaveComplete|0|
|armored-migration|CoastalDefense_TopDown|partial|WaveComplete|125|
|armored-migration|DanubeIndustrialPort|partial|WaveComplete|0|
|brood-tide|CoastalDefense_TopDown|none|WaveComplete|0|
|brood-tide|DanubeIndustrialPort|none|WaveComplete|0|
|calm-before-storm|CoastalDefense_TopDown|none|WaveComplete|860|
|calm-before-storm|DanubeIndustrialPort|none|WaveComplete|0|
|concentrated-emergence|CoastalDefense_TopDown|none|WaveComplete|0|
|concentrated-emergence|DanubeIndustrialPort|none|WaveComplete|0|
|deep-water-disturbance|CoastalDefense_TopDown|none|WaveComplete|550|
|deep-water-disturbance|DanubeIndustrialPort|none|WaveComplete|25|
|emergency-supply|CoastalDefense_TopDown|none|WaveComplete|305|
|emergency-supply|DanubeIndustrialPort|none|WaveComplete|0|
|field-engineers|CoastalDefense_TopDown|none|WaveComplete|615|
|field-engineers|DanubeIndustrialPort|none|WaveComplete|0|
|flooded-shore|CoastalDefense_TopDown|partial|WaveComplete|55|
|flooded-shore|DanubeIndustrialPort|partial|WaveComplete|0|
|fog|CoastalDefense_TopDown|full|WaveComplete|375|
|fog|DanubeIndustrialPort|full|WaveComplete|0|
|mass-emergence|CoastalDefense_TopDown|full|WaveComplete|640|
|mass-emergence|DanubeIndustrialPort|full|WaveComplete|0|
|multi-sector|CoastalDefense_TopDown|none|WaveComplete|20|
|multi-sector|DanubeIndustrialPort|none|WaveComplete|0|
|swarm-surge|CoastalDefense_TopDown|none|WaveComplete|0|
|swarm-surge|DanubeIndustrialPort|none|WaveComplete|0|

## 15. DANUBE BORDER BALANCE

Open coverage and layered positions matter. No terrain or hidden map HP modifier. Three broad approach regions and existing exits remain. Fixed 9000-RSD Wave-40 layouts expose coverage failures; earned progression can invest more than that. Final neutral/partial/full results below use actual starting funds, ordinary threshold and committed owner commands.

## 16. INDUSTRIAL PORT BALANCE

Containers/cranes/buildings and static route families remain. Area/mixed investments exploit chokepoints; a wall at the tested x=10 cannot seal unbuildable dock routes and is not a universal solution. Map validation and collision tests cover every landing-to-exit path. No creature HP differences between maps. Port remains easier for these particular AOE/mixed placements; broader human comparison is still valuable.

## 17. BUILD-ARCHETYPE RESULTS

Prepared 9000-RSD ceilings, comparable purchased investment (actual paid values listed), zero research, seed 1977. Extra investment stays within its archetype. Wall preserves funds for real combat repairs below half HP. .1s production ticks include all weapons, burns, projectiles, carriers, movement and attacks, yielding for navigation budget/pools. The fixture explicitly clears already-resolved cosmetic globs because its scaled clock is paused. These are deterministic comparisons, not native timing or proof against every possible layout.

|Wave|Map|Build|Paid RSD|Result|Kills|Homicides|
|---|---|---|---:|---|---:|---:|
|20|CoastalDefense_TopDown|mg|9000|WaveComplete|690|0|
|20|CoastalDefense_TopDown|wire-flame|8865|GameOver|670|1060|
|20|CoastalDefense_TopDown|precision|8550|WaveComplete|676|495|
|20|CoastalDefense_TopDown|area|8850|GameOver|615|1030|
|20|CoastalDefense_TopDown|mixed|8940|WaveComplete|690|0|
|20|CoastalDefense_TopDown|wall|7140|WaveComplete|690|0|
|20|DanubeIndustrialPort|mg|9000|WaveComplete|690|0|
|20|DanubeIndustrialPort|wire-flame|8865|GameOver|669|1000|
|20|DanubeIndustrialPort|precision|8550|WaveComplete|668|325|
|20|DanubeIndustrialPort|area|8850|WaveComplete|690|0|
|20|DanubeIndustrialPort|mixed|8940|WaveComplete|690|0|
|20|DanubeIndustrialPort|wall|3765|GameOver|289|1005|
|40|CoastalDefense_TopDown|mg|9000|GameOver|995|1000|
|40|CoastalDefense_TopDown|wire-flame|8865|GameOver|538|1000|
|40|CoastalDefense_TopDown|precision|8550|GameOver|75|1010|
|40|CoastalDefense_TopDown|area|8850|GameOver|481|1060|
|40|CoastalDefense_TopDown|mixed|8940|GameOver|1015|1040|
|40|CoastalDefense_TopDown|wall|7140|GameOver|312|1015|
|40|DanubeIndustrialPort|mg|9000|GameOver|1140|1000|
|40|DanubeIndustrialPort|wire-flame|8865|GameOver|890|1025|
|40|DanubeIndustrialPort|precision|8550|GameOver|106|1025|
|40|DanubeIndustrialPort|area|8850|WaveComplete|1200|0|
|40|DanubeIndustrialPort|mixed|8940|WaveComplete|1200|0|
|40|DanubeIndustrialPort|wall|3765|GameOver|132|1010|

No sampled build wins universally across these maps/windows. Heavy-only forces lose to volume; MG/Wire-Flame have heavy/ranged pressure weaknesses; AOE/mixed favor Port. A single chosen layout failing is not proof that its archetype is unviable. No claim of mathematical global optimality.

## 18. ZERO-RESEARCH RESULT

The same earned light-first plan clears Wave 30 then loses at Wave 31 on Border (1065 cumulative Homicides, 17326 RSD left). Port clears Wave 50 with 100 cumulative Homicides and 77875 RSD. The unused Border reserve is a limitation of this fixed between-wave plan, while the persistent Port outcome is a clear balance concern to investigate.

## 19. PARTIAL-RESEARCH RESULT

Both maps clear Wave 25: Border 225 cumulative Homicides, 10863 RSD; Port 100, 10228 RSD. The six-node profile establishes basic viability without full research.

## 20. FULL-RESEARCH RESULT

Both maps clear Wave 50: Border 175 cumulative Homicides, 76574 RSD; Port 75, 81564 RSD. Full research has a stronger long-term outcome than its small nominal multipliers suggest because the early cash and surviving coverage compound. These results do not meet a strong claim that researched late waves remain challenging.

All cases use earned RSD and purchases only between waves. The light-first strategy establishes coverage before upgrading, then adds Flame, Sniper, Mortar and Cannon. Partial profile uses six legitimately ordered nodes from a disclosed development RP profile setup; run RSD is not injected. Full profile uses the actual complete modifier snapshot. Homicides below are cumulative across each run.

|Profile evidence|Map|Last wave|Result|Cumulative Homicides|Ending RSD|Living structures|
|---|---|---:|---|---:|---:|---:|
|final-neutral-50|CoastalDefense_TopDown|31|GameOver|1065|17326|19|
|final-neutral-50|DanubeIndustrialPort|50|WaveComplete|100|77875|45|
|final-partial-25|CoastalDefense_TopDown|25|WaveComplete|225|10863|26|
|final-partial-25|DanubeIndustrialPort|25|WaveComplete|100|10228|33|
|final-full-50|CoastalDefense_TopDown|50|WaveComplete|175|76574|28|
|final-full-50|DanubeIndustrialPort|50|WaveComplete|75|81564|47|

## 21. BOSS TTK RESULTS

Isolated moving bosses, normal Update x1, zero research, authored Wave-25/50 HP scaling, no test damage. Paid midgame 4725 RSD versus specialized 11700. These measurements exclude ordinary/escort damage competition.

|Map|Boss|Force|Paid|HP|TTK seconds|Larvae released|
|---|---|---|---:|---:|---:|---:|
|CoastalDefense_TopDown|Dredger|midgame|4725|8500|11.1834698840976||
|CoastalDefense_TopDown|Dredger|specialized|11700|8500|5.73068141937256||
|CoastalDefense_TopDown|Broodmother|midgame|4725|8250|9.90668296813965|6|
|CoastalDefense_TopDown|Broodmother|specialized|11700|8250|4.77558898925781|6|
|DanubeIndustrialPort|Dredger|midgame|4725|8500|11.4070053100586||
|DanubeIndustrialPort|Dredger|specialized|11700|8500|5.65211868286133||
|DanubeIndustrialPort|Broodmother|midgame|4725|8250|9.88901519775391|6|
|DanubeIndustrialPort|Broodmother|specialized|11700|8250|4.71938705444336|6|

Full natural Update x2 encounters also run normal/escort pressure and the ordinary 1000-Homicides threshold with a disclosed 9000-RSD prepared budget. An unresolved boss has a blank TTK; defeated bosses do not imply the supporting wave was survived.

|Map|Wave|Paid|Result|Boss TTK seconds|Homicides|Peak enemies|
|---|---:|---:|---|---:|---:|---:|
|CoastalDefense_TopDown|25|8775|GameOver|13.4565353393555|1000|112|
|CoastalDefense_TopDown|50|8775|GameOver|11.6042785644531|1025|202|
|DanubeIndustrialPort|25|8775|WaveComplete|14.1362457275391|510|83|
|DanubeIndustrialPort|50|8775|GameOver|11.9319152832031|1025|119|

## 22. UX / READABILITY POLISH

Build tooltips now give role, strength and weakness, effective HP/damage/range/cadence and penetration. Flame direct/burn DPS is explicit. Selection shows penetration beside effective range. Port placement wording uses ground rather than grass. Existing affordability/selection colors, green/red ghost, UI interception, input priorities, Pause/x1/x2, committed rocket impacts and functional UI Toolkit text remain. Dismantle feedback expiry refreshes only when its text changes, removing repeated idle HUD reconstruction. No final art/new UI system.

## 23. TUTORIAL REGRESSION

Actual script-assisted UI onboarding, normal-Update first wave, earned Waves 2–10, intentional end, RP award, research purchase and new run are recorded in `fresh-product-loop.json`. The normal desktop mouse walkthrough was deferred after concurrent user input; it is not counted as completed. Tutorial policy/context/independent persistence remain covered by automated tests and the resolution captures. Tactical copy remains accurate for the new roles.

## 24. SHADOW / COLLISION VISUAL REVIEW

Captured combat/stress/encounter views preserve strict overhead silhouettes and existing short lower-right casts, one shadow per owner. No shadow rebuilding, art replacement or geometry change. Container/crane/corridor routing and defense contact attacks remain covered by both-map collision tests; existing silhouette overhang is not a ground-occupancy error. No expensive crowd physics. The 400-unit fixture is visually dense; lower VFX settings retain core attack/impact feedback.

## 25. PERFORMANCE RESULTS

Windows Editor 6000.3.24f1, Ryzen 5 5500 (6 cores/12 threads), 1920×1080, High VFX, natural Update x1. One-second warmup and five-second unique-frame sample per population. A disclosed stress grant/high HP/.15 movement modifier retains load while real weapons, pathfinding, Spitters and carriers run. Other desktop applications were active; this is not an isolated standalone benchmark or an FPS guarantee. `cpu-samples.json` separately measures warmed registry/cluster/route/movement/attack/policy costs and allocations; it is not a whole-frame profiler attribution.

## 26. 100 / 200 / 300 / 400 ENEMY RESULTS

|Initial|Actual range|Frames|Median ms|p95 ms|Max ms|Mean GC B/frame|Gen0 collections|
|---|---|---:|---:|---:|---:|---:|---:|
|100|100-137|964|4.6796|5.6055|11.1115|11547.125388601|0|
|200|200-236|831|5.8731|7.018|12.5687|12038.3978365385|0|
|300|300-300|706|6.897|8.2371|21.9956|12798.5473833098|0|
|400|400-400|609|8.0186|9.9019|19.9811|15567.8852459016|0|

Registry/pool/projectile cleanup is asserted after each case. The 400 cap naturally prevents further carrier unloading in that sample; lower samples include live carriers. Short samples cannot certify long-session memory stability. Existing path caching, eight-repath/frame budget, acquisition intervals and bounded VFX/enemy/carrier pools were retained; no unprofiled spatial/physics/package rewrite.

## 27. LONG-RUN RESULT

Final earned explicit-clock runs target Wave 50 on both maps for neutral/full and Wave 25 for partial profiles; final reached waves and failures are in the profile table, with ordinary combat damage and funds. The full automated suite's 100-wave transport/breach lifecycle simulation additionally checks pending/alive cleanup, pools, transient wave definitions and progression through Preparation 101. It uses an unlimited threshold and accelerated removal and is not a Wave-100 victory. For the full-tree Wave-50 attempts, allocated Editor memory grows from 504.7 to 513.2 MiB on Border and 506.4 to 510.8 MiB on Port while bounded enemy pools fill (355/194 pooled at the end). Managed memory fluctuates with GC rather than rising every wave. Every successful final scripted wave asserts zero registered/alive/pending organisms. These samples do not establish a multi-hour standalone memory/FPS certification.

## 28. SAVE / LOAD REGRESSION

The full persistence/map/research suites cover all eight structures, paid upgrades, HP/repair/dismantle, event state, exact money/occupancy, Continue, corruption/backup, research snapshots and once-only Game Over/history/RP. The actual product walkthrough awards 18 RP, buys Emergency Fund for 10 RP and starts at 600 RSD. The corrected development CSV contains ten separate completed-wave rows (1–10); earlier merged rows are retained with a before-transition-fix filename. Play Mode exit/re-entry reloads disk services, preserves rank/RP and continues the same 600-RSD run without repeating the grant (`product-restart-persistence.json`). This is a runtime restart, not a separately observed standalone-process quit/relaunch. Existing real user profiles were not changed.

## 29. RESOLUTION TESTS

`Walkthrough.cs` repeats actual 1366×768 / 1600×900 / 1920×1080 checks for intro, selected damaged structure/repair, boss warning, RP summary, Research and Port. Assertions check viewport containment, measured text height and critical HUD separation; captures are native resolutions, not resized images. Additional effective-stat tooltip/selected-Sniper captures are recorded where available. 2560×1440 remains optional and is not claimed for this phase.

## 30. PLACEHOLDER / DEBUG CLEANUP

No new placeholder sprites, asset GUID replacement, default button system or obsolete active creature name. Build text removes anti-infantry wording. Historical asset/file identifiers remain for serialization/migration. Production forced-event/research/debug controls remain excluded or explicitly gated. New local CSV logging is opt-in through `EndlessSurvival.writeBalanceLog` and compiled only in Editor/development; no production UI or telemetry. It observes transactions/registries and is disposed with RunSession. Deferred wave-completion flushing captures the original wave number and commits before an immediate Preparation transition, so accelerated scripts cannot merge or mislabel rows. The regression test checks completion payment and wave identity after advancing to the next Preparation before the next observer Tick. It records wave, spawns/kills/breaches/Homicides, earned/spent/repair/upgrade RSD, destruction, boss TTK and peak population.

## 31. AUTOMATED TEST RESULTS

Final EditMode: 168/168 passed, 0 failed, 0 skipped. Final full PlayMode: 110/110 passed, 0 failed, 0 skipped. Initial corrected attempts remain in the evidence directory, including a zero-test combined filter (not a pass), 167/168 first EditMode and 102/107 first full PlayMode. Those failures exposed superseded stat/price/cargo assertions and precision target abandonment; stable max-HP priority fixes the latter. Later 109/110 runs caught the intentional Spitter reload-hold expectation and a legacy world-freeze test that allowed menu navigation during its wait. The latter fixture now disables its unrelated menu/EventSystem input and asserts that its scene remains loaded. Focused checks cover both corrections. Assertions now follow authored values without removing once-only damage/reward/range/cooldown/ownership checks. Final terminal results, not started/empty runs, are the acceptance evidence.

Release build: Succeeded, 0 errors / 1 warnings, 17.6701876s, 158924095 bytes. Output: `Builds/Phase6E/BorderDefenders.exe`. Console history retains expected observer-test exceptions and an interrupted hot-reload QA session; a stopped/restarted session and final Console ground truth distinguish current state. The final clean Console ground truth is zero errors, one warning and compilationFailed=false. The warning is the intentionally excluded Pipeline runtime server in a non-development release build. No implementation compile failure is accepted. Original navigation report files that tests append are copied to Phase 6E evidence and restored to their initial contents.

## 32. MANUAL PLAYTEST RESULTS

Script-assisted live UI actions and actual captured Game View observation; no human comprehension or optimal mouse-only-play claim. Normal Update first wave, isolated boss TTK, full concurrent boss waves, mixed high-density combat and VFX density changes use real Updates. Other progression/archetype/event cases use disclosed explicit clocks. Failure cases and prepared budgets are preserved. UI controls were exercised through normal bindings. Desktop mouse input was stopped to respect concurrent user activity. The final observation harness disables external keyboard/mouse/EventSystem polling and binds the existing product view once after runtime readiness; combat retains natural Updates. Capture window lookup explicitly uses focus=false after an implicit-focus lookup was found during QA. These are external validation changes, not new production input owners.

## 33. KNOWN ISSUES BY SEVERITY

BLOCKER: none found in compilation, the completed test suites, release build or final clean Console sample. This does not remove the balance issues below.

MAJOR: Hardened Wire provides HP to a defense that ordinary targeting usually ignores, leaving its practical node value weak. Late earned-run reserves and the open-map/Port outcome gap remain substantial in the sampled strategy; the measured reserves and failures are listed explicitly. A prepared budget/lane fixture cannot prove every build is balanced. Native standalone-process persistence and sustained Player profiling were not performed in this phase. These validation limits prevent claiming an unconditional release-quality balance certification.

MINOR: 400-unit scenes remain visually dense; prototype generated sprite polish and previously documented ballistic Spitter shots across static obstacles remain. No new LOS system was added. Optional 1440p review remains unperformed.

COSMETIC: noninteger fitted pixel scaling and existing first-pass sprite frame polish.

## 34. FINAL BALANCE TABLE LOCATION

`Docs/Balance/PHASE_6E_FINAL_BALANCE.md`; complete resolved ScriptableObject dump `Docs/Validation/Phase6E/final_balance-assets.json`. Baseline and final include all defense levels, creatures/bosses, economy/meta/events/carriers and late projections. `source-review.diff` compares this phase with the actual Phase 6D checkout rather than attributing all pre-existing changes to Phase 6E.

## 35. RECOMMENDED NEXT PHASE

Continue Phase 6E balance work before Phase 6F: address late RSD accumulation and the compound full-tree advantage, compare adaptive Border/Port layouts, and resolve Hardened Wire practical value. Complete an isolated standalone-process save/relaunch and sustained Player profiling before certifying the remaining platform-validation limits. No Phase 6F work is started. Keep future audio, new maps/rosters, multiplayer, campaign, Steam, bestiary and online services outside this balance change.

### Representative matrix

|Coverage|Maps|Profiles|Windows / events|Method|
|---|---|---|---|---|
|Earned progression|Both|None / six-node partial / full|1–50 / 1–25 / 1–50 targets; seeded events|Production explicit clocks, actual purchases|
|Six build archetypes|Both|None|20 and 40, no event|Prepared 9000-RSD ceilings|
|All 13 event fights|Both|Representative none/partial/full|Wave 24; swarm/armor/fog/flood/mass plus patterns/economy|Prepared mixed builds, production combat|
|Boss exposure|Both|None|Dredger 25 / Broodmother 50; midgame / specialized|Natural Update x1|
|Concurrent encounter|Both|None|Full Waves 25 / 50, normal threshold|Natural Update x2|
|Density|Port|None|100/200/300/400; mixed roster, weapons and carriers|Natural Update x1, stress-only HP/movement|
|Onboarding/product/persistence|Border, then map tests|Fresh → Emergency Fund|Tutorial → 1–10 → summary → research → new run → Continue|UI bindings, normal first wave then explicit clocks|
|UI|Both contexts|Fresh / actual context|768p, 900p, 1080p; six key contexts|Actual Game View layout/captures|

## Phase 6E.1 Balance Closure

- **Economy:** Waves 1–10 retain their starting funds and income. Between Waves 10–30, ordinary kill income tapers to 12% before integer rounding, and grants taper to 50%; grants then stay at 179 RSD (196 with full research), replacing continued growth to 398/437 at Wave 50. Boss bounties remain 2500/2200. This limits income from larger late formations while retaining repair and upgrade pressure.
- **Port:** eligible direct emergence 30% → 55%; ninth southern inlet and wider dock release positions activate at Wave 11. From Waves 11–31, formation quantity ramps ×5 → ×12, Spitter/Burrower weights ×1 → ×3/×4, pressure ×1 → ×2.25, arrival interval ×1 → ×0.5, with up to six additional carrier slots (32 limit). Late release rate ×3 retains cadence remainder. These increase ranged, bypass and simultaneous pressure across existing chokepoints. Enemy HP remains shared across maps; active cap remains 400. Landing cells reject new construction through the existing placement owner; historical saved structures restore and blocked landing points are skipped.
- **Boss values:** Dredger HP 8000 → 18000, armor 35% → 60%, speed 1.15 → 0.9, siege damage 120 → 150. Broodmother HP 6000 → 13500, armor 15% → 55%, speed 1.65 → 0.9. Moderate combined durability and movement changes extend exposure and allow repeated broods; existing brood cadence and 18-concurrent/60-lifetime caps remain.

Same earned-income light-first plan, seed 1977, 0.1-second production ticks and between-wave purchases; no run-money or enemy-HP grants. All four runs cleared Waves 1–10. Results are representative scripted runs.

|Map / research|Phase 6E result → closure result|Cumulative Homicides before → after|Unused RSD before → after|
|---|---|---:|---:|
|Border / zero|Loss W31 → loss W38|1065 → 1035|17326 → 155|
|Border / full|Clear W50 → loss W41|175 → 1005|76574 → 137|
|Port / zero|Clear W50 → loss W50|100 → 1000|77875 → 2504|
|Port / full|Clear W50 → clear W50|75 → 200|81564 → 7009|

Boss TTK uses the same isolated moving-boss fixtures, zero research, normal Update ×1, and 4725/11700 RSD paid midgame/specialized forces on Border.

|Boss|Midgame TTK before → after|Specialized TTK before → after|Brood releases after, midgame / specialized|
|---|---:|---:|---:|
|Dredger|11.18 → 26.00 s|5.73 → 12.00 s|—|
|Broodmother|9.91 → 24.00 s|4.78 → 11.46 s|3 / 2 (18 / 12 Broodlings)|

**Hardened Wire:** replaces +20% HP with Level III slow 65% → 70%. Levels I/II remain 50%/60%. The focused test verified the actual walkable grid multiplier 0.30, normal speed outside Wire, and cleanup after removal. Existing frozen research snapshots retain their prior values.

Evidence: `Docs/Validation/Phase6E/closure-zero-closure.json`, `closure-full-closure.json`, `closure-boss-ttk.json` (Dredger), `closure-boss-ttkBroodmother.json`, and `closure-play-BalancePlayTests.json`.
