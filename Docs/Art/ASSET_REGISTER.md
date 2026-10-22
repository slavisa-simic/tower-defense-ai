# Art asset register

## Phase 3C.1 additions — 2026-10-01

Authorized by the attached strict-top-down cleanup/world-polish/boat-landings request. Reuses approved Danube master v002 and recorded exact-pixel export authorization. Built-in ImageGen source receipts/prompts/rejected reed v001 and selected reed v002 are recorded in `Batches/p3c1_polish_001.md`; technical selection/integration is not a new human approval claim.

| Runtime path | Source / dimensions | Consumer / status |
|---|---|---|
| `Assets/Game/Art/Environment/Vegetation/bd_env_reeds_v001.png` | `SourceSheets/p3c1_polish_001/bd_src_reeds_v002.png`; 48x48 | Saved riverbank dressing; integrated/reviewed |
| `Assets/Game/Art/Environment/Vegetation/bd_env_shrub_v001.png` | `SourceSheets/p3c1_polish_001/bd_src_shrub_v001.png`; 48x48 | Sparse overhead shrubs; integrated/reviewed |
| `Assets/Game/Art/Environment/Vegetation/bd_env_grass_tuft_v001.png` | `SourceSheets/p3c1_polish_001/bd_src_grass_v001.png`; 32x32 | Sparse field tufts; integrated/reviewed |
| `Assets/Game/Art/Defenses/BarbedWire/bd_defense_wire_mask_00_v002.png` through `bd_defense_wire_mask_15_v002.png` (16 assets) | Generated cross v001 canonical half-arm/center; each 64x64 | BarbedWire prefab and ghost; integrated/reviewed; replaces disconnected v001 binding, retains original assets |
| `Assets/Game/Art/Effects/ContactShadow/bd_fx_contact_shadow_v001.png` | Code-native compact pixel-cluster support ellipse; 64x64, alpha 0/100/180 | Shared world/enemy/defense/boat shadows; integrated/reviewed |
| `Assets/Game/Prefabs/Presentation/RubberLandingBoat.prefab` | Reuses intact rubber boat v001, Basic Infected idle v001 visual seats, shared contact shadow | One scene-owned BoatLandingSystem; integrated/reviewed |

Runtime imports: 51.2 PPU, center, Point, uncompressed, no mipmaps, Clamp, Full Rect, no colliders. Exact native source sizes/hashes and all 20 runtime alpha/import checks: `../Validation/Phase3C1/asset-audit.json`. Export and mask inventories: `Batches/p3c1_exports.json`. Five raw sources remain non-runtime textures; rejected upright reeds v001 is archived without consumers. Existing art PNGs and prefab GUIDs are preserved. Final gameplay/tests/screenshots/limits: `../../PHASE_3C1_REPORT.md`.

Phase 3A-1 completed 2026-09-19. 52 selected runtime sprites: 10 environment, 17 props, 10 base/fortification and 15 UI. All are exported, visually reviewed and imported into Unity. Technical acceptance is recorded here; only the master has separate user selection evidence.

Approved master: `Assets/Game/Art/MasterReferences/bd_ref_danube_master_v002.png`, SHA-256 `354421E55C341BFBCC65993BA20C644842EEC44073CD3BBB7EB3B7794F7D7E45`. User replied **"continue"** after the displayed revision and explicit selection question. The user also authorized deterministic exports: **"Yes—prepare exact game-ready pixel exports"** in response to the resize/crop, palette and seam-cleanup question. No further reference approval is pending.

Canonical reproduction input: `Batches/p3a1_final_manifest.json` (exact prompts, local references, source/output paths, logical dimensions, alpha, repeat axes, palette and panel borders). The selected output evidence is `../Validation/Phase3A1/p3a1_final_manifest-exports.json` and `p3a1_final_manifest-imports.json`; these supersede intermediate export/import evidence. Exact original returned paths, native dimensions and SHA-256 values for every source are in `Batches/p3a1_all_native_receipts.json`. Native source files are byte-identical to generator outputs. Built-in ImageGen was used; no seed or model identifier is asserted.

World defaults: 51.2 PPU, Sprite Single, center pivot, Full Rect, Point, uncompressed, no mipmaps, NPOT None, sRGB, read/write off, no fallback physics shape. UI uses 100 PPU and explicit logical sizing. Source/reference textures are Default, Point, uncompressed, no mipmaps, Clamp, max 4096. Runtime maximum is 4096; actual sizes are verified unchanged by import. All isolated sprites have binary alpha and at least two transparent pixels of margin. Terrain is opaque and repeats only on the specified axes.

## Selected runtime inventory

All rows use approved master v002 and technical acceptance on 2026-09-19. The source column identifies the native generation, not a runtime sheet. No animations or sliced subframes exist in this phase.

| Runtime path | Native source | Px | PPU / repeat | Integration target / current consumer |
|---|---|---|---|---|
| `Assets/Game/Art/Environment/bd_env_deep_danube_water_tile_v001.png` | `Assets/Game/Art/SourceSheets/p3a1_environment_001/bd_src_deep_danube_water_tile_v001.png` | 64x64 | 51.2 / xy | Primary scene / Ocean |
| `Assets/Game/Art/Environment/bd_env_shallow_water_tile_v001.png` | `Assets/Game/Art/SourceSheets/p3a1_environment_001/bd_src_shallow_water_tile_v001.png` | 64x64 | 51.2 / xy | Primary scene / Shallow Water |
| `Assets/Game/Art/Environment/bd_env_muddy_shore_tile_v001.png` | `Assets/Game/Art/SourceSheets/p3a1_environment_001/bd_src_muddy_shore_tile_v001.png` | 64x64 | 51.2 / xy | Primary scene / Beach |
| `Assets/Game/Art/Environment/bd_env_wet_bank_transition_tile_v001.png` | `Assets/Game/Art/SourceSheets/p3a1_environment_001/bd_src_wet_bank_transition_tile_v001.png` | 64x64 | 51.2 / y | Primary scene / Shoreline |
| `Assets/Game/Art/Environment/bd_env_grass_tile_v001.png` | `Assets/Game/Art/SourceSheets/p3a1_environment_001/bd_src_grass_tile_v001.png` | 64x64 | 51.2 / xy | Primary scene / Grass Combat Field |
| `Assets/Game/Art/Environment/bd_env_dirt_tile_v001.png` | `Assets/Game/Art/SourceSheets/p3a1_environment_001/bd_src_dirt_tile_v001.png` | 64x64 | 51.2 / xy | Imported visual inventory; no scene/prefab consumer |
| `Assets/Game/Art/Environment/bd_env_cracked_ground_tile_v001.png` | `Assets/Game/Art/SourceSheets/p3a1_environment_001/bd_src_cracked_ground_tile_v001.png` | 64x64 | 51.2 / xy | Imported visual inventory; no scene/prefab consumer |
| `Assets/Game/Art/Environment/bd_env_bunker_floor_tile_v001.png` | `Assets/Game/Art/SourceSheets/p3a1_environment_001/bd_src_bunker_floor_tile_v001.png` | 64x64 | 51.2 / xy | Imported visual inventory; no scene/prefab consumer |
| `Assets/Game/Art/Environment/bd_env_concrete_tile_v001.png` | `Assets/Game/Art/SourceSheets/p3a1_environment_001/bd_src_concrete_tile_v001.png` | 64x64 | 51.2 / xy | Imported visual inventory; no scene/prefab consumer |
| `Assets/Game/Art/Environment/bd_env_road_tile_v001.png` | `Assets/Game/Art/SourceSheets/p3a1_environment_001/bd_src_road_tile_v001.png` | 64x64 | 51.2 / x | Imported visual inventory; no scene/prefab consumer |
| `Assets/Game/Art/Props/bd_prop_rubber_boat_v001.png` | `Assets/Game/Art/SourceSheets/p3a1_props_001/bd_src_rubber_boat_v001.png` | 192x96 | 51.2 / Clamp | Imported visual inventory; no scene/prefab consumer |
| `Assets/Game/Art/Props/bd_prop_destroyed_rubber_boat_v001.png` | `Assets/Game/Art/SourceSheets/p3a1_props_001/bd_src_destroyed_rubber_boat_v001.png` | 192x96 | 51.2 / Clamp | Imported visual inventory; no scene/prefab consumer |
| `Assets/Game/Art/Props/bd_prop_sandbag_segment_v001.png` | `Assets/Game/Art/SourceSheets/p3a1_props_001/bd_src_sandbag_segment_v001.png` | 64x32 | 51.2 / Clamp | Imported visual inventory; no scene/prefab consumer |
| `Assets/Game/Art/Props/bd_prop_wooden_crate_v001.png` | `Assets/Game/Art/SourceSheets/p3a1_props_001/bd_src_wooden_crate_v001.png` | 64x64 | 51.2 / Clamp | Imported visual inventory; no scene/prefab consumer |
| `Assets/Game/Art/Props/bd_prop_metal_crate_v001.png` | `Assets/Game/Art/SourceSheets/p3a1_props_001/bd_src_metal_crate_v001.png` | 64x64 | 51.2 / Clamp | Imported visual inventory; no scene/prefab consumer |
| `Assets/Game/Art/Props/bd_prop_oil_barrel_v001.png` | `Assets/Game/Art/SourceSheets/p3a1_props_001/bd_src_oil_barrel_v001.png` | 32x32 | 51.2 / Clamp | Imported visual inventory; no scene/prefab consumer |
| `Assets/Game/Art/Props/bd_prop_military_barrel_v001.png` | `Assets/Game/Art/SourceSheets/p3a1_props_001/bd_src_military_barrel_v001.png` | 32x32 | 51.2 / Clamp | Imported visual inventory; no scene/prefab consumer |
| `Assets/Game/Art/Props/bd_prop_spotlight_v001.png` | `Assets/Game/Art/SourceSheets/p3a1_props_001/bd_src_spotlight_v003.png` | 64x64 | 51.2 / Clamp | Imported visual inventory; no scene/prefab consumer |
| `Assets/Game/Art/Props/bd_prop_antenna_v001.png` | `Assets/Game/Art/SourceSheets/p3a1_props_001/bd_src_antenna_v001.png` | 64x64 | 51.2 / Clamp | Imported visual inventory; no scene/prefab consumer |
| `Assets/Game/Art/Props/bd_prop_radar_device_v001.png` | `Assets/Game/Art/SourceSheets/p3a1_props_001/bd_src_radar_device_v001.png` | 96x96 | 51.2 / Clamp | Imported visual inventory; no scene/prefab consumer |
| `Assets/Game/Art/Props/bd_prop_czech_hedgehog_obstacle_v001.png` | `Assets/Game/Art/SourceSheets/p3a1_props_001/bd_src_czech_hedgehog_obstacle_v001.png` | 64x64 | 51.2 / Clamp | Imported visual inventory; no scene/prefab consumer |
| `Assets/Game/Art/Props/bd_prop_barbed_barrier_segment_visual_v001.png` | `Assets/Game/Art/SourceSheets/p3a1_props_001/bd_src_barbed_barrier_segment_visual_v001.png` | 64x32 | 51.2 / Clamp | Imported visual inventory; no scene/prefab consumer |
| `Assets/Game/Art/Props/bd_prop_debris_scrap_v001.png` | `Assets/Game/Art/SourceSheets/p3a1_props_001/bd_src_debris_scrap_v001.png` | 64x64 | 51.2 / Clamp | Imported visual inventory; no scene/prefab consumer |
| `Assets/Game/Art/Props/bd_prop_wrecked_supply_box_v001.png` | `Assets/Game/Art/SourceSheets/p3a1_props_001/bd_src_wrecked_supply_box_v001.png` | 64x64 | 51.2 / Clamp | Imported visual inventory; no scene/prefab consumer |
| `Assets/Game/Art/Props/bd_prop_floodlight_v001.png` | `Assets/Game/Art/SourceSheets/p3a1_props_001/bd_src_floodlight_v001.png` | 64x64 | 51.2 / Clamp | Imported visual inventory; no scene/prefab consumer |
| `Assets/Game/Art/Props/bd_prop_warning_sign_v001.png` | `Assets/Game/Art/SourceSheets/p3a1_props_001/bd_src_warning_sign_v001.png` | 64x64 | 51.2 / Clamp | Imported visual inventory; no scene/prefab consumer |
| `Assets/Game/Art/Props/bd_prop_quarantine_fence_segment_v001.png` | `Assets/Game/Art/SourceSheets/p3a1_props_001/bd_src_quarantine_fence_segment_v001.png` | 64x32 | 51.2 / Clamp | Imported visual inventory; no scene/prefab consumer |
| `Assets/Game/Art/Base/bd_base_bunker_corner_v001.png` | `Assets/Game/Art/SourceSheets/p3a1_base_001/bd_src_bunker_corner_v001.png` | 64x64 | 51.2 / Clamp | Imported visual inventory; no scene/prefab consumer |
| `Assets/Game/Art/Base/bd_base_bunker_wall_segment_v001.png` | `Assets/Game/Art/SourceSheets/p3a1_base_001/bd_src_bunker_wall_segment_v001.png` | 64x32 | 51.2 / Clamp | Imported visual inventory; no scene/prefab consumer |
| `Assets/Game/Art/Base/bd_base_bunker_gate_piece_v001.png` | `Assets/Game/Art/SourceSheets/p3a1_base_001/bd_src_bunker_gate_piece_v001.png` | 128x64 | 51.2 / Clamp | Imported visual inventory; no scene/prefab consumer |
| `Assets/Game/Art/Base/bd_base_watch_post_v001.png` | `Assets/Game/Art/SourceSheets/p3a1_base_001/bd_src_watch_post_v001.png` | 96x96 | 51.2 / Clamp | Imported visual inventory; no scene/prefab consumer |
| `Assets/Game/Art/Base/bd_base_command_post_object_v001.png` | `Assets/Game/Art/SourceSheets/p3a1_base_001/bd_src_command_post_object_v001.png` | 192x192 | 51.2 / Clamp | Imported visual inventory; no scene/prefab consumer |
| `Assets/Game/Art/Base/bd_base_military_tent_topdown_v001.png` | `Assets/Game/Art/SourceSheets/p3a1_base_001/bd_src_military_tent_topdown_v001.png` | 128x128 | 51.2 / Clamp | Imported visual inventory; no scene/prefab consumer |
| `Assets/Game/Art/Base/bd_base_generator_unit_v001.png` | `Assets/Game/Art/SourceSheets/p3a1_base_001/bd_src_generator_unit_v001.png` | 96x64 | 51.2 / Clamp | Imported visual inventory; no scene/prefab consumer |
| `Assets/Game/Art/Base/bd_base_ammo_supply_crate_cluster_v001.png` | `Assets/Game/Art/SourceSheets/p3a1_base_001/bd_src_ammo_supply_crate_cluster_v001.png` | 96x64 | 51.2 / Clamp | Imported visual inventory; no scene/prefab consumer |
| `Assets/Game/Art/Base/bd_base_defensive_sandbag_nest_base_v001.png` | `Assets/Game/Art/SourceSheets/p3a1_base_001/bd_src_defensive_sandbag_nest_base_v001.png` | 64x64 | 51.2 / Clamp | Imported visual inventory; no scene/prefab consumer |
| `Assets/Game/Art/Base/bd_base_river_checkpoint_marker_v001.png` | `Assets/Game/Art/SourceSheets/p3a1_base_001/bd_src_river_checkpoint_marker_v001.png` | 64x64 | 51.2 / Clamp | Imported visual inventory; no scene/prefab consumer |
| `Assets/Game/Art/UI/bd_ui_hud_panel_small_v001.png` | `Assets/Game/Art/SourceSheets/p3a1_ui_001/bd_src_hud_panel_small_v001.png` | 128x64 | 100 / Clamp | Future TopDownHud presentation; no binding yet |
| `Assets/Game/Art/UI/bd_ui_hud_panel_medium_v001.png` | `Assets/Game/Art/SourceSheets/p3a1_ui_001/bd_src_hud_panel_medium_v001.png` | 256x96 | 100 / Clamp | Future TopDownHud presentation; no binding yet |
| `Assets/Game/Art/UI/bd_ui_hud_panel_large_v001.png` | `Assets/Game/Art/SourceSheets/p3a1_ui_001/bd_src_hud_panel_large_v001.png` | 384x128 | 100 / Clamp | Future TopDownHud presentation; no binding yet |
| `Assets/Game/Art/UI/bd_ui_build_button_empty_v001.png` | `Assets/Game/Art/SourceSheets/p3a1_ui_001/bd_src_build_button_empty_v001.png` | 64x64 | 100 / Clamp | Future TopDownHud presentation; no binding yet |
| `Assets/Game/Art/UI/bd_ui_start_wave_button_v001.png` | `Assets/Game/Art/SourceSheets/p3a1_ui_001/bd_src_start_wave_button_v001.png` | 192x64 | 100 / Clamp | Future TopDownHud presentation; no binding yet |
| `Assets/Game/Art/UI/bd_ui_small_status_panel_v001.png` | `Assets/Game/Art/SourceSheets/p3a1_ui_001/bd_src_small_status_panel_v001.png` | 128x48 | 100 / Clamp | Future TopDownHud presentation; no binding yet |
| `Assets/Game/Art/UI/bd_ui_money_icon_v001.png` | `Assets/Game/Art/SourceSheets/p3a1_ui_001/bd_src_money_icon_v002.png` | 32x32 | 100 / Clamp | Future TopDownHud presentation; no binding yet |
| `Assets/Game/Art/UI/bd_ui_base_hp_icon_v001.png` | `Assets/Game/Art/SourceSheets/p3a1_ui_001/bd_src_base_hp_icon_v001.png` | 32x32 | 100 / Clamp | Future TopDownHud presentation; no binding yet |
| `Assets/Game/Art/UI/bd_ui_wave_icon_v001.png` | `Assets/Game/Art/SourceSheets/p3a1_ui_001/bd_src_wave_icon_v001.png` | 32x32 | 100 / Clamp | Future TopDownHud presentation; no binding yet |
| `Assets/Game/Art/UI/bd_ui_mg_nest_icon_v001.png` | `Assets/Game/Art/SourceSheets/p3a1_ui_001/bd_src_mg_nest_icon_v001.png` | 64x64 | 100 / Clamp | Future TopDownHud presentation; no binding yet |
| `Assets/Game/Art/UI/bd_ui_barbed_wire_icon_v001.png` | `Assets/Game/Art/SourceSheets/p3a1_ui_001/bd_src_barbed_wire_icon_v001.png` | 64x64 | 100 / Clamp | Future TopDownHud presentation; no binding yet |
| `Assets/Game/Art/UI/bd_ui_land_mine_icon_v001.png` | `Assets/Game/Art/SourceSheets/p3a1_ui_001/bd_src_land_mine_icon_v001.png` | 64x64 | 100 / Clamp | Future TopDownHud presentation; no binding yet |
| `Assets/Game/Art/UI/bd_ui_sniper_tower_icon_v001.png` | `Assets/Game/Art/SourceSheets/p3a1_ui_001/bd_src_sniper_tower_icon_v001.png` | 64x64 | 100 / Clamp | Future TopDownHud presentation; no binding yet |
| `Assets/Game/Art/UI/bd_ui_build_button_selected_v001.png` | `Assets/Game/Art/SourceSheets/p3a1_ui_001/bd_src_build_button_selected_v001.png` | 64x64 | 100 / Clamp | Future TopDownHud presentation; no binding yet |
| `Assets/Game/Art/UI/bd_ui_build_button_disabled_v001.png` | `Assets/Game/Art/SourceSheets/p3a1_ui_001/bd_src_build_button_disabled_v001.png` | 64x64 | 100 / Clamp | Future TopDownHud presentation; no binding yet |

## Native source inventory

Each retained source is an ordinary non-runtime texture. This table plus the JSON receipts records every source file, including rejected revisions.

| Path | Native px / mode | Status |
|---|---|---|
| `Assets/Game/Art/SourceSheets/p3a1_base_001/bd_src_ammo_supply_crate_cluster_v001.png` | 1536x1024 / RGB | Selected source for one runtime export |
| `Assets/Game/Art/SourceSheets/p3a1_base_001/bd_src_bunker_corner_v001.png` | 1254x1254 / RGB | Selected source for one runtime export |
| `Assets/Game/Art/SourceSheets/p3a1_base_001/bd_src_bunker_gate_piece_v001.png` | 1774x887 / RGB | Selected source for one runtime export |
| `Assets/Game/Art/SourceSheets/p3a1_base_001/bd_src_bunker_wall_segment_v001.png` | 1774x887 / RGB | Selected source for one runtime export |
| `Assets/Game/Art/SourceSheets/p3a1_base_001/bd_src_command_post_object_v001.png` | 1254x1254 / RGB | Selected source for one runtime export |
| `Assets/Game/Art/SourceSheets/p3a1_base_001/bd_src_defensive_sandbag_nest_base_v001.png` | 1254x1254 / RGB | Selected source for one runtime export |
| `Assets/Game/Art/SourceSheets/p3a1_base_001/bd_src_generator_unit_v001.png` | 1536x1024 / RGB | Selected source for one runtime export |
| `Assets/Game/Art/SourceSheets/p3a1_base_001/bd_src_military_tent_topdown_v001.png` | 1254x1254 / RGB | Selected source for one runtime export |
| `Assets/Game/Art/SourceSheets/p3a1_base_001/bd_src_river_checkpoint_marker_v001.png` | 1254x1254 / RGB | Selected source for one runtime export |
| `Assets/Game/Art/SourceSheets/p3a1_base_001/bd_src_watch_post_v001.png` | 1254x1254 / RGB | Selected source for one runtime export |
| `Assets/Game/Art/SourceSheets/p3a1_environment_001/bd_src_bunker_floor_tile_v001.png` | 1254x1254 / RGB | Selected source for one runtime export |
| `Assets/Game/Art/SourceSheets/p3a1_environment_001/bd_src_concrete_tile_v001.png` | 1254x1254 / RGB | Selected source for one runtime export |
| `Assets/Game/Art/SourceSheets/p3a1_environment_001/bd_src_cracked_ground_tile_v001.png` | 1254x1254 / RGB | Selected source for one runtime export |
| `Assets/Game/Art/SourceSheets/p3a1_environment_001/bd_src_deep_danube_water_tile_v001.png` | 1254x1254 / RGB | Selected source for one runtime export |
| `Assets/Game/Art/SourceSheets/p3a1_environment_001/bd_src_dirt_tile_v001.png` | 1254x1254 / RGB | Selected source for one runtime export |
| `Assets/Game/Art/SourceSheets/p3a1_environment_001/bd_src_grass_tile_v001.png` | 1254x1254 / RGB | Selected source for one runtime export |
| `Assets/Game/Art/SourceSheets/p3a1_environment_001/bd_src_muddy_shore_tile_v001.png` | 1254x1254 / RGB | Selected source for one runtime export |
| `Assets/Game/Art/SourceSheets/p3a1_environment_001/bd_src_road_tile_v001.png` | 1254x1254 / RGB | Selected source for one runtime export |
| `Assets/Game/Art/SourceSheets/p3a1_environment_001/bd_src_shallow_water_tile_v001.png` | 1254x1254 / RGB | Selected source for one runtime export |
| `Assets/Game/Art/SourceSheets/p3a1_environment_001/bd_src_wet_bank_transition_tile_v001.png` | 1254x1254 / RGB | Selected source for one runtime export |
| `Assets/Game/Art/SourceSheets/p3a1_master_001/bd_src_danube_master_v001.png` | 1536x1024 / RGB | Archived, not a runtime consumer |
| `Assets/Game/Art/SourceSheets/p3a1_master_001/bd_src_danube_master_v002.png` | 1536x1024 / RGB | Approved master source |
| `Assets/Game/Art/SourceSheets/p3a1_props_001/bd_src_antenna_v001.png` | 1254x1254 / RGB | Selected source for one runtime export |
| `Assets/Game/Art/SourceSheets/p3a1_props_001/bd_src_barbed_barrier_segment_visual_v001.png` | 1774x887 / RGB | Selected source for one runtime export |
| `Assets/Game/Art/SourceSheets/p3a1_props_001/bd_src_czech_hedgehog_obstacle_v001.png` | 1254x1254 / RGB | Selected source for one runtime export |
| `Assets/Game/Art/SourceSheets/p3a1_props_001/bd_src_debris_scrap_v001.png` | 1254x1254 / RGB | Selected source for one runtime export |
| `Assets/Game/Art/SourceSheets/p3a1_props_001/bd_src_destroyed_rubber_boat_v001.png` | 1774x887 / RGB | Selected source for one runtime export |
| `Assets/Game/Art/SourceSheets/p3a1_props_001/bd_src_floodlight_v001.png` | 1254x1254 / RGB | Selected source for one runtime export |
| `Assets/Game/Art/SourceSheets/p3a1_props_001/bd_src_metal_crate_v001.png` | 1254x1254 / RGB | Selected source for one runtime export |
| `Assets/Game/Art/SourceSheets/p3a1_props_001/bd_src_military_barrel_v001.png` | 1254x1254 / RGB | Selected source for one runtime export |
| `Assets/Game/Art/SourceSheets/p3a1_props_001/bd_src_oil_barrel_v001.png` | 1254x1254 / RGB | Selected source for one runtime export |
| `Assets/Game/Art/SourceSheets/p3a1_props_001/bd_src_quarantine_fence_segment_v001.png` | 1774x887 / RGB | Selected source for one runtime export |
| `Assets/Game/Art/SourceSheets/p3a1_props_001/bd_src_radar_device_v001.png` | 1254x1254 / RGB | Selected source for one runtime export |
| `Assets/Game/Art/SourceSheets/p3a1_props_001/bd_src_rubber_boat_v001.png` | 1774x887 / RGB | Selected source for one runtime export |
| `Assets/Game/Art/SourceSheets/p3a1_props_001/bd_src_sandbag_segment_v001.png` | 1774x887 / RGB | Selected source for one runtime export |
| `Assets/Game/Art/SourceSheets/p3a1_props_001/bd_src_spotlight_v001.png` | 1254x1254 / RGB | Archived, not a runtime consumer |
| `Assets/Game/Art/SourceSheets/p3a1_props_001/bd_src_spotlight_v002.png` | 1254x1254 / RGB | Archived, not a runtime consumer |
| `Assets/Game/Art/SourceSheets/p3a1_props_001/bd_src_spotlight_v003.png` | 1254x1254 / RGB | Selected source for one runtime export |
| `Assets/Game/Art/SourceSheets/p3a1_props_001/bd_src_warning_sign_v001.png` | 1254x1254 / RGB | Selected source for one runtime export |
| `Assets/Game/Art/SourceSheets/p3a1_props_001/bd_src_wooden_crate_v001.png` | 1254x1254 / RGB | Selected source for one runtime export |
| `Assets/Game/Art/SourceSheets/p3a1_props_001/bd_src_wrecked_supply_box_v001.png` | 1254x1254 / RGB | Selected source for one runtime export |
| `Assets/Game/Art/SourceSheets/p3a1_ui_001/bd_src_barbed_wire_icon_v001.png` | 1254x1254 / RGB | Selected source for one runtime export |
| `Assets/Game/Art/SourceSheets/p3a1_ui_001/bd_src_base_hp_icon_v001.png` | 1254x1254 / RGB | Selected source for one runtime export |
| `Assets/Game/Art/SourceSheets/p3a1_ui_001/bd_src_build_button_disabled_v001.png` | 1254x1254 / RGB | Selected source for one runtime export |
| `Assets/Game/Art/SourceSheets/p3a1_ui_001/bd_src_build_button_empty_v001.png` | 1254x1254 / RGB | Selected source for one runtime export |
| `Assets/Game/Art/SourceSheets/p3a1_ui_001/bd_src_build_button_selected_v001.png` | 1254x1254 / RGB | Selected source for one runtime export |
| `Assets/Game/Art/SourceSheets/p3a1_ui_001/bd_src_hud_panel_large_v001.png` | 2172x724 / RGB | Selected source for one runtime export |
| `Assets/Game/Art/SourceSheets/p3a1_ui_001/bd_src_hud_panel_medium_v001.png` | 2048x768 / RGB | Selected source for one runtime export |
| `Assets/Game/Art/SourceSheets/p3a1_ui_001/bd_src_hud_panel_small_v001.png` | 1774x887 / RGB | Selected source for one runtime export |
| `Assets/Game/Art/SourceSheets/p3a1_ui_001/bd_src_land_mine_icon_v001.png` | 1254x1254 / RGB | Selected source for one runtime export |
| `Assets/Game/Art/SourceSheets/p3a1_ui_001/bd_src_mg_nest_icon_v001.png` | 1254x1254 / RGB | Selected source for one runtime export |
| `Assets/Game/Art/SourceSheets/p3a1_ui_001/bd_src_money_icon_v001.png` | 1254x1254 / RGB | Archived, not a runtime consumer |
| `Assets/Game/Art/SourceSheets/p3a1_ui_001/bd_src_money_icon_v002.png` | 1254x1254 / RGB | Selected source for one runtime export |
| `Assets/Game/Art/SourceSheets/p3a1_ui_001/bd_src_small_status_panel_v001.png` | 2048x768 / RGB | Selected source for one runtime export |
| `Assets/Game/Art/SourceSheets/p3a1_ui_001/bd_src_sniper_tower_icon_v001.png` | 1254x1254 / RGB | Selected source for one runtime export |
| `Assets/Game/Art/SourceSheets/p3a1_ui_001/bd_src_start_wave_button_v001.png` | 2172x724 / RGB | Selected source for one runtime export |
| `Assets/Game/Art/SourceSheets/p3a1_ui_001/bd_src_wave_icon_v001.png` | 1254x1254 / RGB | Selected source for one runtime export |

## Reference files and rejected revisions

| Path | Status / reason | Consumer |
|---|---|---|
| `Assets/Game/Art/MasterReferences/bd_ref_danube_master_v001.png` | Superseded: facade and noise defects; never approved | None |
| `Assets/Game/Art/MasterReferences/bd_ref_danube_master_v002.png` | User-selected style anchor; native 1536x1024 RGB | Later bounded art batches |

Spotlight source v001 had a painted checkerboard that contaminated its crop; v002 was rejected for a side-view silhouette; v003 supplies the accepted overhead runtime export. Money source v001 read as supply tags; v002 uses clear round coins. Rejected native files remain in SourceSheets only for provenance. Runtime filenames remain v001 because these were corrections before initial delivery, not replacements of approved production assets. Master v001 is also retained as explicitly superseded provenance.

## Validation and limits

52/52 actual dimensions and alpha contracts pass; declared tile seams match at the outer edges. Three build-button alpha masks match exactly; selected brackets and disabled dashed rim differ beyond hue. All 104 selected source/runtime imports were checked. Panel borders are 16 pixels on all sides, with 20-pixel content padding (16 for build buttons); UI background slicing must be explicitly applied by a later UI Toolkit integration. Functional text, prices, state and callbacks remain owned by existing UI/gameplay code.

Native and integer-scale light/dark previews: `Docs/Validation/Phase3A1/<category>-review-*.png`; terrain repeat preview and camera captures are in the same folder. 42 EditMode and 22 PlayMode tests passed. Saved scene diff changes only five terrain renderers and their visual scales. No existing asset GUID, gameplay script, prefab, data, package or project setting was changed. See `PHASE_3A1_REPORT.md` for the scope of the smoke check and remaining presentation limits.

## Phase 3A-2 — core enemies and first animation sheets

Completed 2026-09-20. **15 runtime sheets, 84 frames, 15 unbound animation clips, four references and 20 preserved raw PNGs.** This addition does not change any Phase 3A-1 asset or approval.

Authorization: user's Phase 3A-2 production brief and subsequent “continue”; internal enemy-look selection requested by the brief. Style uses the user-selected Danube v002 above. Exact-pixel preparation continues the recorded export workflow. Enemy reference v001s were technically selected after comparing palette, top-down projection, local +X facing and distinct silhouettes; this is not separate user approval.

Canonical record: `Batches/p3a2_final_manifest.json`. Exact prompts and native-return paths/hashes for every retained source, reference dependencies, per-action source rectangles, constant scale factors, per-frame source pivots, palette, export hashes and Unity GUID/file IDs are recorded there. Technical evidence: `../Validation/Phase3A2/pixel-audit.json`, `unity-imports.json`, and `clip-playback-validation.json`.

### Runtime inventory and consumers

| Enemy / action | Runtime PNG | Sheet px | Frames | FPS / loop |
|---|---|---|---:|---|
| BasicInfected / idle | `Assets/Game/Art/Enemies/BasicInfected/bd_enemy_basic_infected_idle_e_sheet_v001.png` | 256x64 | 4 | 5 / yes |
| BasicInfected / walk | `Assets/Game/Art/Enemies/BasicInfected/bd_enemy_basic_infected_walk_e_sheet_v001.png` | 512x64 | 8 | 8 / yes |
| BasicInfected / attack | `Assets/Game/Art/Enemies/BasicInfected/bd_enemy_basic_infected_attack_e_sheet_v001.png` | 384x64 | 6 | 10 / no |
| BasicInfected / hit | `Assets/Game/Art/Enemies/BasicInfected/bd_enemy_basic_infected_hit_e_sheet_v001.png` | 128x64 | 2 | 10 / no |
| BasicInfected / death | `Assets/Game/Art/Enemies/BasicInfected/bd_enemy_basic_infected_death_e_sheet_v001.png` | 512x64 | 8 | 9 / no |
| Runner / idle | `Assets/Game/Art/Enemies/Runner/bd_enemy_runner_idle_e_sheet_v001.png` | 256x64 | 4 | 5 / yes |
| Runner / run | `Assets/Game/Art/Enemies/Runner/bd_enemy_runner_run_e_sheet_v001.png` | 512x64 | 8 | 12 / yes |
| Runner / attack | `Assets/Game/Art/Enemies/Runner/bd_enemy_runner_attack_e_sheet_v001.png` | 384x64 | 6 | 10 / no |
| Runner / hit | `Assets/Game/Art/Enemies/Runner/bd_enemy_runner_hit_e_sheet_v001.png` | 128x64 | 2 | 10 / no |
| Runner / death | `Assets/Game/Art/Enemies/Runner/bd_enemy_runner_death_e_sheet_v001.png` | 512x64 | 8 | 9 / no |
| Brute / idle | `Assets/Game/Art/Enemies/Brute/bd_enemy_brute_idle_e_sheet_v001.png` | 384x96 | 4 | 5 / yes |
| Brute / walk | `Assets/Game/Art/Enemies/Brute/bd_enemy_brute_walk_e_sheet_v001.png` | 768x96 | 8 | 6 / yes |
| Brute / attack | `Assets/Game/Art/Enemies/Brute/bd_enemy_brute_attack_e_sheet_v001.png` | 576x96 | 6 | 10 / no |
| Brute / hit | `Assets/Game/Art/Enemies/Brute/bd_enemy_brute_hit_e_sheet_v001.png` | 192x96 | 2 | 10 / no |
| Brute / death | `Assets/Game/Art/Enemies/Brute/bd_enemy_brute_death_e_sheet_v001.png` | 768x96 | 8 | 9 / no |

Current consumers: the matching clips in `Assets/Game/Animations/Enemies/<enemy-folder>/`. Future Basic Infected target is the existing Crawler presentation; Runner/Brute remain planned art roles. No gameplay bindings/definitions were created. Import: Sprite Multiple, center/Full Rect, 51.2 PPU, Point, uncompressed, mipmaps/Crunch off, Clamp, NPOT None, input alpha, sRGB, read/write off, no generated physics shape, max 4096. The importer reuses existing sprite IDs by semantic name on reruns.

### Design references

- `Assets/Game/Art/MasterReferences/bd_ref_enemy_lineup_core_v001.png`
- `Assets/Game/Art/MasterReferences/bd_ref_enemy_basic_infected_v001.png`
- `Assets/Game/Art/MasterReferences/bd_ref_enemy_runner_v001.png`
- `Assets/Game/Art/MasterReferences/bd_ref_enemy_brute_v001.png`

These are Default textures with no runtime consumer. All are opaque RGB; still-reference checkerboards are painted. Every actual runtime frame uses verified transparent RGBA with zeroed RGB in transparent pixels and at least two clear border pixels. Selected designs preserve clothing/gear sides and restrained natural skin variation; regional cues identify individual former humans within the fictional contamination story.

### Raw source inventory

| Retained native source | Native px / mode | SHA-256 | Status |
|---|---|---|---|
| `Assets/Game/Art/SourceSheets/p3a2_core_enemies_001/bd_src_enemy_basic_infected_attack_e_sheet_v001.png` | 1536x1024 / RGB | `6510bcb5ac27a4105ca9845ae0a865cee5b6a519c2a8571da016d7e9c5749b73` | selected |
| `Assets/Game/Art/SourceSheets/p3a2_core_enemies_001/bd_src_enemy_basic_infected_death_e_sheet_v001.png` | 1536x1024 / RGB | `0ee82b3f6cb3dffb059fe00b454054c99d574cf831ef571d70e320b1a745686f` | selected |
| `Assets/Game/Art/SourceSheets/p3a2_core_enemies_001/bd_src_enemy_basic_infected_hit_e_sheet_v001.png` | 1774x887 / RGB | `5a10b5f3ca89e1f11339e93d6c848c79d6d86fcbec9cbcbbe8b86c8bebfcaeae` | selected |
| `Assets/Game/Art/SourceSheets/p3a2_core_enemies_001/bd_src_enemy_basic_infected_idle_e_sheet_v001.png` | 1536x1024 / RGB | `a8296c03eafa43fa579454e2eab9730eb0537c54c73c5e8c1c84d6023a78491a` | selected |
| `Assets/Game/Art/SourceSheets/p3a2_core_enemies_001/bd_src_enemy_basic_infected_ref_v001.png` | 1254x1254 / RGB | `bedef6d9962a559db160bb84abf86dad3f46e9cbe5d15b4274c5011045ccf632` | selected |
| `Assets/Game/Art/SourceSheets/p3a2_core_enemies_001/bd_src_enemy_basic_infected_walk_e_sheet_v001.png` | 1536x1024 / RGB | `3e62a776dc86759c1d4abe85728753385cfb39a208eb6e1371620c45c8324d6f` | selected |
| `Assets/Game/Art/SourceSheets/p3a2_core_enemies_001/bd_src_enemy_brute_attack_e_sheet_v001.png` | 1536x1024 / RGB | `2ef30f289d54ecda6443a4fba4af48a7c84c5550e65a46f6d530a04721528ec4` | selected |
| `Assets/Game/Art/SourceSheets/p3a2_core_enemies_001/bd_src_enemy_brute_death_e_sheet_v001.png` | 1536x1024 / RGB | `5ebc47c43e9f23f6e79e559af11efc8fd1e1b9d0991407bbc5d27d64021d61a6` | selected |
| `Assets/Game/Art/SourceSheets/p3a2_core_enemies_001/bd_src_enemy_brute_hit_e_sheet_v001.png` | 1774x887 / RGB | `5d0b5016c3350aa25753c4619559cacbd2c72900ed3fff547e2f88b39d25cbf2` | selected |
| `Assets/Game/Art/SourceSheets/p3a2_core_enemies_001/bd_src_enemy_brute_idle_e_sheet_v001.png` | 1536x1024 / RGB | `1bf9609d0c947c82c52e3dde12d72c9d52a16767eb7802f5de7736b07f5dac76` | selected |
| `Assets/Game/Art/SourceSheets/p3a2_core_enemies_001/bd_src_enemy_brute_ref_v001.png` | 1254x1254 / RGB | `10c2c5253e9166f4124d1f2f170ed121e1947327669743737951a8729c8e66ea` | selected |
| `Assets/Game/Art/SourceSheets/p3a2_core_enemies_001/bd_src_enemy_brute_walk_e_sheet_v001.png` | 1536x1024 / RGB | `cc19858cecd2d500817b17e83e5578d8ba73e50569547897c0621359552fc326` | selected |
| `Assets/Game/Art/SourceSheets/p3a2_core_enemies_001/bd_src_enemy_lineup_core_v001.png` | 1536x1024 / RGB | `7a049c5b64fb70c02286a51018083613daf1b1a02d176ce4ec37f41fd91b8cd8` | selected |
| `Assets/Game/Art/SourceSheets/p3a2_core_enemies_001/bd_src_enemy_runner_attack_e_sheet_v001.png` | 1536x1024 / RGB | `c67dc3915ba2aa2be858ba892cdb9783fb30e93dd935814151e570a58f0abde7` | selected |
| `Assets/Game/Art/SourceSheets/p3a2_core_enemies_001/bd_src_enemy_runner_death_e_sheet_v001.png` | 1536x1024 / RGB | `d27da16ce9fe0e4dfa7c7f1f807fa16a2d0e90419bd7e25b0cd1163563375772` | selected |
| `Assets/Game/Art/SourceSheets/p3a2_core_enemies_001/bd_src_enemy_runner_hit_e_sheet_v001.png` | 1774x887 / RGB | `6371f45ddd401d7cf44666d3f3ce6d5cf86e682514ead193548bdc6a30cb2cad` | selected |
| `Assets/Game/Art/SourceSheets/p3a2_core_enemies_001/bd_src_enemy_runner_idle_e_sheet_v001.png` | 1536x1024 / RGB | `6950911525682f140ade04059e26f620eb885b93663acd53ce34024f6d318ed1` | selected |
| `Assets/Game/Art/SourceSheets/p3a2_core_enemies_001/bd_src_enemy_runner_ref_v001.png` | 1254x1254 / RGB | `95dbb0fdfdcfa1b6fe48e50714b11607d33e64766d8ed00a5b61545e7e69cfd4` | selected |
| `Assets/Game/Art/SourceSheets/p3a2_core_enemies_001/bd_src_enemy_runner_run_e_sheet_v001.png` | 1536x1024 / RGB | `2a9a803310db1d5c3bacc4f31ecbc7c83efa8c48c7a666e2c3c0967c654638c4` | rejected |
| `Assets/Game/Art/SourceSheets/p3a2_core_enemies_001/bd_src_enemy_runner_run_e_sheet_v002.png` | 1536x1024 / RGB | `9ddd599ddb4fc644d1d3d998e8de57718d91da8ac3bc12a9d315ec9fb8f3ceb3` | selected |


All raw sources are byte-identical to their returned files. Runner run v001 is rejected because its second row faces west; v002 replaces it. The other generated action sources are selected. Runner/Brute attack extension poses cross nominal source cell boundaries but not neighboring art; exact custom extraction rectangles preserve the complete hands. Export registration and crown-based constant action scaling correct native montage spacing/size, with no per-frame resizing or synthesized artwork.

Acceptance: native/logic-size contacts reviewed, 1080p/1440p terrain-scale estimates reviewed, 15/15 pixel checks passed, all 84 imported sprites verified and all 84 keys sampled through Unity Animator-backed preview rigs with unchanged transforms. Clips contain only renderer sprite keys and no events. Controller integration and lingering death presentation are deferred; no gameplay/scene/prefab changes were made.


## Phase 3A-3 — Defense assets and structure states (2026-09-20 to 2026-09-21)

Built-in ImageGen production under the requested autonomous internal review. Danube master v002 remains the approved style anchor. Technical acceptance is not separate per-file user approval. Final lineup uses native revision v002 at the explicitly requested reference path. Canonical provenance, source hashes, prompts, transforms and consumers: `Docs/Art/Batches/p3a3_final_manifest.json`; exact prompts under `p3a3_prompts/`.

| Runtime asset | Source | Status / consumer |
|---|---|---|
| `Assets/Game/Art/Defenses/BarbedWire/bd_defense_wire_corner_ne_v001.png` | `Assets/Game/Art/SourceSheets/p3a3_defenses_001/bd_src_wire_corner_ne_v001.png` | validated inventory; Future wire connection presentation; no autotiling added |
| `Assets/Game/Art/Defenses/BarbedWire/bd_defense_wire_corner_nw_v001.png` | `Assets/Game/Art/SourceSheets/p3a3_defenses_001/bd_src_wire_corner_nw_v001.png` | validated inventory; Future wire connection presentation; no autotiling added |
| `Assets/Game/Art/Defenses/BarbedWire/bd_defense_wire_corner_se_v001.png` | `Assets/Game/Art/SourceSheets/p3a3_defenses_001/bd_src_wire_corner_se_v001.png` | validated inventory; Future wire connection presentation; no autotiling added |
| `Assets/Game/Art/Defenses/BarbedWire/bd_defense_wire_corner_sw_v001.png` | `Assets/Game/Art/SourceSheets/p3a3_defenses_001/bd_src_wire_corner_sw_v001.png` | validated inventory; Future wire connection presentation; no autotiling added |
| `Assets/Game/Art/Defenses/BarbedWire/bd_defense_wire_damaged_v001.png` | `Assets/Game/Art/SourceSheets/p3a3_defenses_001/bd_src_wire_damaged_v001.png` | integrated and validated; Assets/Game/Prefabs/Defenses/BarbedWire.prefab |
| `Assets/Game/Art/Defenses/BarbedWire/bd_defense_wire_destroyed_v001.png` | `Assets/Game/Art/SourceSheets/p3a3_defenses_001/bd_src_wire_destroyed_v001.png` | imported; death-frame binding only; no persistent wreck; Assets/Game/Prefabs/Defenses/BarbedWire.prefab |
| `Assets/Game/Art/Defenses/BarbedWire/bd_defense_wire_horizontal_v001.png` | `Assets/Game/Art/SourceSheets/p3a3_defenses_001/bd_src_wire_horizontal_v001.png` | validated inventory; Future wire connection presentation; no autotiling added |
| `Assets/Game/Art/Defenses/BarbedWire/bd_defense_wire_single_v001.png` | `Assets/Game/Art/SourceSheets/p3a3_defenses_001/bd_src_wire_single_v001.png` | integrated and validated; Assets/Game/Prefabs/Defenses/BarbedWire.prefab |
| `Assets/Game/Art/Defenses/BarbedWire/bd_defense_wire_vertical_v001.png` | `Assets/Game/Art/SourceSheets/p3a3_defenses_001/bd_src_wire_vertical_v001.png` | validated inventory; Future wire connection presentation; no autotiling added |
| `Assets/Game/Art/Defenses/LandMine/bd_defense_mine_armed_v001.png` | `Assets/Game/Art/SourceSheets/p3a3_defenses_001/bd_src_mine_armed_v001.png` | integrated and validated; Assets/Game/Prefabs/Defenses/LandMine.prefab |
| `Assets/Game/Art/Defenses/LandMine/bd_defense_mine_idle_v001.png` | `Assets/Game/Art/SourceSheets/p3a3_defenses_001/bd_src_mine_idle_v002.png` | integrated and validated; Assets/Game/Prefabs/Defenses/LandMine.prefab |
| `Assets/Game/Art/Defenses/MachineGunNest/bd_defense_mg_base_damaged_v001.png` | `Assets/Game/Art/SourceSheets/p3a3_defenses_001/bd_src_mg_base_damaged_v001.png` | integrated and validated; Assets/Game/Prefabs/Defenses/MachineGunNest.prefab |
| `Assets/Game/Art/Defenses/MachineGunNest/bd_defense_mg_base_destroyed_v001.png` | `Assets/Game/Art/SourceSheets/p3a3_defenses_001/bd_src_mg_base_destroyed_v001.png` | imported; death-frame binding only; no persistent wreck; Assets/Game/Prefabs/Defenses/MachineGunNest.prefab |
| `Assets/Game/Art/Defenses/MachineGunNest/bd_defense_mg_base_idle_v001.png` | `Assets/Game/Art/SourceSheets/p3a3_defenses_001/bd_src_mg_base_idle_v002.png` | integrated and validated; Assets/Game/Prefabs/Defenses/MachineGunNest.prefab |
| `Assets/Game/Art/Defenses/MachineGunNest/bd_defense_mg_preview_v001.png` | `Composite of registered base idle + weapon/aim idle exports` | integrated and validated; Assets/Game/Prefabs/Defenses/MachineGunNest.prefab |
| `Assets/Game/Art/Defenses/MachineGunNest/bd_defense_mg_weapon_fire_sheet_v001.png` | `Assets/Game/Art/SourceSheets/p3a3_defenses_001/bd_src_mg_weapon_fire_sheet_v001.png` | integrated and validated; Assets/Game/Prefabs/Defenses/MachineGunNest.prefab |
| `Assets/Game/Art/Defenses/MachineGunNest/bd_defense_mg_weapon_idle_v001.png` | `Assets/Game/Art/SourceSheets/p3a3_defenses_001/bd_src_mg_weapon_fire_sheet_v001.png` | integrated and validated; Assets/Game/Prefabs/Defenses/MachineGunNest.prefab |
| `Assets/Game/Art/Defenses/SniperTower/bd_defense_sniper_aim_fire_sheet_v001.png` | `Assets/Game/Art/SourceSheets/p3a3_defenses_001/bd_src_sniper_aim_fire_sheet_v001.png` | integrated and validated; Assets/Game/Prefabs/Defenses/SniperTower.prefab |
| `Assets/Game/Art/Defenses/SniperTower/bd_defense_sniper_aim_idle_v001.png` | `Assets/Game/Art/SourceSheets/p3a3_defenses_001/bd_src_sniper_aim_fire_sheet_v001.png` | integrated and validated; Assets/Game/Prefabs/Defenses/SniperTower.prefab |
| `Assets/Game/Art/Defenses/SniperTower/bd_defense_sniper_base_damaged_v001.png` | `Assets/Game/Art/SourceSheets/p3a3_defenses_001/bd_src_sniper_base_damaged_v001.png` | integrated and validated; Assets/Game/Prefabs/Defenses/SniperTower.prefab |
| `Assets/Game/Art/Defenses/SniperTower/bd_defense_sniper_base_destroyed_v001.png` | `Assets/Game/Art/SourceSheets/p3a3_defenses_001/bd_src_sniper_base_destroyed_v001.png` | imported; death-frame binding only; no persistent wreck; Assets/Game/Prefabs/Defenses/SniperTower.prefab |
| `Assets/Game/Art/Defenses/SniperTower/bd_defense_sniper_base_idle_v001.png` | `Assets/Game/Art/SourceSheets/p3a3_defenses_001/bd_src_sniper_base_idle_v001.png` | integrated and validated; Assets/Game/Prefabs/Defenses/SniperTower.prefab |
| `Assets/Game/Art/Defenses/SniperTower/bd_defense_sniper_preview_v001.png` | `Composite of registered base idle + weapon/aim idle exports` | integrated and validated; Assets/Game/Prefabs/Defenses/SniperTower.prefab |
| `Assets/Game/Art/UI/bd_ui_sniper_tower_icon_v002.png` | `Assets/Game/Art/SourceSheets/p3a3_defenses_001/bd_src_sniper_icon_v002.png` | validated inventory; Future UI icon inventory (v001 preserved) |

| Retained native source | Actual PNG / mode | Original hash match | Disposition |
|---|---|---|---|
| `Assets/Game/Art/SourceSheets/p3a3_defenses_001/bd_src_defense_lineup_v001.png` | 1774x887 RGB | yes | Superseded by v002: corrected overhead sniper |
| `Assets/Game/Art/SourceSheets/p3a3_defenses_001/bd_src_defense_lineup_v002.png` | 1774x887 RGB | yes | Selected reference/source |
| `Assets/Game/Art/SourceSheets/p3a3_defenses_001/bd_src_mg_base_damaged_v001.png` | 1254x1254 RGB | yes | Selected reference/source |
| `Assets/Game/Art/SourceSheets/p3a3_defenses_001/bd_src_mg_base_destroyed_v001.png` | 1254x1254 RGB | yes | Selected reference/source |
| `Assets/Game/Art/SourceSheets/p3a3_defenses_001/bd_src_mg_base_idle_v001.png` | 1254x1254 RGB | yes | Superseded by v002: painted checkerboard |
| `Assets/Game/Art/SourceSheets/p3a3_defenses_001/bd_src_mg_base_idle_v002.png` | 1254x1254 RGB | yes | Selected reference/source |
| `Assets/Game/Art/SourceSheets/p3a3_defenses_001/bd_src_mg_reference_v001.png` | 1254x1254 RGB | yes | Selected reference/source |
| `Assets/Game/Art/SourceSheets/p3a3_defenses_001/bd_src_mg_weapon_fire_sheet_v001.png` | 1983x793 RGB | yes | Selected reference/source |
| `Assets/Game/Art/SourceSheets/p3a3_defenses_001/bd_src_mine_armed_v001.png` | 1295x1215 RGB | yes | Selected reference/source |
| `Assets/Game/Art/SourceSheets/p3a3_defenses_001/bd_src_mine_idle_v001.png` | 1295x1215 RGB | yes | Superseded by v002: painted checkerboard |
| `Assets/Game/Art/SourceSheets/p3a3_defenses_001/bd_src_mine_idle_v002.png` | 1295x1215 RGB | yes | Selected reference/source |
| `Assets/Game/Art/SourceSheets/p3a3_defenses_001/bd_src_mine_reference_v001.png` | 1295x1214 RGB | yes | Selected reference/source |
| `Assets/Game/Art/SourceSheets/p3a3_defenses_001/bd_src_sniper_aim_fire_sheet_v001.png` | 2172x724 RGB | yes | Selected reference/source |
| `Assets/Game/Art/SourceSheets/p3a3_defenses_001/bd_src_sniper_base_damaged_v001.png` | 1254x1254 RGB | yes | Selected reference/source |
| `Assets/Game/Art/SourceSheets/p3a3_defenses_001/bd_src_sniper_base_destroyed_v001.png` | 1254x1254 RGB | yes | Selected reference/source |
| `Assets/Game/Art/SourceSheets/p3a3_defenses_001/bd_src_sniper_base_idle_v001.png` | 1254x1254 RGB | yes | Selected reference/source |
| `Assets/Game/Art/SourceSheets/p3a3_defenses_001/bd_src_sniper_icon_v002.png` | 1254x1254 RGB | yes | Selected reference/source |
| `Assets/Game/Art/SourceSheets/p3a3_defenses_001/bd_src_sniper_reference_v001.png` | 1254x1254 RGB | yes | Superseded by v002: removed visible lower supports |
| `Assets/Game/Art/SourceSheets/p3a3_defenses_001/bd_src_sniper_reference_v002.png` | 1254x1254 RGB | yes | Selected reference/source |
| `Assets/Game/Art/SourceSheets/p3a3_defenses_001/bd_src_wire_corner_ne_v001.png` | 1254x1254 RGB | yes | Selected reference/source |
| `Assets/Game/Art/SourceSheets/p3a3_defenses_001/bd_src_wire_corner_nw_v001.png` | 1254x1254 RGB | yes | Selected reference/source |
| `Assets/Game/Art/SourceSheets/p3a3_defenses_001/bd_src_wire_corner_se_v001.png` | 1254x1254 RGB | yes | Selected reference/source |
| `Assets/Game/Art/SourceSheets/p3a3_defenses_001/bd_src_wire_corner_sw_v001.png` | 1254x1254 RGB | yes | Selected reference/source |
| `Assets/Game/Art/SourceSheets/p3a3_defenses_001/bd_src_wire_damaged_v001.png` | 1746x901 RGB | yes | Selected reference/source |
| `Assets/Game/Art/SourceSheets/p3a3_defenses_001/bd_src_wire_destroyed_v001.png` | 1746x901 RGB | yes | Selected reference/source |
| `Assets/Game/Art/SourceSheets/p3a3_defenses_001/bd_src_wire_horizontal_v001.png` | 1904x826 RGB | yes | Selected reference/source |
| `Assets/Game/Art/SourceSheets/p3a3_defenses_001/bd_src_wire_reference_v001.png` | 1746x901 RGB | yes | Selected reference/source |
| `Assets/Game/Art/SourceSheets/p3a3_defenses_001/bd_src_wire_single_v001.png` | 1747x900 RGB | yes | Selected reference/source |
| `Assets/Game/Art/SourceSheets/p3a3_defenses_001/bd_src_wire_vertical_v001.png` | 1024x1536 RGB | yes | Selected reference/source |

References: `ref_defense_lineup_core_01.png` (v002 source), `bd_ref_mg_reference_v001.png`, `bd_ref_sniper_reference_v002.png`, `bd_ref_wire_reference_v001.png`, `bd_ref_mine_reference_v001.png`. `bd_ref_sniper_reference_v001.png` is superseded. All under `Assets/Game/Art/MasterReferences/`, non-runtime textures.

Validation: 24 runtime PNGs, binary alpha and two-pixel margins, 7 aligned firing frames, 42 EditMode + 22 PlayMode tests, 45 live scene checks. Unity importer evidence and camera/HUD captures under `Docs/Validation/Phase3A3/`. Four prefab GUIDs preserved; gameplay data unchanged. Destroyed sprites do not extend defense lifetime. Existing VFX placeholders remain for Phase 3B.

## Phase 3B — combat and world effects (p3b_core_001)

Validated 2026-10-01; autonomous technical selection authorized by the Phase 3B request, no separate user approval claimed. Master anchor, exact prompts/references, retained source revisions, per-frame export operations and replacement reasons: [batch record](Batches/p3b_core_001.md). Full dimensions, import settings, pivots, FPS, caps, sorting and consumers: [VFX specifications](../../VFX_ART_SPECS.md). Native hashes/original paths: `Docs/Validation/Phase3B/source-receipts.json`.

## Native sources

| Source | Native PNG px | Exact prompt | Selection |
|---|---|---|---|
| `Assets/Game/Art/Effects/SourceSheets/p3b_core_001/bd_src_blood_decal_medium_v001.png` | 1254 x 1254 | [prompt](Batches/p3b_prompts/blood_decal_medium_v001.txt) | Selected; prepared as runtime art |
| `Assets/Game/Art/Effects/SourceSheets/p3b_core_001/bd_src_blood_decal_v001.png` | 1536 x 1024 | [prompt](Batches/p3b_prompts/blood_decal_v001.txt) | Selected; prepared as runtime art |
| `Assets/Game/Art/Effects/SourceSheets/p3b_core_001/bd_src_blood_hit_v001.png` | 2172 x 724 | [prompt](Batches/p3b_prompts/blood_hit_v001.txt) | Selected; prepared as runtime art |
| `Assets/Game/Art/Effects/SourceSheets/p3b_core_001/bd_src_explosion_small_v001.png` | 2172 x 724 | [prompt](Batches/p3b_prompts/explosion_small_v001.txt) | Selected; prepared as runtime art |
| `Assets/Game/Art/Effects/SourceSheets/p3b_core_001/bd_src_impact_concrete_v001.png` | 1536 x 1024 | [prompt](Batches/p3b_prompts/impact_concrete_v001.txt) | Selected; prepared as runtime art |
| `Assets/Game/Art/Effects/SourceSheets/p3b_core_001/bd_src_impact_dirt_v001.png` | 2172 x 724 | [prompt](Batches/p3b_prompts/impact_dirt_v001.txt) | Selected; prepared as runtime art |
| `Assets/Game/Art/Effects/SourceSheets/p3b_core_001/bd_src_impact_flesh_v001.png` | 1536 x 1024 | [prompt](Batches/p3b_prompts/impact_flesh_v001.txt) | Selected; prepared as runtime art |
| `Assets/Game/Art/Effects/SourceSheets/p3b_core_001/bd_src_impact_metal_v001.png` | 1536 x 1024 | [prompt](Batches/p3b_prompts/impact_metal_v001.txt) | Selected; prepared as runtime art |
| `Assets/Game/Art/Effects/SourceSheets/p3b_core_001/bd_src_impact_mud_v001.png` | 2172 x 724 | [prompt](Batches/p3b_prompts/impact_mud_v001.txt) | Selected; prepared as runtime art |
| `Assets/Game/Art/Effects/SourceSheets/p3b_core_001/bd_src_mg_muzzle_v001.png` | 2172 x 724 | [prompt](Batches/p3b_prompts/mg_muzzle_v001.txt) | Rejected: painted checkerboard; v002 replaces it |
| `Assets/Game/Art/Effects/SourceSheets/p3b_core_001/bd_src_mg_muzzle_v002.png` | 2172 x 724 | [prompt](Batches/p3b_prompts/mg_muzzle_v002.txt) | Selected; prepared as runtime art |
| `Assets/Game/Art/Effects/SourceSheets/p3b_core_001/bd_src_mine_explosion_v001.png` | 1536 x 1024 | [prompt](Batches/p3b_prompts/mine_explosion_v001.txt) | Selected; prepared as runtime art |
| `Assets/Game/Art/Effects/SourceSheets/p3b_core_001/bd_src_shore_foam_v001.png` | 1983 x 793 | [prompt](Batches/p3b_prompts/shore_foam_v001.txt) | Selected; prepared as runtime art |
| `Assets/Game/Art/Effects/SourceSheets/p3b_core_001/bd_src_smoke_medium_v001.png` | 2172 x 724 | [prompt](Batches/p3b_prompts/smoke_medium_v001.txt) | Selected; prepared as runtime art |
| `Assets/Game/Art/Effects/SourceSheets/p3b_core_001/bd_src_smoke_small_v001.png` | 2172 x 724 | [prompt](Batches/p3b_prompts/smoke_small_v001.txt) | Selected; prepared as runtime art |
| `Assets/Game/Art/Effects/SourceSheets/p3b_core_001/bd_src_sniper_muzzle_v001.png` | 2172 x 724 | [prompt](Batches/p3b_prompts/sniper_muzzle_v001.txt) | Selected; prepared as runtime art |
| `Assets/Game/Art/Effects/SourceSheets/p3b_core_001/bd_src_vfx_master_v001.png` | 1536 x 1024 | [prompt](Batches/p3b_prompts/master_v001.txt) | Selected reference; copied to ref_vfx_core_01.png |
| `Assets/Game/Art/Effects/SourceSheets/p3b_core_001/bd_src_water_ripple_v001.png` | 2172 x 724 | [prompt](Batches/p3b_prompts/water_ripple_v001.txt) | Selected; prepared as runtime art |
| `Assets/Game/Art/Effects/SourceSheets/p3b_core_001/bd_src_water_splash_large_v001.png` | 2172 x 724 | [prompt](Batches/p3b_prompts/water_splash_large_v001.txt) | Selected; prepared as runtime art |
| `Assets/Game/Art/Effects/SourceSheets/p3b_core_001/bd_src_water_splash_medium_v001.png` | 2172 x 724 | [prompt](Batches/p3b_prompts/water_splash_medium_v001.txt) | Rejected: upright fountain; v002 replaces it |
| `Assets/Game/Art/Effects/SourceSheets/p3b_core_001/bd_src_water_splash_medium_v002.png` | 2172 x 724 | [prompt](Batches/p3b_prompts/water_splash_medium_v002.txt) | Selected; prepared as runtime art |
| `Assets/Game/Art/Effects/SourceSheets/p3b_core_001/bd_src_water_splash_small_v001.png` | 2172 x 724 | [prompt](Batches/p3b_prompts/water_splash_small_v001.txt) | Selected; prepared as runtime art |
| `Assets/Game/Art/Effects/SourceSheets/p3b_core_001/bd_src_water_wake_v001.png` | 2172 x 724 | [prompt](Batches/p3b_prompts/water_wake_v001.txt) | Selected; prepared as runtime art |

## Prepared assets and consumers

All runtime exports are revision v001, prepared from the selected native revision shown. Runtime paths, cells, pivot, FPS, duration, caps, sorting and every consumer are specified in `VFX_ART_SPECS.md` and the machine-readable `p3b_manifest.json`. These records are authoritative together; filenames follow the repository's bd_fx naming rather than the request's illustrative filenames.

| Runtime asset | Native source / construction | Integration status |
|---|---|---|
| `Assets/Game/Art/Effects/Combat/MachineGun/bd_fx_mg_muzzle_sheet_v001.png` | `Assets/Game/Art/Effects/SourceSheets/p3b_core_001/bd_src_mg_muzzle_v002.png` | MG Fired; existing muzzle transform |
| `Assets/Game/Art/Effects/Combat/Sniper/bd_fx_sniper_muzzle_sheet_v001.png` | `Assets/Game/Art/Effects/SourceSheets/p3b_core_001/bd_src_sniper_muzzle_v001.png` | Sniper Fired; existing muzzle transform |
| `Assets/Game/Art/Effects/Combat/Impacts/bd_fx_impact_dirt_sheet_v001.png` | `Assets/Game/Art/Effects/SourceSheets/p3b_core_001/bd_src_impact_dirt_v001.png` | Validated inventory; no surface-impact event exists |
| `Assets/Game/Art/Effects/Combat/Impacts/bd_fx_impact_mud_sheet_v001.png` | `Assets/Game/Art/Effects/SourceSheets/p3b_core_001/bd_src_impact_mud_v001.png` | Validated inventory; no surface-impact event exists |
| `Assets/Game/Art/Effects/Combat/Impacts/bd_fx_impact_concrete_sheet_v001.png` | `Assets/Game/Art/Effects/SourceSheets/p3b_core_001/bd_src_impact_concrete_v001.png` | Validated inventory; no surface-impact event exists |
| `Assets/Game/Art/Effects/Combat/Impacts/bd_fx_impact_metal_sheet_v001.png` | `Assets/Game/Art/Effects/SourceSheets/p3b_core_001/bd_src_impact_metal_v001.png` | Validated inventory; no surface-impact event exists |
| `Assets/Game/Art/Effects/Combat/Impacts/bd_fx_impact_flesh_sheet_v001.png` | `Assets/Game/Art/Effects/SourceSheets/p3b_core_001/bd_src_impact_flesh_v001.png` | MG/sniper Fired at authoritative enemy impact |
| `Assets/Game/Art/Effects/Blood/Hits/bd_fx_blood_hit_sheet_v001.png` | `Assets/Game/Art/Effects/SourceSheets/p3b_core_001/bd_src_blood_hit_v001.png` | EnemyHealth.Damaged (alive) / Died |
| `Assets/Game/Art/Effects/Blood/Death/bd_fx_blood_decal_v001.png` | `Assets/Game/Art/Effects/SourceSheets/p3b_core_001/bd_src_blood_decal_v001.png` | Existing Crawler death |
| `Assets/Game/Art/Effects/Blood/Death/bd_fx_blood_decal_medium_v001.png` | `Assets/Game/Art/Effects/SourceSheets/p3b_core_001/bd_src_blood_decal_medium_v001.png` | Validated inventory; Death API at effectScale > 1 |
| `Assets/Game/Art/Effects/Explosions/Mine/bd_fx_mine_explosion_sheet_v001.png` | `Assets/Game/Art/Effects/SourceSheets/p3b_core_001/bd_src_mine_explosion_v001.png` | LandMine.Detonated; existing AOE represented visually |
| `Assets/Game/Art/Effects/Explosions/Generic/bd_fx_explosion_small_sheet_v001.png` | `Assets/Game/Art/Effects/SourceSheets/p3b_core_001/bd_src_explosion_small_v001.png` | Validated reusable inventory; no invented gameplay use |
| `Assets/Game/Art/Effects/Smoke/bd_fx_smoke_small_sheet_v001.png` | `Assets/Game/Art/Effects/SourceSheets/p3b_core_001/bd_src_smoke_small_v001.png` | MG/sniper health <= 35%; once per 1.2 seconds |
| `Assets/Game/Art/Effects/Smoke/bd_fx_smoke_medium_sheet_v001.png` | `Assets/Game/Art/Effects/SourceSheets/p3b_core_001/bd_src_smoke_medium_v001.png` | Mine aftermath |
| `Assets/Game/Art/Effects/Water/Splash/bd_fx_water_splash_small_sheet_v001.png` | `Assets/Game/Art/Effects/SourceSheets/p3b_core_001/bd_src_water_splash_small_v001.png` | Existing Crawler Water -> non-Water transition |
| `Assets/Game/Art/Effects/Water/Splash/bd_fx_water_splash_medium_sheet_v001.png` | `Assets/Game/Art/Effects/SourceSheets/p3b_core_001/bd_src_water_splash_medium_v002.png` | Validated inventory; no playable Runner exists |
| `Assets/Game/Art/Effects/Water/Splash/bd_fx_water_splash_large_sheet_v001.png` | `Assets/Game/Art/Effects/SourceSheets/p3b_core_001/bd_src_water_splash_large_v001.png` | Validated inventory; no playable Brute exists |
| `Assets/Game/Art/Effects/Water/Wake/bd_fx_water_wake_sheet_v001.png` | `Assets/Game/Art/Effects/SourceSheets/p3b_core_001/bd_src_water_wake_v001.png` | Attached renderer while existing enemy occupies Water |
| `Assets/Game/Art/Effects/World/bd_fx_water_ripple_sheet_v001.png` | `Assets/Game/Art/Effects/SourceSheets/p3b_core_001/bd_src_water_ripple_v001.png` | Existing Water cells; one request per .65 seconds |
| `Assets/Game/Art/Effects/World/bd_fx_shore_foam_sheet_v001.png` | `Assets/Game/Art/Effects/SourceSheets/p3b_core_001/bd_src_shore_foam_v001.png` | Existing Water/Beach boundary; looping strip per row |
| `Assets/Game/Art/Effects/Combat/MachineGun/bd_fx_mg_tracer_v001.png` | `code-native pixel streak: Docs/Art/export_phase3b.py` | MG Fired; visual hitscan segment |
| `Assets/Game/Art/Effects/Combat/Sniper/bd_fx_sniper_tracer_v001.png` | `code-native pixel streak: Docs/Art/export_phase3b.py` | Sniper Fired; visual hitscan segment |

Additional deliverables: `Assets/Game/Art/MasterReferences/ref_vfx_core_01.png`; `Assets/Game/Art/Effects/CombatVfxCatalog.asset`; `Assets/Game/Art/Effects/UIFeedback/bd_reward_digits_v001.ttf` (original digit/plus glyphs from `Docs/Art/create_reward_font.py`, fonttools used only in the external tool runtime). All 18 AnimationClips are listed below; each uses the corresponding ordered catalog frames/FPS and sprite-only binding.

- `Assets/Game/Animations/Effects/bd_fx_mg_muzzle_v001.anim`
- `Assets/Game/Animations/Effects/bd_fx_sniper_muzzle_v001.anim`
- `Assets/Game/Animations/Effects/bd_fx_impact_dirt_v001.anim`
- `Assets/Game/Animations/Effects/bd_fx_impact_mud_v001.anim`
- `Assets/Game/Animations/Effects/bd_fx_impact_concrete_v001.anim`
- `Assets/Game/Animations/Effects/bd_fx_impact_metal_v001.anim`
- `Assets/Game/Animations/Effects/bd_fx_impact_flesh_v001.anim`
- `Assets/Game/Animations/Effects/bd_fx_blood_hit_v001.anim`
- `Assets/Game/Animations/Effects/bd_fx_mine_explosion_v001.anim`
- `Assets/Game/Animations/Effects/bd_fx_explosion_small_v001.anim`
- `Assets/Game/Animations/Effects/bd_fx_smoke_small_v001.anim`
- `Assets/Game/Animations/Effects/bd_fx_smoke_medium_v001.anim`
- `Assets/Game/Animations/Effects/bd_fx_water_splash_small_v001.anim`
- `Assets/Game/Animations/Effects/bd_fx_water_splash_medium_v001.anim`
- `Assets/Game/Animations/Effects/bd_fx_water_splash_large_v001.anim`
- `Assets/Game/Animations/Effects/bd_fx_water_wake_v001.anim`
- `Assets/Game/Animations/Effects/bd_fx_water_ripple_v001.anim`
- `Assets/Game/Animations/Effects/bd_fx_shore_foam_v001.anim`

Validation: 23 native sources verified, 22 runtime PNGs, 18 sprite-only clips, 46 EditMode + 25 PlayMode tests, live primary-scene checks, capped 300-enemy stress. [Report](../../PHASE_3B_REPORT.md). Existing metadata, gameplay data, legacy scene and packages remain unchanged.

## Phase 3C — integrated production scene (p3c_integration_001)

Completed 2026-10-01 under the user's autonomous full-integration authorization. No new raster prompt/generation or accepted-art replacement. [Batch](Batches/p3c_integration_001.md) records revisions, consumers, settings and selection basis; [report](../../PHASE_3C_REPORT.md) records complete validation and limits. Approved master v002 remains the direction anchor. Existing 113 production PNGs / 258 sprites retain bytes, metadata, pivots, 51.2 world / 100 UI PPU, Point/uncompressed/no-mipmap settings and sub-sprite IDs.

| Asset/inventory | Integration target / status |
|---|---|
| `Assets/Game/Art/Environment/bd_env_*_v001.png` | Primary terrain retained; concrete/road/bunker floor, dirt/cracked-ground/silt overlays integrated. Exact used paths and transforms in `Docs/Validation/Phase3C/dressing.json` |
| `Assets/Game/Art/Props/bd_prop_*_v001.png` and Base v001 | River/quarantine/base dressing, 155 renderers with no collision/navigation ownership; same receipt records each used filename |
| `Assets/Game/Art/Base/bd_base_command_post_object_v001.png` | PlayerBase_TopDown Visual/Command Post; unit scale, GroundObjects 8; old roof primitives removed |
| `Assets/Game/Art/Enemies/BasicInfected/bd_enemy_basic_infected_hit_e_sheet_v001.png` | Existing Crawler hit observer: 2 frames, 10 FPS, 0.2s; idle/walk/attack/death retained |
| Existing defense sprites/ghosts and VFX inventory | Existing bindings reviewed and retained; obsolete visual leaves removed; no new gameplay event/type |
| UI foundation v001 and `bd_ui_sniper_tower_icon_v002.png` | Existing UI Toolkit HUD; panel/button explicit 16px slices, icons, text, control states, separate display order |
| `Assets/Game/Art/UI/bd_ui_hud_pixel_v001.ttf` | New original code-native font via `Docs/Art/create_hud_font.py`; full HUD and CombatFeedbackHud; SHA-256 `02E365C6EDE2E38417C1EB9B33FFF4B1D43ABEE08F8964C7ED388266E8CCE4B2` |
| `Assets/Game/Art/MasterReferences/ref_gameplay_integrated_01.png` | New actual wave-five firing screenshot, 1920x1080; Default texture, Point/uncompressed/no mipmaps, NPOT None, max2048, no runtime consumers; SHA-256 `913C83F6E6E68E199F39FC7566F7882EDA2D011593C9719800A3965BEEB0DC82` |

Validation: 46/46 EditMode, 27/27 PlayMode, all five natural waves, 21/21 VFX checks, natural base attack/Game Over, 576-cell access at both actual 1080/1440 resolutions, zero visible placeholders, bounded 100/200-enemy stress with 33 turrets. Existing gameplay data, prior art, metadata, packages, project settings and legacy scene remain byte-identical. Dynamic scaling and Editor-only performance evidence are documented limits. Inventory-only assets remain available without unsupported gameplay additions.

## Phase 4A — paper dinar presentation and playable enemy inventory

User explicitly authorized autonomous direct generation, selection/consistency iteration, money replacement and border integration. Approved master v002 and ref_gameplay_integrated_01 were inspected and used as local style references. Built-in imagegen; exact prompts, dimensions and SHA-256 provenance: [batch p4a_money_001](Batches/p4a_money_001.md). The agent selected candidates under the user's task authorization; no invented separate human approval. Raw sources remain byte-identical below SourceSheets/p4a_money_001.

| Runtime asset | Raw source / revision | Status and integration target |
|---|---|---|
| Assets/Game/Art/UI/ui_icon_money_rsd.png | bd_src_money_rsd_v001.png; 1263x1246 native | Integrated; 32x32 binary-alpha nearest export, UI 100 PPU Point/uncompressed/no mipmaps; TopDownHud.uss money-icon at 24px |
| Assets/Game/Art/UI/ui_icon_money_bundle_rsd.png | bd_src_money_bundle_rsd_v001.png; 1254x1254 native | Selected inventory; same 32x32 settings, no invented gameplay consumer |
| Assets/Game/Art/UI/ui_money_panel_rsd.png | bd_src_money_panel_rsd_v001.png; 1983x793 native | Integrated; 160x64 binary-alpha nearest export, 16px nine-slice; TopDownHud.uss money-panel |
| Assets/Game/Art/UI/Archive/bd_ui_money_icon_v001.png | Existing accepted coin v001; GUID preserved | Archived via AssetDatabase because user requested paper-money replacement; no primary presentation consumer |
| Existing Runner and Brute sprite sheets and animation clips | Existing Phase 3A2/3B accepted revisions | Existing frames bound to new Runner_TopDown / Brute_TopDown prefabs; shared sprite-only runtime presentation, local +X, 51.2 PPU and existing pivots/metadata retained |
| Existing quarantine fence, gate, road, floodlight, marker assets | Existing accepted environment/base/prop v001 | Final Border Checkpoint scene children; three right-edge lanes, no collision/occupancy, world settings/GUIDs retained |

Replacement scope changes money presentation only; primary gameplay changes are explicitly Phase 4A survival scope. Default 1000 homicide limit; unlimited <=0. Final validation and status: [Phase 4A report](../../PHASE_4A_REPORT.md). No next phase begun.

## Phase 4B — heavy defenses

Explicit authorization: the Phase 4B request instructs autonomous direct generation, style rejection/regeneration, first-pass integration and gameplay verification. No separate human approval is claimed. Approved master v002 and integrated gameplay reference were inspected; the agent-selected new lineage anchor is `Assets/Game/Art/MasterReferences/ref_heavy_defense_lineup_01.png`. Built-in imagegen generated every source sheet. Raw selected sources are under `Assets/Game/Art/SourceSheets/p4b_heavy_001/`; exact prompts/returned paths and source/runtime dimensions/hashes are in [prompts](Batches/p4b_heavy_prompts.json) and [inventory](Batches/p4b_heavy_inventory.json).

| Deliverables | Source / revision | Consumer and settings |
|---|---|---|
| `Defenses/MortarPosition/defense_mortar_{base_idle,weapon_idle,fire_sheet,damaged,destroyed}.png` | defense_mortar_pack.png, v001 | MortarPosition.prefab; 128px square frames, four firing frames at 16 FPS, 51.2 PPU, centered swivel exports |
| `Defenses/FlamethrowerPosition/defense_flamethrower_{base_idle,weapon_idle,fire_sheet,damaged,destroyed}.png` | corrected defense_flamethrower_pack.png, v002; original v001 rejected for duplicated tanks and wrong facing | FlamethrowerPosition.prefab; 64px frames, stationary tanks / separate local-+X nozzle, flame lives in VFX |
| `Defenses/AutoCannon/defense_autocannon_{base_idle,weapon_idle,fire_sheet,damaged,destroyed}.png` | defense_autocannon_pack.png, v001 | AutoCannon.prefab; 128px frames, four firing frames, nearest normalized metal bounds and fixed rotation pivot |
| `Defenses/RocketLauncherPosition/defense_rocket_{base_idle,launcher_idle,fire_sheet,damaged,destroyed}.png` | defense_rocket_pack.png, v001 | RocketLauncherPosition.prefab; 128px frames, four firing frames, independently rotating launcher |
| `Effects/Heavy/fx_{mortarmuzzle,mortarshell,mortarexplosion}_sheet.png` | fx_mortar_pack.png, v001 | VfxCatalog + area projectile presentation; 128px frames, four frames per effect |
| `Effects/Heavy/fx_{flamecone,burn,burningoverlay}_sheet.png` | corrected fx_flamethrower_pack.png, v002 | FlameWeapon presentation / one burn overlay per enemy; 64px frames, four frames; original nozzle-containing VFX sheet superseded |
| `Effects/Heavy/fx_{cannonmuzzle,cannontracer,cannonimpact}_sheet.png` | fx_autocannon_pack.png, v001 | existing TurretPresentation/VfxService heavy-shot extension; 64px frames, four frames |
| `Effects/Heavy/fx_{rocketprojectile,rocketsmoke,rocketexplosion}_sheet.png` | corrected fx_rocket_pack.png, v002 | area projectile presentation and existing smoke/burst pools; 128px frames; original smoke containing projectile pixels superseded |
| `UI/ui_icon_{mortar,flamethrower,autocannon,rocket}.png` | selected defense base cells | TopDownHud.uss; 32x32, UI 100 PPU; costs remain actual RSD text |

World PNGs use Point, uncompressed, no mipmaps, Clamp, binary alpha, nearest no-dither 5-bit channels. Runtime frames are row-major source cells exported to exact canvases; Multiple sheets have stable semantic frame names `__f000`–`__f003`, centered pivots. Weapon alignment normalizes opaque metal bounds separately from muzzle fire to correct native scale/pivot drift. Source rectangles, normalization, slicing, prefab and scene authoring are recorded in `Docs/Validation/Phase4B/Author.cs`. Existing gameplay asset GUIDs and old art remain; new assets use requested Phase 4B names. Shared Phase 3C.1 contact shadow is referenced at .28 opacity with world-aligned (+.04,-.04) offset. Gameplay roots remain unrotated, presentation layers rotate locally, heavy footprints are definition-owned and walkable. Validation/status: [batch](Batches/p4b_heavy_001.md) and [Phase 4B report](../../PHASE_4B_REPORT.md).

## Phase 4C — special infected, bosses and dismantle

User authorized autonomous generation, candidate correction/selection and integration. Approved master v002 reused; selection is by the agent under that authorization, with no invented human approval. Exact prompts/revisions: [batch p4c_content_001](Batches/p4c_content_001.md) and its JSON prompt receipt. Requested public-figure violent boss lineup was rejected by imagegen (request02cf49aa-5477-4cd8-8c88-a3aba2eba5c3); ref_boss_lineup_01.png and new Osama animation artwork were not produced. Undead Osama retains existing Brute presentation explicitly as a placeholder.

| Runtime inventory | Selected source revision | Integration/status |
|---|---|---|
| Enemies/Special/IsisBreacher_<action>_v002.png | breacher_v002.png | Five actions/fourframes,64px cells; IsisBreacher_TopDown shared sprite observer |
| Enemies/Special/IsisHeavy_<action>_v002.png | heavy_v002.png | Same; IsisHeavy_TopDown |
| Enemies/Special/PirateRunner_<action>_v002.png | runner_v002.png | Same; PirateRunner_TopDown |
| Enemies/Special/PirateRaider_<action>_v002.png | raider_v002.png | Same; PirateRaider_TopDown |
| Enemies/Special/PirateWarlord_<action>_v002.png | pirateBoss_v003.png | Five actions/fourframes,96px cells; PirateWarlord_TopDown and BossArt |
| Props/MilitantBossTransport_v001.png | militantBoat_v001.png |192x96; MilitantBossTransport hull, existing route/cargo ownership |
| Props/PirateBossTransport_v001.png | pirateBoat_v001.png |192x96; PirateBossTransport hull |
| UI/ui_icon_dismantle.png | toolsIcon_v001.png |64x64 binary-alpha crossed tools; UI Toolkit BuildHud and USS |
| Animations/Enemies/Special/<Id>_<action>.anim | Selected runtime sheets |25sprite-only clips; existing runtime frame arrays remain playback authority |

Raw candidates and corrected selected files preserved under SourceSheets/p4c_content_001; source/runtime dimensions, prompts, crop/export rules, PPU/pivots/FPS and integration targets recorded in the batch and Docs/Validation/Phase4C/art-import.json. All28runtime textures audited Point/uncompressed/noMips/FullRect/51.2PPU;100frame sprites and3single sprites. Existing references/GUIDs, gameplay roots, layers and originals retained. Status PARTIAL due blocked Osama art/lineup and unperformed human mouse-only validation; full88/50tests and post-review5tests pass. See [Phase4C report](../../PHASE_4C_REPORT.md).

## Phase 5A — upgrade overlays (2026-10-03)

Reference: approved Danube master v002. Authorized autonomous generation/integration; agent-selected variants, no separate human image approval claimed. Exact prompts, grid order, revisions, export and import settings: [p5a_upgrades_001](Batches/p5a_upgrades_001.md).

| Asset | Source / revision | Status / consumer |
|---|---|---|
| SourceSheets/p5a_upgrades_001/bd_src_upgrade_overlays_v001.png | Built-in imagegen; approved master style reference; 1536x1024 RGBA | Retained initial candidate, no runtime consumer |
| SourceSheets/p5a_upgrades_001/bd_src_upgrade_overlays_v002.png | Built-in alpha-focused edit of v001; 1536x1024 RGBA | Selected source, original bytes preserved, no runtime consumer |
| Defenses/bd_defense_upgrade_overlays_sheet_v001.png | v002, nearest-neighbor/hard-alpha/no-dither export; 256x256, 16x64px cells | Integrated and validated; sixteen LevelData.overlay references across eight DefenseDefinition assets, DefenseUpgradePresentation on existing prefabs |

Sprites: `upgrade_0`–`upgrade_15`, row-major pairs MG/Sniper/Wire/Mine/Mortar/Flame/Auto Cannon/Rocket II/III. Point, 51.2 PPU, Full Rect, centered, uncompressed, no mipmaps/physics shapes, Clamp, input alpha. Wire/mine retain GroundObjects ordering; weapons retain fixed base/rotating aim ownership. Footprint-scaled overlays add supply/protection details without replacing existing art or GUIDs. UI uses existing panel raster and functional text; selection uses four amber corner markers. Evidence and test results: `Docs/Validation/Phase5A/` and `PHASE_5A_REPORT.md`.

## Phase 5B — temporary event presentation reuse (2026-10-03)

No new generated raster assets. Existing wave chevrons, Auto Cannon icon, dismantle tools and RSD paper bundle are referenced by the thirteen event definition assets under `Assets/Game/Data/Events/`. Fog uses the existing Square as one transparent Simple-mode quad; Flooded Shore reuses the existing shallow-water tile over the first inland ground column. Pirate Raid reuses PirateBossTransport. Original GUIDs/import settings/prefab roots remain unchanged. Scope selection is authorized by the user's autonomous Phase 5B request, with no separate human art approval claimed. Consumer/layout/validation record: `Docs/Art/Batches/p5b_events_001.md` and `Docs/Validation/Phase5B/`.

## Phase 5C — product menu and tooltip reuse (2026-10-03)

No generated PNGs or replacements. MainMenu reuses the existing runtime Danube environment/props/base sprites as static overhead scenery; ProductMenus.uss reuses the existing large military panel and pixel font for all product screens. Original GUIDs/import settings and gameplay prefabs are unchanged. Runtime UI Toolkit text, button states, proportional card bounds/scrolling, and build/management tooltips remain functional. Authorized by the autonomous Phase 5C request; no separate human image approval claimed. Provenance/consumer record: [p5c_product_ui_001](Batches/p5c_product_ui_001.md). Validation: `Docs/Validation/Phase5C/` and `PHASE_5C_REPORT.md`.

## Phase 6A — industrial port (2026-10-03)

Autonomous production/selection authorized by the Phase 6A request; no independent human art approval claimed. Exact prompts and revisions: [p6a_port_001](Batches/p6a_port_001.md), [prompt JSON](Batches/p6a_port_001_prompts.json). All source sizes/hashes and runtime exports: `Docs/Validation/Phase6A/art-manifest.json`. Existing approved assets/GUIDs retained.

| Asset | Source/reference | Status and consumer |
|---|---|---|
| MasterReferences/ref_industrial_port_map_01.png | Built-in ImageGen, approved Danube master v002 style, 1536x1024 | Selected reference under autonomous request; no runtime binding |
| SourceSheets/p6a_port_001/port_props_raw_v001.png | Selected industrial master, 4x4 bounded sheet | Rejected front/side faces, retained bytes, no runtime consumer |
| SourceSheets/p6a_port_001/port_props_raw_v002.png | Built-in overhead corrective edit of v001 | Selected overhead export source, retained bytes |
| SourceSheets/p6a_port_001/port_terrain_raw_v001.png | Industrial/master v002 references, 2x2 materials | Selected export source, retained bytes |
| Props/bd_prop_port_00_v001.png through 15 | v002 alpha-bound slicing, nearest-neighbor 128x128, hard alpha | Integrated/validated Port Environment sprites; containers/roofs/crane/cargo/equipment/dock details |
| Environment/bd_env_port_00_v001.png through 03 | Four source quadrants, nearest-neighbor 64x64 opaque | Integrated/validated port concrete/asphalt/wet concrete/markings |
| Environment/PortEnvironment.spriteatlas | Twenty generated runtime textures | Packed with no rotation, Point, no mipmaps; no source/reference textures |
| UI/map_preview_industrial_port.png | Separate generated preview; original retained in source batch | Integrated/validated IndustrialPort MapDefinition.preview |
| UI/map_preview_danube_border.png | Actual original-map camera capture, 960x540 | Integrated/validated DanubeBorder MapDefinition.preview |

Runtime world sprites: 51.2 PPU, Sprite Single, centered, Point/uncompressed/no mips/NPOT None, retained binary alpha. Terrain Environment 0, roads 1; decoration GroundObjects 2; blockers/equipment/checkpoint 3–5. Static world-aligned contact shadows (+.04,-.04), .27 opacity, no object Update scripts. Existing water/fence/sandbag GUIDs reused. Gameplay masks live in BattlefieldIndustrialPort, with 102 water/163 static blocked/18 non-buildable pier/48 floodable cells. Scene owns geometry; art does not alter stats, economy or footprints. Evidence: `Docs/Validation/Phase6A/` and `PHASE_6A_REPORT.md`.
