"""Generate audit metadata/tables ONLY in this new audit Evidence directory.

No cleanup, deletion, move, compression, Git write, or project mutation.
Candidate sizes are logical lengths and never assert actual freeable blocks.
"""
import csv
import datetime
import hashlib
import json
from collections import defaultdict
from pathlib import Path

E = Path(__file__).resolve().parent
H = E.parents[3]
D = Path("C:/Users/田尻大翔/Documents/UnityProjects/DesertLoop")
V = Path("C:/Users/田尻大翔/.codex/visualizations/2026/09/12/01a095fe-94f6-7783-a93d-497f16bab356")
VH = Path("C:/Users/田尻大翔/.codex/visualizations/2026/08/08/019fdfa3-5ee1-7911-8a24-63282af5bae4")
C = Path("C:/Users/田尻大翔/.codex")
GIB = 1024 ** 3
JST = datetime.timezone(datetime.timedelta(hours=9))
S = json.loads((E / "DirectorySurveyWithDesert.json").read_text(encoding="utf-8"))
G = json.loads((E / "GitRepositories.json").read_text(encoding="utf-8"))
ROWS = {Path(r["path"]): r for r in S["directories"]}
PROJECTS = {Path(p) for p in S["unity_projects"]}
REMOTE = json.loads((E / "RemoteRefs.json").read_text(encoding="utf-8"))


def inside(path, root):
    return path == root or root in path.parents


def protected(path):
    return inside(path, H) and any(part.startswith("Tutorial2p5D") for part in path.parts)


def alias(p):
    p = Path(p)
    for key, root in (("H", H), ("D", D), ("V", V), ("VH", VH), ("C", C)):
        if inside(p, root):
            rel = p.relative_to(root).as_posix()
            return key if rel == "." else key + "/" + rel
    root = Path("C:/Users/田尻大翔/Documents/Codex")
    if inside(p, root):
        rel = p.relative_to(root).as_posix()
        return "DC" if rel == "." else "DC/" + rel
    return p.as_posix()


def sha(path):
    with path.open("rb") as f:
        return hashlib.file_digest(f, "sha256").hexdigest().upper()


def git_info(path):
    nearest = sorted((r for r in G if inside(path, Path(r["path"]))), key=lambda r: len(r["path"]), reverse=True)
    if not nearest:
        return {"git_managed": "No containing repository detected; nested repositories possible", "github_saved": "Unverified", "tracked_files": 0, "origin_tree_files": 0}
    r = nearest[0]
    prefix = path.relative_to(Path(r["path"])).as_posix()
    prefix = "" if prefix == "." else prefix + "/"
    tracked = [f for f in r["tracked_files"] if f.startswith(prefix)]
    origin = [f for f in r["origin_main_tree"] if f.startswith(prefix)]
    changed = [f for f in r["modified_tracked_files"] if f.startswith(prefix)]
    dirty = [f for f in r["status_all"] if f[3:].startswith(prefix)]
    saved = "Partial: tracked subset present in verified origin/main; local/ignored data not saved" if origin else "No files under this path in origin/main"
    if origin and not dirty and not changed and path.is_dir() and ROWS.get(path, {}).get("file_count", 0) == len(origin):
        saved = "All files at path present in origin/main and working tree clean"
    return {"git_managed": "Mixed; tracked + untracked/ignored possible" if tracked else "No tracked files at this path", "github_saved": saved,
            "tracked_files": len(tracked), "origin_tree_files": len(origin), "modified_tracked": len(changed), "local_status_entries": len(dirty)}


def describe(path):
    if protected(path):
        return "最新Tutorial 2.5D作業・関連QA/画像/検証Build", "D. DO NOT DELETE YET", "部分的に再生成可能でも今回の保護指定が優先", "Yes: 未commitの作業を含む", "HIGH"
    if path in {H / ".codex_tmp", H / "output", H / "Builds"}:
        return "旧検証と最新2.5D保護対象が混在する親フォルダ", "D. DO NOT DELETE YET", "旧Library/指定旧Buildのみ候補。親の一括削除禁止", "Yes: 最新未commit作業を含む", "HIGH"
    if path.name == ".git":
        return "Repository履歴/参照/Index", "A. KEEP", "このローカル履歴の完全復元は未確認", "ローカル履歴を含む可能性", "HIGH"
    if inside(path, C / "sessions"):
        return "Codex会話/実行履歴（内容未読）", "A. KEEP", "不可/未確認。単なるUnity logではない", "Yes/未確認", "HIGH"
    if inside(path, H / "Assets") or inside(path, D / "Assets") or inside(path, H / "ProjectSettings") or inside(path, D / "ProjectSettings"):
        return "Production Source/Scene/Asset/設定", "A. KEEP", "未保存変更は不可", "Yes: Production未保存を保護", "HIGH"
    if path == H / "Library" or path == H / "Temp" or path == H / "Logs":
        return "起動中HimoHitoのUnity作業キャッシュ/ログ", "D. DO NOT DELETE YET", "一部可だが使用中", "使用中。消去不可", "HIGH (使用中)"
    if path.parent in PROJECTS and path.name in {"Library", "Temp", "obj"}:
        return "Unity生成キャッシュ (所属Projectは親Path)", "B. SAFE TO DELETE AFTER CONFIRMATION", "可: 同ProjectのSource/Asset/Packages/外部依存を保持", "キャッシュのみを対象。親Sourceは残す", "LOW (全Unity終了・対象確認後)"
    if path == VH or inside(path, VH):
        return "HimoHito過去PowerPoint/動画レビュー/画像", "D. DO NOT DELETE YET", "元入力/採用成果物の保存先が未照合", "Yes/未確認; Git repoなし", "HIGH"
    if path == V / "FREEZE" or "FreezeCheckpoint" in path.parts:
        return "DESERT LOOP Freeze記録/保存Receipt", "A. KEEP", "制作時点の記録", "一部Git保存済み・原記録は維持", "HIGH"
    if path == H / "Docs" / "Archive" or inside(path, H / "Docs" / "Archive"):
        return "提出時Scene必須Archive", "A. KEEP", "Git保存済みでも保存目的のArchive", "Git保存済み。消去対象外", "HIGH"
    if path == H / "Docs" / "VisualConcepts" or inside(path, H / "Docs" / "VisualConcepts"):
        return "2.5D等の画像原本/比較試作", "A. KEEP", "画像原本は不可/未確認", "Yes: 未追跡素材", "HIGH"
    if path in {H / "Builds" / "Submission", H / "Builds" / "SubmissionDocs", H / "output" / "manual", H / "output" / "pdf"}:
        return "提出済みBuildまたは資料の原本", "A. KEEP", "当時と同一内容の再生成は未保証", "Yes: Gitにない提出物", "HIGH"
    if inside(path, H / "output") or inside(path, H / "Builds"):
        if path == H / "output" / "HumanPlaytest_20260930_231714":
            return "旧初回Human Build (QA未保存)", "D. DO NOT DELETE YET", "入力/採用版の対応未確定", "Yes: QA/Build未保存", "HIGH"
        return "Windows Build/提出・共有用成果物", "C. PROBABLY SAFE", "対応する当時Source/外部BGMを確定後に可。byte同一未保証", "Yes: Build自体はGit未保存", "MEDIUM (最新版/必要なHuman版は残す)"
    if inside(path, H / ".codex_tmp"):
        if path in {H / ".codex_tmp" / "SubmissionBuild20260911", H / ".codex_tmp" / "DocsFinalCheckpoint_20261001"}:
            return "提出/Archive checkpoint用snapshot", "A. KEEP", "歴史的snapshotを保持。Libraryだけ別判定", "Yes/未確認: 当時資料/Source", "HIGH"
        if path in {H / ".codex_tmp" / name for name in ("FullGameRegression_20260930", "HumanPlaytest_20260930_231714", "doc_report_0911", "manual_0919")}:
            return "Git未保存QA/原資料/診断copy", "D. DO NOT DELETE YET", "Libraryだけ可。固有Probe/原資料は保持", "Yes: 未保存成果物", "HIGH"
        return "旧隔離QA/Build Project・診断Source/生成物の混在", "C. PROBABLY SAFE", "Libraryのみ可。コピー全体は固有Probe/入力照合が必要", "Yes/未確認: ignoredフォルダ", "MEDIUM-HIGH (全体は削除しない)"
    if inside(path, D / "Logs"):
        return "DESERT LOOP旧診断Unityコピー/Art検証/ログ混在", "D. DO NOT DELETE YET", "Libraryのみ可。Scene/Art/診断Sourceは未保存あり", "Yes/未確認: Logs全体がGit除外", "HIGH (親フォルダ一括禁止)"
    if path == D / "Library":
        return "DESERT LOOP本体Unityキャッシュ", "B. SAFE TO DELETE AFTER CONFIRMATION", "可: FROZEN Productionを保持して再import", "親の1114変更を保護。Editor使用確認必要", "LOW (全Unity終了後)"
    if inside(path, V):
        return "DESERT LOOP旧Proof/隔離Repository・raw Evidence混在", "D. DO NOT DELETE YET", "保存済みSource部分/Libraryは可、raw/旧Proofの固有入力は未確定", "Yes: 少なくとも84 raw dumpsはGit未保存", "HIGH (全体は削除しない)"
    if inside(path, H / "Docs" / "QA"):
        return "HimoHito QA記録・Evidence", "A. KEEP", "検証時点の証拠。Git未保存分あり", "Mixed: CSVでGit保存件数参照", "HIGH"
    if path == H:
        return "HimoHito Production + QA/Build/隔離コピー", "A. KEEP", "未commit Production/最新作業は不可", "Yes", "HIGH (子キャッシュのみ候補)"
    if path == D:
        return "FROZEN DESERT LOOP Production +旧検証コピー", "A. KEEP", "未commit Productionは不可", "Yes: 1114変更", "HIGH (子キャッシュのみ候補)"
    if inside(path, H) or inside(path, D):
        return "Production/資料/生成物を含む親フォルダ", "A. KEEP", "部分のみ可", "Yes/未確認", "HIGH"
    return "監査対象の親ルート/アプリ作業領域", "A. KEEP", "一括再生成不可", "Yes/未確認", "HIGH (一括削除禁止)"


def enriched(r):
    path = Path(r["path"])
    purpose, group, regen, unsaved, risk = describe(path)
    return dict(r, path=path.as_posix(), alias=alias(path), purpose=purpose, classification=group,
                regenerable=regen, contains_unsaved=unsaved, deletion_risk=risk, **git_info(path))


def csv_write(name, rows):
    if not rows:
        return
    fields = list(dict.fromkeys(k for row in rows for k in row))
    with (E / name).open("w", encoding="utf-8-sig", newline="") as f:
        writer = csv.DictWriter(f, fieldnames=fields)
        writer.writeheader()
        writer.writerows(rows)


def markdown_table(name, rows, columns):
    def esc(v):
        return str(v).replace("|", "\\|").replace("\n", " ")
    lines = ["| " + " | ".join(title for _, title in columns) + " |", "| " + " | ".join("---" for _ in columns) + " |"]
    for r in rows:
        lines.append("| " + " | ".join(esc(r.get(k, "")) for k, _ in columns) + " |")
    (E / name).write_text("\n".join(lines) + "\n", encoding="utf-8")


top = [enriched(r) for r in S["directories"][:30]]
for i, r in enumerate(top, 1):
    r["rank"] = i
    r["gib_display"] = f"{r['bytes'] / GIB:.3f}"
    r["modified"] = r["last_modified_jst"]
csv_write("Top30.csv", top)
markdown_table("Top30_Table.md", top, [("rank", "順位"), ("alias", "Path (下記の絶対Path略号)"), ("gib_display", "GiB"), ("file_count", "files"), ("modified", "Last modified JST"), ("purpose", "用途"), ("classification", "判定")])

# Candidate paths are mutually disjoint. Protect the entire current 2.5D workflow,
# including its Library directories, and the actively running HimoHito Library.
low = []
all_cache = []
for r in S["directories"]:
    path = Path(r["path"])
    if path.parent in PROJECTS and path.name in {"Library", "Temp", "obj", "Logs"}:
        entry = enriched(r)
        entry["unity_project"] = path.parent.as_posix()
        all_cache.append(entry)
        if path.name in {"Library", "Temp", "obj"} and not protected(path) and not (path.parent == H):
            # No parent directories are candidates; no Source/Scene/log evidence.
            entry["candidate_tier"] = "LOW_RISK_AFTER_ALL_UNITY_CLOSED"
            entry["candidate_group"] = "Himo old .codex_tmp" if inside(path, H) else "DesertLoop primary/diagnostic copies" if inside(path, D) else "DesertLoop visualizations"
            low.append(entry)
csv_write("UnityCaches.csv", all_cache)
csv_write("LowRiskCandidates.csv", low)
markdown_table("LowRisk_Table.md", sorted(low, key=lambda r: -r["bytes"]), [("alias", "対象Path"), ("gib", "GiB"), ("file_count", "files"), ("last_modified_jst", "Last modified JST"), ("unity_project", "所属Project"), ("classification", "分類")])

medium = []
for p in (H / "output" / "DeterministicSceneSyncAB_20261001_154205", H / "output" / "MainStageCleanReproduction_ExternalBGM_20261001_151833", H / "output" / "HumanPlaytestFixes_20261001_065300"):
    r = enriched(ROWS[p])
    r["candidate_tier"] = "MEDIUM_RISK_AFTER_HUMAN_CONFIRMATION"
    r["approval_condition"] = "採用済み旧QA版。対応Source/QAはGitHub保存あり。歴史Buildの再利用不要をHuman確認。最新2.5D Buildは別途全てKEEP"
    medium.append(r)

# Raw dumps: unique exact historical readings, explicitly omitted at Freeze.
# NOT in recommended medium deletion total. Keep until future reanalysis decision.
excluded = V / "UDF2/Source/Docs/ArtReview/Proof2_HeroShot_20260930/Evidence/FreezeCheckpoint/ExcludedEvidence.csv"
with excluded.open(encoding="utf-8-sig", newline="") as f:
    raw = list(csv.DictReader(f))
for r in raw:
    p = Path(r["local_path"])
    r["observed_bytes"] = p.stat().st_size
    r["observed_sha256"] = sha(p)
    r["sha_matches_freeze"] = r["observed_sha256"].lower() == r["sha256"].lower()
    r["classification"] = "D. DO NOT DELETE YET"
    r["regenerable"] = "再撮影手順/派生CSV PNGはGitHub保存。GPU/環境依存の当時数値byte完全再生成は未保証"
    r["deletion_condition"] = "凍結再開時のraw再解析が不要かHuman判断し、除外manifestと保存済み派生結果を照合するまで削除非推奨"
csv_write("DesertFreezeRawDumps.csv", raw)
csv_write("MediumRiskCandidates.csv", medium)
markdown_table("MediumRisk_Table.md", medium, [("alias", "候補Path"), ("gib", "GiB"), ("file_count", "files"), ("last_modified_jst", "Last modified JST"), ("approval_condition", "必要確認")])

# Parent-level inventory for all relevant tasks, QA, Build buckets, and sessions.
interesting = set()
for p in (H, H / ".codex_tmp", H / "output", H / "Builds", H / "Docs/QA", H / "Docs/Archive", D, D / "Logs", V, VH, C, C / "visualizations", C / "sessions"):
    if p in ROWS:
        interesting.add(p)
    interesting.update(path for path in ROWS if path.parent == p)
inventory = [enriched(ROWS[p]) for p in interesting]
inventory.sort(key=lambda r: -r["bytes"])
csv_write("FolderInventory.csv", inventory)
for name, parent in (("HimoTemp", H / ".codex_tmp"), ("HimoOutput", H / "output"), ("HimoBuilds", H / "Builds"), ("HimoQA", H / "Docs/QA"), ("DesertLogs", D / "Logs"), ("DesertVisualizations", V), ("HimoVisualizations", VH)):
    subset = [r for r in inventory if Path(r["path"]).parent == parent]
    markdown_table(name + "_Table.md", subset, [("alias", "Path"), ("gib", "GiB"), ("file_count", "files"), ("last_modified_jst", "Last modified JST"), ("classification", "分類"), ("github_saved", "GitHub保存状況"), ("contains_unsaved", "未保存"), ("regenerable", "再生成/条件")])

# Prove all proposed paths are disjoint and outside protected 2.5D and source.
selected = [Path(r["path"]) for r in low + medium]
for i, p in enumerate(selected):
    assert not protected(p)
    assert not inside(p, H / "Assets") and not inside(p, D / "Assets")
    for q in selected[i + 1:]:
        assert not inside(p, q) and not inside(q, p), (p, q)
groups = defaultdict(int)
for r in low:
    groups[r["candidate_group"]] += r["bytes"]
protected_sizes = defaultdict(int)
for p in ROWS:
    if protected(p) and p.parent in {H / ".codex_tmp", H / "output", H / "Docs/QA"}:
        protected_sizes[alias(p.parent)] += ROWS[p]["bytes"]
result = {
    "total_scanned_bytes": S["total_bytes"], "total_scanned_gib": S["total_bytes"] / GIB,
    "file_count": sum(r["file_count"] for r in S["root_totals"]),
    "root_totals": S["root_totals"], "unreadable_entries": len(S["errors"]), "skipped_reparse_points": len(S["skipped_reparse_points"]),
    "unconditional_immediate_delete_candidates_gib": 0,
    "unconditional_reason": "Unity processes still running, two project paths unknown; no delete authorization. Confirm all Unity exited before any cache action.",
    "low_risk_candidate_bytes": sum(r["bytes"] for r in low), "low_risk_candidate_gib": sum(r["bytes"] for r in low) / GIB,
    "low_risk_directory_count": len(low), "low_risk_groups_gib": {k: v / GIB for k, v in groups.items()},
    "medium_risk_candidate_bytes": sum(r["bytes"] for r in medium), "medium_risk_candidate_gib": sum(r["bytes"] for r in medium) / GIB,
    "candidate_total_after_confirmation_gib": sum(r["bytes"] for r in low + medium) / GIB,
    "raw_dumps_not_recommended_gib": sum(r["observed_bytes"] for r in raw) / GIB,
    "raw_dumps_count": len(raw), "raw_sha_all_match": all(r["sha_matches_freeze"] for r in raw),
    "protected_latest_2p5d_groups_gib": {k: v / GIB for k, v in protected_sizes.items()},
    "protected_latest_2p5d_minimum_gib": sum(protected_sizes.values()) / GIB,
    "candidate_paths_disjoint": True, "latest_2p5d_candidate_count": 0,
    "desert_frozen_tag_confirmed": REMOTE["https://github.com/bq24014-cmd/DesertLoop.git"]["refs/tags/desert-loop-frozen-2026-09-30^{}"],
    "deletion_count": 0, "moved_count": 0, "compressed_count": 0,
    "limitations": "Logical GiB lower bound (48 metadata errors; 17 links excluded). Not actual allocated/reclaimable blocks. Active files may change. Audit itself adds only new report/Evidence files. No session or credential contents read."
}
(E / "CandidateSummary.json").write_text(json.dumps(result, ensure_ascii=False, indent=2), encoding="utf-8")
print(json.dumps(result, ensure_ascii=False, indent=2))
