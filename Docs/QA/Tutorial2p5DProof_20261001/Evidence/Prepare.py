"""Prepare an isolated working-tree proof; never edit production Assets."""
import hashlib
import json
import shutil
import subprocess
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[4]
QA = ROOT / "Docs/QA/Tutorial2p5DProof_20261001"
TEMP = ROOT / ".codex_tmp/Tutorial2p5DProof_20261001"
BASE = "702a4bc044b0eb35bedb552753ca8495599bc1ee"


def digest(path):
    with path.open("rb") as stream:
        return hashlib.file_digest(stream, "sha256").hexdigest().upper()


def git(*args):
    return subprocess.check_output(
        ["git", "-c", "safe.directory=" + ROOT.as_posix(), *args], cwd=ROOT
    ).decode("utf-8").strip()


def inventory():
    result = {}
    for folder in ("Assets", "Packages", "ProjectSettings", "Docs", "output", "Builds"):
        base = ROOT / folder
        if not base.exists():
            continue
        for path in sorted(base.rglob("*")):
            if path.is_file() and not path.is_relative_to(QA):
                result[path.relative_to(ROOT).as_posix()] = {
                    "size": path.stat().st_size, "sha256": digest(path)
                }
    for name in ("README.md", "AGENTS.md", ".gitignore"):
        path = ROOT / name
        if path.exists():
            result[name] = {"size": path.stat().st_size, "sha256": digest(path)}
    return result


def verify():
    start = json.loads((TEMP / "protection_start.json").read_text(encoding="utf-8"))
    mismatches = []
    for name, info in start["files"].items():
        path = ROOT / name
        if not path.is_file() or digest(path) != info["sha256"]:
            mismatches.append(name)
    new_production = []
    for folder in ("Assets", "Packages", "ProjectSettings"):
        for path in (ROOT / folder).rglob("*"):
            if path.is_file() and path.relative_to(ROOT).as_posix() not in start["files"]:
                new_production.append(path.relative_to(ROOT).as_posix())
    result = {
        "protected_file_count": len(start["files"]), "mismatches": mismatches,
        "new_production_files": new_production, "HEAD": git("rev-parse", "HEAD"),
        "origin_main": git("rev-parse", "origin/main"),
        "staged": git("diff", "--cached", "--name-only"),
        "status": git("status", "--short"),
        "PASS": not mismatches and not new_production and git("rev-parse", "HEAD") == BASE
        and not git("diff", "--cached", "--name-only"),
    }
    (QA / "ProtectionResult.json").write_text(json.dumps(result, ensure_ascii=False, indent=2), encoding="utf-8")
    print(json.dumps(result, ensure_ascii=False, indent=2))
    if not result["PASS"]:
        raise SystemExit(2)


if __name__ == "__main__":
    if len(sys.argv) > 1 and sys.argv[1] == "verify":
        verify()
        raise SystemExit()
    assert git("branch", "--show-current") == "main"
    assert git("rev-parse", "HEAD") == BASE and git("rev-parse", "origin/main") == BASE
    assert not git("diff", "--cached", "--name-only"), "Existing index must be empty"
    project = TEMP / "ProofProject"
    assert not project.exists(), "Refuse to overwrite an existing proof"
    TEMP.mkdir(parents=True, exist_ok=True)
    start = {"HEAD": BASE, "status": git("status", "--short"), "files": inventory()}
    (TEMP / "protection_start.json").write_text(json.dumps(start, ensure_ascii=False, indent=2), encoding="utf-8")
    for folder in ("Assets", "Packages", "ProjectSettings"):
        shutil.copytree(ROOT / folder, project / folder)
    source = QA / "Evidence"
    for name, dest in (
        ("Tutorial2p5DBackground.cs", "Assets/Proof/Tutorial2p5DBackground.cs"),
        ("ProofRuntimeQA.cs", "Assets/Proof/ProofRuntimeQA.cs"),
        ("ProofEntry.cs", "Assets/Editor/ProofEntry.cs"),
    ):
        path = project / dest
        path.parent.mkdir(parents=True, exist_ok=True)
        shutil.copyfile(source / name, path)
    art = ROOT / "Docs/VisualConcepts/Tutorial2p5D_20261001"
    copies = []
    for layer in ("Far", "Mid", "Near"):
        original = art / ("Tutorial2p5D_" + layer + ".png")
        copied = project / "Assets/Resources/Proof/Tutorial2p5D" / original.name
        copied.parent.mkdir(parents=True, exist_ok=True)
        shutil.copyfile(original, copied)
        assert digest(original) == digest(copied)
        copies.append({"source": original.relative_to(ROOT).as_posix(), "copy": str(copied), "sha256": digest(original)})
    manifest = {
        "project": str(project), "protected_file_count": len(start["files"]),
        "baseline": BASE, "working_tree_source": True,
        "main_stage_initial_sha": digest(ROOT / "Assets/Scenes/MainStage.unity"),
        "tutorial_initial_sha": digest(ROOT / "Assets/Scenes/Tutorial.unity"),
        "bgm_sha": digest(ROOT / "Assets/Resources/Audio/YasashiiOdori.mp3"),
        "materials": copies,
    }
    (QA / "Preparation.json").write_text(json.dumps(manifest, ensure_ascii=False, indent=2), encoding="utf-8")
    print(json.dumps(manifest, ensure_ascii=False, indent=2))
