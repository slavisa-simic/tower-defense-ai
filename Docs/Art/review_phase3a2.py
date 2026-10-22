"""Contact/readability views and independent pixel/provenance checks."""
import json,hashlib
from pathlib import Path
import numpy as np
from PIL import Image,ImageDraw
from export_phase3a2 import ROOT,MANIFEST,VALIDATION

def digest(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def main():
    entries=json.loads(MANIFEST.read_text());audits=[]
    for enemy in ['basic_infected','runner','brute']:
        group=[e for e in entries if e['id']==enemy];n=group[0]['frame'];zoom=3 if n==64 else 2
        panel=n*zoom;view=Image.new('RGB',(panel*8,panel*5+30*5),'#1f292c');draw=ImageDraw.Draw(view)
        for row,e in enumerate(group):
            img=Image.open(ROOT/e['output']).convert('RGBA');arr=np.array(img)
            assert img.size==(e['count']*n,n)
            assert set(np.unique(arr[:,:,3]))=={0,255}
            assert not (arr[arr[:,:,3]==0,:3]!=0).any()
            rgb=arr[:,:,:3].astype(int);opaque=arr[:,:,3]>0
            assert not (((rgb[:,:,0]>rgb[:,:,1]+25)&(rgb[:,:,2]>rgb[:,:,1]+25))&opaque).any()
            bounds=[];frame_hashes=[]
            draw.text((8,row*(panel+30)+7),f"{enemy.upper()} / {e['action'].upper()} / {e['fps']} FPS / {e['count']} FRAMES",fill='#c1b19e')
            for i in range(e['count']):
                frame=img.crop((i*n,0,(i+1)*n,n));box=frame.getbbox();bounds.append(box)
                assert box and min(box[0],box[1],n-box[2],n-box[3])>=2
                frame_hashes.append(hashlib.sha256(frame.tobytes()).hexdigest())
                tile=Image.new('RGBA',(n,n),'#e1d7c0' if i%2==0 else '#17201f');tile.alpha_composite(frame)
                view.paste(tile.convert('RGB').resize((panel,panel),Image.Resampling.NEAREST),(i*panel,row*(panel+30)+30))
            assert len(set(frame_hashes))==e['count'],(enemy,e['action'],'duplicate frames')
            audits.append({'output':e['output'],'size':img.size,'rgba':True,'binaryAlpha':True,'transparentRGBZero':True,
                'magentaResiduePixels':0,'uniqueFrames':len(set(frame_hashes)),'bounds':bounds,
                'minimumMargin':min(min(b[0],b[1],n-b[2],n-b[3]) for b in bounds),'sha256':digest(ROOT/e['output'])})
        view.save(VALIDATION/f'{enemy}_contact.png')
    # Scale estimates use the unchanged camera, explicit PPU and existing tiles.
    tiles=[Image.open(ROOT/f'Assets/Game/Art/Environment/bd_env_{t}_tile_v001.png').convert('RGBA')
        for t in ['grass','muddy_shore','concrete']]
    for height in [1080,1440]:
        ratio=height/(2*11.85*51.2);canvas=Image.new('RGBA',(720,400),'#1f292c');draw=ImageDraw.Draw(canvas)
        draw.text((16,10),f'CAMERA-SCALE ESTIMATE: {height}p / {ratio:.3f} screen px per logical px',fill='#c1b19e')
        for row,tile in enumerate(tiles):
            field=Image.new('RGBA',(600,96))
            for x in range(0,600,64):
                for y in range(0,96,64):field.paste(tile,(x,y))
            for col,enemy in enumerate(['basic_infected','runner','brute']):
                e=next(e for e in entries if e['id']==enemy and e['action']=='idle');n=e['frame']
                frame=Image.open(ROOT/e['output']).crop((0,0,n,n)).convert('RGBA')
                field.alpha_composite(frame,(col*190+70-n//2,48-n//2))
            scaled=field.resize((round(field.width*ratio),round(field.height*ratio)),Image.Resampling.NEAREST)
            canvas.alpha_composite(scaled,(4,40+row*118))
        canvas.convert('RGB').save(VALIDATION/f'gameplay-scale-{height}.png')
    # Shared locomotion preview, preserving relative world scale at 3x.
    animation=[]
    moves=[next(e for e in entries if e['id']==enemy and e['action'] in ['walk','run'])
        for enemy in ['basic_infected','runner','brute']]
    sources=[Image.open(ROOT/e['output']).convert('RGBA') for e in moves]
    for tick in range(48):
        board=Image.new('RGBA',(288,112),'#1f292c')
        for x in range(0,288,64):
            for y in range(16,112,64):board.paste(tiles[0],(x,y))
        for col,(e,source) in enumerate(zip(moves,sources)):
            n=e['frame'];index=int(tick*e['fps']/12)%e['count']
            board.alpha_composite(source.crop((index*n,0,(index+1)*n,n)),(col*96+(96-n)//2,16+(96-n)//2))
        frame=board.convert('RGB').resize((864,336),Image.Resampling.NEAREST)
        draw=ImageDraw.Draw(frame)
        for col,name in enumerate(['BASIC INFECTED - 8 FPS','RUNNER - 12 FPS','BRUTE - 6 FPS']):draw.text((col*288+12,14),name,fill='#c1b19e')
        animation.append(frame)
    animation[0].save(VALIDATION/'locomotion-preview.gif',save_all=True,append_images=animation[1:],duration=83,loop=0,disposal=2)
    originals=Path('C:/Users/Slavisa/.codex/generated_images/01a0bb8a-d7b8-73e3-85a2-c47563f6e0d6')
    lookup={digest(p):str(p) for p in originals.glob('*.png')}
    provenance=[]
    for p in sorted((ROOT/'Assets/Game/Art/SourceSheets/p3a2_core_enemies_001').glob('*.png')):
        h=digest(p);im=Image.open(p);assert h in lookup,('Changed native source',p)
        provenance.append({'source':p.relative_to(ROOT).as_posix(),'generated':lookup[h],'size':im.size,'mode':im.mode,'sha256':h,'byteIdentical':True})
    (VALIDATION/'pixel-audit.json').write_text(json.dumps({'sheets':audits,'nativeSources':provenance},indent=2))
    print(f'{len(audits)} sheets / {sum(e["count"] for e in entries)} unique action frames passed; {len(provenance)} native sources unchanged')

if __name__=='__main__':main()
