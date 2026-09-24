"""기존 픽셀 보존과 새 방향 프레임·남쪽 배색 위치를 검사한다."""

from hashlib import sha256
from pathlib import Path
import re

from PIL import Image


ROOT = Path(__file__).resolve().parents[1] / "Assets" / "Art" / "Fish"
SPECIES = {
    "Sardine": (32, (190, 221, 207, 255), "7d55d724919e9cfc223eb44d733be7ebbb0f631bf69bf0d9fadc5c1bf7bbdbbf"),
    "Mackerel": (48, (176, 218, 207, 255), "119c0094be318ab65afdc6932c1e748d1dad6415d2ab3cd5ec4a1765da94ceac"),
    "Tuna": (64, (181, 218, 208, 255), "800355c40095b7d37314cde67e412461a26ff146f7b600791cc4e207a1fabd3a"),
    "Pufferfish": (48, (248, 238, 190, 255), "f550ffcb595292d4a1a8b429959bf0b7e5680c7a0c5121013d071576f98276c4"),
    "Squid": (64, (247, 195, 192, 255), "e19a97fa0cede72ed4883788b93c429f45173672ca1f7a8f14f0f7747c52eee5"),
}
DIRECTIONS = ("horizontal", "vertical", "diagonal", "diagonal_nw")


def check_species(name, cell, belly, original_hash):
    folder = ROOT / name
    image = Image.open(folder / f"{name}_Swim.png").convert("RGBA")
    assert image.size == (cell * 4, cell * 4)
    old_rows = image.crop((0, 0, cell * 4, cell * 3))
    assert sha256(old_rows.tobytes()).hexdigest() == original_hash
    assert set(image.getchannel("A").tobytes()) <= {0, 255}

    for col in range(4):
        ne = image.crop((col * cell, cell * 2, (col + 1) * cell, cell * 3))
        nw = image.crop((col * cell, cell * 3, (col + 1) * cell, cell * 4))
        assert nw.getbbox() and nw.tobytes() != ne.tobytes()
    original_colors = {old_rows.getpixel((x, y)) for y in range(cell * 3)
                       for x in range(cell * 4)}
    new_colors = {image.getpixel((x, y)) for y in range(cell * 3, cell * 4)
                  for x in range(cell * 4)}
    assert new_colors <= original_colors

    north = image.crop((0, cell, cell, cell * 2))
    belly_x = [x for y in range(cell) for x in range(cell) if north.getpixel((x, y)) == belly]
    assert belly_x and sum(belly_x) / len(belly_x) > (cell - 1) / 2
    assert cell - 1 - sum(belly_x) / len(belly_x) < (cell - 1) / 2
    diagonal_belly_y = []
    for row in (2, 3):
        frame = image.crop((0, cell * row, cell, cell * (row + 1)))
        points = [y for y in range(cell) for x in range(cell)
                  if frame.getpixel((x, y)) == belly]
        assert points
        diagonal_belly_y.append(sum(points) / len(points))
    assert diagonal_belly_y[1] < diagonal_belly_y[0]

    meta = (folder / f"{name}_Swim.png.meta").read_text(encoding="utf-8")
    profile = (folder / f"{name}_VisualProfile.asset").read_text(encoding="utf-8")
    guid = re.search(r"^guid: ([0-9a-f]+)$", meta, re.M).group(1)
    profile_meta = (folder / f"{name}_VisualProfile.asset.meta").read_text(encoding="utf-8")
    profile_guid = re.search(r"^guid: ([0-9a-f]+)$", profile_meta, re.M).group(1)
    fish_data = (ROOT.parents[1] / "Data" / "Fish" / f"FishData_{name}.asset").read_text(encoding="utf-8")
    assert f"visualProfile: {{fileID: 11400000, guid: {profile_guid}, type: 2}}" in fish_data
    entries = re.findall(
        r"    - serializedVersion: 2\n      name: (\w+)\n      rect:\n"
        r"        serializedVersion: 2\n        x: (\d+)\n        y: (\d+)\n"
        r"        width: (\d+)\n        height: (\d+)\n.*?"
        r"      spriteID: ([0-9a-f]+)\n      internalID: (-?\d+)\n",
        meta, re.S,
    )
    assert len(entries) == 16
    assert len({entry[5] for entry in entries}) == 16
    assert len({entry[6] for entry in entries}) == 16
    for row, direction in enumerate(DIRECTIONS):
        for col in range(4):
            sprite_name, x, y, width, height, _, file_id = entries[row * 4 + col]
            assert sprite_name == f"{name.lower()}_{direction}_{col}"
            assert (int(x), int(y), int(width), int(height)) == (
                col * cell, (3 - row) * cell, cell, cell)
            assert f"{{fileID: {file_id}, guid: {guid}, type: 3}}" in profile
    assert profile.count("  diagonalNorthWestFrames:\n") == 1


if __name__ == "__main__":
    for species, values in SPECIES.items():
        check_species(species, *values)
        print(f"PASS {species}: 16 frames, old pixels, NW, belly, imports and profile")
    print(f"PASS {len(SPECIES)}/{len(SPECIES)} species")
