# Phase 6C research icons

Scope: four five-icon branch strips plus one distinct RP research-file icon. No world art replacement. Existing master `Assets/Game/Art/MasterReferences/bd_ref_danube_master_v002.png` supplies palette/material language; its obsolete creature imagery is not reused.

Selection is authorized by the user's autonomous Phase 6C request; no independent human art approval is claimed. Raw generated outputs and exact prompts are retained under `Assets/Game/Art/SourceSheets/p6c_research_001/`. Runtime textures belong to `Assets/Game/Art/UI/Research/`. Import through AssetDatabase, Point filtering, no mips, no compression, Sprite/Multiple for branch strips with five equal horizontal cells, Sprite/Single for RP. UI uses authored sprite rectangles at small display sizes, not source/reference imagery.

Planned branch strips: Logistics (fund, crate, salvage tools, supply routes, stockpile); Engineering (emplacement armor, wire, maintenance, modular parts, fortification); Weapons (ammunition, scope, blast, fuel, fire control); Intelligence (river radar, alert, specimen, event weather radar, deep-water hydrophone). RP uses a cyan technical research file, never the RSD banknote.

Acceptance: transparent gutters, readable distinct silhouettes, muted gritty hard pixel clusters, no glossy/fantasy treatment, meaningful selected/purchased/locked UI states in addition to color. Actual dimensions, selected paths, import evidence and UI captures are appended after production.

## Production and integration

Five built-in imagegen calls produced four branch strips and one RP icon. Exact prompts and returned source paths are in `p6c_research_001.prompts.json` and `p6c_research_rp.prompt.json`. No independent human approval is claimed. The executing agent selected v001 after inspection under the autonomous Phase 6C request.

| Runtime texture | Actual pixels | Sprite consumers |
|---|---|---|
| `bd_ui_research_logistics_sheet_v001.png` | 2059×764 | Five Logistics node definitions |
| `bd_ui_research_engineering_sheet_v001.png` | 2172×724 | Five Engineering node definitions |
| `bd_ui_research_weapons_sheet_v001.png` | 2172×724 | Five Weapons node definitions |
| `bd_ui_research_intelligence_sheet_v001.png` | 2172×724 | Five Intelligence node definitions |
| `bd_ui_research_points_v001.png` | 1254×1254 | ResearchCatalog RP currency icon |

Source copies remain byte-identical under `SourceSheets/p6c_research_001/`; runtime textures are copied locally to `Art/UI/Research/`. No generated image depends on a private generator directory at runtime. SHA-256/dimensions: `Docs/Validation/Phase6C/art-manifest.json`. Real RGBA transparency is inspected independently in `alpha-audit.json`. No pixel repaint, interpolation or background-removal substitute was used.

Unity import uses Point, uncompressed, no mips and alpha transparency. Branch strips are Sprite/Multiple, 51.2 PPU; five equal horizontal source regions are tightened to their opaque silhouette bounds through the Sprite Editor provider. Saved rects, pivots and all consumers are retained in `saved-assets.json`; semantic sub-sprite names and IDs are preserved on repeat import. RP is Sprite/Single. These are UI-only icons; no world footprint, sorting, gameplay root, prefab, animation or original asset GUID changed. The technical grid/links/state borders use UI Toolkit drawing and styles.

Visual acceptance: all twenty node icons and RP inspected; actual 1366×768, 1920×1080, 2560×1440 research captures; selected/purchased/locked/insufficient views; preparation intelligence and run summary. Nodes have no pairwise overlap, research card stays on screen and the intelligence panel is separated from speed controls. Node captions were reduced slightly to keep the longest words intact. Existing runtime hover tooltips work; capture helper temporarily hides hover overlay for clean board captures.

Validation and remaining gameplay/balance limits are recorded in `PHASE_6C_REPORT.md`. Raw art remains first-pass generated work, with no final handcrafted-polish claim.
