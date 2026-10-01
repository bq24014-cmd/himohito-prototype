"""Fresh committed source exports plus exactly one explicitly approved external BGM."""
import pathlib,subprocess,hashlib,json,zipfile,io,datetime,re
ROOT=pathlib.Path(__file__).resolve().parents[4]
EV=pathlib.Path(__file__).resolve().parent
EXPECTED='4919233cd1d0815afb438bd35a7336492ec09548'
BGM='Assets/Resources/Audio/YasashiiOdori.mp3'
SCENE='Assets/Scenes/MainStage.unity'
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest().upper()
def git(*args,required=True):
    p=subprocess.run(['git','-c','safe.directory='+ROOT.as_posix(),*args],cwd=ROOT,capture_output=True)
    if required and p.returncode:raise RuntimeError(str(args)+': '+p.stderr.decode('utf-8',errors='replace'))
    return p
def save(name,v):(EV/name).write_text(json.dumps(v,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
state={n:git(*args).stdout.decode('utf-8').strip() for n,args in {
    'head':['rev-parse','HEAD'],'remote':['rev-parse','origin/main'],'branch':['branch','--show-current'],
    'index':['diff','--cached','--name-only'],'status':['status','--short'],
    'source_diff':['diff','--','Assets/Scripts','Assets/Editor','Packages','ProjectSettings'],
    'scene_diff':['diff','--',SCENE]}.items()}
if state['head']!=EXPECTED or state['remote']!=EXPECTED or state['branch']!='main' or state['index'] or state['source_diff'] or state['scene_diff']:
    raise SystemExit('STOP unexpected Git or production state')
original=ROOT/BGM
if not original.is_file():raise SystemExit('STOP existing BGM missing')
identity={'original_path':str(original),'filename':original.name,'file_size':original.stat().st_size,'sha256':sha(original),
    'git_tracked':bool(git('ls-files','--',BGM).stdout.strip()),
    'ignore_rule':git('check-ignore','-v','--',BGM).stdout.decode('utf-8').strip(),
    'documented_reason':'.gitignore comment: DOVA-SYNDROME: licensed for use in the game, not standalone redistribution. README: 音源単体の再配布を避けるためGit管理外。',
    'reason_is_repository_documentation_not_new_license_adjudication':True}
save('external_bgm_identity.json',identity);save('external_git_start.json',state)
protected={}
for folder in ['Assets','Packages','ProjectSettings','output','Builds','Docs']:
    for p in (ROOT/folder).rglob('*'):
        if p.is_file() and 'MainStageCleanReproduction_20261001' not in p.parts and 'Dependencies' not in p.parts:
            protected[p.relative_to(ROOT).as_posix()]=sha(p)
for name in ['README.md','AGENTS.md','.gitignore']:
    if (ROOT/name).exists():protected[name]=sha(ROOT/name)
save('external_protected_start.json',protected)
stamp=datetime.datetime.now().strftime('%Y%m%d_%H%M%S')
task=ROOT/'.codex_tmp/MainStageCleanReproduction_20261001'/('ExternalBGM_'+stamp)
if task.exists():raise SystemExit('STOP fresh destination required')
task.mkdir(parents=True)
archive=git('archive','--format=zip','HEAD','Assets','Packages','ProjectSettings').stdout
manifest={};copies={}
for name in ['BuildProject','QAProject']:
    target=task/name;target.mkdir()
    with zipfile.ZipFile(io.BytesIO(archive)) as z:z.extractall(target)
    for p in target.rglob('*'):
        if not p.is_file():continue
        rel=p.relative_to(target).as_posix();data=p.read_bytes()
        if data.startswith(b'version https://git-lfs.github.com/spec/v1'):
            oid=re.search(rb'oid sha256:([0-9a-f]{64})',data).group(1).decode().upper()
            if sha(ROOT/rel)!=oid:raise SystemExit('STOP LFS payload mismatch '+rel)
            p.write_bytes((ROOT/rel).read_bytes())
        if name=='BuildProject':manifest[rel]=sha(p)
        elif manifest[rel]!=sha(p):raise SystemExit('STOP clean export mismatch '+rel)
    dest=target/BGM
    if dest.exists():raise SystemExit('STOP BGM already exported from HEAD')
    dest.write_bytes(original.read_bytes())
    if sha(dest)!=identity['sha256'] or sha(original)!=identity['sha256']:raise SystemExit('STOP BGM copy mismatch')
    differences=[rel for rel,s in manifest.items() if sha(target/rel)!=s]
    if differences:raise SystemExit('STOP source/scene/settings changed '+str(differences))
    copies[name]={'project':str(target),'copy_destination':str(dest),'copied_sha256':sha(dest),
        'original_sha256_after_copy':sha(original),'committed_input_differences':differences}
save('external_input_manifest.json',manifest)
save('external_build_paths.json',{'baseline_head':EXPECTED,'task':str(task),'copies':copies,
    'condition':'clean HEAD + documented external BGM dependency','old_builds_used_or_overwritten':False})
print(json.dumps({'identity':identity,'copies':copies},ensure_ascii=False,indent=2))
