"""Snapshot before production integration; protect existing files and builds."""
import hashlib
import importlib.util
import json
import shutil
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[4]
QA = ROOT / "Docs/QA/Tutorial2p5DProductionIntegration_20261001"
TEMP = ROOT / ".codex_tmp/Tutorial2p5DProductionIntegration_20261001"
sys.dont_write_bytecode = True
spec = importlib.util.spec_from_file_location("protection", ROOT / "Docs/QA/Tutorial2p5DProof_20261001/Evidence/Prepare.py")
audit = importlib.util.module_from_spec(spec)
spec.loader.exec_module(audit)
audit.QA, audit.TEMP = QA, TEMP
ALLOWED = {"Assets/Scripts/HimoHitoCraftRoomBackground.cs"}


def verify():
    start = json.loads((TEMP / "protection_start.json").read_text(encoding="utf-8"))
    changed, failures = [], []
    for name, info in start["files"].items():
        path = ROOT / name
        if not path.is_file() or audit.digest(path) != info["sha256"]:
            (changed if name in ALLOWED else failures).append(name)
    new_assets = [p.relative_to(ROOT).as_posix() for folder in ("Assets", "Packages", "ProjectSettings")
                  for p in (ROOT / folder).rglob("*") if p.is_file() and p.relative_to(ROOT).as_posix() not in start["files"]]
    unexpected = [n for n in new_assets if not (n.startswith("Assets/Resources/Art/Tutorial2p5D/")
                   or n in {"Assets/Resources/Art/Tutorial2p5D.meta", "Assets/Resources/TutorialBackgroundFloor.shader", "Assets/Resources/TutorialBackgroundFloor.shader.meta", "Assets/Resources/TutorialBackgroundEdge.shader", "Assets/Resources/TutorialBackgroundEdge.shader.meta", "Assets/Scripts/TutorialCraftRoomLayers.cs", "Assets/Scripts/TutorialCraftRoomLayers.cs.meta"})]
    result = {"protected_files": len(start["files"]), "authorized_source_changes": changed,
              "unexpected_changes": failures, "new_production_assets": new_assets, "unexpected_new": unexpected,
              "HEAD": audit.git("rev-parse", "HEAD"), "origin_main": audit.git("rev-parse", "origin/main"),
              "branch": audit.git("branch", "--show-current"), "staged": audit.git("diff", "--cached", "--name-only"),
              "status": audit.git("status", "--short")}
    result["PASS"] = not failures and not unexpected and result["HEAD"] == audit.BASE and not result["staged"]
    (QA / "ProtectionResult.json").write_text(json.dumps(result, ensure_ascii=False, indent=2), encoding="utf-8")
    print(json.dumps(result, ensure_ascii=False, indent=2))
    assert result["PASS"]


if __name__ == "__main__":
    if len(sys.argv) > 1 and sys.argv[1] == "verify":
        verify()
        raise SystemExit()
    assert audit.git("rev-parse", "HEAD") == audit.BASE == audit.git("rev-parse", "origin/main")
    assert audit.git("branch", "--show-current") == "main" and not audit.git("diff", "--cached", "--name-only")
    assert not TEMP.exists(), "Never overwrite previous work"
    TEMP.mkdir(parents=True)
    start = {"HEAD": audit.BASE, "status": audit.git("status", "--short"), "files": audit.inventory()}
    (TEMP / "protection_start.json").write_text(json.dumps(start, ensure_ascii=False, indent=2), encoding="utf-8")
    for folder in ("Assets", "Packages", "ProjectSettings"):
        shutil.copytree(ROOT / folder, TEMP / "BaselineProject" / folder)
    before = QA / "Evidence/BeforeProduction"
    before.mkdir()
    shutil.copyfile(ROOT / "Assets/Scripts/HimoHitoCraftRoomBackground.cs", before / "HimoHitoCraftRoomBackground.cs")
    (QA / "Preparation.json").write_text(json.dumps({"HEAD": audit.BASE, "protected_files": len(start["files"]),
        "main_scene_sha": audit.digest(ROOT / "Assets/Scenes/MainStage.unity"), "tutorial_scene_sha": audit.digest(ROOT / "Assets/Scenes/Tutorial.unity"),
        "bgm_sha": audit.digest(ROOT / "Assets/Resources/Audio/YasashiiOdori.mp3"), "baseline_copy": str(TEMP / "BaselineProject")}, ensure_ascii=False, indent=2), encoding="utf-8")
    print("Protected", len(start["files"]), "files; independent baseline ready")
