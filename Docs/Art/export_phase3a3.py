"""Authorized exact-pixel export: retained ImageGen sources -> registered RGBA sprites."""
from pathlib import Path
import json, hashlib
import numpy as np
from PIL import Image, ImageDraw

ROOT = Path(__file__).resolve().parents[2]
SRC = ROOT/'Assets/Game/Art/SourceSheets/p3a3_defenses_001'
OUT = ROOT/'Assets/Game/Art/Defenses'
QA = ROOT/'Docs/Validation/Phase3A3'
entries=[]

def source(id):
    im=Image.open(SRC/f'bd_src_{id}.png').convert('RGBA')
    a=np.array(im); rgb=a[:,:,:3].astype(int)
    mag=(rgb[:,:,0]>rgb[:,:,1]+30)&(rgb[:,:,2]>rgb[:,:,1]+30)&(rgb[:,:,0]>50)&(rgb[:,:,2]>50)
    if mag.mean()>.05:
        remove=mag
    else:
        neutral=(rgb.max(2)-rgb.min(2)<28)&(rgb.min(2)>155)
        mask=Image.fromarray((neutral*255).astype('uint8'))
        for xy in [(0,0),(im.width-1,0),(0,im.height-1),(im.width-1,im.height-1)]:
            if mask.getpixel(xy)==255:ImageDraw.floodfill(mask,xy,128)
        remove=np.array(mask)==128
    a[remove]=0; a[~remove,3]=255
    return Image.fromarray(a)

def registered(im,n,scale,pivot,target=None):
    target=target or (n/2,n/2)
    # Exact affine nearest sampling; same transform for all structure states.
    return im.transform((n,n),Image.Transform.AFFINE,(1/scale,0,pivot[0]-target[0]/scale,0,1/scale,pivot[1]-target[1]/scale),Image.Resampling.NEAREST)

def add(id,folder,name,n,scale,pivot,count=1,frame_pivots=None,fps=0,target=None,ui=False):
    im=source(id)
    pivots=frame_pivots or [pivot]
    frames=[]
    for i,p in enumerate(pivots):
        cell=im
        if count>1:
            x0=round(i*im.width/count);x1=round((i+1)*im.width/count)
            cell=Image.new('RGBA',im.size);cell.paste(im.crop((x0,0,x1,im.height)),(x0,0))
        frames.append(registered(cell,n,scale,p,target))
    path=(ROOT/'Assets/Game/Art/UI'/name) if ui else OUT/folder/name
    entries.append(dict(id=id,source=str((SRC/f'bd_src_{id}.png').relative_to(ROOT)).replace('\\','/'),
        output=str(path.relative_to(ROOT)).replace('\\','/'),frame=n,count=count,ppu=100 if ui else 51.2,
        scale=scale,source_pivots=pivots,target_pivot=target or [n/2,n/2],fps=fps,frames=frames))

for state in ['idle','damaged','destroyed']:
    add('mg_base_'+state+('_v002' if state=='idle' else '_v001'),'MachineGunNest','bd_defense_mg_base_'+state+'_v001.png',64,.05625,(631,627))
    add('sniper_base_'+state+'_v001','SniperTower','bd_defense_sniper_base_'+state+'_v001.png',96,58/747,(626,578))
add('mg_weapon_fire_sheet_v001','MachineGunNest','bd_defense_mg_weapon_fire_sheet_v001.png',64,40/420,(211,347),4,[(211,347),(708,347),(1204,347),(1702,347)],20)
add('sniper_aim_fire_sheet_v001','SniperTower','bd_defense_sniper_aim_fire_sheet_v001.png',96,46/584,(247,350),3,[(247,350),(971,350),(1695,350)],12)
for state in ['single','damaged','destroyed']:
    add('wire_'+state+'_v001','BarbedWire','bd_defense_wire_'+state+'_v001.png',64,58/995,(873,444))
add('wire_horizontal_v001','BarbedWire','bd_defense_wire_horizontal_v001.png',64,60/1576,(952,409))
add('wire_vertical_v001','BarbedWire','bd_defense_wire_vertical_v001.png',64,60/1434,(512,756))
# Endpoints register to north/east/west/south at 2/62 with a four-pixel join overlap in future placement.
for d,p in [('ne',(445,820)),('nw',(802,820)),('se',(445,411)),('sw',(816,411))]:
    add('wire_corner_'+d+'_v001','BarbedWire','bd_defense_wire_corner_'+d+'_v001.png',64,(29.5 if d=='sw' else 30)/720,p)
for state in ['idle','armed']:
    add('mine_'+state+('_v002' if state=='idle' else '_v001'),'LandMine','bd_defense_mine_'+state+'_v001.png',32,24/768,(646,593))
add('sniper_icon_v002','','bd_ui_sniper_tower_icon_v002.png',64,56/777,(649,579),ui=True)

# One palette for the whole pack, no dithering, only sampled opaque pixel colors.
pixels=np.concatenate([np.array(f)[:,:,:3][np.array(f)[:,:,3]>0] for e in entries for f in e['frames']])
palette=Image.fromarray(pixels.reshape(1,-1,3)).quantize(colors=40,method=Image.Quantize.MEDIANCUT,dither=Image.Dither.NONE)
anchors=[(193,138,62),(240,179,72),(31,41,44),(90,97,93),(193,177,158),(99,107,68),(127,72,51),(147,148,134)]
pal=palette.getpalette()[:120]+[c for color in anchors for c in color]
palette.putpalette(pal+[0]*(768-len(pal)))
palette.save(QA/'palette.png')
for e in entries:
    frames=[]
    for f in e.pop('frames'):
        alpha=f.getchannel('A');f=f.convert('RGB').quantize(palette=palette,dither=Image.Dither.NONE).convert('RGBA');f.putalpha(alpha)
        a=np.array(f);a[a[:,:,3]==0]=0;frames.append(Image.fromarray(a))
    sheet=Image.new('RGBA',(e['frame']*e['count'],e['frame']))
    for i,f in enumerate(frames):sheet.paste(f,(i*e['frame'],0))
    p=ROOT/e['output'];p.parent.mkdir(parents=True,exist_ok=True);sheet.save(p)
    e['bounds']=[f.getbbox() for f in frames];e['sha256']=hashlib.sha256(p.read_bytes()).hexdigest()
    if e['count']>1:
        idle=e['output'].replace('_fire_sheet_','_idle_');frames[0].save(ROOT/idle)
        e['idle_output']=idle
        # Review GIF retains transparent frame silhouette on a flat neutral backing.
        review=[]
        for f in frames:
            bg=Image.new('RGBA',f.size,'#303b3c');bg.alpha_composite(f);review.append(bg.convert('RGB').resize((f.width*5,f.height*5),Image.Resampling.NEAREST))
        review[0].save(QA/(e['id']+'.gif'),save_all=True,append_images=review[1:],duration=150,loop=0)

# Placement previews are the exact authored layers, composed at their shared pivot.
for folder,id,n,layer in [('MachineGunNest','mg',64,'weapon'),('SniperTower','sniper',96,'aim')]:
    base=Image.open(OUT/folder/f'bd_defense_{id}_base_idle_v001.png').convert('RGBA')
    top=Image.open(OUT/folder/f'bd_defense_{id}_{layer}_idle_v001.png').convert('RGBA')
    base.alpha_composite(top);base.save(OUT/folder/f'bd_defense_{id}_preview_v001.png')

(ROOT/'Docs/Art/Batches/p3a3_export_manifest.json').write_text(json.dumps(entries,indent=2))
files=sorted(OUT.glob('*/*.png'))+[ROOT/'Assets/Game/Art/UI/bd_ui_sniper_tower_icon_v002.png']
contact=Image.new('RGB',(1000,((len(files)+4)//5)*230),'#202a2c');draw=ImageDraw.Draw(contact)
audit=[]
for i,p in enumerate(files):
    im=Image.open(p).convert('RGBA'); a=np.array(im); n=im.height;frames=[im.crop((j*n,0,(j+1)*n,n)) for j in range(im.width//n)]
    x=(i%5)*200;y=(i//5)*230
    f=frames[0];bg=Image.new('RGBA',f.size,'#d2d0c4' if i%2 else '#303b3c');bg.alpha_composite(f)
    bg=bg.resize((180,180),Image.Resampling.NEAREST);contact.paste(bg,(x+10,y+8));draw.text((x+5,y+191),p.stem.replace('bd_defense_','').replace('_v001',''),fill='white')
    audit.append(dict(path=str(p.relative_to(ROOT)).replace('\\','/'),size=im.size,alpha=sorted(map(int,np.unique(a[:,:,3]))),transparent_rgb_zero=bool((a[a[:,:,3]==0,:3]==0).all()),bounds=[f.getbbox() for f in frames]))
contact.save(QA/'runtime-contact.png');(QA/'pixel-audit.json').write_text(json.dumps(audit,indent=2))
print('Exported',len(files),'runtime PNGs; audit and contact preview saved.')
