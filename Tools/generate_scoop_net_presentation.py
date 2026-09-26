"""Generate deterministic, project-owned scoop net pixel art and one-shot WAV."""
from pathlib import Path
import math
import random
import struct
import wave

from PIL import Image, ImageDraw


ROOT = Path(__file__).resolve().parents[1]
ART = ROOT / "Assets/Art/Tools/ScoopNet"
VFX = ROOT / "Assets/Art/VFX/Tools"
AUDIO = ROOT / "Assets/Audio/SFX/Tools"
for folder in (ART, VFX, AUDIO):
    folder.mkdir(parents=True, exist_ok=True)


def scoop_frame(phase):
    image = Image.new("RGBA", (48, 48))
    draw = ImageDraw.Draw(image)
    # Each frame moves the hoop across the cursor without rotating the pixels.
    offsets = [(-5, 4), (-2, 3), (2, 1), (5, -1), (2, 0)]
    ox, oy = offsets[phase]
    cx, cy = 28 + ox, 17 + oy
    dark = (45, 70, 77, 255)
    wood = (132, 93, 57, 255)
    light = (224, 190, 120, 255)
    net = (151, 220, 207, 215)
    foam = (241, 250, 221, 255)
    draw.line((8, 42, cx - 6, cy + 7), fill=dark, width=6)
    draw.line((8, 42, cx - 6, cy + 7), fill=wood, width=4)
    draw.line((9, 41, cx - 6, cy + 7), fill=light, width=1)
    draw.rectangle((5, 40, 11, 45), fill=dark)
    draw.rectangle((6, 41, 10, 44), fill=light)
    draw.ellipse((cx - 11, cy - 8, cx + 11, cy + 8), outline=dark, width=3)
    draw.ellipse((cx - 9, cy - 6, cx + 9, cy + 6), outline=foam, width=2)
    for dx in (-5, 0, 5):
        draw.line((cx + dx, cy - 4, cx + dx, cy + 4), fill=net, width=1)
    for dy in (-3, 1, 5):
        draw.line((cx - 7, cy + dy, cx + 7, cy + dy), fill=net, width=1)
    draw.point((cx + 8, cy - 4), fill=foam)
    return image


ready = scoop_frame(2)
ready.save(ART / "ScoopNet_Ready.png")
sheet = Image.new("RGBA", (48 * 5, 48))
for index in range(5):
    sheet.paste(scoop_frame(index), (index * 48, 0))
sheet.save(ART / "ScoopNet_Swing.png")

hit = Image.new("RGBA", (24 * 4, 24))
for frame in range(4):
    draw = ImageDraw.Draw(hit)
    x = frame * 24
    alpha = 225 - frame * 48
    teal = (119, 226, 218, alpha)
    white = (243, 251, 226, alpha)
    radius = 3 + frame * 2
    draw.arc((x + 12 - radius, 12 - radius // 2,
              x + 12 + radius, 12 + radius // 2), 15, 165, fill=white, width=2)
    for dx, dy in ((-radius, -3), (radius, -4), (0, -radius - 2)):
        draw.rectangle((x + 12 + dx, 12 + dy,
                        x + 13 + dx, 13 + dy), fill=teal)
    if frame < 2:
        draw.rectangle((x + 11, 11, x + 13, 12), fill=white)
hit.save(VFX / "ScoopNet_Hit.png")

rng = random.Random(7283)
rate = 44100
duration = .24
samples = []
for i in range(round(rate * duration)):
    t = i / rate
    envelope = max(0.0, 1 - t / duration) ** 2.4
    sweep = (rng.random() * 2 - 1) * math.exp(-19 * t)
    water = math.sin(2 * math.pi * (460 * t - 230 * t * t)) * math.exp(-25 * t)
    droplet = math.sin(2 * math.pi * 760 * t) * math.exp(-48 * max(0, t - .09)) if t >= .09 else 0
    value = envelope * (.12 * sweep + .13 * water + .04 * droplet)
    samples.append(struct.pack("<h", max(-32768, min(32767, round(value * 32767)))))
with wave.open(str(AUDIO / "ScoopNet_Swing.wav"), "wb") as output:
    output.setnchannels(1)
    output.setsampwidth(2)
    output.setframerate(rate)
    output.writeframes(b"".join(samples))
