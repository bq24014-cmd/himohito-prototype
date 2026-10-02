"""Lossless packaging of actual Unity PPM captures; no invented game imagery."""
import csv
import hashlib
import json
import statistics
from pathlib import Path
from PIL import Image, ImageDraw, ImageFont

QA = Path(__file__).resolve().parents[1]
RAW = QA / "Evidence/RuntimeAttempt02"
OUT = QA / "Screenshots"
OUT.mkdir(exist_ok=True)
FONT = ImageFont.truetype("C:/Windows/Fonts/segoeui.ttf", 24)
converted = []
for path in sorted(RAW.glob("*.ppm")):
    target = OUT / (path.stem + ".png")
    with Image.open(path) as image:
        image.save(target)
    converted.append({"path": target.relative_to(QA).as_posix(), "sha256": hashlib.sha256(target.read_bytes()).hexdigest().upper()})

for label in ("01_Standing", "02_Right", "03_Aim", "04_Jump", "05_Attached"):
    before, after = (OUT / (label + "_" + part + ".png") for part in ("Before", "After"))
    if not before.exists() or not after.exists():
        continue
    sheet = Image.new("RGB", (1600, 510), "#171522")
    draw = ImageDraw.Draw(sheet)
    draw.text((18, 12), "BEFORE — original background", font=FONT, fill="#dddded")
    draw.text((818, 12), "AFTER — three background planes", font=FONT, fill="#ff9bc7")
    for image_path, x in ((before, 0), (after, 800)):
        with Image.open(image_path) as image:
            sheet.paste(image.resize((800, 450), Image.Resampling.LANCZOS), (x, 60))
    sheet.save(OUT / (label + "_Comparison.png"))

frames = []
for path in sorted(OUT.glob("Motion_*.png")):
    with Image.open(path) as image:
        frames.append(image.resize((960, 540), Image.Resampling.LANCZOS).convert("P", palette=Image.Palette.ADAPTIVE, colors=192))
if frames:
    frames[0].save(OUT / "ActualUnityMotion.gif", save_all=True, append_images=frames[1:], duration=150, loop=0, optimize=False)
    sheet = Image.new("RGB", (1600, 1476), "#171522")
    draw = ImageDraw.Draw(sheet)
    for cell, index in enumerate((0, 5, 10, 15, 20, 29)):
        x, y = (cell % 2) * 800, (cell // 2) * 492
        draw.text((x + 12, y + 8), f"ACTUAL UNITY FRAME {index:03d}", font=FONT, fill="#dddded")
        with Image.open(OUT / f"Motion_{index:03d}.png") as image:
            sheet.paste(image.resize((800, 450), Image.Resampling.LANCZOS), (x, y + 42))
    sheet.save(OUT / "Motion_ContactSheet.png")

timing = RAW / "render_timing.csv"
stats = {}
if timing.exists():
    rows = list(csv.DictReader(timing.open(encoding="utf-8")))
    for mode in ("baseline", "proof"):
        values = [float(row["render_cpu_ms"]) for row in rows if row["mode"] == mode]
        if values:
            stats[mode] = {"samples": len(values), "median_render_cpu_ms": statistics.median(values), "min": min(values), "max": max(values)}

(QA / "EvidenceManifest.json").write_text(json.dumps({
    "actual_unity_captures": converted,
    "rendering": "Camera.Render world-camera captures; IMGUI HUD/menu excluded. No gameplay camera transform or zoom changed for captures.",
    "animation": "30 captured Unity frames; GIF resized and fixed 150ms/frame for preview, not a real-time FPS measurement. Original PNG frames and simulation-time CSV retained.",
    "performance": stats,
    "performance_limit": "Editor Camera.Render CPU-call timings, alternating modes after 10 warmups. Not GPU timing, standalone FPS, or proof of no performance regression.",
}, ensure_ascii=False, indent=2), encoding="utf-8")
print(json.dumps({"converted": len(converted), "performance": stats}, indent=2))
