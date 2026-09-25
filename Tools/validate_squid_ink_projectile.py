"""Static checks for VS-2C-2 Sprite sheets and serialized references."""

from pathlib import Path
import re

from PIL import Image


ROOT = Path(__file__).resolve().parents[1]
ART = ROOT / "Assets/Art/VFX/Squid"
profile = (ROOT / "Assets/Resources/SquidInkPresentation.asset").read_text(encoding="utf-8")

for name, cell, field in (
    ("Projectile", 16, "projectileFrames"),
    ("Impact", 24, "impactFrames"),
):
    stem = f"Squid_Ink{name}.png"
    with Image.open(ART / stem) as image:
        assert image.mode == "RGBA"
        assert image.size == (cell * 4, cell)
        assert set(image.getchannel("A").tobytes()) == {0, 255}
    meta = (ART / (stem + ".meta")).read_text(encoding="utf-8")
    guid = re.search(r"^guid: ([0-9a-f]{32})$", meta, re.M).group(1)
    assert "spritePixelsToUnits: 83" in meta
    assert "filterMode: 0" in meta and "spriteMode: 2" in meta
    assert len(re.findall(r"^      name: squid_ink_", meta, re.M)) == 4
    assert len(re.findall(rf"^        width: {cell}$", meta, re.M)) == 4
    assert len(re.findall(rf"^        height: {cell}$", meta, re.M)) == 4
    section = re.search(rf"^  {field}:\n((?:  - .*\n){{4}})", profile, re.M).group(1)
    assert len(re.findall(rf"guid: {guid}", section)) == 4
    file_ids = re.findall(r"^      internalID: (-?\d+)$", meta, re.M)[:4]
    assert len(file_ids) == 4
    assert re.findall(r"fileID: (-?\d+)", section) == file_ids
    print(f"{stem}: {cell * 4}x{cell}, RGBA, 4 slices and Profile links OK")

print("VS-2C-2 Sprite static validation passed")
