"""Copy working-tree production and approved art into a NEW isolated trial."""
import importlib.util
import json
import shutil
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[4]
QA = ROOT / "Docs/QA/Tutorial2p5DAllSections_20261001"
TEMP = ROOT / ".codex_tmp/Tutorial2p5DAllSections_20261001"
PREVIOUS = ROOT / "Docs/QA/Tutorial2p5DProof_20261001"
REFINE = ROOT / "Docs/QA/Tutorial2p5DForegroundRefine_20261001"
sys.dont_write_bytecode = True
spec = importlib.util.spec_from_file_location("approved_protection", PREVIOUS / "Evidence/Prepare.py")
audit = importlib.util.module_from_spec(spec)
spec.loader.exec_module(audit)
audit.QA, audit.TEMP = QA, TEMP

if __name__ == "__main__":
    if len(sys.argv) > 1 and sys.argv[1] == "verify":
        audit.verify()
        raise SystemExit()
    assert audit.git("branch", "--show-current") == "main"
    assert audit.git("rev-parse", "HEAD") == audit.BASE
    assert audit.git("rev-parse", "origin/main") == audit.BASE
    assert not audit.git("diff", "--cached", "--name-only")
    project = TEMP / "TrialProject"
    assert not project.exists(), "Never overwrite a previous trial"
    TEMP.mkdir(parents=True, exist_ok=True)
    protection = {"HEAD": audit.BASE, "status": audit.git("status", "--short"), "files": audit.inventory()}
    (TEMP / "protection_start.json").write_text(json.dumps(protection, ensure_ascii=False, indent=2), encoding="utf-8")
    for folder in ("Assets", "Packages", "ProjectSettings"):
        shutil.copytree(ROOT / folder, project / folder)
    art_source = ROOT / ".codex_tmp/Tutorial2p5DForegroundRefine_20261001/RefineProject/Assets/Resources/Proof"
    shutil.copytree(art_source, project / "Assets/Resources/Proof")
    for name, dest in (
        ("TutorialAllBackground.cs", "Assets/Proof/TutorialAllBackground.cs"),
        ("AllSectionsQA.cs", "Assets/Proof/AllSectionsQA.cs"),
        ("TrialEntry.cs", "Assets/Editor/TrialEntry.cs"),
    ):
        target = project / dest
        target.parent.mkdir(parents=True, exist_ok=True)
        shutil.copyfile(QA / "Evidence" / name, target)
    assets = []
    for name in ("Far", "Mid", "Near", "NearRefined"):
        path = art_source / "Tutorial2p5D" / ("Tutorial2p5D_" + name + ".png")
        copied = project / path.relative_to(art_source.parent.parent.parent)
        assert audit.digest(path) == audit.digest(copied)
        assets.append({"source": str(path), "copy": str(copied), "sha256": audit.digest(path)})
    info = {
        "HEAD": audit.BASE, "project": str(project), "working_tree_source": True,
        "protected_files": len(protection["files"]), "assets": assets,
        "main_stage_sha": audit.digest(ROOT / "Assets/Scenes/MainStage.unity"),
        "tutorial_sha": audit.digest(ROOT / "Assets/Scenes/Tutorial.unity"),
        "bgm_sha": audit.digest(ROOT / "Assets/Resources/Audio/YasashiiOdori.mp3"),
        "previous_proof": str(PREVIOUS), "approved_refinement": str(REFINE),
    }
    (QA / "Preparation.json").write_text(json.dumps(info, ensure_ascii=False, indent=2), encoding="utf-8")
    print(json.dumps(info, ensure_ascii=False, indent=2))
