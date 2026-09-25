"""Generate internal pufferfish contact sprites and a short prototype one-shot."""

from math import cos, exp, pi, sin
from pathlib import Path
import struct
import wave

from PIL import Image, ImageDraw


ASSETS = Path(__file__).resolve().parents[1] / "Assets"
FISH = ASSETS / "Art/Fish/Pufferfish"
VFX = ASSETS / "Art/VFX/Pufferfish"
AUDIO = ASSETS / "Audio/SFX/SpecialFish"
SWIM = Image.open(FISH / "Pufferfish_Swim.png").convert("RGBA")

# Copy approved pixels into every direction and alter only the contact pose.
action = Image.new("RGBA", (192, 192))
for row in range(4):
    for frame, scale in enumerate((1.0, 1.04, 1.09, 1.0)):
        source = SWIM.crop((frame * 48, row * 48, (frame + 1) * 48, (row + 1) * 48))
        if scale == 1.0:
            cell = source
        else:
            size = round(48 * scale)
            enlarged = source.resize((size, size), Image.Resampling.NEAREST)
            left = (size - 48) // 2
            cell = enlarged.crop((left, left, left + 48, left + 48))
        action.alpha_composite(cell, (frame * 48, row * 48))
action.save(FISH / "Pufferfish_Disrupt.png")

outline = (47, 51, 48, 255)
gold = (245, 197, 91, 255)
bright = (255, 237, 164, 255)
water = (173, 244, 232, 255)
impact = Image.new("RGBA", (96, 24))
for frame, radius in enumerate(((2, 4), (4, 8), (6, 9), (8, 10))):
    cell = Image.new("RGBA", (24, 24))
    draw = ImageDraw.Draw(cell)
    inner, outer = radius
    cx = cy = 11.5
    for angle in range(0, 360, 45):
        a = angle * pi / 180
        p0 = (round(cx + inner * cos(a)), round(cy + inner * sin(a)))
        p1 = (round(cx + outer * cos(a)), round(cy + outer * sin(a)))
        draw.line((p0, p1), fill=outline, width=3)
        draw.line((p0, p1), fill=bright if frame < 2 else gold, width=1)
    if frame < 3:
        draw.ellipse((9, 9, 14, 14), fill=outline)
        draw.ellipse((10, 10, 13, 13), fill=gold)
    for x, y in ((4, 7), (18, 5), (17, 18))[: max(0, frame - 1)]:
        draw.point((x, y), fill=water)
    impact.alpha_composite(cell, (frame * 24, 0))
VFX.mkdir(parents=True, exist_ok=True)
impact.save(VFX / "Pufferfish_NetImpact.png")

rate = 44100
duration = 0.22
samples = []
for i in range(round(rate * duration)):
    t = i / rate
    envelope = min(1.0, t / 0.006) * exp(-19 * t)
    phase = 2 * pi * (240 * t - 250 * t * t)
    pop = sin(phase) + 0.28 * sin(2 * phase)
    bubble = 0.13 * sin(2 * pi * (520 * t - 780 * t * t))
    sample = max(-1.0, min(1.0, (pop + bubble) * envelope * 0.48))
    samples.append(round(sample * 32767))
AUDIO.mkdir(parents=True, exist_ok=True)
with wave.open(str(AUDIO / "Pufferfish_NetDisrupt.wav"), "wb") as output:
    output.setnchannels(1)
    output.setsampwidth(2)
    output.setframerate(rate)
    output.writeframes(struct.pack(f"<{len(samples)}h", *samples))
