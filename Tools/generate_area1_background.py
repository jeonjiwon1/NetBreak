"""Generate the superseded procedural draft without replacing the selected art.

The active Area 1 image is Assets/Resources/Area1/CoastBackground.png.
"""

from pathlib import Path
from PIL import Image, ImageDraw


ROOT = Path(__file__).resolve().parents[1]
OUTPUT = ROOT / "Tools/CoastBackground_ProceduralDraft.png"
WIDTH, HEIGHT = 256, 144  # Draw on a pixel grid, then enlarge with nearest neighbor.

WATER = (113, 188, 200, 255)
WATER_LIGHT = (122, 197, 206, 255)
WATER_DARK = (103, 178, 193, 255)


def polygon(draw, points, color):
    draw.polygon(points, fill=color)


def water_layer():
    image = Image.new("RGBA", (WIDTH, HEIGHT), WATER)
    draw = ImageDraw.Draw(image)

    # Broad irregular water patches, with no vertical depth gradient or shore.
    polygon(draw, [(0, 0), (81, 0), (81, 12), (72, 12), (72, 22),
                   (57, 22), (57, 27), (32, 27), (32, 36), (0, 36)],
            WATER_LIGHT)
    polygon(draw, [(181, 0), (256, 0), (256, 65), (242, 65),
                   (242, 53), (224, 53), (224, 39), (210, 39),
                   (210, 29), (193, 29), (193, 13), (181, 13)],
            WATER_DARK)
    polygon(draw, [(0, 86), (16, 86), (16, 94), (30, 94), (30, 101),
                   (43, 101), (43, 113), (56, 113), (56, 125),
                   (72, 125), (72, 144), (0, 144)], WATER_DARK)
    polygon(draw, [(184, 144), (184, 131), (198, 131), (198, 118),
                   (211, 118), (211, 109), (228, 109), (228, 96),
                   (242, 96), (242, 84), (256, 84), (256, 144)],
            WATER_LIGHT)

    # A handful of small pixel blocks break the large shapes without noise.
    for x, y, w, h, color in ((35, 42, 13, 4, WATER_LIGHT),
                              (76, 69, 9, 3, WATER_DARK),
                              (173, 35, 15, 4, WATER_LIGHT),
                              (208, 74, 11, 3, WATER_DARK),
                              (82, 117, 9, 4, WATER_LIGHT),
                              (156, 108, 12, 3, WATER_DARK)):
        draw.rectangle((x, y, x + w, y + h), fill=color)
    return image


def underwater_layer(base):
    # These muted shapes are seen through water. The final PNG stays opaque.
    seabed = Image.new("RGBA", (WIDTH, HEIGHT), (0, 0, 0, 0))
    draw = ImageDraw.Draw(seabed)

    sand = (170, 185, 167, 31)
    sand_inner = (178, 189, 169, 18)
    shadow = (55, 132, 151, 31)
    rock = (70, 139, 151, 39)
    rock_light = (145, 186, 177, 31)
    seaweed = (48, 132, 134, 38)
    coral = (129, 157, 142, 26)

    # Irregular shallow sand pockets. No strip reaches across the viewport.
    polygon(draw, [(0, 55), (16, 55), (16, 60), (26, 60), (26, 69),
                   (40, 69), (40, 76), (34, 76), (34, 85), (19, 85),
                   (19, 90), (0, 90)], sand)
    polygon(draw, [(0, 64), (18, 64), (18, 70), (32, 70),
                   (32, 77), (18, 77), (18, 81), (0, 81)], sand_inner)
    polygon(draw, [(218, 65), (233, 65), (233, 60), (256, 60),
                   (256, 112), (240, 112), (240, 104), (223, 104),
                   (223, 96), (212, 96), (212, 77), (218, 77)], sand)
    polygon(draw, [(228, 75), (240, 75), (240, 69), (256, 69),
                   (256, 96), (235, 96), (235, 91), (224, 91)], sand_inner)
    polygon(draw, [(11, 124), (27, 124), (27, 119), (51, 119),
                   (51, 125), (64, 125), (64, 134), (58, 134),
                   (58, 144), (0, 144), (0, 135), (11, 135)], sand)

    # Top-down reef patches have closed outlines and no side-view horizon.
    for x, y, w, h in ((7, 72, 13, 7), (23, 94, 17, 8),
                       (5, 111, 15, 8), (218, 81, 19, 10),
                       (239, 116, 14, 8), (43, 129, 15, 7)):
        draw.ellipse((x - 4, y - 2, x + w + 5, y + h + 4), fill=shadow)
        polygon(draw, [(x, y + 2), (x + 3, y), (x + w - 4, y),
                       (x + w, y + 3), (x + w - 2, y + h),
                       (x + 3, y + h), (x, y + h - 2)], rock)
        draw.rectangle((x + 4, y + 2, x + w - 5, y + 3), fill=rock_light)

    # Small radial sea-grass forms are viewed from above, never upright.
    for x, y in ((13, 99), (43, 82), (34, 114),
                 (229, 53), (237, 102), (217, 124)):
        draw.rectangle((x - 2, y - 2, x + 2, y + 2), fill=seaweed)
        draw.line((x - 1, y - 2, x - 6, y - 8), fill=seaweed, width=2)
        draw.line((x + 1, y - 2, x + 6, y - 8), fill=seaweed, width=2)
        draw.line((x - 2, y + 1, x - 7, y + 5), fill=seaweed, width=2)
        draw.line((x + 2, y + 1, x + 7, y + 5), fill=seaweed, width=2)

    # Coral is a faint seabed accent, confined to the lateral edges.
    for x, y in ((26, 61), (47, 99), (226, 113), (247, 82)):
        draw.rectangle((x, y, x + 3, y + 5), fill=coral)
        draw.rectangle((x - 4, y + 1, x - 1, y + 3), fill=coral)
        draw.rectangle((x + 4, y - 2, x + 6, y + 1), fill=coral)

    return Image.alpha_composite(base, seabed)


def draw_background():
    image = underwater_layer(water_layer())
    image = image.resize((512, 288), Image.Resampling.NEAREST)
    OUTPUT.parent.mkdir(parents=True, exist_ok=True)
    image.save(OUTPUT, optimize=True)
    print(OUTPUT)


if __name__ == "__main__":
    draw_background()
