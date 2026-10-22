"""Non-runtime previews for UI slice borders and modular wall contact anchors."""
from pathlib import Path
import json
import numpy as np
from PIL import Image,ImageDraw
R=Path(__file__).resolve().parents[2]; E=R/'Docs/Validation/Phase3A1'
specs=json.loads((R/'Docs/Art/Batches/p3a1_final_manifest.json').read_text())
panels=[s for s in specs if '/UI/' in s['output'] and s.get('border')]
rows=[]
for i,s in enumerate(panels):
    im=Image.open(R/s['output']).convert('RGBA'); w,h=im.size; b=16
    W,H=(96,96) if s.get('subject','').startswith('build_button') else (w+64,h+32)
    p=Image.new('RGBA',(W,H)); xs=[0,b,w-b,w]; ys=[0,b,h-b,h]; X=[0,b,W-b,W]; Y=[0,b,H-b,H]
    for u in range(3):
        for v in range(3):
            tile=im.crop((xs[u],ys[v],xs[u+1],ys[v+1]))
            p.paste(tile.resize((X[u+1]-X[u],Y[v+1]-Y[v]),Image.Resampling.NEAREST),(X[u],Y[v]))
    for u,v in [(0,0),(2,0),(0,2),(2,2)]:
        assert np.array_equal(np.array(im.crop((xs[u],ys[v],xs[u+1],ys[v+1]))),np.array(p.crop((X[u],Y[v],X[u+1],Y[v+1]))))
    board=Image.new('RGB',(960,220),'#252c2d'); d=ImageDraw.Draw(board)
    d.text((10,8),s['subject']+' / source at left; nine-slice preview at right; all four 16px corners preserved',fill='#eee4cf')
    board.paste(im,(10,40),im); board.paste(p,(490,40),p); board.save(E/f'ui-slice-{i+1}.png')
    rows.append(dict(path=s['output'],border=[16]*4,preview_size=[W,H],corners_identical=True))
(E/'ui-slice-qa.json').write_text(json.dumps(rows,indent=2)+'\n')
corner=Image.open(R/'Assets/Game/Art/Base/bd_base_bunker_corner_v001.png').convert('RGBA')
wall=Image.open(R/'Assets/Game/Art/Base/bd_base_bunker_wall_segment_v001.png').convert('RGBA')
cb=corner.getchannel('A').getbbox(); wb=wall.getchannel('A').getbbox()
def vrange(im,x):
    ys=np.where(np.array(im)[:,x,3]>0)[0]; return [int(ys.min()),int(ys.max()+1)]
cr=vrange(corner,cb[2]-4); wr=vrange(wall,wb[0]+3)
cornerport=[cb[2],sum(cr)/2]; wallport=[wb[0],sum(wr)/2]
offset=[int(cornerport[0]-wallport[0]-3),int(round(cornerport[1]-wallport[1]))]
board=Image.new('RGBA',(150,100),'#4b5538'); board.alpha_composite(wall,(10+offset[0],20+offset[1])); board.alpha_composite(corner,(10,20))
board.resize((600,400),Image.Resampling.NEAREST).save(E/'wall-connection-preview.png')
(E/'base-connection-qa.json').write_text(json.dumps(dict(coordinate_system='Pixels from top-left of each export; values are visual attachment guides, not gameplay cells',corner_right_port=cornerport,wall_left_port=wallport,corner_right_section=cr,wall_left_section=wr,wall_offset_relative_to_corner=offset,overlap_pixels=3,ppu=51.2,placement='Use visual child offsets; preserve root/grid footprint; corner can be rotated in 90-degree increments'),indent=2)+'\n')
print(json.dumps(dict(sliced_panels=len(rows),corner_section=cr,wall_section=wr,offset=offset)))
