"""Read-only crown measurement to review cross-action body scale and registration."""
from pathlib import Path
from collections import deque
import json
import numpy as np
from PIL import Image,ImageDraw
from export_phase3a2 import ROOT,MANIFEST,VALIDATION,key_source

def crown(crop,prone=False):
    box=crop.getbbox(); x0,y0,x1,y1=box;w=x1-x0;h=y1-y0
    arr=np.array(crop);rgb=arr[:,:,:3].astype(int)
    mask=(arr[:,:,3]>0)&(rgb.max(2)<108)&(rgb.min(2)>17)&((rgb.max(2)-rgb.min(2))<35)&(rgb[:,:,0]>=rgb[:,:,1]-3)&(rgb[:,:,1]>=rgb[:,:,2]-3)
    region=np.zeros_like(mask)
    region[round(y0+.12*h):round(y0+.78*h),round(x0+(.57 if prone else .30)*w):round(x0+.92*w)]=True
    mask &= region
    components=[]
    for yy,xx in zip(*np.where(mask)):
        if not mask[yy,xx]:continue
        mask[yy,xx]=False;q=deque([(xx,yy)]);pts=[]
        while q:
            x,y=q.popleft();pts.append((x,y))
            for nx,ny in ((x-1,y),(x+1,y),(x,y-1),(x,y+1)):
                if 0<=ny<mask.shape[0] and 0<=nx<mask.shape[1] and mask[ny,nx]:
                    mask[ny,nx]=False;q.append((nx,ny))
        if len(pts)>20:
            a=np.array(pts);lo=a.min(0);hi=a.max(0)+1
            components.append((len(pts),[int(lo[0]),int(lo[1]),int(hi[0]),int(hi[1])]))
    if not components:raise ValueError('No crown; inspect source')
    _,b=max(components)
    return b

def main():
    entries=json.loads(MANIFEST.read_text());results=[]
    for e in entries:
        p=ROOT/e['source']
        if not p.exists():continue
        clean=key_source(Image.open(p));annotated=clean.convert('RGB');draw=ImageDraw.Draw(annotated)
        fw=clean.width/e['cols'];fh=clean.height/e['rows'];heads=[]
        for i in range(e['count']):
            x=i%e['cols'];y=i//e['cols']
            rect=e.get('sourceRects',[])
            rect=rect[i] if rect else [round(x*fw),round(y*fh),round((x+1)*fw),round((y+1)*fh)]
            b=crown(clean.crop(rect),e['action']=='death' and i>=5);heads.append(b)
            draw.rectangle([b[0]+rect[0],b[1]+rect[1],b[2]+rect[0],b[3]+rect[1]],outline='#00ff00',width=2)
        annotated.save(VALIDATION/f"{e['id']}_{e['action']}_crown_measurement.png")
        results.append({'id':e['id'],'action':e['action'],'crowns':heads,
            'widths':[b[2]-b[0] for b in heads],'heights':[b[3]-b[1] for b in heads]})
    (VALIDATION/'crown-measurements.json').write_text(json.dumps(results,indent=2))
    print(json.dumps([{k:v for k,v in r.items() if k!='crowns'} for r in results],indent=2))

if __name__=='__main__':main()
