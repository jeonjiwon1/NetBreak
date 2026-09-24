"""Draw deterministic VS-2B-3 swim sheets on native pixel grids."""

from pathlib import Path
from math import cos, pi, sin

from PIL import Image, ImageDraw


ROOT = Path(__file__).resolve().parents[1] / "Assets/Art/Fish"
DIRECTIONS = ((1, 0), (0, -1), (0.70710678, -0.70710678),
              (-0.70710678, -0.70710678))


def draw_frame(species, cell, forward, phase):
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

    def disc(u, v, radius, color):
        polygon([(u + radius * cos(i * 2 * pi / 24),
                  v + radius * sin(i * 2 * pi / 24)) for i in range(24)], color)

    if species == "Pufferfish":
        ink = (29, 54, 67, 255)
        shadow = (132, 104, 62, 255)
        gold = (230, 174, 77, 255)
        yellow = (250, 215, 113, 255)
        cream = (248, 238, 190, 255)
        highlight = (255, 249, 213, 255)
        fin = (191, 125, 60, 255)
        bend = (0, 3, 0, -3)[phase]
        flap = (0, -2, 0, 2)[phase]

        # The body stays the same size in every frame; only fins and tail swim.
        polygon([(-15, 0), (-21, -7 + bend), (-20, bend),
                 (-21, 7 + bend)], ink)
        polygon([(-17, 0), (-20, -4 + bend), (-19, bend),
                 (-20, 4 + bend)], fin)
        polygon([(-6, -13), (-12, -18 + flap), (-1, -14)], ink)
        polygon([(-5, -13), (-10, -16 + flap), (-2, -13)], gold)
        polygon([(-7, 12), (-11, 17 - flap), (0, 14)], ink)
        polygon([(-5, 12), (-9, 15 - flap), (-1, 13)], fin)
        disc(0, 0, 15, ink)
        disc(0, 0, 13, gold)
        polygon([(-11, -5), (-7, -10), (2, -11), (10, -7),
                 (13, -2), (12, 1), (5, 0), (-5, -1)], yellow)
        polygon([(-11, 3), (-5, 7), (3, 10), (11, 6),
                 (13, 2), (5, 2), (-4, 2)], cream)
        # Short muzzle and a small eye distinguish the heading at game scale.
        polygon([(12, -3), (17, -2), (18, 1), (14, 3), (12, 2)], ink)
        polygon([(14, -1), (17, -1), (16, 1), (13, 1)], cream)
        disc(8, -5, 2, ink)
        draw.point(pt(8, -6), fill=highlight)
        for u, v in ((-7, -8), (-2, -9), (1, -6), (-10, -3)):
            draw.point(pt(u, v), fill=shadow)
        line([(-8, 8), (-2, 10), (4, 9)], highlight)
    else:
        ink = (29, 47, 70, 255)
        deep = (80, 64, 112, 255)
        violet = (139, 91, 152, 255)
        coral = (198, 125, 166, 255)
        pale = (247, 195, 192, 255)
        highlight = (255, 229, 208, 255)
        sweep = (0, 3, 0, -3)[phase]
        pulse = (0, 2, 0, -2)[phase]

        # Mantle-first swimming: pointed body leads, arms trail behind.
        for offset, length in ((-6, -26), (-2, -28), (2, -27), (6, -25)):
            tip = offset + sweep * (1 if offset > 0 else -1)
            line([(-10, offset // 2), (-18, offset), (length, tip)], ink, 3)
            line([(-12, offset // 2), (-20, offset), (length + 2, tip)], coral)
        polygon([(-8, -7), (-14, -12 + pulse), (-12, -2)], ink)
        polygon([(-8, 7), (-14, 12 - pulse), (-12, 2)], ink)
        polygon([(-8, -6), (-12, -9 + pulse), (-11, -2)], violet)
        polygon([(-8, 6), (-12, 9 - pulse), (-11, 2)], violet)
        polygon([(-14, -5), (-10, -11), (1, -13), (13, -10),
                 (25, -5), (29, 0), (25, 5), (13, 10),
                 (1, 13), (-10, 11), (-14, 5)], ink)
        polygon([(-12, -4), (-8, -9), (2, -11), (13, -8),
                 (24, -4), (27, 0), (23, 4), (12, 8),
                 (2, 11), (-8, 9), (-12, 4)], violet)
        polygon([(-9, -5), (2, -9), (13, -7), (23, -3),
                 (26, 0), (20, 2), (9, 1), (-5, 2)], coral)
        polygon([(-8, 3), (2, 8), (12, 6), (20, 3),
                 (9, 3), (-3, 2)], pale)
        line([(1, -7), (12, -6), (22, -2)], highlight)
        disc(-8, -4, 3, ink)
        draw.point(pt(-8, -5), fill=highlight)
        draw.point(pt(-12, 3), fill=deep)

    return image


for species, cell in (("Pufferfish", 48), ("Squid", 64)):
    sheet = Image.new("RGBA", (cell * 4, cell * 4), (0, 0, 0, 0))
    for row, direction in enumerate(DIRECTIONS):
        for col in range(4):
            sheet.alpha_composite(draw_frame(species, cell, direction, col),
                                  (col * cell, row * cell))
    output = ROOT / species / f"{species}_Swim.png"
    output.parent.mkdir(parents=True, exist_ok=True)
    sheet.save(output)
    print(output)
