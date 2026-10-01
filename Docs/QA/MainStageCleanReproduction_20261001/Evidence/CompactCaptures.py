"""Verify lossless PNG evidence before moving redundant raw captures to task temp."""
from PIL import Image
import pathlib, hashlib, json
ev=pathlib.Path(__file__).resolve().parent
rows=[]
for directory in ['FocusedRuntime','FocusedRuntime2']:
    for p in (ev/directory).glob('*.ppm'):
        png=p.with_suffix('.png')
        with Image.open(p) as raw:
            if not png.exists(): raw.save(png)
            with Image.open(png) as packed: same=raw.mode==packed.mode and raw.size==packed.size and raw.tobytes()==packed.tobytes()
        if not same: raise SystemExit('STOP capture pixels changed '+str(p))
        rows.append({'raw':p.relative_to(ev).as_posix(),'raw_sha256':hashlib.sha256(p.read_bytes()).hexdigest().upper(),
            'png':png.relative_to(ev).as_posix(),'png_sha256':hashlib.sha256(png.read_bytes()).hexdigest().upper(),
            'pixel_identical':same,'raw_retained_under':'.codex_tmp/MainStageCleanReproduction_20261001/RawCaptures/'+directory+'/'+p.name})
(ev/'capture_manifest.json').write_text(json.dumps(rows,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
print('LOSSLESS_CAPTURE_VERIFICATION_PASS files='+str(len(rows)))
