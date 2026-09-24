"""검사한 다섯 시트의 Unity 분할 정보를 네 방향 행으로 확장한다."""

from hashlib import sha256
from pathlib import Path
import re
from uuid import NAMESPACE_URL, uuid5


ROOT = Path(__file__).resolve().parents[1]
SPECIES = {"Sardine": 32, "Mackerel": 48, "Tuna": 64,
           "Pufferfish": 48, "Squid": 64}
DIRECTIONS = ("horizontal", "vertical", "diagonal", "diagonal_nw")


def read_text(path):
    raw = path.read_bytes()
    newline = "\r\n" if b"\r\n" in raw else "\n"
    return raw.decode("utf-8").replace("\r\n", "\n"), newline


def write_text(path, content, newline):
    path.write_bytes(content.replace("\n", newline).encode("utf-8"))


def replace_one(source, old, new):
    if source.count(old) != 1:
        raise ValueError(f"Expected one exact match: {old[:60]}")
    return source.replace(old, new)


def extend_one(species, cell):
    folder = ROOT / "Assets" / "Art" / "Fish" / species
    meta_path = folder / f"{species}_Swim.png.meta"
    profile_path = folder / f"{species}_VisualProfile.asset"
    meta, meta_newline = read_text(meta_path)
    profile, profile_newline = read_text(profile_path)
    prefix = species.lower()

    start_tag = "    sprites:\n"
    end_tag = "    outline: []\n    customData: \n"
    start = meta.index(start_tag, meta.index("  spriteSheet:\n")) + len(start_tag)
    end = meta.index(end_tag, start)
    entries = re.findall(r"    - serializedVersion: 2\n      name: .*?(?=    - serializedVersion: 2\n      name: |\Z)",
                         meta[start:end], re.S)
    if len(entries) != 12 or "".join(entries) != meta[start:end]:
        raise ValueError(f"{species}: expected exactly 12 known sprite entries")

    old_ids = []
    updated = []
    for index, entry in enumerate(entries):
        row, col = divmod(index, 4)
        expected_name = f"{prefix}_{DIRECTIONS[row]}_{col}"
        if f"      name: {expected_name}\n" not in entry:
            raise ValueError(f"{species}: unexpected sprite order at {index}")
        old_y = (2 - row) * cell
        if f"        y: {old_y}\n" not in entry:
            raise ValueError(f"{species}: unexpected old rect at {index}")
        updated.append(replace_one(entry, f"        y: {old_y}\n",
                                   f"        y: {old_y + cell}\n"))
        old_ids.append(int(re.search(r"      internalID: (-?\d+)\n", entry).group(1)))

    new_ids = []
    for col in range(4):
        name = f"{prefix}_diagonal_nw_{col}"
        digest = sha256(name.encode("ascii")).digest()
        file_id = int.from_bytes(digest[:4], "big", signed=True)
        if file_id == 0 or file_id in old_ids or file_id in new_ids:
            raise ValueError(f"{species}: Sprite file ID collision")
        new_ids.append(file_id)
        sprite_id = uuid5(NAMESPACE_URL, f"netbreak/visual/{name}").hex
        entry = entries[col]
        entry = replace_one(entry, f"      name: {prefix}_horizontal_{col}\n",
                            f"      name: {name}\n")
        entry = replace_one(entry, f"        y: {2 * cell}\n", "        y: 0\n")
        entry = replace_one(entry, f"      spriteID: {re.search(r'spriteID: ([0-9a-f]+)', entry).group(1)}\n",
                            f"      spriteID: {sprite_id}\n")
        entry = replace_one(entry, f"      internalID: {old_ids[col]}\n",
                            f"      internalID: {file_id}\n")
        entry = entry.replace("      customData: \n", "      customData:\n")
        entry = entry.replace("      indices: \n", "      indices:\n")
        updated.append(entry)

    meta = meta[:start] + "".join(updated) + meta[end:]
    table_end = meta.index("  mipmapLimitGroupName:", meta.index("    nameFileIdTable:\n"))
    lines = "".join(f"      {prefix}_diagonal_nw_{col}: {file_id}\n"
                    for col, file_id in enumerate(new_ids))
    meta = meta[:table_end] + lines + meta[table_end:]

    sheet_guid = re.search(r"^guid: ([0-9a-f]+)$", meta, re.M).group(1)
    refs = "  diagonalNorthWestFrames:\n" + "".join(
        f"  - {{fileID: {file_id}, guid: {sheet_guid}, type: 3}}\n"
        for file_id in new_ids)
    profile = replace_one(profile, "  framesPerSecond: 8\n", refs + "  framesPerSecond: 8\n")

    write_text(meta_path, meta, meta_newline)
    write_text(profile_path, profile, profile_newline)
    print(f"{species}: preserved 12 Sprite IDs, added 4 NW IDs")


if __name__ == "__main__":
    for fish, size in SPECIES.items():
        extend_one(fish, size)
