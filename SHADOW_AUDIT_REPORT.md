# Visual shadow audit and correction

Original audit and directional revision R2 completed 2026-11-02 in Unity **6000.3.24f1**, URP **17.3.0**. Scope: Danube Border (`CoastalDefense_TopDown`) and Industrial Port (`DanubeIndustrialPort`). Existing Phase 6B work was already uncommitted and was preserved. No scene redesign or subsequent gameplay phase was performed.

## Current result — stronger lower-right directional casts (R2)

The user rejected the first correction as still too flat. R1's offsets projected to only about 1–2½ screen pixels at the existing fitted gameplay camera. Sorting was checked again: **all 79 enabled Danube and 95 enabled Port shadows render above every Environment terrain renderer**, on GroundObjects -20. The remaining weakness was cast length and contrast, not a hidden shadow layer.

Current shared materials retain #1F2629 and one exact **4:3 lower-right direction**:

| Tier | Offset in logical source pixels | Opacity | Consumers |
|---|---|---|---|
| Low | right 4 / down 3 | 36% | Ordinary creatures, small props, reeds/shrubs, wire and mines |
| Medium | right 8 / down 6 | 40% | Containers, ordinary structures, fixed defense footprints |
| Tall | right 12 / down 9 | 40% | Buildings, cranes, command post and large bosses |

At 51.2 PPU, world XY uses negative Y for screen-down. The 1920×1080 capture's current camera projects roughly half a screen pixel per source pixel: container casts are approximately **4×3 screen pixels**, tall structures **6×4½**, and small contacts **2×1½**. Larger source offsets on structures are needed to make the directional fringe visible at gameplay size. They remain a small fraction of the roof/container/crane footprint, with no shear, perspective, blur or long isometric projection. Small static sprites up to 1.5 world units and wire/mines now use Low rather than a structural cast.

This revision changes three existing shared material settings, the shader defaults, small-prop material assignments in both scenes, and the wire/mine shadow material bindings. It adds **no renderer, texture, material instance, pass, manager or runtime loop**. Offsets move the existing geometry; renderer, shader-pass and texture-sample counts stay unchanged. No frame-time benchmark is claimed. Existing dynamic animation/water/burrow/death/pool behavior is retained. Future authoring and current art contracts use R2; R1 values in the historical record below are superseded.

Fresh Unity before/after images use the same scene, camera position, identity rotation, orthographic size and 1920×1080 fixture. The eight-creature/eight-defense QA setup uses isolated temporary persistence and a 20,000 RSD inspection grant, never saved into production scenes. Camera-only captures exclude HUD. Detail sheets use identical crops, 2× nearest-neighbor and documentation labels.

| Map | Before this revision | After this revision | Detail sheet |
|---|---|---|---|
| Industrial Port | [Before](Docs/Validation/ShadowAudit/r2-before-DanubeIndustrialPort.png) | [After](Docs/Validation/ShadowAudit/r2-after-DanubeIndustrialPort.png) | [Directional comparison](Docs/Validation/ShadowAudit/r2-comparison-DanubeIndustrialPort.png) |
| Danube Border | [Before](Docs/Validation/ShadowAudit/r2-before-CoastalDefense_TopDown.png) | [After](Docs/Validation/ShadowAudit/r2-after-CoastalDefense_TopDown.png) | [Directional comparison](Docs/Validation/ShadowAudit/r2-comparison-CoastalDefense_TopDown.png) |

Visual review: containers now have distinct rectangular lower/right cast strips; crane arm/feet silhouettes cast readable ground shapes; roof edges, command post and defense platforms have clearer directional separation; units and small props keep compact body contacts. Native gameplay framing remains unchanged. Source raster art and its restrained edge shading remain intact, with no second shadow renderer added.

R2 validation: compilation passed; shader supported with zero compiler messages; **127/127 EditMode tests passed**, and focused shadow PlayMode tests **2/2 passed** on both maps. The tests now enforce the stronger opacity range and 4:3 world-space direction as well as animation, single-renderer, death/reuse and water/burrow behavior. The previous 82-test full PlayMode result below belongs to R1; the full PlayMode suite was not rerun for this material/style revision. Final Console has **0 errors / 0 warnings**. The known intentional EditMode observer exception and an earlier unrelated malformed `editor status` tooling command were inspected before clearing Console for the final check. `git diff --check` passed. No standalone build or performance benchmark was run.

Final settings, assignments, sorting, camera metadata, compile/test/Console receipts and captures are the `r2-*` files under [Docs/Validation/ShadowAudit](Docs/Validation/ShadowAudit/). Provenance and reproduction: [R2 batch](Docs/Art/Batches/shadow_directional_002.md), `DirectionalR2.cs`, and updated `Author.cs`.

## Original audit and first correction (R1, historical)

The following sections preserve the original diagnosis and first correction's settings, evidence and tests. Use the R2 rule above for current production.

## 1. What existed before

| System | Finding |
|---|---|
| A — baked sprite shadows | Some local dark rims/contact shading on sandbags, roof edges, container corners and crane feet. No reliable shared external directional cast shadow across the generated set. These pixels remain intact. |
| B — separate shadow sprites | **The primary existing system.** Phase 3C.1 added a shared compact ellipse, tinted sprite copies for connected wire, and child SpriteRenderers. Port Phase 6A used offset copies of object sprites. |
| C — URP ShadowCaster2D | **Not used:** zero ShadowCaster2D and zero Light2D components in either saved map. |
| D — another runtime system | Presentation code selected wire masks and hid one enemy shadow while burrowed. There was no separate lighting/shadow manager or geometric cast-shadow system. |
| E — effectively none | Not globally absent, but contact was effectively invisible under several large objects and inconsistent on newer creatures. |

**Rendering audit.** Both cameras actually resolve to `UnityEngine.Rendering.Universal.Renderer2D`, renderer index **1**, despite the PC URP asset's default renderer index **0** being the forward renderer. The active quality/pipeline is PC / `PC_RPAsset`; Mobile also retains its configured pipeline. `TopDown_Renderer` has no renderer features, depth/stencil enabled, light texture scale .5, maximum 16 light and one shadow render texture. Those settings do not create shadows without 2D lights/casters. PC's enabled 3D shadow-map/soft-shadow options do not cast shadows from these unlit sprites.

All **433** Danube and **796** Port saved-scene SpriteRenderers used `TopDownPlaceholder.mat`, whose actual shader is `Universal Render Pipeline/2D/Sprite-Unlit-Default`. Its historical name is not evidence of placeholder artwork. Sorting layers are Environment, GroundObjects, Enemies, Defenses, Effects; camera rotation is identity and gameplay remains XY.

**Historical implementation.** Phase 3C integrated the art and sorting. Phase 3C.1 explicitly introduced `bd_fx_contact_shadow_v001.png`: a 64×64 ellipse, alpha steps 100/180, (+.04,-.04) world offset, renderer opacity generally .25–.30. Wire used its mask; mines/boats had weaker settings. Phase 6A Port authoring copied object sprites at .27 opacity and the same offset. Phase 6B reused creature art for contacts. The audit found **two shadow renderers per active creature prefab**: a dark child under Visual and an additional white-tinted .30-opacity root sprite copy. Four active organic carrier prefabs have no land shadows, consistent with their submerged/wake/emergence presentation. Retired transport prefabs remain archived and are not active dependencies.

Containers, cranes and buildings in Port are saved scene-authored sprites, not dedicated container/crane/building prefabs. Their presentation was corrected in place without introducing new prefab architecture. Enemy/defense/base prefab consumers were inspected separately.

## 2. Why the game looked flat

- The generic ellipse was smaller than many opaque object footprints and stayed mostly hidden under them. Increasing its tint alone would not fix that geometry.
- Texture alpha multiplied renderer alpha: the old ellipse's effective .25–.30 renderer opacity became approximately **10–12% at its edge and 18–21% in its center**.
- The .04-unit offset is about two logical source pixels. At the existing fitted camera scale, its visible fringe was often around one screen pixel. Port used this same offset for small cargo and tall structures.
- Ordinary sprite tint multiplied painted RGB, producing colored/detailed ghost copies instead of a uniform shadow silhouette. Creature duplicates could disagree with facing and animation, and the root duplicate was not controlled by the burrowing visibility path.
- Seven small cargo/oil props had no contact shadow. Some flat Port floor/marking sprites occupied GroundObjects and could cover lower shadow orders.
- One disabled, sprite-less retired boat still had an enabled ellipse below it. That orphan was found and disabled.

Generated artwork was inspected at native size and in screenshots. Its small baked material/edge shading is retained. No PNG was regenerated, painted over or assigned an additional long baked shadow. Existing separate shadows were corrected/reused; duplicate creature shadows were removed.

## 3. Chosen implementation

Retain **separate child SpriteRenderers**, using the owning sprite's alpha silhouette and one shared unlit pixel-shadow shader. This is a flat sprite pass, not a realtime lighting system. It samples alpha only, so source colors cannot appear in the shadow. There is no blur, perspective projection, stretching or shear.

| Shared material | World XY offset, logical pixels per axis | Opacity | Use |
|---|---|---|---|
| PixelShadowLow | (+2,-2) | 28% | Small creatures, shrubs/reeds |
| PixelShadowMedium | (+3,-3) | 30% | Ordinary props, containers, defense footprints |
| PixelShadowTall | (+5,-5) | 33% | Roofs/buildings, cranes, command post, biological bosses |

All use dark neutral **#1F2629**, light from upper left. At 51.2 PPU these offsets are .0391/.0586/.0977 world units per axis. Offset is applied **after rotation and scale in world XY**, so rotating silhouettes cannot rotate the light direction. Children have no additional translation offset.

Each creature has one shadow under Visual, matching its current animated body frame. The existing EnemySpritePresentation hides it in water, while burrowed, on death and on disable, then restores it on pooled land activation. Wire keeps its existing connectivity/mask observer. Defenses use the fixed base/footprint rather than rotating barrel silhouettes. Water carriers intentionally retain water silhouettes, wakes and emergence without land contact shadows.

## 4. Assets and prefabs updated

- New `Assets/Game/Materials/PixelShadow.shader` plus Low/Medium/Tall shared materials and Unity-generated metadata.
- Eight `_TopDown` creature prefabs: Crawler, Skitter, Bulwark, Spitter, Broodling, Burrower, Dredger, Broodmother. Eight duplicate root shadows removed.
- Eight defense prefabs: MachineGunNest, SniperTower, BarbedWire, LandMine, AutoCannon, MortarPosition, RocketLauncherPosition, FlamethrowerPosition.
- `Assets/Game/Prefabs/Gameplay/PlayerBase_TopDown.prefab` and its scene presentation overrides.
- Both saved maps: Danube props, fortifications, vegetation contacts and base; Port's 24 containers, four cranes, roofs/sheds/booths, checkpoint equipment, barriers, vehicles and small cargo. Seven missing small-prop shadows added; hidden boat orphan disabled.
- EnemySpritePresentation receives only shadow frame/lifecycle synchronization. ART_BIBLE, integration contract and asset register record the durable rule; batch `shadow_audit_001.md` records provenance.

Exact consumers and before/after transforms/materials are in [the authoring receipt](Docs/Validation/ShadowAudit/authoring-receipt.json) and `before/after-*.json` inventories. No runtime PNG/import settings, existing GUIDs, source sprite pivots, body transforms, camera, grid, footprints, colliders, gameplay data, economy, navigation or arrival scheduling were changed by this fix. The legacy XZ regression scene is untouched.

## 5. Sorting and materials

Corrected shadows use **GroundObjects -20**, after all Environment terrain and before solid object sprites. Thirty-one Port floor patches, service-compound paving and flat road/hazard markings move to Environment, retaining their relative orders above terrain. Existing body sorting layers/orders remain unchanged. No new sorting layer or Y-sort manager was added.

Body materials remain unlit and unchanged. Only shadow renderers use the new shared materials. Source Point filtering/no mipmaps/uncompressed imports remain intact. No Light2D, ShadowCaster2D, postprocessing, package or URP renderer setting was added.

## 6. Performance impact and validation

Static shadows have no MonoBehaviour or Update loop. Materials are shared; there are no per-object material instances, shadow-map textures, lights, caster meshes or runtime pixel extraction. Each shadow is one ordinary transparent sprite pass with one alpha texture sample. The three material tiers may affect batching; no zero-cost claim is made.

Saved scene inventory: Danube remains **433 renderers / 80 shadow renderers**, with **79 enabled shadows** after disabling the orphan. Port changes **796→803 renderers / 89→96 shadow renderers**, with **95 enabled shadows**; the existing hidden base presentation remains hidden. Each living creature changes from **two shadow renderers to one**, saving one renderer per actor. Enemy frame synchronization adds constant cached-reference work to the existing presentation loop, with no new scene lookup or per-frame managed allocation in that code. A standalone GPU/frame-time benchmark was not run; these are inspected structural costs, not measured FPS gains.

Validation performed:

- Unity compilation completed with no compilation failures; the new shader is supported and has **zero shader compiler messages**.
- Full EditMode suite: **127/127 passed**, no skips/inconclusive results.
- Full PlayMode suite: **82/82 passed**, including two new shadow tests on both maps.
- After the final orphan cleanup, focused shadow PlayMode tests reran **2/2 passed**.
- Regressions cover one shadow per creature, current animation frame, neutral material, death, pool reuse, water suppression and burrowing suppression. Existing suites cover both maps, placement, wire, defenses, aquatic abilities and arrival scheduling.
- Final live before/after fixture render on both maps: **0 Console errors / 0 warnings**. Startup had the existing Input Manager deprecation warning. The EditMode observer-failure test intentionally logs its expected exception; the test passed, that output is retained in `console-after-tests.json`, and Console was cleared before the independent final live review.
- Scoped diff review against a pre-task backup found only presentation renderer/transform/GameObject edits and base-shadow prefab overrides. No gameplay MonoBehaviour/data changes. `git diff --check` passed; pre-existing line-ending notices are unrelated.

No standalone build, standalone performance profile or 2560×1440 review was performed. The existing dynamically fitted, non-integer pixel camera remains; screenshot validation here is 1920×1080.

## 7. Before/after visual review

Actual Unity Play Mode camera captures use the same map framing, eight creature types and eight defenses. The inspection fixture grants 20,000 RSD in temporary isolated storage, pauses time, and never saves fixture actors into the scene. This is an automated presentation inspection, not a human-operated or earned-income balance walkthrough. HUD is excluded from these camera-only renders. Comparison sheets crop identical image rectangles and enlarge 2× using nearest-neighbor; labels are documentation only.

| Map | Full capture before | Full capture after | Detail comparison |
|---|---|---|---|
| Industrial Port | [Before](Docs/Validation/ShadowAudit/before-DanubeIndustrialPort.png) | [After](Docs/Validation/ShadowAudit/after-DanubeIndustrialPort.png) | [Containers, cranes, warehouse](Docs/Validation/ShadowAudit/comparison-DanubeIndustrialPort.png) |
| Danube Border | [Before](Docs/Validation/ShadowAudit/before-CoastalDefense_TopDown.png) | [After](Docs/Validation/ShadowAudit/after-CoastalDefense_TopDown.png) | [Base, defenses, creatures](Docs/Validation/ShadowAudit/comparison-CoastalDefense_TopDown.png) |

Reviewed findings: Port containers have short rectangular down-right ground fringes, cranes retain their open silhouette with compact cast edges, roofs/booths have restrained structural separation, and small cargo has contact. Danube's command post, defense footprints and shrubs separate from their surfaces without long diagonal shapes. Creature contacts follow their bodies instead of showing colored duplicate art. The overhead camera, map layout and combat readability remain intact; subtle baked edge shading is retained without adding another directional shadow.

Reproduction/tooling, inventories, compiler/Console/test receipts and scoped diff review: [Docs/Validation/ShadowAudit](Docs/Validation/ShadowAudit/). Work stops at this shadow correction.
