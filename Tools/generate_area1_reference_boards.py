"""Build panel specific, native size pixel UI sprites for the Area 1 HUD.

No Unity scene or gameplay data is changed. Existing 39 sprites retain their GUIDs.
"""
from pathlib import Path
from PIL import Image, ImageDraw
import hashlib

ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT / "Assets/Resources/UI/Area1"
SHEET = ROOT / "Artifacts/MarineUIBoards/Sources/TropicalMarineUISheet.png"
OUT.mkdir(parents=True, exist_ok=True)

# Hand selected art roles from the reference sheet, never a positional mapping
# from the 39 existing runtime icons.
SOURCE_CROPS = {
    "hud": (1400, 40, 1648, 245),
    "button": (480, 278, 706, 452),
    "ready": (720, 282, 1037, 452),
}


def sheet_part(name):
    with Image.open(SHEET) as source:
        return source.convert("RGBA").crop(SOURCE_CROPS[name])


def source_frame(name, size, src_border, dst_border, center):
    src = sheet_part(name)
    sw, sh = src.size; w, h = size
    sl, st, sr, sb = src_border
    dl, dt, dr, db = dst_border
    sx = (0, sl, sw-sr, sw); sy = (0, st, sh-sb, sh)
    dx = (0, dl, w-dr, w); dy = (0, dt, h-db, h)
    im = canvas(size)
    d = ImageDraw.Draw(im)
    d.rectangle((dl,dt,w-dr-1,h-db-1),fill=P[center])
    if center == "wood4":
        for y in (dt+3, h-db-4):
            d.line((dl,y,w-dr-1,y),fill=P["wood3"],width=1)
    for row in range(3):
        for col in range(3):
            if row == col == 1:
                continue
            part = src.crop((sx[col],sy[row],sx[col+1],sy[row+1]))
            part = part.resize((dx[col+1]-dx[col],dy[row+1]-dy[row]),
                               Image.Resampling.NEAREST)
            im.alpha_composite(part,(dx[col],dy[row]))
    return im


def source_hud_board():
    return source_frame("hud", (256, 254), (45, 70, 45, 40),
                        (29, 42, 29, 26), "navy")


def source_ready_board():
    im = source_frame("ready", (312, 112), (75, 60, 75, 55),
                      (52, 30, 52, 25), "navy")
    # The sheet has its own short rope and corner surf. Remove that entire
    # lower rail so the one authored lower assembly can sit against the sides.
    im.paste((0, 0, 0, 0), (0, 86, 312, 112))
    d = ImageDraw.Draw(im)
    d.rectangle((49, 83, 262, 94), fill=P["navy"])
    d.line((49, 84, 262, 84), fill=P["deep"], width=2)
    for x, y, flip in ((75, 48, False), (243, 48, True)):
        direction = -1 if flip else 1
        d.ellipse((x-11,y-4,x+11,y+4),fill=P["deep"])
        d.polygon([(x-8*direction,y),(x-20*direction,y-7),
                   (x-20*direction,y+7)],fill=P["deep"])
    return im

P = {
    "void": "#061b27", "navy": "#082d3f", "deep": "#063c50",
    "teal": "#09586c", "teal2": "#0c6c81", "cyan": "#16c6d6",
    "foam": "#83ecf0", "white": "#e6fcf4", "shadow": "#241b1a",
    "wood0": "#493023", "wood1": "#805130", "wood2": "#b77a42",
    "wood3": "#d9a45f", "wood4": "#f2d18a", "rope0": "#674b30",
    "rope1": "#bd955c", "rope2": "#f1ce86", "gold": "#f5c151",
    "coral": "#d96351", "leaf": "#397b4d", "leaf2": "#77ab55",
}


def canvas(size):
    return Image.new("RGBA", size, (0, 0, 0, 0))


def rounded(d, box, radius, fill, outline=None, width=1):
    d.rounded_rectangle(box, radius=radius, fill=P.get(fill, fill),
                        outline=P.get(outline, outline) if outline else None,
                        width=width)


def marine_edge(im, w, h, strength=1):
    """Small stone and moss clusters that stay on the outer frame."""
    d = ImageDraw.Draw(im)
    stone = ("#264957", "#41646a", "#668785")
    moss = ("#315b43", "#548455", "#83a86b")
    for x, y, side in ((5, 7, 1), (w-19, 7, -1),
                       (5, h-18, 1), (w-19, h-18, -1)):
        d.rectangle((x, y+3, x+10, y+8), fill=stone[0])
        d.rectangle((x+2, y+1, x+8, y+5), fill=stone[1])
        d.rectangle((x+3, y+1, x+5, y+2), fill=stone[2])
        d.rectangle((x+7, y, x+11, y+3), fill=moss[0])
        d.rectangle((x+8, y, x+9, y+1), fill=moss[2])
        if strength > 1:
            d.rectangle((x+side*5, y+8, x+side*5+2, y+11),
                        fill=moss[1])
    for x in range(31, w-31, 43):
        for y in (4, h-8):
            d.rectangle((x, y, x+5, y+2), fill=stone[0])
            d.rectangle((x+1, y, x+3, y), fill=stone[2])
            d.rectangle((x+6, y+1, x+8, y+2), fill=moss[0])
    return im


def board(size, kind="hud"):
    w, h = size
    im = canvas(size)
    d = ImageDraw.Draw(im)
    def shape(inset, color):
        a, b, c, e = inset, inset, w-1-inset, h-1-inset
        if kind == "time":
            rounded(d, (a, b, c, e), max(3, 13-inset//2), color)
        elif kind == "ready":
            d.polygon([(a+14,b),(a+54,b),(a+60,b+4),(c-60,b+4),
                       (c-54,b),(c-14,b),(c,b+13),(c,e-16),
                       (c-13,e),(a+13,e),(a,e-16),(a,b+13)], fill=P[color])
        elif kind == "inventory":
            d.polygon([(a+14,b),(c-25,b),(c-17,b+5),(c-7,b+5),
                       (c,b+14),(c,e-13),(c-14,e),(a+14,e),
                       (a,e-13),(a,b+14)], fill=P[color])
        elif kind == "hotbar":
            d.polygon([(a+20,b),(c-20,b),(c,b+17),(c,e-16),
                       (c-17,e),(a+17,e),(a,e-16),(a,b+17)], fill=P[color])
        elif kind == "growth":
            d.polygon([(a+16,b),(c-16,b),(c,b+12),(c,e-12),
                       (c-16,e),(a+16,e),(a,e-12),(a,b+12)], fill=P[color])
        else:
            rounded(d, (a,b,c,e), max(3, 14-inset//2), color)
    for inset, color in ((0,"shadow"),(2,"wood0"),(4,"wood3"),
                         (7,"wood1"),(10,"wood2"),(13,"navy")):
        shape(inset, color)
    d.line([(20, 13), (w-21, 13)], fill=P["teal2"], width=2)
    d.line([(20, h-14), (w-21, h-14)], fill=P["deep"], width=2)
    for x in range(23, w-22, 19):
        d.line([(x, 5), (x+5, 5)], fill=P["wood4"], width=2)
        d.point((x+2, h-6), fill=P["wood2"])
    for x in (11, w-12):
        for y in (11, h-12):
            d.ellipse((x-3,y-3,x+3,y+3),fill=P["wood0"])
            d.point((x-1,y-1), fill=P["wood4"])
    # Joined plank seams and darker knots keep the wood from reading as a flat line.
    for x in range(27, w-22, 61):
        d.line((x, h-10, x+7, h-10), fill=P["wood0"], width=2)
        d.point((x+2, h-7), fill=P["wood4"])
    return marine_edge(im, w, h)


def rope(d, xy, horizontal=True, length=30, thick=8):
    x, y = xy
    if horizontal:
        d.rounded_rectangle((x, y, x+length, y+thick), radius=thick//2,
                            fill=P["rope0"])
        d.line((x+2, y+2, x+length-2, y+2), fill=P["rope2"], width=2)
        for i in range(3, length-2, 8):
            d.line((x+i, y+1, x+i+5, y+thick-1), fill=P["rope1"], width=3)
            d.line((x+i+1, y+1, x+i+5, y+thick-2), fill=P["rope2"])
    else:
        d.rounded_rectangle((x, y, x+thick, y+length), radius=thick//2,
                            fill=P["rope0"])
        d.line((x+2, y+2, x+2, y+length-2), fill=P["rope2"], width=2)
        for i in range(3, length-2, 8):
            d.line((x+1, y+i, x+thick-1, y+i+5), fill=P["rope1"], width=3)
            d.line((x+1, y+i+1, x+thick-2, y+i+5), fill=P["rope2"])


def board_hud():
    im = board((256, 254), "hud"); d = ImageDraw.Draw(im)
    # Separate rows and a noticeably recessed EXP track sit below the sign.
    for y in (63, 90, 117, 144, 171, 207, 234):
        d.line((18, y, 238, y), fill=P["teal2"], width=1)
        d.line((20, y+1, 236, y+1), fill=P["void"], width=1)
    d.line((19, 188, 237, 188), fill=P["cyan"], width=2)
    d.line((19, 189, 237, 189), fill=P["void"])
    rope(d, (18, 9), length=32)
    rope(d, (206, 9), length=32)
    return im


def board_time():
    im = board((300, 46), "time"); d = ImageDraw.Draw(im)
    rope(d, (6, 4), length=26)
    rope(d, (269, 4), length=24)
    return im


def board_ready():
    im = board((312, 112), "ready"); d = ImageDraw.Draw(im)
    # Large, quiet fish silhouettes behind the TMP heading.
    for x, y, flip in ((55, 47, False), (253, 43, True)):
        direction = -1 if flip else 1
        d.ellipse((x-13, y-5, x+13, y+5), fill=P["deep"])
        d.polygon([(x-10*direction, y), (x-23*direction, y-9),
                   (x-23*direction, y+9)], fill=P["deep"])
        d.point((x+8*direction, y-2), fill=P["teal"])
    d.line((24, 72, 288, 72), fill=P["teal2"], width=2)
    rope(d, (4, 8), length=29)
    rope(d, (279, 8), length=29)
    return im


def board_inventory():
    im = board((388, 132), "inventory"); d = ImageDraw.Draw(im)
    d.line((17, 34, 371, 34), fill=P["wood3"], width=2)
    for x in (11, 370):
        rope(d, (x, 9), horizontal=False, length=30)
    return im


def board_hotbar():
    im = board((716, 120), "hotbar"); d = ImageDraw.Draw(im)
    for x in (146, 286, 426, 566):
        rope(d, (x, 3), horizontal=False, length=111, thick=10)
    return im


def sign(size, light=False):
    w, h = size; im = canvas(size); d = ImageDraw.Draw(im)
    rounded(d, (1, 2, w-2, h-2), 8, "shadow")
    rounded(d, (3, 3, w-4, h-4), 7, "wood3")
    rounded(d, (6, 6, w-7, h-7), 5, "wood1")
    rounded(d, (9, 9, w-10, h-10), 3, "wood4" if light else "deep")
    d.line((15, 6, w-16, 6), fill=P["wood4"], width=2)
    for x in (15, w-16):
        d.ellipse((x-2, h//2-2, x+2, h//2+2), fill=P["rope2"])
    return im


def cell(size, selected=False, item=False):
    w, h = size; im = canvas(size); d = ImageDraw.Draw(im)
    rounded(d, (1, 1, w-2, h-2), 5, "shadow")
    rounded(d, (3, 3, w-4, h-4), 4, "cyan" if selected else "wood2")
    rounded(d, (6, 6, w-7, h-7), 3, "teal" if selected else "wood0")
    rounded(d, (8, 8, w-9, h-9), 2, "deep" if selected else "navy")
    d.line((13, 9, w-14, 9), fill=P["foam" if selected else "teal2"], width=2)
    d.line((13, h-10, w-14, h-10), fill=P["cyan" if selected else "teal"], width=2)
    if item:
        rounded(d, (16, 21, w-17, h-25), 2, "void", "teal2", 2)
    return marine_edge(im, w, h)


def wave():
    # One irregular lower frame and one continuous crest, without a rope bar.
    im = canvas((324, 28))
    d = ImageDraw.Draw(im)
    d.polygon([(0, 12), (9, 10), (18, 12), (28, 9), (304, 9),
               (315, 11), (323, 12), (323, 22), (314, 26),
               (10, 26), (0, 22)], fill=P["shadow"])
    d.polygon([(2, 12), (20, 12), (30, 10), (294, 10),
               (306, 12), (322, 12), (317, 22), (305, 24),
               (17, 24), (5, 22)], fill="#385863")
    d.line((14, 22, 309, 22), fill=P["wood1"], width=3)
    d.line((23, 25, 301, 25), fill=P["rope0"], width=2)
    # Broken warm twists read as rope embedded in the stone, not a straight rail.
    for x in range(29, 301, 11):
        y = 22 + (x // 11) % 2
        d.line((x, y, x+4, y+2), fill=P["rope1"], width=2)
        d.point((x+1, y), fill=P["rope2"])
    for x in (15, 64, 117, 178, 231, 292):
        d.rectangle((x, 17, x+9, 19), fill="#51746c")
        d.rectangle((x+2, 16, x+5, 17), fill="#80a888")
    marine_edge(im, 324, 28)
    # Reuse the one authored foam crest, now seated directly on this rail.
    with Image.open(OUT / "decor_wave.png") as source:
        crest = source.convert("RGBA").resize((324, 25),
                                                Image.Resampling.NEAREST)
    im.alpha_composite(crest, (0, 1))
    return im


def pixelize_decor(name):
    source = Image.open(OUT / ("decor_" + name + ".png")).convert("RGBA")
    small = source.resize((24, 24), Image.Resampling.BOX)
    pixels = small.load()
    for y in range(24):
        for x in range(24):
            r,g,b,a = pixels[x,y]
            pixels[x,y] = (r,g,b,255) if a >= 88 else (0,0,0,0)
    return small.resize((48,48), Image.Resampling.NEAREST)


def palm_vine():
    im=canvas((32,36)); d=ImageDraw.Draw(im)
    d.line((8,35,8,27,12,20,12,11,16,6),fill=P["shadow"],width=6)
    d.line((8,35,8,27,12,20,12,11,16,6),fill=P["wood2"],width=4)
    d.line((8,33,9,28,12,22,13,15),fill=P["wood4"],width=1)
    for p in (((16,6),(4,5),(1,9)),((16,6),(6,1),(3,3)),
              ((16,6),(21,1),(28,2)),((16,6),(28,5),(31,10)),
              ((16,6),(24,12),(29,17)),((16,6),(10,11),(6,16))):
        d.line(p,fill=P["leaf"],width=5)
        d.line(p,fill=P["leaf2"],width=2)
    d.ellipse((15,7,19,11),fill=P["gold"])
    return im.resize((64,72),Image.Resampling.NEAREST)


def gull_perch():
    im=canvas((28,24)); d=ImageDraw.Draw(im)
    d.line((8,19,8,22,4,22),fill=P["gold"],width=2)
    d.line((17,18,17,22,21,22),fill=P["gold"],width=2)
    d.ellipse((4,7,22,18),fill=P["shadow"])
    d.ellipse((5,6,21,17),fill=P["white"])
    d.polygon([(8,11),(1,2),(14,9),(18,16)],fill=P["deep"])
    d.line((8,11,3,3,15,11),fill=P["foam"],width=2)
    d.ellipse((17,4,25,11),fill=P["white"])
    d.polygon([(24,8),(28,9),(24,11)],fill=P["gold"])
    d.point((22,7),fill=P["shadow"])
    return im.resize((56,48),Image.Resampling.NEAREST)


def hud_crown():
    im = canvas((256, 70))
    im.alpha_composite(sign((206, 40)), (25, 12))
    d = ImageDraw.Draw(im)
    rope(d, (28, 52), length=23, thick=5)
    rope(d, (209, 52), length=20, thick=5)
    im.alpha_composite(palm_vine(), (0, 0))
    im.alpha_composite(gull_perch().resize((49, 42), Image.Resampling.NEAREST),
                       (204, 1))
    return im


def inventory_crown():
    im = canvas((388, 50))
    im.alpha_composite(sign((368, 26)), (10, 15))
    d = ImageDraw.Draw(im)
    rope(d, (11, 40), length=34, thick=5)
    im.alpha_composite(pixelize_decor("leaf"), (0, 1))
    im.alpha_composite(pixelize_decor("crate"), (339, 0))
    return im


def hud_sign():
    im = canvas((174, 44))
    im.alpha_composite(source_frame("button", (174, 38),
                                    (58, 55, 58, 55), (22, 12, 22, 12),
                                    "navy"), (0, 0))
    d = ImageDraw.Draw(im)
    # Short side posts physically overlap the HUD's top plank.
    rope(d, (14, 32), horizontal=False, length=12, thick=5)
    rope(d, (155, 32), horizontal=False, length=12, thick=5)
    d.rectangle((7, 34, 19, 37), fill=P["wood1"])
    d.rectangle((154, 34, 166, 37), fill=P["wood1"])
    return im


def speed_stack():
    im=canvas((122,138)); d=ImageDraw.Draw(im)
    rope(d,(0,9),horizontal=False,length=121,thick=6)
    rope(d,(116,9),horizontal=False,length=121,thick=6)
    for y in (0,131):
        rounded(d,(2,y,119,y+6),3,"wood0")
        d.line((11,y+2,109,y+2),fill=P["wood4"],width=2)
    return marine_edge(im, 122, 138)


def connector():
    im = canvas((16, 108)); d = ImageDraw.Draw(im)
    rope(d, (3, 0), horizontal=False, length=108, thick=10)
    for y in (5, 96):
        rounded(d, (0, y, 15, y+11), 4, "wood0")
        rounded(d, (2, y+2, 13, y+9), 3, "rope1")
        d.line((4, y+3, 11, y+3), fill=P["rope2"], width=2)
    for y in (18, 47, 76):
        d.rectangle((2, y, 5, y+3), fill="#3f7253")
        d.point((4, y), fill="#8bad70")
    return im


def exp_part(size, fill=False):
    w, h=size; im=canvas(size); d=ImageDraw.Draw(im)
    rounded(d, (0, 0, w-1, h-1), 3, "cyan" if fill else "wood3")
    if fill:
        d.line((3, 1, w-4, 1), fill=P["white"])
    else:
        rounded(d, (2, 2, w-3, h-3), 1, "void")
    return im


def save(name, im):
    target = OUT / (name + ".png")
    im.save(target, optimize=True)
    meta = target.with_suffix(".png.meta")
    if meta.exists():
        return
    guid = hashlib.md5(("netbreak-reference-board-" + name).encode()).hexdigest()
    max_size = 1 << (max(im.size)-1).bit_length()
    meta.write_text(f"""fileFormatVersion: 2
guid: {guid}
TextureImporter:
  internalIDToNameTable: []
  externalObjects: {{}}
  serializedVersion: 13
  mipmaps:
    mipMapMode: 0
    enableMipMap: 0
    sRGBTexture: 1
    linearTexture: 0
  isReadable: 0
  streamingMipmaps: 0
  vTOnly: 0
  textureFormat: 1
  maxTextureSize: {max_size}
  textureSettings:
    serializedVersion: 2
    filterMode: 0
    aniso: 1
    mipBias: 0
    wrapU: 1
    wrapV: 1
    wrapW: 1
  nPOTScale: 0
  spriteMode: 1
  spriteExtrude: 1
  spriteMeshType: 0
  alignment: 0
  spritePivot: {{x: 0.5, y: 0.5}}
  spritePixelsToUnits: 100
  spriteBorder: {{x: 0, y: 0, z: 0, w: 0}}
  alphaUsage: 1
  alphaIsTransparency: 1
  textureType: 8
  textureShape: 1
  platformSettings:
  - serializedVersion: 4
    buildTarget: DefaultTexturePlatform
    maxTextureSize: {max_size}
    textureFormat: -1
    textureCompression: 0
    compressionQuality: 100
    crunchedCompression: 0
    overridden: 0
  spriteSheet:
    serializedVersion: 2
    sprites: []
    outline: []
    physicsShape: []
    bones: []
    spriteID: 5e97eb03825dee720800000000000000
    internalID: 0
  spritePackingTag: 
  assetBundleName: 
  assetBundleVariant: 
""", encoding="utf-8")


assets = {
    "ref_hud_board": source_hud_board(),
    "ref_time_board": source_frame("button", (300, 46),
                                   (58, 55, 58, 55), (30, 14, 30, 14), "navy"),
    "ref_ready_board": source_ready_board(),
    "ref_inventory_board": board_inventory(),
    "ref_hotbar_board": board_hotbar(),
    "ref_growth_board": source_frame("button", (225, 58),
                                     (58, 55, 58, 55), (34, 17, 34, 17), "navy"),
    "ref_hud_sign": hud_sign(),
    "ref_inventory_sign": inventory_crown(),
    "ref_start_button": source_frame("button", (248, 42),
                                     (58, 55, 58, 55), (33, 13, 33, 13), "wood4"),
    "ref_item_slot": cell((84, 82), item=True),
    "ref_hotbar_slot": cell((136, 106)),
    "ref_hotbar_selected": cell((136, 106), selected=True),
    "ref_speed_button": cell((116, 40)),
    "ref_speed_selected": cell((116, 40), selected=True),
    "ref_number_badge": cell((22, 20), selected=True),
    "ref_wave_strip": wave(),
    "ref_rope_connector": connector(),
    "ref_speed_stack": speed_stack(),
    "ref_exp_track": exp_part((196, 8)),
    "ref_exp_fill": exp_part((192, 4), True),
}
for deco in ("starfish", "shell", "coral", "leaf", "rope_knot", "clock"):
    assets["ref_decor_" + deco] = pixelize_decor(deco)
for name, art in assets.items():
    save(name, art)
print(f"Generated {len(assets)} Area 1 panel sprites")
