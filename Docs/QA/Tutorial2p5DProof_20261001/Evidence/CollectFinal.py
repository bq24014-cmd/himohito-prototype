"""Collect proof metadata and identify every added artifact, no production writes."""
import hashlib
import json
import shutil
from pathlib import Path

ROOT = Path(__file__).resolve().parents[4]
QA = ROOT / "Docs/QA/Tutorial2p5DProof_20261001"
TEMP = ROOT / ".codex_tmp/Tutorial2p5DProof_20261001"
PROJECT = TEMP / "ProofProject"
start = json.loads((TEMP / "protection_start.json").read_text(encoding="utf-8"))


def sha(path):
    with path.open("rb") as file:
        return hashlib.file_digest(file, "sha256").hexdigest().upper()


for name in ("UnityProof.log", "UnityProof_Retry.log", "UnityProof_Attempt02.log", "UnityBuild.log"):
    path = TEMP / name
    if path.exists():
        shutil.copyfile(path, QA / "Evidence" / name)
for path in PROJECT.joinpath("Assets").rglob("*.meta"):
    relative = path.relative_to(PROJECT)
    if "Proof" in relative.parts or relative.as_posix() == "Assets/Editor/ProofEntry.cs.meta":
        dest = QA / "Evidence/ImportedMetadata" / relative
        dest.parent.mkdir(parents=True, exist_ok=True)
        shutil.copyfile(path, dest)

base_diffs = []
for name, info in start["files"].items():
    if name.startswith(("Assets/Scripts/", "Packages/", "ProjectSettings/")):
        copy = PROJECT / name
        if not copy.exists() or sha(copy) != info["sha256"]:
            base_diffs.append(name)

build = ROOT / "output/Tutorial2p5DProof_20261001_Proof01/Game"
scene_comparison = {}
for name in ("Tutorial.unity", "MainStage.unity"):
    original = ROOT / "Assets/Scenes" / name
    copied = PROJECT / "Assets/Scenes" / name
    scene_comparison[name] = {"production_sha": sha(original), "isolated_post_build_sha": sha(copied), "same": sha(original) == sha(copied)}

result = {
    "production_changes": "See ProtectionResult.json; source/scene/content SHA verification against task start.",
    "isolated_baseline_differences": base_diffs,
    "isolated_settings_note": "Only isolation of companyName/productName is intentional. Any SceneSync serialization is restricted to this isolated project and documented separately.",
    "scenes": scene_comparison,
    "proof_build": str(build),
    "build_result": (build / "BUILD_RESULT.txt").read_text(encoding="utf-8"),
    "exe_sha256": sha(build / "HimoHito2p5DProof.exe"),
    "qa_files": [p.relative_to(ROOT).as_posix() for p in sorted(QA.rglob("*")) if p.is_file()],
    "added_proof_assets_only_in_isolation": [p.relative_to(PROJECT).as_posix() for p in sorted(PROJECT.joinpath("Assets/Proof").rglob("*")) if p.is_file()]
    + [p.relative_to(PROJECT).as_posix() for p in sorted(PROJECT.joinpath("Assets/Resources/Proof").rglob("*")) if p.is_file()],
    "launchers": ["output/Tutorial2p5DProof_20261001_Proof01/START_PROOF.bat", "output/Tutorial2p5DProof_20261001_Proof01/START_BASELINE.bat"],
    "original_doc_art_unchanged": all(sha(ROOT / item["source"]) == item["sha256"] for item in json.loads((QA / "Preparation.json").read_text(encoding="utf-8"))["materials"]),
}
(QA / "ChangeManifest.json").write_text(json.dumps(result, ensure_ascii=False, indent=2), encoding="utf-8")
print(json.dumps({k: v for k, v in result.items() if k not in ("qa_files", "added_proof_assets_only_in_isolation")}, ensure_ascii=False, indent=2))
