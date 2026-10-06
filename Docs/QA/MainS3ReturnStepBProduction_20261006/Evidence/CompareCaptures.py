from pathlib import Path
import json
from PIL import Image, ImageChops, ImageDraw

base = Path(__file__).resolve().parent
run = base / "Run04"
screens = base.parent / "Screenshots"
screens.mkdir(exist_ok=True)
rows = []
for prefix in ("01_Gameplay", "02_Near", "03_Surroundings", "04_FullObject", "04b_BareEnd"):
    a = Image.open(run / (prefix + "_Production.png")).convert("RGB")
    b = Image.open(run / (prefix + "_ApprovedProof.png")).convert("RGB")
    delta = ImageChops.difference(a, b)
    pixels = sum(any(channel for channel in p) for p in delta.getdata())
    rows.append({"view": prefix, "size": list(a.size), "differentPixels": pixels, "bounds": delta.getbbox(), "status": "PASS" if pixels == 0 else "FAIL"})
    board = Image.new("RGB", (a.width * 2, a.height + 44), (18, 18, 30))
    board.paste(b, (0, 44))
    board.paste(a, (a.width, 44))
    draw = ImageDraw.Draw(board)
    draw.text((20, 15), "APPROVED PROOF", fill=(245, 235, 210))
    draw.text((a.width + 20, 15), "PRODUCTION", fill=(245, 235, 210))
    board.save(screens / (prefix + "_Approved_vs_Production.png"))
record = {"method": "Synchronous same-frame Unity render-only camera pairs; no gameplay/camera change. RGB decoded pixel comparison.", "comparisons": rows, "failures": sum(r["status"] == "FAIL" for r in rows)}
(base / "PixelComparison.json").write_text(json.dumps(record, ensure_ascii=False, indent=2), encoding="utf-8")
print(json.dumps(record, ensure_ascii=False, indent=2))
if record["failures"]:
    raise SystemExit(1)
