"""User-authorized deterministic exports; native ImageGen sources are never modified.

Usage: python Docs/Art/export_phase3a1.py manifest.json
Each entry specifies source, output, size, palette/colors, alpha, repeat axes.
"""
from pathlib import Path
import hashlib
import json
import sys
import numpy as np
from PIL import Image, ImageDraw, ImageFilter, __version__ as pillow_version

ROOT = Path(__file__).resolve().parents[2]

def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()

def export(spec):
    src = ROOT / spec['source']
    out = ROOT / spec['output']
    assert src.resolve().is_relative_to(ROOT / 'Assets/Game/Art/SourceSheets')
    assert out.resolve().is_relative_to(ROOT / 'Assets/Game/Art')
    assert '/SourceSheets/' not in out.as_posix()
    native = Image.open(src)
    im = native.convert('RGBA')
    matte = spec.get('matte')
    if matte:
        pixels = np.array(im)
        rgb = pixels[:,:,:3].astype(np.int16)
        if matte == 'bright_neutral':
            candidate = (rgb.max(axis=2)-rgb.min(axis=2)<24) & (rgb.min(axis=2)>105)
            flood = Image.fromarray(candidate.astype(np.uint8)).copy()
            for seed in [(0,0),(im.width-1,0),(0,im.height-1),(im.width-1,im.height-1)]:
                if flood.getpixel(seed) == 1:
                    ImageDraw.floodfill(flood,seed,2)
            bg = np.array(flood)==2
        elif matte == 'magenta':
            # Include dark chroma spill along generated outlines, not just the bright matte.
            bg = (rgb[:,:,0]>30) & (rgb[:,:,2]>30) & (rgb[:,:,1]<150) & (rgb[:,:,0]>rgb[:,:,1]*1.35) & (rgb[:,:,2]>rgb[:,:,1]*1.35)
        else:
            raise ValueError('Unknown matte mode')
        pixels[bg,3]=0
        pixels[bg,:3]=0  # Exclude matte color from adaptive palette clustering.
        im=Image.fromarray(pixels)
    size = tuple(spec['size'])
    bbox = None
    if spec.get('alpha'):
        if im.getchannel('A').getextrema()[0] == 255:
            raise ValueError(f'Genuine alpha missing: {src}')
        bbox = im.getchannel('A').getbbox()
        if not bbox:
            raise ValueError(f'Empty source: {src}')
        if bbox == (0,0,im.width,im.height):
            raise ValueError(f'Background remains on all edges: {src}')
        im = im.crop(bbox)
        margin = spec.get('margin', 2)
        scale = min((size[0]-margin*2)/im.width, (size[1]-margin*2)/im.height)
        fitted = (size[0]-margin*2, size[1]-margin*2) if spec.get('fit') == 'stretch_panel' else (max(1, round(im.width*scale)), max(1, round(im.height*scale)))
        im = im.resize(fitted, Image.Resampling.NEAREST)
        canvas = Image.new('RGBA', size)
        canvas.paste(im, ((size[0]-im.width)//2, (size[1]-im.height)//2))
        im = canvas
    else:
        im = im.resize(size, Image.Resampling.NEAREST)
    if spec.get('mode_filter'):
        im = im.filter(ImageFilter.ModeFilter(spec['mode_filter']))
    arr = np.array(im, dtype=np.float32)
    repeat = spec.get('repeat', '')
    border = spec.get('seam_border', 4)
    for axis, length in [('x', size[0]), ('y', size[1])]:
        if axis not in repeat:
            continue
        for d in range(border):
            weight = (1 - d / border) * 0.5
            if axis == 'x':
                a, b = arr[:, d, :3].copy(), arr[:, length-1-d, :3].copy()
                arr[:, d, :3] = a*(1-weight)+b*weight
                arr[:, length-1-d, :3] = b*(1-weight)+a*weight
            else:
                a, b = arr[d, :, :3].copy(), arr[length-1-d, :, :3].copy()
                arr[d, :, :3] = a*(1-weight)+b*weight
                arr[length-1-d, :, :3] = b*(1-weight)+a*weight
    arr = np.clip(np.rint(arr), 0, 255).astype(np.uint8)
    if spec.get('palette'):
        palette = np.array([[int(c[i:i+2],16) for i in (1,3,5)] for c in spec['palette']], dtype=np.int32)
        dist = ((arr[:,:,:3].astype(np.int32)[:,:,None,:]-palette[None,None,:,:])**2).sum(axis=3)
        arr[:,:,:3] = palette[dist.argmin(axis=2)].astype(np.uint8)
    else:
        quant = Image.fromarray(arr[:,:,:3]).quantize(colors=spec.get('colors', 24), method=Image.Quantize.MEDIANCUT, dither=Image.Dither.NONE).convert('RGB')
        arr[:,:,:3] = np.asarray(quant)
    # Hard pixel alpha, explicitly recorded; avoids blurred outlines after nearest export.
    arr[:,:,3] = np.where(arr[:,:,3] >= 128, 255, 0) if spec.get('alpha') else 255
    arr[arr[:,:,3] == 0,:3] = 0
    im = Image.fromarray(arr)
    out.parent.mkdir(parents=True, exist_ok=True)
    im.save(out)
    report = dict(spec, native_size=list(native.size), native_mode=native.mode,
        source_sha256=sha(src), output_sha256=sha(out), crop_bbox=bbox,
        actual_size=list(im.size), alpha_extrema=im.getchannel('A').getextrema(),
        transparent_pixels=int((arr[:,:,3]==0).sum()),
        color_count=len(np.unique(arr[arr[:,:,3]>0,:3],axis=0)),
        method='nearest-neighbor; paired seam-edge blend before palette mapping; no dithering; binary alpha threshold 128',
        pillow_version=pillow_version,
        edge_x_equal=bool(np.array_equal(arr[:,0],arr[:,-1])),
        edge_y_equal=bool(np.array_equal(arr[0],arr[-1])))
    if 'x' in repeat: assert report['edge_x_equal']
    if 'y' in repeat: assert report['edge_y_equal']
    return report, im

def main():
    path = ROOT / sys.argv[1]
    specs = json.loads(path.read_text(encoding='utf-8-sig'))
    rows, images = [], []
    for spec in specs:
        row, im = export(spec)
        rows.append(row)
        images.append((Path(spec['output']).stem, im, spec.get('repeat','')))
    evidence = ROOT / 'Docs/Validation/Phase3A1'
    evidence.mkdir(parents=True, exist_ok=True)
    (evidence/(path.stem+'-exports.json')).write_text(json.dumps(rows,indent=2)+'\n')
    width, height = 900, 420*len(images)
    board = Image.new('RGB',(width,height),'#191f22')
    draw = ImageDraw.Draw(board)
    for i,(name,im,repeat) in enumerate(images):
        y = i*420
        draw.text((12,y+8),name,fill='#e6dfc7')
        if repeat:
            grid=Image.new('RGBA',(im.width*3, im.height*3))
            for a in range(3):
                for b in range(3): grid.paste(im,(a*im.width,b*im.height))
            scale=max(1,min(3,600//grid.width,384//grid.height))
            preview=grid.resize((grid.width*scale,grid.height*scale),Image.Resampling.NEAREST)
            board.paste(preview,(12,y+30),preview)
        else:
            scale=min(390/im.width,280/im.height)
            preview=im.resize((round(im.width*scale),round(im.height*scale)),Image.Resampling.NEAREST)
            for x,bg in [(12,'#ece4d4'),(458,'#101416')]:
                draw.rectangle((x,y+30,x+420,y+340),fill=bg)
                board.paste(preview,(x+12,y+44),preview)
    board.save(evidence/(path.stem+'-preview.png'))
    print(json.dumps({'exported':len(rows),'manifest':str(path),'evidence':str(evidence)},indent=2))

if __name__ == '__main__': main()
