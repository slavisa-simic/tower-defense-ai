"""Apply reviewed source crown measurements to constant action scales and pivots.

This registers existing generated poses; no painted pixels or synthesized poses.
One-pixel breathing/gait bob, two-pixel recoil and a short eastward death settle
are explicitly recorded in the final manifest rather than inferred at runtime.
"""
import json
import numpy as np
from export_phase3a2 import MANIFEST,VALIDATION

def main():
    entries=json.loads(MANIFEST.read_text())
    measurements=json.loads((VALIDATION/'crown-measurements.json').read_text())
    by_key={(m['id'],m['action']):m for m in measurements}
    for e in entries:
        m=by_key[(e['id'],e['action'])]
        diameter={'basic_infected':10,'runner':9,'brute':14}[e['id']]
        offset={'basic_infected':(3,-2),'runner':(4,-2),'brute':(7,-4)}[e['id']]
        widths=m['widths'][:4] if e['action']=='death' else m['widths']
        scale=diameter/float(np.median(widths))
        e['exportScale']=scale
        e['scaleBasis']={'crownTargetPixels':diameter,'medianNativeCrownWidth':float(np.median(widths)),
            'method':'one scale per action; no per-frame resizing'}
        targets=[];pivots=[]
        for i,b in enumerate(m['crowns']):
            dx=dy=0
            if e['action']=='idle':dy=[0,0,1,0][i]
            elif e['action'] in ['walk','run']:dy=[0,0,-1,0,0,0,-1,0][i]
            elif e['action']=='attack':dx=([0,-1,0,-3,1,0] if e['id']=='brute' else [0,-1,0,2,1,0])[i]
            elif e['action']=='hit':dx=[-2,-1][i];dy=[-1,0][i]
            elif e['action']=='death':
                dx=([0,0,2,4,6,9,10,10] if e['id']=='brute' else [0,0,1,2,5,7,8,8])[i]
                dy=[0,1,2,3,3,3,3,3][i]
            tx=e['frame']/2+offset[0]+dx;ty=e['frame']/2+offset[1]+dy
            cx=(b[0]+b[2])/2;cy=(b[1]+b[3])/2
            pivots.append([round(cx-(tx-e['frame']/2)/scale,5),round(cy-(ty-e['frame']/2)/scale,5)])
            targets.append([tx,ty])
        e['measuredCrownRects']=m['crowns'];e['crownTargets']=targets;e['sourcePivots']=pivots
    MANIFEST.write_text(json.dumps(entries,indent=2),encoding='utf-8')
    print('Recorded crown scale and fixed-canvas registration for',len(entries),'actions')

if __name__=='__main__':main()
