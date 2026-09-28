"""Generate the Area 1 shark's 4-direction, 4-frame top-down swim sheet."""

from pathlib import Path
from PIL import Image, ImageDraw


CELL = 96
OUT = Path(__file__).resolve().parents[1] / "Assets/Art/Fish/CoastBoss/CoastBoss_Swim.png"
INK = "#102b3e"
SHADOW = "#264b62"
BACK = "#416e83"
MID = "#579b9f"
LIGHT = "#a8d2c9"
BELLY = "#d4ded0"


def frame(phase):
    im = Image.new("RGBA", (CELL, CELL))
    d = ImageDraw.Draw(im)
    wag = (0, 2, 0, -2)[phase]
    # Forked tail, narrow tail stock, broad shoulders, and a pointed snout.
    d.polygon([(29, 47), (7, 23 + wag), (12, 45 + wag),
               (8, 73 + wag), (30, 52)], fill=INK)
    d.polygon([(26, 47), (11, 30 + wag), (16, 46 + wag),
               (11, 66 + wag), (27, 51)], fill=BACK)
    d.polygon([(24, 46), (12, 30 + wag), (18, 43 + wag),
               (24, 45)], fill=LIGHT)
    d.polygon([(26, 50), (16, 53 + wag), (12, 65 + wag)], fill=SHADOW)

    # Paired pectoral fins and a long triangular dorsal profile.
    d.polygon([(43, 39), (38, 12), (55, 20), (66, 38)], fill=INK)
    d.polygon([(45, 37), (42, 18), (54, 23), (61, 38)], fill=SHADOW)
    d.line([(41, 17), (53, 22), (61, 35)], fill=LIGHT, width=2)
    d.polygon([(45, 56), (39, 82), (55, 75), (69, 58)], fill=INK)
    d.polygon([(47, 58), (43, 76), (56, 71), (64, 58)], fill=SHADOW)
    d.line([(43, 77), (54, 72), (63, 58)], fill=MID, width=2)

    body = [(24, 45), (32, 40), (43, 35), (58, 33), (72, 37),
            (84, 43), (91, 48), (84, 53), (70, 60), (53, 63),
            (38, 58), (28, 52)]
    d.polygon(body, fill=INK)
    d.polygon([(29, 45), (42, 39), (57, 37), (70, 40),
               (83, 45), (87, 48), (81, 50), (69, 53),
               (54, 55), (41, 54), (30, 50)], fill=BACK)
    d.polygon([(30, 45), (43, 39), (59, 37), (71, 41),
               (83, 45), (75, 46), (62, 44), (48, 44),
               (38, 47)], fill=MID)
    d.line([(34, 43), (45, 38), (60, 36), (71, 40), (79, 43)], fill=LIGHT, width=2)
    d.polygon([(43, 54), (56, 56), (70, 54), (84, 50),
               (78, 54), (66, 58), (53, 60), (41, 56)], fill=BELLY)
    d.line([(48, 52), (60, 53), (72, 50)], fill=SHADOW, width=2)
    # Small high dorsal fin on the back, offset to stay visible from above.
    d.polygon([(48, 40), (46, 25), (60, 39)], fill=INK)
    d.polygon([(49, 37), (49, 29), (56, 37)], fill=BACK)
    d.line([(48, 27), (57, 38)], fill=LIGHT, width=1)
    # Eye, gill cuts, and a restrained mouth line.
    d.rectangle((78, 45, 80, 47), fill=INK)
    d.point((79, 45), fill=BELLY)
    for x in (68, 72, 76):
        d.line([(x, 46), (x - 1, 49)], fill=INK, width=1)
    d.line([(81, 51), (86, 49)], fill=INK, width=1)
    return im


def main():
    sheet = Image.new("RGBA", (CELL * 4, CELL * 4))
    # Rows: east, north, north-east, north-west. Westward headings flip both axes.
    for row, angle in enumerate((0, 90, 45, 135)):
        for col in range(4):
            sprite = frame(col).rotate(angle, Image.Resampling.NEAREST)
            sheet.alpha_composite(sprite, (col * CELL, row * CELL))
    OUT.parent.mkdir(parents=True, exist_ok=True)
    sheet.save(OUT)


if __name__ == "__main__":
    main()
