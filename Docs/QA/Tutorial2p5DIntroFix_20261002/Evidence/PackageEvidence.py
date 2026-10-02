"""Package unretouched Unity captures and audit the one-condition change."""
import csv
import difflib
import hashlib
import json
import math
import statistics
from pathlib import Path
from PIL import Image, ImageDraw, ImageFont

ROOT = Path(__file__).resolve().parents[4]
QA = Path(__file__).resolve().parents[1]
OUT = QA / "Screenshots"
OUT.mkdir(exist_ok=True)
FONT = ImageFont.truetype("C:/Windows/Fonts/segoeui.ttf", 23)
POINTS = ["A_TutorialCommitted", "B_IntroStart", "C_IntroMiddle", "D_IntroNearEnd", "E_GameplayResumed", "F_AfterMovement"]
records, manifest = {}, []
for variant in ("Before", "After"):
    raw = QA / "Evidence" / ("Timeline" + variant)
    records[variant] = list(csv.DictReader((raw / "frames.csv").open(encoding="utf-8")))
    dest = OUT / variant
    dest.mkdir(exist_ok=True)
    for path in sorted(raw.glob("*.ppm")):
        target = dest / (path.stem + ".png")
        with Image.open(path) as im:
            im.save(target)
        manifest.append({"path": target.relative_to(QA).as_posix(), "sha256": hashlib.sha256(target.read_bytes()).hexdigest().upper()})

for point in POINTS:
    sheet = Image.new("RGB", (1600, 502), "#171522")
    draw = ImageDraw.Draw(sheet)
    for variant, x, color in (("Before", 0, "#ddddec"), ("After", 800, "#ff9bc7")):
        draw.text((x + 14, 9), variant.upper() + " / " + point, font=FONT, fill=color)
        with Image.open(OUT / variant / (point + ".png")) as im:
            sheet.paste(im.resize((800, 450), Image.Resampling.LANCZOS), (x, 52))
    sheet.save(OUT / (point + "_Comparison.png"))

overview = Image.new("RGB", (1600, 1506), "#171522")
for i, point in enumerate((POINTS[1], POINTS[2], POINTS[4])):
    with Image.open(OUT / (point + "_Comparison.png")) as im:
        overview.paste(im, (0, i * 502))
overview.save(OUT / "Intro_BeforeAfter_Overview.png")

# Pair nearest actual samples by intro elapsed ratio. Never synthesize camera frames.
before_series = [r for r in records["Before"] if r["label"].startswith("Sequence_")]
after_series = [r for r in records["After"] if r["label"].startswith("Sequence_")]
frames, pair_map = [], []
for row in after_series:
    ratio = float(row["preview_elapsed"]) / float(row["preview_total"])
    prior = min(before_series, key=lambda r: abs(float(r["preview_elapsed"]) / float(r["preview_total"]) - ratio))
    sheet = Image.new("RGB", (1280, 396), "#171522")
    draw = ImageDraw.Draw(sheet)
    for variant, record, x, color in (("Before", prior, 0, "#ddddec"), ("After", row, 640, "#ff9bc7")):
        draw.text((x + 10, 4), variant.upper() + " / ACTUAL UNITY INTRO", font=FONT, fill=color)
        with Image.open(OUT / variant / (record["label"] + ".png")) as im:
            sheet.paste(im.resize((640, 360), Image.Resampling.LANCZOS), (x, 36))
    frames.append(sheet.convert("P", palette=Image.Palette.ADAPTIVE, colors=192))
    pair_map.append({"before": prior["label"], "after": row["label"], "after_progress": ratio})
frames[0].save(OUT / "Intro_BeforeAfter.gif", save_all=True, append_images=frames[1:], duration=200, loop=0, optimize=False)

def pan(t, duration, ramp=1):
    if t <= 0: return 0
    if t >= duration: return 1
    area = duration - ramp
    if t < ramp: return (.5*t - ramp/(2*math.pi)*math.sin(math.pi*t/ramp))/area
    if t > duration-ramp:
        rem = duration-t
        return 1-(.5*rem - ramp/(2*math.pi)*math.sin(math.pi*rem/ramp))/area
    return (t-.5*ramp)/area

camera_checks = {}
for variant, rows in records.items():
    preview = [r for r in rows if r["preview_active"] == "True"]
    deviations = [abs(float(r["camera_x"]) - (63 + (-4-63)*pan(max(0, float(r["preview_elapsed"])-.7), float(r["preview_total"])-.7))) for r in preview]
    camera_checks[variant] = {"preview_samples": len(preview), "max_error_vs_existing_pan_formula": max(deviations),
        "y_z_size_states": sorted(set((r["camera_y"], r["camera_z"], r["orthographic_size"]) for r in preview))}
    assert max(deviations) < .0001

before = (QA / "Evidence/BeforeProduction/TutorialCraftRoomLayers.cs").read_bytes()
after = (ROOT / "Assets/Scripts/TutorialCraftRoomLayers.cs").read_bytes()
old = b"bool show = enabled && run.Outcome != PrototypeRunController.RunOutcome.WaitingToStart &&\n                !MainStagePreview.IsActive && !StageStartTransition.IsActive;"
new = b"bool show = enabled && run.Outcome != PrototypeRunController.RunOutcome.WaitingToStart;"
exact = before.replace(old, new) == after
normalized = before.replace(b"\r\n", b"\n").replace(old.replace(b"\r\n", b"\n"), new) == after.replace(b"\r\n", b"\n")
assert normalized, "Unauthorized Source difference"
patch = "".join(difflib.unified_diff(before.decode().splitlines(True), after.decode().splitlines(True),
    fromfile="BeforeProduction/TutorialCraftRoomLayers.cs", tofile="Assets/Scripts/TutorialCraftRoomLayers.cs"))
(QA / "Evidence/VisibilityCondition.patch").write_text(patch, encoding="utf-8", newline="")
summary = {"camera": camera_checks, "camera_pair_state_equal": camera_checks["Before"]["y_z_size_states"] == camera_checks["After"]["y_z_size_states"],
    "source_byte_exact_expected_replacement": exact, "source_normalized_exact_expected_replacement": normalized,
    "source_before_sha": hashlib.sha256(before).hexdigest().upper(), "source_after_sha": hashlib.sha256(after).hexdigest().upper(),
    "milestones": {k: [r for r in v if r["label"] in POINTS] for k, v in records.items()},
    "captures": manifest, "gif_pairs": pair_map,
    "limits": "Camera.Render world captures exclude IMGUI/menu/cloth/HUD. Actual poses retained; no camera override. A is commit behind opening cloth, not initial menu click. GIF uses nearest recorded intro progress, resized fixed200ms samples, not frame-exact realtime video or FPS measurement."}
(QA / "EvidenceManifest.json").write_text(json.dumps(summary, ensure_ascii=False, indent=2), encoding="utf-8")
print(json.dumps({"source_exact": exact, "camera": camera_checks, "captures": len(manifest), "gif_frames": len(frames)}, ensure_ascii=False, indent=2))

regression = QA / "Evidence/Regression"
reg_out = OUT / "Regression"
reg_out.mkdir(exist_ok=True)
for path in sorted(regression.glob("*.ppm")):
    if path.stem.startswith("Motion_") or path.stem.endswith("_Before"):
        continue
    with Image.open(path) as im:
        im.save(reg_out / (path.stem + ".png"))
overview = Image.new("RGB", (1600, 984), "#171522")
draw = ImageDraw.Draw(overview)
for i in range(4):
    x, y = (i % 2)*800, (i // 2)*492
    draw.text((x+14, y+8), f"SECTION {i+1} / REGRESSION PASS", font=FONT, fill="#ff9bc7")
    with Image.open(reg_out / f"S{i+1}_Standing_After.png") as im:
        overview.paste(im.resize((800, 450), Image.Resampling.LANCZOS), (x, y+42))
overview.save(OUT / "AllSections_Regression_Overview.png")
timings = {}
for filename, value in (("render_timing.csv", "render_cpu_ms"), ("frame_timing.csv", "frame_ms")):
    rows = list(csv.DictReader((regression / filename).open(encoding="utf-8")))
    timings[filename] = {}
    for mode in sorted(set(r["mode"] for r in rows)):
        values = [float(r[value]) for r in rows if r["mode"] == mode]
        timings[filename][mode] = {"samples": len(values), "median_ms": statistics.median(values), "max_ms": max(values)}
(QA / "Evidence/TimingSummary.json").write_text(json.dumps(timings, indent=2), encoding="utf-8")
print(json.dumps(timings, indent=2))
