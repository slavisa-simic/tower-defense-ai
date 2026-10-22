"""Read-only validation and paginated previews of the selected Phase 3A-1 pixels."""
from pathlib import Path
import json, hashlib, math
import numpy as np
from PIL import Image, ImageDraw
ROOT=Path(__file__).resolve().parents[2]
OUT=ROOT/'Docs/Validation/Phase3A1'
specs=json.loads((ROOT/'Docs/Art/Batches/p3a1_final_manifest.json').read_text())
rows=[]
for s in specs:
    p=ROOT/s['output']; im=Image.open(p); a=np.array(im)
    assert im.size==tuple(s['size']) and im.mode=='RGBA', p
    assert set(np.unique(a[:,:,3])).issubset({0,255}), p
    assert (a[:,:,3].min()==0) == s['alpha'], p
    if s.get('matte')=='magenta':
        rgb=a[:,:,:3].astype(float)
        assert not np.any((rgb[:,:,0]>30)&(rgb[:,:,2]>30)&(rgb[:,:,1]<150)&(rgb[:,:,0]>rgb[:,:,1]*1.35)&(rgb[:,:,2]>rgb[:,:,1]*1.35)&(a[:,:,3]>0)), p
    bbox=im.getchannel('A').getbbox()
    if s['alpha']: assert min(bbox[0],bbox[1],im.width-bbox[2],im.height-bbox[3])>=2, p
    if 'x' in s.get('repeat',''): assert np.array_equal(a[:,0],a[:,-1]),p
    if 'y' in s.get('repeat',''): assert np.array_equal(a[0],a[-1]),p
    rows.append(dict(path=s['output'],size=im.size,bbox=bbox,alpha=s['alpha'],sha256=hashlib.sha256(p.read_bytes()).hexdigest()))
buttons=[np.array(Image.open(ROOT/f'Assets/Game/Art/UI/bd_ui_build_button_{state}_v001.png')) for state in ['empty','selected','disabled']]
button_alpha_equal=all(np.array_equal(buttons[0][:,:,3],x[:,:,3]) for x in buttons[1:])
report=dict(count=len(rows),all_dimensions_alpha_margins_and_seams_passed=True,button_alpha_equal=button_alpha_equal,assets=rows)
(OUT/'final-pixel-audit.json').write_text(json.dumps(report,indent=2)+'\n')
for category in ['Environment','Props','Base','UI']:
    group=[s for s in specs if '/'+category+'/' in s['output']]
    per=4
    for page in range(math.ceil(len(group)/per)):
        chunk=group[page*per:page*per+per]
        board=Image.new('RGB',(1120,210*len(chunk)),'#252c2d'); d=ImageDraw.Draw(board)
        for i,s in enumerate(chunk):
            y=i*210; im=Image.open(ROOT/s['output']).convert('RGBA')
            d.text((10,y+6),f"{s.get('subject',Path(s['output']).stem)}  {im.width}x{im.height}",fill='#eee4cf')
            for x,bg in [(10,'#eee4cf'),(570,'#101717')]:
                d.rectangle((x,y+26,x+540,y+200),fill=bg)
                scale=max(1,min(4,520//im.width,164//im.height))
                p=im.resize((im.width*scale,im.height*scale),Image.Resampling.NEAREST)
                board.paste(p,(x+(540-p.width)//2,y+26+(174-p.height)//2),p)
        board.save(OUT/f'{category.lower()}-review-{page+1}.png')
print(json.dumps({k:v for k,v in report.items() if k!='assets'},indent=2))
