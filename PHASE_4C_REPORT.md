# Phase 4C — high-pressure endless mode, dismantle and special encounters

Validation date: 2026-10-02. Unity 6000.3.24f1, primary XY scene CoastalDefense_TopDown. Existing eight defenses, economy/state ownership and legacy regression scene retained. No next phase started.

## 1. Status

**PARTIAL.** Gameplay, tuning, four special infected, both functional bosses, transports, dismantle, automated checks and live performance validation are implemented. The image-generation service rejected the requested violent public-figure boss lineup (HTTP 400 moderation_blocked; request 02cf49aa-5477-4cd8-8c88-a3aba2eba5c3). Undead Osama therefore retains the existing Brute presentation; its requested new animation art and ref_boss_lineup_01.png are absent. No generated candidate is represented as separately human-approved. A human mouse-only difficulty pass and standalone Player profiling remain unperformed.

## 2. Original difficulty baseline

Before changes, seeded wave counts at 1/3/5/10/20/25 were 10/12/21/25/38/52. Wave-one no-defense natural play peaked at five active enemies, averaged 3.4, and had one boat. First unloading took seven seconds; final spawn occurred around 26 seconds. The prior Phase 4B earned-money defensive layout cleared wave 25 with zero Homicides. Evidence: Docs/Validation/Phase4C/baseline.json and PHASE_4B_REPORT.md.

## 3. New enemy pressure settings

Budget = 16 + 8*(wave-1) + 0.08*(wave-1)^2, rounded through the existing generator. Normal enemy composition remains cost-weighted and capped at 240 scheduled enemies. Bosses and escorts are additive. Seed 1977 produces:

| Wave | Planned enemies | Normal budget | Boat cap | Base interval seconds |
|---|---:|---:|---:|---:|
| 1 | 16 | 16 | 2 | 2.200 |
| 3 | 22 | 32 | 2 | 2.018 |
| 5 | 38 | 49 | 3 | 1.864 |
| 10 | 52 | 94 | 4 | 1.566 |
| 20 | 87 | 196 | 6 | 1.186 |
| 25 | 139 | 254 | 8 | 1.058 |
| 30 | 156 | 315 | 8 | 0.954 |
| 50 | 289 | 600 | 8 | 0.686 |
| 100 | 289 | 1592 | 8 | 0.650 |

Counts vary with the seeded weighted composition. Basic and normal Runner groups remain substantial; theme waves increase special weights without replacing the ordinary crowds. Growth is driven primarily by pressure, with existing secondary health/speed scaling retained. Evidence: pressure-settings.json.

## 4. Boat speed before / after

Normal speed increases from 2.5 to 5 world units/second. Entry moves from x=-34.375 to -23.5; berth remains x=-16.875. Approach time falls from seven seconds to approximately 1.325 seconds. Unload interval falls from 0.25 to 0.15 seconds; departure delay from one to 0.2 seconds. Capacity grows from five to seven transport units. Large enemies consume three units. Pirate boss boat runs at 6.5 units/second. Existing river route validation remains in use; boss hull canvases preserve normal boat bounds.

## 5. Boat concurrency settings

Initial two boats, one additional slot per four waves, maximum eight: min(8,2+(wave-1)/4). Arithmetic handles extreme wave numbers without overflow. A lane can accept a new approach once its prior departing boat has cleared the entry safety distance. Five lanes are reused round-robin, enabling return/approach overlap. The live earned run reached six simultaneous boats; natural wave-50 density reached eight. Pending cargo and active enemies still gate wave completion.

## 6. Boat arrival interval scaling

max(0.65,2.2/(1+0.045*(wave-1))). Warlord encounters multiply the interval by 0.7, giving approximately 0.481 seconds at wave 50 and 0.455 at the floor. Boat concurrency and active-enemy limits still apply. First boat dispatch occurs immediately after Start Wave.

## 7. Active enemy targets/caps

Runtime active cap remains 200; normal scheduled cap 240. Largest current boss bundle adds 49, giving a total planned cap of 289. Natural no-defense controls sampled 19 real seconds: wave 25 peaked at 59 active enemies and five boats; wave 50 peaked at 74 and eight boats. The earned-defense wave-25 run peaked at 47 enemies/six boats. These are measured densities, not a claim of naturally sustaining 200 enemies. Controlled stress reached 235 active including 35 real boat arrivals.

## 8. Dismantle architecture

BuildManager owns validation/removal/refund through the existing DefenseBase placement architecture. Placed defenses capture their owner and original committed construction payment. Removal latches once, clears occupancy and disables the root before refund notification; the placement guard prevents reentrant double payment. Existing disable/unregister paths remove Wire links, navigation modifiers and owned visual effects. UI observes state and EconomyManager; it has no currency mutation authority. PlacementController/Input retain the existing mouse, Escape/right-click cancellation and build selection handling. Dismantle mode stays selected after a successful sale. Orange hover preview identifies the occupied defense and refund; green feedback shows credited money. Game Over disables interaction. Fired projectiles and already-applied finite enemy burns continue their independent lifetime after removal.

## 9. Dismantle refund rules

Exactly floor(original paid construction cost * 0.5), in integer RSD. No sale of null, foreign, inactive, destroyed, already sold or consumed mine targets. Refunds count in EconomyManager.RefundIncome and StructuresDismantled, never kill/completion TotalEarned. A 150 RSD machine gun returns 75; odd payments round down. All eight defense types were checked. Live combat visual receipt: dismantle-visual.json, dismantle-hover.png, dismantle-refund.png; balance 350 to 425, occupancy cleared, mode retained, TimeScale=1.

## 10. Special infected

Four data definitions and prefabs join the existing EnemyController, health, movement, registry, targeting, sprite presentation and death/reward flow. Appended enemy kinds preserve existing enum values. No new disembarkation or combat subsystem. All seven ordinary types remain valid turret targets. Theme weights rise on waves modulo seven equal to three (militant) or five (pirate).

## 11. Special infected stats

Base values before endless scaling:

| Enemy | HP | Speed | Kill reward RSD | Breach Homicides | Budget cost | First normal wave | Boat units |
|---|---:|---:|---:|---:|---:|---:|---:|
| ISIS Breacher | 180 | 3.2 | 55 | 80 | 3 | 10 | 1 |
| ISIS Heavy | 900 | 1.4 | 120 | 160 | 8 | 18 | 3 |
| Somali Pirate Runner | 70 | 5.5 | 45 | 90 | 3 | 12 | 1 |
| Somali Pirate Raider | 220 | 3.7 | 65 | 100 | 4 | 20 | 2 |

Generated strict-overhead idle/move/attack/hit/death sequences are bound on all four prefabs. Existing gameplay components, shadows and burn feedback remain.

## 12. Boss framework

BossDefinition derives from EnemyDefinition, adding art, transport, escorts, arrival and boat speed modifiers. BossEncounterDefinition chooses one alternating encounter every 25 waves starting at 25. WaveManager remains the sole scheduling/completion authority. BossEncounterManager observes wave, landing, health, removal and breach events; BossHUDController presents warning, name and live HP without intercepting map clicks. Boss cargo gets a distinct boat; escorts and ordinary groups continue landing. BorderBreachService owns once-only penalties and boss-breach statistics. Enemy death owns normal once-only reward and attribution. Armor mitigation uses double arithmetic and saturation to avoid lethal int overflow; tests cover 0%,20%,35% and int.MaxValue.

## 13. Undead Osama implementation

Wave 25,75,... Base HP 8000, speed 1.15, 35% armor, reward 2500 RSD, breach +1000 Homicides. Its own militant transport carries the boss; escort bundle is 12 Breachers, four Heavies, 14 Basic and four Brutes (34 supporting enemies). Gameplay, HP UI, damage, death reward and breach are tested. Requested custom boss art and lineup were blocked; existing Brute art remains explicitly identified as a placeholder. No attempt was made to bypass the generation rejection.

## 14. Somali Pirate Warlord implementation

Wave 50,100,... Base HP 6000, speed 2.6, 20% armor, reward 2200 RSD, breach +900. Pirate transport speed multiplier 1.3 and arrival multiplier 0.7. Escorts: 20 Pirate Runners, 12 Raiders and 16 ordinary Runners (48). Generated overhead animated presentation and transport are integrated. At wave 50 existing health scaling makes HP 8250. Automated tests validate both kill and breach outcomes and reward once-only behavior.

## 15. Boss wave pressure

Seeded wave 25 contains 104 normal enemies +34 escorts +boss =139; wave 50 contains 240 normal +48 escorts +boss =289. Bosses are added rather than substituted for crowds. Natural controls show real overlapping transports and supporting enemies on the field. Earned-money gameplay cleared waves 1–24 and failed wave 25 when Osama breached: 117 kills that wave, 47 peak active, six peak boats, 1225 total Homicides and Game Over. No boss reward or completion bonus was granted for this breach. Compared with the old zero-breach wave-25 layout, the encounter is consequential.

## 16. Art generated

Five character animation sets (four infected and Pirate Warlord), two boss transport hulls and dismantle tools icon. Twenty-five runtime four-frame sheets (100 frame sprites), twenty-five sprite-only clips and three single-sprite textures. Source v001 candidates with tilted/front-facing anatomy were rejected; corrected overhead faction v002 and Pirate Warlord v003 selected under the user's autonomous authorization. Sources are preserved below Assets/Game/Art/SourceSheets/p4c_content_001. Approved master v002 was reused. Runtime character cells are 64px, boss96px, boats192x96, icon64px; Point/uncompressed/no mipmaps/FullRect/Clamp, 51.2 PPU, center pivots. Nearest export, binary alpha and five-bit channel quantization; source pixels retained separately. Exact prompts/provenance and import receipts: Docs/Art/Batches/p4c_content_001.prompts.json, p4c_content_001.md, art-import.json and final-audit.json. Existing asset GUIDs and placeholders were retained.

## 17. Automated tests

Full suites: **88/88 EditMode; 50/50 PlayMode**, zero failures/skips. Additional post-visual/import review run: **5/5 Phase4CPlayTests** and **2/2 ArtIntegrationTests**. Tests cover all-defense refunds, ownership, repeated removal, wire/cardinal links/navigation, consumed mines/Game Over, boat overlap/cargo completion, boss transport/HP/armor/rewards/breach, seeded pressure and extreme-index arithmetic. Existing 100-wave real-boat acceleration test remains passing. Older fixtures explicitly isolate finite-wave assumptions and account for additive encounters/new capacity; core assertions remain. Tests found and resolved an actual lethal armor overflow and percentage-rounding error. Initial failure evidence is retained separately. Final saved scene has zero missing scripts and is not dirty. Unity compilation succeeds. Evidence: editmode-tests.json, playmode-tests.json, post-review-tests.json, final-audit.json and final-console.json.

## 18. Manual difficulty test

Live Update-driven play with visual screenshot inspection was performed, using scripted placements and actual Start Wave UI submit callbacks, at 4x game time. Starting money was the real 500 RSD; no external money in the progression run. The former Phase4B layout was built/reinforced using earned income through wave 12, then held fixed. Early waves 1–2 caused 225 Homicides; later ordinary waves were controlled by the established layout. Wave 25 ended the run at 1225. Highest reached25; total earned64385, spent6975, end money57910. Busy boss/boat and combat dismantle screenshots were inspected. This is script-driven live play, not a completed human mouse-only manual pass. Evidence: Progress.cs, progress-through-25.json and progress-end-25.png. Density/stress fixtures intentionally use threshold overrides and injected money/HP solely for controlled measurement, never as earned-run proof.

## 19. Performance results

Editor GameView1920x1080, TimeScale1, five-second real-combat samples; seven enemy types, 21 mixed defenses, wire, flame/burn feedback, projectiles and five actual boats. Initial enemies receive high HP only in the stress fixture to preserve load. Boat cargo adds35 enemies.

| Initial enemies | Actual active range | Frames | Median ms | p95 ms | Maximum ms | Mean GC bytes/frame |
|---|---|---:|---:|---:|---:|---:|
| 100 | 100–135 | 1392 | 3.47 | 4.28 | 81.91 | 10908 |
| 150 | 150–185 | 1172 | 4.18 | 5.01 | 93.44 | 10890 |
| 200 | 200–235 | 1049 | 4.63 | 5.60 | 120.90 | 11642 |

No generation0/1/2 GC collection occurred in these short samples. Maximum allocation34621 bytes/frame; managed memory grew3.27–5.47MB. Editor/harness overhead is included. Max frame timings include the first frame after synchronous fixture setup and cannot establish hitch-free steady-state behavior. The practical cap200 is retained; the measured p95 does not justify lowering density or adding speculative pooling packages. Standalone Player frame timings and long-duration memory/GC behavior are unverified. Raw evidence: stress-100/150/200.json and corresponding images; supplementary50-enemy sample retained.

## 20. Known issues

Missing custom Osama animation artwork and requested two-boss lineup due service rejection. No human mouse-only difficulty pass. Editor-only short performance samples include allocation and startup hitches; standalone and sustained-session profiling remains. Later defended waves can still become controlled after substantial reinvestment, but boss25 defeated the former comfortable layout; no claim that every wave continuously saturates active cap. The existing end-run report lists original per-type kill rows while its total includes all enemy kinds; boss counters and peak pressure are available in RunStatistics. The repository already contained extensive uncommitted work; changes were reviewed by relevant code/data/prefab references and validation rather than treating the entire Git diff as newly authored. No commits/staging or unrelated cleanup performed.

## 21. Recommended next phase

Finish the outstanding Phase4C art requirement with a user-supplied acceptable asset or an explicitly revised art brief, then perform human input/playtest and standalone/sustained performance verification. Gameplay is available for that review now; full Phase4C definition of done is not complete. No further boss, defense, multiplayer, campaign, tech tree, permanent upgrade, Steam, leaderboard or audio work has begun.


