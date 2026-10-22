# Defense art specifications — Phase 3A-3

Completed 2026-09-21. Batch: `p3a3_defenses_001`. See [production report](PHASE_3A3_REPORT.md), [batch record](Docs/Art/Batches/p3a3_defenses_001.md), and [asset register](Docs/Art/ASSET_REGISTER.md).

## Source of truth and visual scale

Inspected the existing four DefenseDefinitions, defense prefabs, and primary `Assets/Game/Scenes/CoastalDefense_TopDown.unity` scene before production. The map is 32 x 18 cells on XY, with 1.25 world units per cell. **Every defense still occupies exactly 1 x 1 cell.** Build occupancy, navigation rules, costs, health, range, targeting, damage, mine triggering, wire slowdown, and wave configuration are unchanged.

World assets use **51.2 PPU**, so 64 pixels represent one cell. All integrated sprite children use unit scale and white tint. Canvas bounds are presentation space, not collision or build footprints.

| Defense | Per-frame canvas | Canvas world size | Intact base silhouette | Gameplay footprint |
|---|---|---|---|---|
| Machine Gun Nest | 64 x 64 | 1.25 x 1.25 | 51 x 54 px | 1 x 1 cell |
| Sniper Tower | 96 x 96 | 1.875 x 1.875 | 56 x 58 px | 1 x 1 cell |
| Barbed Wire | 64 x 64 | 1.25 x 1.25 | 58 x 26 px (single) | 1 x 1 cell |
| Land Mine | 32 x 32 | 0.625 x 0.625 | 24 x 24 px | 1 x 1 cell |

The sniper's extra transparent canvas accommodates its long aiming layer; its deck is only slightly larger than the MG emplacement. The mine remains much smaller than a cell. Connected wire variants are future art inventory; they do not alter the primary single-cell obstacle.

## Reference and art direction

Style anchor: `Assets/Game/Art/MasterReferences/bd_ref_danube_master_v002.png`, the previously approved Phase 3A-1 revision.

Final lineup: `Assets/Game/Art/MasterReferences/ref_defense_lineup_core_01.png` (1774 x 887, selected native lineup v002). It is a style board, not a gameplay sheet. Individual stills are `bd_ref_mg_reference_v001.png`, `bd_ref_sniper_reference_v002.png`, `bd_ref_wire_reference_v001.png`, and `bd_ref_mine_reference_v001.png` in that folder. The earlier sniper v001 is retained as superseded.

Strict perpendicular overhead, olive military paint, dark steel, sandbags, worn timber, rust, mud, and practical quarantine equipment. The sniper uses an overhead square deck with its ladder hatch inside the deck; no visible tower legs or front facade. Lighting follows the Art Handbook's upper-left light and subtle lower-right contact shading. Contact shadows are baked, opaque pixel clusters with transparent surroundings. There are no separate shadow renderers. Rotating weapon layers use restrained, nearly neutral shading so rotation does not create conspicuous moving cast shadows.

The runtime pack shares a 48-color palette, nearest-neighbor sampling, no dithering, and binary alpha. Transparent RGB is zero. Every frame has at least a two-pixel clear margin. Review sheets include both light and dark backgrounds.

## Complete runtime export inventory

All paths are relative to repository root. Pivot coordinates are measured from each frame's bottom-left in Unity. All pivots normalize to (0.5, 0.5). The UI icon has no gameplay footprint.

| Asset path | PNG size | Frames / FPS | PPU | Pivot (pixels) |
|---|---|---|---|---|
| `Assets/Game/Art/Defenses/BarbedWire/bd_defense_wire_corner_ne_v001.png` | 64x64 | 1 / - | 51.2 | (32, 32) |
| `Assets/Game/Art/Defenses/BarbedWire/bd_defense_wire_corner_nw_v001.png` | 64x64 | 1 / - | 51.2 | (32, 32) |
| `Assets/Game/Art/Defenses/BarbedWire/bd_defense_wire_corner_se_v001.png` | 64x64 | 1 / - | 51.2 | (32, 32) |
| `Assets/Game/Art/Defenses/BarbedWire/bd_defense_wire_corner_sw_v001.png` | 64x64 | 1 / - | 51.2 | (32, 32) |
| `Assets/Game/Art/Defenses/BarbedWire/bd_defense_wire_damaged_v001.png` | 64x64 | 1 / - | 51.2 | (32, 32) |
| `Assets/Game/Art/Defenses/BarbedWire/bd_defense_wire_destroyed_v001.png` | 64x64 | 1 / - | 51.2 | (32, 32) |
| `Assets/Game/Art/Defenses/BarbedWire/bd_defense_wire_horizontal_v001.png` | 64x64 | 1 / - | 51.2 | (32, 32) |
| `Assets/Game/Art/Defenses/BarbedWire/bd_defense_wire_single_v001.png` | 64x64 | 1 / - | 51.2 | (32, 32) |
| `Assets/Game/Art/Defenses/BarbedWire/bd_defense_wire_vertical_v001.png` | 64x64 | 1 / - | 51.2 | (32, 32) |
| `Assets/Game/Art/Defenses/LandMine/bd_defense_mine_armed_v001.png` | 32x32 | 1 / - | 51.2 | (16, 16) |
| `Assets/Game/Art/Defenses/LandMine/bd_defense_mine_idle_v001.png` | 32x32 | 1 / - | 51.2 | (16, 16) |
| `Assets/Game/Art/Defenses/MachineGunNest/bd_defense_mg_base_damaged_v001.png` | 64x64 | 1 / - | 51.2 | (32, 32) |
| `Assets/Game/Art/Defenses/MachineGunNest/bd_defense_mg_base_destroyed_v001.png` | 64x64 | 1 / - | 51.2 | (32, 32) |
| `Assets/Game/Art/Defenses/MachineGunNest/bd_defense_mg_base_idle_v001.png` | 64x64 | 1 / - | 51.2 | (32, 32) |
| `Assets/Game/Art/Defenses/MachineGunNest/bd_defense_mg_preview_v001.png` | 64x64 | 1 / - | 51.2 | (32, 32) |
| `Assets/Game/Art/Defenses/MachineGunNest/bd_defense_mg_weapon_fire_sheet_v001.png` | 256x64 | 4 / 20 | 51.2 | (32, 32) |
| `Assets/Game/Art/Defenses/MachineGunNest/bd_defense_mg_weapon_idle_v001.png` | 64x64 | 1 / - | 51.2 | (32, 32) |
| `Assets/Game/Art/Defenses/SniperTower/bd_defense_sniper_aim_fire_sheet_v001.png` | 288x96 | 3 / 12 | 51.2 | (48, 48) |
| `Assets/Game/Art/Defenses/SniperTower/bd_defense_sniper_aim_idle_v001.png` | 96x96 | 1 / - | 51.2 | (48, 48) |
| `Assets/Game/Art/Defenses/SniperTower/bd_defense_sniper_base_damaged_v001.png` | 96x96 | 1 / - | 51.2 | (48, 48) |
| `Assets/Game/Art/Defenses/SniperTower/bd_defense_sniper_base_destroyed_v001.png` | 96x96 | 1 / - | 51.2 | (48, 48) |
| `Assets/Game/Art/Defenses/SniperTower/bd_defense_sniper_base_idle_v001.png` | 96x96 | 1 / - | 51.2 | (48, 48) |
| `Assets/Game/Art/Defenses/SniperTower/bd_defense_sniper_preview_v001.png` | 96x96 | 1 / - | 51.2 | (48, 48) |
| `Assets/Game/Art/UI/bd_ui_sniper_tower_icon_v002.png` | 64x64 | 1 / - | 100 | (32, 32) |

## Pivots, muzzle attachments, and sheets

| Contract | MG | Sniper |
|---|---|---|
| Rotation owner | Existing `TurretPresentation` / `Rotating Weapon` | Existing `TurretPresentation` / `Rotating Weapon` |
| Authored aim | Local +X | Local +X |
| Mount pixel | (32, 32) | (48, 48) |
| Idle muzzle pixel | (58, 32) | (82, 48) |
| Muzzle local position | (0.5078125, 0, 0) | (0.6640625, 0, 0) |
| Base / aiming sprite local offset | (0, 0, 0) | (0, 0, 0) |
| Sheet layout | 4 columns x 1 row, 64 x 64 each | 3 columns x 1 row, 96 x 96 each |
| Sub-sprites | `bd_defense_mg_weapon_fire__f000` through `f003` | `bd_defense_sniper_aim_fire__f000` through `f002` |
| Playback | 20 FPS, 0.20 s, once per shot | 12 FPS, 0.25 s, once per shot |

Sheets are left-to-right, no padding/gutters between equal rectangular cells. Unity rect for frame i is (i * frameWidth, 0, frameWidth, frameHeight). Alpha margins are inside each cell. Scale and mount registration are shared across each action; only intentional recoil moves the barrel/receiver pixels. Idle is exported from the registered first frame. The muzzle transform is the fixed nominal muzzle/VFX attachment; recoil does not move gameplay or the transform.

The existing `Fired` notification starts discrete sprite playback in `TurretPresentation`. It returns to idle after the last frame and resets on enable/disable. No animation events, root motion, Animator, clips, or controllers were added. No muzzle flash or tracer is baked into these sheets.

## State definitions and consumers

| State | Visual rule | Binding |
|---|---|---|
| Intact MG/sniper/wire | Health above half maximum | Base idle / wire single |
| Damaged MG/sniper/wire | CurrentHealth <= MaxHealth / 2 while alive | Matching damaged base, same pivot and registration |
| Destroyed MG/sniper/wire | Existing Died notification | Matching destroyed sprite, immediately before existing destruction |
| Mine idle | Existing mine has not armed | Mine idle |
| Mine armed | Existing LandMine.Armed is true | Mine armed with subtle mechanical/indicator difference |
| Mine consumed | Existing detonation/removal | Existing object removal; no persistent wreck |
| Placement ghost | Existing placement selection and validity | MG/sniper composite preview; wire single; mine idle |

`DefenseSpritePresentation` only observes existing state. It does not extend lifetime. Destroyed exports are available and bound, but the game removes dead defenses immediately; a persistent wreck would require later presentation work. A triggered mine frame and looping armed animation were omitted because the existing detonation has no pre-explosion delay and a static armed indication is sufficient.

Connected wire exports: horizontal, vertical, NE/NW/SE/SW bends. Cardinal names indicate the directions extending from the bend. Their logical junction is (32, 32), and connectors finish near pixel 2/62 to preserve alpha margins. A future connected layout can overlap visual ends by approximately four pixels; these are not asserted to be seamless edge-to-edge terrain tiles. No autotiling system is implemented.

## Prefab and sorting contract

Existing prefab paths: `Assets/Game/Prefabs/Defenses/{MachineGunNest,SniperTower,BarbedWire,LandMine}.prefab`. Their GUIDs, roots, gameplay components, and definitions are preserved.

- MG/sniper: reuse `Sandbag Base` and `Rotating Weapon/Receiver` renderers. Base is fixed on Defenses order 0; aiming layer is Defenses order 2. Old Inset/Barrel shape renderers are disabled and cleared. Existing `Rotating Weapon/Muzzle`, flash, and reusable tracer remain.
- Wire: reuse the first `Wire strand` renderer at local origin, GroundObjects order 1. Extra placeholder strands/posts are disabled and cleared.
- Mine: reuse `Mine body`, unrotated and unit scale, GroundObjects order 2. Placeholder Trigger art is disabled and cleared. Existing explosion presentation remains.
- Primary scene: `DefensePreviewPresentation` is added to the existing Defense Placement object. It changes only the existing ghost sprite and visual scale after placement state updates. `PlacementController` retains position, visibility, green/red tint, validity, and placement ownership.

Layer order remains Environment, GroundObjects, Enemies, Defenses, Effects. Wire/mine sit above terrain and below enemies/tall defenses. Weapon layers remain above their fixed bases.

## Unity imports, UI, and reproducibility

Unity 6000.3.24f1. Runtime imports are Sprite, Single or Multiple as listed, Point, Uncompressed, Crunch off, mipmaps off, Clamp, NPOT None, sRGB, alpha transparency enabled, Full Rect mesh, center pivot, Read/Write off, no generated physics shape, max texture size 512. Batch-specific platform overrides are cleared. Native references/raw sources remain ordinary non-runtime textures with max size 4096. Unrelated import settings are untouched.

The prior MG/wire/mine build icons remain appropriate and unchanged. Only the sniper icon has a new v002, matching the corrected deck. Previous v001 remains available. Icons are inventory: the existing UI Toolkit build controls remain live-text controls without new icon bindings or layout changes.

Generation receipts, exact prompts, native image dimensions/hashes, and export relationships are in `Docs/Art/Batches/p3a3_final_manifest.json`. `Docs/Art/export_phase3a3.py` performs the authorized normalization/background removal/palette reduction/packing. Source-space pivots, scale, frame crops, and hashes are in `p3a3_export_manifest.json`. `Docs/Art/audit_phase3a3.py` verifies alpha/margins and creates the register and task-only diffs. The raw 29 PNGs are byte-identical copies of the image generator outputs.

Validation evidence: `Docs/Validation/Phase3A3/`. See the report for test counts, live scene checks, visual captures, and limits.

