"""Add explicitly discovered DESERT LOOP Production; read-only metadata."""
import importlib.util
import json
import sys
from pathlib import Path

HERE = Path(__file__).resolve().parent
sys.dont_write_bytecode = True
spec = importlib.util.spec_from_file_location("survey", HERE / "Survey.py")
audit = importlib.util.module_from_spec(spec)
spec.loader.exec_module(audit)
base = json.loads((HERE / "DirectorySurvey.json").read_text(encoding="utf-8"))
root = Path("C:/Users/田尻大翔/Documents/UnityProjects/DesertLoop")
print("Read-only extra survey:", root, flush=True)
size, count, latest = audit.walk(root, root)
base["root_totals"].append({"path": str(root), "bytes": size, "gib": size/audit.GIB, "file_count": count, "last_modified_jst": audit.stamp(latest)})
base["total_bytes"] += size
base["directories"] = sorted(base["directories"] + audit.rows, key=lambda r: -r["bytes"])
for key, value in (("repositories", audit.repos), ("unity_projects", audit.projects), ("errors", audit.errors),
                   ("skipped_reparse_points", audit.links), ("large_files", audit.large)):
    base[key] += value
base["large_files"].sort(key=lambda r: -r["bytes"])
(HERE / "DirectorySurveyWithDesert.json").write_text(json.dumps(base, ensure_ascii=False, indent=2), encoding="utf-8")
print("DESERT LOOP GiB", size/audit.GIB, "files", count, "errors", len(audit.errors), flush=True)
print("Combined GiB", base["total_bytes"]/audit.GIB, flush=True)
