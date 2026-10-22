"""Reproduce approved-scope exact pixel preparation; raw ImageGen sources are never modified."""
from pathlib import Path
import json, hashlib, math
import numpy as np
from PIL import Image, ImageDraw

ROOT=Path(__file__).resolve().parents[2]
ART=ROOT/'Assets/Game/Art/Effects'
SRC=ART/'SourceSheets/p3b_core_001'
OUT=ROOT/'Docs/Validation/Phase3B'
OUT.mkdir(parents=True,exist_ok=True)
# id, source, source revision, count, logical frame, folder, fps, channel, layer, order, lifetime, opacity, fade
ROWS=[
 ('MgMuzzle','mg_muzzle',2,4,32,'Combat/MachineGun',48,'Burst','Effects',10,0,1,.15),
 ('SniperMuzzle','sniper_muzzle',1,3,48,'Combat/Sniper',40,'Burst','Effects',10,0,1,.15),
 ('ImpactDirt','impact_dirt',1,4,32,'Combat/Impacts',20,'Burst','Effects',3,0,.85,.4),
 ('ImpactMud','impact_mud',1,4,32,'Combat/Impacts',20,'Burst','Effects',3,0,.9,.4),
 ('ImpactConcrete','impact_concrete',1,4,32,'Combat/Impacts',24,'Burst','Effects',3,0,.9,.4),
 ('ImpactMetal','impact_metal',1,4,32,'Combat/Impacts',24,'Burst','Effects',3,0,1,.4),
 ('ImpactFlesh','impact_flesh',1,4,32,'Combat/Impacts',24,'Burst','Effects',3,0,.7,.4),
 ('BloodHit','blood_hit',1,4,32,'Blood/Hits',20,'Burst','Effects',2,0,.9,.35),
 ('BloodDecal','blood_decal',1,1,32,'Blood/Death',1,'Decal','GroundObjects',0,12,.7,.5),
 ('BloodDecalMedium','blood_decal_medium',1,1,48,'Blood/Death',1,'Decal','GroundObjects',0,12,.65,.5),
 ('MineExplosion','mine_explosion',1,8,128,'Explosions/Mine',16,'Burst','Effects',8,0,.95,.4),
 ('ExplosionSmall','explosion_small',1,6,64,'Explosions/Generic',18,'Burst','Effects',8,0,.9,.4),
 ('SmokeSmall','smoke_small',1,6,48,'Smoke',6,'Smoke','Effects',1,0,.32,.5),
 ('SmokeMedium','smoke_medium',1,6,64,'Smoke',6,'Smoke','Effects',1,0,.28,.6),
 ('SplashSmall','water_splash_small',1,6,48,'Water/Splash',16,'Burst','GroundObjects',3,0,.75,.5),
 ('SplashMedium','water_splash_medium',2,6,64,'Water/Splash',16,'Burst','GroundObjects',3,0,.7,.5),
 ('SplashLarge','water_splash_large',1,6,96,'Water/Splash',14,'Burst','GroundObjects',3,0,.65,.5),
 ('WaterWake','water_wake',1,4,48,'Water/Wake',6,'Burst','GroundObjects',2,0,.45,.4),
 ('WaterRipple','water_ripple',1,6,64,'World',5,'Burst','Environment',6,0,.28,.6),
 ('ShoreFoam','shore_foam',1,4,64,'World',3,'Burst','Environment',8,0,.2,.4),
]

def key(im):
    a=np.array(im.convert('RGBA')).copy();r,g,b=[a[:,:,i].astype(int) for i in range(3)]
    # Remove the flat key and its dark/desaturated spill, including holes within smoke/rings.
    matte=(r>g+12)&(b>g+12)&(b>r*.55)
    a[:,:,3]=np.where(matte,0,255);a[a[:,:,3]==0,:3]=0
    return Image.fromarray(a)

def export(row):
    ident,name,rev,count,n,folder,fps,channel,layer,order,life,opacity,fade=row
    source=SRC/f'bd_src_{name}_v{rev:03}.png'
    if not source.exists(): return None
    im=key(Image.open(source));w,h=im.size
    columns=4 if name=='mine_explosion' else count; rows=2 if name=='mine_explosion' else 1
    cw,ch=w/columns,h/rows
    cells=[];centers=[]
    for i in range(count):
        x,y=i%columns,i//columns
        cell=im.crop((round(x*cw),round(y*ch),round((x+1)*cw),round((y+1)*ch)))
        centers.append((cell.width/2,cell.height/2));cells.append(cell)
    if name=='mine_explosion':
        # Measured blast origins; row whitespace is asymmetric in the native montage.
        centers=[(204,280),(198,280),(180,280),(178,280),(204,216),(198,216),(180,216),(178,216)]
    # Use one extent for the entire sequence: expansion is not normalized away.
    extent=0
    for cell,(cx,cy) in zip(cells,centers):
        box=cell.getbbox()
        if box: extent=max(extent,cx-box[0],box[2]-cx,cy-box[1],box[3]-cy)
    side=math.ceil(extent*2/(.80 if n>=64 else .78))
    sheet=Image.new('RGBA',(n*count,n));frames=[];rectangles=[]
    for i,(cell,(cx,cy)) in enumerate(zip(cells,centers)):
        rect=(round(cx-side/2),round(cy-side/2),round(cx-side/2)+side,round(cy-side/2)+side)
        frame=cell.crop(rect).resize((n,n),Image.Resampling.NEAREST)
        # Palette reduction is shared across the complete action below, never dithered.
        frames.append(frame);sheet.paste(frame,(i*n,0));rectangles.append(rect)
    alpha=sheet.getchannel('A');rgb=sheet.convert('RGB').quantize(colors=32,dither=Image.Dither.NONE).convert('RGB')
    sheet=rgb.convert('RGBA');sheet.putalpha(alpha)
    a=np.array(sheet);a[a[:,:,3]==0,:3]=0;sheet=Image.fromarray(a)
    path=ART/folder/f'bd_fx_{name}{"_sheet" if count>1 else ""}_v001.png';path.parent.mkdir(parents=True,exist_ok=True);sheet.save(path)
    frames=[sheet.crop((i*n,0,(i+1)*n,n)) for i in range(count)]
    # Muzzle pivots use actual emission points from the first bright flash after export.
    pivot=[.5,.5]
    if ident in ('MgMuzzle','SniperMuzzle'):
        a=np.array(frames[0]);mask=(a[:,:,0]>200)&(a[:,:,1]>170)&(a[:,:,2]>80)&(a[:,:,3]>0)
        yy,xx=np.where(mask)
        if len(xx):pivot=[round(float(np.median(xx))/n,5),round(1-float(np.median(yy))/n,5)]
    for i,frame in enumerate(frames):
        bg=Image.new('RGBA',(n,n),'#263233' if i%2==0 else '#c2c1ad');bg.alpha_composite(frame)
        bg.resize((n*4,n*4),Image.Resampling.NEAREST).save(OUT/f'{name}-frame-{i:02}.png')
    previews=[]
    for frame in frames:
        bg=Image.new('RGBA',(n,n),'#263233');bg.alpha_composite(frame);previews.append(bg.resize((n*4,n*4),Image.Resampling.NEAREST).convert('RGB'))
    if count>1:previews[0].save(OUT/f'{name}.gif',save_all=True,append_images=previews[1:],duration=round(1000/fps),loop=0)
    return dict(id=ident,name=name,source=str(source.relative_to(ROOT)).replace('\\','/'),sourceSize=[w,h],sourceHash=hashlib.sha256(source.read_bytes()).hexdigest(),
       path=str(path.relative_to(ROOT)).replace('\\','/'),count=count,frame=n,dimensions=list(sheet.size),fps=fps,channel=channel,layer=layer,order=order,lifetime=life,
       opacity=opacity,fade=fade,pivot=pivot,sourceCenters=centers,sourceCropSide=side,crops=rectangles,loop=ident in ('WaterWake','WaterRipple','ShoreFoam'),
       frameBounds=[list(f.getbbox()) if f.getbbox() else None for f in frames])

def tracers():
    # Tiny code-native pixel streaks: geometry, not generated scene/art substitutes.
    result=[]
    for ident,name,folder,height,life in [('MgTracer','mg_tracer','Combat/MachineGun',3,.045),('SniperTracer','sniper_tracer','Combat/Sniper',5,.06)]:
        im=Image.new('RGBA',(64,height));d=ImageDraw.Draw(im);mid=height//2
        d.line((1,mid,62,mid),fill='#fff0b0',width=1)
        if height==5:d.line((9,mid-1,58,mid-1),fill='#ca883e',width=1)
        for x in (4,19,35,51):d.point((x,mid+1),fill='#cd944c')
        path=ART/folder/f'bd_fx_{name}_v001.png';path.parent.mkdir(parents=True,exist_ok=True);im.save(path)
        result.append(dict(id=ident,name=name,source='code-native pixel streak: Docs/Art/export_phase3b.py',path=str(path.relative_to(ROOT)).replace('\\','/'),count=1,frame=64,
          dimensions=list(im.size),fps=1,channel='Tracer',layer='Effects',order=9,lifetime=life,opacity=.8 if height==3 else 1,fade=0,pivot=[.5,.5],loop=False))
    return result

if __name__=='__main__':
    manifest=[x for x in map(export,ROWS) if x]+tracers()
    (ROOT/'Docs/Art/Batches/p3b_manifest.json').write_text(json.dumps(manifest,indent=2)+'\n')
    thumbs=[]
    for item in manifest:
        sheet=Image.open(ROOT/item['path']);n=item['frame'];count=item['count']
        index=min(count-1,1);frame=sheet.crop((index*n,0,(index+1)*n,sheet.height))
        bg=Image.new('RGBA',(150,180),'#263233');frame.thumbnail((140,140),Image.Resampling.NEAREST)
        scale=min(3,140/max(frame.size));frame=frame.resize((round(frame.width*scale),round(frame.height*scale)),Image.Resampling.NEAREST)
        bg.alpha_composite(frame,((150-frame.width)//2,(145-frame.height)//2));ImageDraw.Draw(bg).text((4,154),item['name'],fill='white');thumbs.append(bg)
    contact=Image.new('RGBA',(150*5,180*math.ceil(len(thumbs)/5)),'#182123')
    for i,t in enumerate(thumbs):contact.paste(t,((i%5)*150,(i//5)*180))
    contact.convert('RGB').save(OUT/'runtime-contact.png')
    print('Exported',len(manifest),'sheets/sprites')
