"""Reuse the previous proof's protection routine, preserving that proof in full."""
import importlib.util
import json
import shutil
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[4]
QA = ROOT / "Docs/QA/Tutorial2p5DForegroundRefine_20261001"
TEMP = ROOT / ".codex_tmp/Tutorial2p5DForegroundRefine_20261001"
PREVIOUS = ROOT / "Docs/QA/Tutorial2p5DProof_20261001"
spec = importlib.util.spec_from_file_location("previous_proof_protection", PREVIOUS / "Evidence/Prepare.py")
audit = importlib.util.module_from_spec(spec)
sys.dont_write_bytecode = True
spec.loader.exec_module(audit)
audit.QA = QA
audit.TEMP = TEMP

if __name__ == "__main__":
    if len(sys.argv) > 1 and sys.argv[1] == "verify":
        audit.verify()
        raise SystemExit()
    assert audit.git("branch", "--show-current") == "main"
    assert audit.git("rev-parse", "HEAD") == audit.BASE
    assert audit.git("rev-parse", "origin/main") == audit.BASE
    assert not audit.git("diff", "--cached", "--name-only")
    project = TEMP / "RefineProject"
    assert not project.exists(), "Do not overwrite any proof"
    TEMP.mkdir(parents=True, exist_ok=True)
    protection = {"HEAD": audit.BASE, "status": audit.git("status", "--short"), "files": audit.inventory()}
    (TEMP / "protection_start.json").write_text(json.dumps(protection, ensure_ascii=False, indent=2), encoding="utf-8")
    for folder in ("Assets", "Packages", "ProjectSettings"):
        shutil.copytree(ROOT / folder, project / folder)
    old_project = ROOT / ".codex_tmp/Tutorial2p5DProof_20261001/ProofProject"
    shutil.copytree(old_project / "Assets/Resources/Proof", project / "Assets/Resources/Proof")
    for name, destination in (
        ("Tutorial2p5DBackground.cs", "Assets/Proof/Tutorial2p5DBackground.cs"),
        ("ProofRuntimeQA.cs", "Assets/Proof/ProofRuntimeQA.cs"),
        ("ProofEntry.cs", "Assets/Editor/ProofEntry.cs"),
        ("FloorBlend.shader", "Assets/Resources/Proof/FloorBlend.shader"),
    ):
        path = project / destination
        path.parent.mkdir(parents=True, exist_ok=True)
        shutil.copyfile(QA / "Evidence" / name, path)
    refined = QA / "Assets/Tutorial2p5D_NearRefined.png"
    copied = project / "Assets/Resources/Proof/Tutorial2p5D" / refined.name
    shutil.copyfile(refined, copied)
    assert audit.digest(copied) == audit.digest(refined)
    manifest = {
        "project": str(project), "baseline_HEAD": audit.BASE,
        "previous_proof": PREVIOUS.relative_to(ROOT).as_posix(),
        "protected_files": len(protection["files"]),
        "main_stage_sha": audit.digest(ROOT / "Assets/Scenes/MainStage.unity"),
        "tutorial_sha": audit.digest(ROOT / "Assets/Scenes/Tutorial.unity"),
        "bgm_sha": audit.digest(ROOT / "Assets/Resources/Audio/YasashiiOdori.mp3"),
        "refined_near_sha": audit.digest(refined),
        "copied_near_sha": audit.digest(copied),
    }
    (QA / "Preparation.json").write_text(json.dumps(manifest, ensure_ascii=False, indent=2), encoding="utf-8")
    print(json.dumps(manifest, ensure_ascii=False, indent=2))
