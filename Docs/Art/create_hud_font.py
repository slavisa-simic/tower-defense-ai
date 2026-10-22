"""Original 5x7 HUD glyphs extending the Phase 3B reward language; no external font artwork."""
from pathlib import Path
import runpy, sys, tempfile
sys.path.insert(0, str(Path(tempfile.gettempdir()) / 'bd-p3c-fonttools'))
# Read glyph definitions only; do not execute the old generator or rewrite its asset.
source=Path(__file__).with_name('create_reward_font.py').read_text()
base={}
exec(source[:source.index('fb=FontBuilder')].replace("sys.path.insert(0,str(Path(tempfile.gettempdir())/'bd-p3b-fonttools'))", ''), base)
patterns = dict(base['PATTERNS'])
letters = {
'A':'01110 10001 10001 11111 10001 10001 10001', 'B':'11110 10001 10001 11110 10001 10001 11110',
'C':'01111 10000 10000 10000 10000 10000 01111', 'D':'11110 10001 10001 10001 10001 10001 11110',
'E':'11111 10000 10000 11110 10000 10000 11111', 'F':'11111 10000 10000 11110 10000 10000 10000',
'G':'01111 10000 10000 10111 10001 10001 01111', 'H':'10001 10001 10001 11111 10001 10001 10001',
'I':'01110 00100 00100 00100 00100 00100 01110', 'J':'00001 00001 00001 00001 10001 10001 01110',
'K':'10001 10010 10100 11000 10100 10010 10001', 'L':'10000 10000 10000 10000 10000 10000 11111',
'M':'10001 11011 10101 10101 10001 10001 10001', 'N':'10001 11001 10101 10011 10001 10001 10001',
'O':'01110 10001 10001 10001 10001 10001 01110', 'P':'11110 10001 10001 11110 10000 10000 10000',
'Q':'01110 10001 10001 10001 10101 10010 01101', 'R':'11110 10001 10001 11110 10100 10010 10001',
'S':'01111 10000 10000 01110 00001 00001 11110', 'T':'11111 00100 00100 00100 00100 00100 00100',
'U':'10001 10001 10001 10001 10001 10001 01110', 'V':'10001 10001 10001 10001 10001 01010 00100',
'W':'10001 10001 10001 10101 10101 11011 10001', 'X':'10001 10001 01010 00100 01010 10001 10001',
'Y':'10001 10001 01010 00100 00100 00100 00100', 'Z':'11111 00001 00010 00100 01000 10000 11111',
'$':'00100 01111 10100 01110 00101 11110 00100', ':':'00000 00100 00100 00000 00100 00100 00000',
'/':'00001 00010 00010 00100 01000 01000 10000', '-':'00000 00000 00000 11111 00000 00000 00000',
'|':'00100 00100 00100 00100 00100 00100 00100', '.':'00000 00000 00000 00000 00000 00110 00110',
'!':'00100 00100 00100 00100 00100 00000 00100', '?':'01110 10001 00001 00010 00100 00000 00100',
}
patterns.update({k:v.split() for k,v in letters.items()})
patterns.update({k.lower():patterns[k] for k in letters if k.isalpha()})
patterns['—']=patterns['-']; patterns['–']=patterns['-']
fb=base['FontBuilder'](1000,isTTF=True)
order=['.notdef']+['uni'+format(ord(c),'04X') for c in patterns]
fb.setupGlyphOrder(order); fb.setupCharacterMap({ord(c):'uni'+format(ord(c),'04X') for c in patterns})
glyphs={};metrics={}
for name in order:
    pen=base['TTGlyphPen'](None)
    pattern=patterns.get(chr(int(name[3:],16))) if name!='.notdef' else patterns['?']
    for y,row in enumerate(pattern):
        for x,v in enumerate(row):
            if v=='1':
                xx,yy=x*100,(6-y)*100
                pen.moveTo((xx,yy));pen.lineTo((xx,yy+100));pen.lineTo((xx+100,yy+100));pen.lineTo((xx+100,yy));pen.closePath()
    glyphs[name]=pen.glyph();metrics[name]=(600,0)
fb.setupGlyf(glyphs);fb.setupHorizontalMetrics(metrics);fb.setupHorizontalHeader(ascent=800,descent=-200)
fb.setupNameTable({'familyName':'Border Defenders HUD','styleName':'Regular','uniqueFontIdentifier':'BorderDefendersHUD-1','fullName':'Border Defenders HUD','psName':'BorderDefendersHUD-Regular','version':'Version 1.0'})
fb.setupOS2(sTypoAscender=800,sTypoDescender=-200,usWinAscent=800,usWinDescent=200);fb.setupPost();fb.setupMaxp()
path=Path(__file__).resolve().parents[2]/'Assets/Game/Art/UI/bd_ui_hud_pixel_v001.ttf'
fb.save(str(path));print(path)
