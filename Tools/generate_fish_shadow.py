"""Build the shared, pixel edged underwater shadow sprite."""

from pathlib import Path
import struct
import zlib


WIDTH, HEIGHT = 32, 16
OUTPUT = Path(__file__).resolve().parents[1] / "Assets/Resources/Fish/FishShadow.png"


def chunk(kind, data):
    return struct.pack(">I", len(data)) + kind + data + struct.pack(">I", zlib.crc32(kind + data) & 0xFFFFFFFF)


def pixel(x, y):
    dx = (x + 0.5 - WIDTH / 2) / 14.5
    dy = (y + 0.5 - HEIGHT / 2) / 6.5
    radius = dx * dx + dy * dy
    if radius > 1:
        return bytes((0, 0, 0, 0))
    alpha = 72 if radius > 0.72 else 112 if radius > 0.38 else 144
    return bytes((17, 65, 79, alpha))


rows = b"".join(b"\0" + b"".join(pixel(x, y) for x in range(WIDTH)) for y in range(HEIGHT))
png = (b"\x89PNG\r\n\x1a\n"
       + chunk(b"IHDR", struct.pack(">IIBBBBB", WIDTH, HEIGHT, 8, 6, 0, 0, 0))
       + chunk(b"IDAT", zlib.compress(rows))
       + chunk(b"IEND", b""))
OUTPUT.parent.mkdir(parents=True, exist_ok=True)
OUTPUT.write_bytes(png)
