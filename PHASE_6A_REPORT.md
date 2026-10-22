# Phase 6A — second map and Danube Industrial Port

Validation date: 2026-10-03. Unity 6000.3.24f1. Supersedes the stale blocked inspection: the completed Phase 5C baseline was present and extended.

## 1. Status
Implementation complete; final regression and Console results are appended below. Existing unrelated uncommitted content preserved. No staging/commit/reset, package, new defense/faction, audio, multiplayer, campaign or next phase.

## 2. Map architecture
MapDefinition stores stable identity, name, scene, description, style, preview and battlefield. MapCatalog resolves identities. Scene-owned MapRuntimeContext binds existing GridManager, boats and breach service and validates initialization. Grid/pathfinding, economy, combat, waves, director and Homicides remain in existing owners. BattlefieldDefinition adds explicit water/ground/static-blocker/no-build/floodable masks; empty masks preserve original/legacy behavior. No scene-name gameplay branches, LOS or interiors.

## 3. Map definitions
Assets/Game/Data/Maps/DanubeBorder.asset: river_border / DANUBE BORDER / CoastalDefense_TopDown, preserving original battlefield/GUID. IndustrialPort.asset: industrial_port / DANUBE INDUSTRIAL PORT / DanubeIndustrialPort. MainMenu remains build index zero; port and separate legacy scene are included.

## 4. Map select flow
New Game → preview/name/description/style cards → Select → Start → replacement confirmation when applicable → selected scene. Selection/cancellation does not delete a save. Restart uses current map; Game Over New Run opens selection. Continue resolves saved identity automatically. Actual UI Toolkit submit actions validated both map launches, history and Continue at 2560×1440.

## 5. Industrial Port layout
32×18 XY cells at 1.25 units; same orthographic camera. West/lower-left river, three projecting wet-concrete docks, staggered container blocks, warehouse/service roofs, three eastward route families with cross connections, cleared building pockets and fortified inland roads. Single-cell removal test confirms no universal one-cell chokepoint.

## 6. Landing sectors
Three explicit sectors: North Dock 3 points, Central Terminal 3, South Dock 2: eight berths and sixteen unloading starts. Berth X varies between projecting pier tips and recessed bank loading areas. Simple horizontal authored approaches avoid naval pathfinding. Hull sweeps check actual water cells, allowing off-grid western/lower river. Port boats account for nearby traffic across berths; original lane behavior retained.

## 7. Breach zones
Two cells (31,5) and (31,12) feed the shared BorderBreachService/RunStatistics/Homicides threshold. No duplicate penalties or loss pools. Existing base root/components retained with industrial checkpoint presentation.

## 8. Pathfinding design
Existing weighted grid paths route every enemy. Static masks initialize before spawning. Context validates every unloading start and approach and reports invalid development geometry loudly. Validate returns landing/exits/buildable/blocked/invalid counts. One northern unloading point initially overlapped the crane; it was corrected. Successful startup logs were removed because legacy tests reject unexpected logs.

## 9. Buildable / blocked areas
Port has 290 buildable and 265 non-walkable cells including water/static obstacles; original has 459 buildable. Dock tips walkable/non-buildable. Container/building/crane/checkpoint masks block movement and placement. Small equipment/cargo/oil/ladders are decorative, without gameplay scripts/colliders. Shared green/red placement and occupancy rules remain.

## 10. Art generated
Built-in ImageGen generated ref_industrial_port_map_01.png, bounded sixteen-prop sheet, corrected overhead revision, four-material terrain sheet and port preview. Agent selection authorized by autonomous request, not independent human approval. Exact prompts, provenance and raw revisions: Docs/Art/Batches/p6a_port_001.md and prompt JSON; dimensions/hashes: Docs/Validation/Phase6A/art-manifest.json. First prop revision rejected for front/side faces; only overhead v002 exported.

## 11. Environment assets
Sixteen 128×128 prop exports and four 64×64 terrain exports; 51.2 PPU, Point, uncompressed, no mips, binary alpha. Rust/olive/steel-blue/open containers; warehouse/service roofs; forklift/truck/crane; pallets/crates/barrel lids/generator; mooring/ladder/barrier/oil decal/floodlight. Concrete/asphalt/wet-dock/stained-marking variation. Existing water/fences/sandbags reused. Static contact shadows at (+.04,-.04), no shadow Update. Twenty generated runtime textures packed into PortEnvironment atlas, rotation disabled/Point. Raw/reference images have no runtime consumers.

## 12. Event compatibility
All thirteen forced event definitions pass port tests: Mass Landing, Pirate Raid, Militant Assault, Heavy Assault, Runner Swarm, Fog, Ammunition Shortage, Emergency Supply, Field Engineers, Calm Before Storm, Multi-Sector, Concentrated Landing, Flooded Shore. Flood checks authored dockside cells across footprints and matching visual overlays; existing structures preserved, inland concrete available. Name remains Flooded Shore. Shared Fog visibility/range effects clear correctly; three sectors receive boats. No second event manager/director.

## 13. Boss compatibility
Both existing boss transports dispatch/unload/path to normal breach service. Both hulls are 3.75×1.875 units and fit validated berths. No new boss tuning. Existing encounter/escort/armor/reward/UI tests retained; actual boss HUD captured.

## 14. Save/load changes
Run snapshots and RunRecord store mapId. Session capture uses scene context; wrong-map restoration is rejected before applying state. Menu preflight resolves correct battlefield and rejects blocked/no-build/overlapping footprints. Port integration builds all eight defenses, upgrades II/III, damages/repairs applicable defenses, dismantles one, saves/continues and verifies seven restored defenses, money, levels and HP. Existing detailed wire/event/economy/history persistence regression remains.

## 15. Save version change
Active version 1→2. Version 1 resolves only to Danube Border regardless of absent mapId, remaining untouched until successful Preparation checkpoint writes v2. Version 2 requires nonempty identity and known map for Continue. Unknown maps/versions/incompatible footprints reject without deleting primary/backup. History stays schema v1 with optional identity; missing identity displays original map. Preparation-only saves, active-wave rollback, atomic writes, recovery and GUID finalization unchanged.

## 16. Run History changes
Each record and Game Over summary shows map name. Live finalized runs on both maps produce distinct map identities; both-map-history.png verifies display. Other stats/time/economy bindings preserved.

## 17. Map 1 regression
Original art/scene roots/configuration retained apart from context/catalog references. 122/122 EditMode passed. Initial full PlayMode 73/75: two failures solely from successful-validation Debug.Log, removed. Focused five-test TopDownPlayTests then passed. Final full-suite result appended below. Suite covers original and legacy maps, bosses/events/waves/build/save/Continue/wire.

## 18. Map 2 early-wave test
Scripted 0.1-second shared wave/boat/enemy/MG ticks, shared 500 RSD start, no extra money/health. Three MG opening, prompt earned-income purchases and between-wave upgrades/repair clear Waves 1–5: 649 kills, 650 Homicides, 12,712 RSD. Passive three-MG opening failed Wave 1; purchases without upgrades cleared four and failed Wave 5. All receipts retained. This is a workable earned-income strategy, not human balance certification.

## 19. Map 2 midgame test
Accelerated real wave/boat/enemy ticks complete all Waves 10–20, ordinary breach resolution, threshold explicitly disabled for traversal coverage. Verifies routes/deployment/events/completion; not undefended survival or earned-income midgame victory. Management fixture uses disclosed 50,000 RSD budget. No starting-money/combat/pricing changes.

## 20. Boss tests
Both port cargo cases pass with exactly one boss breach each. Existing reward/armor/encounter/escort/UI regression remains. Live Undead Osama HUD and landed boss capture. No manual earned-income boss victory claimed.

## 21. Event tests
Four port PlayMode tests pass including thirteen event sweep, sector arrivals, flooding, Fog cleanup, waves and save/Continue. Initial event fixture retained combat state between forced cases; corrected to independent Preparation restores. Eligibility architecture was not bypassed.

## 22. Performance observations
Static environment SpriteRenderers without per-prop Update/colliders, runtime atlas, shared bounded enemy/boat/VFX pools and path caches. Early/midgame/event/boss and high-count regressions complete without route stalls. Long standalone profiling not performed; no sustained-FPS claim.

## 23. Screenshots / visual review
Reviewed reference, rejected/corrected sheets, gameplay-size container-defenses, multi-sector, mass-landing, boss-encounter/HUD, earned-income combat/result, map selection/confirmation, port HUD/Game Over and both-map history. Live UI 2560×1440; camera screenshots 1920×1080 omit UI. Zero missing scripts across both maps/menu. Orthographic identity camera preserved. Map1 preview is actual capture; port preview generated illustration matching layout, not gameplay evidence.

## 24. Known issues / limits
Human mouse-only usability, earned-income midgame/boss victories and long standalone profiling unperformed. Opening demands spending/upgrades; comparative balance follow-up advisable. Pixel exports are generated content, not handcrafted final art. No audio/LOS/interiors. Read final summary for current Console/test results rather than intermediate buffers.

## 25. Recommended next phase
Phase 6A is scoped to reusable two-map play. Owner chooses next phase; none started. Human comparative balance/performance review should precede further content expansion.


## Final validation summary

STATUS: COMPLETE. Full EditMode 122/122 passed (1.89 s); final full PlayMode 75/75 passed (205.74 s), zero skipped/inconclusive. After the final HUD margin presentation fix, six persistence/menu integration tests passed again (13.81 s). Live UI checks passed at actual 1920×1080 and 2560×1440. Port-specific wire cross/T/corner/straight, Level III and exact-money/T-mask restoration through Continue also passed (disclosed 5,000 RSD fixture), recorded in port-wire-roundtrip.txt.

Windows release build: Builds/Phase6A/BorderDefenders.exe, succeeded in 23.97 s, 121,360,927 bytes, zero errors. One intentional package warning: RuntimePipelineConfig enableInBuilds=false disables the Pipeline control endpoint in Player builds. The new executable was built; separate native player launch was not performed for Phase 6A.

Final Console ground truth: compilationFailed=false, zero errors/warnings. Intermediate compile/test/log buffers archived. Final saved-port validation checks all sixteen unloading-to-exit paths; Editor stopped in DanubeIndustrialPort. Zero missing scripts in menu and both maps. Source review diff, tests, authoring/build/QA scripts, generated-art hashes and captures retained under Docs/Validation/Phase6A.

Visual correction: HudArtPresentation now covers only camera-reserved top/bottom margins with opaque non-picking strips, preventing previous-scene backbuffer remnants. Original camera/grid/HUD bindings unchanged. Actual 1080p boss/HUD capture was inspected after the correction. Map 1 preview was recaptured without viewport letterboxing. A mistakenly Assets-relative tool capture was copied to the validation folder and removed through AssetDatabase.

No implementation defect remains known from these checks. Human earned-income midgame/boss victories and sustained standalone profiling remain validation limits, not claimed outcomes. Ready for owner-selected next-phase planning with comparative balance/performance review; no next phase started.
