
from pathlib import Path
from random import Random

from PIL import (
    Image,
    ImageDraw,
    ImageEnhance,
    ImageFont,
    ImageOps,
)

# ============================================================
# POTAMORPH — README MEDIA GENERATOR
# ============================================================

ROOT = Path(__file__).resolve().parents[3]

MEDIA = ROOT / "Docs/Readme/Media"
SOURCE = ROOT / "Docs/Readme/Source"
ART = ROOT / "Assets/Game/Art"

LOGO = SOURCE / "potamorph-logo.png"
FONT = ART / "UI/bd_ui_hud_pixel_v001.ttf"

MEDIA.mkdir(parents=True, exist_ok=True)

# ============================================================
# THEME
# ============================================================

BG = (8, 18, 23, 255)
PANEL = (8, 19, 24, 255)
GOLD = (211, 151, 42, 255)
GOLD_DARK = (113, 79, 27, 255)
IVORY = (240, 229, 198, 255)
MUTED = (171, 176, 158, 255)
TEAL = (48, 100, 104, 255)
GRID = (19, 38, 42, 255)

HERO_SIZE = (1280, 350)
SECTION_SIZE = (1280, 112)


# ============================================================
# FONTS & TEXT
# ============================================================

def get_font(size):
    candidates = [
        FONT,
        Path("C:/Windows/Fonts/consola.ttf"),
        Path("C:/Windows/Fonts/lucon.ttf"),
    ]

    for path in candidates:
        try:
            if path.exists():
                return ImageFont.truetype(str(path), size)
        except OSError:
            pass

    return ImageFont.load_default()


def fitted_font(draw, text, start_size, max_width):
    size = start_size

    while size >= 12:
        font = get_font(size)
        box = draw.textbbox((0, 0), text, font=font)

        if box[2] - box[0] <= max_width:
            return font

        size -= 1

    return get_font(12)


def draw_text(draw, xy, text, font, color=IVORY):
    x, y = xy

    draw.text(
        (x + 2, y + 2),
        text,
        font=font,
        fill=(0, 0, 0, 255),
    )

    draw.text(
        (x, y),
        text,
        font=font,
        fill=color,
    )


def centered_text(draw, rect, text, font, color=IVORY):
    x1, y1, x2, y2 = rect

    # Text bounding boxes include glyph offsets.
    # Subtracting those offsets gives true visual centering.
    box = draw.textbbox((0, 0), text, font=font)

    width = box[2] - box[0]
    height = box[3] - box[1]

    x = x1 + ((x2 - x1) - width) / 2 - box[0]
    y = y1 + ((y2 - y1) - height) / 2 - box[1]

    draw.text((round(x), round(y)), text, font=font, fill=color)


# ============================================================
# DRAWING UTILITIES
# ============================================================

def gradient_background(width, height):
    image = Image.new("RGBA", (width, height), BG)
    draw = ImageDraw.Draw(image)

    for y in range(height):
        t = y / max(1, height - 1)

        color = (
            int(7 + 5 * t),
            int(17 + 9 * t),
            int(22 + 10 * t),
            255,
        )

        draw.line((0, y, width, y), fill=color)

    return image


def draw_grid(draw, width, height):
    for x in range(0, width, 32):
        draw.line((x, 0, x, height), fill=GRID, width=1)

    for y in range(0, height, 28):
        draw.line((0, y, width, y), fill=GRID, width=1)


def draw_noise(draw, width, height):
    rng = Random(2026)

    for _ in range(180):
        x = rng.randrange(width)
        y = rng.randrange(height)

        draw.point(
            (x, y),
            fill=(38, 58, 58, 255),
        )


def draw_hazards(draw, x, y, count=6):
    for i in range(count):
        sx = x + i * 20

        draw.polygon(
            [
                (sx + 8, y),
                (sx + 19, y),
                (sx + 11, y + 8),
                (sx, y + 8),
            ],
            fill=GOLD,
        )


def draw_frame(draw, width, height):
    # Outer industrial frame
    draw.rectangle(
        (3, 3, width - 4, height - 4),
        outline=GOLD,
        width=2,
    )

    draw.rectangle(
        (9, 9, width - 10, height - 10),
        outline=GOLD_DARK,
        width=1,
    )

    # Corner reinforcements
    length = 24

    corners = [
        (13, 13, 1, 1),
        (width - 14, 13, -1, 1),
        (13, height - 14, 1, -1),
        (width - 14, height - 14, -1, -1),
    ]

    for x, y, dx, dy in corners:
        draw.line(
            (x, y, x + dx * length, y),
            fill=GOLD,
            width=3,
        )
        draw.line(
            (x, y, x, y + dy * length),
            fill=GOLD,
            width=3,
        )

    # Small rivets
    for x in (15, width - 16):
        for y in (15, height - 16):
            draw.ellipse(
                (x - 2, y - 2, x + 2, y + 2),
                fill=IVORY,
            )

    draw_hazards(draw, 50, 5, 6)
    draw_hazards(draw, width - 180, height - 13, 6)


# ============================================================
# HERO BACKGROUND
# ============================================================

def load_hero_background():
    width, height = HERO_SIZE

    # These are HUD-free world-camera captures.
    # Do not use Phase6E validation screenshots here:
    # they may contain HOMICIDES, currency and other UI.
    candidates = [
        MEDIA / "industrial-port.png",
        MEDIA / "danube-border.png",
    ]

    background = None

    for path in candidates:
        if path.exists():
            with Image.open(path) as img:
                background = ImageOps.fit(
                    img.convert("RGB"),
                    HERO_SIZE,
                    method=Image.Resampling.LANCZOS,
                )
            break

    if background is None:
        return gradient_background(width, height)

    background = ImageEnhance.Color(background).enhance(0.72)
    background = ImageEnhance.Brightness(background).enhance(0.55)
    background = background.convert("RGBA")

    # Dark teal cinematic tint
    tint = Image.new(
        "RGBA",
        HERO_SIZE,
        (4, 20, 26, 120),
    )

    background = Image.alpha_composite(background, tint)

    # Additional horizontal darkening at the bottom
    overlay = Image.new("RGBA", HERO_SIZE, (0, 0, 0, 0))
    draw = ImageDraw.Draw(overlay)

    for y in range(height):
        t = y / max(1, height - 1)
        alpha = int(16 + 85 * t)

        draw.line(
            (0, y, width, y),
            fill=(2, 8, 12, alpha),
        )

    return Image.alpha_composite(background, overlay)


# ============================================================
# LOGO
# ============================================================

def paste_logo(canvas, target_box):
    if not LOGO.exists():
        raise FileNotFoundError(
            f"Missing POTAMORPH logo: {LOGO}"
        )

    x, y, width, height = target_box

    with Image.open(LOGO) as source:
        logo = source.convert("RGBA")

    alpha = logo.getchannel("A")
    bounds = alpha.getbbox()

    if bounds:
        logo = logo.crop(bounds)

    logo = ImageOps.contain(
        logo,
        (width, height),
        method=Image.Resampling.LANCZOS,
    )

    px = x + (width - logo.width) // 2
    py = y + (height - logo.height) // 2

    canvas.alpha_composite(logo, (px, py))


# ============================================================
# HERO
# ============================================================

def build_hero():
    width, height = HERO_SIZE

    canvas = load_hero_background()
    draw = ImageDraw.Draw(canvas)

    # Additional dark backing for the logo
    draw.rounded_rectangle(
        (18, 19, 346, 329),
        radius=12,
        fill=(5, 14, 18, 255),
        outline=GOLD_DARK,
        width=1,
    )

    paste_logo(canvas, (30, 23, 304, 296))

    draw = ImageDraw.Draw(canvas)

    # Fully opaque central text panel
    panel = (366, 79, 1086, 267)

    draw.rounded_rectangle(
        panel,
        radius=12,
        fill=PANEL,
        outline=GOLD_DARK,
        width=2,
    )

    draw.line(
        (384, 97, 384, 250),
        fill=GOLD,
        width=4,
    )

    title = "POTAMORPH"
    title_font = fitted_font(draw, title, 61, 650)

    draw_text(
        draw,
        (405, 96),
        title,
        title_font,
        IVORY,
    )

    draw_text(
        draw,
        (409, 169),
        "DEFEND THE DANUBE",
        get_font(23),
        GOLD,
    )

    subtitle = "TACTICAL TOWER DEFENSE / AQUATIC HORROR"

    subtitle_font = fitted_font(
        draw,
        subtitle,
        19,
        650,
    )

    draw_text(
        draw,
        (409, 213),
        subtitle,
        subtitle_font,
        IVORY,
    )

    # Outside the central panel
    draw_text(
        draw,
        (406, 288),
        "UNITY 6  //  C SHARP  //  TWO BATTLEFRONTS",
        get_font(17),
        IVORY,
    )

    draw_frame(draw, width, height)

    canvas.convert("RGB").save(
        MEDIA / "hero.png",
        optimize=True,
    )


# ============================================================
# SECTION BACKGROUNDS
# No screenshots — only generated tactical graphics.
# ============================================================

def draw_radar(draw):
    cx, cy = 1135, 56

    for radius in (18, 32, 45):
        draw.ellipse(
            (
                cx - radius,
                cy - radius,
                cx + radius,
                cy + radius,
            ),
            outline=TEAL,
            width=2,
        )

    draw.line(
        (cx - 46, cy, cx + 46, cy),
        fill=TEAL,
    )

    draw.line(
        (cx, cy - 45, cx, cy + 45),
        fill=TEAL,
    )

    draw.line(
        (cx, cy, cx + 27, cy - 26),
        fill=GOLD,
        width=3,
    )

    draw.ellipse(
        (cx + 22, cy - 31, cx + 30, cy - 23),
        fill=GOLD,
    )


def draw_system_network(draw):
    nodes = [
        (1060, 37),
        (1120, 55),
        (1183, 30),
        (1222, 70),
        (1149, 85),
    ]

    edges = [
        (0, 1),
        (1, 2),
        (1, 4),
        (2, 3),
        (3, 4),
    ]

    for a, b in edges:
        draw.line(
            (*nodes[a], *nodes[b]),
            fill=TEAL,
            width=2,
        )

    for index, (x, y) in enumerate(nodes):
        color = GOLD if index == 1 else TEAL

        draw.rectangle(
            (x - 6, y - 6, x + 6, y + 6),
            fill=PANEL,
            outline=color,
            width=2,
        )


def draw_workshop(draw):
    # Blueprint-like component and circuit traces
    draw.rounded_rectangle(
        (1080, 27, 1186, 81),
        radius=5,
        outline=TEAL,
        width=2,
    )

    draw.rectangle(
        (1112, 40, 1155, 68),
        outline=GOLD,
        width=2,
    )

    for x in (1098, 1118, 1138, 1158, 1178):
        draw.line(
            (x, 19, x, 27),
            fill=TEAL,
            width=2,
        )

        draw.line(
            (x, 81, x, 92),
            fill=TEAL,
            width=2,
        )

    draw.line(
        (1035, 55, 1080, 55),
        fill=GOLD,
        width=2,
    )

    draw.line(
        (1186, 55, 1231, 55),
        fill=GOLD,
        width=2,
    )


def draw_terminal(draw):
    draw.rounded_rectangle(
        (1053, 23, 1223, 87),
        radius=6,
        fill=(9, 26, 30, 255),
        outline=TEAL,
        width=2,
    )

    # Terminal prompt
    draw.line(
        (1070, 43, 1085, 55, 1070, 67),
        fill=GOLD,
        width=3,
    )

    draw.line(
        (1098, 68, 1140, 68),
        fill=GOLD,
        width=2,
    )

    for x in (1099, 1128, 1157, 1186):
        draw.rectangle(
            (x, 39, x + 16, 43),
            fill=TEAL,
        )


DECORATIONS = {
    "field": draw_radar,
    "systems": draw_system_network,
    "workshop": draw_workshop,
    "deploy": draw_terminal,
}


def section_background(key):
    width, height = SECTION_SIZE

    canvas = gradient_background(width, height)
    draw = ImageDraw.Draw(canvas)

    draw_grid(draw, width, height)
    draw_noise(draw, width, height)

    # Text region is solid and opaque.
    # No gameplay screenshots or HUD elements.
    draw.rounded_rectangle(
        (126, 15, 998, 97),
        radius=7,
        fill=PANEL,
        outline=GOLD_DARK,
        width=1,
    )

    # Decorative region
    draw.rounded_rectangle(
        (1015, 15, 1245, 97),
        radius=5,
        fill=(10, 27, 31, 255),
        outline=TEAL,
        width=1,
    )

    DECORATIONS[key](draw)

    return canvas


# ============================================================
# SECTION NUMBER
# ============================================================

def draw_number_badge(draw, number):
    rect = (25, 22, 94, 90)

    draw.rounded_rectangle(
        rect,
        radius=4,
        fill=(5, 12, 17, 255),
        outline=GOLD,
        width=2,
    )

    centered_text(
        draw,
        rect,
        number,
        get_font(30),
        GOLD,
    )


# ============================================================
# SECTION BANNERS
# ============================================================

def build_section(key, number, title, subtitle):
    width, height = SECTION_SIZE

    canvas = section_background(key)
    draw = ImageDraw.Draw(canvas)

    draw_number_badge(draw, number)

    # Only a vertical separator.
    # No horizontal line near the subtitle.
    draw.line(
        (111, 24, 111, 88),
        fill=GOLD,
        width=2,
    )

    title_font = fitted_font(
        draw,
        title,
        31,
        815,
    )

    draw_text(
        draw,
        (145, 23),
        title,
        title_font,
        IVORY,
    )

    subtitle_font = fitted_font(
        draw,
        subtitle,
        16,
        790,
    )

    draw_text(
        draw,
        (147, 73),
        subtitle,
        subtitle_font,
        GOLD,
    )

    draw_frame(draw, width, height)

    canvas.convert("RGB").save(
        MEDIA / f"section-{key}.png",
        optimize=True,
    )


def build_map_carousel():
    import base64
    from io import BytesIO
    from PIL import Image, ImageOps, ImageDraw, ImageFont

    width, body, footer = 960, 500, 44
    hold, transition = 3.0, 0.4
    duration = 2 * (hold + transition)

    font_path = (
        ROOT / "Assets/Game/Art/UI/bd_ui_hud_pixel_v001.ttf"
    )

    maps = [
        (
            "DANUBE BORDER",
            "OPEN RIVERBANK / LAYERED DEFENSE",
            ROOT / "Docs/Validation/Phase6E/"
            "encounter-25-CoastalDefense_TopDown.png",
        ),
        (
            "INDUSTRIAL PORT",
            "CARGO DOCKS / MULTIPLE ROUTES",
            ROOT / "Docs/Validation/Phase6E/"
            "encounter-25-DanubeIndustrialPort.png",
        ),
    ]

    def slide(title, subtitle, path, number):
        if not path.is_file():
            raise FileNotFoundError(
                f"Missing map screenshot: {path}"
            )

        with Image.open(path) as source:
            canvas = ImageOps.fit(
                source.convert("RGB"),
                (width, body),
                method=Image.Resampling.LANCZOS,
            ).convert("RGBA")

        # Dark opaque label area
        overlay = Image.new("RGBA", canvas.size)
        dark = ImageDraw.Draw(overlay)
        dark.rectangle(
            (0, body - 100, width, body),
            fill=(5, 16, 20, 237),
        )

        canvas = Image.alpha_composite(
            canvas, overlay
        ).convert("RGB")

        d = ImageDraw.Draw(canvas)
        d.line(
            (0, body - 100, width, body - 100),
            fill="#C18A3E",
            width=3,
        )

        def font(size):
            return ImageFont.truetype(
                str(font_path), size
            )

        d.text(
            (24, body - 82),
            title,
            font=font(34),
            fill="#F1E5C6",
        )

        d.text(
            (26, body - 38),
            subtitle,
            font=font(16),
            fill="#DBA750",
        )

        d.text(
            (width - 106, body - 55),
            f"{number} / 02",
            font=font(18),
            fill="#D2D7CA",
        )

        return canvas

    frames = [
        slide(title, subtitle, path, i + 1)
        for i, (title, subtitle, path)
        in enumerate(maps)
    ]

    # Embed images directly into SVG
    def uri(img):
        buffer = BytesIO()
        img.save(
            buffer,
            format="JPEG",
            quality=91,
            optimize=True,
            subsampling=0,
        )
        encoded = base64.b64encode(
            buffer.getvalue()
        ).decode("ascii")

        return "data:image/jpeg;base64," + encoded

    images = [uri(img) for img in frames]

    # Third image clones the first for seamless looping
    sequence = [images[0], images[1], images[0]]

    images_xml = "".join(
        f'<image href="{src}" x="{i * width}" '
        f'y="0" width="{width}" height="{body}"/>'
        for i, src in enumerate(sequence)
    )

    times = (
        f"0;{hold/duration:.8f};"
        f"{(hold+transition)/duration:.8f};"
        f"{(2*hold+transition)/duration:.8f};1"
    )

    values = (
        f"0 0;0 0;-{width} 0;"
        f"-{width} 0;-{2*width} 0"
    )

    switches = (
        f"0;{(hold+transition/2)/duration:.8f};"
        f"{(2*hold+1.5*transition)/duration:.8f};1"
    )

    # Animated slide indicators
    dots = ""

    for i in range(2):
        x = width // 2 - 34 + i * 44

        states = (
            "1;0;1;1" if i == 0
            else "0;1;0;0"
        )

        dots += (
            f'<rect x="{x}" y="{body+20}" '
            f'rx="3" width="24" height="5" '
            f'fill="#3C5257"/>'
        )

        dots += (
            f'<rect x="{x}" y="{body+20}" '
            f'rx="3" width="24" height="5" '
            f'fill="#D49A2F">'
            f'<animate attributeName="opacity" '
            f'values="{states}" '
            f'keyTimes="{switches}" '
            f'calcMode="discrete" '
            f'dur="{duration}s" '
            f'repeatCount="indefinite"/>'
            f'</rect>'
        )

    svg = (
        f'<svg xmlns="http://www.w3.org/2000/svg" '
        f'width="{width}" height="{body+footer}" '
        f'viewBox="0 0 {width} {body+footer}" '
        f'role="img">'
        '<title>POTAMORPH — Danube Border and '
        'Industrial Port</title>'
        '<defs><clipPath id="carousel-frame">'
        '<rect width="960" height="500"/>'
        '</clipPath></defs>'
        '<g clip-path="url(#carousel-frame)"><g>'
        f'{images_xml}'
        '<animateTransform '
        'attributeName="transform" '
        'type="translate" '
        f'values="{values}" '
        f'keyTimes="{times}" '
        'keySplines="0 0 1 1;0.22 1 0.36 1;'
        '0 0 1 1;0.22 1 0.36 1" '
        'calcMode="spline" '
        f'dur="{duration}s" '
        'repeatCount="indefinite"/>'
        '</g></g>'
        f'<rect y="{body}" width="{width}" '
        f'height="{footer}" fill="#08171B"/>'
        f'{dots}'
        f'<rect x="1" y="1" '
        f'width="{width-2}" '
        f'height="{body+footer-2}" '
        'fill="none" stroke="#917135" '
        'stroke-width="2"/>'
        '</svg>'
    )

    (MEDIA / "maps-carousel.svg").write_text(
        svg, encoding="utf-8"
    )

    print("Generated maps-carousel.svg")

# ============================================================
# MAIN
# ============================================================

def main():
    build_hero()

    sections = [
        (
            "field",
            "01",
            "HOLD THE RIVER",
            "MAPS / GAMEPLAY / DEFENSES",
        ),
        (
            "systems",
            "02",
            "INSIDE THE DEFENSE LINE",
            "COMMANDS / STATE / PRESENTATION",
        ),
        (
            "workshop",
            "03",
            "BUILT WITH CODEX",
            "ITERATION / ART PIPELINE / TESTING",
        ),
        (
            "deploy",
            "04",
            "OPEN THE PROJECT",
            "SETUP / CONTROLS / RUNNING THE GAME",
        ),
    ]

    build_map_carousel()

    for key, number, title, subtitle in sections:
        build_section(key, number, title, subtitle)

    print("POTAMORPH media generated successfully.")
    print(f"Output: {MEDIA}")
    print("Hero: 1280x350")
    print("Sections: 1280x112")


if __name__ == "__main__":
    main()
