"""Draw the VS-2C-2 ink blob and splat on small transparent pixel grids."""

from pathlib import Path
import re
from uuid import UUID, uuid5

from PIL import Image, ImageDraw


DEST = Path(__file__).resolve().parents[1] / "Assets/Art/VFX/Squid"
SOURCE_META = DEST / "Squid_InkPuff.png.meta"
OUTLINE = (29, 47, 70, 255)
INK = (40, 24, 66, 255)
DEEP = (80, 64, 112, 255)
HIGHLIGHT = (92, 51, 115, 255)


def projectile(frame):
    image = Image.new("RGBA", (16, 16))
    draw = ImageDraw.Draw(image)
    shapes = (
        [(4, 7), (6, 4), (10, 4), (13, 7), (11, 11), (6, 12), (3, 9)],
        [(3, 8), (5, 5), (9, 4), (12, 6), (13, 10), (9, 12), (5, 11)],
        [(4, 6), (7, 4), (11, 5), (13, 8), (10, 12), (5, 11), (3, 9)],
        [(3, 7), (6, 4), (10, 5), (12, 7), (12, 10), (8, 12), (4, 10)],
    )
    draw.polygon(shapes[frame], fill=OUTLINE)
    draw.polygon([(5, 7), (7, 5), (10, 6), (11, 8), (9, 10), (6, 10)], fill=INK)
    draw.point((7 + frame % 2, 6), fill=HIGHLIGHT)
    draw.rectangle((1, 6 + frame % 2, 2, 7 + frame % 2), fill=DEEP)
    draw.point((2, 11 - frame % 2), fill=INK)
    return image


def impact(frame):
    image = Image.new("RGBA", (24, 24))
    draw = ImageDraw.Draw(image)
    if frame == 0:
        draw.ellipse((8, 8, 16, 15), fill=OUTLINE)
        draw.ellipse((10, 9, 15, 13), fill=INK)
    else:
        radius = (0, 6, 9, 7)[frame]
        draw.ellipse((12 - radius, 12 - radius // 2,
                      12 + radius, 12 + radius // 2), fill=OUTLINE)
        draw.ellipse((14 - radius, 13 - radius // 2,
                      10 + radius, 11 + radius // 2), fill=INK)
        for x, y in ((12, 3), (20, 7), (21, 15), (14, 20), (4, 17), (3, 7)):
            dx, dy = x - 12, y - 12
            px = 12 + dx * radius // 9
            py = 12 + dy * radius // 9
            draw.rectangle((px, py, px + 1, py + 1), fill=DEEP)
        if frame < 3:
            draw.line((8, 10, 12, 9), fill=HIGHLIGHT, width=1)
        else:
            draw.rectangle((10, 10, 13, 11), fill=(0, 0, 0, 0))
    return image


DEST.mkdir(parents=True, exist_ok=True)
for filename, cell, painter in (
    ("Squid_InkProjectile.png", 16, projectile),
    ("Squid_InkImpact.png", 24, impact),
):
    sheet = Image.new("RGBA", (cell * 4, cell))
    for index in range(4):
        sheet.alpha_composite(painter(index), (index * cell, 0))
    sheet.save(DEST / filename)

    # Preserve the proven Unity 6 Sprite importer layout and assign fresh asset
    # and Sprite IDs. The Editor Setup menu can later reapply these same slices.
    meta_path = DEST / (filename + ".meta")
    if not meta_path.exists() or meta_path.stat().st_size < 100:
        template = SOURCE_META.read_text(encoding="utf-8")
        name = filename.removesuffix(".png")
        stem = "squid_ink_projectile" if cell == 16 else "squid_ink_impact"
        guid = uuid5(UUID("e31bf135-9ad2-4054-810e-bfe266cff744"), name).hex
        template = template.replace("941085e75227f934db2b32a097d45215", guid)
        template = template.replace("Squid_InkPuff", name)
        template = template.replace("squid_ink_puff", stem)
        template = re.sub(r"(?m)^(        x: )(0|32|64|96)$",
                          lambda match: match.group(1) + str(int(match.group(2)) * cell // 32),
                          template)
        template = template.replace("width: 32", f"width: {cell}")
        template = template.replace("height: 32", f"height: {cell}")
        template = re.sub(r"(?m)^(      spriteID: )([0-9a-f]{32})$",
                          lambda match: match.group(1) +
                          uuid5(UUID("e31bf135-9ad2-4054-810e-bfe266cff744"),
                                name + match.group(2)).hex,
                          template)
        meta_path.write_text(template, encoding="utf-8", newline="\n")

print("Generated 4x16px projectile and 4x24px impact RGBA sheets")
