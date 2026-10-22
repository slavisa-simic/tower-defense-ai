# Phase 5A — structure selection, upgrades, repair and game speed

Validation date: 2026-10-03. Unity 6000.3.24f1. Primary scene: `Assets/Game/Scenes/CoastalDefense_TopDown.unity`.

## 1. Status

COMPLETE. Implementation, full automated regression and script-assisted live validation pass. Existing uncommitted project work preserved. No audio system, new defense, save/load, tech tree, campaign or multiplayer added. No next phase started.

## 2. Selection architecture

`StructureSelection` observes a single BuildManager-owned defense. `PlacementInput` remains the one map input dispatcher: placement first, global dismantle second, normal structure selection otherwise. Selection resolves through authoritative grid occupancy, including any cell of a multi-cell structure. Empty terrain clears selection. Escape/RMB clears selection/cancels the active mode; HUD pointer interception still prevents map commands. Starting placement/global dismantle clears inspection. Death, consumption, external removal, dismantling and Game Over close the panel through events. No collider-based enemy/boat/prop selection or per-frame scan of defenses.

## 3. Upgrade architecture

Each existing `DefenseDefinition` contains serialized `DefenseLevelData` for II/III: cost, absolute multipliers from Level I and overlay sprite. `DefenseBase` owns level, current/max health, original ConstructionCost and TotalInvestment. Existing weapons read instance properties; configuration assets are never mutated during play. BuildManager supplies explicit ownership/availability/affordability-checked `TryUpgrade`/`TryRepair` commands with its existing transaction guard; EconomyManager remains the currency owner. Upgrade events refresh acquisition/cooldowns, wire slowdown and presentation immediately. Fired projectiles retain their committed launch stats.

## 4. Level rules and health

Newly constructed defenses start at I; II then III, with III maximum. Damage/max HP use Unity's RoundToInt. Maximum-health increases preserve the amount of missing HP: 300/180 HP upgraded to maximum 450 becomes 330/450. Upgrading does not replace a paid full repair. Mine health remains one at every level; an untriggered mine can upgrade/dismantle, but any damage destroys it, so it has no living damaged repair state. Detonation removes it from selection/actions immediately.

## 5. Upgrade costs

II = ceil(original cost × .75), III = ceil(original cost × 1.25). Costs are serialized per level and editable independently.

| Defense | Base RSD | Upgrade II RSD | Upgrade III RSD | Total at III RSD |
|---|---:|---:|---:|---:|
| Wire | 50 | 38 | 63 | 151 |
| Mine | 75 | 57 | 94 | 226 |
| MG | 150 | 113 | 188 | 451 |
| Sniper | 300 | 225 | 375 | 900 |
| Mortar | 400 | 300 | 500 | 1200 |
| Flamethrower | 300 | 225 | 375 | 900 |
| Auto Cannon | 600 | 450 | 750 | 1800 |
| Rocket | 900 | 675 | 1125 | 2700 |

## 6. Per-defense upgrade effects

Values are absolute multipliers of original Level I values, not compounding II→III. General maximum HP: II ×1.25, III ×1.5, with Wire/Mine exceptions below. Damage normally II ×1.25, III ×1.6.

| Defense | Level II | Level III |
|---|---|---|
| MG | Damage 25, rate 4.4/s | Damage 32, rate 4.8/s, range 5.75 cells |
| Sniper | Damage 125, range 12.1 | Damage 160, rate .6/s, range 12.65 |
| Wire | HP 300, movement .4 (60% slow) | HP 400, movement .3 (70% slow) |
| Mine | Damage 188 | Damage 240, blast radius 2.1 cells |
| Mortar | Rate 1/2.4s, damage 120 | Damage 192, rate 1/2.4s, splash ×1.2 (2.7 cells) |
| Flamethrower | 15/tick =75 direct DPS, range 3.85, burn DPS 18.75 | 19/tick =95 direct DPS, range 4.025, burn DPS 24, burn duration 3s |
| Auto Cannon | Damage 69 | Damage 88, rate 2.4/s, range 8.25 |
| Rocket | Reload 4s, damage 250 | Damage 400, reload 4s, splash ×1.2 |

Existing footprints, blocking/occupancy conventions, targeting modes, minimum ranges, arming delay, Flame tick period/cone and transport/combat architecture remain in place. Connected-wire base sprites/adjacency are unchanged; its upgraded slowdown refreshes the owned traversal overlay and navigation revision only on a level change.

## 7–9. Repair architecture, formula and wave modifiers

Full, immediate repair of a living owned damaged defense only. Repairs cannot revive destroyed/consumed defenses. Formula:

`ceil(TotalInvestment × 0.50 × missingHP / maxHP × phaseMultiplier)`, minimum 1 RSD when HP is missing.

Preparation/completion multiplier .75; Spawning/Combat multiplier 1.25. Game Over blocks actions. The basis fraction and modifiers are serialized on BuildManager. Phase comes from the existing WaveManager; pausing does not change combat repair pricing. Repair spend is excluded from investment/refund. Full HP, invalid selection, foreign/unowned targets and insufficient money reject without payment. Panel buttons show current costs/disabled affordability.

Validated III MG example: investment451, 225 missing of450 → 85 RSD Preparation /141 RSD combat. Repairs restore450HP.

## 10. Dismantle integration

Panel and global mode use the same existing command. Refund = floor((original construction cost + paid upgrades) × .50). Original-only refunds remain unchanged, including downward rounding for odd prices. Repairs never increase refund. Repeat/invalid dismantle cannot pay twice; occupancy and wire-neighbor/traversal cleanup remain authoritative. III MG refund225RSD; removing it immediately closes inspection.

## 11. Game speed

One `GameSpeedController` on the existing state object explicitly owns Pause/x1/x2 UI commands: Time.timeScale 0/1/2. Existing enemies, boats, wave scheduling, burn, projectiles, warning timers and VFX already use scaled delta/time. UI input, layout and money/affordability subscriptions remain responsive during pause. Game Over resets speed to1 while existing IsPlaying checks stop gameplay; disable/exit restores1. Fixed timestep is unchanged. Live measured scaled/unscaled time ratios: ~1.000 and2.000.

## 12. UI changes

Existing UIDocument/UXML/USS retained. Structure panel shows name, Roman level, current/max HP and relevant stats: Wire slow, Mine blast, weapons damage/reload/range, Flame DPS/burn, area weapons splash/minimum. Upgrade/repair/refund costs are live text. Max level/full HP/insufficient RSD disable relevant actions. Selected speed uses border/background; selection uses small amber corners. Panel/controls were visually inspected at actual1080p and1440p with the existing pixel font. Build bar and Start Wave/global dismantle bindings coexist.

`StructureHud` subscribes only to selection, money, game/speed events. It checks the WaveManager state value to refresh phase pricing only on transitions, avoiding per-passenger Changed callbacks and all-structure polling.

## 13. Art/generated assets

Sixteen generated add-on sprites, II/III for each existing defense. Existing base/aim/fire/damage sprites, muzzle transforms, GUIDs and wire variants preserved. Authoring/import uses Unity APIs. Atlas at `Assets/Game/Art/Defenses/bd_defense_upgrade_overlays_sheet_v001.png`; raw revisions under `SourceSheets/p5a_upgrades_001/`. Source actual1536x1024RGBA; runtime256x256atlas, 64px/cell,51.2PPU,Point,uncompressed,hard alpha,no mips. Full prompt/reference/revision/import/consumer record: `Docs/Art/Batches/p5a_upgrades_001.md` and asset register. Autonomous selection authorized by this request; no independent human art approval claimed.

## 14. Automated tests

Full EditMode:99/99 passed, including two new health-capacity/repair/death rules. Final full PlayMode:59/59 passed in148.13seconds, including eight management tests and the complete pre-existing51-test regression suite. Focused cases verify actual LevelIII weapon damage, upgraded turret range, consumed-mine deselection and external-removal panel cleanup. Results: `Docs/Validation/Phase5A/editmode-envelope.json`, `playmode-envelope.json` and `management-tests-final.json`. Compilation succeeds. Expected exception logs from the pre-existing broken-observer test are recorded, not implementation failures; final Console is inspected separately.

Final Console:zero errors/warnings, compilationFailed=false; `console-final.json`. Prior entries preserved in `console-history.json` before clearing. Saved integration audit confirms one selection/speed/HUD controller, eight valid definitions/prefabs, sixteen overlays, no missing scripts and clean saved scene. Runtime raster audit confirms only0/255alpha and transparent centers in all16cells. Scope review is in `source-review.diff`; it uses the existing Phase4D source snapshots because this checkout already had extensive untracked/uncommitted content. No commit/staging/reset occurred. Editor remains stopped in the saved primary scene; validation budget, fixtures, damage and speed are not saved.

## 15. Live Play Mode validation

Script-assisted live UI actions and actual Game View visual inspection, not a human mouse-only session. A disclosed50,000RSD fixture built all24defense/level combinations, clicked real UI Toolkit submit actions, inspected pricing/selection, repaired during Preparation/active Wave, dismantled an invested structure and exercised Pause/x1/x2. Natural Update-driven combat cleared Wave1 and returned to Preparation for Wave2. This prepared-budget fixture does not claim normal earned-money balance. Receipts/captures: `live-walkthrough.json`, `all-levels.png`, `preparation-repair.png`, `wave-repair.png`, `paused-selection.png`, `active-combat.png`.

Initial live capture was invalid because an unfocused Editor was not rendering/ticking reliably. The fixture now enables temporary Application.runInBackground and focuses Game View; final measurements compare scaled/unscaled elapsed time after warm-up. No project setting or gameplay workaround was added. Existing automated regressions cover boat unloading, bosses, breach/Game Over and wave completion; focused tests pause a real dispatched boat and wave clock while the panel remains usable.

Actual1920x1080 and2560x1440 panel checks cover Mortar/Flame/Wire/Mine text, multi-cell screen-to-occupant mapping, UI button hit targets and clearance above the existing build bar. Receipts: `resolution-1080.json`, `resolution-1440.json` and `panel-<type>-<height>.png` captures. Both resolutions were visually inspected; final Game View is1920x1080.

## 16. Known issues and limits

No known Phase5A gameplay/compile regression after validation. Human mouse-only usability/balance and long standalone profiling were not performed. Earlier Phase4C/4D report limitations (boss art, demanding opening balance and long-session profiling) remain outside this phase. Mine repair is intentionally unavailable because its existing one-HP state is full or destroyed. This is functional selection/upgrade/repair infrastructure, not final upgrade balance or save/load.

## 17. Recommended next phase

Ready for planning the next phase after reviewing human usability and upgrade/repair balance under normal earned income. Saveable instance state is organized, but no save/load system was started. No next phase started automatically.
