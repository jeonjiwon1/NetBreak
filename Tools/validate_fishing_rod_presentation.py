"""Check VS-2D-1 source assets and serialized references without Unity."""

from pathlib import Path
import re
import wave

from PIL import Image


ROOT = Path(__file__).resolve().parents[1]
ASSETS = ROOT / "Assets"
PROFILE = (ASSETS / "Resources/FishingRodPresentation.asset").read_text(encoding="utf-8")


def verify_sprite(relative: str, size: tuple[int, int], cell: int,
                  count: int, ppu: int) -> None:
    path = ASSETS / relative
    with Image.open(path) as image:
        assert image.mode == "RGBA" and image.size == size, relative
        assert image.getbbox() is not None, relative
    meta = Path(str(path) + ".meta").read_text(encoding="utf-8")
    asset_guid = re.search(r"^guid: ([0-9a-f]{32})$", meta, re.M).group(1)
    assert f"spritePixelsToUnits: {ppu}" in meta, relative
    assert "spriteMode: 2" in meta and "filterMode: 0" in meta, relative
    assert "textureCompression: 0" in meta and "enableMipMap: 0" in meta, relative
    for index in range(count):
        name = f"{path.stem}_{index}"
        match = re.search(rf"^      {name}: (-?\d+)$", meta, re.M)
        assert match, name
        assert f"{{fileID: {match.group(1)}, guid: {asset_guid}, type: 3}}" in PROFILE, name
        assert f"        x: {index * cell}" in meta, name
    print(f"OK {relative}: {size}, {count} cells, PPU {ppu}")


verify_sprite("Art/Tools/FishingRod/FishingRod_Idle.png", (32, 32), 32, 1, 32)
verify_sprite("Art/Tools/FishingRod/FishingRod_Attack.png", (96, 32), 32, 3, 32)
verify_sprite("Art/VFX/Tools/FishingRod_Hit.png", (64, 16), 16, 4, 83)

wav = ASSETS / "Audio/SFX/Tools/FishingRod_Hit.wav"
with wave.open(str(wav), "rb") as audio:
    assert audio.getnchannels() == 1 and audio.getsampwidth() == 2
    assert audio.getframerate() == 44100
    assert 0.18 <= audio.getnframes() / audio.getframerate() <= 0.20
audio_meta = Path(str(wav) + ".meta").read_text(encoding="utf-8")
audio_guid = re.search(r"^guid: ([0-9a-f]{32})$", audio_meta, re.M).group(1)
assert f"{{fileID: 8300000, guid: {audio_guid}, type: 3}}" in PROFILE
script_meta = (ASSETS / "Scripts/Gear/FishingRodPresentationProfile.cs.meta").read_text(
    encoding="utf-8"
)
script_guid = re.search(r"^guid: ([0-9a-f]{32})$", script_meta, re.M).group(1)
assert f"guid: {script_guid}, type: 3" in PROFILE
assert PROFILE.count("  attackFrames:") == 1 and PROFILE.count("  hitFrames:") == 1
print("OK WAV, profile script and asset references")
