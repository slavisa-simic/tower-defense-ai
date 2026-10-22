"""Build the Phase 3B asset inventory from the reviewed export manifest."""
from pathlib import Path
import json, difflib
ROOT=Path(__file__).resolve().parents[3]
read=lambda p:json.loads((ROOT/p).read_text(encoding='utf-8-sig'))
rows=read('Docs/Art/Batches/p3b_manifest.json')
sources=read('Docs/Validation/Phase3B/source-receipts.json')
caps={'Burst':96,'Tracer':64,'Smoke':16,'Decal':48,'Corpse':32}
events={
 'MgMuzzle':'MG Fired; existing muzzle transform', 'SniperMuzzle':'Sniper Fired; existing muzzle transform',
 'MgTracer':'MG Fired; visual hitscan segment', 'SniperTracer':'Sniper Fired; visual hitscan segment',
 'ImpactDirt':'Validated inventory; no surface-impact event exists', 'ImpactMud':'Validated inventory; no surface-impact event exists',
 'ImpactConcrete':'Validated inventory; no surface-impact event exists', 'ImpactMetal':'Validated inventory; no surface-impact event exists',
 'ImpactFlesh':'MG/sniper Fired at authoritative enemy impact', 'BloodHit':'EnemyHealth.Damaged (alive) / Died',
 'BloodDecal':'Existing Crawler death', 'BloodDecalMedium':'Validated inventory; Death API at effectScale > 1',
 'MineExplosion':'LandMine.Detonated; existing AOE represented visually', 'ExplosionSmall':'Validated reusable inventory; no invented gameplay use',
 'SmokeSmall':'MG/sniper health <= 35%; once per 1.2 seconds', 'SmokeMedium':'Mine aftermath',
 'SplashSmall':'Existing Crawler Water -> non-Water transition',
 'SplashMedium':'Validated inventory; no playable Runner exists', 'SplashLarge':'Validated inventory; no playable Brute exists',
 'WaterWake':'Attached renderer while existing enemy occupies Water', 'WaterRipple':'Existing Water cells; one request per .65 seconds',
 'ShoreFoam':'Existing Water/Beach boundary; looping strip per row'}

spec='''# Phase 3B VFX art specifications

Validated 2026-10-01 in Unity 6000.3.24f1. First-pass strict overhead XY pixel art for the existing playable game. The approved style anchor is `Assets/Game/Art/MasterReferences/bd_ref_danube_master_v002.png`; the technically selected VFX board is `Assets/Game/Art/MasterReferences/ref_vfx_core_01.png`. Selection is within the user's explicit autonomous production request; no separate user approval is claimed.

## Import and playback contract

World effects use 51.2 pixels per unit (64 logical pixels per 1.25-unit terrain cell), Point filtering, uncompressed RGBA, no mipmaps, Clamp, no NPOT resizing, Full Rect meshes, no generated physics shapes, and read/write disabled. Import max size is 4096 to preserve all native sources; actual runtime sheets are at most 1024 pixels wide. Runtime alpha is binary with zero RGB outside alpha. Fractional opacity below is applied by shared-material SpriteRenderer color, not baked blur. Original generated sources are preserved separately and never used as runtime frames.

Equal-cell sheets read left to right. Normalized pivots are shown below. Muzzles face local +X; their hot-core pivots attach to the existing MG/sniper muzzle transforms. Other pivots are centered. Existing camera/framing remains unchanged; Point filtering does not make its non-integer screen scaling pixel-perfect.

The 18 animated assets also have sprite-only AnimationClips at `Assets/Game/Animations/Effects/bd_fx_<name>_v001.anim`. Clips contain one SpriteRenderer.m_Sprite curve, no float curves and no animation events. Every frame has been sampled on a temporary Animator rig. Runtime pools use the same ordered sprite arrays and FPS directly, with no per-effect Animator or controller. Wake/foam are attached/simple loops; ambient ripple requests are one-shots. Catalog: `Assets/Game/Art/Effects/CombatVfxCatalog.asset`.

## Runtime inventory

Pool caps are shared by channel, not granted separately to each effect. All pooled slots are prewarmed and recycle the oldest visual on overflow.

| Effect | Asset path | Sheet px | Frames / cell px | FPS | Pivot x,y | Intended event / consumer | Pool / maximum | Sorting layer:order |
|---|---|---|---|---|---|---|---|---|
'''
for e in rows:
    pool=f"{e['channel']} / {caps[e['channel']]} shared"
    if e['id']=='WaterWake':pool='One existing child renderer per living enemy'
    if e['id']=='ShoreFoam':pool='Fixed at boundary creation; <= grid height (18)'
    cell=f"{e['frame']} x {e['frame']}" if e['count']>1 else 'single sprite'
    spec+=f"| {e['name']} | `{e['path']}` | {' x '.join(map(str,e['dimensions']))} | {e['count']} / {cell} | {e['fps']} | {e['pivot'][0]}, {e['pivot'][1]} | {events[e['id']]} | {pool} | {e['layer']}:{e['order']} |\n"
spec+='''
## Timing and intensity

| Effect | Lifetime | Maximum opacity | Fade |
|---|---|---|---|
'''
for e in rows:
    life=e['lifetime'] or e['count']/e['fps']
    duration='loop while in Water' if e['id']=='WaterWake' else 'loop while game is Playing' if e['id']=='ShoreFoam' else f'{life:.3f} s'
    spec+=f"| {e['name']} | {duration} | {e['opacity']} | Last {e['fade']*100:g}% of pooled lifetime |\n"
spec+='''
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
'''
(ROOT/'VFX_ART_SPECS.md').write_text(spec,encoding='utf-8')

batch='''# Phase 3B core VFX batch — p3b_core_001

Status: COMPLETE, validated 2026-10-01. Production began 2026-09-21 and resumed on the user's continue instruction. The user explicitly authorized autonomous generation, correction, import and integration. All selected outputs received technical/visual review; selection does not claim separate user approval.

## Direction and provenance

Built-in ImageGen produced the reference and native artwork directly. The master board used previously approved `Assets/Game/Art/MasterReferences/bd_ref_danube_master_v002.png`. Runtime generations used that approved anchor and the selected `Assets/Game/Art/MasterReferences/ref_vfx_core_01.png` as style references; the MG v002 edit used the rejected v001 sheet as its edit input. Exact prompts are linked per source below. Native generation files are preserved byte-for-byte under `Assets/Game/Art/Effects/SourceSheets/p3b_core_001/`; all 23 hashes match original tool outputs in `Docs/Validation/Phase3B/source-receipts.json`.

The master board is an opaque 1536x1024 style document. Runtime generations use a uniform magenta export key after the initial transparency request yielded a painted checkerboard. The export script keys this backing to hard alpha, uses measured common registration/crop extents, nearest-neighbor reduction and a shared 32-color action palette without dithering. It never edits native source files. Mine row anchors are individually registered without normalizing frame expansion. Two tiny tracer streaks and the original reward digit font are code-native assets, not substitute generated illustrations.

Rejected sources are retained for provenance only: MG v001 has a painted checkerboard; medium water splash v001 has an upright fountain perspective. MG v002 and medium splash v002 replace those sources in all runtime exports. No rejected source is bound to runtime. Master and every exported frame were visually inspected; runtime audit confirms equal cells, nonempty frames, transparent zero-RGB borders and no magenta residue. Point/alpha behavior and scene intensity were reviewed at 1080p/1440p, with HUD captured at the current 1280x720 Game View.

## Native sources

| Source | Native PNG px | Exact prompt | Selection |
|---|---|---|---|
'''
for s in sorted(sources,key=lambda s:s['path']):
    name=Path(s['path']).stem.removeprefix('bd_src_')
    prompt='master_v001' if name=='vfx_master_v001' else name
    disposition='Selected; prepared as runtime art'
    if name=='mg_muzzle_v001':disposition='Rejected: painted checkerboard; v002 replaces it'
    elif name=='water_splash_medium_v001':disposition='Rejected: upright fountain; v002 replaces it'
    elif name=='vfx_master_v001':disposition='Selected reference; copied to ref_vfx_core_01.png'
    batch+=f"| `{s['path']}` | {' x '.join(map(str,s['dimensions']))} | [prompt](p3b_prompts/{prompt}.txt) | {disposition} |\n"
batch+='''
## Prepared assets and consumers

All runtime exports are revision v001, prepared from the selected native revision shown. Runtime paths, cells, pivot, FPS, duration, caps, sorting and every consumer are specified in `VFX_ART_SPECS.md` and the machine-readable `p3b_manifest.json`. These records are authoritative together; filenames follow the repository's bd_fx naming rather than the request's illustrative filenames.

| Runtime asset | Native source / construction | Integration status |
|---|---|---|
'''
for e in rows:batch+=f"| `{e['path']}` | `{e['source']}` | {events[e['id']]} |\n"
batch+='''
Additional deliverables: `Assets/Game/Art/MasterReferences/ref_vfx_core_01.png`; `Assets/Game/Art/Effects/CombatVfxCatalog.asset`; `Assets/Game/Art/Effects/UIFeedback/bd_reward_digits_v001.ttf` (original digit/plus glyphs from `Docs/Art/create_reward_font.py`, fonttools used only in the external tool runtime). All 18 AnimationClips are listed below; each uses the corresponding ordered catalog frames/FPS and sprite-only binding.

'''
for e in rows:
    if e['count']>1:batch+=f"- `Assets/Game/Animations/Effects/bd_fx_{e['name']}_v001.anim`\n"
batch+='''
## Integration and validation

Only the primary scene, two turret prefabs, Crawler presentation, EnemyHealth's new committed-damage notification, TurretPresentation and MineExplosionPresentation change preexisting project files. Their GUIDs are preserved. Crawler is the existing gameplay type; binding Basic Infected art does not create an enemy type. Surface-specific non-flesh impacts, generic explosion, medium decal and medium/large splashes remain validated inventory because no current event/type consumes them. No boat props are instantiated, so boat ambience is not bound. Existing mine debris is visual sprite content only.

46/46 EditMode and 25/25 PlayMode tests passed. All live primary-scene checks passed, including MG/sniper damage, immediate death/reward, mine AOE edges and occupancy release, water exit, smoke, wave rewards and Game Over. 22 runtime texture audits and 18 complete clip sampling checks passed; 23/23 sources match originals. 100/300 moving-enemy and 32-MG stress fixtures passed; all pool overflow/clear checks passed. See `PHASE_3B_REPORT.md` for measurements and limitations. No live validation fixture was saved into the scene. Temporary imported screenshot staging was verified and removed via AssetDatabase; evidence remains under Docs.
'''
(ROOT/'Docs/Art/Batches/p3b_core_001.md').write_text(batch,encoding='utf-8')

register=ROOT/'Docs/Art/ASSET_REGISTER.md'
text=register.read_text(encoding='utf-8');marker='## Phase 3B — combat and world effects (p3b_core_001)'
text=text.split(marker)[0].rstrip()+'\n\n'+marker+'\n\n'
text+='Validated 2026-10-01; autonomous technical selection authorized by the Phase 3B request, no separate user approval claimed. Master anchor, exact prompts/references, retained source revisions, per-frame export operations and replacement reasons: [batch record](Batches/p3b_core_001.md). Full dimensions, import settings, pivots, FPS, caps, sorting and consumers: [VFX specifications](../../VFX_ART_SPECS.md). Native hashes/original paths: `Docs/Validation/Phase3B/source-receipts.json`.\n\n'
text+=batch[batch.index('## Native sources'):batch.index('## Integration and validation')].replace('](p3b_prompts/','](Batches/p3b_prompts/')
text+='Validation: 23 native sources verified, 22 runtime PNGs, 18 sprite-only clips, 46 EditMode + 25 PlayMode tests, live primary-scene checks, capped 300-enemy stress. [Report](../../PHASE_3B_REPORT.md). Existing metadata, gameplay data, legacy scene and packages remain unchanged.\n'
register.write_text(text,encoding='utf-8')

diff=[]
for relative in ['Enemies/EnemyHealth.cs','Presentation/TurretPresentation.cs','Presentation/MineExplosionPresentation.cs']:
    before=(ROOT/'Docs/Validation/Phase3B/baseline-scripts'/relative).read_text().splitlines(keepends=True)
    after=(ROOT/'Assets/Game/Scripts'/relative).read_text().splitlines(keepends=True)
    diff.extend(difflib.unified_diff(before,after,'baseline/'+relative,'Assets/Game/Scripts/'+relative))
(ROOT/'Docs/Validation/Phase3B/code-review.diff').write_text(''.join(diff),encoding='utf-8')
print('Wrote specifications, batch, register and scoped code diff')
