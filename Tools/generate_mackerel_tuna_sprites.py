"""Draw the VS-2B-2 directional fish sheets on their native pixel grids."""

from pathlib import Path
from PIL import Image, ImageDraw


ROOT = Path(__file__).resolve().parents[1] / "Assets/Art/Fish"
DIRECTIONS = ((1, 0), (0, -1), (0.70710678, -0.70710678),
              (-0.70710678, -0.70710678))


def make_frame(species, cell, forward, phase):
    image = Image.new("RGBA", (cell, cell), (0, 0, 0, 0))
    draw = ImageDraw.Draw(image)
    fx, fy = forward
    px, py = -fy, fx
    center = (cell - 1) / 2

    def pt(u, v):
        return (round(center + fx * u + px * v),
                round(center + fy * u + py * v))

    def polygon(points, color):
        draw.polygon([pt(u, v) for u, v in points], fill=color)

    def line(points, color, width=1):
        draw.line([pt(u, v) for u, v in points], fill=color, width=width)

    if species == "Mackerel":
        ink = (22, 47, 65, 255)
        back = (38, 91, 113, 255)
        teal = (57, 143, 151, 255)
        belly = (176, 218, 207, 255)
        light = (233, 244, 215, 255)
        fin = (34, 77, 98, 255)
        bend = (0, 3, 0, -3)[phase]
        polygon([(-16, 0), (-22, -7 + bend), (-20, bend),
                 (-22, 7 + bend)], ink)
        polygon([(-17, 0), (-21, -5 + bend), (-19, bend),
                 (-21, 5 + bend)], fin)
        polygon([(-8, -7), (-14, -10), (1, -8)], ink)
        polygon([(-7, -7), (-12, -9), (0, -7)], fin)
        polygon([(-9, 7), (-13, 10), (1, 8)], ink)
        polygon([(-16, 0), (-13, -5), (-8, -8), (3, -9),
                 (12, -7), (18, -4), (21, 0), (18, 4),
                 (12, 7), (3, 9), (-8, 8), (-13, 5)], ink)
        polygon([(-14, 0), (-11, -5), (-7, -7), (4, -8),
                 (13, -6), (18, -3), (19, 0), (16, 3),
                 (10, 5), (2, 7), (-8, 6)], back)
        polygon([(-12, -2), (-7, -6), (3, -7), (12, -5),
                 (18, -2), (15, 1), (5, 2), (-6, 2)], teal)
        polygon([(-13, 1), (-6, 4), (4, 6), (12, 5),
                 (18, 2), (14, 2), (4, 3), (-6, 2)], belly)
        line([(-10, -5), (-6, -6), (-4, -5)], ink)
        line([(-4, -6), (0, -7), (2, -5)], ink)
        line([(3, -6), (7, -6), (9, -4)], ink)
        line([(-7, -3), (4, -4), (12, -3)], light)
        draw.point(pt(16, -2), fill=ink)
        draw.point(pt(17, -2), fill=light)
        draw.point(pt(19, 1), fill=ink)
    else:
        ink = (21, 42, 63, 255)
        back = (34, 69, 98, 255)
        blue = (48, 105, 132, 255)
        teal = (74, 151, 158, 255)
        belly = (181, 218, 208, 255)
        light = (235, 243, 215, 255)
        fin = (35, 80, 104, 255)
        bend = (0, 4, 0, -4)[phase]
        # A narrow peduncle and forked tail distinguish tuna from mackerel.
        polygon([(-22, 0), (-29, -12 + bend), (-27, -4 + bend),
                 (-26, bend), (-27, 4 + bend), (-29, 12 + bend)], ink)
        polygon([(-25, 0), (-28, -9 + bend), (-27, -3 + bend),
                 (-27, 3 + bend), (-28, 9 + bend)], fin)
        polygon([(-13, -10), (-17, -14), (-2, -12), (4, -10)], ink)
        polygon([(-11, -10), (-15, -13), (1, -10)], fin)
        polygon([(-12, 10), (-17, 14), (1, 11)], ink)
        polygon([(-24, 0), (-21, -5), (-16, -10), (-5, -13),
                 (9, -13), (20, -10), (27, -6), (29, -2),
                 (29, 3), (26, 7), (19, 10), (9, 13),
                 (-5, 13), (-16, 10), (-21, 5)], ink)
        polygon([(-22, 0), (-19, -5), (-14, -9), (-4, -11),
                 (9, -11), (20, -8), (27, -4), (28, 1),
                 (24, 5), (17, 8), (7, 11), (-5, 11),
                 (-16, 8)], back)
        polygon([(-19, -3), (-11, -9), (2, -10), (15, -8),
                 (24, -5), (28, -2), (23, 2), (11, 3),
                 (-6, 3)], blue)
        polygon([(-17, -2), (-8, -7), (3, -8), (16, -6),
                 (25, -3), (22, 0), (7, 1), (-8, 1)], teal)
        polygon([(-21, 2), (-12, 6), (0, 10), (11, 9),
                 (21, 6), (28, 3), (23, 2), (10, 4),
                 (-4, 4), (-16, 2)], belly)
        line([(-12, -8), (1, -9), (14, -7), (23, -4)], light)
        for u in (-19, -16, -13):
            line([(u, -6), (u - 2, -10)], fin)
        draw.point(pt(23, -4), fill=ink)
        draw.point(pt(24, -4), fill=light)
        draw.point(pt(28, 1), fill=ink)

    return image


for species, cell in (("Mackerel", 48), ("Tuna", 64)):
    sheet = Image.new("RGBA", (4 * cell, 4 * cell), (0, 0, 0, 0))
    for row, direction in enumerate(DIRECTIONS):
        for col in range(4):
            sheet.alpha_composite(make_frame(species, cell, direction, col),
                                  (col * cell, row * cell))
    output = ROOT / species / f"{species}_Swim.png"
    output.parent.mkdir(parents=True, exist_ok=True)
    sheet.save(output)
    print(output)
