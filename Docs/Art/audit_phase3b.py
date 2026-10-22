from pathlib import Path
import json, hashlib, math
from PIL import Image, ImageDraw
import numpy as np
ROOT=Path(__file__).resolve().parents[2];OUT=ROOT/'Docs/Validation/Phase3B'
manifest=json.loads((ROOT/'Docs/Art/Batches/p3b_manifest.json').read_text())
audit=[];reviews=[[],[]]
for entry in manifest:
    im=Image.open(ROOT/entry['path']);a=np.array(im);n=entry['frame'];count=entry['count']
    has_alpha=im.mode=='RGBA';binary=set(np.unique(a[:,:,3])).issubset({0,255});zero=not np.any(a[a[:,:,3]==0,:3])
    rgb=a[:,:,:3].astype(int);chroma=(rgb[:,:,0]>rgb[:,:,1]+12)&(rgb[:,:,2]>rgb[:,:,1]+12)&(rgb[:,:,2]>rgb[:,:,0]*.55)&(a[:,:,3]>0)
    bounds=[im.crop((i*n,0,min((i+1)*n,im.width),im.height)).getbbox() for i in range(count)]
    margin=1 if entry['channel']=='Tracer' else 2
    frames_clean=all(b is not None and b[0]>=margin and b[2]<=min(n,im.width)-margin and
        (entry['channel']=='Tracer' or (b[1]>=margin and b[3]<=im.height-margin)) for b in bounds)
    audit.append(dict(path=entry['path'],size=list(im.size),expectedSize=entry['dimensions'],rgba=has_alpha,binaryAlpha=binary,transparentRgbZero=zero,chromaPixels=int(chroma.sum()),frameBounds=bounds,framesClean=frames_clean,
        sha256=hashlib.sha256((ROOT/entry['path']).read_bytes()).hexdigest()))
    row=Image.new('RGB',(1060,148),'#182326');ImageDraw.Draw(row).text((8,5),entry['name'],fill='white')
    for i in range(count):
        f=im.crop((i*n,0,(i+1)*n,im.height));scale=min(3,120/max(f.size));f=f.resize((round(f.width*scale),round(f.height*scale)),Image.Resampling.NEAREST)
        bg=Image.new('RGBA',(128,120),'#d1cfbf' if i%2 else '#263233');bg.alpha_composite(f,((128-f.width)//2,(120-f.height)//2));row.paste(bg,(i*132,24))
    reviews[1 if entry['id'].startswith(('Smoke','Splash','Water','Shore')) else 0].append(row)
for i,rows in enumerate(reviews):
    im=Image.new('RGB',(1060,len(rows)*148),'#182326')
    for j,row in enumerate(rows):im.paste(row,(0,j*148))
    im.save(OUT/('frame-review-world.png' if i else 'frame-review-combat.png'))
(OUT/'pixel-audit.json').write_text(json.dumps(audit,indent=2)+'\n')
# Compare only the task's preexisting project files; baseline predates integration.
baseline=json.loads((OUT/'baseline-hashes.json').read_text(encoding='utf-8-sig'))
changed=[];missing=[]
for item in baseline:
    p=ROOT/item['path']
    if not p.exists():missing.append(item['path'])
    elif hashlib.sha256(p.read_bytes()).hexdigest().lower()!=item['hash'].lower():changed.append(item['path'])
(OUT/'repository-integrity.json').write_text(json.dumps(dict(baseline=len(baseline),unchanged=len(baseline)-len(changed)-len(missing),changed=changed,missing=missing),indent=2)+'\n')
# Receipts are local source provenance, not approval claims.
generation=Path.home()/'.codex/generated_images/01a0c311-10b8-70e0-b29b-69dd1264aba6'
originals={hashlib.sha256(p.read_bytes()).hexdigest():str(p) for p in generation.glob('*.png')}
sources=[]
for p in (ROOT/'Assets/Game/Art/Effects/SourceSheets/p3b_core_001').glob('*.png'):
    digest=hashlib.sha256(p.read_bytes()).hexdigest()
    sources.append(dict(path=str(p.relative_to(ROOT)).replace('\\','/'),sha256=digest,dimensions=list(Image.open(p).size),original=originals.get(digest),matchesOriginal=digest in originals))
(OUT/'source-receipts.json').write_text(json.dumps(sources,indent=2)+'\n')
assert all(x['size']==x['expectedSize'] and x['rgba'] and x['binaryAlpha'] and x['transparentRgbZero'] and x['chromaPixels']==0 and x['framesClean'] for x in audit)
print(json.dumps(dict(runtime=len(audit),sources=len(sources),sourcesVerified=sum(s['matchesOriginal'] for s in sources),changed=changed,missing=missing),indent=2))
