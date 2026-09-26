"""Create stable Unity import metadata for the original VS-2D-1 assets."""

from pathlib import Path
import re
import uuid


ROOT = Path(__file__).resolve().parents[1]
ASSETS = ROOT / "Assets"
TEMPLATE = (ASSETS / "Art/VFX/Pufferfish/Pufferfish_NetImpact.png.meta").read_text(
    encoding="utf-8"
)
IDS = [79328738, 1197123931, 766587199, 1000670799]


def guid() -> str:
    return uuid.uuid4().hex


def write_once(path: Path, content: str) -> None:
    if not path.exists():
        path.write_text(content, encoding="utf-8", newline="\n")


def folder(path: Path) -> None:
    write_once(Path(str(path) + ".meta"),
               f"fileFormatVersion: 2\nguid: {guid()}\nfolderAsset: yes\n"
               "DefaultImporter:\n  externalObjects: {}\n  userData: \n"
               "  assetBundleName: \n  assetBundleVariant: \n")


def sprite_meta(path: Path, prefix: str, cell: int, count: int, ppu: int) -> tuple[str, list[int]]:
    meta = Path(str(path) + ".meta")
    if meta.exists():
        source = meta.read_text(encoding="utf-8")
        asset_guid = re.search(r"^guid: ([0-9a-f]+)$", source, re.M).group(1)
        values = [int(value) for value in re.findall(r"^      internalID: (-?\d+)$", source, re.M)]
        return asset_guid, values[:count]

    asset_guid = guid()
    text = re.sub(r"^guid: [0-9a-f]+$", f"guid: {asset_guid}", TEMPLATE, count=1, flags=re.M)
    names = [f"{prefix}_{i}" for i in range(count)]
    id_table = "  internalIDToNameTable:\n" + "".join(
        f"  - first:\n      213: {IDS[i]}\n    second: {names[i]}\n"
        for i in range(count)
    )
    text = re.sub(r"  internalIDToNameTable:\n.*?(?=  externalObjects:)",
                  id_table, text, count=1, flags=re.S)
    text = re.sub(r"spritePixelsToUnits: \d+", f"spritePixelsToUnits: {ppu}", text, count=1)
    sprites = ""
    for i, name in enumerate(names):
        sprites += (
            "    - serializedVersion: 2\n"
            f"      name: {name}\n"
            "      rect:\n        serializedVersion: 2\n"
            f"        x: {i * cell}\n        y: 0\n"
            f"        width: {cell}\n        height: {cell}\n"
            "      alignment: 0\n      pivot: {x: 0.5, y: 0.5}\n"
            "      border: {x: 0, y: 0, z: 0, w: 0}\n"
            "      customData: \n      outline: []\n      physicsShape: []\n"
            "      tessellationDetail: 0\n      bones: []\n"
            f"      spriteID: {guid()}\n      internalID: {IDS[i]}\n"
            "      vertices: []\n      indices: \n      edges: []\n      weights: []\n"
        )
    sheet = (
        "  spriteSheet:\n    serializedVersion: 2\n    sprites:\n" + sprites +
        "    outline: []\n    customData: \n    physicsShape: []\n"
        "    bones: []\n"
        f"    spriteID: {guid()}\n"
        "    internalID: 0\n    vertices: []\n    indices: \n"
        "    edges: []\n    weights: []\n    secondaryTextures: []\n"
        "    spriteCustomMetadata:\n      entries: []\n    nameFileIdTable:\n" +
        "".join(f"      {names[i]}: {IDS[i]}\n" for i in range(count))
    )
    text = re.sub(r"  spriteSheet:\n.*?(?=  mipmapLimitGroupName:)",
                  sheet, text, count=1, flags=re.S)
    write_once(meta, text)
    return asset_guid, IDS[:count]


for directory in ("Art/Tools", "Art/Tools/FishingRod", "Art/VFX/Tools", "Audio/SFX/Tools"):
    folder(ASSETS / directory)

idle_guid, idle_ids = sprite_meta(ASSETS / "Art/Tools/FishingRod/FishingRod_Idle.png",
                                  "FishingRod_Idle", 32, 1, 32)
attack_guid, attack_ids = sprite_meta(ASSETS / "Art/Tools/FishingRod/FishingRod_Attack.png",
                                      "FishingRod_Attack", 32, 3, 32)
hit_guid, hit_ids = sprite_meta(ASSETS / "Art/VFX/Tools/FishingRod_Hit.png",
                                "FishingRod_Hit", 16, 4, 83)

audio_path = ASSETS / "Audio/SFX/Tools/FishingRod_Hit.wav.meta"
audio_guid = guid()
if audio_path.exists():
    audio_guid = re.search(r"^guid: ([0-9a-f]+)$", audio_path.read_text(encoding="utf-8"), re.M).group(1)
else:
    audio_template = (ASSETS / "Audio/SFX/SpecialFish/Pufferfish_NetDisrupt.wav.meta").read_text(
        encoding="utf-8"
    )
    write_once(audio_path, re.sub(r"^guid: [0-9a-f]+$", f"guid: {audio_guid}",
                                  audio_template, count=1, flags=re.M))

scripts = {}
for relative in ("Scripts/Gear/FishingRodPresentation.cs",
                 "Scripts/Gear/FishingRodPresentationProfile.cs",
                 "Editor/FishingRodPresentationArtSetup.cs",
                 "Editor/Tests/FishingRodPresentationTests.cs"):
    path = ASSETS / (relative + ".meta")
    script_guid = guid()
    if path.exists():
        script_guid = re.search(r"^guid: ([0-9a-f]+)$", path.read_text(encoding="utf-8"), re.M).group(1)
    else:
        write_once(path, f"fileFormatVersion: 2\nguid: {script_guid}\n")
    scripts[relative] = script_guid

profile_path = ASSETS / "Resources/FishingRodPresentation.asset"
ref = lambda item_id, asset_guid: f"{{fileID: {item_id}, guid: {asset_guid}, type: 3}}"
profile = (
    "%YAML 1.1\n%TAG !u! tag:unity3d.com,2011:\n--- !u!114 &11400000\n"
    "MonoBehaviour:\n  m_ObjectHideFlags: 0\n"
    "  m_CorrespondingSourceObject: {fileID: 0}\n"
    "  m_PrefabInstance: {fileID: 0}\n  m_PrefabAsset: {fileID: 0}\n"
    "  m_GameObject: {fileID: 0}\n  m_Enabled: 1\n  m_EditorHideFlags: 0\n"
    f"  m_Script: {{fileID: 11500000, guid: {scripts['Scripts/Gear/FishingRodPresentationProfile.cs']}, type: 3}}\n"
    "  m_Name: FishingRodPresentation\n"
    "  m_EditorClassIdentifier: Assembly-CSharp::FishingRodPresentationProfile\n"
    f"  idleSprite: {ref(idle_ids[0], idle_guid)}\n"
    "  attackFrames:\n" +
    "".join(f"  - {ref(item_id, attack_guid)}\n" for item_id in attack_ids) +
    "  hitFrames:\n" +
    "".join(f"  - {ref(item_id, hit_guid)}\n" for item_id in hit_ids) +
    f"  hitClip: {{fileID: 8300000, guid: {audio_guid}, type: 3}}\n"
    "  attackDuration: 0.27\n  lineDuration: 0.18\n"
    "  hitDuration: 0.22\n  hitVolume: 0.28\n"
)
write_once(profile_path, profile)
write_once(Path(str(profile_path) + ".meta"),
           f"fileFormatVersion: 2\nguid: {guid()}\nNativeFormatImporter:\n"
           "  externalObjects: {}\n  mainObjectFileID: 11400000\n"
           "  userData: \n  assetBundleName: \n  assetBundleVariant: \n")
