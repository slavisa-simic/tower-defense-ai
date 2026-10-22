"""Catalog actual accepted assets, source hashes, prompts and Unity consumers."""
import json
from pathlib import Path
from export_phase3a2 import ROOT,MANIFEST,VALIDATION,PALETTE

def main():
    entries=json.loads(MANIFEST.read_text())
    audit=json.loads((VALIDATION/'pixel-audit.json').read_text())
    exports=json.loads((VALIDATION/'exports-all.json').read_text())
    imports=json.loads((VALIDATION/'unity-imports.json').read_text())
    sources=audit['nativeSources']; refs=[]
    for subject in ['lineup_core','basic_infected','runner','brute']:
        name=f'bd_ref_enemy_{subject}_v001'
        source=f'Assets/Game/Art/SourceSheets/p3a2_core_enemies_001/bd_src_enemy_{subject}{"" if subject=="lineup_core" else "_ref"}_v001.png'
        prompt='Docs/Art/Batches/p3a2_lineup_v001.prompt.txt' if subject=='lineup_core' else f'Docs/Art/Batches/{name}.prompt.txt'
        refs.append({'reference':f'Assets/Game/Art/MasterReferences/{name}.png','source':source,'promptFile':prompt,
            'prompt':(ROOT/prompt).read_text(),'styleReference':'Assets/Game/Art/MasterReferences/bd_ref_danube_master_v002.png',
            'inputReferences':['Assets/Game/Art/MasterReferences/bd_ref_danube_master_v002.png']+([] if subject=='lineup_core' else ['Assets/Game/Art/MasterReferences/bd_ref_enemy_lineup_core_v001.png']),
            'status':'technically selected design reference; no separate user approval asserted',
            'alpha':'opaque RGB reference only; individual stills contain a painted checkerboard, not transparency'})
    for source in sources:
        e=next((e for e in entries if e['source']==source['source']),None)
        ref=next((r for r in refs if r['source']==source['source']),None)
        source['promptFile']=e['promptPath'] if e else ref['promptFile'] if ref else 'Docs/Art/Batches/bd_enemy_runner_run_v001.prompt.txt'
        source['status']='selected' if e or ref else 'rejected'
        source['reason']=None if e or ref else 'Runner run v001 reverses west in the bottom row; regenerated as v002'
    data={'phase':'Phase 3A-2','dates':'2026-09-19 to 2026-09-20','tool':'built-in ImageGen',
        'master':'Assets/Game/Art/MasterReferences/bd_ref_danube_master_v002.png',
        'masterSHA256':'354421e55c341bfbcc65993ba20c644842eec44073cd3bbb7eb3b7794f7d7e45',
        'palette':PALETTE,'references':refs,'nativeSources':sources,'actions':entries,'exports':exports,'unity':imports,
        'approval':'Existing user-selected Danube v002; enemy designs technically selected under requested internal review. No per-file user approval invented.',
        'consumers':'15 unbound sprite-only clips reference the runtime sheets; no gameplay prefab, scene, UI or data consumers'}
    (ROOT/'Docs/Art/Batches/p3a2_final_manifest.json').write_text(json.dumps(data,indent=2),encoding='utf-8')
    rows=['| Enemy / action | Runtime PNG | Sheet px | Frames | FPS / loop |', '|---|---|---|---:|---|']
    for e in entries:
        rows.append(f"| {e['folder']} / {e['action']} | `{e['output']}` | {e['frame']*e['count']}x{e['frame']} | {e['count']} | {e['fps']} / {'yes' if e['loop'] else 'no'} |")
    table='\n'.join(rows)
    ref_list='\n'.join('- `'+r['reference']+'`' for r in refs)
    manifest=f'''# Phase 3A-2 asset manifest

Completed 2026-09-20. Three enemy sets, **15 transparent runtime sheets / 84 frames**, four design references, and 15 unbound Unity animation clips. Approved style anchor: Danube master v002. Exact native sources, prompts, hashes, source extraction rectangles, scale/registration, Unity sprite IDs and clip consumers are in `Docs/Art/Batches/p3a2_final_manifest.json`.

## Runtime sheets

All files below passed pixel/export and Unity import validation. Every action uses one horizontal row, center pivots, local +X facing and 51.2 PPU. Each named frame is `bd_enemy_<enemy>_<action>_e__fNNN`. These are prepared visual assets; current gameplay still uses its existing Crawler placeholder.

{table}

## References and source retention

{ref_list}

References are opaque non-runtime design images. Individual stills contain a painted checkerboard and must never be treated as transparent sprites. Runtime sheets have verified binary alpha. All 20 raw ImageGen PNGs are retained byte-identically in `Assets/Game/Art/SourceSheets/p3a2_core_enemies_001/`: 19 selected generations and one rejected Runner run v001. The selected Runner run uses source v002; its first runtime export is v001.

## Prepared animations

`Assets/Game/Animations/Enemies/BasicInfected/`, `Runner/` and `Brute/` each contain five clips named `bd_enemy_<enemy>_<action>_e_v001.anim`. Each clip has exactly the frame count shown above, one discrete `Visual/Body:SpriteRenderer.m_Sprite` curve, zero events and zero transform curves. No AnimatorController, prefab or gameplay hookup was added. Existing immediate death behavior is unchanged.

Review evidence: `Docs/Validation/Phase3A2/` contains per-action frame strips/GIFs, three contact boards, camera-scale estimates at 1080p/1440p, shared locomotion preview, pixel audit, native provenance, Unity imports and playback validation. See `PHASE_3A2_REPORT.md` for test results and boundaries.
'''
    (ROOT/'ASSET_LIST_PHASE_3A2.md').write_text(manifest,encoding='utf-8')
    marker='## Phase 3A-2 — core enemies and first animation sheets'
    path=ROOT/'Docs/Art/ASSET_REGISTER.md';old=path.read_text(encoding='utf-8')
    if marker in old:old=old.split(marker)[0].rstrip()+'\n'
    raw_table='| Retained native source | Native px / mode | SHA-256 | Status |\n|---|---|---|---|\n'
    for s in sources:raw_table+=f"| `{s['source']}` | {s['size'][0]}x{s['size'][1]} / {s['mode']} | `{s['sha256']}` | {s['status']} |\n"
    section=f'''
{marker}

Completed 2026-09-20. **15 runtime sheets, 84 frames, 15 unbound animation clips, four references and 20 preserved raw PNGs.** This addition does not change any Phase 3A-1 asset or approval.

Authorization: user's Phase 3A-2 production brief and subsequent “continue”; internal enemy-look selection requested by the brief. Style uses the user-selected Danube v002 above. Exact-pixel preparation continues the recorded export workflow. Enemy reference v001s were technically selected after comparing palette, top-down projection, local +X facing and distinct silhouettes; this is not separate user approval.

Canonical record: `Batches/p3a2_final_manifest.json`. Exact prompts and native-return paths/hashes for every retained source, reference dependencies, per-action source rectangles, constant scale factors, per-frame source pivots, palette, export hashes and Unity GUID/file IDs are recorded there. Technical evidence: `../Validation/Phase3A2/pixel-audit.json`, `unity-imports.json`, and `clip-playback-validation.json`.

### Runtime inventory and consumers

{table}

Current consumers: the matching clips in `Assets/Game/Animations/Enemies/<enemy-folder>/`. Future Basic Infected target is the existing Crawler presentation; Runner/Brute remain planned art roles. No gameplay bindings/definitions were created. Import: Sprite Multiple, center/Full Rect, 51.2 PPU, Point, uncompressed, mipmaps/Crunch off, Clamp, NPOT None, input alpha, sRGB, read/write off, no generated physics shape, max 4096. The importer reuses existing sprite IDs by semantic name on reruns.

### Design references

{ref_list}

These are Default textures with no runtime consumer. All are opaque RGB; still-reference checkerboards are painted. Every actual runtime frame uses verified transparent RGBA with zeroed RGB in transparent pixels and at least two clear border pixels. Selected designs preserve clothing/gear sides and restrained natural skin variation; regional cues identify individual former humans within the fictional contamination story.

### Raw source inventory

{raw_table}

All raw sources are byte-identical to their returned files. Runner run v001 is rejected because its second row faces west; v002 replaces it. The other generated action sources are selected. Runner/Brute attack extension poses cross nominal source cell boundaries but not neighboring art; exact custom extraction rectangles preserve the complete hands. Export registration and crown-based constant action scaling correct native montage spacing/size, with no per-frame resizing or synthesized artwork.

Acceptance: native/logic-size contacts reviewed, 1080p/1440p terrain-scale estimates reviewed, 15/15 pixel checks passed, all 84 imported sprites verified and all 84 keys sampled through Unity Animator-backed preview rigs with unchanged transforms. Clips contain only renderer sprite keys and no events. Controller integration and lingering death presentation are deferred; no gameplay/scene/prefab changes were made.
'''
    path.write_text(old.rstrip()+'\n'+section,encoding='utf-8')
    print('Catalogued 15 runtime sheets, 15 clips, 4 references and 20 sources')

if __name__=='__main__':main()
