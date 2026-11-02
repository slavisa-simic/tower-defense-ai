# Phase 6C — meta progression and research

Validation date: 2026-11-02. Unity 6000.3.24f1. Strict overhead XY; both production maps remain available. Audio remains deferred.

## 1. Status

**COMPLETE.** Final EditMode 155/155 and full PlayMode 96/96 passed; Windows release build succeeds with zero errors. Final Console has zero errors/warnings and compilationFailed=false. Both-map balance, three-run progression, snapshot fairness, UI resolution and saved-asset checks pass with the limits described below. Existing Phase 6B/navigation/shadow work was preserved. No staging, commit, reset, package, multiplayer, new map, campaign, online service or next phase.

## 2. Meta save architecture

`PlayerProfileService` owns independent profile transactions. `ResearchTreeService` owns prerequisite/rank/currency/purchase policy. `ResearchCatalog` and twenty `ResearchNodeDefinition` ScriptableObjects own authored research. `ResearchRewardSettings` owns reward tuning and milestones. `MetaModifierService` is an immutable, copied effective-value view owned by each `RunSession`; BuildManager and WaveManager receive that same view. UI observes these services and issues commands. EconomyManager continues to own run RSD.

`Application.persistentDataPath/BorderDefenders/profile_save.json` and `.json.bak` are independent of `active_run.json`, `run_history.json` and `settings.json`. New Game, Restart and active-run deletion never delete research, RP, lifetime statistics or persistent map bests. For an installation without a profile, known map bests are seeded from existing history without awarding retroactive RP or changing old records.

## 3. RP system

Permanent RP is earned after Game Over and spent only in Main Menu Research. RSD cannot purchase research. No ordinary kill awards individual RP or produces research popups. The generated cyan research-file icon and RP text are distinct from the RSD banknote.

Run completion commits its RP breakdown, claimed milestones, run receipt, lifetime totals and map best in one profile transaction. Receipts remain independent of the rolling twenty-entry history. Repeated Game Over/finalization, application reload, history eviction and primary-profile backup recovery cannot re-award a recorded run. Profile recording precedes history and active-save cleanup; failures remain visible and retryable from the summary. SaveGameService also checks profile receipts when deciding whether Continue is valid.

## 4. RP formula

For Wave Reached below 5, standard RP is zero. Otherwise:

`WaveRP = min(10000, floor(WaveReached × 0.5))`

`BossRP = min(10000, BossesKilled × 10)`

`QualityRP = min(20, floor(WaveReached / 20) × 5)` when final Homicides are at most `max(250, WaveReached × 25)`; otherwise zero.

`RunRP = WaveRP + BossRP + QualityRP + newly claimed milestone rewards`.

All tuning is serialized in `Assets/Game/Data/Research/ResearchRewards.asset`. The per-wave Homicides allowance matters because normal loss occurs at a fixed casualty threshold: a long failed run can qualify for quality, while repeated early deaths cannot. For example, Wave 20 / one boss / 400 Homicides yields 25 standard RP before first-time milestones. Wave 40 / 1000 Homicides can earn 10 quality RP. Ordinary kill count does not enter the formula.

## 5. Milestone rewards

Each ID is claimed once, in the same durable transaction as RP:

| Milestone | RP |
|---|---:|
| First Wave 5 | 8 |
| First Wave 10 | 5 |
| First Wave 25 | 10 |
| First Wave 50 | 20 |
| First boss kill | 10 |
| First Dredger kill | 10 |
| First Broodmother kill | 10 |
| Wave 10+ with Homicides within max(250, 25 × wave) | 5 |
| Wave 20 on Danube Border | 5 |
| Wave 20 on Industrial Port | 5 |

First Wave 5 grants 2 standard + 8 milestone = 10 RP, enough for Emergency Fund or River Monitoring. A repeated Wave 2 death earns zero. Boss/type milestones may stack once on the same run.

## 6. Tech tree architecture

Twenty meaningful nodes, four branches of five, total **610 RP**. All currently have MaxRank 1. Definitions support ID, display name, description, branch, RP cost, tier, icon, prerequisites, effect array and bounded rank. Typed effects are aggregated centrally into numeric modifiers/information flags. No combat component checks research node IDs. Run gameplay never reads profile JSON.

## 7. Logistics branch

Emergency Fund adds 100 starting RSD; Supply Contracts reduces Preparation repair by 10%; Field Salvage raises refund from 50% to 55%; Logistics Network increases only wave-completion rewards by 10%; Reserve Stockpile adds another 150 starting RSD. Kill rewards, event supply and base construction prices are unchanged. Repair spending never enters refundable investment.

## 8. Engineering branch

Reinforced Emplacements adds 10% blocking-defense HP; Hardened Wire adds 20% Wire HP without additional slow; Field Maintenance reduces combat repair by 10%; Modular Construction reduces II/III upgrade costs by 10%; Fortification Doctrine adds another 10 percentage points of blocking HP. Full blocking bonus is 20%, not compounded 21%. Mines remain at one HP and Wire uses its separate modifier. Upgrades still preserve missing HP.

## 9. Weapons branch

Improved Ammunition adds 5% MG/Auto Cannon damage. Optical Calibration adds 8% Sniper range. Area Munitions adds 10% Mortar/Rocket splash radius. Thermal Mixture adds 10% Flame burn duration. Advanced Fire Control adds 5% weapon fire rate, including actual Flame damage-tick cadence. Wire and Mines receive no fire-rate bonus. Fired area projectiles retain committed launch stats; base definitions are not mutated.

## 10. Intelligence branch

River Monitoring reveals rounded approximate count; Advance Warning adds dominant threat class; Biological Analysis adds High/Medium/Low composition; Event Forecasting reveals the following wave's event one wave early; Deep-Water Sensor adds carrier-pressure class, approach sectors and early massive-signature warning. Concentrated emergence identifies the actual focused event sector and warns that other sectors remain possible. Ordinary distributed approaches are labeled as all three sectors rather than inventing a hidden dominant lane.

Counts/composition come from the committed generated plan. Forecasting clones captured event policy state, advances the clone and supplies no economy owner: no reroll, supply credit, cooldown mutation or duplicate event occurs. Existing boss warnings remain. No seeds, budgets, weights or developer pressure values enter player-facing intelligence.

## 11. Node costs

| Branch | Node | RP | Prerequisites |
|---|---|---:|---|
| Logistics | Emergency Fund | 10 | None |
| Logistics | Supply Contracts | 20 | Emergency Fund |
| Logistics | Field Salvage | 25 | Emergency Fund |
| Logistics | Logistics Network | 35 | Supply Contracts |
| Logistics | Reserve Stockpile | 50 | Field Salvage + Logistics Network |
| Engineering | Reinforced Emplacements | 15 | None |
| Engineering | Hardened Wire | 20 | Reinforced Emplacements |
| Engineering | Field Maintenance | 30 | Reinforced Emplacements |
| Engineering | Modular Construction | 35 | Reinforced Emplacements |
| Engineering | Fortification Doctrine | 55 | Hardened Wire + Field Maintenance + Modular Construction |
| Weapons | Improved Ammunition | 20 | None |
| Weapons | Optical Calibration | 25 | Improved Ammunition |
| Weapons | Area Munitions | 30 | Improved Ammunition |
| Weapons | Thermal Mixture | 30 | Improved Ammunition |
| Weapons | Advanced Fire Control | 60 | Improved Ammunition + Optical Calibration + Area Munitions + Thermal Mixture |
| Intelligence | River Monitoring | 10 | None |
| Intelligence | Advance Warning | 20 | River Monitoring |
| Intelligence | Biological Analysis | 30 | Advance Warning |
| Intelligence | Event Forecasting | 35 | River Monitoring |
| Intelligence | Deep-Water Sensor Network | 55 | Biological Analysis + Event Forecasting |

## 12. Prerequisites

Every listed prerequisite is required (AND). The catalog is checked for unique IDs, four equal branches, positive bounded content and an acyclic dependency graph. Purchases reject missing prerequisites, insufficient RP, repeat maximum rank and non-menu context. One explicit Research button purchases the selected node; no respec/free reset exists in production UI.

## 13. Modifier order

Defense values: authored Level I definition → frozen meta multiplier → absolute structure-level multiplier → temporary event modifier → existing runtime/status processing. Damage and maximum HP use Unity RoundToInt once. Wire HP and blocking HP are separate; additive research HP is aggregated before level multiplication. Fog multiplies the researched/leveled range and cancellation restores it without changing configuration.

Repair: `ceil(paid investment × .5 × missingHP/maxHP × phase × meta discount × event discount)`, minimum one RSD if damaged. Preparation phase .75; combat 1.25. Upgrade: `ceil(authored next-level cost × meta × event)`, minimum one. Multipliers used in money arithmetic are normalized to six decimal places; rounding occurs at the final price. Refund is `floor(TotalInvestment × refundFraction)`, where investment is paid construction plus paid upgrades. Refund has a .75 safety ceiling; the shipped tree reaches only .55. Wave reward uses floor(base completion reward × 1.10). Kill income is not multiplied.

Structure panels, upgrade previews, build tooltips, repair/refund buttons and wave-completion text use the same effective queries as gameplay. Flame DPS uses the actual modified damage-tick interval. Build tooltips are rebuilt only when the modifier snapshot or event range changes, not for every shot/kill.

## 14. Run meta snapshot

Fresh RunSession captures effective research once, constructs a copied immutable MetaModifierService and injects it into existing gameplay owners. Starting RSD is base balance + flat research exactly once, with no earned-income credit for that initial grant. `RunSaveData.metaSnapshot` stores values and information flags, not mutable references to research assets. Defense restoration and menu compatibility preflight calculate expected HP from that snapshot.

## 15. Active run / new research behavior

Research is accessible only from Main Menu; paused active gameplay cannot open/purchase it. A player can leave an active run, purchase research and Continue, but that run keeps its saved modifiers, HP, weapon stats and exact money. The next New Game gets the current profile. Mandatory automated UI journey verifies Run A neutral → buy Emergency Fund/Reinforced Emplacements → Continue A still neutral → Run B starts at 600 RSD and builds 330-HP MG. Full-tree Continue separately restores 540-HP Level III MG, damage, paid discounted investment and saved money.

## 16. Profile versioning

Profile schema 1; run schema 4. Run versions 1/2 retain aquatic migration, then receive neutral research; version 3 receives neutral research directly. Existing run money never gains a retroactive starting grant. Unknown/invalid versions and nonfinite/out-of-budget snapshots reject. Old history entries have no fabricated RP award and display no research line.

LocalJsonStore supplies bounded reads, flushed temporary writes and atomic replacement. Profile commits refresh primary and backup with the same award receipts; a failed primary write spends/unlocks nothing. A failed backup refresh reports a warning after a committed primary. Corrupt primary attempts backup; both invalid copies disable profile transactions with a clear message and preserve active-run files. The invalid default version prevents a truncated `{}` from masquerading as a new profile.

Lifetime totals track runs, completed waves, kills, Homicides, total/type boss kills and RP earned/spent. Persistent per-map highest wave is displayed independently of history eviction. Editor/development-only methods provide Grant RP, Unlock Node/All, Reset Research and Reset Meta Statistics; no such controls are in production UI. Resetting research does not reset milestones or run receipts.

## 17. Tech tree UI

Main Menu order: Continue, New Game, Research, Run History, Settings, Quit. Research uses the existing UI Toolkit, pixel font and military panel, with a dark technical grid, four branch columns, generated icons, prerequisite elbows and a selected-node detail panel. Available RP is cyan. Locked, insufficient RP, ready, selected and researched states use explicit wording/markers and borders in addition to color. Hover tooltips and select detail show effect, price and prerequisites. Purchase persists immediately and refreshes dependent nodes without restarting.

Actual 1366×768, 1920×1080 and 2560×1440 captures pass card-containment, twenty-node layout and pairwise no-overlap assertions. Purchased, locked and insufficient-RP views were captured. Runtime tooltip visibility is hidden only by the capture helper for unobscured screenshots; normal hover behavior remains enabled. The longest node labels were tightened after visual review.

## 18. Generated art

Four generated branch strips, twenty sliced icons, plus one generated RP research-file icon. Raw source copies, exact prompts and selection evidence: `Docs/Art/Batches/p6c_research_001.md`, prompt JSON files and `Assets/Game/Art/SourceSheets/p6c_research_001/`. Runtime textures: `Assets/Game/Art/UI/Research/`. Existing master `bd_ref_danube_master_v002.png` supplied palette/material language only; obsolete creature/boat content was not reused. Selection was authorized by the autonomous request, not independent human approval.

Actual raw/runtime dimensions and SHA-256 are in `Docs/Validation/Phase6C/art-manifest.json`. Branch sprites use alpha-bounded rects inside five equal horizontal source regions; sprite IDs are retained on repeat authoring. Point filtering, uncompressed, no mips, real alpha. Raw pixels are retained; no generated world art or existing GUID was replaced. Asset references are local and packaged.

## 19. Intelligence UI

Preparation panel is hidden without information research and during combat. It refreshes at wave/state/event boundaries, not on each spawned passenger. It sits below speed controls, with a tested non-overlap assertion, and leaves the right structure panel available. All research gives actionable creature/threat and focused-sector information before Start Wave, including one-wave-ahead events. Boss Preparation capture is retained alongside the run-research summary.

## 20. Automated test results

Final EditMode: **155/155 passed** in 3.12 seconds. Final full PlayMode: **96/96 passed** in 365.56 seconds. Zero skipped/inconclusive. Includes fifteen new profile/reward/tree/snapshot/effect/corruption rules and six new runtime research integration tests. The runtime suite also covers existing aquatic enemies, navigation/collision, both maps, all defenses, events, structure management, bosses, persistence and 100-wave lifecycle. Existing persistence/aquatic assertions were updated only for the intentional schema 4 migration. Terminal envelopes: `editmode-final.json`, `playmode-final.json`.

The initial full runtime run found a test-fixture profile leak (83/96 passed). The research fixture now restores the previous storage directory/profile cache on teardown; the terminal rerun is the acceptance result. The earlier `{}` corruption failure is fixed and covered. All attempts are retained as diagnostics rather than presented as passes.

Windows release: `Builds/Phase6C/BorderDefenders.exe`, 158,884,863 total output bytes; final incremental build 6.10 seconds, zero errors and one intentional Pipeline warning confirming its runtime endpoint is disabled in release. The first cold build succeeded in 57.19 seconds, but the CLI's independent 30-second request timeout logged an error; that receipt is retained separately and superseded by the clean build with an explicit CLI timeout. Native Player launch was not performed for this phase.

Final saved-assets audit: all three production scenes clean, no missing scripts, exactly one product controller per scene and one RunSession/intelligence HUD per gameplay map, twenty unique node sprites plus RP, correct Point/uncompressed/no-mipmap imports and real alpha. Final fresh-menu → Research → New Game → neutral build/save → Main Menu → Research smoke passes. Console is checked during Play Mode and stopped after archiving intermediate diagnostics: zero errors/warnings, compilationFailed=false. Editor ends stopped in clean MainMenu. Source comparison uses captured pre-task files because the checkout already contained extensive prior work; `source-review.diff` and `git diff --check` were reviewed. Packages and project settings retain their pre-task changes without additional modifications.

## 21. RP economy simulation

Real reward/milestone/purchase/persistence services simulated ten average and ten stronger runs, then continued to completion. Average case: Wave 40, one boss, 500 Homicides; stronger case: Wave 60, two bosses, 400 Homicides; maps alternate. Greedy purchases respect prerequisites and tier/cost ordering. Current result: first useful purchase Run 1; full tree **Run 14 average / Run 9 stronger**. The stronger result is slightly faster than the approximate 10–20 target and still requires substantial runs. Initial fixed-Homicides quality tuning was superseded because ordinary loss uses a fixed casualty threshold. `rp-economy.json` contains every first-ten award, milestone, purchase and balance; it is synthetic performance input, not claimed human run performance.

## 22. Zero-research balance test

Both maps clear Waves 1–10 with seed 1977, real 500 RSD start, threshold 1000, earned income only and scripted MG spending/upgrades/repairs. No money grant or lethal fixture damage. First recorded pass: Border 2543 kills / 350 Homicides; Port 2533 / 600. A repeated late-test opening also clears all ten with slightly different timing outcomes (275 / 550 Homicides). Paths/attacks use production owners and explicit .1-second ticks. Early game remains playable without research. These are scripted strategies, not human mouse-only certification.

## 23. Full-tree balance test

Both maps clear the same ten-wave strategy from 750 RSD with full research: 2557 kills and zero Homicides. The moderate bonuses and one extra opening purchase improve early survival. No first-tier research doubles damage. Full budget: +250 starting RSD, +20% blocking/Wire HP, +5% selected weapon damage, +5% fire rate, +8% Sniper range, +10% splash/burn duration, .9 repair/upgrade factors, .55 refund and +10% completion income.

Controlled late-pressure check jumps the existing thirteen-defense earned opening force from Wave 11 Preparation to Wave 25 without further construction/repairs, money grants or test damage. Both neutral and full-tree forces lose to ordinary breaches. Full-tree Border fails after 27.4 simulated seconds; Port after 32.7, each at 1000 Homicides. This demonstrates vulnerability, not a claim of expert balance, earned-income boss victory or full-investment optimal play. Player power was not compensated by increasing enemy tuning.

## 24. Manual progression test

Script-assisted actual menu/button journey completes three distinct scheduler runs: five waves → +10 RP → Emergency Fund; ten waves → +15 RP → River Monitoring; twenty waves → +20 RP → Supply Contracts. Resulting RP balances: 0, 5, 5. The next runs retain the +100 RSD starting effect, and deeper research becomes affordable. This product-flow fixture uses accelerated arrivals and explicit lethal test damage, separate from earned-income balance. No human mouse-only play is claimed. Receipts: `progression.json`; real nonzero reward summary capture: `run-research-summary.png`.

## 25. Known issues and validation limits

No implementation defect remains known from these checks. Human mouse-only usability/balance, native Player launch, long-session standalone profiling and optimal earned-income midgame/boss victories remain unperformed. The late-pressure check intentionally leaves the early force unchanged despite retained RSD; it does not establish an optimal full-tree survival ceiling. Both-corrupt profile files are preserved and require recovery rather than silently resetting progression. Preparation-only active-run checkpoints retain their established combat-quit rollback behavior. Research art is a coherent generated first pass; original generated alpha is retained rather than claiming binary-alpha exports.

## 26. Recommended next phase

Ready for owner review and the owner's chosen next phase. Human balance/usability and sustained standalone performance review remain recommended alongside future work. No next phase was started automatically.
