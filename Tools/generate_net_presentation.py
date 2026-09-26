"""Generate deterministic, project-owned pixel net tiles, contact frames and place WAV."""
from pathlib import Path
import math
import random
import struct
import wave

from PIL import Image, ImageDraw


ROOT = Path(__file__).resolve().parents[1]
ART = ROOT / "Assets/Art/Tools/Net"
VFX = ROOT / "Assets/Art/VFX/Tools"
AUDIO = ROOT / "Assets/Audio/SFX/Tools"
for folder in (ART, VFX, AUDIO):
    folder.mkdir(parents=True, exist_ok=True)

mesh = Image.new("RGBA", (16, 16))
d = ImageDraw.Draw(mesh)
thread = (170, 226, 207, 115)
knot = (237, 238, 191, 185)
for x in range(-16, 32, 8):
    d.line((x, 0, x + 16, 15), fill=thread, width=1)
    d.line((x + 16, 0, x, 15), fill=thread, width=1)
for x in (0, 8):
    for y in (0, 8):
        d.point((x, y), fill=knot)
mesh.save(ART / "Net_Mesh.png")

rope = Image.new("RGBA", (16, 4))
d = ImageDraw.Draw(rope)
d.rectangle((0, 1, 15, 2), fill=(227, 203, 142, 230))
for x in (2, 10):
    d.line((x, 0, x + 2, 3), fill=(136, 112, 77, 230), width=1)
rope.save(ART / "Net_Rope.png")

sheet = Image.new("RGBA", (96, 24))
for frame in range(4):
    d = ImageDraw.Draw(sheet)
    ox = frame * 24
    radius = 3 + frame * 2
    color = (186, 239, 217, max(50, 180 - frame * 38))
    for dx, dy in ((-radius, 0), (radius, 0), (0, -radius), (0, radius)):
        d.rectangle((ox + 12 + dx, 12 + dy, ox + 13 + dx, 13 + dy), fill=color)
    if frame < 3:
        d.rectangle((ox + 11, 11, ox + 13, 13), fill=(241, 240, 189, 175 - frame * 35))
sheet.save(VFX / "Net_Contact.png")

rng = random.Random(7244)
rate = 44100
duration = 0.29
samples = []
for i in range(round(rate * duration)):
    t = i / rate
    envelope = (1 - t / duration) ** 2.5
    rope_tone = math.sin(2 * math.pi * (305 * t + 160 * t * t)) * math.exp(-16 * t)
    splash = (rng.random() * 2 - 1) * math.exp(-22 * max(0, t - .045))
    value = .25 * envelope * rope_tone + .15 * envelope * splash
    samples.append(struct.pack("<h", max(-32768, min(32767, round(value * 32767)))))
with wave.open(str(AUDIO / "Net_Place.wav"), "wb") as output:
    output.setnchannels(1)
    output.setsampwidth(2)
    output.setframerate(rate)
    output.writeframes(b"".join(samples))
