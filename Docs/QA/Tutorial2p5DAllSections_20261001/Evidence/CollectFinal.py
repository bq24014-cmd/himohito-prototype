"""Inventory this new trial; never modify production or any previous evidence."""
import hashlib
import json
import shutil
import subprocess
from pathlib import Path

ROOT = Path(__file__).resolve().parents[4]
QA = ROOT / "Docs/QA/Tutorial2p5DAllSections_20261001"
TEMP = ROOT / ".codex_tmp/Tutorial2p5DAllSections_20261001"
PROJECT = TEMP / "TrialProject"
BUILD = ROOT / "output/Tutorial2p5DAllSections_20261001_Trial01/Game"
PUBLIC = Path("C:/Users/Public/HimoHito_Tutorial2p5DAllSections_20261001_Trial01")
start = json.loads((TEMP / "protection_start.json").read_text(encoding="utf-8"))


def sha(path):
    with path.open("rb") as file:
        return hashlib.file_digest(file, "sha256").hexdigest().upper()


for name in ("UnityTrial01.log", "UnityTrial02.log", "UnityBuild.log"):
    path = TEMP / name
    if path.exists():
        shutil.copyfile(path, QA / "Evidence" / name)
for path in PROJECT.joinpath("Assets").rglob("*.meta"):
    relative = path.relative_to(PROJECT)
    if "Proof" in relative.parts or path.name == "Proof.meta" or relative.as_posix() == "Assets/Editor/TrialEntry.cs.meta":
        dest = QA / "Evidence/ImportedMetadata" / relative
        dest.parent.mkdir(parents=True, exist_ok=True)
        shutil.copyfile(path, dest)

source_diffs, setting_diffs = [], []
for name, info in start["files"].items():
    if name.startswith(("Assets/Scripts/", "Assets/Editor/", "Packages/", "ProjectSettings/")):
        copy = PROJECT / name
        if not copy.exists() or sha(copy) != info["sha256"]:
            (setting_diffs if name.startswith("ProjectSettings/") else source_diffs).append(name)

scenes = {}
for name in ("Tutorial.unity", "MainStage.unity"):
    source = ROOT / "Assets/Scenes" / name
    copy = PROJECT / "Assets/Scenes" / name
    scenes[name] = {"production_sha": sha(source), "isolated_after_build_sha": sha(copy), "same": sha(source) == sha(copy)}

launch = {}
log = PUBLIC / "PlayerTrial.log"
if log.exists():
    copied = QA / "Evidence/PlayerTrial.log"
    shutil.copyfile(log, copied)
    launch = {"public_exe": str(PUBLIC / "Game/HimoHitoTutorialAll2p5D.exe"),
              "sha_matches": sha(PUBLIC / "Game/HimoHitoTutorialAll2p5D.exe") == sha(BUILD / "HimoHitoTutorialAll2p5D.exe"),
              "player_log": copied.relative_to(QA).as_posix()}

result = {
    "baseline": start["HEAD"], "input": "Protected working-tree snapshot, NOT clean HEAD",
    "production": "ProtectionResult.json contains hash comparison of all 3495 pre-existing protected files",
    "isolated_production_source_differences": source_diffs,
    "isolated_settings_differences": setting_diffs,
    "settings_note": "Only isolated companyName/productName intentionally changed. Existing Editor Scene Sync may serialize isolated MainStage; production Scene is untouched.",
    "scene_comparison": scenes,
    "trial_source": ["Evidence/TutorialAllBackground.cs", "Evidence/AllSectionsQA.cs", "Evidence/TrialEntry.cs"],
    "added_trial_assets_only_in_isolation": [p.relative_to(PROJECT).as_posix() for p in sorted(PROJECT.joinpath("Assets/Proof").rglob("*")) if p.is_file()]
        + [p.relative_to(PROJECT).as_posix() for p in sorted(PROJECT.joinpath("Assets/Resources/Proof").rglob("*")) if p.is_file()]
        + ["Assets/Editor/TrialEntry.cs", "Assets/Editor/TrialEntry.cs.meta"],
    "build": str(BUILD), "build_result": (BUILD / "BUILD_RESULT.txt").read_text(encoding="utf-8"),
    "exe_sha256": sha(BUILD / "HimoHitoTutorialAll2p5D.exe"), "launch": launch,
    "launchers": ["START_TRIAL.bat", "START_BEFORE.bat", "START_PRODUCTION.bat"],
    "art_and_bgm": "Previous approved assets and ignored BGM copied byte-for-byte; no originals changed",
    "qa_files": [p.relative_to(ROOT).as_posix() for p in sorted(QA.rglob("*")) if p.is_file()],
    "git_index": subprocess.check_output(["git", "-c", "safe.directory=" + ROOT.as_posix(), "diff", "--cached", "--name-only"], cwd=ROOT).decode("utf-8").strip(),
}
(QA / "ChangeManifest.json").write_text(json.dumps(result, ensure_ascii=False, indent=2), encoding="utf-8")
assert not source_diffs, source_diffs
assert setting_diffs == ["ProjectSettings/ProjectSettings.asset"], setting_diffs
assert not result["git_index"]
print(json.dumps({k: v for k, v in result.items() if k not in ("qa_files", "added_trial_assets_only_in_isolation")}, ensure_ascii=False, indent=2))
