"""Original 5x7 digit font for functional reward labels. No third-party glyph artwork."""
from pathlib import Path
import sys, tempfile
sys.path.insert(0,str(Path(tempfile.gettempdir())/'bd-p3b-fonttools'))
from fontTools.fontBuilder import FontBuilder
from fontTools.pens.ttGlyphPen import TTGlyphPen

PATTERNS={
 '0':['01110','10001','10011','10101','11001','10001','01110'],
 '1':['00100','01100','00100','00100','00100','00100','01110'],
 '2':['01110','10001','00001','00010','00100','01000','11111'],
 '3':['11110','00001','00001','01110','00001','00001','11110'],
 '4':['00010','00110','01010','10010','11111','00010','00010'],
 '5':['11111','10000','10000','11110','00001','00001','11110'],
 '6':['01110','10000','10000','11110','10001','10001','01110'],
 '7':['11111','00001','00010','00100','01000','01000','01000'],
 '8':['01110','10001','10001','01110','10001','10001','01110'],
 '9':['01110','10001','10001','01111','00001','00001','01110'],
 '+':['00000','00100','00100','11111','00100','00100','00000'],
 ' ':['00000']*7}
fb=FontBuilder(1000,isTTF=True);order=['.notdef']+['uni'+format(ord(c),'04X') for c in PATTERNS]
fb.setupGlyphOrder(order);fb.setupCharacterMap({ord(c):'uni'+format(ord(c),'04X') for c in PATTERNS})
glyphs={};metrics={}
for name in order:
    pen=TTGlyphPen(None)
    pattern=PATTERNS.get(chr(int(name[3:],16))) if name!='.notdef' else ['11111','10001','10101','10101','10101','10001','11111']
    for y,row in enumerate(pattern):
        for x,v in enumerate(row):
            if v=='1':
                xx,yy=x*100,(6-y)*100
                pen.moveTo((xx,yy));pen.lineTo((xx,yy+100));pen.lineTo((xx+100,yy+100));pen.lineTo((xx+100,yy));pen.closePath()
    glyphs[name]=pen.glyph();metrics[name]=(600,0)
fb.setupGlyf(glyphs);fb.setupHorizontalMetrics(metrics);fb.setupHorizontalHeader(ascent=800,descent=-200)
fb.setupNameTable({'familyName':'Border Defenders Reward','styleName':'Regular','uniqueFontIdentifier':'BorderDefendersReward-1','fullName':'Border Defenders Reward','psName':'BorderDefendersReward-Regular','version':'Version 1.0'})
fb.setupOS2(sTypoAscender=800,sTypoDescender=-200,usWinAscent=800,usWinDescent=200);fb.setupPost();fb.setupMaxp()
path=Path(__file__).resolve().parents[2]/'Assets/Game/Art/Effects/UIFeedback/bd_reward_digits_v001.ttf'
path.parent.mkdir(parents=True,exist_ok=True);fb.save(str(path));print(path)
