# Phase 5C — product flow, local saves, settings and run history

Validation date: 2026-10-03. Unity 6000.3.24f1. Primary gameplay remains strict overhead XY, water left/base right.

## 1. Status
COMPLETE. Product flow, safe local persistence, run history, pause/settings and UI polish are implemented and validated. Final regression: 120/120 EditMode and 71/71 PlayMode tests passed, with no skipped tests. Windows development and release builds succeeded. Release artifact: `Builds/Phase5C/BorderDefenders.exe` (113,651,283 bytes total build output); release build has zero errors and one intentional package warning confirming the runtime control endpoint is disabled. Development QA build had zero warnings/errors. Saved-scene audit found zero missing scripts, one product controller per scene and exactly one gameplay RunSession. Final Console is clean after archiving test/build output.

## 2. Main Menu flow and previous startup
Before this phase, build index zero was CoastalDefense_TopDown. Its scene-owned managers initialized on Awake/Start, GameStateController entered Playing, WaveManager generated Wave 1 in Preparation, and the player used Start Wave. There was no persistence, settings service, or menu; PlayerPrefs was unused.

MainMenu is now build index zero, followed by the existing top-down scene and the separate legacy regression scene. Continue, New Game, Run History, Settings and Quit are real UI Toolkit actions. New Game confirms replacement of an existing valid active save. Scene reload creates a fresh run and resets all simulation owners while retaining independent history/settings. Scene changes show a short LOADING presentation.

## 3. Save architecture
RunSession coordinates the existing scene owners; SaveGameService owns versioned active-run policy; LocalJsonStore owns bounded JSON reads and flushed temporary writes followed by File.Replace/File.Move; RunHistoryService and SettingsService have independent files. There is no global gameplay replacement, duplicate economy/wave manager, enemy serializer, PlayerPrefs state, package, telemetry or cloud service.

## 4. Save location
`Application.persistentDataPath/BorderDefenders/`. On this PC the parent is `C:/Users/Slavisa/AppData/LocalLow/Border Defenders/Shoreline Defense`. Files: active_run.json, run_history.json, settings.json; `.bak` retains the previous durable file and `.tmp` is never used as a Continue source. Tests/live review use isolated temporary directories. Runtime files never go into Assets.

## 5. Save version
Version 1 is a Preparation-only schema. Other versions fail gracefully; no speculative migration. Files over 4 MiB and invalid values are rejected. Active saves include run identity, UTC save time, upcoming wave/completed count, seed, RSD and lifetime economy counters, run duration, breach threshold, statistics, structures and event state. Kills use the existing nine EnemyKind indices. Event dictionaries serialize as bounded key/value lists. Stable defense IDs remain the existing identifiers.

## 6. Autosave rule
Save at initial Preparation, every completed-wave transition into Preparation, immediately before Start Wave, and after successful build/upgrade/repair/dismantle or event changes in Preparation. Stable actions debounce for 0.25 unscaled seconds; Start Wave and menu/application exit flush pending state. No scans or JSON writes every frame. Boundary observers read a cheap wave index, avoiding generation reentrancy. Finite legacy waves do not enter the endless product save path.

## 7. Active-wave quit behavior
Pause → Main Menu during Preparation saves first and refuses to leave if saving fails. During an active wave it confirms that current wave progress will be lost, with Cancel/quit actions. Continue resumes the last Preparation checkpoint. Closing the application during combat keeps that checkpoint. No active enemies, boats or projectiles are serialized. Save/history failures are visible; failed history finalization can be retried before leaving the summary.

## 8. Data saved
Exact balance, earned/spent/refund RSD, dismantle count; Homicides and maximum; all nine kill counters, boss breaches, highest wave, peak enemies/boats, built/upgraded/repaired counts; run seconds; wave/completed count and run seed; bounded event cooldowns/history/counts/recent exclusions and next-event schedule. Each structure includes definition ID, anchor, level, current/max HP, original cost, paid investment and mine arming time remaining. Repair cost is never added to refundable investment.

## 9. Structure restoration
Fresh scene systems initialize before RunSession's late Start restores a pending snapshot, before the first gameplay Update. Preflight rejects incompatible definitions/health tuning, illegal or overlapping footprints. Economy and breach threshold restore explicitly; BuildManager creates inactive staged instances without purchases, sets owned occupancy and instance state, then activates them. Existing wire adjacency and traversal listeners rebuild masks/navigation from cells; visual orientation is not stored. Destroyed/dismantled/consumed mines are excluded. UI refreshes through existing owner notifications.

## 10. Procedural/event restoration
WaveManager reconstructs its generator from the saved seed and wave index. EndlessEventManager restores the committed upcoming event, sector, warning/time, remaining waves, active preparation effects, actual supply received, cooldowns/recent/history/counts and next-event wave. Restoration never calls Prepare or pays supply again. Per-wave seeded generation preserves unrevealed procedural composition and the existing alternating boss-wave rule; live boss entities are unnecessary at this boundary.

## 11. Run History
Last 20 completed runs, newest first, with local display of UTC end time, wave reached, kills, Homicides, bosses, RSD earned, duration and peak enemies. Highest wave is highlighted. GUID run identity makes recording idempotent. History is durably copied to primary/backup before active-save cleanup; the recovered history also rejects a dead active backup.

## 12. Game Over flow
Product summary shows Border Defense Collapsed, wave reached, kills, Homicides, bosses, RSD earned, structures built and duration. New Run/Main Menu finalize history before leaving. Exactly-once finalization keeps a captured immutable record and retries failed disk operations without duplicate entries. Active primary/backup are removed after recording; failed history writes also attempt to remove dead Continue data and keep visible retry feedback.

## 13. Pause menu
Resume, Settings, confirmed Restart Run and Main Menu. Existing GameSpeedController owns Time.timeScale and remembers the last x1/x2 speed; Resume restores it. Gameplay uses scaled time; UI and feedback remain responsive. Escape priority: cancel placement/dismantle, cancel/close modal, clear structure selection, then pause. Space toggles pause; 1–8 use visual build-slot order; Delete opens a refund confirmation for the selected structure.

## 14. Settings
Persistent resolution, fullscreen/windowed, VSync, Low/Medium/High VFX density and reward popups. Apply & Save updates UI/VFX/VSync immediately; display mode applies in the standalone Player. Density limits each existing pool to 25%/50%/100% capacity and clears existing secondary visuals on change, leaving damage/enemy counts unchanged. No fake audio, screen-shake, damage-number or quality controls.

## 15. UI/UX changes
Existing 1280x720 PanelSettings scaling and HUD bindings retained. Military panel/font reuse, explicit hover/focus/pressed/disabled states, scrollable proportional menu cards, shared confirmations, real runtime tooltips, upgrade stat previews, exact effective repair/refund prices, small save feedback, expanded upcoming event wording and compact active indicator with hover detail. Modal pointer interception prevents map commands. Existing development pressure UI remains behind its development plus explicit debug flag.

## 16. Generated UI art
None needed. Existing runtime environment/props/base sprites provide static overhead menu scenery; existing military panels and font serve product screens. No source/reference texture is bound to runtime. No original asset GUID, texture importer, gameplay prefab, footprint or presentation ownership was replaced. Record: Docs/Art/Batches/p5c_product_ui_001.md and asset register.

## 17. Resolution tests
Live checks passed at actual 1366x768, 1600x900, 1920x1080 and 2560x1440 Game View sizes. Main Menu, settings, pause, HUD, announced Fog, selected mortar management and actual upgrade tooltips were captured. Bounds assertions verify menu containment, accessible Settings Back, HUD containment and separation of selection/build panels. Screenshots and layout measurements are in Docs/Validation/Phase5C. Settings checkbox contrast, tooltip anchoring and save feedback placement were refined during review.

## 18. Save/load tests
Seven persistence EditMode tests pass as part of the final 120/120 EditMode suite (1.88 seconds). The six-test product integration suite passes as part of the final 71/71 PlayMode regression (186.93 seconds), with no skipped/inconclusive tests. Tests cover all eight structures, paid upgrades, damage/repair/refunds, multi-cell occupancy, straight/corner/T/cross wire, living mine arming state, exact economy/statistics and scene transitions. Initial failures exposed absent-directory deletion and event-generation reentrancy; both were fixed and rerun successfully. Final results: Docs/Validation/Phase5C/test-summary.json, editmode-final.json and playmode-final.json.

The Windows development Player independently launched MainMenu, created a run, placed a machine-gun nest, upgraded it twice, damaged it, saved, returned to menu and continued. The same GUID/seed, 49 RSD, 451 RSD investment, level III and 420/450 HP were restored. Double Game Over yielded exactly one history entry and no valid Continue. Player log contains no warnings/errors/exceptions. Native display checks measured 1366x768 Windowed/VSync off and 1920x1080 FullScreenWindow/VSync on. Windows chose borderless fullscreen for the requested fullscreen mode. QA uses an isolated temporary local-data directory and its development control endpoint is disabled in the release configuration.

## 19. Corruption tests
EditMode tests cover malformed primary/backup, unsupported version, invalid HP, blocked temporary write preserving the old valid save, and duplicate/bounded history with dead-backup rejection. PlayMode tests cover usable Main Menu with malformed or unsupported saves and successful Continue from a recovered backup.

## 20. Full player flow
The automated launch → New Game → build → real boat wave → autosave → Main Menu → Continue → next wave → Game Over → summary → history → New Run flow passed, including replacement-confirmation cancellation, correct wave/stat/economy restoration and a clean fresh state retaining history. Wave draining explicitly uses accelerated ticks and lethal test damage; this is product-flow verification, not earned-income balance certification. Live visual checks and independent Windows Player save/Continue/finalization checks also passed. Live UI captures use controlled Fog/wave and economy fixtures; the direct native Game Over check ends a Preparation run before Wave 1, so its Wave Reached is correctly zero.

## 21. Known issues
No outstanding Phase 5C defect is known from the completed checks. Earlier human balance, boss-art and sustained profiling limitations remain outside this phase. No human mouse-only run or long-session performance certification is claimed. Version 1 has graceful rejection rather than migration, and active-wave progress intentionally rolls back to the last Preparation checkpoint. Display behavior remains platform-dependent; Windows selected borderless fullscreen in the native check.

## 22. Recommended next phase
Ready for the next production phase. Keep the existing balance and profiling follow-up separate from product-flow completion. The next phase should be explicitly chosen by the owner; none was started. Report and reproducible scripts, raw test/build results, standalone snapshots, scene audit, baseline diffs and final resolution captures are in Docs/Validation/Phase5C. Existing unrelated repository work was preserved; no commit or staging operation was performed.
