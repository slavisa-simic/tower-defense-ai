"""Build the delivery catalog from selected export/import evidence; never edits Unity assets."""
from pathlib import Path
import json,hashlib,re,difflib
from PIL import Image
ROOT=Path(__file__).resolve().parents[2]
E=ROOT/'Docs/Validation/Phase3A1'
def read(p): return json.loads((ROOT/p).read_text(encoding='utf-8-sig'))
def sha(p): return hashlib.sha256(p.read_bytes()).hexdigest()
def write(p,text): (ROOT/p).write_text(text,encoding='utf-8')
specs=read('Docs/Art/Batches/p3a1_final_manifest.json')
exports={r['output']:r for r in read('Docs/Validation/Phase3A1/p3a1_final_manifest-exports.json')}
imports={r['path']:r for r in read('Docs/Validation/Phase3A1/p3a1_final_manifest-imports.json')}
master='Assets/Game/Art/MasterReferences/bd_ref_danube_master_v002.png'
assert sha(ROOT/master)=='354421e55c341bfbcc65993ba20c644842eec44073cd3bbb7eb3b7794f7d7e45'
assert len(specs)==52 and len({s['output'] for s in specs})==52
for s in specs:
    ex=exports[s['output']]; imp=imports[s['output']]
    assert ex['output_sha256']==sha(ROOT/s['output']) and ex['source_sha256']==sha(ROOT/s['source'])
    assert imp['filter']=='Point' and imp['compression']=='Uncompressed' and imp['type']=='Sprite' and not imp['mipmapEnabled']
    assert imp['spritePixelsPerUnit']==(100 if '/UI/' in s['output'] else 51.2)
    assert [imp['width'],imp['height']]==s['size']
    assert imp['wrapU']==('Repeat' if 'x' in s.get('repeat','') else 'Clamp')
    assert imp['wrapV']==('Repeat' if 'y' in s.get('repeat','') else 'Clamp')
    assert imports[s['source']]['type']=='Default'

# Native sources are matched byte-for-byte to the original returned generator files.
generated=Path('C:/Users/Slavisa/.codex/generated_images/01a0b1b5-5187-73b1-ab47-d87b085ef564')
byhash={sha(p):str(p) for p in generated.glob('*.png')}
provenance=[]
for p in sorted((ROOT/'Assets/Game/Art/SourceSheets').rglob('*.png')):
    digest=sha(p); assert digest in byhash,p
    im=Image.open(p)
    provenance.append(dict(path=p.relative_to(ROOT).as_posix(),returned_path=byhash[digest],sha256=digest,native_size=im.size,native_mode=im.mode))
write('Docs/Art/Batches/p3a1_all_native_receipts.json',json.dumps(provenance,indent=2)+'\n')
baseline=read('Docs/Validation/Phase3A1/preexisting-files.json')
changed=[r['path'] for r in baseline if not (ROOT/r['path']).exists() or sha(ROOT/r['path'])!=r['sha256'].lower()]
assert changed==['Assets/Game/Scenes/CoastalDefense_TopDown.unity'],changed
integrity=dict(preexisting_files=len(baseline),unchanged=len(baseline)-len(changed),changed=changed,existing_asset_guids_preserved=True,new_runtime_sprites=len(specs),native_sources=len(provenance))
write('Docs/Validation/Phase3A1/repository-integrity-final.json',json.dumps(integrity,indent=2)+'\n')
old=(E/'preintegration-scene.unity.txt').read_text(); new=(ROOT/changed[0]).read_text()
write('Docs/Validation/Phase3A1/terrain-scene.diff',''.join(difflib.unified_diff(old.splitlines(True),new.splitlines(True),fromfile='before',tofile='after')))

header='''# Art asset register

Phase 3A-1 completed 2026-09-19. 52 selected runtime sprites: 10 environment, 17 props, 10 base/fortification and 15 UI. All are exported, visually reviewed and imported into Unity. Technical acceptance is recorded here; only the master has separate user selection evidence.

Approved master: `Assets/Game/Art/MasterReferences/bd_ref_danube_master_v002.png`, SHA-256 `354421E55C341BFBCC65993BA20C644842EEC44073CD3BBB7EB3B7794F7D7E45`. User replied **"continue"** after the displayed revision and explicit selection question. The user also authorized deterministic exports: **"Yes—prepare exact game-ready pixel exports"** in response to the resize/crop, palette and seam-cleanup question. No further reference approval is pending.

Canonical reproduction input: `Batches/p3a1_final_manifest.json` (exact prompts, local references, source/output paths, logical dimensions, alpha, repeat axes, palette and panel borders). The selected output evidence is `../Validation/Phase3A1/p3a1_final_manifest-exports.json` and `p3a1_final_manifest-imports.json`; these supersede intermediate export/import evidence. Exact original returned paths, native dimensions and SHA-256 values for every source are in `Batches/p3a1_all_native_receipts.json`. Native source files are byte-identical to generator outputs. Built-in ImageGen was used; no seed or model identifier is asserted.

World defaults: 51.2 PPU, Sprite Single, center pivot, Full Rect, Point, uncompressed, no mipmaps, NPOT None, sRGB, read/write off, no fallback physics shape. UI uses 100 PPU and explicit logical sizing. Source/reference textures are Default, Point, uncompressed, no mipmaps, Clamp, max 4096. Runtime maximum is 4096; actual sizes are verified unchanged by import. All isolated sprites have binary alpha and at least two transparent pixels of margin. Terrain is opaque and repeats only on the specified axes.

## Selected runtime inventory

All rows use approved master v002 and technical acceptance on 2026-09-19. The source column identifies the native generation, not a runtime sheet. No animations or sliced subframes exist in this phase.

| Runtime path | Native source | Px | PPU / repeat | Integration target / current consumer |
|---|---|---|---|---|
'''
consumers={'deep_danube_water_tile':'Ocean','shallow_water_tile':'Shallow Water','muddy_shore_tile':'Beach','wet_bank_transition_tile':'Shoreline','grass_tile':'Grass Combat Field'}
for s in specs:
    subject=s.get('subject',Path(s['output']).stem.removeprefix('bd_env_').removesuffix('_v001'))
    cat=s['output'].split('/')[3]
    consumer='Primary scene / '+consumers[subject] if subject in consumers else ('Future TopDownHud presentation; no binding yet' if cat=='UI' else 'Imported visual inventory; no scene/prefab consumer')
    header+=f"| `{s['output']}` | `{s['source']}` | {s['size'][0]}x{s['size'][1]} | {100 if cat=='UI' else 51.2} / {s.get('repeat','Clamp') or 'Clamp'} | {consumer} |\n"
header+='\n## Native source inventory\n\nEach retained source is an ordinary non-runtime texture. This table plus the JSON receipts records every source file, including rejected revisions.\n\n| Path | Native px / mode | Status |\n|---|---|---|\n'
selected={s['source'] for s in specs}
for r in provenance:
    p=r['path']; status='Selected source for one runtime export' if p in selected else ('Approved master source' if p.endswith('danube_master_v002.png') else 'Archived, not a runtime consumer')
    header+=f"| `{p}` | {r['native_size'][0]}x{r['native_size'][1]} / {r['native_mode']} | {status} |\n"
header+='''
## Reference files and rejected revisions

| Path | Status / reason | Consumer |
|---|---|---|
| `Assets/Game/Art/MasterReferences/bd_ref_danube_master_v001.png` | Superseded: facade and noise defects; never approved | None |
| `Assets/Game/Art/MasterReferences/bd_ref_danube_master_v002.png` | User-selected style anchor; native 1536x1024 RGB | Later bounded art batches |

Spotlight source v001 had a painted checkerboard that contaminated its crop; v002 was rejected for a side-view silhouette; v003 supplies the accepted overhead runtime export. Money source v001 read as supply tags; v002 uses clear round coins. Rejected native files remain in SourceSheets only for provenance. Runtime filenames remain v001 because these were corrections before initial delivery, not replacements of approved production assets. Master v001 is also retained as explicitly superseded provenance.

## Validation and limits

52/52 actual dimensions and alpha contracts pass; declared tile seams match at the outer edges. Three build-button alpha masks match exactly; selected brackets and disabled dashed rim differ beyond hue. All 104 selected source/runtime imports were checked. Panel borders are 16 pixels on all sides, with 20-pixel content padding (16 for build buttons); UI background slicing must be explicitly applied by a later UI Toolkit integration. Functional text, prices, state and callbacks remain owned by existing UI/gameplay code.

Native and integer-scale light/dark previews: `Docs/Validation/Phase3A1/<category>-review-*.png`; terrain repeat preview and camera captures are in the same folder. 42 EditMode and 22 PlayMode tests passed. Saved scene diff changes only five terrain renderers and their visual scales. No existing asset GUID, gameplay script, prefab, data, package or project setting was changed. See `PHASE_3A1_REPORT.md` for the scope of the smoke check and remaining presentation limits.
'''
write('Docs/Art/ASSET_REGISTER.md',header)

manifest='''# Phase 3A-1 asset manifest

Completed 2026-09-19: all 52 mandatory runtime PNGs exported and imported using approved master v002. Optional label elements remain deferred. Every row is technically accepted for this foundation delivery. Native source provenance, prompts, revisions and settings are in `Docs/Art/ASSET_REGISTER.md` and `Docs/Art/Batches/p3a1_final_manifest.json`.

64x64 terrain tiles cover one 1.25-unit cell at 51.2 PPU. All world art shares that PPU; UI is 100 PPU with explicit logical dimensions. Props/base/icons have real alpha, centered pivots and at least two pixels of margin. No gameplay footprint is inferred from the visual canvas.
'''
for cat in ['Environment','Props','Base','UI']:
    group=[s for s in specs if '/'+cat+'/' in s['output']]
    manifest+=f'\n## {cat} ({len(group)})\n\n| Asset file | Exact px | Alpha / repeat | Status |\n|---|---|---|---|\n'
    for s in group:
        manifest+=f"| `{s['output']}` | {s['size'][0]}x{s['size'][1]} | {'Alpha' if s['alpha'] else 'Opaque'} / {s.get('repeat','Clamp') or 'Clamp'} | Exported, reviewed, imported |\n"
manifest+='\nFive environment textures are used by the primary scene. Other files are prepared inventory for bounded later integration. UI layouts/bindings, enemies, defense world sprites/animations, VFX and audio remain unchanged. Next recommended work: **Phase 3A-2 — Core Enemies + First Animation Sheets**, using master v002.\n'
write('ASSET_LIST_PHASE_3A1.md',manifest)
print(json.dumps(integrity,indent=2))
