# Phase 6E evidence

Use `PHASE_6E_REPORT.md` at the repository root for the acceptance decision and known issues. This directory retains failed probes and rejected tuning trials so they cannot be mistaken for final results. Current evidence is listed below; older `final-*-30` files precede the burn/cadence fix and are superseded by the Wave-50 attempts.

|Evidence|What it establishes|Limits|
|---|---|---|
|`baseline-assets.json`, `baseline-serialized-assets.json`, `baseline-source.json`|Before-change resolved values, authored assets and actual Phase 6D checkout|The runtime checkout already contained uncommitted Phase 6D work|
|`final_balance-assets.json`|Final resolved data; human-readable tables in `Docs/Balance/PHASE_6E_FINAL_BALANCE.md`|Nominal efficiency differs from effective combat coverage/overkill|
|`final-neutral-50.json`, `final-partial-25.json`, `final-full-50.json`|Both-map earned-income progression attempts, live state and memory/pool counters each wave|Production owners advanced with explicit .1s ticks; fixed between-wave spending plan|
|`final-archetypes-20.json`, `final-archetypes-40.json`|Six archetypes on both maps, actual investment and failures|Prepared 9000-RSD ceilings; individual layouts, not global optimality|
|`event-*.json`|All thirteen events fought on both maps after the burn fix|Representative profiles, prepared mixed layouts at Wave 24|
|`natural-first-wave.json`, `fresh-product-loop.json`, `fresh-*.csv`|Final clean-profile onboarding, natural Update first wave, earned 2–10, once-only award/history, research/new-run UI bindings; actual development CSV transactions|Script-assisted controls; later waves use explicit clocks|
|`product-restart-persistence.json`|Disk reload after exiting/re-entering Play Mode; same run, exact money and remaining RP|Runtime restart; not a standalone executable quit/relaunch|
|`boss-ttk.json`, `full-encounters.json`, `active-boss-*.png`|Natural Update boss exposure and concurrent encounters on both maps|Disclosed purchased budgets; boss death and wave survival are separate outcomes|
|`stress-final.json`, `cpu-samples.json`, `stress-final-*.png`|100/200/300/400 initial populations, Editor main-thread samples, warmed subsystem costs and cleanup assertions|Short shared-host Editor samples; stress-only HP/speed retention; no Player FPS guarantee|
|`vfx-review.json`, `vfx-density-*.png`|Live density setting changes and readable attack feedback|One advancing fight, not a damage comparison between settings|
|`intro-*`, `repair-*`, `boss-*`, `rp-*`, `research-tree-*`, `port-*` resolution JSON/PNG pairs|18 actual resolution/context layout checks at 1366×768, 1600×900 and 1920×1080|Boss-warning tutorial captures are distinct from live boss health-bar captures|
|`effective-stats.json`, `effective-*.png`|Runtime tooltips and upgraded selected-Sniper stats at all three sizes|Native event bindings, no desktop mouse-input claim|
|`burn-timing-before.json`, `burn-timing-after.json`|Reproduction and correction of frame-size-dependent armored burn|Focused numerical probe; production regression test also covers it|
|`editmode-final.json`, `playmode-final.json`, `build.json`|Terminal full-suite and release build receipts|Started/empty/penultimate runs are not acceptance evidence|
|`source-review.diff`|Phase 6E runtime changes against the actual captured checkout|Normal Git diff also contains preserved Phase 6D work|

`Simulation.cs`, `Observe.cs`, `Snapshot.cs`, `Walkthrough.cs`, `Setup.cs`, `TimingProbe.cs`, `RP.cs` and `Build.cs` are external Editor validation scripts. They remain outside `Assets` and are not production controls. Run them through `unity command run_script --file ... --entry ...`; long calls require both the Editor `--timeout_ms` and CLI `--timeout` arguments. Do not run concurrent scene-mutating scripts, recompile during a live fixture, or use another person's active desktop input.

`Setup.Fresh` temporarily opts an isolated product walkthrough into development CSV logging. `Setup.Finish` restores the authored logging default to false and removes its Editor session redirect. Existing user saves are not the validation target.
