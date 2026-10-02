"""Read-only repository metadata. Optional locks disabled, never fetch/stage."""
import json
import os
import subprocess
from pathlib import Path
from urllib.parse import urlsplit, urlunsplit

HERE = Path(__file__).resolve().parent
survey = json.loads((HERE / "DirectorySurveyWithDesert.json").read_text(encoding="utf-8"))
env = dict(os.environ, GIT_OPTIONAL_LOCKS="0")


def git(repo, *args):
    result = subprocess.run(["git", "-c", "safe.directory=" + Path(repo).as_posix(), *args],
                            cwd=repo, env=env, capture_output=True)
    return result.returncode, result.stdout.decode("utf-8", errors="replace").rstrip("\r\n")


results = []
for repo in survey["repositories"]:
    info = {"path": repo}
    for key, args in (("branch", ("branch", "--show-current")), ("HEAD", ("rev-parse", "HEAD")),
                      ("origin_main", ("rev-parse", "origin/main")), ("status", ("status", "--short")),
                      ("staged", ("diff", "--cached", "--name-only")), ("remote", ("config", "--get", "remote.origin.url"))):
        code, value = git(repo, *args)
        if key == "remote" and "://" in value:
            parsed = urlsplit(value)
            value = urlunsplit((parsed.scheme, parsed.hostname or "", parsed.path, "", ""))
        info[key] = value if code == 0 else None
    code, tracked = git(repo, "ls-files", "-z")
    info["tracked_files"] = tracked.split("\0") if code == 0 and tracked else []
    code, dirty = git(repo, "diff", "--name-only", "HEAD")
    info["modified_tracked_files"] = dirty.splitlines() if code == 0 and dirty else []
    code, porcelain = git(repo, "status", "--porcelain=v1", "--untracked-files=all", "-z")
    info["status_all"] = [entry for entry in porcelain.split("\0") if entry] if code == 0 and porcelain else []
    code, _ = git(repo, "merge-base", "--is-ancestor", "HEAD", "origin/main")
    info["HEAD_is_ancestor_of_origin_main"] = code == 0
    code, tree = git(repo, "ls-tree", "-r", "-z", "-l", "origin/main")
    info["origin_main_tree"] = {}
    if code == 0:
        for entry in tree.split("\0"):
            if "\t" not in entry: continue
            metadata, name = entry.split("\t", 1)
            mode, kind, oid, size = metadata.split()
            info["origin_main_tree"][name] = {"oid": oid, "size": int(size) if size.isdigit() else None}
    results.append(info)
    print(json.dumps({k: v for k, v in info.items() if k in {"path", "branch", "HEAD", "origin_main", "staged", "HEAD_is_ancestor_of_origin_main"}} | {"status_all_count": len(info["status_all"]), "modified_tracked_count": len(info["modified_tracked_files"]), "tracked_count": len(info["tracked_files"])}, ensure_ascii=False), flush=True)
(HERE / "GitRepositories.json").write_text(json.dumps(results, ensure_ascii=False, indent=2), encoding="utf-8")
