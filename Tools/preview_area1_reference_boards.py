"""Offline layout approximation of the Area 1 HUD; this is not a Unity capture."""
from pathlib import Path
from PIL import Image, ImageDraw, ImageFont

ROOT = Path(__file__).resolve().parents[1]
ART = ROOT / "Assets/Resources/UI/Area1"
OUT = ROOT / "Artifacts/MarineUIBoards"
OUT.mkdir(parents=True, exist_ok=True)
FONT = ROOT / "Assets/UI/Fonts/NanumGothic-Bold.ttf"

im = Image.open(ROOT / "Assets/Resources/Area1/CoastBackground.png").convert("RGBA")
im = im.resize((1920, 1080), Image.Resampling.NEAREST)


def sprite(name, x, y, w=None, h=None):
    if name.startswith("decor_") and (ART / ("ref_" + name + ".png")).exists():
        name = "ref_" + name
    part = Image.open(ART / (name + ".png")).convert("RGBA")
    w = w or part.width; h = h or part.height
    part = part.resize((int(w), int(h)), Image.Resampling.NEAREST)
    im.alpha_composite(part, (int(x), int(y)))


def label(text, box, size=16, color="#f7efcf", align="center"):
    d = ImageDraw.Draw(im)
    font = ImageFont.truetype(str(FONT), size)
    x, y, w, h = box
    if "\n" in text:
        lines = text.split("\n")
        line_h = size + 2
        for i, line in enumerate(lines):
            label(line, (x, y+(h-len(lines)*line_h)//2+i*line_h, w, line_h),
                  size, color, align)
        return
    bounds = d.textbbox((0, 0), text, font=font)
    tw, th = bounds[2]-bounds[0], bounds[3]-bounds[1]
    tx = x if align == "left" else x+w-tw if align == "right" else x+(w-tw)//2
    d.text((tx, y+(h-th)//2-bounds[1]), text, font=font, fill=color)


sprite("ref_hud_board", 20, 20)
sprite("ref_hud_sign", 60, 12)
label("NETBREAK", (73, 17, 148, 27), 19, "#ffd777")
stats = [("골드", "0", "gold"), ("포획 수", "0", "catch"),
         ("어획률", "0.0%", "rate"), ("레벨", "1", "level"),
         ("경험치", "0 / 30", "exp"), ("조업 단계", "준비", "stage"),
         ("현재 구간", "조업 준비", "area")]
for i, (name, value, icon) in enumerate(stats):
    y = 59 + (i*23 if i < 5 else 135+(i-5)*24)
    sprite("icon_stat_"+icon, 52, y+2, 20, 20)
    label(name, (75, y, 77, 25), 15, align="left")
    label(value, (154, y, 87, 25), 16, "#ffd777" if i == 0 else "#84efe5" if i > 4 else "#f7efcf", "right")
sprite("ref_exp_track", 76, 175, 164, 8)

sprite("ref_time_board", 810, 16)
sprite("decor_clock", 822, 24, 29, 29)
label("플레이 시간: 00:00", (842, 19, 260, 38), 21)
sprite("ref_ready_board", 804, 70)
sprite("ref_wave_strip", 798, 154)
label("조업 준비", (835, 98, 250, 29), 24)
sprite("ref_start_button", 836, 129)
label("조업 시작", (848, 134, 224, 32), 21, "#39251b")

sprite("ref_inventory_board", 1512, 20)
sprite("ref_inventory_sign", 1512, 12)
label("≡  아이템", (1556, 28, 274, 25), 15, align="left")
for name, x, y, s in (("shell",1504,129,30),("starfish",1885,134,29)):
    sprite("decor_"+name,x,y,s,s)
for i in range(4):
    x=1527+i*91
    sprite("ref_item_slot",x,60)
    sprite("icon_empty",x+26,85,32,32)
    sprite("ref_number_badge",x+8,64)
    label(str(i+1),(x+8,64,22,20),16)
    label("비어 있음",(x+6,117,72,18),15)

sprite("ref_hotbar_board",602,946)
names=("LMB","Q","W","E","R")
icons=("landing","empty","empty","tactical","signature")
texts=("뜰채\n고정 도구","비어 있음","비어 있음","잠김\n미니보스 보상","잠김\n보스 보상")
for i in range(5):
    x=612+i*140
    sprite("ref_hotbar_selected" if i == 0 else "ref_hotbar_slot",x,953)
    sprite("icon_"+icons[i],x+45,965,45,45)
    sprite("key",x+6,958,47 if i==0 else 29,23)
    label(names[i],(x+6,958,47 if i==0 else 29,23),15)
    label(texts[i],(x+6,1003,124,52),16)
for i in range(4):
    sprite("ref_rope_connector",745+i*140,952)
sprite("decor_leaf",581,982,53,53)
sprite("decor_coral",1288,1013,49,49)

sprite("ref_speed_stack", 1778, 812)
for i in range(3):
    y=816+i*45
    sprite("ref_speed_selected" if i == 0 else "ref_speed_button",1781,y)
    sprite("icon_speed",1792,y+8,25,25)
    label("x"+str(i+1),(1824,y+5,65,30),20)
sprite("ref_growth_board",1675,1002)
label("성장 관리 [Tab]",(1685,1010,205,42),20)
sprite("decor_leaf",1657,982,36,36)
sprite("decor_shell",1878,1020,32,32)

im.convert("RGB").save(OUT / "HUD_STATIC_PREVIEW.png")
print(OUT / "HUD_STATIC_PREVIEW.png")
