"""Package unretouched all-Tutorial Unity captures and exact terrain-only masks.

Run only after the Unity QA output has finished:
  python PackageProductionComparisons.py --runtime Runtime01 --output Screenshots/Final

The runtime may be an absolute path or a name beneath this QA's Evidence folder.
Output must be a NEW directory beneath this QA folder. Full-size PPM screenshots
are converted losslessly to PNG. Only presentation sheets are resized/labeled;
no generated art assets or source screenshots are modified. Requires Pillow.
"""
from pathlib import Path
import argparse
import csv
import hashlib
import json
import math
import re

from PIL import Image, ImageChops, ImageDraw, ImageFont


VARIANTS = ("Before", "After")
LABELS = ("BEFORE / CURRENT", "AFTER / HYBRID PRODUCTION")
TERRAINS = (
    "Start Ground", "Tutorial Landing", "Tutorial T2 Landing",
    "Tutorial T3 Landing", "Tutorial T4 Goal Floor",
)
STATE_FIELDS = (
    "frame", "section", "outcome", "player_x", "player_y", "grounded", "attached",
    "rope_selected", "rope_remaining", "bridges", "cam_x", "cam_y", "cam_z",
    "ortho", "width", "height",
)
PROJECTION_FIELDS = (
    "min_x", "max_x", "min_y", "max_y", "viewport_left", "viewport_right",
    "viewport_bottom", "viewport_top",
)
SECTIONS = (
    ("SECTION 1 / START GROUND", "S1_Standing"),
    ("SECTION 2 / FIRST LANDING", "S2_Standing"),
    ("SECTION 3 / BRIDGE BANK", "S3_Standing"),
    ("SECTION 4 / WIDE GOAL FLOOR", "S4_WideStanding"),
)
ALL_TARGETS = (
    ("START GROUND / 6 x 10", "S1_Standing"),
    ("FIRST LANDING / 6 x 10", "S2_Standing"),
    ("BRIDGE BANK / 6 x 10", "S3_Standing"),
    ("MERGE BANK / 6 x 10", "S4_Standing"),
    ("GOAL FLOOR / 15 x 10", "S4_WideStanding"),
)
BACKGROUND = "#131824"
TITLE = "#f1ddbb"
TEXT = "#c9cdd8"
ACCENT = "#eed2a5"
PAIR_PATTERN = re.compile(r"^(.+)_(Before|After)$")


def refuse_existing(path):
    if path.exists():
        raise RuntimeError(f"Refuse overwrite: {path}")
    return path


def sha256(path):
    digest = hashlib.sha256()
    with path.open("rb") as stream:
        for chunk in iter(lambda: stream.read(1024 * 1024), b""):
            digest.update(chunk)
    return digest.hexdigest().upper()


def read_csv(path):
    with path.open(encoding="utf-8-sig", newline="") as stream:
        return list(csv.DictReader(stream))


def font(size):
    for candidate in ("C:/Windows/Fonts/segoeui.ttf", "/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf"):
        if Path(candidate).is_file():
            return ImageFont.truetype(candidate, size)
    return ImageFont.load_default()


def fit(image, width, height):
    factor = min(width / image.width, height / image.height)
    size = (max(1, round(image.width * factor)), max(1, round(image.height * factor)))
    return image.resize(size, Image.Resampling.LANCZOS)


def paste_centered(sheet, image, x, y, width, height):
    scaled = fit(image, width, height)
    sheet.paste(scaled, (x + (width - scaled.width) // 2, y + (height - scaled.height) // 2))


def pair_sheet(images, moment, destination, title_font, small_font):
    sheet = Image.new("RGB", (1920, 676), BACKGROUND)
    draw = ImageDraw.Draw(sheet)
    draw.text((20, 12), "HIMOHITO / TUTORIAL PRODUCTION TERRAIN / " + moment.upper(), font=title_font, fill=TITLE)
    draw.text((20, 49), "Same Unity frame and camera. Only the five audited Tutorial terrain visuals are toggled.", font=small_font, fill=TEXT)
    for index, source in enumerate(images):
        x = index * 960
        draw.text((x + 18, 81), LABELS[index], font=small_font, fill=ACCENT)
        paste_centered(sheet, source, x, 109, 960, 540)
    draw.text((20, 652), "Full-size PNGs retained. World-camera captures exclude Overlay UI; automated QA is not Human approval.", font=small_font, fill=TEXT)
    sheet.save(refuse_existing(destination))


def overview_sheet(pairs, out, entries, filename, heading, allow_partial, title_font, small_font):
    # A storyboard of actual local views, never a synthetic continuous panorama.
    row_height, start_y = 590, 104
    sheet = Image.new("RGB", (1920, start_y + row_height * len(entries) + 36), BACKGROUND)
    draw = ImageDraw.Draw(sheet)
    draw.text((20, 12), heading, font=title_font, fill=TITLE)
    draw.text((20, 48), "BEFORE / CURRENT", font=small_font, fill=ACCENT)
    draw.text((980, 48), "AFTER / HYBRID PRODUCTION", font=small_font, fill=ACCENT)
    draw.text((20, 76), "Actual section views at equal layout scale; matching camera within each row. Not a stitched panorama.", font=small_font, fill=TEXT)
    records = []
    for index, (description, moment) in enumerate(entries):
        top = start_y + index * row_height
        draw.text((20, top), description + "  /  " + moment, font=small_font, fill=TITLE)
        if moment not in pairs:
            if not allow_partial:
                raise ValueError("Missing overview moment " + moment)
            draw.text((20, top + 55), "NOT CAPTURED / INCOMPLETE QA", font=title_font, fill="#ee9a9a")
            records.append({"label": description, "moment": moment, "captured": False})
            continue
        for column, variant in enumerate(VARIANTS):
            path = out / (pairs[moment][variant]["label"] + ".png")
            with Image.open(path) as source:
                paste_centered(sheet, source.convert("RGB"), column * 960, top + 30, 960, 540)
        records.append({"label": description, "moment": moment, "captured": True,
                        "source_labels": [pairs[moment][variant]["label"] for variant in VARIANTS]})
    draw.text((20, sheet.height - 30), "No terrain art editing / no camera repositioning / no MainStage production change. Human Review pending.", font=small_font, fill=TEXT)
    sheet.save(refuse_existing(out / filename))
    return {"file": filename, "rows": records, "layout": "two columns Before/After; real full-camera views; no scene stitching"}


def projected_rectangle(row, width, height, margin=3):
    # Unity's actual WorldToViewportPoint was sampled with the capture's
    # RenderTexture bound. PPM coordinates start at top-left, viewport at bottom.
    values = {key: float(row[key]) for key in PROJECTION_FIELDS}
    if not all(math.isfinite(value) for value in values.values()):
        raise ValueError("Nonfinite collider/projection evidence")
    if values["min_x"] >= values["max_x"] or values["min_y"] >= values["max_y"]:
        raise ValueError("Empty or reversed collider bounds")
    exact = (
        width * values["viewport_left"], height * (1 - values["viewport_top"]),
        width * values["viewport_right"], height * (1 - values["viewport_bottom"]),
    )
    expanded = (math.floor(exact[0]) - margin, math.floor(exact[1]) - margin,
                math.ceil(exact[2]) + margin, math.ceil(exact[3]) + margin)
    clipped = (max(0, expanded[0]), max(0, expanded[1]), min(width, expanded[2]), min(height, expanded[3]))
    if clipped[0] >= clipped[2] or clipped[1] >= clipped[3]:
        clipped = None
    return exact, expanded, clipped


def compare(images, projection):
    width, height = images[0].size
    mask = Image.new("L", (width, height), 0)
    draw = ImageDraw.Draw(mask)
    rectangles = []
    for target in TERRAINS:
        row = projection[target]
        exact, expanded, clipped = projected_rectangle(row, width, height)
        if clipped is not None:
            left, top, right, bottom = clipped
            draw.rectangle((left, top, right - 1, bottom - 1), fill=255)
        rectangles.append({"target": target, "world_bounds": {key: float(row[key]) for key in ("min_x", "max_x", "min_y", "max_y")},
                           "exact_projected_pixels": exact, "expanded_rectangle_3px": expanded, "clipped_mask_rectangle": clipped})
    delta = ImageChops.difference(*images)
    red, green, blue = delta.split()
    # Max-channel difference preserves a one-unit change in ANY channel; an
    # RGB-to-luminance conversion could round small blue differences to zero.
    difference = ImageChops.lighter(ImageChops.lighter(red, green), blue)
    outside = ImageChops.multiply(difference, ImageChops.invert(mask))
    outside_histogram = outside.histogram()
    changed = sum(difference.histogram()[1:])
    outside_changed = sum(outside_histogram[1:])
    return {"width": width, "height": height, "terrain_masks": rectangles,
            "rectangle_convention": "half-open [left,top,right,bottom)",
            "mask_basis": "union of all five exact Unity-projected Collider bounds, floor/ceil outward, then +3 pixels each side",
            "union_mask_pixels": mask.histogram()[255], "total_changed_pixels": changed,
            "inside_terrain_union_changed_pixels": changed - outside_changed,
            "outside_terrain_union_changed_pixels": outside_changed,
            "outside_max_channel_difference": max((index for index, count in enumerate(outside_histogram) if count), default=0),
            "difference_bbox": difference.getbbox(), "outside_difference_bbox": outside.getbbox(),
            "outside_terrain_union_exact_match": outside_changed == 0}


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--runtime", required=True)
    parser.add_argument("--output", required=True)
    parser.add_argument("--allow-partial", action="store_true", help="Package an incomplete diagnostic run, prominently marked incomplete.")
    args = parser.parse_args()
    qa = Path(__file__).resolve().parent.parent
    source_arg, out_arg = Path(args.runtime), Path(args.output)
    runtime = source_arg.resolve() if source_arg.is_absolute() else (qa / "Evidence" / source_arg).resolve()
    out = out_arg.resolve() if out_arg.is_absolute() else (qa / out_arg).resolve()
    if not out.is_relative_to(qa) or out == qa or out == runtime or out.is_relative_to(runtime):
        raise ValueError("Output must be a distinct new directory beneath this QA folder, outside runtime captures")
    refuse_existing(out)
    manifest = refuse_existing(qa / "Evidence" / ("ImageComparison_" + runtime.name + ".json"))
    source_rows = read_csv(runtime / "captures.csv")
    rows = {row["label"]: row for row in source_rows}
    if len(rows) != len(source_rows):
        raise ValueError("Duplicate capture labels")
    projection_rows = read_csv(runtime / "terrain_projection.csv")
    projection = {}
    for row in projection_rows:
        group = projection.setdefault(row["label"], {})
        if row["target"] in group:
            raise ValueError("Duplicate target projection: " + row["label"] + "/" + row["target"])
        group[row["target"]] = row
    pairs = {}
    for label, row in rows.items():
        match = PAIR_PATTERN.fullmatch(label)
        if match:
            moment, variant = match.groups()
            pairs.setdefault(moment, {})[variant] = row
    if not pairs:
        raise ValueError("No captured Before/After pairs")
    for moment, pair in pairs.items():
        if set(pair) != set(VARIANTS):
            raise ValueError("Incomplete pair: " + moment)
        for field in STATE_FIELDS:
            if pair["Before"][field] != pair["After"][field]:
                raise ValueError(f"Same-frame state mismatch: {moment}/{field}")
        if pair["Before"]["hybrid"] != "False" or pair["After"]["hybrid"] != "True":
            raise ValueError("Variant flag mismatch: " + moment)
        for variant in VARIANTS:
            label = pair[variant]["label"]
            if set(projection.get(label, {})) != set(TERRAINS):
                raise ValueError("Expected five exact terrain projections: " + label)
            if not (runtime / (label + ".ppm")).is_file():
                raise FileNotFoundError(label + ".ppm")
        for target in TERRAINS:
            for field in PROJECTION_FIELDS:
                if projection[pair["Before"]["label"]][target][field] != projection[pair["After"]["label"]][target][field]:
                    raise ValueError(f"Terrain projection/bounds mismatch: {moment}/{target}/{field}")
    required = {moment for _, moment in ALL_TARGETS} | {"Intro_Resumed", "S4_Clear"}
    missing = sorted(required - set(pairs))
    if missing and not args.allow_partial:
        raise ValueError("Missing required full-route capture moments: " + ", ".join(missing))
    out.mkdir(parents=True, exist_ok=False)
    title_font, small_font = font(25), font(18)
    conversions = []
    for source in sorted(runtime.glob("*.ppm")):
        dest = refuse_existing(out / (source.stem + ".png"))
        with Image.open(source) as image:
            rgb = image.convert("RGB"); digest = hashlib.sha256(rgb.tobytes()).hexdigest().upper(); size = rgb.size
            rgb.save(dest)
        with Image.open(dest) as image:
            if hashlib.sha256(image.convert("RGB").tobytes()).hexdigest().upper() != digest:
                raise RuntimeError("Lossless conversion check failed: " + str(dest))
        conversions.append({"source": str(source), "png": str(dest), "width": size[0], "height": size[1],
                            "rgb_sha256": digest, "png_sha256": sha256(dest), "lossless_verified": True})
    comparisons = []
    for moment, pair in sorted(pairs.items()):
        images = []
        for variant in VARIANTS:
            with Image.open(out / (pair[variant]["label"] + ".png")) as image:
                images.append(image.convert("RGB"))
        expected = (int(pair["Before"]["width"]), int(pair["Before"]["height"]))
        if images[0].size != images[1].size or images[0].size != expected:
            raise ValueError("Pair dimensions mismatch CSV: " + moment)
        measurement = compare(images, projection[pair["Before"]["label"]])
        measurement.update({"moment": moment, "frame": int(pair["Before"]["frame"]), "section": int(pair["Before"]["section"]),
                            "source_labels": [pair[variant]["label"] for variant in VARIANTS], "reported_pair_state_fields_match": True})
        comparisons.append(measurement)
        pair_sheet(images, moment, out / ("Comparison_" + moment + ".png"), title_font, small_font)
        for section, (_, representative) in enumerate(SECTIONS, 1):
            if moment == representative:
                pair_sheet(images, moment, out / (f"Section{section}_Comparison.png"), title_font, small_font)
        if moment == "S4_Standing":
            pair_sheet(images, moment, out / "Section4_Entry_Comparison.png", title_font, small_font)
    overview = overview_sheet(pairs, out, SECTIONS, "Overview_Comparison.png", "HIMOHITO / TUTORIAL ALL SECTIONS / BEFORE - AFTER", args.allow_partial, title_font, small_font)
    target_overview = overview_sheet(pairs, out, ALL_TARGETS, "AllFiveTerrain_Comparison.png", "HIMOHITO / ALL FIVE NORMAL TERRAIN TARGETS", args.allow_partial, title_font, small_font)
    outside_max = max(record["outside_terrain_union_changed_pixels"] for record in comparisons)
    record = {
        "runtime": runtime.name, "runtime_path": str(runtime), "output": str(out), "partial": bool(missing), "missing_moments": missing,
        "pair_count": len(comparisons), "all_pairs_outside_terrain_exact_match": outside_max == 0,
        "outside_terrain_max_changed_pixels": outside_max,
        "source_evidence": {name: sha256(runtime / name) for name in ("captures.csv", "terrain_projection.csv")},
        "conversion": "Lossless full-resolution Unity PPM -> PNG; exact RGB SHA256 verified; source captures/art untouched",
        "sheet_processing": "Identical resizing for each Before/After pair, plus labels. No retouching, erasure, repainting or inferred intermediate frames",
        "limits": "World-camera screenshots exclude Overlay UI. Pixel isolation proves only same-frame terrain-region changes; it does not replace Human visual/readability review or the separate gameplay regression. No MainStage production rollout.",
        "runtime_result": (runtime / "result.txt").read_text(encoding="utf-8") if (runtime / "result.txt").is_file() else "UNAVAILABLE / runtime not confirmed finished",
        "conversions": conversions, "comparisons": comparisons, "overview": overview, "all_target_overview": target_overview,
    }
    manifest.write_text(json.dumps(record, ensure_ascii=False, indent=2), encoding="utf-8")
    print(json.dumps({"png_count": len(list(out.glob("*.png"))), "pair_count": len(comparisons), "partial": bool(missing),
                      "outside_terrain_max_changed_pixels": outside_max, "output": str(out), "manifest": str(manifest)}))
    # Keep all evidence even on a failed isolation check; do not label it PASS.
    if outside_max or missing:
        raise SystemExit(2)


if __name__ == "__main__":
    main()
