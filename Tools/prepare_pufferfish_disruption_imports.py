"""Seed stable Unity import IDs for new assets; Editor Setup verifies/relinks them."""

from pathlib import Path
import re


ROOT = Path(__file__).resolve().parents[1]
ASSETS = ROOT / "Assets"
ACTION_GUID = "3198b55294e1455e9af2a95335071741"
IMPACT_GUID = "8a115ba590a3436a912c965ac77e33d8"
AUDIO_GUID = "fe461a79a0e84fc58893a542299e094c"
PROFILE_GUID = "ef322d530c4841bd9a9942804f3eb8cb"
SCRIPT_GUID = "c978bd98e47849b59d377894367df383"


def write_new(path, content):
    path.parent.mkdir(parents=True, exist_ok=True)
    if path.exists():
        if path.read_text(encoding="utf-8") != content:
            raise RuntimeError(f"Existing import metadata differs: {path}")
        return
    path.write_text(content, encoding="utf-8", newline="\n")


action = (ASSETS / "Art/Fish/Squid/Squid_InkAttack.png.meta").read_text(encoding="utf-8")
action = action.replace("b5893681b83d1594b98c806b9c0ec55d", ACTION_GUID)
action = action.replace("Squid_InkAttack", "Pufferfish_Disrupt")
action = action.replace("squid_ink_", "puffer_disrupt_")
action = re.sub(r"(        [xy]: )(192|128|64)(\n)",
                lambda match: match[1] + str(int(match[2]) * 3 // 4) + match[3], action)
action = re.sub(r"(        (?:width|height): )64(\n)", r"\g<1>48\2", action)
write_new(ASSETS / "Art/Fish/Pufferfish/Pufferfish_Disrupt.png.meta", action)

impact = (ASSETS / "Art/VFX/Squid/Squid_InkImpact.png.meta").read_text(encoding="utf-8")
impact = impact.replace("9cdd9bed85fc581680d5c332792f233f", IMPACT_GUID)
impact = impact.replace("Squid_InkImpact", "Pufferfish_NetImpact")
impact = impact.replace("squid_ink_impact_", "puffer_net_impact_")
write_new(ASSETS / "Art/VFX/Pufferfish/Pufferfish_NetImpact.png.meta", impact)

audio = (ASSETS / "Audio/SFX/SpecialFish/Squid_InkRelease.wav.meta").read_text(encoding="utf-8")
audio = audio.replace("83e267c39e7065843860dc2b6de3baa9", AUDIO_GUID)
write_new(ASSETS / "Audio/SFX/SpecialFish/Pufferfish_NetDisrupt.wav.meta", audio)

write_new(ASSETS / "Art/VFX/Pufferfish.meta",
          "fileFormatVersion: 2\nguid: 6659f29cf56d483cb8da845a344583cb\nfolderAsset: yes\nDefaultImporter:\n  externalObjects: {}\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n")

for relative, guid in (
    ("Scripts/Fish/IFishSpecialAnimation.cs", "87e499289f6541c8b49408ee0d7e2a2a"),
    ("Scripts/Fish/PufferfishDisruptionProfile.cs", SCRIPT_GUID),
    ("Editor/PufferfishDisruptionArtSetup.cs", "bf29ee93217d4af1a815276139c82cd8"),
    ("Editor/Tests/PufferfishDisruptionTests.cs", "c97d1cd6e25f4a4285a2f26026e3d918"),
):
    write_new(ASSETS / f"{relative}.meta", f"fileFormatVersion: 2\nguid: {guid}\n")

action_ids = [int(value) for value in re.findall(r"^      internalID: (-?\d+)$", action, re.M)]
impact_ids = [int(value) for value in re.findall(r"^      internalID: (-?\d+)$", impact, re.M)]
assert len(action_ids) == 16 and len(impact_ids) == 4

def refs(ids, guid):
    return "\n".join(f"  - {{fileID: {value}, guid: {guid}, type: 3}}" for value in ids)

profile = f"""%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!114 &11400000
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 0}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: {SCRIPT_GUID}, type: 3}}
  m_Name: PufferfishDisruption
  m_EditorClassIdentifier: Assembly-CSharp::PufferfishDisruptionProfile
  horizontalFrames:
{refs(action_ids[0:4], ACTION_GUID)}
  verticalFrames:
{refs(action_ids[4:8], ACTION_GUID)}
  diagonalFrames:
{refs(action_ids[8:12], ACTION_GUID)}
  diagonalNorthWestFrames:
{refs(action_ids[12:16], ACTION_GUID)}
  impactFrames:
{refs(impact_ids, IMPACT_GUID)}
  framesPerSecond: 12
  soundVolume: 0.34
  soundClip: {{fileID: 8300000, guid: {AUDIO_GUID}, type: 3}}
"""
write_new(ASSETS / "Resources/PufferfishDisruption.asset", profile)
write_new(ASSETS / "Resources/PufferfishDisruption.asset.meta",
          f"fileFormatVersion: 2\nguid: {PROFILE_GUID}\nNativeFormatImporter:\n  externalObjects: {{}}\n  mainObjectFileID: 11400000\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n")
