"""Generate native-grid squid action, ink puff and a short internal prototype SFX."""

from pathlib import Path
from math import cos, pi, sin
import random
import struct
import wave

from PIL import Image, ImageDraw


ROOT = Path(__file__).resolve().parents[1] / "Assets"
FISH = ROOT / "Art/Fish/Squid"
VFX = ROOT / "Art/VFX/Squid"
AUDIO = ROOT / "Audio/SFX/SpecialFish"

SWIM = Image.open(FISH / "Squid_Swim.png").convert("RGBA")
ATTACK = Image.new("RGBA", (256, 256), (0, 0, 0, 0))
DIRECTIONS = ((1, 0), (0, -1), (.70710678, -.70710678),
              (-.70710678, -.70710678))
OUTLINE = (29, 47, 70, 255)
DEEP = (80, 64, 112, 255)
VIOLET = (139, 91, 152, 255)
INK = (40, 24, 66, 255)
INK_LIGHT = (92, 51, 115, 255)

for row, (fx, fy) in enumerate(DIRECTIONS):
    px, py = -fy, fx

    def point(u, v):
        return (round(31.5 + fx * u + px * v),
                round(31.5 + fy * u + py * v))

    for frame in range(4):
        cell = SWIM.crop((frame * 64, row * 64, (frame + 1) * 64, (row + 1) * 64))
        draw = ImageDraw.Draw(cell)
        # Keep the approved body, outline, palette and eye. Change only the arm
        # roots and ink sac on the same 64 px grid.
        if frame == 0:
            draw.line([point(-12, -7), point(-17, -4)], fill=DEEP, width=2)
            draw.line([point(-12, 7), point(-17, 4)], fill=DEEP, width=2)
        elif frame == 1:
            draw.polygon([point(-15, -5), point(-22, -4), point(-24, 0),
                          point(-22, 4), point(-15, 5)], fill=OUTLINE)
            draw.polygon([point(-17, -3), point(-21, -2), point(-23, 0),
                          point(-21, 2), point(-17, 3)], fill=VIOLET)
        elif frame == 2:
            draw.polygon([point(-16, -6), point(-25, -6), point(-30, -2),
                          point(-30, 2), point(-25, 6), point(-16, 6)], fill=OUTLINE)
            draw.polygon([point(-19, -4), point(-26, -4), point(-29, 0),
                          point(-26, 4), point(-19, 4)], fill=INK)
            draw.line([point(-22, -3), point(-27, -2)], fill=INK_LIGHT, width=2)
        else:
            draw.polygon([point(-17, -4), point(-23, -3), point(-25, 0),
                          point(-23, 3), point(-17, 4)], fill=OUTLINE)
            draw.polygon([point(-19, -2), point(-23, -2), point(-24, 0),
                          point(-23, 2), point(-19, 2)], fill=DEEP)
        ATTACK.alpha_composite(cell, (frame * 64, row * 64))

FISH.mkdir(parents=True, exist_ok=True)
ATTACK.save(FISH / "Squid_InkAttack.png")

PUFF = Image.new("RGBA", (128, 32), (0, 0, 0, 0))
for frame, radius in enumerate((4, 8, 12, 10)):
    cell = Image.new("RGBA", (32, 32), (0, 0, 0, 0))
    draw = ImageDraw.Draw(cell)
    cx = cy = 15
    draw.ellipse((cx-radius, cy-radius+2, cx+radius, cy+radius-2), fill=OUTLINE)
    draw.ellipse((cx-radius+2, cy-radius+3, cx+radius-2, cy+radius-3), fill=INK)
    if frame > 0:
        for dx, dy, r in ((-radius, -radius//2, 2), (radius, radius//3, 2),
                          (radius//2, -radius, 1), (-radius//2, radius, 1)):
            draw.ellipse((cx+dx-r, cy+dy-r, cx+dx+r, cy+dy+r), fill=DEEP)
        draw.line((cx-3, cy-2, cx+2, cy-2), fill=INK_LIGHT, width=2)
    if frame == 3:
        for dx, dy in ((-5, -3), (4, 2), (0, 5)):
            draw.rectangle((cx+dx, cy+dy, cx+dx+2, cy+dy+2), fill=(0, 0, 0, 0))
    PUFF.alpha_composite(cell, (frame * 32, 0))
VFX.mkdir(parents=True, exist_ok=True)
PUFF.save(VFX / "Squid_InkPuff.png")

random.seed(2041)
rate = 44100
duration = .28
samples = []
low_noise = 0.0
for i in range(int(rate * duration)):
    t = i / rate
    attack = min(1.0, t / .025)
    decay = (1.0 - t / duration) ** 2
    low_noise = .82 * low_noise + .18 * random.uniform(-1, 1)
    bubble = sin(2 * pi * (150 * t + 280 * t * t))
    wet = .7 * low_noise + .3 * bubble
    sample = max(-1.0, min(1.0, wet * attack * decay * .42))
    samples.append(round(sample * 32767))
AUDIO.mkdir(parents=True, exist_ok=True)
with wave.open(str(AUDIO / "Squid_InkRelease.wav"), "wb") as output:
    output.setnchannels(1)
    output.setsampwidth(2)
    output.setframerate(rate)
    output.writeframes(struct.pack("<" + "h" * len(samples), *samples))

print("Generated 256x256 action, 128x32 puff and 44.1 kHz mono SFX")
