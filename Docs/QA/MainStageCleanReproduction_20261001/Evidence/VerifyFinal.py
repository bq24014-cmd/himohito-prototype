"""Read-only production checks and generated evidence receipts; no Git mutation."""
import pathlib, hashlib, subprocess, json, datetime, zipfile
ROOT = pathlib.Path(__file__).resolve().parents[4]
EV = pathlib.Path(__file__).resolve().parent
TASK = ROOT / '.codex_tmp/MainStageCleanReproduction_20261001'
SCENE = 'Assets/Scenes/MainStage.unity'
EXPECTED = '4919233cd1d0815afb438bd35a7336492ec09548'
def sha(p): return hashlib.sha256(p.read_bytes()).hexdigest().upper() if p.is_file() else 'MISSING'
def git(*args):
    p = subprocess.run(['git', '-c', 'safe.directory=' + ROOT.as_posix(), *args], cwd=ROOT, capture_output=True)
    return {'exit': p.returncode, 'stdout': p.stdout.decode('utf-8', errors='replace'), 'stderr': p.stderr.decode('utf-8', errors='replace')}
def save(name, data): (EV/name).write_text(json.dumps(data, ensure_ascii=False, indent=2)+'\n', encoding='utf-8')
start = json.loads((EV/'protected_start.json').read_text(encoding='utf-8'))
manifest = json.loads((EV/'clean_input_manifest.json').read_text(encoding='utf-8'))
unexpected = []; restored = []
for rel, before in start.items():
    after = sha(ROOT/rel)
    if before != after:
        item = {'path': rel, 'before': before, 'after': after}
        (restored if rel == SCENE and after == manifest[SCENE] else unexpected).append(item)
state = {name: git(*args) for name,args in {
    'status':['status'], 'status_short':['status','--short'], 'index':['diff','--cached','--name-only'],
    'head':['rev-parse','HEAD'], 'origin_main':['rev-parse','origin/main'], 'branch':['branch','--show-current'],
    'scene_diff':['diff','--',SCENE], 'scene_diff_check':['diff','--check','--',SCENE],
    'patch_applies_to_head':['apply','--check',str(ROOT/'Docs/Archive/SubmissionScene_20260911/MainStage_submission_vs_HEAD.patch')]
}.items()}
save('git_final.json',state)
copies = {}
for name in ['BuildProject','QAProject']:
    missing_or_changed = [{'path':rel, 'expected':s, 'actual':sha(TASK/name/rel)} for rel,s in manifest.items() if sha(TASK/name/rel)!=s]
    production = [x for x in missing_or_changed if x['path'].startswith('Assets/Scripts/') or x['path']==SCENE]
    copies[name] = {'production_scripts_and_main_scene_exact_head':not production,
        'input_deltas_after_editor_import':missing_or_changed}
save('clean_input_final.json',copies)
with zipfile.ZipFile(ROOT/'Docs/Archive/SubmissionScene_20260911/MainStage_submission_20260911.zip') as z:
    archived_sha = hashlib.sha256(z.read('MainStage.unity')).hexdigest().upper()
log = (EV/'CleanHeadBuild.log').read_text(encoding='utf-8',errors='replace')
build = {'result':'FAIL', 'reason':'BGM missing; committed BackgroundMusicBuildCheck stops build',
    'build_failure_present': 'BuildFailedException: BGM' in log and 'Windows build failed: Failed' in log,
    'exe_exists':(TASK/'BuildProject/Builds/Windows/Game/HimoHitoPrototype.exe').exists(),
    'exe_launch':'NOT_RUN_NO_EXE', 'ignored_bgm_sha256':sha(ROOT/'Assets/Resources/Audio/YasashiiOdori.mp3'),
    'ignored_bgm_committed':git('cat-file','-e','HEAD:Assets/Resources/Audio/YasashiiOdori.mp3')['exit']==0,
    'ignored_by':git('check-ignore','Assets/Resources/Audio/YasashiiOdori.mp3'),
    'attempt_path':str(TASK/'BuildProject/Builds/Windows/Game')}
save('build_result.json',build)
proof = {'utc':datetime.datetime.now(datetime.timezone.utc).isoformat(), 'protected_files':len(start),
    'authorized_scene_restore':restored, 'unexpected_changes':unexpected,
    'scene_diff_zero':not state['scene_diff']['stdout'] and state['scene_diff']['exit']==0,
    'scene_diff_check_pass':state['scene_diff_check']['exit']==0,
    'scene_sha':sha(ROOT/SCENE), 'submission_archive_sha':archived_sha,
    'head':state['head']['stdout'].strip(), 'origin_main':state['origin_main']['stdout'].strip(),
    'index_empty':not state['index']['stdout'].strip(),
    'existing_output_files_preserved':not any(x['path'].startswith('output/') for x in unexpected),
    'source_unchanged':not any(x['path'].startswith('Assets/Scripts/') for x in unexpected),
    'commit_push_performed':False}
proof['pass'] = not unexpected and len(restored)==1 and proof['scene_diff_zero'] and proof['scene_diff_check_pass'] and proof['index_empty'] and proof['head']==proof['origin_main']==EXPECTED
save('protection_result.json',proof)
print(json.dumps({'protection':proof,'build':build,'copies':copies},ensure_ascii=False,indent=2))
if not proof['pass']: raise SystemExit('PROTECTION FAILED')
