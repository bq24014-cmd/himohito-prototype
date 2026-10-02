"""Read-only project/content preservation snapshots. Writes only audit Evidence."""
import hashlib
import json
import os
import subprocess
import sys
from pathlib import Path

E = Path(__file__).resolve().parent
H = E.parents[3]
D = Path("C:/Users/田尻大翔/Documents/UnityProjects/DesertLoop")
env = dict(os.environ, GIT_OPTIONAL_LOCKS="0")


def git(root, *args):
    p = subprocess.run(["git", "-c", "safe.directory=" + root.as_posix(), *args], cwd=root, env=env, capture_output=True)
    if p.returncode:
        raise RuntimeError("read-only git query failed")
    return p.stdout.decode("utf-8", errors="replace").rstrip("\r\n")


def sha(path):
    with path.open("rb") as f:
        return hashlib.file_digest(f, "sha256").hexdigest().upper()


def metadata(root):
    rows = {}
    if root.exists():
        for base, dirs, files in os.walk(root, followlinks=False):
            dirs[:] = [d for d in dirs if not Path(base, d).is_symlink()]
            for name in files:
                p = Path(base, name)
                if p.is_symlink():
                    continue
                stat_path = "\\\\?\\" + str(p.absolute()) if os.name == "nt" else p
                s = os.stat(stat_path)
                rows[p.relative_to(root).as_posix()] = [s.st_size, s.st_mtime_ns]
    return rows


def snapshot(root):
    protected_files = set()
    for name in ("Assets", "ProjectSettings", "Packages"):
        protected_files.update(p for p in (root / name).rglob("*") if p.is_file())
    for name in (".git/HEAD", ".git/index", ".git/config", ".git/refs/remotes/origin/main", "README.md", "Docs/LEARNING_LOG.md", "Docs/Progress/CURRENT.md"):
        p = root / name
        if p.is_file():
            protected_files.add(p)
    for name in git(root, "diff", "--name-only", "HEAD").splitlines():
        p = root / name
        if p.is_file():
            protected_files.add(p)
    status = [s for s in git(root, "status", "--porcelain=v1", "--untracked-files=all", "-z").split("\0") if s]
    if root == H:
        status = [s for s in status if not s[3:].startswith("Docs/Maintenance/DiskUsageAudit_20261002/")]
    return {"branch": git(root, "branch", "--show-current"), "HEAD": git(root, "rev-parse", "HEAD"),
            "origin_main": git(root, "rev-parse", "origin/main"), "staged": git(root, "diff", "--cached", "--name-only"),
            "status_without_new_audit": status,
            "sha256": {p.relative_to(root).as_posix(): sha(p) for p in sorted(protected_files)}}


phase = sys.argv[1]
result = {"HimoHito": snapshot(H), "DesertLoop": snapshot(D)}
# No shared .codex runtime/session contents. Preserve local Build and QA via
# file path/size/mtime lists (not merely the parent directory timestamp).
result["Himo_artifact_metadata"] = {}
for name in ("output", "Builds", ".codex_tmp", "Docs/QA", "Docs/VisualConcepts", "Docs/Archive"):
    result["Himo_artifact_metadata"][name] = metadata(H / name)
(E / ("Protection" + phase.capitalize() + ".json")).write_text(json.dumps(result, ensure_ascii=False, indent=2), encoding="utf-8")
if phase == "end":
    start = json.loads((E / "ProtectionStart.json").read_text(encoding="utf-8"))
    early = json.loads((E / "HimoProtectionStart.json").read_text(encoding="utf-8"))
    comparison = {"Himo_source_settings_git_identity_unchanged": result["HimoHito"] == start["HimoHito"],
                  "Desert_source_settings_git_identity_unchanged": result["DesertLoop"] == start["DesertLoop"],
                  "Himo_Build_QA_temp_artifact_metadata_unchanged": result["Himo_artifact_metadata"] == start["Himo_artifact_metadata"],
                  "early_Himo_key_sha_all_match": all(result["HimoHito"]["sha256"].get(k) == v for k, v in early["sha256"].items()),
                  "Himo_source_files_hashed": len(result["HimoHito"]["sha256"]),
                  "Desert_source_files_hashed": len(result["DesertLoop"]["sha256"]),
                  "Himo_early_HEAD_matches": result["HimoHito"]["HEAD"] == early["HEAD"],
                  "Himo_early_origin_main_matches": result["HimoHito"]["origin_main"] == early["origin_main"],
                  "Himo_index_empty": result["HimoHito"]["staged"] == "", "Desert_index_empty": result["DesertLoop"]["staged"] == ""}
    comparison["all_protection_checks_pass"] = all(v for k, v in comparison.items() if not k.endswith("_hashed"))
    (E / "ProtectionComparison.json").write_text(json.dumps(comparison, ensure_ascii=False, indent=2), encoding="utf-8")
    print(json.dumps(comparison, ensure_ascii=False, indent=2))
else:
    print("Read-only preservation snapshot created.", len(result["HimoHito"]["sha256"]), len(result["DesertLoop"]["sha256"]))
