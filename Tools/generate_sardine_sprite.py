"""Draw the VS-2B-1 sardine sheet directly on a 32-pixel grid."""

from pathlib import Path
from PIL import Image, ImageDraw


CELL = 32
SHEET = Image.new("RGBA", (128, 96), (0, 0, 0, 0))
INK = (24, 49, 68, 255)
BACK = (42, 108, 125, 255)
TEAL = (66, 153, 157, 255)
BELLY = (190, 221, 207, 255)
LIGHT = (239, 244, 212, 255)
FIN = (42, 91, 110, 255)


def frame(forward, phase):
    image = Image.new("RGBA", (CELL, CELL), (0, 0, 0, 0))
    draw = ImageDraw.Draw(image)
    fx, fy = forward
    px, py = -fy, fx

    def pt(u, v):
        return (round(15.5 + fx * u + px * v),
                round(15.5 + fy * u + py * v))

    def polygon(points, color):
        draw.polygon([pt(u, v) for u, v in points], fill=color)

    # Tail swings across the direction of travel. Head and pivot never move.
    bend = (0, 2, -1, -2)[phase]
    polygon([(-10, 0), (-15, -5 + bend), (-13, bend),
             (-15, 5 + bend)], INK)
    polygon([(-11, 0), (-14, -4 + bend), (-12, bend),
             (-14, 4 + bend)], FIN)
    polygon([(-6, -4), (-9, -7), (-2, -5)], INK)
    polygon([(-5, -4), (-8, -6), (-2, -4)], FIN)
    polygon([(-5, 4), (-7, 7), (0, 5)], INK)

    shape = [(-11, 0), (-9, -3), (-6, -5), (1, -6),
             (7, -5), (11, -3), (14, 0), (11, 3),
             (7, 5), (1, 6), (-6, 5), (-9, 3)]
    polygon(shape, INK)
    polygon([(-9, 0), (-6, -4), (1, -5), (7, -4),
             (11, -2), (12, 0), (10, 2), (6, 3),
             (0, 4), (-6, 3)], BACK)
    polygon([(-8, 0), (-5, -3), (2, -4), (8, -3),
             (11, -1), (8, 0), (1, 1), (-5, 2)], TEAL)
    polygon([(-8, 1), (-3, 3), (3, 4), (8, 3),
             (11, 1), (7, 2), (1, 2), (-5, 1)], BELLY)
    draw.line([pt(-4, -3), pt(3, -3), pt(8, -2)], fill=LIGHT, width=1)
    draw.point(pt(9, -2), fill=INK)
    draw.point(pt(10, -2), fill=LIGHT)
    draw.point(pt(12, 1), fill=INK)
    return image


directions = ((1, 0), (0, -1), (0.70710678, -0.70710678))
for row, direction in enumerate(directions):
    for col in range(4):
        SHEET.alpha_composite(frame(direction, col), (col * CELL, row * CELL))

output = Path(__file__).resolve().parents[1] / "Assets/Art/Fish/Sardine/Sardine_Swim.png"
output.parent.mkdir(parents=True, exist_ok=True)
SHEET.save(output)
print(output)
