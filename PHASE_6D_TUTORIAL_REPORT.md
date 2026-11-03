# Phase 6D — contextual tutorial and first-run onboarding

Validation date: 2026-11-02. Unity 6000.3.24f1. Both production maps remain strict overhead XY. Audio remains deferred.

## 1. Status

**COMPLETE.** Final EditMode 163/163 and full PlayMode 103/103 passed. Windows release build succeeded with zero errors. Console has no implementation errors or compile failures; its one build warning confirms the existing Pipeline agent server is disabled in release Players. No separate tutorial level, difficulty modifier, artificial tutorial income, multiplayer, new map, audio, achievements or next phase was added.

## 2. Tutorial architecture

`Assets/Game/Scripts/Tutorial/` contains five focused sources:

- `TutorialDefinition.cs`: 43 stable lesson keys, centralized short copy, event category, priority, context, highlight target, dismiss policy and pause policy. Plain authored data is appropriate for this small fixed catalog; no additional asset-authoring framework is required.
- `TutorialProfileState.cs`: tutorial-only persistence and transactions.
- `TutorialManager.cs`: independently testable priority/deduplication queue with one active prompt.
- `TutorialRuntime.cs`: scene event adapter and context checks.
- `TutorialView.cs`: UI Toolkit card, temporary UI/world outlines and centralized dynamic details.

The existing product menu installs one scene-owned adapter after services initialize. BuildManager adds a committed `DefensePlaced` event; EnemyRegistry adds `EnemyRegistered`. Existing selection, health, attack, wave, event, breach and menu events supply the rest. Subscriptions are released on scene destruction and pooled-enemy removal. Startup makes one existing-structure scan; no tutorial scans the world every frame. The UI clock only updates presentation/timing. Gameplay owners remain authoritative.

## 3. Profile persistence

`Application.persistentDataPath/BorderDefenders/tutorial_profile.json` is independent of `profile_save.json`, `active_run.json`, `run_history.json` and `settings.json`. It stores its format version, enabled preference, shown lesson IDs and completed-run eligibility. It uses the existing bounded atomic JSON store and backup recovery. A lesson is marked shown when actually presented, rather than when queued. New Game, Restart, Continue and Game Over do not clear it.

Tutorial data is absent from RunSaveData and RunMetaSnapshot. The existing display/settings schema and research modifiers are unchanged. The Settings tutorial toggle saves immediately to the tutorial profile. Reset clears only tutorial progress, preserving the ON/OFF preference and every other file. Save errors are visible in Settings.

## 4. Basic onboarding flow

First new run: short quarantine-line intro → Homicides/objective → RSD → Build → grid/green/red placement → first MG → Start Wave. These are ordinary Preparation and combat scenes. Placement is free, requires real funds and uses the real grid. The final Build wording recommends land near the river and covering the approaches, without prescribing cells. Building during combat remains available.

BasicsCompleted derives from objective, build, Start Wave, Repair and Upgrade exposure. Creature discoveries do not gate basic completion. Important unseen lessons can still appear on later runs.

## 5. Structure tutorials

All eight defenses have concise role hints. MG is introduced after actual placement; Wire, Mine, Sniper, Mortar, Flame, Auto Cannon and Rocket are introduced on selection. Wire/mines explicitly remain walkable. Wire joins, Mortar minimum range and slowed-target Flame synergy use current mechanics; no elemental weakness is invented.

Damaged structures introduce HP and invite selection. Meaningful damage at 80% HP or lower queues Repair, which waits for the real selected damaged structure. It explicitly teaches cheaper Preparation repair. Upgrade waits for a selected, affordable upgrade and displays current → next HP/damage plus the real price; the existing full tooltip remains available. Dismantle requires a later Preparation, at least three live structures and a valid selection; its actual refund appears in the prompt and original button. Neither action is forced. Actual destruction explains no refund; ordinary mine consumption is excluded.

## 6. Creature tutorials

Independent one-time hints cover Crawler, Skitter, Bulwark, Spitter, Broodling and Burrower. Broodlings require at least six encounters during the assault before the swarm hint. Spitter explains ranged structure damage. Burrower explains the limited underground bypass and references the existing visible animated ground trail. Creature hints remain non-blocking. No encyclopedia was created.

## 7. Collision / blockade tutorial

On a solid placement, while the blockade lesson remains unseen, the adapter checks real landing-to-breach paths through the existing pathfinder. A sealed route introduces structure breaching. Actual obstruction attacks introduce DEFENSE UNDER ATTACK once, observing the existing contact-attack event and target. Ordinary Spitter shots with an open border route do not falsely claim all routes are blocked. Navigation, collision, walkable traps and combat policy remain unchanged.

## 8. Event tutorial

The committed event queues a Preparation explanation with its actual authored name/description. Fog and flooded cells have their own one-time lessons. Flood copy preserves existing structures. The original event panel stays visible and receives a temporary outline; tutorial UI does not replace its information or change event state.

## 9. Boss tutorial

The first scheduled boss receives the highest tutorial priority, followed by the Dredger/Broodmother role card. It explains separate boss HP and large breach penalties. The tutorial reserves space below the existing boss warning/HP region; no encounter pause or replacement boss UI was introduced. Dredger recommends heavy weapons; Broodmother explains additional Broodlings.

## 10. RP / Tech Tree tutorial

Only after a recorded Game Over does the summary introduce the real RP award, including a legitimate zero-RP early loss. On returning to Main Menu, Research receives a one-time outline and future-run explanation. First Research visit teaches RP costs, prerequisites and new-run application. Branch-header hover descriptions cover Logistics, Engineering, Weapons and Intelligence. A successful purchase independently explains that Continue retains its original snapshot. Active intelligence research introduces the real Preparation intel once. Research is not automatically opened.

## 11. Map-specific tutorials

First Map Select briefly compares the open Danube field with Port chokepoints. First Industrial Port run explains permanent containers/buildings and natural chokepoints. Previously shown basic controls do not replay. The native walkthrough verified Port after an early Danube loss.

## 12. Priority / queue

One card is active at a time. Boss priority 160 outranks structure attack 140, blockade 130 and breach 125. First intro/core controls precede normal discoveries; optional speed guidance has priority 10 and is eligible after two completed waves. Equal priorities preserve insertion order. Critical lessons can temporarily preempt a lower card, which returns to the same scene queue. Multiple occurrences refresh contextual subjects without duplicate popups.

Selection-dependent lessons defer when their target is unselected, repaired, unaffordable, dead or otherwise unavailable. Preparation and menu lessons wait for their real pages. Gameplay prompts cannot appear over the summary or pause/settings screens. Queued stale events cannot describe a different current event.

## 13. Pause vs non-blocking rules

Tutorials never set Time.timeScale or GameSpeed. Preparation has no countdown, so its intro can wait for Continue without pausing combat. Gameplay banners expire after nine unscaled seconds and can be dismissed with Continue, Enter or Space. Space dismissal does not also toggle Pause. The player's ordinary pause menu suspends gameplay guidance; x2 does not halve its readable duration.

Inline menu cards remain until dismissal/navigation to avoid moving a button under the pointer when a timer expires. Skip has an inline YES/NO confirmation and does not restart the run. Reset requires a separate existing product-menu confirmation. Development-only Inspector context menus provide ID trigger/complete, reset, complete all and queue inspection; triggers respect their context.

## 14. UI / art created

No generated raster art, texture import, prefab rebuild, scene change or asset replacement. The card reuses the existing medium military HUD panel and pixel font. UI/world highlights use temporary UI Toolkit outlines. The actual structure, event, money, Homicides, Research and Start Wave controls remain bound to their existing owners. See `Docs/Art/Batches/p6d_tutorial_001.md` and the asset register.

Walkthrough findings fixed during this phase: menu-only guidance buttons initially inherited default styling; timed inline cards shifted menu targets; a detached Research hover tooltip could linger after navigation; scene unloading could refresh a placement ghost after its SpriteRenderer was destroyed. All received focused fixes.

## 15. Automated test results

- Final EditMode: **163/163 passed**, including eight new tutorial policy/persistence tests (`Docs/Validation/Phase6D/editmode-final.json`).
- Final full PlayMode: **103/103 passed** in 363.50 seconds (`Docs/Validation/Phase6D/playmode-final.json`).
- Focused final tutorial PlayMode after the refinements: **7/7 passed** (`tutorial-playmode-final.json`).
- Windows release Player: **Succeeded**, 0 errors, 17.91 seconds, 158,920,783 bytes (`build.json`), at `Builds/Phase6D/BorderDefenders.exe`.
- One expected package build warning: `RuntimePipelineConfig` has `enableInBuilds=false`, so the agent Pipeline server is excluded from the release Player. This pre-existing configuration was preserved. Final Console: 0 errors, compilationFailed=false; the same expected warning remains recorded rather than being hidden.
- Final diff/whitespace check passes. Legacy navigation fixtures appended their own report files; these results were copied into the Phase 6D evidence folder and the unrelated baseline reports restored.

Coverage includes independent persistence/reload, disabled guidance, reset file isolation, priority/deduplication/preemption, ineligible contexts, actual blockade attacks, all six creature discoveries, actual structure controls/refund/preview, summary/research/Port transitions, boss HUD separation, stable menu targets and unscaled x2 timing. Existing navigation, pooling, economy, damage, wave, save, research and art suites were also exercised. The final full run uses an isolated base profile, including legacy fixtures that do not redirect their own storage.

## 16. Manual fresh-profile test

Agent-operated native Unity Game View walkthrough using actual mouse/keyboard input and a fresh isolated directory. Followed New Game → Danube → short intro/objective/RSD/build/placement/MG/Start Wave → normal live emergence/Crawlers. Placed three freely chosen MGs for the normal 450 RSD total. No injected starting money, enemy HP reduction, scheduled pause or forced placement. The first run lost normally at Wave 1 after 01:51, 33 kills and 1,000 Homicides. The summary correctly awarded 0 RP. This is a real early-loss usability check, not evidence of successful balance or novice comprehension.

The initial deep defensive placement left approaches insufficiently covered; Build wording was improved to recommend river coverage. Repair/Upgrade and later encounters were validated through real gameplay commands in integration fixtures and assisted runtime contexts, rather than claiming all occurred naturally in that short run. Evidence includes `fresh-intro-1920.png`, `fresh-combat-1920.png` and `fresh-profile-after-loss.json`.

## 17. Manual loss / retry test

Returned to Main Menu after the loss, then entered first Research and started Industrial Port through the actual map controls. The completed intro/objective/RSD/build/placement/MG/Start Wave lessons stayed completed; the unseen Port hint appeared alone. Unseen structure lessons remained available. Persistent state was copied to `manual-profile-final.json`. Tests separately verify Continue and subsequent New Game suppression.

## 18. Experienced-player skip test

Native walkthrough: PAUSE → Settings → Tutorial OFF → Back → Resume → select previously unseen Wire. No tutorial appeared; ordinary build selection and standard hover tooltips worked. `manual-tutorial-off.png` captures that state. Automated coverage additionally verifies Skip YES/NO, no queued/active guidance, real placement/wave start with guidance disabled, and reset confirmation without re-enabling OFF.

## 19. Resolution test

**18 captures/checks passed:** intro, selected damaged structure/Repair, first boss warning, summary/RP, Research introduction and Port at each of **1366×768, 1600×900 and 1920×1080**. The harness changed the actual Game View resolution and verified it, rather than resizing screenshots. It checked card viewport bounds, measured text height and separation from Homicides, money, event panel, boss panel, build bar, selected structure panel and Preparation intel where present. Smallest-resolution screenshots were visually inspected; no tutorial text clipping or critical HUD overlap was found. Research content remains normally scrollable while its inline card is open.

Receipts and PNGs: `Docs/Validation/Phase6D/<context>-<width>x<height>.*`. `Walkthrough.cs` reproduces the assisted layout scenarios. The opt-in `TutorialValidationTools` Editor helper redirects before scene startup; its flag is cleared after validation. None of these helpers ship in the release Player.

## 20. Known issues / limits

No known blocking Phase 6D issue in completed checks. An unfamiliar human player has not been tested; this walkthrough was operated by the implementing agent. The natural early loss shows that first-wave coverage/difficulty should be examined in the next balance pass. This phase deliberately leaves baseline wave pressure and economy unchanged. The release Player was built, but native standalone launch and long-session profiling were not performed. Optional creature portraits/arrows were unnecessary because existing sprites, outlines and short text supply the requested guidance.

## 21. Recommended next phase

Ready for a separate polish/balance phase emphasizing fresh human-player comprehension, early river coverage, first-wave survival and lesson cadence under real decision time. That phase has not been started.
