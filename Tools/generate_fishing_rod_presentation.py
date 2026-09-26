"""Generate original pixel sprites and a restrained fishing rod hit sound."""

from math import exp, pi, sin
from pathlib import Path
import struct
import wave

from PIL import Image, ImageDraw


ASSETS = Path(__file__).resolve().parents[1] / "Assets"
BODY = ASSETS / "Art/Tools/FishingRod"
VFX = ASSETS / "Art/VFX/Tools"
AUDIO = ASSETS / "Audio/SFX/Tools"
for folder in (BODY, VFX, AUDIO):
    folder.mkdir(parents=True, exist_ok=True)

INK = (35, 58, 62, 255)
WOOD = (144, 92, 57, 255)
WOOD_LIGHT = (208, 154, 91, 255)
METAL = (219, 235, 206, 255)
GOLD = (244, 209, 115, 255)
WATER = (185, 246, 225, 255)


def rod(bend: int) -> Image.Image:
    image = Image.new("RGBA", (32, 32))
    d = ImageDraw.Draw(image)
    # A dark, wide foot keeps the tool readable on light cyan water.
    d.polygon([(8, 27), (23, 27), (25, 30), (6, 30)], fill=INK)
    d.rectangle((10, 27, 21, 28), fill=WOOD)
    d.rectangle((14, 23, 17, 27), fill=INK)
    d.rectangle((15, 23, 16, 26), fill=METAL)
    # Angled pole and golden tip. Each attack pose bends only the upper shaft.
    tip = (24 + bend, 3 + bend)
    joints = [(16, 24), (18, 19), (20, 14), (22 + bend // 2, 9), tip]
    d.line(joints, fill=INK, width=4, joint="curve")
    d.line(joints, fill=WOOD, width=2, joint="curve")
    d.point((16, 22), fill=WOOD_LIGHT)
    d.point((18, 18), fill=WOOD_LIGHT)
    d.point((20, 14), fill=WOOD_LIGHT)
    d.ellipse((tip[0] - 1, tip[1] - 1, tip[0] + 1, tip[1] + 1), fill=GOLD)
    # Reel is a functional, distinct round silhouette.
    d.ellipse((9, 17, 17, 25), fill=INK)
    d.ellipse((11, 19, 15, 23), fill=METAL)
    d.point((13, 21), fill=INK)
    d.line((9, 21, 6, 21), fill=INK, width=2)
    d.point((6, 20), fill=GOLD)
    return image


idle = rod(0)
idle.save(BODY / "FishingRod_Idle.png")
attack = Image.new("RGBA", (96, 32))
for index, bend in enumerate((0, -2, -1)):
    attack.alpha_composite(rod(bend), (index * 32, 0))
attack.save(BODY / "FishingRod_Attack.png")

impact = Image.new("RGBA", (64, 16))
for index in range(4):
    cell = Image.new("RGBA", (16, 16))
    d = ImageDraw.Draw(cell)
    if index < 3:
        radius = (2, 4, 5)[index]
        d.arc((8 - radius, 8 - radius, 8 + radius, 8 + radius),
              185, 355, fill=INK, width=2)
        d.arc((8 - radius, 8 - radius, 8 + radius, 8 + radius),
              190, 350, fill=WATER, width=1)
        d.point((8, 7 - radius), fill=GOLD)
        d.point((3 + index, 10 - index), fill=METAL)
        d.point((12 - index, 11 - index), fill=METAL)
    else:
        d.point((3, 7), fill=WATER)
        d.point((12, 8), fill=WATER)
    impact.alpha_composite(cell, (index * 16, 0))
impact.save(VFX / "FishingRod_Hit.png")

rate = 44100
duration = 0.19
samples = []
for index in range(round(rate * duration)):
    t = index / rate
    envelope = min(1.0, t / 0.004) * exp(-25 * t)
    reel = sin(2 * pi * (820 * t - 1100 * t * t))
    water = 0.24 * sin(2 * pi * (470 * t - 900 * t * t))
    click = 0.12 * sin(2 * pi * 1500 * t) * exp(-95 * t)
    value = max(-1.0, min(1.0, (reel * 0.29 + water + click) * envelope))
    samples.append(round(value * 32767))
with wave.open(str(AUDIO / "FishingRod_Hit.wav"), "wb") as output:
    output.setnchannels(1)
    output.setsampwidth(2)
    output.setframerate(rate)
    output.writeframes(struct.pack(f"<{len(samples)}h", *samples))
