"""Read-only addendum audit. Every output is NEW inside the requested audit.

Never stage/fetch/checkout/delete/move/compress; do not overwrite earlier reports.
"""
import csv
import datetime
import hashlib
import json
import os
import subprocess
import sys
from pathlib import Path

E = Path(__file__).resolve().parent
H = E.parents[3]
D = Path("C:/Users/田尻大翔/Documents/UnityProjects/DesertLoop")
GIB = 1024 ** 3
JST = datetime.timezone(datetime.timedelta(hours=9))
env = dict(os.environ, GIT_OPTIONAL_LOCKS="0")
github_main = {"HimoHito": "702a4bc044b0eb35bedb552753ca8495599bc1ee", "DesertLoop": "e6dc6acf0227382cc94398e6c1d36f716ff3bbb0"}


def now():
    return datetime.datetime.now(JST).isoformat(timespec="seconds")


def write_new(name, data):
    with (E / name).open("x", encoding="utf-8") as f:
        json.dump(data, f, ensure_ascii=False, indent=2)


def git(root, *args, allow_failure=False):
    p = subprocess.run(["git", "-c", "safe.directory=" + root.as_posix(), *args], cwd=root, env=env, capture_output=True)
    if p.returncode and not allow_failure:
        raise RuntimeError("read-only git query failed: " + str(args))
    return p.returncode, p.stdout.decode("utf-8", errors="replace").rstrip("\r\n")


def tree(root, ref):
    _, output = git(root, "ls-tree", "-r", "-z", "-l", ref)
    result = {}
    for entry in output.split("\0"):
        if "\t" not in entry:
            continue
        meta, name = entry.split("\t", 1)
        mode, kind, oid, size = meta.split()
        result[name] = {"oid": oid, "size": int(size) if size.isdigit() else None}
    return result


def names(root, *args):
    return {s for s in git(root, *args)[1].split("\0") if s}


def existing(path):
    try:
        os.stat("\\\\?\\" + str(path.absolute()) if os.name == "nt" else path)
        return True
    except FileNotFoundError:
        return False


def sha(path):
    with path.open("rb") as f:
        return hashlib.file_digest(f, "sha256").hexdigest().upper()


def protection(root):
    protected = set()
    for name in ("Assets", "ProjectSettings", "Packages"):
        protected.update(p for p in (root / name).rglob("*") if p.is_file())
    for name in (".git/HEAD", ".git/index", ".git/config", ".git/refs/remotes/origin/main", "README.md", "Docs/LEARNING_LOG.md", "Docs/Progress/CURRENT.md"):
        p = root / name
        if p.is_file():
            protected.add(p)
    for name in names(root, "diff", "--no-ext-diff", "--no-textconv", "--name-only", "-z", "HEAD"):
        p = root / name
        if p.is_file():
            protected.add(p)
    status = sorted(s for s in git(root, "status", "--porcelain=v1", "--untracked-files=all", "-z")[1].split("\0") if s)
    if root == H:
        status = [s for s in status if not s[3:].startswith("Docs/Maintenance/DiskUsageAudit_20261002/")]
    return {"branch": git(root, "branch", "--show-current")[1], "HEAD": git(root, "rev-parse", "HEAD")[1],
            "origin_main": git(root, "rev-parse", "origin/main")[1], "staged": git(root, "diff", "--cached", "--name-only")[1],
            "status_without_new_audit": status, "sha256": {p.relative_to(root).as_posix(): sha(p) for p in sorted(protected)}}


def repo_detail(label, root):
    head = tree(root, "HEAD")
    origin = tree(root, "origin/main")
    tracked = names(root, "ls-files", "-z")
    status = [s for s in git(root, "status", "--porcelain=v1", "--untracked-files=all", "-z")[1].split("\0") if s]
    untracked = {s[3:] for s in status if s.startswith("?? ")}
    vs_head = names(root, "diff", "--no-ext-diff", "--no-textconv", "--name-only", "-z", "HEAD")
    vs_origin = names(root, "diff", "--no-ext-diff", "--no-textconv", "--name-only", "-z", "origin/main")
    identity = protection(root)
    verified = identity["origin_main"] == github_main[label]
    ancestor = git(root, "merge-base", "--is-ancestor", "HEAD", "origin/main", allow_failure=True)[0] == 0
    selected = set(head) | set(origin) | tracked | untracked
    ignored = set()
    if root == H:
        ignored.add("Assets/Resources/Audio/YasashiiOdori.mp3")
        assert git(root, "check-ignore", "Assets/Resources/Audio/YasashiiOdori.mp3", allow_failure=True)[0] == 0
        selected |= ignored
    files = []
    for name in sorted(selected):
        if root == H and name.startswith("Docs/Maintenance/DiskUsageAudit_20261002/"):
            continue
        present = existing(root / name)
        state = "missing" if not present else "ignored" if name in ignored else "untracked" if name in untracked else "tracked_modified" if name in tracked and (name in vs_head or name not in head) else "tracked_clean" if name in tracked else "not_in_local_index"
        head_match = "not_present" if not present else "not_in_HEAD" if name not in head else "unverified_untracked_or_ignored" if name not in tracked else "different" if name in vs_head else "same_under_git_comparison"
        origin_match = "not_present" if not present else "not_in_origin_main" if name not in origin else "unverified_untracked_or_ignored" if name not in tracked else "different" if name in vs_origin else "same_under_git_comparison"
        saved_current = verified and origin_match == "same_under_git_comparison"
        historical = verified and ancestor and head_match == "same_under_git_comparison"
        files.append({"repo": label, "absolute_path": (root / name).as_posix(), "relative_path": name,
                      "local_index_tracked": name in tracked, "working_tree_state": state,
                      "HEAD_contains_path": name in head, "origin_main_contains_path": name in origin,
                      "HEAD_blob": head.get(name, {}).get("oid"), "origin_main_blob": origin.get(name, {}).get("oid"),
                      "working_tree_vs_HEAD": head_match, "working_tree_vs_origin_main": origin_match,
                      "GitHub_main_has_current_tracked_content": saved_current,
                      "clean_HEAD_content_saved_in_remote_history": historical,
                      "deletion_note": "KEEP current unsaved/unverified content" if not saved_current else "Saved does not authorize deletion; Production/archive/latest work remain KEEP"})
    return {"repo": label, "path": root.as_posix(), "identity": identity, "origin_main_matches_live_GitHub_main": verified,
            "GitHub_main_verified_jst": "2026-10-02 additional safety audit (read-only ls-remote)",
            "HEAD_ancestor_of_origin_main": ancestor, "index_tracked_count": len(tracked), "HEAD_tree_file_count": len(head),
            "origin_main_tree_file_count": len(origin), "status_untracked_count": len(untracked),
            "notes": "Path inclusion is separate from current content. Comparison uses Git normalization; untracked content at a remote path remains unverified and KEEP.",
            "files": files}


phase = sys.argv[1]
if phase == "start":
    before = {"measured_jst": now(), "HimoHito": protection(H), "DesertLoop": protection(D)}
    write_new("AdditionalSafety_ProtectionStart.json", before)
    # Preserve earlier audit outputs too: no existing report/Evidence edits.
    script_name = Path(__file__).name
    earlier = {p.relative_to(E.parent).as_posix(): sha(p) for p in E.parent.rglob("*")
               if p.is_file() and p.name != script_name and not p.name.startswith("AdditionalSafety")}
    write_new("AdditionalSafety_PreexistingAuditHashes.json", earlier)
    results = [repo_detail(label, root) for label, root in (("HimoHito", H), ("DesertLoop", D))]
    write_new("AdditionalSafety_RepositoryStates.json", results)
    all_files = [row for r in results for row in r["files"]]
    with (E / "AdditionalSafety_FileGitStates.csv").open("x", encoding="utf-8-sig", newline="") as f:
        writer = csv.DictWriter(f, fieldnames=list(all_files[0]))
        writer.writeheader()
        writer.writerows(all_files)
    important = [r for r in all_files if r["repo"] == "HimoHito" and (r["relative_path"] in {
        "Assets/Scenes/MainStage.unity", "Assets/Scripts/HimoHitoCraftRoomBackground.cs", "Assets/Scripts/TutorialCraftRoomLayers.cs",
        "Assets/Resources/TutorialBackgroundEdge.shader", "Assets/Resources/TutorialBackgroundFloor.shader",
        "Assets/Resources/Audio/YasashiiOdori.mp3", "README.md", "Docs/LEARNING_LOG.md"} or r["relative_path"].startswith("Assets/Resources/Art/Tutorial2p5D/"))]
    write_new("AdditionalSafety_Protected2p5DGitStates.json", important)
    print(json.dumps([{k: v for k, v in r.items() if k not in {"files", "identity"}} for r in results], ensure_ascii=False, indent=2))
    print("2.5D modified/untracked content saved-to-main false:", all(not r["GitHub_main_has_current_tracked_content"] for r in important if r["working_tree_state"] in {"tracked_modified", "untracked", "ignored"}))
elif phase == "end":
    before = json.loads((E / "AdditionalSafety_ProtectionStart.json").read_text(encoding="utf-8"))
    after = {"measured_jst": now(), "HimoHito": protection(H), "DesertLoop": protection(D)}
    write_new("AdditionalSafety_ProtectionEnd.json", after)
    hashes = json.loads((E / "AdditionalSafety_PreexistingAuditHashes.json").read_text(encoding="utf-8"))
    previous_reports_unchanged = all((E.parent / p).is_file() and sha(E.parent / p) == value for p, value in hashes.items())
    low = list(csv.DictReader((E / "LowRiskCandidates.csv").open(encoding="utf-8-sig")))
    medium = list(csv.DictReader((E / "MediumRiskCandidates.csv").open(encoding="utf-8-sig")))
    selected = [Path(r["path"]) for r in low + medium]
    disjoint = all(not (p == q or p in q.parents or q in p.parents) for i, p in enumerate(selected) for q in selected[i + 1:])
    total = sum(int(r["bytes"]) for r in low + medium)
    prior_summary = json.loads((E / "CandidateSummary.json").read_text(encoding="utf-8"))
    result = {"Himo_Production_Git_unchanged": before["HimoHito"] == after["HimoHito"],
              "Desert_Production_Git_unchanged": before["DesertLoop"] == after["DesertLoop"],
              "all_preexisting_audit_files_unchanged": previous_reports_unchanged,
              "existing_audit_file_count_verified": len(hashes), "candidate_path_count": len(selected),
              "candidate_paths_pairwise_disjoint": disjoint, "candidate_bytes_recalculated": total,
              "candidate_gib_recalculated": total / GIB,
              "candidate_total_matches_existing_report": total == prior_summary["low_risk_candidate_bytes"] + prior_summary["medium_risk_candidate_bytes"],
              "any_new_2p5d_candidate": any("Tutorial2p5D" in p.as_posix() for p in selected),
              "writes_outside_new_audit_files": "None performed", "cleanup_performed": False,
              "notes": "Only NEW addendum files created; existing audit files protected by SHA. Unrelated active processes may write their own files."}
    write_new("AdditionalSafety_Verification.json", result)
    print(json.dumps(result, ensure_ascii=False, indent=2))
else:
    raise ValueError("phase must be start or end")
