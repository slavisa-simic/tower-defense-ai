# Phase 3A-2 asset manifest

Completed 2026-09-20. Three enemy sets, **15 transparent runtime sheets / 84 frames**, four design references, and 15 unbound Unity animation clips. Approved style anchor: Danube master v002. Exact native sources, prompts, hashes, source extraction rectangles, scale/registration, Unity sprite IDs and clip consumers are in `Docs/Art/Batches/p3a2_final_manifest.json`.

## Runtime sheets

All files below passed pixel/export and Unity import validation. Every action uses one horizontal row, center pivots, local +X facing and 51.2 PPU. Each named frame is `bd_enemy_<enemy>_<action>_e__fNNN`. These are prepared visual assets; current gameplay still uses its existing Crawler placeholder.

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

## References and source retention

- `Assets/Game/Art/MasterReferences/bd_ref_enemy_lineup_core_v001.png`
- `Assets/Game/Art/MasterReferences/bd_ref_enemy_basic_infected_v001.png`
- `Assets/Game/Art/MasterReferences/bd_ref_enemy_runner_v001.png`
- `Assets/Game/Art/MasterReferences/bd_ref_enemy_brute_v001.png`

References are opaque non-runtime design images. Individual stills contain a painted checkerboard and must never be treated as transparent sprites. Runtime sheets have verified binary alpha. All 20 raw ImageGen PNGs are retained byte-identically in `Assets/Game/Art/SourceSheets/p3a2_core_enemies_001/`: 19 selected generations and one rejected Runner run v001. The selected Runner run uses source v002; its first runtime export is v001.

## Prepared animations

`Assets/Game/Animations/Enemies/BasicInfected/`, `Runner/` and `Brute/` each contain five clips named `bd_enemy_<enemy>_<action>_e_v001.anim`. Each clip has exactly the frame count shown above, one discrete `Visual/Body:SpriteRenderer.m_Sprite` curve, zero events and zero transform curves. No AnimatorController, prefab or gameplay hookup was added. Existing immediate death behavior is unchanged.

Review evidence: `Docs/Validation/Phase3A2/` contains per-action frame strips/GIFs, three contact boards, camera-scale estimates at 1080p/1440p, shared locomotion preview, pixel audit, native provenance, Unity imports and playback validation. See `PHASE_3A2_REPORT.md` for test results and boundaries.
