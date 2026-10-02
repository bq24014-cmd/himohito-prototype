"""Preserve all pre-existing files; make two working-tree diagnostic copies."""
import importlib.util
import json
import shutil
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[4]
QA = ROOT / "Docs/QA/Tutorial2p5DIntroFix_20261002"
TEMP = ROOT / ".codex_tmp/Tutorial2p5DIntroFix_20261002"
SOURCE = "Assets/Scripts/TutorialCraftRoomLayers.cs"
sys.dont_write_bytecode = True
spec = importlib.util.spec_from_file_location("protection", ROOT / "Docs/QA/Tutorial2p5DProof_20261001/Evidence/Prepare.py")
audit = importlib.util.module_from_spec(spec)
spec.loader.exec_module(audit)
audit.QA, audit.TEMP = QA, TEMP


def prepare(which):
    project = TEMP / (which + "Project")
    assert not project.exists(), "Never overwrite previous copies"
    for folder in ("Assets", "Packages", "ProjectSettings"):
        shutil.copytree(ROOT / folder, project / folder)
    helpers = QA / "Evidence"
    previous = ROOT / "Docs/QA/Tutorial2p5DProductionIntegration_20261001/Evidence"
    for original, target in (
        (helpers / "IntroEntry.cs", project / "Assets/Editor/IntroEntry.cs"),
        (helpers / "IntroTimelineQA.cs", project / "Assets/Diagnostics/IntroTimelineQA.cs"),
        (previous / "IntegrationQA.cs", project / "Assets/Diagnostics/IntegrationQA.cs"),
        (previous / "ReferenceTrial.cs", project / "Assets/Diagnostics/ReferenceTrial.cs"),
        (ROOT / ".codex_tmp/Tutorial2p5DAllSections_20261001/TrialProject/Assets/Resources/Proof/FloorBlend.shader",
         project / "Assets/Resources/LegacyFloor.shader"),
    ):
        target.parent.mkdir(parents=True, exist_ok=True)
        shutil.copyfile(original, target)
    print("Prepared", which, "copy", flush=True)


def verify():
    start = json.loads((TEMP / "protection_start.json").read_text(encoding="utf-8"))
    changed, unexpected = [], []
    for name, info in start["files"].items():
        path = ROOT / name
        if not path.is_file() or audit.digest(path) != info["sha256"]:
            (changed if name == SOURCE else unexpected).append(name)
    new_production = [p.relative_to(ROOT).as_posix() for folder in ("Assets", "Packages", "ProjectSettings")
                      for p in (ROOT / folder).rglob("*") if p.is_file() and p.relative_to(ROOT).as_posix() not in start["files"]]
    result = {"protected_file_count": len(start["files"]), "authorized_changes": changed,
              "unexpected_changes": unexpected, "new_production_files": new_production,
              "HEAD": audit.git("rev-parse", "HEAD"), "origin_main": audit.git("rev-parse", "origin/main"),
              "branch": audit.git("branch", "--show-current"), "staged": audit.git("diff", "--cached", "--name-only"),
              "status": audit.git("status", "--short"),
              "main_scene_sha": audit.digest(ROOT / "Assets/Scenes/MainStage.unity"),
              "tutorial_scene_sha": audit.digest(ROOT / "Assets/Scenes/Tutorial.unity"),
              "bgm_sha": audit.digest(ROOT / "Assets/Resources/Audio/YasashiiOdori.mp3"),
              "production_source_sha": audit.digest(ROOT / SOURCE)}
    result["PASS"] = not unexpected and not new_production and result["HEAD"] == start["HEAD"] == result["origin_main"] and not result["staged"]
    (QA / "ProtectionResult.json").write_text(json.dumps(result, ensure_ascii=False, indent=2), encoding="utf-8")
    print(json.dumps(result, ensure_ascii=False, indent=2))
    assert result["PASS"]


if __name__ == "__main__":
    mode = sys.argv[1]
    if mode == "verify":
        verify()
    elif mode in ("after", "copy-before"):
        prepare("After" if mode == "after" else "Before")
    else:
        assert audit.git("rev-parse", "HEAD") == audit.BASE == audit.git("rev-parse", "origin/main")
        assert audit.git("branch", "--show-current") == "main" and not audit.git("diff", "--cached", "--name-only")
        assert not TEMP.exists()
        TEMP.mkdir(parents=True)
        print("Hashing protected Production, QA, audio and old builds...", flush=True)
        start = {"HEAD": audit.BASE, "status": audit.git("status", "--short"), "files": audit.inventory()}
        (TEMP / "protection_start.json").write_text(json.dumps(start, ensure_ascii=False, indent=2), encoding="utf-8")
        before = QA / "Evidence/BeforeProduction"
        before.mkdir()
        shutil.copyfile(ROOT / SOURCE, before / "TutorialCraftRoomLayers.cs")
        (QA / "Preparation.json").write_text(json.dumps({"HEAD": audit.BASE, "branch": "main", "status": start["status"],
            "protected_files": len(start["files"]), "main_scene_sha": audit.digest(ROOT / "Assets/Scenes/MainStage.unity"),
            "tutorial_scene_sha": audit.digest(ROOT / "Assets/Scenes/Tutorial.unity"), "bgm_sha": audit.digest(ROOT / "Assets/Resources/Audio/YasashiiOdori.mp3"),
            "production_source_sha": audit.digest(ROOT / SOURCE)}, ensure_ascii=False, indent=2), encoding="utf-8")
        print("Snapshot complete:", len(start["files"]), "files", flush=True)
        if mode == "before":
            prepare("Before")
