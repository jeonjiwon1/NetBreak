"""Static verification for the Pufferfish disruption prototype assets."""

from pathlib import Path
import re
import wave

from PIL import Image


ROOT = Path(__file__).resolve().parents[1]
ASSETS = ROOT / "Assets"
ACTION = ASSETS / "Art/Fish/Pufferfish/Pufferfish_Disrupt.png"
IMPACT = ASSETS / "Art/VFX/Pufferfish/Pufferfish_NetImpact.png"
SWIM = ASSETS / "Art/Fish/Pufferfish/Pufferfish_Swim.png"
SOUND = ASSETS / "Audio/SFX/SpecialFish/Pufferfish_NetDisrupt.wav"
PROFILE = ASSETS / "Resources/PufferfishDisruption.asset"


def sprites(path):
    meta = path.with_name(path.name + ".meta").read_text(encoding="utf-8")
    names = re.findall(r"^      name: (.+)$", meta, re.M)
    ids = [int(value) for value in re.findall(r"^      internalID: (-?\d+)$", meta, re.M)]
    return meta, names, ids


action = Image.open(ACTION).convert("RGBA")
impact = Image.open(IMPACT).convert("RGBA")
swim = Image.open(SWIM).convert("RGBA")
assert action.size == (192, 192) and impact.size == (96, 24)
assert swim.size == (192, 192)
for row in range(4):
    box = (0, row * 48, 48, (row + 1) * 48)
    assert action.crop(box).tobytes() == swim.crop(box).tobytes()
    assert action.crop((48, row * 48, 144, (row + 1) * 48)).getbbox()
assert impact.getbbox()
print("PNG dimensions, four directions, copied base poses: PASS")

action_meta, action_names, action_ids = sprites(ACTION)
impact_meta, impact_names, impact_ids = sprites(IMPACT)
assert len(action_names) == len(action_ids) == 16
assert len(impact_names) == len(impact_ids) == 4
assert "spritePixelsToUnits: 83" in action_meta and "spritePixelsToUnits: 83" in impact_meta
assert "spriteMode: 2" in action_meta and "spriteMode: 2" in impact_meta
assert "textureCompression: 0" in action_meta and "textureCompression: 0" in impact_meta
for row, direction in enumerate(("horizontal", "vertical", "diagonal", "diagonal_nw")):
    for col in range(4):
        assert action_names[row * 4 + col] == f"puffer_disrupt_{direction}_{col}"
for col in range(4):
    assert impact_names[col] == f"puffer_net_impact_{col}"
profile = PROFILE.read_text(encoding="utf-8")
action_guid = re.search(r"^guid: (\w+)$", action_meta, re.M)[1]
impact_guid = re.search(r"^guid: (\w+)$", impact_meta, re.M)[1]
for file_id in action_ids:
    assert f"{{fileID: {file_id}, guid: {action_guid}, type: 3}}" in profile
for file_id in impact_ids:
    assert f"{{fileID: {file_id}, guid: {impact_guid}, type: 3}}" in profile
assert "framesPerSecond: 12" in profile and "soundVolume: 0.34" in profile
print("Unity slice metadata and profile references: PASS")

with wave.open(str(SOUND), "rb") as sound:
    assert sound.getnchannels() == 1
    assert sound.getsampwidth() == 2
    assert sound.getframerate() == 44100
    assert abs(sound.getnframes() / sound.getframerate() - .22) < .001
print("WAV mono / 44.1 kHz / 16-bit / 0.22 s: PASS")
