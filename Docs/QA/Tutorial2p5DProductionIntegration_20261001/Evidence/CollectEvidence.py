"""Collect exact production scope, build sizes, raw timings and seam pixels."""
import csv
import hashlib
import json
import shutil
import statistics
import subprocess
from pathlib import Path

from PIL import Image, ImageDraw, ImageFont

QA = Path(__file__).resolve().parents[1]
ROOT = QA.parents[2]
TEMP = ROOT / ".codex_tmp/Tutorial2p5DProductionIntegration_20261001"


def digest(path):
    with path.open("rb") as stream:
        return hashlib.file_digest(stream, "sha256").hexdigest().upper()


def git(*args):
    return subprocess.check_output(["git", "-c", "safe.directory=" + ROOT.as_posix(), *args], cwd=ROOT).decode("utf-8").strip()


start = json.loads((TEMP / "protection_start.json").read_text(encoding="utf-8"))
assets = [ROOT / "Assets/Scripts/HimoHitoCraftRoomBackground.cs",
          ROOT / "Assets/Scripts/TutorialCraftRoomLayers.cs", ROOT / "Assets/Scripts/TutorialCraftRoomLayers.cs.meta",
          ROOT / "Assets/Resources/Art/Tutorial2p5D.meta"]
assets += list((ROOT / "Assets/Resources/Art/Tutorial2p5D").glob("*"))
assets += list((ROOT / "Assets/Resources").glob("TutorialBackground*"))
production = []
for path in sorted(assets):
    name = path.relative_to(ROOT).as_posix()
    isolated = TEMP / "IntegrationProject" / name
    production.append({"path": name, "bytes": path.stat().st_size, "sha256": digest(path),
                       "isolated_source_equal": digest(path) == digest(isolated)})
gameplay = []
for name, info in start["files"].items():
    if name.startswith("Assets/Scripts/") and name != "Assets/Scripts/HimoHitoCraftRoomBackground.cs":
        if digest(ROOT / name) != info["sha256"]:
            gameplay.append(name)
assert not gameplay
assert all(p["isolated_source_equal"] for p in production if p["path"].endswith((".cs", ".shader", ".png")))

scenes = {}
for name in ("Assets/Scenes/Tutorial.unity", "Assets/Scenes/MainStage.unity", "Assets/Resources/Audio/YasashiiOdori.mp3"):
    sha = digest(ROOT / name)
    scenes[name] = {"before": start["files"][name]["sha256"], "after": sha, "unchanged": sha == start["files"][name]["sha256"]}
assert all(p["unchanged"] for p in scenes.values())

(QA / "ProductionManifest.json").write_text(json.dumps({"production": production, "preexisting_gameplay_source_differences": gameplay,
    "scene_bgm_protection": scenes, "diagnostics_in_production": (ROOT / "Assets/Scripts/StandaloneFrameProbe.cs").exists(),
    "HEAD": git("rev-parse", "HEAD"), "origin_main": git("rev-parse", "origin/main"), "staged": git("diff", "--cached", "--name-only")}, indent=2), encoding="utf-8")
(QA / "Evidence/Production.patch").write_text(git("diff", "--", "Assets/Scripts/HimoHitoCraftRoomBackground.cs") + "\n", encoding="utf-8")

logs = QA / "Evidence/Logs"
logs.mkdir(exist_ok=True)
for path in TEMP.glob("Unity*.log"):
    shutil.copyfile(path, logs / path.name)

builds = {}
for label, suffix in (("baseline", "Baseline05"), ("production", "Production04")):
    folder = ROOT / f"output/Tutorial2p5DProductionIntegration_20261001_{suffix}/Game"
    text = (folder / "BUILD_RESULT.txt").read_text(encoding="utf-8")
    parts = dict(row.split("=", 1) for row in text.splitlines() if "=" in row)
    builds[label] = {"path": str(folder), "result": text.splitlines()[0], "reported_bytes": int(parts["bytes"]),
                     "seconds": float(parts["seconds"]), "exe_sha256": digest(folder / "HimoHitoProduction2p5D.exe")}
delta = builds["production"]["reported_bytes"] - builds["baseline"]["reported_bytes"]
(QA / "BuildSummary.json").write_text(json.dumps({"builds": builds, "delta_bytes": delta, "delta_mib": delta / 1048576,
    "delta_percent": 100 * delta / builds["baseline"]["reported_bytes"],
    "scope": "Same working-tree baseline / production, Windows64 release; identical inactive QA probe in both; production isolated Resources also contains tiny legacy comparison shader. Not a clean Git-only build; documented external BGM and existing working-tree state are retained."}, indent=2), encoding="utf-8")

perf = {}
for label, folder in (("baseline", "StandaloneBaselineAdoption"), ("production", "StandaloneProductionAdoption")):
    base = QA / "Evidence" / folder
    if not (base / "result.txt").exists():
        continue
    lines = (base / "result.txt").read_text(encoding="utf-8").splitlines()
    parts = dict(row.split("=", 1) for row in lines if "=" in row)
    rows = list(csv.DictReader((base / "frames.csv").open(encoding="utf-8")))
    # Discard recorder's first undefined sample and the first ten startup frames in both.
    samples = rows[10:]
    values = [float(row["frame_ms"]) for row in samples]
    cpu = [int(row["main_thread_ns"]) / 1000000 for row in samples if row["valid"] == "True"]
    perf[label] = {"result": lines[0], "recorded": len(rows), "used": len(samples), "median_frame_ms": statistics.median(values),
        "p95_frame_ms": sorted(values)[int(len(values)*.95)-1], "observed_mean_fps": 1000 / statistics.mean(values),
        "median_main_thread_ms": statistics.median(cpu), "texture_bytes": int(parts["textureBytes"]),
        "resolution": parts["resolution"], "vsync": parts["vsync"], "targetFrameRate": parts["targetFrameRate"],
        "2p5d_visible": parts["2p5d_visible"], "legacy_hidden": parts["legacy_hidden"]}
if len(perf) == 2:
    (QA / "StandalonePerformance.json").write_text(json.dumps({"results": perf,
        "texture_delta_bytes": perf["production"]["texture_bytes"] - perf["baseline"]["texture_bytes"],
        "scope": "Sequential Windows standalone hidden helper windows, runInBackground enabled only by explicit diagnostic flag, Section1 stationary,1600x900,vsync0,uncapped,240 frames (230 used). Camera/input/physics unchanged. Main Thread includes waits and CSV IO; not pure CPU busy time, not full-playthrough FPS or a low-spec guarantee."}, indent=2), encoding="utf-8")

raw = QA / "Evidence/Runtime05"
if (raw / "seam_position.txt").exists():
    parts = dict(row.split("=", 1) for row in (raw / "seam_position.txt").read_text(encoding="utf-8").splitlines() if "=" in row)
    x = round(float(parts["mid_edge_viewport_x"]) * 1600)
    # Crop original rendered pixels, no synthetic retouching of the scene.
    font = ImageFont.truetype("C:/Windows/Fonts/segoeui.ttf", 24)
    sheet = Image.new("RGB", (1000, 560), "#171522")
    draw = ImageDraw.Draw(sheet)
    draw.text((15, 8), "T1 BEFORE — actual Mid boundary", font=font, fill="#eeeeef")
    draw.text((515, 8), "PRODUCTION AFTER — actual pixels", font=font, fill="#ff9bc7")
    metrics = {}
    for mode, offset in (("Before", 0), ("After", 500)):
        with Image.open(raw / f"S4_Clear_{mode}.ppm") as im:
            box = (max(0, x - 100), 600, min(1600, x + 100), 800)
            crop = im.crop(box)
            sheet.paste(crop.resize((500, 500), Image.Resampling.NEAREST), (offset, 60))
            y0, y1 = 720, 755
            sums = {str(px): [round(statistics.mean(im.getpixel((px, y))[c] for y in range(y0, y1)), 3) for c in range(3)]
                    for px in range(x - 2, x + 3)}
            metrics[mode] = {"source_box": box, "sample_y": [y0, y1], "column_mean_rgb": sums}
    sheet.save(QA / "Screenshots/GoalSeam_Closeup.png")
    (QA / "Evidence/SeamPixels.json").write_text(json.dumps({"mid_edge_pixel_x": x,
        "floor_edge_pixel_x": float(parts["floor_edge_pixel_x"]), "metrics": metrics,
        "scope": "Nearest-neighbor enlargement of raw actual Unity captures, no edited scene pixels. Camera.Render world-only evidence."}, indent=2), encoding="utf-8")
print(json.dumps({"source_files": len(production), "protected_scenes_bgm": scenes, "build_delta_bytes": delta,
                   "standalone_results": perf}, indent=2))
