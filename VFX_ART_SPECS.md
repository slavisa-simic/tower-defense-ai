# Phase 3B VFX art specifications

Validated 2026-10-01 in Unity 6000.3.24f1. First-pass strict overhead XY pixel art for the existing playable game. The approved style anchor is `Assets/Game/Art/MasterReferences/bd_ref_danube_master_v002.png`; the technically selected VFX board is `Assets/Game/Art/MasterReferences/ref_vfx_core_01.png`. Selection is within the user's explicit autonomous production request; no separate user approval is claimed.

## Import and playback contract

World effects use 51.2 pixels per unit (64 logical pixels per 1.25-unit terrain cell), Point filtering, uncompressed RGBA, no mipmaps, Clamp, no NPOT resizing, Full Rect meshes, no generated physics shapes, and read/write disabled. Import max size is 4096 to preserve all native sources; actual runtime sheets are at most 1024 pixels wide. Runtime alpha is binary with zero RGB outside alpha. Fractional opacity below is applied by shared-material SpriteRenderer color, not baked blur. Original generated sources are preserved separately and never used as runtime frames.

Equal-cell sheets read left to right. Normalized pivots are shown below. Muzzles face local +X; their hot-core pivots attach to the existing MG/sniper muzzle transforms. Other pivots are centered. Existing camera/framing remains unchanged; Point filtering does not make its non-integer screen scaling pixel-perfect.

The 18 animated assets also have sprite-only AnimationClips at `Assets/Game/Animations/Effects/bd_fx_<name>_v001.anim`. Clips contain one SpriteRenderer.m_Sprite curve, no float curves and no animation events. Every frame has been sampled on a temporary Animator rig. Runtime pools use the same ordered sprite arrays and FPS directly, with no per-effect Animator or controller. Wake/foam are attached/simple loops; ambient ripple requests are one-shots. Catalog: `Assets/Game/Art/Effects/CombatVfxCatalog.asset`.

## Runtime inventory

Pool caps are shared by channel, not granted separately to each effect. All pooled slots are prewarmed and recycle the oldest visual on overflow.

| Effect | Asset path | Sheet px | Frames / cell px | FPS | Pivot x,y | Intended event / consumer | Pool / maximum | Sorting layer:order |
|---|---|---|---|---|---|---|---|---|
| mg_muzzle | `Assets/Game/Art/Effects/Combat/MachineGun/bd_fx_mg_muzzle_sheet_v001.png` | 128 x 32 | 4 / 32 x 32 | 48 | 0.375, 0.53125 | MG Fired; existing muzzle transform | Burst / 96 shared | Effects:10 |
| sniper_muzzle | `Assets/Game/Art/Effects/Combat/Sniper/bd_fx_sniper_muzzle_sheet_v001.png` | 144 x 48 | 3 / 48 x 48 | 40 | 0.33333, 0.52083 | Sniper Fired; existing muzzle transform | Burst / 96 shared | Effects:10 |
| impact_dirt | `Assets/Game/Art/Effects/Combat/Impacts/bd_fx_impact_dirt_sheet_v001.png` | 128 x 32 | 4 / 32 x 32 | 20 | 0.5, 0.5 | Validated inventory; no surface-impact event exists | Burst / 96 shared | Effects:3 |
| impact_mud | `Assets/Game/Art/Effects/Combat/Impacts/bd_fx_impact_mud_sheet_v001.png` | 128 x 32 | 4 / 32 x 32 | 20 | 0.5, 0.5 | Validated inventory; no surface-impact event exists | Burst / 96 shared | Effects:3 |
| impact_concrete | `Assets/Game/Art/Effects/Combat/Impacts/bd_fx_impact_concrete_sheet_v001.png` | 128 x 32 | 4 / 32 x 32 | 24 | 0.5, 0.5 | Validated inventory; no surface-impact event exists | Burst / 96 shared | Effects:3 |
| impact_metal | `Assets/Game/Art/Effects/Combat/Impacts/bd_fx_impact_metal_sheet_v001.png` | 128 x 32 | 4 / 32 x 32 | 24 | 0.5, 0.5 | Validated inventory; no surface-impact event exists | Burst / 96 shared | Effects:3 |
| impact_flesh | `Assets/Game/Art/Effects/Combat/Impacts/bd_fx_impact_flesh_sheet_v001.png` | 128 x 32 | 4 / 32 x 32 | 24 | 0.5, 0.5 | MG/sniper Fired at authoritative enemy impact | Burst / 96 shared | Effects:3 |
| blood_hit | `Assets/Game/Art/Effects/Blood/Hits/bd_fx_blood_hit_sheet_v001.png` | 128 x 32 | 4 / 32 x 32 | 20 | 0.5, 0.5 | EnemyHealth.Damaged (alive) / Died | Burst / 96 shared | Effects:2 |
| blood_decal | `Assets/Game/Art/Effects/Blood/Death/bd_fx_blood_decal_v001.png` | 32 x 32 | 1 / single sprite | 1 | 0.5, 0.5 | Existing Crawler death | Decal / 48 shared | GroundObjects:0 |
| blood_decal_medium | `Assets/Game/Art/Effects/Blood/Death/bd_fx_blood_decal_medium_v001.png` | 48 x 48 | 1 / single sprite | 1 | 0.5, 0.5 | Validated inventory; Death API at effectScale > 1 | Decal / 48 shared | GroundObjects:0 |
| mine_explosion | `Assets/Game/Art/Effects/Explosions/Mine/bd_fx_mine_explosion_sheet_v001.png` | 1024 x 128 | 8 / 128 x 128 | 16 | 0.5, 0.5 | LandMine.Detonated; existing AOE represented visually | Burst / 96 shared | Effects:8 |
| explosion_small | `Assets/Game/Art/Effects/Explosions/Generic/bd_fx_explosion_small_sheet_v001.png` | 384 x 64 | 6 / 64 x 64 | 18 | 0.5, 0.5 | Validated reusable inventory; no invented gameplay use | Burst / 96 shared | Effects:8 |
| smoke_small | `Assets/Game/Art/Effects/Smoke/bd_fx_smoke_small_sheet_v001.png` | 288 x 48 | 6 / 48 x 48 | 6 | 0.5, 0.5 | MG/sniper health <= 35%; once per 1.2 seconds | Smoke / 16 shared | Effects:1 |
| smoke_medium | `Assets/Game/Art/Effects/Smoke/bd_fx_smoke_medium_sheet_v001.png` | 384 x 64 | 6 / 64 x 64 | 6 | 0.5, 0.5 | Mine aftermath | Smoke / 16 shared | Effects:1 |
| water_splash_small | `Assets/Game/Art/Effects/Water/Splash/bd_fx_water_splash_small_sheet_v001.png` | 288 x 48 | 6 / 48 x 48 | 16 | 0.5, 0.5 | Existing Crawler Water -> non-Water transition | Burst / 96 shared | GroundObjects:3 |
| water_splash_medium | `Assets/Game/Art/Effects/Water/Splash/bd_fx_water_splash_medium_sheet_v001.png` | 384 x 64 | 6 / 64 x 64 | 16 | 0.5, 0.5 | Validated inventory; no playable Runner exists | Burst / 96 shared | GroundObjects:3 |
| water_splash_large | `Assets/Game/Art/Effects/Water/Splash/bd_fx_water_splash_large_sheet_v001.png` | 576 x 96 | 6 / 96 x 96 | 14 | 0.5, 0.5 | Validated inventory; no playable Brute exists | Burst / 96 shared | GroundObjects:3 |
| water_wake | `Assets/Game/Art/Effects/Water/Wake/bd_fx_water_wake_sheet_v001.png` | 192 x 48 | 4 / 48 x 48 | 6 | 0.5, 0.5 | Attached renderer while existing enemy occupies Water | One existing child renderer per living enemy | GroundObjects:2 |
| water_ripple | `Assets/Game/Art/Effects/World/bd_fx_water_ripple_sheet_v001.png` | 384 x 64 | 6 / 64 x 64 | 5 | 0.5, 0.5 | Existing Water cells; one request per .65 seconds | Burst / 96 shared | Environment:6 |
| shore_foam | `Assets/Game/Art/Effects/World/bd_fx_shore_foam_sheet_v001.png` | 256 x 64 | 4 / 64 x 64 | 3 | 0.5, 0.5 | Existing Water/Beach boundary; looping strip per row | Fixed at boundary creation; <= grid height (18) | Environment:8 |
| mg_tracer | `Assets/Game/Art/Effects/Combat/MachineGun/bd_fx_mg_tracer_v001.png` | 64 x 3 | 1 / single sprite | 1 | 0.5, 0.5 | MG Fired; visual hitscan segment | Tracer / 64 shared | Effects:9 |
| sniper_tracer | `Assets/Game/Art/Effects/Combat/Sniper/bd_fx_sniper_tracer_v001.png` | 64 x 5 | 1 / single sprite | 1 | 0.5, 0.5 | Sniper Fired; visual hitscan segment | Tracer / 64 shared | Effects:9 |

## Timing and intensity

| Effect | Lifetime | Maximum opacity | Fade |
|---|---|---|---|
| mg_muzzle | 0.083 s | 1 | Last 15% of pooled lifetime |
| sniper_muzzle | 0.075 s | 1 | Last 15% of pooled lifetime |
| impact_dirt | 0.200 s | 0.85 | Last 40% of pooled lifetime |
| impact_mud | 0.200 s | 0.9 | Last 40% of pooled lifetime |
| impact_concrete | 0.167 s | 0.9 | Last 40% of pooled lifetime |
| impact_metal | 0.167 s | 1 | Last 40% of pooled lifetime |
| impact_flesh | 0.167 s | 0.7 | Last 40% of pooled lifetime |
| blood_hit | 0.200 s | 0.9 | Last 35% of pooled lifetime |
| blood_decal | 12.000 s | 0.7 | Last 50% of pooled lifetime |
| blood_decal_medium | 12.000 s | 0.65 | Last 50% of pooled lifetime |
| mine_explosion | 0.500 s | 0.95 | Last 40% of pooled lifetime |
| explosion_small | 0.333 s | 0.9 | Last 40% of pooled lifetime |
| smoke_small | 1.000 s | 0.32 | Last 50% of pooled lifetime |
| smoke_medium | 1.000 s | 0.28 | Last 60% of pooled lifetime |
| water_splash_small | 0.375 s | 0.75 | Last 50% of pooled lifetime |
| water_splash_medium | 0.375 s | 0.7 | Last 50% of pooled lifetime |
| water_splash_large | 0.429 s | 0.65 | Last 50% of pooled lifetime |
| water_wake | loop while in Water | 0.45 | Last 40% of pooled lifetime |
| water_ripple | 1.200 s | 0.28 | Last 60% of pooled lifetime |
| shore_foam | loop while game is Playing | 0.2 | Last 40% of pooled lifetime |
| mg_tracer | 0.045 s | 0.8 | Last 0% of pooled lifetime |
| sniper_tracer | 0.060 s | 1 | Last 0% of pooled lifetime |

Mine debris is contained in its eight sprite frames, without physics fragments. The maximum ring is approximately 80% of the 128px canvas. VfxService uses `2 * gameplayRadius / (frameWorldWidth * .8)` for visual scale only; LandMine retains all damage and AOE queries. Smoke drifts at (0.08, 0.06) world units/second and fades; structure puffs use scale .7, mine smoke at most 1.5.

## Existing enemy and UI feedback

The only playable enemy remains Crawler. Its existing Visual/Body now binds the Phase 3A-2 Basic Infected idle/walk/attack/death frames. No Runner/Brute prefab, definition, spawn or wave is added. Walk plays at 8 FPS, attack at 10, idle at 5. Existing EnemyFacing owns rotation. Death plays the existing eight frames at 9 FPS in the 32-slot Corpse pool on GroundObjects:4, holds for .6 seconds in the total lifetime, and fades during the final 30%. Gameplay removal and reward happen immediately. Death art lasts approximately 1.489 seconds; it is not targetable. Enemy tint lasts .075 seconds and restores the original color without instantiating a material.

| UI feedback | Asset / owner | Lifetime / capacity | Behavior |
|---|---|---|---|
| Kill reward | `Assets/Game/Art/Effects/UIFeedback/bd_reward_digits_v001.ttf`; CombatFeedbackHud | .85 s; 24 labels | Original 5x7 digit/plus font, 18px label, pale green, rises 32 panel units/s, fades over final .3s; oldest label reused |
| Wave incoming | Existing UI Toolkit font; CombatFeedbackHud | 1.5 s; one banner | `WAVE n INCOMING`, upper center, 26px text, fades over final .35s |
| Wave completion reward | Existing UI Toolkit font; CombatFeedbackHud | 1.5 s; same banner | `WAVE COMPLETE +amount`; observes transition after authoritative completion reward |
| Game Over | Existing PrototypeHud label plus dimmer | one overlay | Base destroyed message, 50% dark tint, controls retain original disabled states |

Reward text is read from the existing enemy/wave definition; only EnemyRewardService/WaveManager/EconomyManager mutate money. UI labels are precreated on enable, unsubscribed and removed on disable. String updates/UI Toolkit internals can allocate per reward; the zero-allocation claim applies only to the measured warmed sprite dispatch path.

## Presentation ownership and limits

`VfxService` is one scene-owned component with 96 Burst, 64 Tracer, 16 Smoke, 48 Decal and 32 Corpse slots (256 renderers). `WorldWaterPresentation` adds at most one shore strip per row (18 here). Enemy wakes use one reusable child per living enemy. Counts therefore cannot accumulate independently of the bounded pools/current live enemy population. Game Over and disable clear transient pools and reject new effects. Original flash/tracer renderers remain disabled fallbacks for scenes without the service.

Existing layers are retained: Environment for water ambience; GroundObjects for blood, wakes, splash and detached death; Enemies/Defenses retain living bodies; Effects for combat; UI Toolkit above the world. No sorting layer, collider, gameplay root, footprint, material instance or simulation value is added by an effect.

## Reproduction and evidence

- Exact prompts: `Docs/Art/Batches/p3b_prompts/`; selection/receipt record: `Docs/Art/Batches/p3b_core_001.md`.
- Export: `Docs/Art/export_phase3b.py`; per-frame crops, pivots, hashes and filenames: `Docs/Art/Batches/p3b_manifest.json`.
- Unity authoring: `Docs/Validation/Phase3B/ImportAndIntegrate.cs` (Import then Integrate, Edit Mode primary scene); existing asset GUIDs and sprite IDs are reused.
- Frame/alpha/source audit: `Docs/Art/audit_phase3b.py`; Unity importer/clip audit: `Docs/Validation/Phase3B/ValidateAssets.cs`.
- Tests, live assertions, screenshots, performance observations and final Console status: `Docs/Validation/Phase3B/`.
- Completion and real limitations: `PHASE_3B_REPORT.md`.
