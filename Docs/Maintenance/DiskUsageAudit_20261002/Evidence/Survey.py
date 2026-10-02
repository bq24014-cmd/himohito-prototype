"""Read-only metadata survey. No delete/move/compress/git mutation operations."""
import datetime
import hashlib
import json
import os
import subprocess
from pathlib import Path

PROJECT = Path(__file__).resolve().parents[4]
EVIDENCE = Path(__file__).resolve().parent
ROOTS = [Path("C:/Users/田尻大翔/Documents/Codex"), Path("C:/Users/田尻大翔/.codex")]
GIB = 1024 ** 3
JST = datetime.timezone(datetime.timedelta(hours=9))
rows, errors, links, repos, projects, large = [], [], [], [], [], []


def stamp(t):
    return datetime.datetime.fromtimestamp(t, JST).isoformat(timespec="seconds") if t else None


def walk(path, root, depth=0):
    size = count = 0
    latest = 0
    try:
        own = path.stat(follow_symlinks=False)
        latest = own.st_mtime
        entries = list(os.scandir(path))
    except OSError as ex:
        errors.append({"path": str(path), "error": str(ex)})
        return size, count, latest
    names = {e.name for e in entries}
    if ".git" in names:
        repos.append(str(path))
    if {"Assets", "ProjectSettings"}.issubset(names):
        projects.append(str(path))
    direct_size = direct_count = 0
    for entry in entries:
        name = Path(entry.path)
        try:
            info = entry.stat(follow_symlinks=False)
            # No traversal through junctions/symlinks; avoid counting outside scope twice.
            if entry.is_symlink() or (getattr(info, "st_file_attributes", 0) & 0x400):
                links.append({"path": str(name), "kind": "reparse/symlink", "size": info.st_size})
                continue
            if entry.is_dir(follow_symlinks=False):
                subsize, subcount, sublatest = walk(name, root, depth + 1)
                size += subsize; count += subcount; latest = max(latest, sublatest)
            elif entry.is_file(follow_symlinks=False):
                size += info.st_size; count += 1
                direct_size += info.st_size; direct_count += 1
                latest = max(latest, info.st_mtime)
                if info.st_size >= 100 * 1024 ** 2:
                    large.append({"path": str(name), "bytes": info.st_size, "modified_jst": stamp(info.st_mtime)})
        except OSError as ex:
            errors.append({"path": str(name), "error": str(ex)})
    rows.append({"path": str(path), "root": str(root), "depth": depth, "bytes": size,
                 "gib": round(size / GIB, 6), "file_count": count, "last_modified_jst": stamp(latest),
                 "directory_modified_jst": stamp(own.st_mtime), "direct_bytes": direct_size, "direct_file_count": direct_count})
    return size, count, latest


def git(*args):
    env = dict(os.environ, GIT_OPTIONAL_LOCKS="0")
    return subprocess.check_output(["git", "-c", "safe.directory=" + PROJECT.as_posix(), *args], cwd=PROJECT, env=env).decode("utf-8").strip()


def sha(path):
    with path.open("rb") as f:
        return hashlib.file_digest(f, "sha256").hexdigest().upper()


if __name__ == "__main__":
    started = datetime.datetime.now(JST).isoformat(timespec="seconds")
    protection = {"branch": git("branch", "--show-current"), "HEAD": git("rev-parse", "HEAD"),
                  "origin_main": git("rev-parse", "origin/main"), "status": git("status", "--short"),
                  "staged": git("diff", "--cached", "--name-only"), "sha256": {}}
    for relative in ("Assets/Scenes/MainStage.unity", "Assets/Scenes/Tutorial.unity",
                     "Assets/Scripts/TutorialCraftRoomLayers.cs", "Assets/Scripts/HimoHitoCraftRoomBackground.cs",
                     "Assets/Resources/Audio/YasashiiOdori.mp3", "Assets/Resources/TutorialBackgroundEdge.shader",
                     "Assets/Resources/TutorialBackgroundFloor.shader"):
        protection["sha256"][relative] = sha(PROJECT / relative)
    for path in (PROJECT / "Assets/Resources/Art/Tutorial2p5D").rglob("*"):
        if path.is_file():
            protection["sha256"][path.relative_to(PROJECT).as_posix()] = sha(path)
    (EVIDENCE / "HimoProtectionStart.json").write_text(json.dumps(protection, ensure_ascii=False, indent=2), encoding="utf-8")
    totals = []
    for root in ROOTS:
        print("Survey started:", root, flush=True)
        size, count, latest = walk(root, root)
        totals.append({"path": str(root), "bytes": size, "gib": size / GIB, "file_count": count, "last_modified_jst": stamp(latest)})
        print("Survey complete:", root, "GiB", round(size / GIB, 3), "files", count, flush=True)
    result = {"started_jst": started, "finished_jst": datetime.datetime.now(JST).isoformat(timespec="seconds"),
              "unit": "GiB = 1024^3 bytes, logical file lengths; not physical allocated/freeable disk blocks",
              "root_totals": totals, "total_bytes": sum(t["bytes"] for t in totals),
              "directories": sorted(rows, key=lambda r: -r["bytes"]), "repositories": repos,
              "unity_projects": projects, "large_files": sorted(large, key=lambda r: -r["bytes"]),
              "errors": errors, "skipped_reparse_points": links,
              "limitations": "Metadata-only file-size scan; ancestors overlap. Reparse points excluded. Concurrent active files may change. No credential/session contents read."}
    (EVIDENCE / "DirectorySurvey.json").write_text(json.dumps(result, ensure_ascii=False, indent=2), encoding="utf-8")
    print("Total GiB", round(result["total_bytes"] / GIB, 3), "directories", len(rows), "errors", len(errors), "links", len(links), flush=True)
    print("Repositories", json.dumps(repos, ensure_ascii=False), flush=True)
    print("Unity project roots", json.dumps(projects, ensure_ascii=False), flush=True)
