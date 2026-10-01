import hashlib,json,pathlib,subprocess,zipfile
from PIL import Image
ROOT=pathlib.Path.cwd();EV=ROOT/'Docs/QA/MainStageCleanReproduction_20261001/Evidence/DeterministicSceneSync'
def load(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def sha(p):
    with p.open('rb') as f:return hashlib.file_digest(f,'sha256').hexdigest().upper()
paths=load(EV/'paths.json');result=load(EV/'comparison.json');manifest=load(EV/'input_manifest.json')
result['human_exe_AB']={'PASS':True,'question':'A/B both Stage Selection display and start operation normal?','answer':'はい','scope':'Both new exe launch/menu/start; NOT a full keyboard playthrough'}
result['runtime_smoke']={}
for label in ['A','B']:
    result['runtime_smoke'][label]={}
    for mode in ['Full','Focused']:
        packet=EV/f'{mode}Runtime{label}/result.txt'
        if not packet.exists():raise SystemExit('STOP pending runtime '+str(packet))
        text=packet.read_text();result['runtime_smoke'][label][mode]=text
        if 'failures=0' not in text:raise SystemExit('STOP runtime FAIL')
    for ppm in (EV/f'FocusedRuntime{label}').glob('*.ppm'):
        with Image.open(ppm) as im:
            png=ppm.with_suffix('.png');im.save(png)
            with Image.open(png) as check:
                if im.convert('RGB').tobytes()!=check.convert('RGB').tobytes():raise SystemExit('STOP PNG pixels')
    project=pathlib.Path(paths['copies'][label]['project'])
    result[label]['post_runtime_scene_sha']=sha(project/'Assets/Scenes/MainStage.unity')
    result[label]['source_changed_after_runtime']=[p for p,s in manifest.items() if p.endswith('.cs') and sha(project/p)!=s]
    result[label]['all_scenes_except_main_changes']=[p for p,s in manifest.items() if p.endswith('.unity') and p!='Assets/Scenes/MainStage.unity' and sha(project/p)!=s]
trace=pathlib.Path(paths['trace_build_project'])
traceLog=(EV/'TraceBuild.log').read_text(encoding='utf-8',errors='replace')
traceStack=(EV/'TraceBuild/scene_saving_stack.txt').read_text()
result['build_trace_success']='Build Finished, Result: Success.' in traceLog
result['build_trace_stack_sync_to_save']=all(s in traceStack for s in ['MainStageSceneSync.Sync','MainStageSceneSync.OnSceneOpened','EditorSceneManager.SaveScene','WindowsPrototypeBuilder.BuildWindowsPrototype'])
result['trace_build_scene_sha']=sha(trace/'Assets/Scenes/MainStage.unity')
result['trace_build_scene_matches_AB']=result['trace_build_scene_sha']==result['A']['after_sha']==result['B']['after_sha']
protected=load(EV/'protected_start.json')
result['protected_existing_files']=len(protected)
result['protected_file_changes']=[p for p,s in protected.items() if not (ROOT/p).is_file() or sha(ROOT/p)!=s]
result['original_bgm_sha256']=sha(ROOT/'Assets/Resources/Audio/YasashiiOdori.mp3')
def git(*args):return subprocess.run(['git',*args],cwd=ROOT,check=True,capture_output=True).stdout.decode('utf-8').strip()
result['git_final']={'branch':git('branch','--show-current'),'HEAD':git('rev-parse','HEAD'),'origin/main':git('rev-parse','origin/main'),'index':git('diff','--cached','--name-only'),'production_diff':git('diff','--','Assets','ProjectSettings','Packages'),'status':git('status','--short')}
strict=all([result['post_scene_byte_identical'],result['patch_content_identical'],not result['all_snapshot_semantic_differences'],not result['runtime_14_differences'],all(v['different_pixels']==0 for v in result['visual']),result['build_trace_success'],result['build_trace_stack_sync_to_save'],result['trace_build_scene_matches_AB'],not result['protected_file_changes'],not result['git_final']['index'],not result['git_final']['production_diff']])
strict=strict and all(not result[l]['source_changed_after_runtime'] and not result[l]['all_scenes_except_main_changes'] and result[l]['post_runtime_scene_sha']==result[l]['after_sha'] for l in ['A','B'])
result['case']='D1' if strict else 'UNRESOLVED'
result['cleanup']='PASS' if strict else 'FAIL'
result['condition']='clean HEAD + documented external BGM dependency + Unity 6000.3.21f1 + existing deterministic MainStage Scene Sync'
result['commit_or_push_performed']=False
result['git_save_ready']=strict
(EV/'final_result.json').write_text(json.dumps(result,ensure_ascii=False,indent=2),encoding='utf-8')
# Lossless archive of bulky raw/normalized inspector snapshots; raw originals retained in task temp.
snapshotFiles=[EV/f'Snapshot{l}/all_scene_serialized.json' for l in ['A','B','Cause']]+[EV/f'Snapshot{l}/semantic_all_scene.json' for l in ['A','B']]
archive=EV/'SceneSnapshots.zip'
if archive.exists():raise SystemExit('STOP archive already exists')
snapshotManifest={p.relative_to(EV).as_posix():sha(p) for p in snapshotFiles}
with zipfile.ZipFile(archive,'w',zipfile.ZIP_DEFLATED,compresslevel=9) as z:
    for p in snapshotFiles:z.write(p,p.relative_to(EV).as_posix())
with zipfile.ZipFile(archive) as z:
    if any(hashlib.sha256(z.read(p)).hexdigest().upper()!=s for p,s in snapshotManifest.items()):raise SystemExit('STOP snapshot archive hash mismatch')
(EV/'snapshot_archive_manifest.json').write_text(json.dumps(snapshotManifest,indent=2),encoding='utf-8')
raw=pathlib.Path(paths['task'])/'RawEvidence';raw.mkdir(exist_ok=False)
generated=snapshotFiles+list(EV.glob('Runtime14*/*.ppm'))+list(EV.glob('FocusedRuntime*/*.ppm'))
for p in generated:
    dest=raw/p.relative_to(EV);dest.parent.mkdir(parents=True,exist_ok=True);p.rename(dest)
print(json.dumps({k:result[k] for k in ['case','cleanup','condition','protected_existing_files','protected_file_changes','build_trace_stack_sync_to_save','trace_build_scene_matches_AB','git_final']},ensure_ascii=False,indent=2))
