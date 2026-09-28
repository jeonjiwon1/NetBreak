"""Draw the Area 1 giant tuna's directional swim sheet on its native pixel grid."""

from pathlib import Path
from PIL import Image, ImageDraw


CELL = 88
OUT = Path(__file__).resolve().parents[1] / "Assets/Art/Fish/CoastMiniBoss/CoastMiniBoss_Swim.png"
DIRECTIONS = ((1, 0), (0, -1), (0.70710678, -0.70710678),
              (-0.70710678, -0.70710678))
INK = (19, 42, 63, 255)
DEEP = (30, 68, 96, 255)
BACK = (42, 91, 124, 255)
BLUE = (55, 124, 149, 255)
TEAL = (75, 157, 162, 255)
BELLY = (181, 218, 208, 255)
LIGHT = (232, 242, 220, 255)
GOLD = (220, 182, 89, 255)


def frame(forward, phase):
    image = Image.new("RGBA", (CELL, CELL))
    draw = ImageDraw.Draw(image)
    fx, fy = forward
    px, py = -fy, fx
    center = (CELL - 1) / 2

    def pt(u, v):
        return (round(center + fx * u + px * v),
                round(center + fy * u + py * v))

    def polygon(points, color):
        draw.polygon([pt(u, v) for u, v in points], fill=color)

    def line(points, color, width=1):
        draw.line([pt(u, v) for u, v in points], fill=color, width=width)

    bend = (0, 3, 1, -3)[phase]
    # Long, deeply forked tuna tail and a narrow stock; stronger than normal Tuna.
    polygon([(-29, 0), (-39, -20 + bend), (-36, -8 + bend),
             (-34, -2), (-36, 8 + bend), (-39, 20 + bend)], INK)
    polygon([(-32, 0), (-38, -16 + bend), (-35, -7 + bend),
             (-35, 7 + bend), (-38, 16 + bend)], DEEP)
    line([(-37, -18 + bend), (-34, -5 + bend), (-31, -1)], BELLY)

    # Thin swept fins and finlets keep this silhouette distinct from the shark.
    polygon([(-9, -14), (-18, -25), (-6, -18), (6, -15)], INK)
    polygon([(-8, -15), (-15, -22), (-4, -17)], DEEP)
    line([(-16, -24), (-8, -17), (-3, -16)], GOLD)
    polygon([(-10, 14), (-18, 25), (-5, 18), (7, 15)], INK)
    polygon([(-8, 15), (-15, 22), (-3, 17)], DEEP)
    line([(-16, 24), (-8, 17)], GOLD)
    polygon([(12, -12), (4, -20), (21, -11)], INK)
    polygon([(12, -13), (7, -18), (19, -11)], BACK)
    polygon([(12, 12), (4, 20), (21, 11)], INK)
    polygon([(12, 13), (7, 18), (19, 11)], BACK)
    for u in (-27, -22, -17):
        polygon([(u, -9), (u - 2, -13), (u + 3, -10)], INK)
        draw.point(pt(u - 1, -12), fill=GOLD)
        polygon([(u, 9), (u - 2, 13), (u + 3, 10)], INK)

    polygon([(-34, 0), (-29, -6), (-22, -11), (-8, -15),
             (10, -15), (24, -11), (34, -6), (39, -1),
             (39, 2), (34, 7), (23, 12), (10, 15),
             (-8, 15), (-22, 11), (-29, 6)], INK)
    polygon([(-32, 0), (-27, -6), (-20, -10), (-7, -13),
             (10, -13), (24, -9), (34, -4), (37, 0),
             (33, 5), (22, 10), (9, 13), (-8, 13),
             (-22, 9)], DEEP)
    polygon([(-29, -3), (-18, -10), (-4, -12), (11, -11),
             (25, -8), (35, -3), (31, 1), (17, 4),
             (-2, 4), (-19, 2)], BACK)
    polygon([(-26, -3), (-12, -9), (5, -10), (21, -7),
             (34, -3), (29, 0), (12, 2), (-6, 1)], BLUE)
    polygon([(-23, 0), (-9, -6), (7, -6), (23, -4),
             (32, -2), (25, 2), (9, 3), (-8, 2)], TEAL)
    polygon([(-30, 2), (-19, 7), (-4, 12), (11, 12),
             (25, 9), (37, 3), (31, 2), (18, 5),
             (1, 6), (-16, 3)], BELLY)
    line([(-20, -8), (-6, -11), (10, -11), (25, -7), (32, -4)], LIGHT, 2)
    line([(-8, 8), (7, 10), (23, 7)], LIGHT)
    line([(20, -7), (27, -5)], GOLD)
    draw.point(pt(32, -4), fill=INK)
    draw.point(pt(33, -4), fill=LIGHT)
    line([(32, 3), (38, 1)], INK)
    if phase == 2:
        draw.point(pt(3, -11), fill=LIGHT)
    return image


def main():
    sheet = Image.new("RGBA", (CELL * 4, CELL * 4))
    for row, direction in enumerate(DIRECTIONS):
        for col in range(4):
            sheet.alpha_composite(frame(direction, col), (col * CELL, row * CELL))
    OUT.parent.mkdir(parents=True, exist_ok=True)
    sheet.save(OUT)


if __name__ == "__main__":
    main()
