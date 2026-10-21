"""Package original RGBA assets without pixel editing into a self-contained static canvas viewer."""
from pathlib import Path
import base64,json
root=Path(__file__).resolve().parent.parent
names={"red":"ReferenceRed-v1.png","green":"ReferenceGreen-v2.png","warm":"HimoHitoWarmScissorsParts-v3.png"}
data={k:"data:image/png;base64,"+base64.b64encode((root/"Assets"/v).read_bytes()).decode() for k,v in names.items()}
template=(root/"Renderer"/"template.html").read_text(encoding="utf-8")
(root/"index.html").write_text(template.replace("__SOURCES__",json.dumps(data)),encoding="utf-8")
print("Packed static design viewer; no Unity or gameplay execution.")
