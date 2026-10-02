"""Package real Unity world-camera captures, without redrawing gameplay."""
import csv
import hashlib
import json
import statistics
import sys
from pathlib import Path
from PIL import Image, ImageDraw, ImageFont

QA = Path(__file__).resolve().parents[1]
RAW = QA / "Evidence" / (sys.argv[1] if len(sys.argv) > 1 else "Runtime01")
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
    draw.text((18, 12), "BEFORE — R1 section1 / original later", font=font, fill="#ddddec")
    draw.text((818, 12), "AFTER — continuous Tutorial trial", font=font, fill="#ff9bc7")
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
    "comparison": "Before: approved R1 only in Section1, original production in later sections. After: new all-Tutorial trial. Same physical state within each pair.",
    "motion_frames": len(motion),
    "gif_limit": "Sampled actual route frames, resized and fixed180ms/frame. Not realtime video or FPS measurement.",
    "performance": stats,
    "performance_limit": "Editor Camera.Render CPU-call timings at final Clear view, warmed alternate15 samples each. Before here is original Section4 background. Not GPU timing or standalone FPS.",
}, ensure_ascii=False, indent=2), encoding="utf-8")
print(json.dumps({"captures": len(converted), "motion_frames": len(motion), "performance": stats}, indent=2))
