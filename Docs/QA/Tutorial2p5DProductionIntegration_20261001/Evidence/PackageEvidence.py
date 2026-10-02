"""Package real Unity world-camera captures, without redrawing gameplay."""
import csv
import hashlib
import json
import statistics
import sys
from pathlib import Path
from PIL import Image, ImageDraw, ImageFont

QA = Path(__file__).resolve().parents[1]
RAW = QA / "Evidence" / (sys.argv[1] if len(sys.argv) > 1 else "Runtime02")
OUT = QA / "Screenshots"
OUT.mkdir(exist_ok=True)
font = ImageFont.truetype("C:/Windows/Fonts/segoeui.ttf", 24)
converted = []
for path in sorted(RAW.glob("*.ppm")):
    target = OUT / (path.stem + ".png")
    with Image.open(path) as image:
        image.save(target)
    converted.append({"path": target.relative_to(QA).as_posix(), "sha256": hashlib.sha256(target.read_bytes()).hexdigest().upper()})

for before in sorted(OUT.glob("*_Before.png")):
    label = before.stem.removesuffix("_Before")
    after = OUT / (label + "_After.png")
    if not after.exists():
        continue
    sheet = Image.new("RGB", (1600, 510), "#171522")
    draw = ImageDraw.Draw(sheet)
    draw.text((18, 12), "BEFORE — approved T1 trial", font=font, fill="#ddddec")
    draw.text((818, 12), "AFTER — production seam fix", font=font, fill="#ff9bc7")
    for path, x in ((before, 0), (after, 800)):
        with Image.open(path) as image:
            sheet.paste(image.resize((800, 450), Image.Resampling.LANCZOS), (x, 60))
    sheet.save(OUT / (label + "_Comparison.png"))

overview = Image.new("RGB", (1600, 984), "#171522")
draw = ImageDraw.Draw(overview)
for i in range(4):
    x, y = (i % 2) * 800, (i // 2) * 492
    draw.text((x + 14, y + 8), f"TUTORIAL SECTION {i + 1} — ACTUAL UNITY", font=font, fill="#ff9bc7")
    path = OUT / f"S{i + 1}_Standing_After.png"
    if path.exists():
        with Image.open(path) as image:
            overview.paste(image.resize((800, 450), Image.Resampling.LANCZOS), (x, y + 42))
overview.save(OUT / "AllSections_Overview.png")

motion = list(csv.DictReader((RAW / "motion.csv").open(encoding="utf-8")))
for section in range(1, 5):
    frames = []
    for row in motion:
        if int(row["section"]) != section:
            continue
        with Image.open(OUT / (row["frame"] + ".png")) as image:
            frames.append(image.resize((800, 450), Image.Resampling.LANCZOS).convert("P", palette=Image.Palette.ADAPTIVE, colors=192))
    if frames:
        frames[0].save(OUT / f"Section{section}_ActualMotion.gif", save_all=True, append_images=frames[1:], duration=180, loop=0, optimize=False)

stats = {}
timing = RAW / "render_timing.csv"
if timing.exists():
    rows = list(csv.DictReader(timing.open(encoding="utf-8")))
    for mode in ("before", "all"):
        values = [float(row["render_cpu_ms"]) for row in rows if row["mode"] == mode]
        if values:
            stats[mode] = {"samples": len(values), "median_render_cpu_ms": statistics.median(values), "min": min(values), "max": max(values)}

(QA / "EvidenceManifest.json").write_text(json.dumps({
    "runtime_evidence": RAW.relative_to(QA).as_posix(), "captures": converted,
    "camera": "Real unmodified gameplay Camera.Render, 1600x900. IMGUI/HUD excluded. No camera pose/zoom override for screenshot.",
    "comparison": "Before: approved T1 all sections. After: production integration with edge clamp. Same physical state within each pair.",
    "motion_frames": len(motion),
    "gif_limit": "Sampled actual route frames, resized and fixed180ms/frame. Not realtime video or FPS measurement.",
    "performance": stats,
    "performance_limit": "Editor Camera.Render CPU-call timings at final Clear view, warmed alternate15 samples each. Before here is original Section4 background (not T1). Not GPU timing or standalone FPS.",
}, ensure_ascii=False, indent=2), encoding="utf-8")
print(json.dumps({"captures": len(converted), "motion_frames": len(motion), "performance": stats}, indent=2))


frames = list(csv.DictReader((RAW / "frame_timing.csv").open(encoding="utf-8")))
perf = {}
for mode in ("original", "production"):
    rows = [row for row in frames if row["mode"] == mode]
    frame = [float(row["frame_ms"]) for row in rows]
    cpu = [int(row["main_thread_ns"]) / 1000000 for row in rows if row["valid"] == "True"]
    perf[mode] = {"samples": len(rows), "median_frame_ms": statistics.median(frame),
        "p95_frame_ms": sorted(frame)[int(len(frame)*.95)-1], "mean_observed_fps": 1000 / statistics.mean(frame),
        "main_thread_valid": len(cpu), "median_main_thread_ms": statistics.median(cpu) if cpu else None}
(QA / "PerformanceSummary.json").write_text(json.dumps({"render_call": stats, "editor_frames": perf,
    "scope": "Batchmode Editor final Clear view,120 samples/mode, no captures, uncapped diagnostic recording clock reset. Main Thread recorder CPU counter may include waiting. No standalone gameplay/FPS guarantee."}, indent=2), encoding="utf-8")
