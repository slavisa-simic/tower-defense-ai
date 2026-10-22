"""Build self-contained README SVGs from the game's existing pixels and HUD font.

Requires Pillow. Run from the repository root. Original game assets are read only.
"""
from pathlib import Path
from html import escape
from PIL import Image, ImageDraw, ImageFont

ROOT = Path(__file__).resolve().parents[3]
OUT = ROOT / "Docs/Readme/Media"
ART = ROOT / "Assets/Game/Art"
FONT = ImageFont.truetype(str(ART / "UI/bd_ui_hud_pixel_v001.ttf"), 16)
P = {"bg": "#131e22", "edge": "#55604e", "bone": "#dfd7bd",
     "amber": "#c18a3e", "muted": "#a8ad96", "water": "#17444c", "olive": "#4b5538"}


def pixel_paths(image, color=None):
    """Keep exact RGBA pixels; merge equal neighboring pixels into horizontal runs."""
    image = image.convert("RGBA")
    groups = {}
    for y in range(image.height):
        x = 0
        while x < image.width:
            rgba = image.getpixel((x, y))
            end = x + 1
            while end < image.width and image.getpixel((end, y)) == rgba:
                end += 1
            if rgba[3]:
                fill = color or "#%02x%02x%02x" % rgba[:3]
                key = (fill, rgba[3])
                groups.setdefault(key, []).append(f"M{x} {y}h{end-x}v1h-{end-x}z")
            x = end
    return "".join(f'<path fill="{fill}" opacity="{alpha/255:.3f}" d="{"".join(parts)}"/>'
                   for (fill, alpha), parts in groups.items())


def label(text, x, y, scale=2, color=None):
    mask = Image.new("L", (max(1, int(FONT.getlength(text))), 16))
    ImageDraw.Draw(mask).text((0, -2), text, font=FONT, fill=255)
    mask = mask.point(lambda v: 255 if v > 127 else 0)
    image = Image.new("RGBA", mask.size, (255, 255, 255, 0))
    image.putalpha(mask)
    return f'<g transform="translate({x} {y}) scale({scale/2})">{pixel_paths(image, color or P["bone"])}</g>'


def sprite(path, x, y, scale, angle=0):
    image = Image.open(ART/path)
    rotation = f' rotate({angle} {image.width/2} {image.height/2})' if angle else ''
    return f'<g transform="translate({x} {y}) scale({scale}){rotation}">{pixel_paths(image)}</g>'


def text(content, x, y, size=18, color=None):
    return f'<text x="{x}" y="{y}" fill="{color or P["muted"]}" font-family="monospace" font-size="{size}">{escape(content)}</text>'


def shell(title, height, body):
    return (f'<svg xmlns="http://www.w3.org/2000/svg" width="1280" height="{height}" viewBox="0 0 1280 {height}" role="img">'
            f'<title>{escape(title)}</title><rect width="1280" height="{height}" fill="{P["bg"]}"/>'
            f'<path d="M0 1h1280M0 {height-1}h1280" stroke="{P["edge"]}" stroke-width="2"/>'
            f'{body}</svg>\n')


def rivets(height):
    return "".join(f'<rect x="{x}" y="{y}" width="5" height="5" fill="{P["edge"]}"/>'
                   for x in (14, 1261) for y in (13, height-18))


def build():
    OUT.mkdir(parents=True, exist_ok=True)
    body = f'<path d="M854 0v350" stroke="{P["edge"]}" stroke-width="2"/>'
    body += '<defs><clipPath id="vignette"><rect x="872" y="18" width="390" height="314"/></clipPath></defs>'
    body += '<g clip-path="url(#vignette)">'
    body += f'<rect x="872" y="18" width="390" height="314" fill="{P["olive"]}"/>'
    body += f'<rect x="872" y="18" width="146" height="314" fill="{P["water"]}"/>'
    body += '<path d="M1006 18h24v76h-9v62h11v70h-14v106h-12z" fill="#5a4633"/>'
    for i in range(16):
        x, y = 886 + (i * 37) % 111, 32 + (i * 43) % 280
        body += f'<path d="M{x} {y}h13v3h-7v3h-11v-3h5z" fill="#2d5c5d"/>'
    for i in range(24):
        x, y = 1039 + (i * 47) % 214, 30 + (i * 67) % 288
        body += f'<path d="M{x} {y}h7v4h6v5h-9v-3h-4z" fill="#636b44"/>'
    body += sprite(Path("Props/bd_prop_rubber_boat_v001.png"), 883, 175, .64)
    body += sprite(Path("Defenses/MachineGunNest/bd_defense_mg_preview_v001.png"), 1092, 72, 2, 180)
    body += sprite(Path("UI/bd_ui_barbed_wire_icon_v001.png"), 1080, 232, 1)
    body += '</g>'
    body += label("FIELD DOSSIER  /  PHASE 6A", 48, 36, 2, P["amber"])
    body += label("BORDER", 48, 88, 10)
    body += label("DEFENDERS", 48, 176, 10)
    body += text("The Danube quarantine line.", 51, 285, 23)
    body += text("STRICT OVERHEAD  /  PIXEL ART  /  UNITY 6", 51, 320, 15, P["amber"])
    body += rivets(350)
    (OUT/"hero.svg").write_text(shell("Border Defenders — the Danube quarantine line", 350, body), encoding="utf-8")

    sections = [
        ("field", "01", "HOLD THE RIVER", "MAPS / DEFENSES / INCOMING CONTACTS", "UI/bd_ui_mg_nest_icon_v001.png"),
        ("systems", "02", "INSIDE THE DEFENSE LINE", "COMMANDS / STATE / PRESENTATION", "UI/ui_icon_autocannon.png"),
        ("workshop", "03", "BUILT WITH CODEX", "PHASE BRIEFS / ART BIBLE / CUSTOM SKILLS", "UI/bd_ui_land_mine_icon_v001.png"),
        ("deploy", "04", "OPEN THE PROJECT", "SETUP / CONTROLS / VALIDATION", "UI/ui_icon_money_rsd.png"),
    ]
    for name, number, heading, subtitle, icon in sections:
        body = f'<rect x="0" y="0" width="7" height="112" fill="{P["amber"]}"/>'
        body += label(number, 38, 38, 4, P["amber"])
        body += f'<path d="M112 23v66" stroke="{P["edge"]}"/>'
        body += label(heading, 144, 25, 4)
        body += text(subtitle, 146, 87, 16)
        image = Image.open(ART/icon)
        factor = min(1, 76/image.width, 76/image.height)
        body += sprite(Path(icon), 1180, 18, factor)
        body += rivets(112)
        (OUT/f"section-{name}.svg").write_text(shell(heading.title(), 112, body), encoding="utf-8")


if __name__ == "__main__":
    build()
    print("Built five self-contained SVGs using existing game pixels and the HUD font.")
