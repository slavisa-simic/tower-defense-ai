"""Deterministic preparation of retained ImageGen sources; never creates artwork.

Source grid extraction, magenta key, one constant scale per action, fixed-size
transparent canvases, shared no-dither palette, review strips and receipts.
Explicit reviewed source pivots/scales can override the initial estimates.
"""
from pathlib import Path
import argparse, hashlib, json, math
import numpy as np
from PIL import Image, ImageDraw

ROOT = Path(__file__).resolve().parents[2]
MANIFEST = ROOT / 'Docs/Art/Batches/p3a2_animation_manifest.json'
VALIDATION = ROOT / 'Docs/Validation/Phase3A2'
PALETTE = [
    '14191a','1f292c','292d2b','353930','414733','4b5538','636b44','7a8055',
    '3b3028','4b372d','5a4633','6b503b','816445','987750','ae8e61','c1a87c',
    'c1b19e','d2bea0','e0caa6','939486','73766a','5a615d','444d4c','323d42',
    '3f505a','52636b','657982','7c8c90','493023','603a28','75482f','895738',
    '9d6944','b47e52','c18e62','d2a275','542e27','6c382b','7f4833','934433',
    'a65c3f','bc7650','66594c','7d6d59','9d8b71','b3a088','a77836','c18a3e'
]

def hash_file(p):
    return hashlib.sha256(p.read_bytes()).hexdigest()

def key_source(img):
    a = np.array(img.convert('RGBA'))
    rgb = a[:,:,:3].astype(np.int16)
    r,g,b = rgb[:,:,0], rgb[:,:,1], rgb[:,:,2]
    key = (r > g + 25) & (b > g + 25) & (r > 1.3*g + 12) & (b > 1.3*g + 12)
    # Every source sheet must actually use the declared magenta backing.
    if key.mean() < .2:
        raise ValueError('Expected keyed source backing; inspect source before export')
    a[:,:,3] = np.where(key, 0, np.where(a[:,:,3] >= 128,255,0))
    a[a[:,:,3] == 0] = 0
    return Image.fromarray(a)

def palette_image():
    p=Image.new('P',(1,1))
    values=[int(c[i:i+2],16) for c in PALETTE for i in (0,2,4)]
    p.putpalette(values + values[-3:]*(256-len(PALETTE)))
    return p

def export(entry):
    source=ROOT/entry['source']
    img=Image.open(source)
    clean=key_source(img)
    fw=clean.width/entry['cols']; fh=clean.height/entry['rows']
    crops=[]; boxes=[]
    for i in range(entry['count']):
        x=i%entry['cols']; y=i//entry['cols']
        rect=entry.get('sourceRects',[])
        rect=tuple(rect[i]) if rect else (round(x*fw),round(y*fh),round((x+1)*fw),round((y+1)*fh))
        crop=clean.crop(rect); box=crop.getbbox()
        if not box: raise ValueError(f'Empty frame {i}: {source}')
        if min(box[0],box[1],crop.width-box[2],crop.height-box[3]) < 3:
            raise ValueError(f'Source frame crosses grid edge: {source}, frame {i}, {box}')
        crops.append(crop);boxes.append(box)
    # One scale for the complete action, never independently fitting frames.
    median_w=float(np.median([b[2]-b[0] for b in boxes[:4]]))
    scale=entry.get('exportScale',entry['body'][0]/median_w)
    n=entry['frame']; sheet=Image.new('RGBA',(n*entry['count'],n))
    frames=[]; details=[]
    for i,(crop,box) in enumerate(zip(crops,boxes)):
        pivot=entry.get('sourcePivots',[])
        if pivot: pivot=pivot[i]
        else: pivot=[(box[0]+box[2])/2,(box[1]+box[3])/2]
        art=crop.crop(box)
        size=(max(1,round(art.width*scale)),max(1,round(art.height*scale)))
        art=art.resize(size,Image.Resampling.NEAREST)
        alpha=art.getchannel('A')
        art=art.convert('RGB').quantize(palette=palette_image(),dither=Image.Dither.NONE).convert('RGBA')
        art.putalpha(alpha)
        pos=(round(n/2+(box[0]-pivot[0])*scale),round(n/2+(box[1]-pivot[1])*scale))
        if min(*pos,n-pos[0]-size[0],n-pos[1]-size[1])<2:
            raise ValueError(f'Logical frame clipping: {source} {i} {pos} {size}')
        frame=Image.new('RGBA',(n,n));frame.alpha_composite(art,pos)
        arr=np.array(frame);arr[arr[:,:,3]==0]=0;frame=Image.fromarray(arr)
        frames.append(frame);sheet.alpha_composite(frame,(i*n,0))
        bounds=frame.getbbox()
        details.append({'frame':i,'sourceCell':list(crop.size),'sourceBounds':list(box),'sourcePivot':pivot,'rect':[i*n,0,n,n],
            'name':f"bd_enemy_{entry['id']}_{entry['action']}_e__f{i:03d}",'bounds':list(bounds),
            'opaquePixels':int((arr[:,:,3]>0).sum()),'sha256':hashlib.sha256(frame.tobytes()).hexdigest()})
    output=ROOT/entry['output'];output.parent.mkdir(parents=True,exist_ok=True);sheet.save(output)
    # QA only: every logical frame at 4x, on alternating light/dark panels.
    zoom=4; panel_w=n*zoom; label_h=24
    review=Image.new('RGB',(panel_w*entry['count'],panel_w*2+label_h),'#202929')
    draw=ImageDraw.Draw(review)
    for i,frame in enumerate(frames):
        draw.text((i*panel_w+5,5),f"{entry['id']} {entry['action']} {i:02d}",fill='#eeeecc')
        for row,color in enumerate(['#e1d7c0','#141b1b']):
            backing=Image.new('RGBA',(n,n),color);backing.alpha_composite(frame)
            review.paste(backing.convert('RGB').resize((panel_w,panel_w),Image.Resampling.NEAREST),(i*panel_w,label_h+row*panel_w))
    review.save(VALIDATION/f"{entry['id']}_{entry['action']}_frames.png")
    # Animated review (no runtime dependency).
    gif=[]
    for f in frames:
        tile=Image.new('RGBA',(n,n),'#4b5538');tile.alpha_composite(f)
        gif.append(tile.convert('RGB').resize((n*6,n*6),Image.Resampling.NEAREST))
    gif[0].save(VALIDATION/f"{entry['id']}_{entry['action']}.gif",save_all=True,append_images=gif[1:],
        duration=round(1000/entry['fps']),loop=0 if entry['loop'] else 1,disposal=2)
    return {'id':entry['id'],'action':entry['action'],'source':entry['source'],'output':entry['output'],
        'sourceSize':list(img.size),'sourceMode':img.mode,'sourceSHA256':hash_file(source),
        'size':list(sheet.size),'mode':sheet.mode,'exportScale':scale,'scaleMethod':'constant-per-action',
        'outputSHA256':hash_file(output),'ppu':51.2,'pivot':[.5,.5],'fps':entry['fps'],'loop':entry['loop'],
        'alphaValues':list(np.unique(np.array(sheet)[:,:,3]).astype(int).tolist()),'frames':details}

def main():
    parser=argparse.ArgumentParser();parser.add_argument('--enemy');args=parser.parse_args()
    VALIDATION.mkdir(parents=True,exist_ok=True)
    entries=json.loads(MANIFEST.read_text(encoding='utf-8'))
    results=[]
    for entry in entries:
        if args.enemy and entry['id']!=args.enemy: continue
        if not (ROOT/entry['source']).exists(): continue
        results.append(export(entry))
    name=f"exports-{args.enemy or 'all'}.json"
    (VALIDATION/name).write_text(json.dumps(results,indent=2),encoding='utf-8')
    print(json.dumps([{'enemy':r['id'],'action':r['action'],'size':r['size'],'scale':r['exportScale'],
        'frameBounds':[f['bounds'] for f in r['frames']]} for r in results],indent=2))

if __name__=='__main__': main()
