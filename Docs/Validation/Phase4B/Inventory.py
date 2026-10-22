import pathlib,json,hashlib
from PIL import Image
root=pathlib.Path('.')
paths=list((root/'Assets/Game/Art/SourceSheets/p4b_heavy_001').glob('*.png'))+list((root/'Assets/Game/Art/Effects/Heavy').glob('*.png'))
for folder in ['MortarPosition','FlamethrowerPosition','AutoCannon','RocketLauncherPosition']:
 paths+=list((root/'Assets/Game/Art/Defenses'/folder).glob('*.png'))
paths += [root/'Assets/Game/Art/UI'/('ui_icon_'+n+'.png') for n in ['mortar','flamethrower','autocannon','rocket']]
paths += [root/'Assets/Game/Art/MasterReferences/ref_heavy_defense_lineup_01.png']
rows=[]
for p in sorted(set(paths)):
 if not p.exists(): continue
 with Image.open(p) as im:
  rows.append(dict(path=p.as_posix(),size=list(im.size),mode=im.mode,alpha=list(im.getchannel('A').getextrema()) if 'A' in im.getbands() else None,sha256=hashlib.sha256(p.read_bytes()).hexdigest()))
(root/'Docs/Art/Batches/p4b_heavy_inventory.json').write_text(json.dumps(rows,indent=2)+'\n')
print(len(rows),'images recorded')
