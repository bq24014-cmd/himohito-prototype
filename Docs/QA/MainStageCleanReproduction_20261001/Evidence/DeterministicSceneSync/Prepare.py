import datetime, hashlib, io, json, pathlib, subprocess, zipfile

ROOT=pathlib.Path.cwd()
EV=ROOT/'Docs/QA/MainStageCleanReproduction_20261001/Evidence/DeterministicSceneSync'
EXPECTED='4919233cd1d0815afb438bd35a7336492ec09548'
BGM='Assets/Resources/Audio/YasashiiOdori.mp3'
SCENE='Assets/Scenes/MainStage.unity'
EXPECTED_BGM='25C52DA234D42C7A5E5CC91D45AEED14829F845EA15932A88AC94AB006781E83'
def sha(path):
    with path.open('rb') as f:return hashlib.file_digest(f,'sha256').hexdigest().upper()
def git(*args):
    return subprocess.run(['git','-c','safe.directory='+ROOT.as_posix(),*args],cwd=ROOT,check=True,capture_output=True).stdout
def save(name,data): (EV/name).write_text(json.dumps(data,ensure_ascii=False,indent=2),encoding='utf-8')
state={k:git(*v).decode('utf-8').strip() for k,v in {'HEAD':['rev-parse','HEAD'],'origin/main':['rev-parse','origin/main'],'branch':['branch','--show-current'],'index':['diff','--cached','--name-only'],'status':['status','--short'],'production_diff':['diff','--','Assets','Packages','ProjectSettings']}.items()}
if state['HEAD']!=EXPECTED or state['origin/main']!=EXPECTED or state['branch']!='main' or state['index'] or state['production_diff']:raise SystemExit('STOP unexpected baseline')
if sha(ROOT/BGM)!=EXPECTED_BGM:raise SystemExit('STOP BGM identity mismatch')
save('git_start.json',state)
protected={}
for folder in ['Assets','Packages','ProjectSettings','Docs','output','Builds']:
    for p in (ROOT/folder).rglob('*'):
        if p.is_file() and 'MainStageCleanReproduction_20261001' not in p.parts: protected[p.relative_to(ROOT).as_posix()]=sha(p)
for name in ['README.md','AGENTS.md','.gitignore']:
    if (ROOT/name).is_file():protected[name]=sha(ROOT/name)
save('protected_start.json',protected)
task=ROOT/'.codex_tmp/MainStageCleanReproduction_20261001'/('DeterministicAB_'+datetime.datetime.now().strftime('%Y%m%d_%H%M%S'))
task.mkdir(parents=True,exist_ok=False)
payload=git('archive','--format=zip','HEAD','Assets','Packages','ProjectSettings')
manifest={};copies={}
for label in ['A','B']:
    project=task/('Copy'+label);project.mkdir()
    with zipfile.ZipFile(io.BytesIO(payload)) as archive:archive.extractall(project)
    current={p.relative_to(project).as_posix():sha(p) for p in project.rglob('*') if p.is_file()}
    if not manifest:manifest=current
    elif manifest!=current:raise SystemExit('STOP clean export mismatch')
    dest=project/BGM
    if dest.exists():raise SystemExit('STOP external BGM tracked unexpectedly')
    dest.write_bytes((ROOT/BGM).read_bytes())
    if sha(dest)!=EXPECTED_BGM:raise SystemExit('STOP BGM copy mismatch')
    scene_before=(project/SCENE).read_bytes()
    if scene_before!=git('show','HEAD:'+SCENE):raise SystemExit('STOP scene not HEAD')
    (EV/('Copy'+label+'_before.unity.txt')).write_bytes(scene_before)
    copies[label]={'project':str(project),'scene_before_sha256':sha(project/SCENE),'bgm_sha256':sha(dest),'initial_committed_files':len(current)}
save('input_manifest.json',manifest)
save('paths.json',{'head':EXPECTED,'task':str(task),'copies':copies,'unity':'6000.3.21f1','production_read_only':True})
print(json.dumps(copies,ensure_ascii=False,indent=2))
