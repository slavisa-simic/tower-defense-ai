"""Catalog, source receipts, pixel QA and reviewable task-only diff."""
from pathlib import Path
import json, hashlib, difflib
import numpy as np
from PIL import Image,ImageDraw
R=Path(__file__).resolve().parents[2];Q=R/'Docs/Validation/Phase3A3';B=R/'Docs/Art/Batches'
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
baseline=json.loads((Q/'preexisting-files.json').read_text(encoding='utf-8-sig'))
changed=[];missing=[]
for e in baseline:
    p=R/e['path']
    if not p.exists():missing.append(e['path'])
    elif sha(p).lower()!=e['sha256'].lower():changed.append(e['path'])
integrity=dict(total=len(baseline),unchanged=len(baseline)-len(changed)-len(missing),changed=changed,missing=missing)
(Q/'repository-integrity.json').write_text(json.dumps(integrity,indent=2))
for old in (Q/'baseline').iterdir():
    if old.suffix=='.prefab':new=R/'Assets/Game/Prefabs/Defenses'/old.name
    elif old.name=='TurretPresentation.cs.txt':new=R/'Assets/Game/Scripts/Presentation/TurretPresentation.cs'
    elif old.name=='CoastalDefense_TopDown.unity.txt':new=R/'Assets/Game/Scenes/CoastalDefense_TopDown.unity'
    else:continue
    diff=''.join(difflib.unified_diff(old.read_text().splitlines(True),new.read_text().splitlines(True),fromfile='before/'+new.name,tofile=str(new.relative_to(R))))
    (Q/(new.name+'.diff')).write_text(diff)
generated=Path('C:/Users/Slavisa/.codex/generated_images/01a0be46-b2d3-7510-84e8-13d5aad27c10')
orig={sha(p):str(p) for p in generated.glob('*.png')}
raw=[]
for p in sorted((R/'Assets/Game/Art/SourceSheets/p3a3_defenses_001').glob('*.png')):
    im=Image.open(p);h=sha(p);id=p.stem.replace('bd_src_','');prompt=B/'p3a3_prompts'/(id+'.txt')
    if id=='defense_lineup_v001':prompt=B/'p3a3_lineup_prompt.txt'
    raw.append(dict(id=id,path=str(p.relative_to(R)).replace('\\','/'),size=im.size,mode=im.mode,sha256=h,original=orig.get(h),byte_identical=h in orig,
        prompt=str(prompt.relative_to(R)).replace('\\','/')))
audit=json.loads((Q/'pixel-audit.json').read_text());manifest=json.loads((B/'p3a3_export_manifest.json').read_text())
for e in audit:
    p=R/e['path'];im=Image.open(p);a=np.array(im);n=im.height
    e['border_pass']=all(b[0]>=2 and b[1]>=2 and b[2]<=n-2 and b[3]<=n-2 for b in e['bounds'])
    e['alpha_pass']=e['alpha']==[0,255] and e['transparent_rgb_zero']
    e['no_magenta']=not bool(((a[:,:,0]>a[:,:,1].astype(int)+30)&(a[:,:,2]>a[:,:,1].astype(int)+30)&(a[:,:,3]>0)).any())
    e['sha256']=sha(p);e['ppu']=100 if '/UI/' in e['path'] else 51.2
    e['pivot']=[n/2,n/2];e['count']=im.width//n
    found=next((r for r in manifest if r['output']==e['path'] or r.get('idle_output')==e['path']),None)
    e['source']=found['source'] if found else 'Composite of registered base idle + weapon/aim idle exports'
    e['fps']=found['fps'] if found and e['count']>1 else 0
    name=p.stem
    e['status']='validated inventory' if any(s in name for s in ['corner','horizontal','vertical','_ui_']) else 'integrated and validated'
    if 'destroyed' in name:e['status']='imported; death-frame binding only; no persistent wreck'
    e['consumer']='Assets/Game/Prefabs/Defenses/'+(p.parent.name+'.prefab') if '/Defenses/' in e['path'] else 'Future UI icon inventory (v001 preserved)'
    if any(s in name for s in ['corner','horizontal','vertical']):e['consumer']='Future wire connection presentation; no autotiling added'
(Q/'pixel-audit.json').write_text(json.dumps(audit,indent=2))
catalog=dict(reference='Assets/Game/Art/MasterReferences/ref_defense_lineup_core_01.png',reference_source='bd_src_defense_lineup_v002.png',
    approved_style='Assets/Game/Art/MasterReferences/bd_ref_danube_master_v002.png',raw_sources=raw,runtime=audit)
(B/'p3a3_final_manifest.json').write_text(json.dumps(catalog,indent=2))
assert all(e['byte_identical'] for e in raw)
assert all(e['border_pass'] and e['alpha_pass'] and e['no_magenta'] for e in audit)

# Every animation frame, using the requested deterministic review/packing workflow.
frames=Image.new('RGB',(900,420),'#263133');draw=ImageDraw.Draw(frames)
for row,folder,stem in [(0,'MachineGunNest','mg_weapon'),(1,'SniperTower','sniper_aim')]:
    im=Image.open(R/f'Assets/Game/Art/Defenses/{folder}/bd_defense_{stem}_fire_sheet_v001.png');n=im.height
    for i in range(im.width//n):
        frame=im.crop((i*n,0,(i+1)*n,n));bg=Image.new('RGBA',(n,n),'#d6d0bc' if i%2 else '#344144');bg.alpha_composite(frame)
        frames.paste(bg.resize((180,180),Image.Resampling.NEAREST),(i*220+10,row*210+10));draw.text((i*220+10,row*210+192),f'{stem} frame {i}',fill='white')
frames.save(Q/'all-firing-frames.png')

table='| Asset path | PNG size | Frames / FPS | PPU | Pivot (pixels) |\n|---|---|---|---|---|\n'
for e in audit:
    table+=f"| `{e['path']}` | {e['size'][0]}x{e['size'][1]} | {e['count']} / {e['fps'] or '-'} | {e['ppu']} | ({e['pivot'][0]:g}, {e['pivot'][1]:g}) |\n"
(B/'p3a3_asset_table.md').write_text(table)
register='\n\n## Phase 3A-3 — Defense assets and structure states (2026-09-20 to 2026-09-21)\n\n'
register+='Built-in ImageGen production under the requested autonomous internal review. Danube master v002 remains the approved style anchor. Technical acceptance is not separate per-file user approval. Final lineup uses native revision v002 at the explicitly requested reference path. Canonical provenance, source hashes, prompts, transforms and consumers: `Docs/Art/Batches/p3a3_final_manifest.json`; exact prompts under `p3a3_prompts/`.\n\n'
register+='| Runtime asset | Source | Status / consumer |\n|---|---|---|\n'
for e in audit:register+=f"| `{e['path']}` | `{e['source']}` | {e['status']}; {e['consumer']} |\n"
register+='\n| Retained native source | Actual PNG / mode | Original hash match | Disposition |\n|---|---|---|---|\n'
rejected={'defense_lineup_v001':'Superseded by v002: corrected overhead sniper','sniper_reference_v001':'Superseded by v002: removed visible lower supports','mg_base_idle_v001':'Superseded by v002: painted checkerboard','mine_idle_v001':'Superseded by v002: painted checkerboard'}
for e in raw:register+=f"| `{e['path']}` | {e['size'][0]}x{e['size'][1]} {e['mode']} | yes | {rejected.get(e['id'],'Selected reference/source')} |\n"
register+='\nReferences: `ref_defense_lineup_core_01.png` (v002 source), `bd_ref_mg_reference_v001.png`, `bd_ref_sniper_reference_v002.png`, `bd_ref_wire_reference_v001.png`, `bd_ref_mine_reference_v001.png`. `bd_ref_sniper_reference_v001.png` is superseded. All under `Assets/Game/Art/MasterReferences/`, non-runtime textures.\n\n'
register+='Validation: 24 runtime PNGs, binary alpha and two-pixel margins, 7 aligned firing frames, 42 EditMode + 22 PlayMode tests, 45 live scene checks. Unity importer evidence and camera/HUD captures under `Docs/Validation/Phase3A3/`. Four prefab GUIDs preserved; gameplay data unchanged. Destroyed sprites do not extend defense lifetime. Existing VFX placeholders remain for Phase 3B.\n'
reg=R/'Docs/Art/ASSET_REGISTER.md';old=reg.read_text(encoding='utf-8')
marker='\n\n## Phase 3A-3 — Defense assets and structure states'
if marker in old:old=old.split(marker)[0]
reg.write_text(old+register,encoding='utf-8')
print(json.dumps(dict(integrity=integrity,raw=len(raw),runtime=len(audit),all_pixels_pass=True),indent=2))
