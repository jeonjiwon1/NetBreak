// Redraw only the shared 32x32 empty item icon. Keep its Unity .meta and GUID.
const fs = require('fs');
const path = require('path');
const zlib = require('zlib');

const target = path.join(__dirname, '..', 'Assets', 'Resources', 'UI',
  'Area1', 'icon_empty.png');
if (!fs.existsSync(target + '.meta'))
  throw new Error('The existing icon_empty.png.meta is required.');

const width = 32;
const height = 32;
const pixels = Buffer.alloc(width * height * 4);
const ink = [58, 73, 72, 255];
const rope = [193, 151, 91, 255];
const light = [244, 211, 143, 255];
const sea = [93, 177, 172, 255];

for (let y = 3; y <= 28; y++)
  for (let x = 3; x <= 28; x++) {
    // Two-pixel bars retain the previous 26-pixel reach in both directions.
    const vertical = x >= 15 && x <= 16;
    const horizontal = y >= 15 && y <= 16;
    if (!vertical && !horizontal) continue;

    let color = (x === 3 || x === 28 || y === 3 || y === 28)
      ? ink : (x === 15 || y === 15) ? light : rope;
    if ((vertical && (y === 8 || y === 23) && x === 16) ||
        (horizontal && (x === 8 || x === 23) && y === 16))
      color = sea;
    const at = (y * width + x) * 4;
    for (let i = 0; i < 4; i++) pixels[at + i] = color[i];
  }

function chunk(type, data) {
  const body = Buffer.concat([Buffer.from(type), data]);
  let crc = 0xffffffff;
  for (const byte of body) {
    crc ^= byte;
    for (let i = 0; i < 8; i++)
      crc = (crc >>> 1) ^ ((crc & 1) ? 0xedb88320 : 0);
  }
  const length = Buffer.alloc(4);
  length.writeUInt32BE(data.length);
  const checksum = Buffer.alloc(4);
  checksum.writeUInt32BE((crc ^ 0xffffffff) >>> 0);
  return Buffer.concat([length, body, checksum]);
}

const rows = Buffer.alloc(height * (width * 4 + 1));
for (let y = 0; y < height; y++)
  pixels.copy(rows, y * (width * 4 + 1) + 1, y * width * 4,
    (y + 1) * width * 4);
const ihdr = Buffer.alloc(13);
ihdr.writeUInt32BE(width, 0);
ihdr.writeUInt32BE(height, 4);
ihdr[8] = 8; // RGBA8
ihdr[9] = 6;
fs.writeFileSync(target, Buffer.concat([
  Buffer.from('89504e470d0a1a0a', 'hex'),
  chunk('IHDR', ihdr), chunk('IDAT', zlib.deflateSync(rows)),
  chunk('IEND', Buffer.alloc(0))
]));
