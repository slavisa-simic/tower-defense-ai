"""Read-only contact previews of exported pixels on contrasting backdrops."""
import json,sys,math
from pathlib import Path
from PIL import Image,ImageDraw
ROOT=Path(__file__).resolve().parents[2]
specs=json.loads((ROOT/sys.argv[1]).read_text(encoding='utf-8-sig'))
board=Image.new('RGB',(1120,math.ceil(len(specs)/2)*230),'#252c2d')
d=ImageDraw.Draw(board)
for i,s in enumerate(specs):
    x,y=(i%2)*560,(i//2)*230
    im=Image.open(ROOT/s['output']).convert('RGBA')
    d.text((x+8,y+5),Path(s['output']).stem.replace('bd_','').replace('_v001',''),fill='#eee4cb')
    d.text((x+8,y+20),f'{im.width} x {im.height}; native export, nearest integer preview',fill='#b9c3b3')
    for dx,bg in [(8,'#eee4cf'),(285,'#101717')]:
        d.rectangle((x+dx,y+40,x+dx+265,y+220),fill=bg)
        scale=max(1,min(4,250//im.width,172//im.height))
        p=im.resize((im.width*scale,im.height*scale),Image.Resampling.NEAREST)
        board.paste(p,(x+dx+(265-p.width)//2,y+40+(180-p.height)//2),p)
out=ROOT/'Docs/Validation/Phase3A1'/(Path(sys.argv[1]).stem+'-contact.png')
board.save(out)
print(out)
