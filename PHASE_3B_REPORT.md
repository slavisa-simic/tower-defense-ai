# Phase 3B — Combat VFX and world effects

Validated 2026-10-01, Unity 6000.3.24f1. Primary scene: `Assets/Game/Scenes/CoastalDefense_TopDown.unity`.

## 1. STATUS

**COMPLETE** for the current playable game. The first-pass package contains one VFX reference board, 23 preserved native generation outputs (including the reference and two rejected revisions), 22 runtime PNG assets, 18 sprite-only AnimationClips, a catalog, an original reward digit font, bounded presentation components, and regression/visual evidence. Required consumers are integrated. Gameplay damage, targeting, movement, placement, rewards, waves and Game Over remain authoritative in their existing owners.

The actual repository has one playable Crawler enemy and imported Basic Infected/Runner/Brute art inventory. Basic Infected presentation is now bound to the existing Crawler; no enemy type was added. Medium/large arrival effects are validated inventory for types not yet playable. This follows the project's actual architecture and the prohibition on new gameplay systems.

## 2. Master VFX reference

`Assets/Game/Art/MasterReferences/ref_vfx_core_01.png` is the 1536x1024 overhead VFX board. It was generated before the runtime batch using the previously approved `bd_ref_danube_master_v002.png` anchor, then reviewed for compact scale, palette and overhead camera consistency. It shows MG/sniper flashes, impact, blood, mine, smoke and water studies.

Native outputs and exact prompts are recorded in [the batch record](Docs/Art/Batches/p3b_core_001.md) and [asset register](Docs/Art/ASSET_REGISTER.md). Technical selection was authorized by the user's autonomous request; no separate user approval of generated candidates is claimed.

## 3. MG effects generated

Four 32px muzzle frames at 48 FPS, with yellow-white core, amber edges and compact smoke. A 64x3px code-native streak lives .045 seconds. The flash follows the existing muzzle and remains separate from the unchanged weapon recoil sprites. `TurretPresentation` observes the existing Fired event. The MG's damage, 4 shots/second and targeting code are unchanged.

The first source had a painted checkerboard and was rejected. Its generated v002 correction uses a flat export key, converted to genuine runtime alpha. Both native revisions remain recorded.

## 4. Sniper effects generated

Three sharper, brighter 48px muzzle frames at 40 FPS. A 64x5px tracer has a white/yellow center and narrow amber accent, visible for .06 seconds. It shares the existing hitscan endpoints and recoil event. Sniper damage, range, targeting priority and .5 shots/second remain unchanged.

## 5. Impact effects

Four-frame dirt, mud, concrete, metal and flesh impacts were generated, prepared, imported and clipped. Flesh is requested by current valid MG/sniper enemy shots. There is no surface-impact dispatch in the existing game; the other four remain validated catalog inventory rather than adding a surface-detection system or fictional misses. All five can be requested through the same presentation API.

## 6. Blood effects

A compact four-frame blood hit, 32px small decal and 48px medium decal are present. Live damage uses the hit sheet plus a .075-second sprite tint. Death uses the existing eight-frame Basic Infected death animation in a detached visual slot, a small burst and restrained decal. Immediate enemy removal, targeting exclusion and rewards are preserved. Death visuals fade after approximately 1.489 seconds; decals last at most 12 seconds and share a 48-slot cap.

The medium decal supports the existing effect-scale parameter but is currently inventory, because no larger playable enemy exists. One dedicated hit sequence plus the distinct flesh impact avoids redundant variants in this first pass. No permanent gore or material-per-hit allocation is used.

## 7. Mine explosion

An eight-frame 128px mine sequence at 16 FPS contains ignition, bright blast, dirt/debris expansion, smoke and fading fragments. Debris uses pixels in the animation, with no physics objects. Separate medium smoke supplies the aftermath. Visual size is derived from the already-computed gameplay radius; it does not write or query damage itself.

Live validation confirmed 150 damage just inside the unchanged 2.1875-unit AOE, no damage just outside it, one detonation, and immediate occupancy release. A separate six-frame 64px generic explosion is imported and validated for reuse, with no new gameplay use added.

## 8. Smoke

Small 48px and medium 64px six-frame sequences run at 6 FPS with low opacity (.32/.28), subtle drift and fade. The shared pool has 16 slots. MG/sniper emit small .7-scale puffs only at or below 35% health, at most once per 1.2 seconds per structure. Full-health structures emit none. Mine aftermath fades in one second. Live checks confirm both conditions and smoke expiry.

## 9. Water effects

Small/medium/large six-frame splashes use 48/64/96px cells. The current Crawler receives the small splash on its Water-to-shore transition. A four-frame attached wake is visible while in Water and switches off on exit. Existing movement and grid terrain determine the transition; no spawn or transport mechanics changed.

Faint pooled water ripples and narrow looping shoreline foam give the current river limited motion. At most 18 fixed shore renderers are created once. No boat props are instantiated in the current scene, so no boat consumer is added. The first medium splash source was rejected for upright fountain perspective and regenerated as a planar overhead disturbance.

## 10. UI feedback

`CombatFeedbackHud` adds 24 reusable UI Toolkit reward labels, using an original pixel digit/plus font. Correct kill amounts appear in pale green, rise briefly and fade over .85 seconds. Wave start displays `WAVE n INCOMING`; completion displays `WAVE COMPLETE +amount`, each for 1.5 seconds with a short fade. The completion banner reads the existing wave reward and does not pay it again.

Game Over adds modest darkening and the base-destroyed message to the existing presentation. Existing start-wave/build controls retain their bindings, selection/affordability behavior and disabled states. No new health bars or mandatory UI clutter were introduced.

## 11. Effects integrated

`VfxService` is a scene-owned presentation component, not a gameplay manager or static singleton. It receives requests through the existing turret, mine, enemy lifecycle and UI observers. `EnemyHealth.Damaged` reports committed damage after the existing health operation; it does not change health rules. No VFX callback grants money or causes damage.

Preexisting files changed are limited to:

- `Assets/Game/Scripts/Enemies/EnemyHealth.cs`
- `Assets/Game/Scripts/Presentation/TurretPresentation.cs`
- `Assets/Game/Scripts/Presentation/MineExplosionPresentation.cs`
- Existing MG, sniper and Crawler top-down prefabs
- The primary top-down scene

New presentation components, tests and art are separate additions. All preexisting metadata/GUIDs, gameplay data, other gameplay code, packages, project settings and the Phase 1 scene remain unchanged against the captured baseline. 749 of 756 baseline files are byte-identical, seven are the intended integrations, and none is missing. The scoped code diff is `Docs/Validation/Phase3B/code-review.diff`.

The existing placeholder muzzle/tracer implementation remains a fallback for scenes without the service. Existing older setup utilities can restore placeholders if rerun; the saved primary scene/prefabs are the integrated source of truth. Unity authoring was performed through AssetDatabase/PrefabUtility/scene APIs.

## 12. Pooling implementation

| Channel | Prewarmed cap | Typical consumers |
|---|---:|---|
| Burst | 96 | Muzzles, impacts, blood hits, explosions, splashes, ripples |
| Tracer | 64 | MG/sniper shot segments |
| Smoke | 16 | Mine aftermath, heavily damaged structures |
| Decal | 48 | Blood decals |
| Corpse | 32 | Detached existing death animations |
| UI reward labels | 24 | Kill rewards |

The 256 world slots reuse SpriteRenderers and shared materials. Overflow replaces the oldest visual, never gameplay objects. Reuse resets sprite, tint, lifetime, fade, transform, scale, rotation, sorting, drift and follow reference. Disable/Game Over clears pools and rejects new effects. Enemy wakes reuse one child per living enemy; shore strips are fixed. A 10,000-request overflow run left the same 274 service-child renderers (256 pool slots plus 18 shore strips), then all pools cleared successfully.

## 13. Performance observations

Measured in the live Unity Editor at a 1280x720 Game View on AMD Ryzen 5 5500 / Radeon RX 6600, Windows 11. These are three-second samples of unique frame deltas observed via EditorApplication.update after warm-up, not a standalone Player, GPU profiler or target-hardware certification. Enemies used high health so the load stayed stable; they retained real movement, registry and presentation. All 32 MG nests remained active.

| Moving enemies | Sampled frames | Median frame delta | 95th percentile | Maximum |
|---:|---:|---:|---:|---:|
| 100 | 1352 | 2.20 ms | 2.54 ms | 3.54 ms |
| 300 | 847 | 3.49 ms | 4.30 ms | 6.61 ms |

The fixture recorded 632 cumulative shots. Natural peaks were 82/96 Burst and 22/64 Tracer slots, with no recycling under that shooting load. Deliberate overflow separately exercised all five channels. A warmed same-thread loop of 10,000 muzzle requests took 7.07 ms and allocated **0 managed bytes**. That narrow measurement does not claim that all Unity/UI/gameplay work is allocation-free; reward strings and UI Toolkit can allocate. No obvious rapid-fire stall or unbounded visual accumulation occurred. Evidence: `Docs/Validation/Phase3B/stress-results.json` and reproducible `Stress.cs`.

## 14. Test results

- **46/46 EditMode passed**, zero failures/skips/inconclusive.
- **25/25 PlayMode passed**, zero failures/skips/inconclusive.
- Four new EditMode cases cover pool cap/recycling, reuse reset/follow/fade/sorting, clear/reuse, and committed damage notification.
- Three new PlayMode cases cover observer re-enable and exactly-once reward/death lifetime, one water-exit splash/wake stop, and Game Over cleanup/rejection/economy preservation.
- Existing suites cover placement/occupancy and route validation, economy transactions, movement/wire modifiers, targeting/cooldowns, mine damage/rewards, waves, base/Game Over, and top-down/legacy grid behavior.
- 22/22 runtime PNG checks pass; 23/23 preserved sources match original generation hashes. Unity importer checks and every frame of all 18 sprite-only clips pass.
- Unity compilation completed successfully. Final Console ground truth: **0 errors, 0 warnings**. Historical captured entries include the expected observer-failure log in a passing existing economy test and the preexisting Input Manager deprecation warning; neither is a new VFX issue.
- `git diff --check` passed; Git printed only an existing EnemySpawner CRLF normalization notice. New task files were separately checked for whitespace.

Evidence is in `Docs/Validation/Phase3B/`: `editmode-tests.json`, `playmode-tests-first.json`, `pixel-audit.json`, `source-receipts.json`, `unity-imports.json`, `asset-validation.json`, `compile-final.json`, `console-final.json`, and `repository-integrity.json`. The PlayMode filename says first because it was the first full run; all 25 passed and no runtime code changed afterward.

## 15. Manual Play Mode validation

The primary scene was actually run in the existing Editor. A live walkthrough drove its real gameplay owners, paused key moments for visual inspection, and recorded **29/29 passing assertions**. This was an agent-driven Editor walkthrough and screenshot review, not a claim of a human mouse-play session. Temporary fixture objects and economy/health values were discarded on leaving Play Mode; none was saved into the scene.

| Check | Result |
|---|---|
| MG/sniper muzzle placement, distinct strength and thin tracers | Pass; both actual shot events and screenshot inspection |
| Real damage/cooldowns and several MG nests firing | Pass; 120 combined MG/sniper damage, >30 repeated shots in initial walkthrough |
| Hit tint restore, blood and death animation | Pass; detached death persists after immediate target removal |
| Mine blast, AOE, mine cleanup | Pass; inside/outside radius checks and immediate released occupancy |
| Smoke fade and low-health-only puffs | Pass |
| Actual movement across Water boundary, single splash, wake stop | Pass |
| Correct +25 kill reward and exactly-once payment | Pass |
| Wave incoming/completion, correct +100 completion payment | Pass |
| Game Over disables waves/building, clears/rejects transient VFX | Pass |
| Sorting, readable enemies/structures, limited brightness/blood/smoke | Pass at normal zoom; 1080p/1440p world captures reviewed |
| Crowded world | Pass with 100 then 300 moving enemies and 32 MG nests |
| Current Console | No errors/warnings |

Screenshots include `combat-shots-1080.png`, `combat-shots-1440.png`, `combat-live-1080.png`, `death-reward-hud.png`, `water-wake-1080.png`, `water-exit-splash-1080.png`, `mine-explosion-1080.png`, `mine-explosion-1440.png`, `mine-smoke-1080.png`, `structure-smoke-1080.png`, `wave-start-hud.png`, `wave-complete-hud.png`, `game-over-hud.png`, and `stress-300-hud.png`. The stress screenshot is paused after the synthetic cap/clear check, so it documents actor density rather than active shooting peaks. HUD screenshots are 1280x720; world-only captures use the exact listed resolutions.

## 16. Remaining visual problems

No known blocking Phase 3B integration defect remains. The existing camera uses non-integer screen scaling, so small rotating sprites can still show pixel stepping/shimmer despite correct Point imports. That camera behavior predates this phase and was preserved. The existing base placeholder and unbound prop/UI art from earlier phases remain visible; they are outside this VFX batch. Overlapping crowds remain dense because this phase adds no separation gameplay.

Runner/Brute-specific hookups, boat-prop disturbance, non-flesh surface consumers and generic-explosion consumers cannot be demonstrated in current gameplay because those playable types/events/props do not exist. Their requested reusable art is imported and validated. A standalone build/long-duration target-hardware profile was not run; only the measured bounded Editor stress test is claimed.

## 17. Readiness for next phase

**Ready for Phase 3C.** Phase 3B's current-game definition of done is met. [VFX_ART_SPECS.md](VFX_ART_SPECS.md) records every runtime asset's path, dimensions, frame count, FPS, pivot, consumer, sorting and cap. The asset register and batch record retain source revisions, prompts and validation. The Editor is stopped with the saved primary scene; Phase 3C has not been started.
