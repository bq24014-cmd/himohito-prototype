import hashlib, json, subprocess
from pathlib import Path
from PIL import Image

root = Path.cwd()
e = root/'Docs/QA/MainStageCleanReproduction_20261001/Evidence'
paths = json.loads((e/'external_build_paths.json').read_text(encoding='utf-8-sig'))
build = Path(paths['copies']['BuildProject']['project'])
head_scene = subprocess.run(['git','show','HEAD:Assets/Scenes/MainStage.unity'],check=True,stdout=subprocess.PIPE).stdout
(e/'external_HEAD_MainStage_reference.unity.txt').write_bytes(head_scene)
diff = subprocess.run(['git','diff','--no-index','--',(e/'external_HEAD_MainStage_reference.unity.txt').as_posix(),(build/'Assets/Scenes/MainStage.unity').as_posix()],stdout=subprocess.PIPE)
if diff.returncode not in (0,1): raise RuntimeError('Scene diff command failed')
(e/'external_build_scene_generated.patch').write_bytes(diff.stdout)
stat = subprocess.run(['git','diff','--no-index','--stat','--',(e/'external_HEAD_MainStage_reference.unity.txt').as_posix(),(build/'Assets/Scenes/MainStage.unity').as_posix()],stdout=subprocess.PIPE)
(e/'external_build_scene_generated_stat.txt').write_bytes(stat.stdout)
captures=[]
for ppm in (e/'ExternalBGMFocusedRuntime').glob('*.ppm'):
    png=ppm.with_suffix('.png')
    with Image.open(ppm) as im:
        im.save(png)
        with Image.open(png) as check:
            if im.convert('RGB').tobytes()!=check.convert('RGB').tobytes(): raise RuntimeError('PNG changed pixels')
    captures.append({'ppm':ppm.name,'png':png.name,'pixel_equal':True})
# Relocate only this run's generated raw captures into its own temp directory; preserve them.
raw=Path(paths['task'])/'RawCapturesExternal'
raw.mkdir(exist_ok=False)
for item in captures:
    (e/'ExternalBGMFocusedRuntime'/item['ppm']).rename(raw/item['ppm'])
exe=build/'Builds/Windows/Game/HimoHitoPrototype.exe'
summary={'baseline_head':paths['baseline_head'],'condition':'clean HEAD + documented external BGM dependency','build_success':'Build Finished, Result: Success.' in (e/'ExternalBGMBuild.log').read_text(encoding='utf-8',errors='replace'),'exe_exists':exe.is_file(),'exe_sha256':hashlib.sha256(exe.read_bytes()).hexdigest().upper(),'full_runtime':(e/'ExternalBGMFullRuntime/result.txt').read_text(),'focused_runtime':(e/'ExternalBGMFocusedRuntime/result.txt').read_text(),'runtime_error_classification':'Each run recorded one UnityEditor.Search.SearchDatabase startup/index exception; diagnostic assertions all pass. Raw exceptions retained. Not a Player gameplay stack.','production_scene_diff':0,'build_copy_scene_diff_zero':diff.returncode==0,'build_copy_scene_sha256':hashlib.sha256((build/'Assets/Scenes/MainStage.unity').read_bytes()).hexdigest().upper(),'head_scene_sha256':hashlib.sha256(head_scene).hexdigest().upper(),'raw_capture_directory':str(raw),'png_pixel_checks':captures,'exe_ui_smoke':'UNCONFIRMED: computer-use backend unavailable after skill recovery; process launch is not menu/start confirmation','final_gate':'FAIL: Build copy Scene changed automatically; exe UI smoke not confirmed. Do not commit/push.'}
(e/'external_runtime_summary.json').write_text(json.dumps(summary,ensure_ascii=False,indent=2),encoding='utf-8')
print(json.dumps(summary,ensure_ascii=False,indent=2))
