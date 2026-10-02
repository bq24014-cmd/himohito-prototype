"""Read-only source image edge identity; no raster assets are edited."""
import json
from pathlib import Path
import numpy as np
from PIL import Image

ROOT = Path(__file__).resolve().parents[4]
QA = Path(__file__).resolve().parents[1]
ART = ROOT / ".codex_tmp/Tutorial2p5DAllSections_20261001/TrialProject/Assets/Resources/Proof/Tutorial2p5D"
result = {}
for name in ("Far", "Mid", "NearRefined"):
    with Image.open(ART / f"Tutorial2p5D_{name}.png") as im:
        arr = np.array(im.convert("RGBA"))
    result[name] = {"size": [arr.shape[1], arr.shape[0]], "right_columns": [], "left_columns": []}
    for side, xs in (("left_columns", range(8)), ("right_columns", range(arr.shape[1]-8, arr.shape[1]))):
        for x in xs:
            col = arr[:, x, :]
            result[name][side].append({"x": int(x), "mean_rgba": col.mean(axis=0).round(3).tolist(),
                "opaque": int((col[:,3] == 255).sum()), "transparent": int((col[:,3] == 0).sum()),
                "blue_excess_gt50": int(((col[:,2].astype(float)-col[:,0]) > 50).sum())})
(QA / "Evidence/SourceEdges.json").write_text(json.dumps(result, indent=2), encoding="utf-8")
print(json.dumps(result, indent=2))
