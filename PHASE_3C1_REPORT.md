# Phase 3C.1 — strict top-down cleanup, world polish and boat landings

Validated in Unity 6000.3.24f1 on 2026-10-01. Primary scene: `Assets/Game/Scenes/CoastalDefense_TopDown.unity`. Evidence: `Docs/Validation/Phase3C1/`. Asset receipt: `Docs/Art/Batches/p3c1_polish_001.md`.

## 1. STATUS

**COMPLETE.** Final full suites, live walkthrough, asset audit, screenshot review and Console checks pass. This phase adds one rubber transport presentation and connected wire while preserving the four defenses, Crawler gameplay, authoritative grid, economy, A*, health, rewards and Game Over. No Phase 4 systems were started.

## 2. Isometric issues found

The baseline was run before changes: preparation screenshot captured, wave one started, direct enemy arrivals observed. Findings and the short plan were written to `Docs/Validation/Phase3C1/PLAN.md` before correction.

The camera already used orthographic projection, identity rotation and perpendicular XY framing. The integrated command-post roof, MG sandbag top, sniper deck/hatch, crates, barrel tops, tent, fences, antenna/floodlight footprints and inflatable boat were overhead. These were retained. In-plane Z rotation of a boat/wreck does not indicate isometric camera pitch. There was no reason to replace correct art wholesale.

The actual gaps were inconsistent contact separation, no vegetation, static-only craft, enemies appearing directly at river spawn cells, east-facing idle weapons and disconnected wire. A new reed candidate was rejected because its upright stalks read from the side; v002 fixes that with radial overhead crowns. Existing Runner/Brute inventory was inspected as overhead art; neither has a playable definition/prefab in this project, and no new enemy class was introduced.

## 3. Top-down corrections performed

Retained the existing TopDownCamera and URP 2D renderer: orthographic, identity rotation, negative-Z camera, unchanged 32x18 XY grid with 1.25-unit cells and base cell (29,9). Gameplay roots are unrotated; weapon/boat visual children rotate only around Z. Native and gameplay screenshots were separately audited. No front facade, tilted world, long diagonal shadow or 3/4 replacement sprite was introduced.

The decorative river backdrop extends 12.5 units left of the existing map, using the accepted water tile. It covers off-map approach/return space visible in the camera; it adds no grid/navigation/build cells. Static intact boats were removed from scene dressing to make live arrivals unambiguous. The retained wreck was relocated to water near row 16, clear of the five landing lanes.

## 4. Shadow implementation

Separate Contact Shadow SpriteRenderer children share one 64x64 compact pixel-clustered ellipse and the existing shared sprite material. Color #1F292C, world-aligned offset (+.04,-.04), two short alpha steps 100/180; default renderer opacity .25-.30. Mines use .12, wire .20 with its connected silhouette, boat .18. No per-object material instances, realtime 3D shadows or shadow Update scripts.

Primary Crawler, all four defenses, player base, boats and substantial static props now have contact shadows. Sandbags, crate/barrel tops, generator, tent, bunker/walls, antenna/floodlights and shrubs/reeds were reviewed. There are no large trees/rocks in the active scene requiring additional owners; no tree forest was added. Existing subtle baked contacts remain compatible. Rotating weapon layers carry no long cast shadow. Future Runner/Brute integration must apply the same root-owned shadow convention; this phase does not invent their gameplay prefabs.

## 5. Vegetation added

119 deterministic saved props: 26 reed clumps, 81 grass tufts and 12 shrub crowns. Three accepted source silhouettes, valid quarter-turn variants, no scale interpolation or per-run randomization. The fixed authoring seed is 3101; this is a scene placement seed, not an ImageGen seed.

Reeds follow the water/mud fringe with rows 2,5,8,11,14 clear for landing. Sparse field grass/shrubs render on GroundObjects -9; their contacts use -10. The base apron stays cleared. Vegetation has zero MonoBehaviours, colliders, occupancy or navigation effects. Mines/wire/enemies remain above it. No A* change was made for vegetation.

## 6. Boat landing architecture

WaveManager owns composition, group scheduling, pending/living counts and completion rewards. One BoatLandingSystem on the wave root owns bounded craft presentation and reserved configurable lanes. EnemySpawner.TrySpawnAt remains the sole enemy instantiation/initialization path: validates current walkability, spacing and a real route to the actual base, then runs the existing Initialize/registry/A* flow. Boats do not duplicate health, rewards, combat or navigation.

The rubber prefab contains a hull, five visual passenger seats and a contact shadow. Seats disappear individually after successful unloading; they are renderers, not EnemyControllers. Boats arrive, stop, unload, wait, face back into the river, leave and despawn. The hull's full swept horizontal lane is validated against the water edge and vertical river bounds. Five lanes are selected round-robin, reserved until departure, with at most three active craft. No naval physics/colliders, player controls, damage or other craft types.

Existing Phase 3B pooled wakes appear behind moving craft; a compact medium shore disturbance and small unloading splashes reuse the same VfxService. No new VFX pool or material manager.

## 7. Boat capacity and timing

- Capacity 5 units; current Crawler costs 1. EnemyDefinition has one transportCapacityUnits configuration field, default 1, for future authored weight without introducing types.
- Five berth rows: 2,5,8,11,14. Entry x=-34.375, berth x=-16.875; y follows the original grid row center. Three unloading positions are on the first shore cell, at x=-14.625 with y offsets 0,+.38,-.38.
- Speed 2.5 units/sec; 17.5-unit approach takes 7 seconds. Boat dispatch interval 2 seconds, unloading minimum delay .25 seconds, stopped hold 1 second. Spawner's existing 1-unit separation can extend that minimum while the previous passenger clears the bow.
- Blocked/occupied/unreachable exits retry without losing scheduled passengers. Existing WaveDefinition counts, rewards and combat/economy values were not rebalanced.
- All scheduled passengers remain pending until actually spawned. Combat state begins only after all passengers unload; completion additionally requires every wave-owned living enemy removed. Empty returning boats cannot hold an already completed wave open.
- Game Over stops wave ticking/scheduling, cancels all craft and rejects further unloading. Disable also clears craft and event subscriptions.

## 8. Defense default-facing fix

Native sprite inspection verified both barrels face local +X. TurretPresentation stores visualRotationOffset in one serialized field (0 for these assets), computes the left rest direction from Vector2.left, and initializes MG/sniper weapons left. Root rotation/muzzle references stay intact. Live target aiming retains immediate control. After .75 seconds without a target, weapons return smoothly at 180 degrees/sec; idle delay, return behavior, speed and visual offset are configurable. Saved prefab weapon transforms also face left.

## 9. Barbed Wire connectivity architecture

WireConnectivity reads only four cardinal occupied cells. ConnectedWirePresentation selects a Sprite by the mask and refreshes its shadow. Placement updates the new cell and its four neighbors after activation; death/disable refreshes only adjacent wire. No whole-map scan, wire Update loop or neighbor-search manager.

DefenseSpritePresentation delegates connected-wire sprite ownership to this component, avoiding a competing idle sprite overwrite. Placement ghost reads the same mask without mutating neighbors. BarbedWire still owns the same traversal modifier; cost $50, 1x1 occupancy, walkability, cost multiplier 2 and movement multiplier .5 remain unchanged. Removing the final wire restores normal traversal.

## 10. Bitmask mapping

| Mask | Connection |
|---:|---|
| 0 | isolated |
| 1 | end N |
| 2 | end E |
| 3 | corner NE |
| 4 | end S |
| 5 | straight NS |
| 6 | corner ES |
| 7 | T NES |
| 8 | end W |
| 9 | corner NW |
| 10 | straight EW |
| 11 | T NEW |
| 12 | corner SW |
| 13 | T NSW |
| 14 | T ESW |
| 15 | cross |

## 11. Assets generated/replaced

Built-in ImageGen generated five raw candidates: rejected reeds v001, selected reeds v002, shrub v001, grass v001 and wire cross v001. Raw PNGs are retained in `Assets/Game/Art/SourceSheets/p3c1_polish_001/`; exact prompts/returned filenames/native dimensions/hashes are in the batch and asset audit. No claimed new user approval, model identity or generation seed.

Runtime exports: reeds/shrub 48x48, grass 32x32; 16 wire v002 masks 64x64; shared code-native contact sprite 64x64. Export uses recorded nearest sampling, hard alpha and no-dither channel reduction. Opposing wire boundaries match all 64 horizontal plus 64 vertical mask pairs pixel-for-pixel. All 20 runtime import/alpha checks passed. World imports retain 51.2 PPU, Point, uncompressed, no mipmaps, Full Rect, centered pivots and no generated physics shapes.

Existing artwork remains unchanged. Six existing primary prefabs retain GUIDs and gameplay roots; scene/prefab editing used Unity AssetDatabase/PrefabUtility/EditorSceneManager. Original v001 wire assets remain available. No packages, UI redesign, new defense/enemy roster or multiplayer added. ART_HANDBOOK and ASSET_REGISTER contain the durable rules and output inventory.

## 12. Tests executed

Final compilation: clean in `compile-final.json`. Final complete EditMode suite: **62/62 passed**, including all 16 cardinal masks. Final complete PlayMode suite: **33/33 passed**. Zero failed, skipped or inconclusive tests in either final suite. Results: `editmode-tests.json`, `playmode-tests.json`. Final Console: **0 errors and 0 warnings** after the passing fresh live review (`console-final.json`); the earlier startup Input Manager warning is documented separately.

New runtime coverage checks wire neighbor updates/removal, real connected ghost/no neighbor mutation, unchanged slowdown, every sprite slot, distinct landing lanes, capacity, bounded concurrent craft, full-hull river validation, successful registered/pathing disembarkation, retries on blocked shore, pending waves/completion/reward exactly once, Game Over before and after unloading, left initial facing and delayed idle return, orthographic camera and logic/collider-free vegetation.

All existing gameplay tests run, including legacy XZ regression, grid/A*, economy/placement, MG/sniper targeting, health, mines, wire, rewards, WaveManager, Game Over, art/HUD and pooled VFX. Two historical exact direct-spawn scheduler tests explicitly configure transport null so their unchanged precise assertions still exercise that compatibility path; new primary-scene tests cover boat scheduling. No failure assertions, damage expectations or gameplay thresholds were removed.

During development a new test referenced an unavailable Physics2D test assembly; the collider audit was changed to inspect loaded component types without adding a package. A repeat-authoring iteration initially accessed a removed shadow renderer; null handling now makes repeated authoring safe. Direct Camera.Render during screenshot work triggered URP attachment errors; capture now uses Game View repaint and the installed composited-view encoder, with no runtime rendering workaround. The new idle-return fixture originally checked after 1.4s, earlier than the configured .75s delay plus 1s half-turn; its wait is now 2.2s with the same left-facing assertion. The failing historical result is retained in `playmode-tests-idle-timing.json`. These were fixed before final validation.

## 13. Manual Play Mode validation

Live Editor scripts drove real HUD controls, normal enemy movement/combat and purchases funded by the original $500 plus earned money. No health, damage, wave counts or money was injected in the natural walkthrough. Simulation used 3x speed. This is an automated live walkthrough with visual inspection, not a human mouse-only playtest.

| Wave | Real kills | Base HP at completion | Balance after kill/bonus rewards |
|---:|---:|---:|---:|
| 1 | 10 | 100 | $425 |
| 2 | 15 | 20 | $600 |
| 3 | 20 | 20 | $900 |
| 4 | 25 | 20 | $1,400 |
| 5 | 30 | 20 | $2,050 |

All five reached completion, each paid the exact 25-per-kill and 100 wave bonus, and the fifth entered AllWavesComplete. Grouping sends consecutive passengers to each landing, making lane coverage matter: this recorded layout took 80 damage during wave two; reinforcing upper lanes then preserved the remaining health. No balance values were altered to force success.

Walkthrough assertions and exact outcomes are retained in `walkthrough-checks-first.json`. Its final post-walkthrough HUD fixture initially expected 99 HP even though the natural base had 20; that incorrect fixture assumption was corrected to previousHealth-1 and rerun separately. The earlier failed row is retained, not counted as a passing final check. Final separate HUD/Game Over checks: `live-checks.json`.

Wire topology screenshot is a separate Play Mode fixture with added currency solely to showcase many segments; it was never saved. Arrival review steps only presentation time to capture approach/first unload. Active combat capture pauses on a real TurretCombat.Fired event. No fixture objects, currency or damaged base are saved into the primary scene.

## 14. Performance observations and screenshot review

Static vegetation/shadow renderers have zero Update methods and share material/sprites. Connected wire changes are constant local work, not global scans. One wave tick advances at most three boats; craft seats are plain renderers, and wakes/splashes reuse existing bounded pools. Each unload validates A* before normal initialization; it intentionally incurs that additional route query at a low disembark rate. Boat GameObjects are created per arrival and destroyed on return; pooling is not required for this small bound. No standalone FPS/GC benchmark or stress-profile improvement is claimed in this phase.

Reviewed preparation, populated approach, bow disembarkation, actual firing and continuous wire isolated/straight/corner/T/cross screenshots at 1920x1080. No obvious isometric asset, long cast shadow, floating boat over an empty margin, obscured enemy/mine or wire seam remains in the final review. Wreck placement and world-aligned contact offsets were checked in the final scene. Early captures with stale frames were superseded by refreshed files. Main evidence: `preparation.png`, `boat-arriving.png`, `disembarking.png`, `active-combat.png`, `wire-topologies.png`, wave captures and `scene-audit.json`.

## 15. Remaining issues and readiness

Dynamic camera fit still does not guarantee integer screen pixel scaling, as in Phase 3C. Boat grouping changes encounter cadence; broader player difficulty testing and a standalone hardware profile remain useful before shipping. Existing Input Manager deprecation is unrelated to this phase. Runner/Brute remain art inventory rather than playable types. No additional boats/enemies/defenses, audio pass, campaign, progression or multiplayer were started.

**Ready to proceed to the next explicitly authorized phase.** All required primary-scene checks passed: strict perpendicular camera/art audit, compact shadows, readable nonblocking vegetation, five configurable landing lanes, routed staggered unloading, capacity/concurrency limits, pending-wave completion, Game Over cancellation, MG/sniper left rest and functional aiming, all 16 wire variants/local placement-removal updates/preview, unchanged wire slowdown, complete regression suites and clean final Console. The required phase report exists. No failed or unperformed validation is represented as passing. Editor is left stopped in the clean saved primary scene with no fixture purchases, boats or damaged base.
