# Phase 3A-3 — Defense assets and structure states

Completed 2026-09-21. Detailed contract: [DEFENSE_ART_SPECS.md](DEFENSE_ART_SPECS.md). Production history: [batch record](Docs/Art/Batches/p3a3_defenses_001.md). Evidence directory: [Phase3A3](Docs/Validation/Phase3A3).

## 1. STATUS

**COMPLETE.** Generated and saved the first production art set for all four existing defenses, integrated it into their existing prefabs and placement previews, and validated the primary scene. The batch contains 29 preserved native generations and 24 runtime PNG exports: 23 defense exports plus one revised sniper UI icon. All required assets and documentation exist. Optional mine animation/trigger and optional wire junctions are not needed for this first pass.

## 2. Master defense lineup

`Assets/Game/Art/MasterReferences/ref_defense_lineup_core_01.png` is the final 1774 x 887 style reference, selected from native lineup v002. It follows the previously approved `bd_ref_danube_master_v002.png`. The four structures share the established palette, overhead projection, contact shading, and scale hierarchy. Four individual defense reference stills were generated and reviewed before their production assets.

Lineup v001 established the production direction first. Internal review corrected the sniper to hide the lower legs/braces; final lineup v002 records that corrected deck design. Internal selection was authorized by the user's autonomous workflow; it is not a claim of a separate user approval of each image. The approved Phase 3A-1 master was not changed.

## 3. MG assets created

Six PNGs in `Assets/Game/Art/Defenses/MachineGunNest/`: fixed idle, damaged, and destroyed bases; rotating weapon idle; four-frame 256 x 64 firing sheet; and a composed placement preview. Frames are 64 x 64, 51.2 PPU, 20 FPS. The mount is (32, 32), muzzle (58, 32), and the existing muzzle child is at local (0.5078125, 0, 0). Firing animates recoil without a baked final muzzle flash. The existing base and rotating weapon transforms remain separate.

## 4. Sniper assets created

Six PNGs in `Assets/Game/Art/Defenses/SniperTower/`: fixed idle, damaged, and destroyed bases; rotating aim idle; three-frame 288 x 96 firing sheet; and composed placement preview. Frames are 96 x 96, 51.2 PPU, 12 FPS. The deck silhouette is 56 x 58 pixels, so its larger transparent canvas does not enlarge its one-cell footprint. Mount (48, 48), muzzle (82, 48), local attachment (0.6640625, 0, 0). No final tracer was generated.

The sniper alone required a revised 64 x 64 UI icon, `Assets/Game/Art/UI/bd_ui_sniper_tower_icon_v002.png`. The previous icon is preserved. UI remains the existing functional text controls; the revised icon is imported inventory for later UI binding.

## 5. Wire assets created

Nine 64 x 64 PNGs in `Assets/Game/Art/Defenses/BarbedWire/`: single, horizontal, vertical, four cardinal corners, damaged, and destroyed. The single sprite and structure states are bound. Six connected variants are validated inventory; no autotiling or navigation system was introduced. Wire remains walkable and retains its existing slowdown. GroundObjects sorting keeps it below enemies and tall defenses.

## 6. Mine assets created

Two 32 x 32 PNGs in `Assets/Game/Art/Defenses/LandMine/`: idle and armed, with a 24 x 24 silhouette. The presentation observes the existing armed flag. It remains small at gameplay scale and disappears through existing detonation/removal. A triggered frame and armed animation were omitted because there is no gameplay pre-explosion delay to display them. No explosion art was generated.

## 7. Assets regenerated and why

| Candidate | Correction and disposition |
|---|---|
| MG idle source v001 | Painted checkerboard was unsuitable alpha input. ImageGen v002 replaced the background with a flat chroma key while retaining the base design. Raw v001 retained, excluded from exports. |
| Mine idle source v001 | Same painted-checkerboard defect. ImageGen v002 supplies the selected clean-key source. Raw v001 retained. |
| Sniper reference v001 | Visible lower supports suggested oblique perspective. v002 hides them and puts ladder rungs inside the overhead deck hatch. v001 retained as superseded. |
| Defense lineup v001 | Final v002 updates both sniper depictions to the corrected deck. Original retained. |
| Existing sniper UI icon v001 | New v002 matches the corrected top-down deck. Existing file/GUID preserved. |
| Runtime export candidates | Registration/cropping was corrected to prevent neighboring sheet cells bleeding into frames; all final frames use stable pivots and shared action scale. The SW wire corner was fitted to preserve its two-pixel transparent margin. |

All final runtime PNGs have hard binary alpha, zero transparent RGB, no magenta residue, and a shared 48-color palette. Contact sheets and all seven firing frames were inspected on light/dark backgrounds. Native source images remain unmodified, with matching generator SHA-256 receipts.

## 8. Unity import configuration

Unity 6000.3.24f1: Sprite Single/Multiple, 51.2 world PPU (100 for the UI icon), Point, Uncompressed, no Crunch/mipmaps, Clamp, NPOT None, sRGB, alpha transparency, Full Rect, center pivot, Read/Write off, no generated physics shape, max size 512. The two sheets are sliced into exact 64/96-pixel cells with stable names/IDs. Native references and raw source sheets are non-runtime textures. Import evidence: `Docs/Validation/Phase3A3/unity-imports.json`.

## 9. Assets actually integrated

All four existing defense prefabs now display the generated art. Gameplay roots, prefab GUIDs, definitions, footprints, and gameplay components remain intact. Reused visual children have unit scale/white tint; replaced placeholder body renderers are disabled and cleared. Existing flash/tracer/explosion presentation remains for Phase 3B.

`DefenseSpritePresentation` observes health and arming state; damaged art starts at half health. Destroyed art is bound on death, but dead defenses still disappear immediately. There is no persistent wreck or death delay. `TurretPresentation` adds discrete recoil sprites driven by its existing Fired event and keeps its existing aiming/VFX behavior. No Animator was added. `DefensePreviewPresentation` is attached to the existing scene placement object and supplies actual-art ghosts while preserving placement tint and ownership.

Task-only baseline comparison covers 627 pre-existing Assets/Packages/ProjectSettings files: **621 unchanged, exactly six modified, none missing**. The six are the four defense prefabs, the primary scene, and TurretPresentation.cs. The scene diff only adds the presentation component and its references. Two new presentation scripts accompany the new art. Gameplay definitions/balance, existing tests, enemy art, UI bindings, packages, project settings, and the legacy scene are unchanged. Existing working-tree changes were not reset or staged.

## 10. Tests executed

| Validation | Result | Evidence |
|---|---|---|
| Unity compilation | Passed; no compile errors | `compile-final.json` |
| Existing EditMode suite | 42/42 passed; zero failed/skipped | `editmode-tests.json` |
| Existing PlayMode suite | 22/22 passed; zero failed/skipped | `playmode-tests.json` |
| Primary-scene live checks | 45/45 passed | `live-checks.json`, `Walkthrough.cs` |
| Runtime image audit | 24/24 passed dimensions, alpha, margins, chroma cleanup | `pixel-audit.json` |
| Native generation receipts | 29/29 byte-identical | `Docs/Art/Batches/p3a3_final_manifest.json` |
| Repository preservation | Only the six intended pre-existing files changed | `repository-integrity.json`, six `.diff` files |
| Diff whitespace | Passed | `git diff --check` |

Evidence filenames above are under `Docs/Validation/Phase3A3/` unless otherwise qualified. The suites cover economy/placement/occupancy/routes, combat, wire, mine, waves, and Game Over, including the legacy regression scene. No standalone build or hardware performance profiling was run. Runtime textures are small (largest 288 x 96), static defenses have no Animator, and new presentation updates use cached references with no per-frame allocations.

The first live recoil checks exposed a validation-fixture issue: this project's Player loop stops while unfocused with Run In Background false. The walkthrough temporarily enabled that runtime flag and bounded delta time so timed frames could be observed. The complete rerun passed; the earlier result is preserved as `live-checks-first.json`. Runtime settings were restored and Play Mode stopped; project settings are unchanged.

Console history includes the pre-existing Input Manager deprecation warning and an expected observer exception deliberately exercised by a passing EditMode test. Both are documented in `console-evidence.json`. Final Editor inspection reports zero current errors/warnings, no compilation failure, and an unmodified scene in Edit Mode.

## 11. Play Mode validation

Completed an agent-driven live Editor walkthrough in the primary scene, followed by visual inspection of rendered frames at **1920 x 1080** and **2560 x 1440**. This was operated through Unity's live API, not a human mouse/keyboard playtest. The scene and its real placement/combat/state components were running; temporary fixture state was discarded on leaving Play Mode.

- All four defense visuals load, with no visible replaced body/weapon shape placeholders.
- All four selections, valid green ghosts, invalid red ghosts, placements, grid alignment, root transforms, and original costs pass. Four placements charge the original total of 575.
- MG and sniper bases remain fixed while weapons aim, fire, play recoil, return to idle, and retain muzzle attachment. Combined controlled-shot damage remains the original 120.
- All three damaged bases display the expected registered state sprites.
- Wire remains walkable, slows enemies, and restores movement speed when they leave.
- Mine arms through the existing flag, triggers, releases occupancy, and is removed.
- Start Wave and remaining/alive counts work. Game Over stops building and waves; HUD capture confirms the state.
- All four designs were compared beside Phase 3A-2 BasicInfected art over grass, mud, wet shoreline, and bunker concrete. These review copies are visual fixtures only; the saved scene is unchanged.

Representative captures: `defenses-idle-1080.png`, `defenses-idle-1440.png`, `defenses-firing-1080.png`, `defenses-damaged-1080.png`, eight `ghost-*-valid/invalid.png` captures, `mine-armed-1080.png`, `wave-smoke-1080.png`, `game-over-hud.png`, `terrain-review-1080.png`, and `terrain-review-1440.png`. Final contact views: `runtime-contact.png`, `all-firing-frames.png`; optional GIF previews are stored alongside them.

## 12. Known visual issues

No blocking defense-art defects remain. The existing camera fits arbitrary viewport sizes, so sprite rotation and movement are not guaranteed to land on integer screen pixels. Both requested resolutions were checked; no camera/package change was made. The mine intentionally has subtle idle/armed differences and remains small. Connected wire is prepared art inventory, not a seamless implemented autotile system. Destroyed assets cannot linger under the current immediate destruction lifecycle.

## 13. Remaining placeholders

The original muzzle flashes, sniper tracer, and mine explosion remain temporary effects, as requested for Phase 3B. They can look oversized/bright beside the finished defense sprites. Existing gameplay enemy/base presentation and text-only UI remain at their prior integration stage; terrain is the Phase 3A-1 art. Existing MG/wire/mine icon assets are retained. No final audio, enemy redesign, new defense, combat mechanic, or world-effect pack was introduced.

## 14. Readiness for Phase 3B

**Phase 3B can begin.** Defense pivots, +X aiming convention, muzzle transforms, sprite states, sorting, and existing effect consumers are documented and validated. Phase 3B has not been started. It can replace the existing effects without changing damage, targeting, placement, or lifetime ownership.
