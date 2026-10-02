"""NEW evidence only: HEAD/origin content distinctions for all audited Repo copies."""
import csv
import datetime
import json
import os
import subprocess
from pathlib import Path

E = Path(__file__).resolve().parent
env = dict(os.environ, GIT_OPTIONAL_LOCKS="0", PYTHONDONTWRITEBYTECODE="1")
previous = json.loads((E / "GitRepositories.json").read_text(encoding="utf-8"))
production = json.loads((E / "AdditionalSafety_RepositoryStates.json").read_text(encoding="utf-8"))
production_roots = {r["path"] for r in production}
results = list(production)


def git(root, *args):
    p = subprocess.run(["git", "-c", "safe.directory=" + root.as_posix(), *args], cwd=root, env=env, capture_output=True)
    if p.returncode:
        raise RuntimeError("read-only Git query failed")
    return p.stdout.decode("utf-8", errors="replace").rstrip("\r\n")


def names(root, *args):
    return {x for x in git(root, *args).split("\0") if x}


def tree(root, ref):
    return {entry.split("\t", 1)[1]: entry.split("\t", 1)[0].split()[2]
            for entry in git(root, "ls-tree", "-r", "-z", ref).split("\0") if "\t" in entry}


def present(p):
    try:
        os.stat("\\\\?\\" + str(p.absolute()) if os.name == "nt" else p)
        return True
    except FileNotFoundError:
        return False


for record in previous:
    root = Path(record["path"])
    if root.as_posix() in production_roots:
        continue
    head_sha = git(root, "rev-parse", "HEAD")
    origin_sha = git(root, "rev-parse", "origin/main")
    verified = origin_sha == "e6dc6acf0227382cc94398e6c1d36f716ff3bbb0"
    head, origin = tree(root, "HEAD"), tree(root, "origin/main")
    tracked = names(root, "ls-files", "-z")
    untracked = {x[3:] for x in names(root, "status", "--porcelain=v1", "--untracked-files=all", "-z") if x.startswith("?? ")}
    changed_head = names(root, "diff", "--no-ext-diff", "--no-textconv", "--name-only", "-z", "HEAD")
    changed_origin = names(root, "diff", "--no-ext-diff", "--no-textconv", "--name-only", "-z", "origin/main")
    label = "DesertLoop copy / " + root.parent.name + "/" + root.name
    files = []
    for rel in sorted(set(head) | set(origin) | tracked | untracked):
        exists = present(root / rel)
        state = "missing" if not exists else "untracked" if rel in untracked else "tracked_modified" if rel in tracked and (rel in changed_head or rel not in head) else "tracked_clean" if rel in tracked else "not_in_local_index"
        head_match = "not_present" if not exists else "not_in_HEAD" if rel not in head else "unverified_untracked_or_ignored" if rel not in tracked else "different" if rel in changed_head else "same_under_git_comparison"
        origin_match = "not_present" if not exists else "not_in_origin_main" if rel not in origin else "unverified_untracked_or_ignored" if rel not in tracked else "different" if rel in changed_origin else "same_under_git_comparison"
        files.append({"repo": label, "absolute_path": (root / rel).as_posix(), "relative_path": rel,
                      "local_index_tracked": rel in tracked, "working_tree_state": state,
                      "HEAD_contains_path": rel in head, "origin_main_contains_path": rel in origin,
                      "HEAD_blob": head.get(rel), "origin_main_blob": origin.get(rel),
                      "working_tree_vs_HEAD": head_match, "working_tree_vs_origin_main": origin_match,
                      "GitHub_main_has_current_tracked_content": verified and origin_match == "same_under_git_comparison",
                      "clean_HEAD_content_saved_in_remote_history": verified and record["HEAD_is_ancestor_of_origin_main"] and head_match == "same_under_git_comparison",
                      "deletion_note": "Saved subset does not authorize deleting Source/.git/raw or entire parent"})
    results.append({"repo": label, "path": root.as_posix(), "identity": {"HEAD": head_sha, "origin_main": origin_sha},
                    "origin_main_matches_live_GitHub_main": verified, "files": files})

with (E / "AdditionalSafety_AllRepoFileStates.csv").open("x", encoding="utf-8-sig", newline="") as f:
    writer = csv.DictWriter(f, fieldnames=list(production[0]["files"][0]))
    writer.writeheader()
    for r in results:
        writer.writerows(r["files"])

folder_rows = list(csv.DictReader((E / "FolderInventory.csv").open(encoding="utf-8-sig")))
top_rows = list(csv.DictReader((E / "Top30.csv").open(encoding="utf-8-sig")))
folders = {row["path"]: row for row in folder_rows + top_rows}
out = []
for key, old in folders.items():
    p = Path(key)
    containing = sorted((r for r in results if Path(r["path"]) == p or Path(r["path"]) in p.parents), key=lambda r: -len(r["path"]))
    row = {"path": p.as_posix(), "alias": old["alias"], "classification": old["classification"]}
    if not containing:
        row.update({"containing_repo": "None (may contain nested repos)", "tracked_count": "N/A", "untracked_count": "N/A", "modified_tracked_count": "N/A",
                    "HEAD_path_count": "N/A", "origin_main_path_count": "N/A", "current_content_matches_origin_count": "N/A",
                    "HEAD": "N/A", "origin_main": "N/A", "GitHub_current_folder_saved": "Not asserted; parent can contain nested repositories/local-only work"})
    else:
        r = containing[0]
        matching = [file for file in r["files"] if Path(file["absolute_path"]) == p or p in Path(file["absolute_path"]).parents]
        counts = {"tracked_count": sum(f["local_index_tracked"] for f in matching),
                  "untracked_count": sum(f["working_tree_state"] == "untracked" for f in matching),
                  "modified_tracked_count": sum(f["working_tree_state"] == "tracked_modified" for f in matching),
                  "HEAD_path_count": sum(f["HEAD_contains_path"] for f in matching),
                  "origin_main_path_count": sum(f["origin_main_contains_path"] for f in matching),
                  "current_content_matches_origin_count": sum(f["GitHub_main_has_current_tracked_content"] for f in matching)}
        row.update(counts)
        row.update({"containing_repo": r["path"], "HEAD": r["identity"]["HEAD"], "origin_main": r["identity"]["origin_main"],
                    "GitHub_current_folder_saved": "Only matching tracked subset verified; untracked/modified/ignored files NOT saved by path inclusion"})
    out.append(row)
with (E / "AdditionalSafety_FolderGitStates.csv").open("x", encoding="utf-8-sig", newline="") as f:
    writer = csv.DictWriter(f, fieldnames=list(out[0]))
    writer.writeheader()
    writer.writerows(out)
with (E / "AdditionalSafety_GitStateSummary.json").open("x", encoding="utf-8") as f:
    json.dump({"recorded_jst": datetime.datetime.now(datetime.timezone(datetime.timedelta(hours=9))).isoformat(timespec="seconds"),
               "repository_count": len(results), "folder_count": len(out), "file_state_rows": sum(len(r["files"]) for r in results),
               "all_origin_main_refs_match_verified_GitHub": all(r["origin_main_matches_live_GitHub_main"] for r in results),
               "no_production_files_written": True, "all_outputs_new": True}, f, ensure_ascii=False, indent=2)
print("Repo count", len(results), "Folder states", len(out), "file rows", sum(len(r["files"]) for r in results))
