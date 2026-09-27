"""Build deterministic project-owned cast net pixel art and a short WAV."""
from pathlib import Path
import math
import random
import struct
import wave

from PIL import Image, ImageDraw

ROOT = Path(__file__).resolve().parents[1]
ART = ROOT / "Assets/Art/Tools/CastNet"
VFX = ROOT / "Assets/Art/VFX/Tools"
AUDIO = ROOT / "Assets/Audio/SFX/Tools"
for folder in (ART, VFX, AUDIO):
    folder.mkdir(parents=True, exist_ok=True)

shadow = (31, 67, 76, 205)
rope = (190, 166, 112, 225)
mesh = (156, 218, 199, 165)
foam = (226, 248, 223, 220)

folded = Image.new("RGBA", (16, 16))
d = ImageDraw.Draw(folded)
d.line((4, 3, 12, 11), fill=shadow, width=3)
d.line((3, 6, 10, 13), fill=rope, width=2)
d.ellipse((4, 3, 11, 10), outline=foam, width=1)
d.line((5, 5, 11, 10), fill=mesh, width=1)
d.point((3, 13), fill=rope)
folded.save(ART / "CastNet_Folded.png")


def net_frame(index):
    image = Image.new("RGBA", (64, 64))
    draw = ImageDraw.Draw(image)
    cx = cy = 32
    radius = (5, 12, 21, 27, 31)[index]
    alpha = 100 if index == 4 else 225
    edge = (190, 166, 112, alpha)
    inner = (156, 218, 199, min(alpha, 150))
    if index == 0:
        image.paste(folded.resize((16, 16), Image.Resampling.NEAREST), (24, 24), folded)
        return image
    draw.ellipse((cx-radius, cy-radius, cx+radius, cy+radius), outline=edge, width=2)
    for offset in range(-radius+6, radius, 8):
        half = math.isqrt(max(0, radius*radius-offset*offset))
        draw.line((cx+offset, cy-half, cx+offset, cy+half), fill=inner, width=1)
        draw.line((cx-half, cy+offset, cx+half, cy+offset), fill=inner, width=1)
    for angle in range(0, 360, 45):
        rad = math.radians(angle)
        x = round(cx + radius * math.cos(rad))
        y = round(cy + radius * math.sin(rad))
        draw.rectangle((x-1, y-1, x+1, y+1), fill=shadow if index < 4 else edge)
    draw.ellipse((cx-2, cy-2, cx+2, cy+2), fill=rope)
    return image


opening = Image.new("RGBA", (64*5, 64))
for frame in range(5):
    opening.paste(net_frame(frame), (64*frame, 0))
opening.save(ART / "CastNet_Open.png")

area = Image.new("RGBA", (64*4, 64))
for frame in range(4):
    tile = Image.new("RGBA", (64, 64))
    draw = ImageDraw.Draw(tile)
    radius = 25 + frame*2
    a = 170 - frame*40
    draw.ellipse((32-radius, 32-radius, 32+radius, 32+radius),
                 outline=(161, 232, 231, a), width=2)
    for angle in (30, 150, 270):
        rad = math.radians(angle)
        x = round(32 + radius * math.cos(rad))
        y = round(32 + radius * math.sin(rad))
        draw.line((x-2, y-1, x+1, y), fill=(230, 249, 231, a), width=1)
    area.paste(tile, (64*frame, 0))
area.save(VFX / "CastNet_Area.png")

hit = Image.new("RGBA", (24*4, 24))
for frame in range(4):
    tile = Image.new("RGBA", (24, 24))
    draw = ImageDraw.Draw(tile)
    a = 220 - frame*48
    radius = 3 + frame*2
    draw.arc((12-radius, 12-radius//2, 12+radius, 12+radius//2),
             10, 170, fill=(224, 249, 226, a), width=2)
    for x, y in ((5-frame, 8), (18+frame//2, 7), (12, 4-frame//2)):
        draw.rectangle((x, y, x+1, y+1), fill=(136, 218, 211, a))
    hit.paste(tile, (24*frame, 0))
hit.save(VFX / "CastNet_Hit.png")

rng = random.Random(2724)
rate = 44100
duration = .34
samples = []
for i in range(round(rate*duration)):
    t = i/rate
    rope_rustle = (rng.random()*2-1) * math.exp(-23*t)
    splash_t = max(0, t-.095)
    splash = (rng.random()*2-1)*math.exp(-22*splash_t) if t >= .095 else 0
    low = math.sin(2*math.pi*(210*t - 45*t*t))*math.exp(-16*t)
    value = .12*rope_rustle + .1*splash + .07*low
    samples.append(struct.pack("<h", max(-32768, min(32767, round(value*32767)))))
with wave.open(str(AUDIO / "CastNet_Open.wav"), "wb") as output:
    output.setnchannels(1)
    output.setsampwidth(2)
    output.setframerate(rate)
    output.writeframes(b"".join(samples))
