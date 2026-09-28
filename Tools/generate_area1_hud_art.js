// Deterministic, hand-drawn pixel UI assets for the Area 1 HUD.
// Run with: node Tools/generate_area1_hud_art.js
const fs = require('fs');
const path = require('path');
const crypto = require('crypto');
const zlib = require('zlib');

const output = path.join(__dirname, '..', 'Assets', 'Resources', 'UI', 'Area1');
fs.mkdirSync(output, { recursive: true });

function surface(width, height) {
  const pixels = Buffer.alloc(width * height * 4);
  function dot(x, y, color) {
    if (x < 0 || y < 0 || x >= width || y >= height) return;
    pixels.set(color, (y * width + x) * 4);
  }
  function rect(x, y, w, h, color) {
    for (let yy = y; yy < y + h; yy++)
      for (let xx = x; xx < x + w; xx++) dot(xx, yy, color);
  }
  function line(x0, y0, x1, y1, color) {
    x0 = Math.round(x0); y0 = Math.round(y0);
    x1 = Math.round(x1); y1 = Math.round(y1);
    const dx = Math.abs(x1 - x0), dy = Math.abs(y1 - y0);
    const sx = x0 < x1 ? 1 : -1, sy = y0 < y1 ? 1 : -1;
    let error = dx - dy;
    while (true) {
      dot(x0, y0, color);
      if (x0 === x1 && y0 === y1) break;
      const twice = error * 2;
      if (twice > -dy) { error -= dy; x0 += sx; }
      if (twice < dx) { error += dx; y0 += sy; }
    }
  }
  function disk(cx, cy, rx, ry, color) {
    for (let y = Math.floor(cy - ry); y <= Math.ceil(cy + ry); y++)
      for (let x = Math.floor(cx - rx); x <= Math.ceil(cx + rx); x++)
        if (((x - cx) / rx) ** 2 + ((y - cy) / ry) ** 2 <= 1)
          dot(x, y, color);
  }
  return { width, height, pixels, dot, rect, line, disk };
}

const C = {
  shadow: [2, 17, 30, 255], deep: [3, 33, 49, 248],
  sea: [5, 48, 65, 246], inset: [3, 28, 43, 248],
  wood: [94, 63, 41, 255], woodLight: [157, 112, 68, 255],
  sand: [220, 181, 109, 255], rope: [168, 137, 87, 255],
  foam: [69, 210, 224, 255], white: [238, 245, 232, 255],
  red: [210, 85, 70, 255], leaf: [49, 137, 100, 255],
  gold: [241, 187, 74, 255], clear: [0, 0, 0, 0],
  woodDark: [55, 37, 30, 255], woodBright: [204, 146, 75, 255],
  ropeLight: [231, 194, 119, 255], foamBright: [163, 249, 255, 255],
  teal: [10, 91, 112, 255], coral: [231, 84, 74, 255],
  coralLight: [255, 151, 105, 255], leafDark: [18, 81, 63, 255],
  leafLight: [92, 187, 91, 255]
};

function frame(kind) {
  const p = surface(32, 32);
  const fill = kind === 'panel' ? C.sea : C.inset;
  const edge = kind === 'selected_slot' ? C.foam :
    kind === 'button' ? C.rope : [32, 123, 145, 255];
  p.rect(1, 1, 30, 30, C.shadow);
  p.rect(2, 2, 28, 28, C.woodDark);
  p.rect(3, 3, 26, 26, kind === 'button' ? C.woodBright : edge);
  p.rect(4, 4, 24, 24, kind === 'button' ? C.sand : C.deep);
  p.rect(5, 5, 22, 22, kind === 'button' ? C.ropeLight : fill);
  p.line(5, 5, 26, 5, kind === 'selected_slot' ? C.foamBright :
    kind === 'button' ? C.white : C.teal);
  p.line(5, 26, 26, 26, kind === 'button' ? C.wood : [11, 70, 92, 255]);
  if (kind === 'panel' || kind === 'header' || kind === 'button') {
    p.rect(2, 2, 4, 2, C.woodBright);
    p.rect(26, 2, 4, 2, C.woodBright);
    p.dot(3, 3, C.ropeLight); p.dot(28, 3, C.ropeLight);
    p.dot(3, 28, C.rope); p.dot(28, 28, C.rope);
  }
  if (kind === 'header') {
    p.line(7, 3, 24, 3, C.ropeLight);
    p.line(7, 27, 24, 27, C.foam);
  }
  if (kind === 'selected_slot') {
    p.line(6, 4, 25, 4, C.foamBright);
    p.line(4, 6, 4, 25, C.foamBright);
    p.dot(5, 5, C.white); p.dot(26, 5, C.white);
    p.dot(5, 26, C.foam); p.dot(26, 26, C.foam);
  }
  return p;
}

function finishIcon(p, name) {
  // One-pixel dark outline gives the small semantic icon a clean edge on teal.
  const before = Buffer.from(p.pixels);
  for (let y = 1; y < p.height - 1; y++) for (let x = 1; x < p.width - 1; x++) {
    const here = (y * p.width + x) * 4;
    if (before[here + 3]) continue;
    const neighbors = [here - 4, here + 4, here - p.width * 4,
      here + p.width * 4];
    if (neighbors.some(i => before[i + 3])) p.dot(x, y, C.shadow);
  }
  // Small sea glints give the provided 28 targets a common material language.
  const bubbles = name === 'empty' ? [[3, 5], [27, 25]] :
    name.startsWith('stat_') ? [[27, 4]] : [[3, 7], [27, 25]];
  for (const [x, y] of bubbles) {
    if (!p.pixels[(y * p.width + x) * 4 + 3]) p.dot(x, y, C.foam);
    if (!p.pixels[((y + 1) * p.width + x) * 4 + 3])
      p.dot(x, y + 1, C.teal);
  }
  return p;
}

function icon(name) {
  const p = surface(32, 32), { rect, line, dot } = p;
  if (name === 'landing') {
    line(5, 26, 19, 13, C.wood); line(6, 26, 20, 13, C.sand);
    rect(15, 7, 12, 2, C.foam); rect(12, 9, 3, 12, C.foam);
    rect(27, 9, 2, 12, C.foam); rect(15, 21, 12, 2, C.foam);
    for (let x = 16; x < 27; x += 4) line(x, 10, x + 4, 20, C.white);
    for (let y = 11; y < 21; y += 4) line(15, y, 26, y, C.deep);
  } else if (name === 'bait') {
    rect(14, 4, 4, 5, C.rope); rect(10, 9, 12, 4, C.white);
    rect(7, 13, 18, 7, C.red); rect(9, 20, 14, 5, C.white);
    rect(12, 25, 8, 2, C.shadow); rect(15, 10, 2, 14, C.rope);
  } else if (name === 'net') {
    rect(5, 11, 22, 16, C.wood); rect(7, 9, 18, 2, C.rope);
    rect(7, 13, 18, 12, C.deep);
    for (let x = 8; x < 25; x += 4) line(x, 13, x + 4, 24, C.rope);
    for (let y = 15; y < 25; y += 4) line(8, y, 24, y, C.woodLight);
    rect(4, 25, 24, 3, C.sand);
  } else if (name === 'cast') {
    rect(13, 3, 6, 3, C.rope); rect(9, 7, 14, 3, C.foam);
    rect(6, 12, 20, 3, C.foam); rect(4, 18, 24, 3, C.foam);
    rect(3, 24, 26, 3, C.rope);
    for (let x = 9; x < 25; x += 5) line(15, 8, x, 25, C.white);
  } else if (name === 'rod') {
    line(7, 27, 22, 5, C.wood); line(8, 27, 23, 5, C.sand);
    line(23, 5, 27, 17, C.foam); line(27, 17, 25, 21, C.foam);
    rect(4, 25, 7, 3, C.rope); dot(25, 22, C.gold);
  } else if (name === 'tactical') {
    rect(12, 5, 8, 4, C.rope); rect(9, 9, 14, 2, C.sand);
    rect(7, 12, 18, 15, C.gold); rect(9, 14, 14, 11, C.sand);
    rect(14, 13, 4, 10, C.white); rect(11, 16, 10, 4, C.white);
    dot(4, 8, C.foam); dot(27, 10, C.foam);
  } else if (name === 'signature') {
    rect(5, 11, 22, 15, C.wood); rect(7, 13, 18, 11, C.woodLight);
    rect(5, 8, 22, 5, C.sand); rect(8, 6, 16, 3, C.wood);
    rect(14, 9, 4, 15, C.gold); rect(13, 16, 6, 4, C.shadow);
    rect(3, 25, 26, 3, C.rope);
  } else if (name === 'empty') {
    rect(14, 8, 4, 16, C.foam); rect(8, 14, 16, 4, C.foam);
  } else if (name === 'storm_orb') {
    rect(11, 6, 10, 3, C.foam); rect(7, 10, 18, 12, C.deep);
    rect(10, 8, 12, 15, C.foam); rect(12, 11, 8, 10, C.white);
    line(20, 3, 16, 13, C.gold); line(16, 13, 21, 12, C.gold);
    line(21, 12, 13, 28, C.gold);
  } else if (name === 'capacitor_coil') {
    rect(6, 9, 20, 15, C.wood); rect(8, 11, 16, 11, C.deep);
    for (let x = 9; x < 24; x += 4) rect(x, 10, 2, 13, C.gold);
    rect(3, 13, 4, 7, C.foam); rect(25, 13, 4, 7, C.foam);
  } else if (name === 'spectral_scabbard') {
    line(6, 25, 24, 7, C.wood); line(7, 26, 25, 8, C.rope);
    line(10, 21, 22, 9, C.foam); rect(4, 23, 8, 3, C.gold);
  } else if (name === 'autonomous_sword_array') {
    for (let x = 7; x < 26; x += 8) {
      line(x, 7, x, 21, C.white); rect(x - 2, 19, 5, 2, C.foam);
      rect(x - 1, 22, 3, 5, C.gold);
    }
  } else if (name === 'frost_sigil') {
    line(16, 3, 16, 28, C.foam); line(4, 16, 28, 16, C.foam);
    line(8, 8, 24, 24, C.white); line(24, 8, 8, 24, C.white);
    rect(13, 13, 6, 6, C.deep);
  } else if (name === 'frost_crystal') {
    rect(13, 3, 6, 4, C.white); rect(10, 7, 12, 5, C.foam);
    rect(7, 12, 18, 10, C.foam); rect(10, 22, 12, 4, C.white);
    rect(14, 26, 4, 3, C.foam); line(16, 7, 16, 25, C.white);
  } else if (name === 'stat_gold') {
    rect(8, 6, 16, 3, C.gold); rect(5, 9, 22, 15, C.gold);
    rect(8, 24, 16, 3, C.sand); rect(11, 12, 10, 9, C.sand);
    rect(14, 14, 4, 5, C.gold);
  } else if (name === 'stat_catch') {
    rect(7, 12, 16, 11, C.foam); rect(21, 9, 4, 17, C.foam);
    rect(24, 6, 4, 6, C.foam); rect(24, 23, 4, 6, C.foam);
    dot(11, 16, C.shadow); line(6, 23, 3, 26, C.white);
  } else if (name === 'stat_rate') {
    rect(13, 3, 6, 3, C.white); rect(15, 6, 2, 4, C.foam);
    rect(7, 10, 18, 17, C.foam); rect(9, 12, 14, 13, C.deep);
    line(16, 18, 21, 14, C.white); dot(16, 18, C.gold);
  } else if (name === 'stat_level') {
    line(16, 3, 20, 12, C.gold); line(20, 12, 29, 13, C.gold);
    line(29, 13, 22, 20, C.gold); line(22, 20, 24, 29, C.gold);
    line(24, 29, 16, 24, C.sand); line(16, 24, 8, 29, C.sand);
    line(8, 29, 10, 20, C.gold); line(10, 20, 3, 13, C.gold);
    line(3, 13, 12, 12, C.gold); line(12, 12, 16, 3, C.gold);
  } else if (name === 'stat_exp') {
    rect(5, 8, 22, 18, C.gold); rect(7, 10, 18, 14, C.deep);
    rect(9, 12, 3, 10, C.sand); rect(12, 12, 3, 3, C.sand);
    rect(12, 16, 3, 2, C.sand); rect(12, 20, 3, 2, C.sand);
    rect(18, 12, 3, 10, C.sand); rect(21, 12, 3, 4, C.sand);
  } else if (name === 'stat_stage') {
    rect(14, 3, 4, 17, C.rope); rect(9, 17, 14, 4, C.white);
    rect(7, 20, 4, 5, C.white); rect(21, 20, 4, 5, C.white);
    rect(10, 25, 12, 3, C.foam);
  } else if (name === 'stat_area') {
    rect(10, 4, 12, 4, C.white); rect(7, 8, 18, 14, C.foam);
    rect(10, 11, 12, 8, C.deep); rect(13, 22, 6, 6, C.foam);
    dot(16, 15, C.gold);
  } else if (name === 'speed') {
    line(4, 18, 10, 12, C.foam); line(10, 12, 16, 18, C.foam);
    line(16, 18, 22, 12, C.foam); line(22, 12, 28, 18, C.foam);
    line(4, 25, 10, 19, C.white); line(10, 19, 16, 25, C.white);
    line(16, 25, 22, 19, C.white); line(22, 19, 28, 25, C.white);
  }
  return finishIcon(p, name);
}

function ornament(name) {
  const p = surface(48, 48);
  const { dot, rect, line, disk } = p;
  const thick = (x0, y0, x1, y1, color, radius = 1) => {
    for (let dx = -radius; dx <= radius; dx++)
      for (let dy = -radius; dy <= radius; dy++)
        if (Math.abs(dx) + Math.abs(dy) <= radius + 1)
          line(x0 + dx, y0 + dy, x1 + dx, y1 + dy, color);
  };
  const shine = (x, y) => {
    line(x - 2, y, x + 2, y, C.white);
    line(x, y - 2, x, y + 2, C.white);
  };
  if (name === 'palm') {
    thick(18, 43, 22, 32, C.woodDark, 4);
    thick(22, 32, 20, 22, C.wood, 3);
    thick(20, 22, 24, 13, C.woodLight, 2);
    line(17, 39, 21, 38, C.ropeLight);
    line(20, 31, 24, 30, C.ropeLight);
    line(19, 24, 22, 23, C.ropeLight);
    const fronds = [[23, 11, 3, 7], [23, 11, 8, 0],
      [24, 10, 18, 1], [24, 10, 36, 2], [25, 10, 43, 9],
      [25, 10, 42, 20], [23, 11, 9, 20]];
    for (const [x0, y0, x1, y1] of fronds) {
      thick(x0, y0, x1, y1, C.leafDark, 3);
      line(x0, y0 - 1, x1, y1 - 2, C.leafLight);
    }
    disk(22, 14, 3, 3, C.gold);
    disk(27, 15, 3, 3, C.wood);
    dot(21, 13, C.ropeLight);
  } else if (name === 'gull') {
    thick(15, 22, 31, 24, C.shadow, 3);
    disk(27, 20, 6, 6, C.shadow);
    thick(9, 20, 19, 5, C.shadow, 3);
    thick(15, 21, 30, 26, C.white, 2);
    disk(27, 20, 5, 5, C.white);
    thick(9, 19, 19, 5, C.white, 2);
    thick(16, 25, 35, 30, C.deep, 2);
    line(16, 24, 31, 27, C.foamBright);
    rect(31, 18, 7, 2, C.gold);
    dot(28, 18, C.shadow);
    line(24, 27, 23, 37, C.gold);
    line(23, 37, 18, 39, C.gold);
  } else if (name === 'starfish') {
    const arms = [[23, 3, 27, 20], [23, 3, 19, 20],
      [43, 17, 28, 22], [43, 17, 30, 29],
      [36, 43, 26, 29], [36, 43, 22, 31],
      [11, 43, 19, 29], [11, 43, 17, 25],
      [3, 17, 17, 21], [3, 17, 20, 25]];
    for (const [x0, y0, x1, y1] of arms)
      thick(x0, y0, x1, y1, C.woodDark, 4);
    for (const [x0, y0, x1, y1] of arms)
      thick(x0, y0, x1, y1, C.coral, 2);
    disk(23, 24, 9, 8, C.coral);
    disk(21, 20, 3, 3, C.coralLight);
    for (const [x, y] of [[23, 10], [10, 20], [36, 20], [17, 34], [29, 34]])
      dot(x, y, C.coralLight);
  } else if (name === 'shell') {
    disk(24, 28, 18, 13, C.woodDark);
    disk(24, 26, 16, 12, C.ropeLight);
    disk(24, 28, 14, 9, C.sand);
    for (let x = 11; x <= 37; x += 5) {
      line(24, 36, x, Math.round(17 + Math.abs(x - 24) / 4), C.woodLight);
      dot(x, Math.round(18 + Math.abs(x - 24) / 4), C.white);
    }
    rect(13, 35, 22, 4, C.wood);
    rect(16, 35, 16, 2, C.ropeLight);
  } else if (name === 'coral') {
    const branches = [[23, 42, 24, 19], [24, 26, 12, 9],
      [13, 14, 7, 14], [24, 21, 33, 6], [31, 11, 40, 10],
      [25, 32, 38, 23], [13, 22, 7, 27]];
    for (const b of branches) thick(...b, C.woodDark, 4);
    for (const b of branches) thick(...b, C.coral, 2);
    for (const [x, y] of [[12, 9], [7, 14], [33, 6], [40, 10], [38, 23], [7, 27]])
      disk(x, y, 3, 3, C.coralLight);
    disk(23, 42, 11, 3, C.leafDark);
  } else if (name === 'leaf') {
    thick(5, 41, 36, 5, C.leafDark, 2);
    line(7, 40, 35, 8, C.ropeLight);
    for (let i = 0; i < 6; i++) {
      const x = 13 + i * 4, y = 33 - i * 4;
      thick(x, y, x - 10, y - 8, C.leafDark, 2);
      thick(x, y, x + 10, y + 4, C.leafDark, 2);
      line(x, y - 1, x - 8, y - 9, C.leafLight);
      line(x + 1, y, x + 9, y + 3, C.leafLight);
    }
  } else if (name === 'rope_knot') {
    thick(3, 38, 18, 26, C.woodDark, 4);
    thick(30, 25, 45, 38, C.woodDark, 4);
    disk(24, 22, 14, 13, C.woodDark);
    disk(24, 22, 11, 10, C.rope);
    disk(24, 22, 5, 5, C.deep);
    thick(5, 37, 18, 26, C.ropeLight, 2);
    thick(30, 25, 43, 36, C.ropeLight, 2);
    line(12, 34, 20, 27, C.white);
  } else if (name === 'bobber') {
    line(23, 1, 23, 10, C.ropeLight);
    rect(19, 9, 8, 4, C.wood);
    disk(23, 25, 15, 15, C.shadow);
    disk(23, 24, 12, 12, C.white);
    rect(11, 21, 24, 7, C.coral);
    rect(14, 16, 8, 3, C.coralLight);
    disk(23, 37, 3, 2, C.rope);
    shine(16, 15);
  } else if (name === 'wave') {
    for (const [x, y] of [[3, 28], [17, 24], [29, 27]]) {
      thick(x, y, x + 7, y - 7, C.teal, 3);
      thick(x + 7, y - 7, x + 14, y, C.teal, 3);
      line(x, y - 2, x + 7, y - 9, C.foamBright);
      line(x + 7, y - 9, x + 13, y - 3, C.white);
    }
    line(6, 34, 39, 34, C.foam);
    dot(10, 38, C.white); dot(35, 38, C.foamBright);
  } else if (name === 'crate') {
    rect(5, 10, 38, 31, C.woodDark);
    rect(8, 13, 32, 25, C.wood);
    rect(10, 16, 28, 5, C.woodBright);
    rect(10, 25, 28, 4, C.woodLight);
    rect(5, 19, 38, 4, C.ropeLight);
    rect(9, 11, 5, 28, C.woodBright);
    rect(34, 11, 5, 28, C.woodBright);
    for (const x of [11, 37]) for (const y of [18, 32]) dot(x, y, C.gold);
    rect(16, 7, 17, 5, C.sand);
  } else if (name === 'clock') {
    disk(24, 24, 20, 20, C.shadow);
    disk(24, 24, 17, 17, C.foam);
    disk(24, 24, 13, 13, C.deep);
    line(24, 14, 24, 25, C.white);
    line(24, 25, 33, 28, C.white);
    disk(24, 25, 2, 2, C.gold);
    for (const [x, y] of [[24, 10], [38, 24], [24, 38], [10, 24]])
      disk(x, y, 1, 1, C.ropeLight);
  } else throw new Error('Unknown ornament: ' + name);
  const materialHighlights = [
    [C.wood, C.woodLight], [C.woodLight, C.woodBright],
    [C.rope, C.ropeLight], [C.sand, C.white],
    [C.leafDark, C.leaf], [C.leaf, C.leafLight],
    [C.coral, C.coralLight], [C.foam, C.foamBright]
  ];
  const source = Buffer.from(p.pixels);
  for (let y = 2; y < 46; y++) for (let x = 2; x < 46; x++) {
    const at = (y * p.width + x) * 4;
    if (!source[at + 3] || (x * 17 + y * 23) % 11 > 1) continue;
    for (const [base, highlight] of materialHighlights) {
      if (source[at] === base[0] && source[at + 1] === base[1] &&
          source[at + 2] === base[2]) {
        dot(x, y, highlight);
        break;
      }
    }
  }
  return p;
}

function chunk(type, data) {
  const typeBytes = Buffer.from(type);
  const body = Buffer.concat([typeBytes, data]);
  let crc = 0xffffffff;
  for (const byte of body) {
    crc ^= byte;
    for (let i = 0; i < 8; i++) crc = (crc >>> 1) ^ ((crc & 1) ? 0xedb88320 : 0);
  }
  const header = Buffer.alloc(4); header.writeUInt32BE(data.length);
  const tail = Buffer.alloc(4); tail.writeUInt32BE((crc ^ 0xffffffff) >>> 0);
  return Buffer.concat([header, body, tail]);
}

function save(name, p, border) {
  const rows = Buffer.alloc(p.height * (p.width * 4 + 1));
  for (let y = 0; y < p.height; y++)
    p.pixels.copy(rows, y * (p.width * 4 + 1) + 1, y * p.width * 4, (y + 1) * p.width * 4);
  const ihdr = Buffer.alloc(13);
  ihdr.writeUInt32BE(p.width, 0); ihdr.writeUInt32BE(p.height, 4);
  ihdr[8] = 8; ihdr[9] = 6;
  const png = Buffer.concat([
    Buffer.from('89504e470d0a1a0a', 'hex'),
    chunk('IHDR', ihdr), chunk('IDAT', zlib.deflateSync(rows)), chunk('IEND', Buffer.alloc(0))
  ]);
  const filename = path.join(output, name + '.png');
  fs.writeFileSync(filename, png);
  if (fs.existsSync(filename + '.meta')) return; // Existing GUIDs and importer settings are authoritative.
  const guid = crypto.createHash('md5').update('netbreak-area1-hud-' + name).digest('hex');
  fs.writeFileSync(filename + '.meta', `fileFormatVersion: 2\nguid: ${guid}\nTextureImporter:\n  internalIDToNameTable: []\n  externalObjects: {}\n  serializedVersion: 13\n  mipmaps:\n    mipMapMode: 0\n    enableMipMap: 0\n    sRGBTexture: 1\n    linearTexture: 0\n    fadeOut: 0\n    borderMipMap: 0\n    mipMapsPreserveCoverage: 0\n    alphaTestReferenceValue: 0.5\n    mipMapFadeDistanceStart: 1\n    mipMapFadeDistanceEnd: 3\n  bumpmap:\n    convertToNormalMap: 0\n    externalNormalMap: 0\n    heightScale: 0.25\n    normalMapFilter: 0\n  isReadable: 0\n  streamingMipmaps: 0\n  vTOnly: 0\n  ignoreMipmapLimit: 0\n  grayScaleToAlpha: 0\n  generateCubemap: 6\n  textureFormat: 1\n  maxTextureSize: 32\n  textureSettings:\n    serializedVersion: 2\n    filterMode: 0\n    aniso: 1\n    mipBias: 0\n    wrapU: 1\n    wrapV: 1\n    wrapW: 1\n  nPOTScale: 0\n  lightmap: 0\n  compressionQuality: 100\n  spriteMode: 1\n  spriteExtrude: 1\n  spriteMeshType: 0\n  alignment: 0\n  spritePivot: {x: 0.5, y: 0.5}\n  spritePixelsToUnits: 32\n  spriteBorder: {x: ${border}, y: ${border}, z: ${border}, w: ${border}}\n  spriteGenerateFallbackPhysicsShape: 0\n  alphaUsage: 1\n  alphaIsTransparency: 1\n  spriteTessellationDetail: -1\n  textureType: 8\n  textureShape: 1\n  maxTextureSizeSet: 0\n  compressionQualitySet: 0\n  textureFormatSet: 0\n  platformSettings:\n  - serializedVersion: 4\n    buildTarget: DefaultTexturePlatform\n    maxTextureSize: 32\n    resizeAlgorithm: 0\n    textureFormat: -1\n    textureCompression: 0\n    compressionQuality: 100\n    crunchedCompression: 0\n    allowsAlphaSplitting: 0\n    overridden: 0\n    ignorePlatformSupport: 0\n    androidETC2FallbackOverride: 0\n    forceMaximumCompressionQuality_BC6H_BC7: 0\n  spriteSheet:\n    serializedVersion: 2\n    sprites: []\n    outline: []\n    physicsShape: []\n    bones: []\n    spriteID: 5e97eb03825dee720800000000000000\n    internalID: 0\n    vertices: []\n    indices: \n    edges: []\n    weights: []\n  spritePackingTag: \n  pSDRemoveMatte: 0\n  pSDShowRemoveMatteOption: 0\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n`);
  const metaFile = filename + '.meta';
  fs.writeFileSync(metaFile,
    fs.readFileSync(metaFile, 'utf8')
      .replace(/maxTextureSize: 32/g, `maxTextureSize: ${Math.max(32, p.width)}`)
      .replace(/[ \t]+$/gm, ''));
}

for (const kind of ['panel', 'slot', 'selected_slot', 'header', 'button', 'key'])
  save(kind, frame(kind), 5);
for (const name of ['landing', 'bait', 'net', 'cast', 'rod', 'tactical', 'signature',
  'empty', 'storm_orb', 'capacitor_coil', 'spectral_scabbard',
  'autonomous_sword_array', 'frost_sigil', 'frost_crystal',
  'stat_gold', 'stat_catch', 'stat_rate', 'stat_level', 'stat_exp',
  'stat_stage', 'stat_area', 'speed'])
  save('icon_' + name, icon(name), 0);
for (const name of ['palm', 'gull', 'starfish', 'shell', 'coral', 'leaf',
  'rope_knot', 'bobber', 'wave', 'crate', 'clock'])
  save('decor_' + name, ornament(name), 0);
